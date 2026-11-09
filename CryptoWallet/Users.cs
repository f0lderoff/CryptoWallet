using Npgsql;
using System;
using System.Windows.Forms;

namespace CryptoWallet.Database
{
    public static class Users
    {
        public static bool Register(string login, string email, string password)
        {
            try
            {
                string hash = BCrypt.Net.BCrypt.HashPassword(password);

                using (var conn = DB.GetConnection())
                {
                    conn.Open();

                    using (var cmd = new NpgsqlCommand(@"
                        INSERT INTO Users (login, email, password, createdAt)
                        VALUES (@login, @email, @password, NOW())
                        RETURNING id", conn))
                    {
                        cmd.Parameters.AddWithValue("login", login);
                        cmd.Parameters.AddWithValue("email", email);
                        cmd.Parameters.AddWithValue("password", hash);

                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            Session.UserId = Convert.ToInt32(result);
                            Session.Login = login;
                            Session.Email = email;
                            return true;
                        }

                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public static bool Login(string loginOrEmail, string password)
        {
            try
            {
                string hash = null;

                using (var conn = DB.GetConnection())
                {
                    conn.Open();

                    using (var cmd = new NpgsqlCommand(@"
                        SELECT password, login, email, id
                        FROM Users
                        WHERE login = @value OR email = @value
                        LIMIT 1", conn))
                    {
                        cmd.Parameters.AddWithValue("value", loginOrEmail);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                hash = reader.GetString(0);
                                Session.Login = reader.GetString(1);
                                Session.Email = reader.GetString(2);
                                Session.UserId = reader.GetInt32(3);
                            }
                        }
                    }
                }

                bool valid = !string.IsNullOrEmpty(hash) && BCrypt.Net.BCrypt.Verify(password, hash);
                return valid;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public static bool UpdatePasswordByEmail(string email, string newPassword)
        {
            try
            {
                string hash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                using (var conn = DB.GetConnection())
                {
                    conn.Open();

                    using (var cmd = new NpgsqlCommand(@"
                        UPDATE users
                        SET password = @password
                        WHERE email = @email", conn))
                    {
                        cmd.Parameters.AddWithValue("password", hash);
                        cmd.Parameters.AddWithValue("email", email);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool UserExists(string login)
        {
            try
            {
                using (var conn = DB.GetConnection())
                {
                    conn.Open();

                    using (var cmd = new NpgsqlCommand(@"
                        SELECT 1
                        FROM Users
                        WHERE login = @login", conn))
                    {
                        cmd.Parameters.AddWithValue("login", login);

                        using (var reader = cmd.ExecuteReader())
                        {
                            return reader.Read();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public static bool EmailExists(string email)
        {
            try
            {
                using (var conn = DB.GetConnection())
                {
                    conn.Open();

                    using (var cmd = new NpgsqlCommand(@"
                        SELECT 1
                        FROM Users
                        WHERE email = @email
                        LIMIT 1", conn))
                    {
                        cmd.Parameters.AddWithValue("email", email);

                        using (var reader = cmd.ExecuteReader())
                        {
                            return reader.Read();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }
    }
}