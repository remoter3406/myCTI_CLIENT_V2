namespace myCTI2
{
    partial class CTIausgabe
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CTIausgabe));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.labelUrsprung = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.anrufer = new System.Windows.Forms.Label();
            this.ursprung = new System.Windows.Forms.Label();
            this.ziel = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.timerRing = new System.Windows.Forms.Timer(this.components);
            this.timerTopMost = new System.Windows.Forms.Timer(this.components);
            this.Kampagne = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Black;
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pictureBox1.Image = global::myCTI2.Resource1.myCTI1;
            this.pictureBox1.InitialImage = global::myCTI2.Resource1.myCTI1;
            this.pictureBox1.Location = new System.Drawing.Point(12, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(200, 200);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F);
            this.label1.Location = new System.Drawing.Point(228, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 29);
            this.label1.TabIndex = 1;
            this.label1.Text = "Anrufer:";
            // 
            // labelUrsprung
            // 
            this.labelUrsprung.AutoSize = true;
            this.labelUrsprung.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F);
            this.labelUrsprung.Location = new System.Drawing.Point(278, 93);
            this.labelUrsprung.Name = "labelUrsprung";
            this.labelUrsprung.Size = new System.Drawing.Size(46, 29);
            this.labelUrsprung.TabIndex = 2;
            this.labelUrsprung.Text = "für:";
            this.labelUrsprung.Click += new System.EventHandler(this.labelUrsprung_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F);
            this.label2.Location = new System.Drawing.Point(229, 183);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(95, 29);
            this.label2.TabIndex = 3;
            this.label2.Text = "Ruf bei:";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // anrufer
            // 
            this.anrufer.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F);
            this.anrufer.Location = new System.Drawing.Point(330, 16);
            this.anrufer.Name = "anrufer";
            this.anrufer.Size = new System.Drawing.Size(537, 69);
            this.anrufer.TabIndex = 4;
            this.anrufer.Click += new System.EventHandler(this.anrufer_Click);
            // 
            // ursprung
            // 
            this.ursprung.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F);
            this.ursprung.Location = new System.Drawing.Point(330, 93);
            this.ursprung.Name = "ursprung";
            this.ursprung.Size = new System.Drawing.Size(537, 33);
            this.ursprung.TabIndex = 5;
            this.ursprung.Click += new System.EventHandler(this.ursprung_Click);
            // 
            // ziel
            // 
            this.ziel.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F);
            this.ziel.Location = new System.Drawing.Point(330, 179);
            this.ziel.Name = "ziel";
            this.ziel.Size = new System.Drawing.Size(537, 33);
            this.ziel.TabIndex = 6;
            this.ziel.Click += new System.EventHandler(this.ziel_Click);
            // 
            // timer1
            // 
            this.timer1.Interval = 15000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // timerRing
            // 
            this.timerRing.Interval = 4000;
            this.timerRing.Tick += new System.EventHandler(this.timerRing_Tick);
            // 
            // timerTopMost
            // 
            this.timerTopMost.Interval = 2000;
            this.timerTopMost.Tick += new System.EventHandler(this.timerTopMost_Tick);
            // 
            // Kampagne
            // 
            this.Kampagne.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F);
            this.Kampagne.ForeColor = System.Drawing.Color.Red;
            this.Kampagne.Location = new System.Drawing.Point(234, 135);
            this.Kampagne.Name = "Kampagne";
            this.Kampagne.Size = new System.Drawing.Size(633, 33);
            this.Kampagne.TabIndex = 7;
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(233, 215);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(211, 31);
            this.button1.TabIndex = 8;
            this.button1.Text = "Weiterleiten zu ";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.Location = new System.Drawing.Point(450, 215);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(211, 31);
            this.button2.TabIndex = 9;
            this.button2.Text = "Weiterleiten zu ";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Visible = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button3.Location = new System.Drawing.Point(667, 215);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(211, 31);
            this.button3.TabIndex = 10;
            this.button3.Text = "Weiterleiten zu ";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Visible = false;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // CTIausgabe
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.ClientSize = new System.Drawing.Size(886, 251);
            this.ControlBox = false;
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.Kampagne);
            this.Controls.Add(this.ziel);
            this.Controls.Add(this.ursprung);
            this.Controls.Add(this.anrufer);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.labelUrsprung);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CTIausgabe";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Anruf";
            this.Load += new System.EventHandler(this.CTIausgabe_Load);
            this.VisibleChanged += new System.EventHandler(this.CTIausgabe_VisibleChanged);
            this.Click += new System.EventHandler(this.CTIausgabe_Click);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelUrsprung;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label anrufer;
        private System.Windows.Forms.Label ursprung;
        private System.Windows.Forms.Label ziel;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Timer timerRing;
        private System.Windows.Forms.Timer timerTopMost;
        private System.Windows.Forms.Label Kampagne;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
    }
}