using CryptoWallet.Database;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace CryptoWallet
{
    public static class TransactionService
    {
        private class BalanceState
        {
            public decimal Amount { get; set; }
            public decimal Invested { get; set; }
        }

        public static DataTable GetTransactions(int userId)
        {
            var dt = new DataTable();

            using (var conn = DB.GetConnection())
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand(@"
                    SELECT
                        t.id,
                        c.code AS coin,
                        t.type,
                        t.amount,
                        t.priceusd,
                        t.createdat
                    FROM transactions t
                    JOIN coin c ON c.id = t.coin_id
                    WHERE t.user_id = @user_id
                    ORDER BY t.createdat DESC, t.id DESC", conn))
                {
                    cmd.Parameters.AddWithValue("user_id", userId);

                    using (var adapter = new NpgsqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }

            return dt;
        }

        public static bool AddTransaction(int userId, int coinId, string type, decimal amount, decimal priceUsd, decimal fee = 0m)
        {
            using (var conn = DB.GetConnection())
            {
                conn.Open();

                using (var dbTx = conn.BeginTransaction())
                {
                    try
                    {
                        decimal currentAmount = 0m;
                        decimal currentInvested = 0m;

                        using (var balanceCmd = new NpgsqlCommand(@"
                            SELECT amount, investedusd
                            FROM balance
                            WHERE user_id = @user_id AND coin_id = @coin_id
                            FOR UPDATE", conn, dbTx))
                        {
                            balanceCmd.Parameters.AddWithValue("user_id", userId);
                            balanceCmd.Parameters.AddWithValue("coin_id", coinId);

                            using (var reader = balanceCmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    currentAmount = reader.GetDecimal(0);
                                    currentInvested = reader.GetDecimal(1);
                                }
                            }
                        }

                        decimal newAmount = currentAmount;
                        decimal newInvested = currentInvested;
                        decimal diffChange = 0m;
                        decimal investedChange = 0m;

                        if (type == "buy")
                        {
                            investedChange = amount * priceUsd + fee;
                            newAmount += amount;
                            newInvested += investedChange;
                        }
                        else if (type == "sell")
                        {
                            if (currentAmount < amount)
                                throw new InvalidOperationException("Недостаточно монет на балансе.");

                            decimal avgCost = currentAmount > 0 ? currentInvested / currentAmount : 0m;
                            decimal soldCost = avgCost * amount;
                            decimal currentSell = amount * priceUsd - fee;

                            diffChange = currentSell - soldCost;

                            newAmount = currentAmount - amount;
                            newInvested = currentInvested - soldCost;

                            investedChange = -soldCost;

                            if (newAmount < 0) newAmount = 0;
                        }
                        else
                        {
                            throw new InvalidOperationException("Неизвестный тип транзакции.");
                        }

                        using (var balanceUpsert = new NpgsqlCommand(@"
                            INSERT INTO balance (user_id, coin_id, amount, investedusd)
                            VALUES (@user_id, @coin_id, @amount, @investedusd)
                            ON CONFLICT (user_id, coin_id)
                            DO UPDATE SET
                                amount = EXCLUDED.amount,
                                investedusd = EXCLUDED.investedusd", conn, dbTx))
                        {
                            balanceUpsert.Parameters.AddWithValue("user_id", userId);
                            balanceUpsert.Parameters.AddWithValue("coin_id", coinId);
                            balanceUpsert.Parameters.AddWithValue("amount", newAmount);
                            balanceUpsert.Parameters.AddWithValue("investedusd", newInvested);
                            balanceUpsert.ExecuteNonQuery();
                        }

                        using (var statsUpsert = new NpgsqlCommand(@"
                            INSERT INTO wallet (user_id, total_invested, realized_profit)
                            VALUES (@user_id, @total_invested, @realized_profit)
                            ON CONFLICT (user_id)
                            DO UPDATE SET
                                total_invested = wallet.total_invested + @total_invested,
                                realized_profit = wallet.realized_profit + @realized_profit", conn, dbTx))
                        {
                            statsUpsert.Parameters.AddWithValue("user_id", userId);
                            statsUpsert.Parameters.AddWithValue("total_invested", investedChange);
                            statsUpsert.Parameters.AddWithValue("realized_profit", diffChange);
                            statsUpsert.ExecuteNonQuery();
                        }

                        using (var cmd = new NpgsqlCommand(@"
                            INSERT INTO transactions
                                (user_id, coin_id, type, amount, priceusd, fee, createdat)
                            VALUES
                                (@user_id, @coin_id, @type, @amount, @priceusd, @fee, NOW())", conn, dbTx))
                        {
                            cmd.Parameters.AddWithValue("user_id", userId);
                            cmd.Parameters.AddWithValue("coin_id", coinId);
                            cmd.Parameters.AddWithValue("type", type);
                            cmd.Parameters.AddWithValue("amount", amount);
                            cmd.Parameters.AddWithValue("priceusd", priceUsd);
                            cmd.Parameters.AddWithValue("fee", fee);
                            cmd.ExecuteNonQuery();
                        }

                        dbTx.Commit();
                        return true;
                    }
                    catch
                    {
                        dbTx.Rollback();
                        return false;
                    }
                }
            }
        }

        public static bool DeleteTransaction(int transactionId, int userId)
        {
            using (var conn = DB.GetConnection())
            {
                conn.Open();

                using (var dbTx = conn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new NpgsqlCommand(@"
                            DELETE FROM transactions
                            WHERE id = @id AND user_id = @user_id", conn, dbTx))
                        {
                            cmd.Parameters.AddWithValue("id", transactionId);
                            cmd.Parameters.AddWithValue("user_id", userId);

                            int rows = cmd.ExecuteNonQuery();
                            if (rows == 0)
                                throw new InvalidOperationException("Транзакция не найдена.");

                        }

                        RebuildWallet(userId, conn, dbTx);

                        dbTx.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        dbTx.Rollback();

                        MessageBox.Show(
                            $"Не удалось удалить транзакцию:\n{ex.Message}",
                            "CryptoWallet",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );

                        return false;
                    }
                }
            }
        }

        private static void RebuildWallet(int userId, NpgsqlConnection conn, NpgsqlTransaction dbTx)
        {
            var balances = new Dictionary<int, BalanceState>();
            decimal totalInvested = 0m;
            decimal realizedProfit = 0m;

            using (var cmd = new NpgsqlCommand(@"
                SELECT coin_id, type, amount, priceusd, fee
                FROM transactions
                WHERE user_id = @user_id
                ORDER BY createdat ASC, id ASC", conn, dbTx))
            {
                cmd.Parameters.AddWithValue("user_id", userId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int coinId = reader.GetInt32(0);
                        string type = reader.GetString(1);
                        decimal amount = reader.GetDecimal(2);
                        decimal priceUsd = reader.GetDecimal(3);
                        decimal fee = reader.GetDecimal(4);

                        if (!balances.TryGetValue(coinId, out var state))
                        {
                            state = new BalanceState();
                            balances[coinId] = state;
                        }

                        if (type == "buy")
                        {
                            decimal investedChange = amount * priceUsd + fee;
                            state.Amount += amount;
                            state.Invested += investedChange;
                            totalInvested += investedChange;
                        }
                        else if (type == "sell")
                        {
                            if (state.Amount < amount)
                                throw new InvalidOperationException(
                                    "BUG!!!: Нельзя удалить эту транзакцию, потому что после пересчёта история продаж становится некорректной."
                                );

                            decimal avgCost = state.Amount > 0 ? state.Invested / state.Amount : 0m;
                            decimal soldCost = avgCost * amount;
                            decimal sellValue = amount * priceUsd - fee;
                            decimal profit = sellValue - soldCost;

                            state.Amount -= amount;
                            state.Invested -= soldCost;

                            totalInvested -= soldCost;
                            realizedProfit += profit;
                        }
                        else
                        {
                            throw new InvalidOperationException("Неизвестный тип транзакции.");
                        }
                    }
                }
            }

            using (var clearBalance = new NpgsqlCommand(@"
                DELETE FROM balance
                WHERE user_id = @user_id", conn, dbTx))
            {
                clearBalance.Parameters.AddWithValue("user_id", userId);
                clearBalance.ExecuteNonQuery();
            }

            using (var clearWallet = new NpgsqlCommand(@"
                DELETE FROM wallet
                WHERE user_id = @user_id", conn, dbTx))
            {
                clearWallet.Parameters.AddWithValue("user_id", userId);
                clearWallet.ExecuteNonQuery();
            }

            foreach (var item in balances)
            {
                if (item.Value.Amount <= 0m)
                    continue;

                using (var insertBalance = new NpgsqlCommand(@"
                    INSERT INTO balance (user_id, coin_id, amount, investedusd)
                    VALUES (@user_id, @coin_id, @amount, @investedusd)", conn, dbTx))
                {
                    insertBalance.Parameters.AddWithValue("user_id", userId);
                    insertBalance.Parameters.AddWithValue("coin_id", item.Key);
                    insertBalance.Parameters.AddWithValue("amount", item.Value.Amount);
                    insertBalance.Parameters.AddWithValue("investedusd", item.Value.Invested);
                    insertBalance.ExecuteNonQuery();
                }
            }

            using (var upsertWallet = new NpgsqlCommand(@"
                INSERT INTO wallet (user_id, total_invested, realized_profit)
                VALUES (@user_id, @total_invested, @realized_profit)
                ON CONFLICT (user_id)
                DO UPDATE SET
                    total_invested = EXCLUDED.total_invested,
                    realized_profit = EXCLUDED.realized_profit", conn, dbTx))
            {
                upsertWallet.Parameters.AddWithValue("user_id", userId);
                upsertWallet.Parameters.AddWithValue("total_invested", totalInvested);
                upsertWallet.Parameters.AddWithValue("realized_profit", realizedProfit);
                upsertWallet.ExecuteNonQuery();
            }
        }
    }
}