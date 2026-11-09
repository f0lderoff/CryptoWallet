namespace CryptoWallet
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.content_Panel = new System.Windows.Forms.Panel();
            this.top_Panel = new System.Windows.Forms.Panel();
            this.reconDb_Button = new SiticoneNetFrameworkUI.SiticoneExpressiveButton();
            this.top_Panel.SuspendLayout();
            this.SuspendLayout();
            // 
            // content_Panel
            // 
            resources.ApplyResources(this.content_Panel, "content_Panel");
            this.content_Panel.Name = "content_Panel";
            // 
            // top_Panel
            // 
            this.top_Panel.Controls.Add(this.reconDb_Button);
            resources.ApplyResources(this.top_Panel, "top_Panel");
            this.top_Panel.Name = "top_Panel";
            // 
            // reconDb_Button
            // 
            resources.ApplyResources(this.reconDb_Button, "reconDb_Button");
            this.reconDb_Button.BorderColor = System.Drawing.Color.Transparent;
            this.reconDb_Button.BorderSize = 0;
            this.reconDb_Button.CanBeep = true;
            this.reconDb_Button.CanShake = true;
            this.reconDb_Button.Cursor = System.Windows.Forms.Cursors.Default;
            this.reconDb_Button.DialogResult = System.Windows.Forms.DialogResult.None;
            this.reconDb_Button.EnableExpressions = true;
            this.reconDb_Button.EnableKeyboardSupport = false;
            this.reconDb_Button.FillColor = System.Drawing.Color.Black;
            this.reconDb_Button.FillColor2 = System.Drawing.Color.DimGray;
            this.reconDb_Button.ForeColorCustom = System.Drawing.Color.White;
            this.reconDb_Button.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            this.reconDb_Button.HoverFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(40)))), ((int)(((byte)(108)))));
            this.reconDb_Button.HoverFillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(206)))), ((int)(((byte)(26)))), ((int)(((byte)(98)))));
            this.reconDb_Button.HoverForeColor = System.Drawing.Color.White;
            this.reconDb_Button.IsReadOnly = false;
            this.reconDb_Button.LeftIcon = null;
            this.reconDb_Button.LeftIconMargin = 15;
            this.reconDb_Button.LeftIconSize = new System.Drawing.Size(20, 20);
            this.reconDb_Button.LiquidEffect = true;
            this.reconDb_Button.Name = "reconDb_Button";
            this.reconDb_Button.PressedBorderRadius = 10;
            this.reconDb_Button.PressFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(143)))), ((int)(((byte)(14)))), ((int)(((byte)(60)))));
            this.reconDb_Button.PressFillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(15)))), ((int)(((byte)(55)))));
            this.reconDb_Button.PressForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.reconDb_Button.RightIcon = null;
            this.reconDb_Button.RightIconMargin = 15;
            this.reconDb_Button.RightIconSize = new System.Drawing.Size(20, 20);
            this.reconDb_Button.RippleColor = System.Drawing.Color.White;
            this.reconDb_Button.SqueezeOnPress = true;
            this.reconDb_Button.TextLeftMargin = 0;
            this.reconDb_Button.TextRightMargin = 0;
            this.reconDb_Button.Click += new System.EventHandler(this.reconDb_Button_Click);
            // 
            // Form1
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.top_Panel);
            this.Controls.Add(this.content_Panel);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.top_Panel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel content_Panel;
        private SiticoneNetFrameworkUI.SiticoneExpressiveButton reconDb_Button;
        private System.Windows.Forms.Panel top_Panel;
    }
}

