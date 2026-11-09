using CryptoWallet.Database;
using Newtonsoft.Json.Linq;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CryptoWallet
{
    public static class Price
    {
        private static readonly HttpClient http = new HttpClient();
        private const string API_URL = "https://api.coingecko.com/api/v3/simple/price";

        static Price()
        {
            http.DefaultRequestHeaders.UserAgent.ParseAdd("CryptoWalletApp/1.0");
        }

        public static async Task<decimal?> GetPriceUSD(string GeckoId)
        {
            try
            {
                string url = $"{API_URL}?ids={GeckoId}&vs_currencies=usd";

                string json = await http.GetStringAsync(url);

                JObject data = JObject.Parse(json);

                var price = data[GeckoId]?["usd"];

                if (price == null) return null;

                return price.Value<decimal>();
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(
                    $"Ошибка сети CoinGekoAPI:\n{ex.Message}",
                    "CryptoWallet",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"CoinGekoAPI Ошибка получения цены:\n{ex.Message}",
                    "CryptoWallet",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return null;
            }
        }

        public static async Task UpdateCoinPrice(string GeckoId, string code)
        {
            try
            {
                decimal? price = await GetPriceUSD(GeckoId);

                if (price == null) return;

                using (var conn = DB.GetConnection())
                {
                    await conn.OpenAsync();

                    using (var cmd = new NpgsqlCommand(@"
                        UPDATE Coin
                        SET currentPriceUSD = @price
                        WHERE code = @code", conn))
                    {
                        cmd.Parameters.AddWithValue("price", price.Value);
                        cmd.Parameters.AddWithValue("code", code);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show(
                    $"Ошибка базы данных:\n{ex.Message}",
                    "CryptoWallet",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"CoinGekoAPI Ошибка обновления цены:\n{ex.Message}",
                    "CryptoWallet",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        public static async Task UpdateAllPrices()
        {
            try
            {
                var coins = CoinService.GetCoins();

                if (coins.Count == 0) return;

                string ids = string.Join(",", coins.Select(c => c.GeckoId));
                string url = $"{API_URL}?ids={ids}&vs_currencies=usd";

                string json = await http.GetStringAsync(url);

                JObject data = JObject.Parse(json);

                using (var conn = DB.GetConnection())
                {
                    conn.Open();

                    foreach (var coin in coins)
                    {
                        try
                        {
                            var priceAPI = data[coin.GeckoId]?["usd"];
                            if (priceAPI == null) continue;

                            decimal price = priceAPI.Value<decimal>();

                            using (var cmd = new NpgsqlCommand(@"
                                UPDATE Coin
                                SET currentPriceUSD = @price
                                WHERE code = @code", conn))
                            {
                                cmd.Parameters.AddWithValue("price", price);
                                cmd.Parameters.AddWithValue("code", coin.Code);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(
                                $"CoinGekoAPI Ошибка обновления монеты {coin.Code}:\n{ex.Message}",
                                "CryptoWallet",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning
                            );
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(
                    $"Ошибка сети CoinGekoAPI:\n{ex.Message}",
                    "CryptoWallet",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show(
                    $"Ошибка базы данных:\n{ex.Message}",
                    "CryptoWallet",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Общая ошибка:\n{ex.Message}",
                    "CryptoWallet",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}