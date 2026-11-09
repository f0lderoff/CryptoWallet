using CryptoWallet.Database;
using Npgsql;
using System.Collections.Generic;

namespace CryptoWallet
{
    public class CoinItem
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string GeckoId { get; set; }
        public decimal Price { get; set; }

        public override string ToString()
        {
            return Code;
        }
    }

    public static class CoinService
    {
        public static List<CoinItem> GetCoins()
        {
            var list = new List<CoinItem>();

            using (var conn = DB.GetConnection())
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand(@"
                    SELECT id, code, GeckoId, currentPriceUSD
                    FROM Coin
                    ORDER BY code ASC", conn))
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

            return list;
        }
    }
}