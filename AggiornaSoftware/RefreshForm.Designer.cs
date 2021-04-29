namespace AggiornaSoftware
{
    partial class RefreshForm
    {
        /// <summary>
        /// Variabile di progettazione necessaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Pulire le risorse in uso.
        /// </summary>
        /// <param name="disposing">ha valore true se le risorse gestite devono essere eliminate, false in caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Codice generato da Progettazione Windows Form

        /// <summary>
        /// Metodo necessario per il supporto della finestra di progettazione. Non modificare
        /// il contenuto del metodo con l'editor di codice.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RefreshForm));
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.reportFiles = new System.Windows.Forms.ListView();
            this.pluginLabel = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.programmaLabel = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.dataLast = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.statuslbl = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.strButton = new System.Windows.Forms.Button();
            this.btnRefreshSoluzione = new System.Windows.Forms.Button();
            this.gbSoftware = new System.Windows.Forms.GroupBox();
            this.lvSoftware = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnInstall = new System.Windows.Forms.Button();
            this.btnDisinstall = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.gbSoftware.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(6, 22);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(130, 42);
            this.button2.TabIndex = 3;
            this.button2.Text = "Aggiorna";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(142, 22);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(132, 42);
            this.button3.TabIndex = 4;
            this.button3.Text = "Sostituisci";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.reportFiles);
            this.groupBox3.Location = new System.Drawing.Point(12, 157);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(686, 239);
            this.groupBox3.TabIndex = 7;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Analisi";
            // 
            // reportFiles
            // 
            this.reportFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.reportFiles.CheckBoxes = true;
            this.reportFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.pluginLabel,
            this.programmaLabel,
            this.dataLast,
            this.statuslbl});
            this.reportFiles.GridLines = true;
            this.reportFiles.HideSelection = false;
            this.reportFiles.Location = new System.Drawing.Point(6, 19);
            this.reportFiles.Name = "reportFiles";
            this.reportFiles.Size = new System.Drawing.Size(672, 214);
            this.reportFiles.TabIndex = 7;
            this.reportFiles.UseCompatibleStateImageBehavior = false;
            this.reportFiles.View = System.Windows.Forms.View.Details;
            // 
            // pluginLabel
            // 
            this.pluginLabel.Text = "Nome";
            this.pluginLabel.Width = 140;
            // 
            // programmaLabel
            // 
            this.programmaLabel.Text = "Path";
            this.programmaLabel.Width = 254;
            // 
            // dataLast
            // 
            this.dataLast.Text = "Ultima data";
            this.dataLast.Width = 150;
            // 
            // statuslbl
            // 
            this.statuslbl.Text = "Stato";
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar1});
            this.toolStrip1.Location = new System.Drawing.Point(0, 564);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(717, 25);
            this.toolStrip1.TabIndex = 8;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(100, 22);
            // 
            // strButton
            // 
            this.strButton.Location = new System.Drawing.Point(566, 22);
            this.strButton.Name = "strButton";
            this.strButton.Size = new System.Drawing.Size(112, 42);
            this.strButton.TabIndex = 9;
            this.strButton.Text = "Aggiungi a startup";
            this.strButton.UseVisualStyleBackColor = true;
            this.strButton.Click += new System.EventHandler(this.strButton_Click);
            // 
            // btnRefreshSoluzione
            // 
            this.btnRefreshSoluzione.Location = new System.Drawing.Point(6, 19);
            this.btnRefreshSoluzione.Name = "btnRefreshSoluzione";
            this.btnRefreshSoluzione.Size = new System.Drawing.Size(130, 41);
            this.btnRefreshSoluzione.TabIndex = 10;
            this.btnRefreshSoluzione.Text = "Aggiorna Server";
            this.btnRefreshSoluzione.UseVisualStyleBackColor = true;
            this.btnRefreshSoluzione.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // gbSoftware
            // 
            this.gbSoftware.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbSoftware.Controls.Add(this.lvSoftware);
            this.gbSoftware.Location = new System.Drawing.Point(12, 12);
            this.gbSoftware.Name = "gbSoftware";
            this.gbSoftware.Size = new System.Drawing.Size(686, 139);
            this.gbSoftware.TabIndex = 8;
            this.gbSoftware.TabStop = false;
            this.gbSoftware.Text = "Software";
            // 
            // lvSoftware
            // 
            this.lvSoftware.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvSoftware.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader4});
            this.lvSoftware.GridLines = true;
            this.lvSoftware.HideSelection = false;
            this.lvSoftware.Location = new System.Drawing.Point(6, 19);
            this.lvSoftware.Name = "lvSoftware";
            this.lvSoftware.Size = new System.Drawing.Size(672, 114);
            this.lvSoftware.TabIndex = 7;
            this.lvSoftware.UseCompatibleStateImageBehavior = false;
            this.lvSoftware.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Nome";
            this.columnHeader1.Width = 140;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Path";
            this.columnHeader2.Width = 254;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Stato";
            // 
            // btnInstall
            // 
            this.btnInstall.Location = new System.Drawing.Point(429, 22);
            this.btnInstall.Name = "btnInstall";
            this.btnInstall.Size = new System.Drawing.Size(131, 42);
            this.btnInstall.TabIndex = 11;
            this.btnInstall.Text = "Installa non installati";
            this.btnInstall.UseVisualStyleBackColor = true;
            this.btnInstall.Click += new System.EventHandler(this.btnInstall_Click);
            // 
            // btnDisinstall
            // 
            this.btnDisinstall.Location = new System.Drawing.Point(280, 22);
            this.btnDisinstall.Name = "btnDisinstall";
            this.btnDisinstall.Size = new System.Drawing.Size(143, 42);
            this.btnDisinstall.TabIndex = 12;
            this.btnDisinstall.Text = "Disinstalla installati";
            this.btnDisinstall.UseVisualStyleBackColor = true;
            this.btnDisinstall.Click += new System.EventHandler(this.btnDisinstall_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.strButton);
            this.groupBox1.Controls.Add(this.button2);
            this.groupBox1.Controls.Add(this.btnDisinstall);
            this.groupBox1.Controls.Add(this.btnInstall);
            this.groupBox1.Controls.Add(this.button3);
            this.groupBox1.Location = new System.Drawing.Point(12, 402);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(686, 77);
            this.groupBox1.TabIndex = 13;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Azioni";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.btnRefreshSoluzione);
            this.groupBox2.Location = new System.Drawing.Point(12, 485);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(686, 70);
            this.groupBox2.TabIndex = 14;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Comandi SuperUser";
            // 
            // RefreshForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(717, 589);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.gbSoftware);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.groupBox3);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(733, 628);
            this.Name = "RefreshForm";
            this.Text = "Aggiorna Plugin";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RefreshForm_FormClosing);
            this.Load += new System.EventHandler(this.RefreshForm_Load);
            this.groupBox3.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.gbSoftware.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ColumnHeader pluginLabel;
        private System.Windows.Forms.ColumnHeader programmaLabel;
        private System.Windows.Forms.ColumnHeader dataLast;
        public System.Windows.Forms.ListView reportFiles;
        private System.Windows.Forms.ColumnHeader statuslbl;
        private System.Windows.Forms.ToolStrip toolStrip1;
        public System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.Button strButton;
        private System.Windows.Forms.Button btnRefreshSoluzione;
        private System.Windows.Forms.GroupBox gbSoftware;
        public System.Windows.Forms.ListView lvSoftware;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.Button btnInstall;
        private System.Windows.Forms.Button btnDisinstall;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
    }
}

