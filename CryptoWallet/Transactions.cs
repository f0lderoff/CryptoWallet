using CryptoWallet.Database;
using System;
using System.Windows.Forms;
using System.Drawing;

namespace CryptoWallet
{
    public partial class Transactions : UserControl
    {
        public Transactions()
        {
            InitializeComponent();
        }

        public void LoadTransactions()
        {
            transactions_Dgv.DataSource = TransactionService.GetTransactions(Session.UserId);
            transactions_Dgv.MultiSelect = false;

            transactions_Dgv.Columns["id"].Visible = false;

            transactions_Dgv.Columns["coin"].HeaderText = "Монета";
            transactions_Dgv.Columns["coin"].Width = 100;

            transactions_Dgv.Columns["type"].HeaderText = "Тип";
            transactions_Dgv.Columns["type"].Width = 75;

            transactions_Dgv.Columns["amount"].HeaderText = "Кол-во";
            transactions_Dgv.Columns["amount"].DefaultCellStyle.Format = "0.######";

            transactions_Dgv.Columns["priceusd"].HeaderText = "Цена";
            transactions_Dgv.Columns["priceusd"].DefaultCellStyle.Format = "0.######";

            transactions_Dgv.Columns["createdat"].HeaderText = "Дата";
            transactions_Dgv.Columns["createdat"].Width = 225;
        }

        public event Action<string> Navigate;

        public void UpdateUser()
        {
            name_Txt.Text = Session.Login;
        }

        private void dashboard_Btn_Click(object sender, EventArgs e)
        {
            Navigate?.Invoke("dashboard");
        }

        private void Transactions_Load(object sender, EventArgs e)
        {
            LoadTransactions();
            LoadWallet();
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

        private void dell_Btn_Click(object sender, EventArgs e)
        {
            if (transactions_Dgv.SelectedRows.Count == 0 && transactions_Dgv.CurrentRow == null)
            {
                MessageBox.Show(
                    "Выберите транзакцию для удаления.",
                    "CryptoWallet",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            var row = transactions_Dgv.SelectedRows.Count > 0 ? transactions_Dgv.SelectedRows[0] : transactions_Dgv.CurrentRow;

            if (row == null || row.Cells["id"].Value == null)
            {
                MessageBox.Show(
                    "Не удалось определить выбранную транзакцию.",
                    "CryptoWallet",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            int transactionId = Convert.ToInt32(row.Cells["id"].Value);
            string coin = row.Cells["coin"]?.Value?.ToString() ?? "UNKNOWN";
            string type = row.Cells["type"]?.Value?.ToString() ?? "UNKNOWN";
            string amount = row.Cells["amount"]?.Value?.ToString() ?? "UNKNOWN";
            string createdat = row.Cells["createdat"]?.Value?.ToString() ?? "UNKNOWN";

            var confirm = MessageBox.Show(
                $"Удалить транзакцию {type} {coin} ({amount}) шт.\nза {createdat}?",
                "CryptoWallet",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirm != DialogResult.Yes) return;

            bool deleted = TransactionService.DeleteTransaction(transactionId, Session.UserId);

            if (!deleted) return;

            LoadTransactions();
            LoadWallet();
        }
    }
}