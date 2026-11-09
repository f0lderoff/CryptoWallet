using CryptoWallet.Database;
using System;
using System.Windows.Forms;

namespace CryptoWallet
{
    public partial class Coins : UserControl
    {
        public Coins()
        {
            InitializeComponent();
        }

        public void UpdateUser()
        {
            name_Txt.Text = Session.Login;
        }

        public event Action<string> Navigate;

        private void dashboard_Btn_Click(object sender, EventArgs e)
        {
            Navigate?.Invoke("dashboard");
        }
    }
}
