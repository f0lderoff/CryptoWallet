using CryptoWallet.Database;
using System;
using System.Windows.Forms;
using System.Net.Mail;

namespace CryptoWallet
{
    public partial class Authentication : UserControl
    {
        private enum EmailMode
        {
            None,
            Register,
            ResetPassword
        }

        private EmailMode currentMode = EmailMode.None;

        private VerifyEmail verifyEmail;
        private string pendingLogin;
        private string pendingEmail;
        private string pendingPassword;
        private string resetEmail;

        public Authentication()
        {
            InitializeComponent();

            verifyEmail = new VerifyEmail(
                "smtp.gmail.com",
                587,
                "emailverifycryptowallet@gmail.com",
                "uene kciv pkti uebe"
            );

            verifyEmail_Panel.Visible = false;
            chngPass_Panel.Visible = false;
        }

        public event Action<string> Navigate;

        private void ShowPanel(Control panel)
        {
            log_Panel.Visible = false;
            reg_Panel.Visible = false;
            enterEmail_Panel.Visible = false;
            verifyEmail_Panel.Visible = false;
            chngPass_Panel.Visible = false;

            panel.Visible = true;
            panel.BringToFront();
        }

        private void showPass_Btn_CheckedChanged(object sender, EventArgs e)
        {
            pass_TxtBox.UseSystemPasswordChar = !showPass_Btn.Checked;
            pass1_TxtBox.UseSystemPasswordChar = !showPass1_Btn.Checked;
            newPass_TxtBox.UseSystemPasswordChar = !showPass2_Btn.Checked;
        }

        private void hreg_Btn_Click(object sender, EventArgs e)
        {
            ShowPanel(reg_Panel);

            hreg_Btn.Enabled = false;
            hlogin_Btn.Enabled = true;
        }

        private void hlogin_Btn_Click(object sender, EventArgs e)
        {
            ShowPanel(log_Panel);

            hreg_Btn.Enabled = true;
            hlogin_Btn.Enabled = false;
        }

        private void login_Btn_Click(object sender, EventArgs e)
        {
            string login = login_TxtBox.Text.Trim();
            string password = pass1_TxtBox.Text;

            if (Users.Login(login, password))
            {
                Navigate?.Invoke("dashboard");

                login_TxtBox.Clear();
                pass1_TxtBox.Clear();
            }
            else
            {
                MessageBox.Show(
                    "Неверный логин или пароль",
                    "CryptoWallet",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private async void reg_Btn_Click(object sender, EventArgs e)
        {
            string login = nick_TxtBox.Text.Trim();
            string email = email_TxtBox.Text.Trim();
            string password = pass_TxtBox.Text;

            if (string.IsNullOrWhiteSpace(login))
            {
                MessageBox.Show("Введите логин", "CryptoWallet",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (Users.UserExists(login))
            {
                MessageBox.Show("Логин уже занят", "CryptoWallet",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var mail = new MailAddress(email);
            }
            catch
            {
                MessageBox.Show("Введите корректный email", "CryptoWallet",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (Users.EmailExists(email))
            {
                MessageBox.Show("Email уже используется", "CryptoWallet",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (password.Length < 8)
            {
                MessageBox.Show("Пароль минимум 8 символов", "CryptoWallet",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            verifyEmail.GenerateCode();

            reg_Btn.Enabled = false;
            reg_Btn.Text = "Отправляем код...";

            var result = await verifyEmail.SendCodeAsync(email);

            if (!result.Ok)
            {
                MessageBox.Show(
                    result.Error,
                    "CryptoWallet",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            pendingLogin = login;
            pendingEmail = email;
            pendingPassword = password;

            regEmail_Btn.Text = "Зарегистрироваться";
            ShowPanel(verifyEmail_Panel);
            verifyEmail_TxtBox.Clear();

            reg_Btn.Enabled = true;
            reg_Btn.Text = "Зарегистрироваться";
        }

        private void regEmail_Btn_Click(object sender, EventArgs e)
        {
            string code = verifyEmail_TxtBox.Text.Trim();

            if (!verifyEmail.IsValid(code))
            {
                MessageBox.Show(
                    "Неверный или просроченный код",
                    "CryptoWallet",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            if (currentMode == EmailMode.Register)
            {
                bool ok = Users.Register(pendingLogin, pendingEmail, pendingPassword);

                if (!ok)
                {
                    MessageBox.Show(
                        "Ошибка регистрации",
                        "CryptoWallet",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return;
                }

                verifyEmail.Reset();
                currentMode = EmailMode.None;

                pendingLogin = null;
                pendingEmail = null;
                pendingPassword = null;

                ShowPanel(log_Panel);

                Navigate?.Invoke("dashboard");
                return;
            }

            if (currentMode == EmailMode.ResetPassword)
            {
                verifyEmail.Reset();
                currentMode = EmailMode.None;

                verifyEmail_Panel.Visible = false;
                ShowPanel(chngPass_Panel);
                return;
            }
        }

        private async void frgtPass_Btn_Click(object sender, EventArgs e)
        {
            currentMode = EmailMode.ResetPassword;

            enterEmail_TxtBox.Clear();
            ShowPanel(enterEmail_Panel);
        }

        private async void sendCode_Btn_Click(object sender, EventArgs e)
        {
            string email = enterEmail_TxtBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show(
                    "Введите email",
                    "CryptoWallet",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            try
            {
                var mail = new MailAddress(email);
            }
            catch
            {
                MessageBox.Show(
                    "Введите корректный email",
                    "CryptoWallet",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            if (!Users.EmailExists(email))
            {
                MessageBox.Show(
                    "Пользователь с таким email не найден",
                    "CryptoWallet",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            resetEmail = email;

            verifyEmail.GenerateCode();

            sendCode_Btn.Enabled = false;
            sendCode_Btn.Text = "Отправляем код...";

            var result = await verifyEmail.SendCodeAsync(email);

            sendCode_Btn.Enabled = true;
            sendCode_Btn.Text = "Отправить код";

            if (!result.Ok)
            {
                MessageBox.Show(
                    result.Error,
                    "CryptoWallet",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            verifyEmail_TxtBox.Clear();
            regEmail_Btn.Text = "Подтвердить код";
            ShowPanel(verifyEmail_Panel);
        }

        private void chngPass_Btn_Click(object sender, EventArgs e)
        {
            string newPassword = newPass_TxtBox.Text;

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                MessageBox.Show(
                    "Введите новый пароль",
                    "CryptoWallet",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            if (newPassword.Length < 8)
            {
                MessageBox.Show(
                    "Пароль должен содержать минимум 8 символов",
                    "CryptoWallet",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            bool ok = Users.UpdatePasswordByEmail(resetEmail, newPassword);

            if (!ok)
            {
                MessageBox.Show(
                    "Не удалось изменить пароль",
                    "CryptoWallet",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            MessageBox.Show(
                "Пароль успешно изменён",
                "CryptoWallet",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            newPass_TxtBox.Clear();
            resetEmail = null;

            ShowPanel(log_Panel);
        }
    }
}
