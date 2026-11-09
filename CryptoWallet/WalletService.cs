using CryptoWallet.Database;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;

namespace CryptoWallet
{
    public class WalletSummary
    {
        public decimal Total { get; set; }
        public decimal Invested { get; set; }
        public decimal RealizedProfit { get; set; }
        public decimal Diff
        {
            get
            {
                return Total + RealizedProfit - Invested;
            }
        }
    }

    public static class WalletService
    {
        public static WalletSummary GetSummary(int userId)
        {
            var result = new WalletSummary();

            using (var conn = DB.GetConnection())
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand(@"
                    SELECT
                        COALESCE(SUM(b.amount * c.currentpriceusd), 0) AS total_value,
                        COALESCE(s.total_invested, 0) AS total_invested,
                        COALESCE(s.realized_profit, 0) AS realized_profit
                    FROM wallet s
                    LEFT JOIN balance b ON b.user_id = s.user_id
                    LEFT JOIN coin c ON c.id = b.coin_id
                    WHERE s.user_id = @user_id
                    GROUP BY s.total_invested, s.realized_profit", conn))
                {
                    cmd.Parameters.AddWithValue("user_id", userId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            result.Total = reader.GetDecimal(0);
                            result.Invested = reader.GetDecimal(1);
                            result.RealizedProfit = reader.GetDecimal(2);
                        }
                    }
                }
            }

            return result;
        }

        public static DataTable GetCoinDetails(int userId)
        {
            var dt = new DataTable();

            using (var conn = DB.GetConnection())
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand(@"
                SELECT
                    c.code AS coin,
                    b.amount,
                    CASE 
                        WHEN b.amount > 0 THEN b.investedusd / b.amount
                        ELSE 0
                    END AS avg_buy_price,
                    c.currentpriceusd AS current_price,
                    b.investedusd AS invested_value,
                    (b.amount * c.currentpriceusd) AS current_value,
                    ((b.amount * c.currentpriceusd) - b.investedusd) AS profit
                FROM balance b
                JOIN coin c ON c.id = b.coin_id
                WHERE b.user_id = @user_id and b.amount > 0", conn))
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

        public static List<CoinItem> GetUserCoins(int userId)
        {
            var list = new List<CoinItem>();

            using (var conn = DB.GetConnection())
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand(@"
                    SELECT DISTINCT c.id, c.code, c.GeckoId, c.currentPriceUSD
                    FROM balance b
                    JOIN coin c ON c.id = b.coin_id
                    WHERE b.user_id = @user_id AND b.amount > 0
                    ORDER BY c.code ASC", conn))
                {
                    cmd.Parameters.AddWithValue("user_id", userId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new CoinItem
                            {
                                Id = reader.GetInt32(0),
                                Code = reader.GetString(1),
                                GeckoId = reader.GetString(2),
                                Price = reader.GetDecimal(3)
                            });
                        }
                    }
                }
            }

            return list;
        }
    }
}