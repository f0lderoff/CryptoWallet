using CryptoWallet.Database;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CryptoWallet
{
    public partial class Form1 : Form
    {
        Authentication authenticationMenu = new Authentication();
        Dashboard dashboardMenu = new Dashboard();
        Transactions transactionsMenu = new Transactions();
        Coins coinsMenu = new Coins();

        public Form1()
        {
            InitializeComponent();
            MaximizeBox = false;

            bool connected = DB.TestConnection();

            if (!connected)
            {
                MessageBox.Show(
                    "Не удалось подключиться к DB",
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }

            DB.ConnectionChanged += UpdateReconButton;
            UpdateReconButton();

            authenticationMenu.Navigate += Navigate;
            dashboardMenu.Navigate += Navigate;
            transactionsMenu.Navigate += Navigate;
            coinsMenu.Navigate += Navigate;

            top_Panel.Controls.Add(reconDb_Button);

            _ = InitializePrices();
            LoadControl(authenticationMenu);
        }

        private async Task InitializePrices()
        {
            await Price.UpdateAllPrices();
        }

        private void UpdateReconButton()
        {
            reconDb_Button.Visible = !DB.IsConnected;
        }

        private void reconDb_Button_Click(object sender, EventArgs e)
        {
            reconDb_Button.Enabled = false;

            bool ok = DB.TestConnection();

            MessageBox.Show(
                ok ? "Переподключение успешно." : "Не удалось подключиться",
                "PostgreSQL",
                MessageBoxButtons.OK,
                ok ? MessageBoxIcon.Information : MessageBoxIcon.Error
            );

            reconDb_Button.Enabled = true;
        }

        private async void Navigate(string page)
        {
            switch (page)
            {
                case "authentication":
                    LoadControl(authenticationMenu);
                    break;

                case "dashboard":
                    LoadControl(dashboardMenu);
                    dashboardMenu.UpdateUser();
                    dashboardMenu.LoadCoins(Session.UserId);
                    dashboardMenu.LoadWallet();
                    dashboardMenu.LoadDashboardGrid();
                    break;

                case "transactions":
                    LoadControl(transactionsMenu);
                    transactionsMenu.UpdateUser();
                    transactionsMenu.LoadTransactions();
                    transactionsMenu.LoadWallet();
                    break;

                case "coins":
                    LoadControl(coinsMenu);
                    coinsMenu.UpdateUser();
                    break;
            }
        }

        private void LoadControl(UserControl control)
        {
            content_Panel.Controls.Clear();
            content_Panel.Controls.Add(control);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}