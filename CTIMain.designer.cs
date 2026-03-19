namespace myCTI2
{
    partial class CTIMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CTIMain));
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.buttonHelp = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.buttonShowNumber = new System.Windows.Forms.Button();
            this.buttonHideNumber = new System.Windows.Forms.Button();
            this.buttonHookOn = new System.Windows.Forms.Button();
            this.buttonSpeaker = new System.Windows.Forms.Button();
            this.buttonIM = new System.Windows.Forms.Button();
            this.buttonSearch = new System.Windows.Forms.Button();
            this.buttonBack = new System.Windows.Forms.Button();
            this.buttonZeitplan = new System.Windows.Forms.Button();
            this.buttonHome = new System.Windows.Forms.Button();
            this.buttonForward = new System.Windows.Forms.Button();
            this.buttonUpdate = new System.Windows.Forms.Button();
            this.labelRDP = new System.Windows.Forms.Label();
            this.checkBoxKampagne = new System.Windows.Forms.CheckBox();
            this.checkBoxSecondPhone = new System.Windows.Forms.CheckBox();
            this.webView2 = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)(this.webView2)).BeginInit();
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // buttonHelp
            // 
            this.buttonHelp.FlatAppearance.BorderSize = 0;
            this.buttonHelp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonHelp.ForeColor = System.Drawing.SystemColors.Control;
            this.buttonHelp.Image = global::myCTI2.Resource1.help;
            this.buttonHelp.Location = new System.Drawing.Point(0, 1);
            this.buttonHelp.Name = "buttonHelp";
            this.buttonHelp.Size = new System.Drawing.Size(28, 24);
            this.buttonHelp.TabIndex = 1;
            this.buttonHelp.UseVisualStyleBackColor = true;
            this.buttonHelp.Click += new System.EventHandler(this.buttonHelp_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(31, 1);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(107, 22);
            this.textBox1.TabIndex = 2;
            this.textBox1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress_1);
            // 
            // buttonShowNumber
            // 
            this.buttonShowNumber.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.buttonShowNumber.ForeColor = System.Drawing.Color.ForestGreen;
            this.buttonShowNumber.Location = new System.Drawing.Point(144, 1);
            this.buttonShowNumber.Name = "buttonShowNumber";
            this.buttonShowNumber.Size = new System.Drawing.Size(36, 24);
            this.buttonShowNumber.TabIndex = 3;
            this.buttonShowNumber.Text = "Nr.";
            this.buttonShowNumber.UseVisualStyleBackColor = true;
            this.buttonShowNumber.Click += new System.EventHandler(this.buttonShowNumber_Click);
            // 
            // buttonHideNumber
            // 
            this.buttonHideNumber.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Strikeout))));
            this.buttonHideNumber.ForeColor = System.Drawing.Color.Red;
            this.buttonHideNumber.Location = new System.Drawing.Point(186, 1);
            this.buttonHideNumber.Name = "buttonHideNumber";
            this.buttonHideNumber.Size = new System.Drawing.Size(36, 24);
            this.buttonHideNumber.TabIndex = 4;
            this.buttonHideNumber.Text = "Nr.";
            this.buttonHideNumber.UseVisualStyleBackColor = true;
            this.buttonHideNumber.Click += new System.EventHandler(this.buttonHideNumber_Click);
            // 
            // buttonHookOn
            // 
            this.buttonHookOn.FlatAppearance.BorderSize = 0;
            this.buttonHookOn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonHookOn.ForeColor = System.Drawing.SystemColors.Control;
            this.buttonHookOn.Image = global::myCTI2.Resource1.hookon;
            this.buttonHookOn.Location = new System.Drawing.Point(239, 1);
            this.buttonHookOn.Name = "buttonHookOn";
            this.buttonHookOn.Size = new System.Drawing.Size(23, 21);
            this.buttonHookOn.TabIndex = 5;
            this.buttonHookOn.UseVisualStyleBackColor = true;
            this.buttonHookOn.Click += new System.EventHandler(this.buttonHookOn_Click);
            // 
            // buttonSpeaker
            // 
            this.buttonSpeaker.FlatAppearance.BorderSize = 0;
            this.buttonSpeaker.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonSpeaker.ForeColor = System.Drawing.SystemColors.Control;
            this.buttonSpeaker.Image = global::myCTI2.Resource1.lsp;
            this.buttonSpeaker.Location = new System.Drawing.Point(278, 3);
            this.buttonSpeaker.Name = "buttonSpeaker";
            this.buttonSpeaker.Size = new System.Drawing.Size(21, 19);
            this.buttonSpeaker.TabIndex = 6;
            this.buttonSpeaker.UseVisualStyleBackColor = true;
            this.buttonSpeaker.Click += new System.EventHandler(this.buttonSpeaker_Click);
            // 
            // buttonIM
            // 
            this.buttonIM.FlatAppearance.BorderSize = 0;
            this.buttonIM.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonIM.ForeColor = System.Drawing.SystemColors.Control;
            this.buttonIM.Image = global::myCTI2.Resource1.IM;
            this.buttonIM.Location = new System.Drawing.Point(346, 1);
            this.buttonIM.Name = "buttonIM";
            this.buttonIM.Size = new System.Drawing.Size(32, 22);
            this.buttonIM.TabIndex = 7;
            this.buttonIM.UseVisualStyleBackColor = true;
            this.buttonIM.Click += new System.EventHandler(this.buttonIM_Click);
            // 
            // buttonSearch
            // 
            this.buttonSearch.FlatAppearance.BorderSize = 0;
            this.buttonSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonSearch.ForeColor = System.Drawing.SystemColors.Control;
            this.buttonSearch.Image = global::myCTI2.Resource1.suche;
            this.buttonSearch.Location = new System.Drawing.Point(384, 1);
            this.buttonSearch.Name = "buttonSearch";
            this.buttonSearch.Size = new System.Drawing.Size(32, 23);
            this.buttonSearch.TabIndex = 8;
            this.buttonSearch.UseVisualStyleBackColor = true;
            this.buttonSearch.Click += new System.EventHandler(this.buttonSearch_Click);
            // 
            // buttonBack
            // 
            this.buttonBack.FlatAppearance.BorderSize = 0;
            this.buttonBack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonBack.ForeColor = System.Drawing.SystemColors.Control;
            this.buttonBack.Image = global::myCTI2.Resource1.Backward;
            this.buttonBack.Location = new System.Drawing.Point(530, 3);
            this.buttonBack.Name = "buttonBack";
            this.buttonBack.Size = new System.Drawing.Size(20, 22);
            this.buttonBack.TabIndex = 9;
            this.buttonBack.UseVisualStyleBackColor = true;
            this.buttonBack.Click += new System.EventHandler(this.buttonBack_Click);
            // 
            // buttonZeitplan
            // 
            this.buttonZeitplan.FlatAppearance.BorderSize = 0;
            this.buttonZeitplan.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonZeitplan.ForeColor = System.Drawing.SystemColors.Control;
            this.buttonZeitplan.Image = ((System.Drawing.Image)(resources.GetObject("buttonZeitplan.Image")));
            this.buttonZeitplan.Location = new System.Drawing.Point(433, 3);
            this.buttonZeitplan.Name = "buttonZeitplan";
            this.buttonZeitplan.Size = new System.Drawing.Size(20, 22);
            this.buttonZeitplan.TabIndex = 10;
            this.buttonZeitplan.UseVisualStyleBackColor = true;
            this.buttonZeitplan.Click += new System.EventHandler(this.buttonZeitplan_Click);
            // 
            // buttonHome
            // 
            this.buttonHome.FlatAppearance.BorderSize = 0;
            this.buttonHome.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonHome.ForeColor = System.Drawing.SystemColors.Control;
            this.buttonHome.Image = global::myCTI2.Resource1.home;
            this.buttonHome.Location = new System.Drawing.Point(504, 3);
            this.buttonHome.Name = "buttonHome";
            this.buttonHome.Size = new System.Drawing.Size(20, 22);
            this.buttonHome.TabIndex = 11;
            this.buttonHome.UseVisualStyleBackColor = true;
            this.buttonHome.Click += new System.EventHandler(this.buttonHome_Click);
            // 
            // buttonForward
            // 
            this.buttonForward.FlatAppearance.BorderSize = 0;
            this.buttonForward.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonForward.ForeColor = System.Drawing.SystemColors.Control;
            this.buttonForward.Image = global::myCTI2.Resource1.Forward;
            this.buttonForward.Location = new System.Drawing.Point(556, 3);
            this.buttonForward.Name = "buttonForward";
            this.buttonForward.Size = new System.Drawing.Size(20, 22);
            this.buttonForward.TabIndex = 12;
            this.buttonForward.UseVisualStyleBackColor = true;
            this.buttonForward.Click += new System.EventHandler(this.buttonForward_Click);
            // 
            // buttonUpdate
            // 
            this.buttonUpdate.FlatAppearance.BorderSize = 0;
            this.buttonUpdate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonUpdate.ForeColor = System.Drawing.SystemColors.Control;
            this.buttonUpdate.Image = global::myCTI2.Resource1.refresh;
            this.buttonUpdate.Location = new System.Drawing.Point(478, 3);
            this.buttonUpdate.Name = "buttonUpdate";
            this.buttonUpdate.Size = new System.Drawing.Size(20, 22);
            this.buttonUpdate.TabIndex = 13;
            this.buttonUpdate.UseVisualStyleBackColor = true;
            this.buttonUpdate.Click += new System.EventHandler(this.buttonUpdate_Click);
            // 
            // labelRDP
            // 
            this.labelRDP.AutoSize = true;
            this.labelRDP.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelRDP.ForeColor = System.Drawing.Color.Red;
            this.labelRDP.Location = new System.Drawing.Point(894, 9);
            this.labelRDP.Name = "labelRDP";
            this.labelRDP.Size = new System.Drawing.Size(208, 18);
            this.labelRDP.TabIndex = 14;
            this.labelRDP.Text = "REMOTEDESKTOP AKTIV";
            this.labelRDP.Visible = false;
            // 
            // checkBoxKampagne
            // 
            this.checkBoxKampagne.AutoSize = true;
            this.checkBoxKampagne.Location = new System.Drawing.Point(621, 12);
            this.checkBoxKampagne.Name = "checkBoxKampagne";
            this.checkBoxKampagne.Size = new System.Drawing.Size(77, 17);
            this.checkBoxKampagne.TabIndex = 15;
            this.checkBoxKampagne.Text = "Kampange";
            this.checkBoxKampagne.UseVisualStyleBackColor = true;
            this.checkBoxKampagne.Visible = false;
            this.checkBoxKampagne.CheckedChanged += new System.EventHandler(this.checkBoxKampagne_CheckedChanged);
            // 
            // checkBoxSecondPhone
            // 
            this.checkBoxSecondPhone.AutoSize = true;
            this.checkBoxSecondPhone.Location = new System.Drawing.Point(737, 12);
            this.checkBoxSecondPhone.Name = "checkBoxSecondPhone";
            this.checkBoxSecondPhone.Size = new System.Drawing.Size(91, 17);
            this.checkBoxSecondPhone.TabIndex = 16;
            this.checkBoxSecondPhone.Text = "Zweit Telefon";
            this.checkBoxSecondPhone.UseVisualStyleBackColor = true;
            this.checkBoxSecondPhone.Visible = false;
            this.checkBoxSecondPhone.CheckedChanged += new System.EventHandler(this.checkBoxSecondPhone_CheckedChanged);
            // 
            // webView2
            // 
            this.webView2.AllowExternalDrop = true;
            this.webView2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.webView2.CreationProperties = null;
            this.webView2.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webView2.Location = new System.Drawing.Point(0, 31);
            this.webView2.Name = "webView2";
            this.webView2.Size = new System.Drawing.Size(1820, 956);
            this.webView2.TabIndex = 18;
            this.webView2.ZoomFactor = 1D;
            // 
            // CTIMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1820, 987);
            this.Controls.Add(this.webView2);
            this.Controls.Add(this.checkBoxSecondPhone);
            this.Controls.Add(this.checkBoxKampagne);
            this.Controls.Add(this.labelRDP);
            this.Controls.Add(this.buttonUpdate);
            this.Controls.Add(this.buttonForward);
            this.Controls.Add(this.buttonHome);
            this.Controls.Add(this.buttonZeitplan);
            this.Controls.Add(this.buttonBack);
            this.Controls.Add(this.buttonSearch);
            this.Controls.Add(this.buttonIM);
            this.Controls.Add(this.buttonSpeaker);
            this.Controls.Add(this.buttonHookOn);
            this.Controls.Add(this.buttonHideNumber);
            this.Controls.Add(this.buttonShowNumber);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.buttonHelp);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CTIMain";
            this.Text = "myCTI Client V2.1.1";
            this.SizeChanged += new System.EventHandler(this.CTIMain_SizeChanged);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.CTIMain_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.webView2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button buttonHelp;
        private System.Windows.Forms.Button buttonShowNumber;
        private System.Windows.Forms.Button buttonHideNumber;
        private System.Windows.Forms.Button buttonHookOn;
        private System.Windows.Forms.Button buttonSpeaker;
        private System.Windows.Forms.Button buttonIM;
        private System.Windows.Forms.Button buttonSearch;
        private System.Windows.Forms.Button buttonBack;
        internal System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button buttonZeitplan;
        private System.Windows.Forms.Button buttonHome;
        private System.Windows.Forms.Button buttonForward;
        private System.Windows.Forms.Button buttonUpdate;
        private System.Windows.Forms.Label labelRDP;
        private System.Windows.Forms.CheckBox checkBoxKampagne;
        private System.Windows.Forms.CheckBox checkBoxSecondPhone;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView2;
    }
}