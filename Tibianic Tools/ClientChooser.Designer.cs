namespace TibianicTools
{
    partial class ClientChooser
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ClientChooser));
            this.picboxClose = new System.Windows.Forms.PictureBox();
            this.picboxMinimize = new System.Windows.Forms.PictureBox();
            this.comboboxClients = new System.Windows.Forms.ComboBox();
            this.btnChoose = new System.Windows.Forms.Button();
            this.lblTitle = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picboxClose)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picboxMinimize)).BeginInit();
            this.SuspendLayout();
            // 
            // picboxClose
            // 
            this.picboxClose.BackColor = System.Drawing.Color.Transparent;
            this.picboxClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picboxClose.Location = new System.Drawing.Point(128, 2);
            this.picboxClose.Name = "picboxClose";
            this.picboxClose.Size = new System.Drawing.Size(51, 19);
            this.picboxClose.TabIndex = 5;
            this.picboxClose.TabStop = false;
            this.picboxClose.MouseClick += new System.Windows.Forms.MouseEventHandler(this.picboxClose_MouseClick);
            // 
            // picboxMinimize
            // 
            this.picboxMinimize.BackColor = System.Drawing.Color.Transparent;
            this.picboxMinimize.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picboxMinimize.Location = new System.Drawing.Point(106, 0);
            this.picboxMinimize.Name = "picboxMinimize";
            this.picboxMinimize.Size = new System.Drawing.Size(37, 19);
            this.picboxMinimize.TabIndex = 4;
            this.picboxMinimize.TabStop = false;
            this.picboxMinimize.MouseClick += new System.Windows.Forms.MouseEventHandler(this.picboxMinimize_MouseClick);
            // 
            // comboboxClients
            // 
            this.comboboxClients.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.comboboxClients.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboboxClients.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.comboboxClients.FormattingEnabled = true;
            this.comboboxClients.Location = new System.Drawing.Point(2, 25);
            this.comboboxClients.Name = "comboboxClients";
            this.comboboxClients.Size = new System.Drawing.Size(121, 19);
            this.comboboxClients.TabIndex = 6;
            this.comboboxClients.Click += new System.EventHandler(this.comboboxClients_Click);
            // 
            // btnChoose
            // 
            this.btnChoose.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.btnChoose.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnChoose.BackgroundImage")));
            this.btnChoose.Font = new System.Drawing.Font("Tahoma", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnChoose.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnChoose.Location = new System.Drawing.Point(128, 25);
            this.btnChoose.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnChoose.Name = "btnChoose";
            this.btnChoose.Size = new System.Drawing.Size(46, 19);
            this.btnChoose.TabIndex = 35;
            this.btnChoose.Text = "Choose";
            this.btnChoose.UseVisualStyleBackColor = false;
            this.btnChoose.Click += new System.EventHandler(this.btnChoose_Click);
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblTitle.ForeColor = System.Drawing.Color.Silver;
            this.lblTitle.Location = new System.Drawing.Point(4, 6);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(61, 11);
            this.lblTitle.TabIndex = 36;
            this.lblTitle.Text = "Tibianic Tools";
            // 
            // ClientChooser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(5F, 11F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.BackgroundImage = global::TibianicTools.Properties.Resources.background_black;
            this.ClientSize = new System.Drawing.Size(177, 51);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.btnChoose);
            this.Controls.Add(this.comboboxClients);
            this.Controls.Add(this.picboxClose);
            this.Controls.Add(this.picboxMinimize);
            this.Font = new System.Drawing.Font("Tahoma", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.Name = "ClientChooser";
            this.Text = "Tibianic Tools";
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ClientChooser_MouseUp);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ClientChooser_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ClientChooser_MouseMove);
            ((System.ComponentModel.ISupportInitialize)(this.picboxClose)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picboxMinimize)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picboxClose;
        private System.Windows.Forms.PictureBox picboxMinimize;
        private System.Windows.Forms.ComboBox comboboxClients;
        private System.Windows.Forms.Button btnChoose;
        private System.Windows.Forms.Label lblTitle;
    }
}