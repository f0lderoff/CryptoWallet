using CryptoWallet.Database;
using Guna.UI2.WinForms;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CryptoWallet
{
    public partial class Dashboard : UserControl
    {
        public Dashboard()
        {
            InitializeComponent();
        }

        private List<CoinItem> coins;

        public event Action<string> Navigate;

        private Timer refreshTimer;

        private void Dashboard_Load(object sender, EventArgs e)
        {
            coin_ComboBox.SelectedIndexChanged += CoinChanged;
            coin1_ComboBox.SelectedIndexChanged += Coin1Changed;

            refreshTimer = new Timer();
            refreshTimer.Interval = 5 * 60 * 1000;
            refreshTimer.Tick += async (s, args) => await RefreshAllAsync();
            refreshTimer.Start();

        }

        private async Task RefreshAllAsync()
        {
            await Price.UpdateAllPrices();
            LoadCoins(Session.UserId);
            LoadWallet();
            LoadDashboardGrid();
        }

        public void LoadCoins(int userId)
        {
            coin_ComboBox.DataSource = CoinService.GetCoins();

            coin1_ComboBox.DataSource = WalletService.GetUserCoins(Session.UserId);
        }

        public void LoadWallet()
        {
            var summary = WalletService.GetSummary(Session.UserId);

            total_Txt.Text = summary.Total.ToString("0.##");
            invested_Txt.Text = summary.Invested.ToString("0.##");
            diff_Txt.Text = summary.Diff.ToString("0.##");

            decimal diffPct = 0;

            if (summary.Invested > 0)
            {
                diffPct = (summary.Diff / summary.Invested) * 100;
            }

            diff_Pct.Text = diffPct.ToString("0.##") + "%";

            if (summary.Diff > 0)
            {
                diff_Card.BackgroundColor2 = Color.FromArgb(46, 204, 113);
            }
            else if (summary.Diff < 0)
            {
                diff_Card.BackgroundColor2 = Color.FromArgb(231, 76, 60);
            }
            else
            {
                diff_Card.BackgroundColor2 = Color.Gray;
            }
        }

        public void LoadDashboardGrid()
        {

            dashboard_Dgv.DataSource = WalletService.GetCoinDetails(Session.UserId);
            dashboard_Dgv.Columns["coin"].HeaderText = "Монета";
            dashboard_Dgv.Columns["amount"].HeaderText = "Кол-во";
            dashboard_Dgv.Columns["amount"].DefaultCellStyle.Format = "0.######";
            dashboard_Dgv.Columns["avg_buy_price"].HeaderText = "Ср. цена покупки";
            dashboard_Dgv.Columns["avg_buy_price"].DefaultCellStyle.Format = "0.######";
            dashboard_Dgv.Columns["current_price"].HeaderText = "Цена 1шт.";
            dashboard_Dgv.Columns["current_price"].DefaultCellStyle.Format = "0.######";
            dashboard_Dgv.Columns["invested_value"].HeaderText = "Потрачено";
            dashboard_Dgv.Columns["invested_value"].DefaultCellStyle.Format = "0.######";
            dashboard_Dgv.Columns["current_value"].HeaderText = "Цена";
            dashboard_Dgv.Columns["current_value"].DefaultCellStyle.Format = "0.######";
            dashboard_Dgv.Columns["profit"].HeaderText = "+/-";
            dashboard_Dgv.Columns["profit"].DefaultCellStyle.Format = "0.######";

            if (dashboard_Dgv.Rows.Count == 0) placeholderDgv_Txt.Visible = true;
            else placeholderDgv_Txt.Visible = false;
        }

        private void CoinChanged(object sender, EventArgs e)
        {
            if (coin_ComboBox.SelectedItem is CoinItem coin)
            {
                price_TxtBox.Text = coin.Price.ToString("0.########");
            }
        }

        private void Coin1Changed(object sender, EventArgs e)
        {
            if (coin1_ComboBox.SelectedItem is CoinItem coin)
            {
                price1_TxtBox.Text = coin.Price.ToString("0.########");
            }
        }

        private void history_Btn_Click(object sender, EventArgs e)
        {
            Navigate?.Invoke("transactions");
        }

        private void coin_Btn_Click(object sender, EventArgs e)
        {
            Navigate?.Invoke("coins");
        }

        public void UpdateUser()
        {
            name_Txt.Text = Session.Login;
        }

        private void add_Btn_Click(object sender, EventArgs e)
        {
            if (!(coin_ComboBox.SelectedItem is CoinItem coin))
            {
                MessageBox.Show("Выберите, пожалуйста, монету", "CryptoWallet", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            decimal amount = amount_Numeric.Value;

            if (amount <= 0)
            {
                MessageBox.Show("Укажите, пожалуйста, корректное количество", "CryptoWallet", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string normalized = price_TxtBox.Text.Replace(',', '.');

            if (!decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal priceUsd) | priceUsd < 0)
            {
                MessageBox.Show("Укажите, пожалуйста, корректную цену", "CryptoWallet", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bool ok = TransactionService.AddTransaction(
                Session.UserId,
                coin.Id,
                "buy",
                amount,
                priceUsd,
                0m
            );

            if (!ok)
            {
                MessageBox.Show("Не удалось добавить транзакцию.", "CryptoWallet", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            LoadWallet();
            LoadDashboardGrid();
        }

        private void sell_Btn_Click(object sender, EventArgs e)
        {
            if (!(coin1_ComboBox.SelectedItem is CoinItem coin))
            {
                MessageBox.Show("Выберите, пожалуйста, монету", "CryptoWallet", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            decimal amount = amount1_Numeric.Value;

            if (amount <= 0)
            {
                MessageBox.Show("Укажите, пожалуйста, корректное количество", "CryptoWallet", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string normalized = price1_TxtBox.Text.Replace(',', '.');

            if (!decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal priceUsd) | priceUsd < 0)
            {
                MessageBox.Show("Укажите, пожалуйста, корректную цену", "CryptoWallet", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bool ok = TransactionService.AddTransaction(
                Session.UserId,
                coin.Id,
                "sell",
                amount,
                priceUsd,
                0m
            );

            if (!ok)
            {
                MessageBox.Show("Не удалось добавить транзакцию.", "CryptoWallet", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            LoadWallet();
            LoadDashboardGrid();
        }

        private async void refresh_Btn_Click(object sender, EventArgs e)
        {
            await RefreshAllAsync();
        }

        private void quit_Btn_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Вы точно хотите выйти из акканута?", "CryptoWallet", MessageBoxButtons.YesNo,MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Session.Login = null;
                Session.Email = null;
                Navigate?.Invoke("authentication");
            }
        }
    }
}
