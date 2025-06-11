namespace HOMEPAGE
{
    partial class PELANGGAN
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
            this.dgvPelanggan = new System.Windows.Forms.DataGridView();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.btnHapus = new System.Windows.Forms.Button();
            this.btnTambah = new System.Windows.Forms.Button();
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.txtNama = new System.Windows.Forms.TextBox();
            this.txtIdPelanggan = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnKEMBALI = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.import = new System.Windows.Forms.Button();
            this.Analisis = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPelanggan)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvPelanggan
            // 
            this.dgvPelanggan.BackgroundColor = System.Drawing.Color.White;
            this.dgvPelanggan.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPelanggan.GridColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.dgvPelanggan.Location = new System.Drawing.Point(454, 82);
            this.dgvPelanggan.Name = "dgvPelanggan";
            this.dgvPelanggan.RowHeadersWidth = 51;
            this.dgvPelanggan.RowTemplate.Height = 24;
            this.dgvPelanggan.Size = new System.Drawing.Size(472, 410);
            this.dgvPelanggan.TabIndex = 32;
            this.dgvPelanggan.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvPelanggan_CellClick);
            this.dgvPelanggan.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvPelanggan_CellContentClick);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Font = new System.Drawing.Font("Times New Roman", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRefresh.Location = new System.Drawing.Point(358, 314);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(90, 35);
            this.btnRefresh.TabIndex = 31;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnUpdate
            // 
            this.btnUpdate.Font = new System.Drawing.Font("Times New Roman", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnUpdate.Location = new System.Drawing.Point(135, 313);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(98, 35);
            this.btnUpdate.TabIndex = 30;
            this.btnUpdate.Text = "Update";
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // btnHapus
            // 
            this.btnHapus.Font = new System.Drawing.Font("Times New Roman", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnHapus.Location = new System.Drawing.Point(250, 313);
            this.btnHapus.Name = "btnHapus";
            this.btnHapus.Size = new System.Drawing.Size(94, 36);
            this.btnHapus.TabIndex = 29;
            this.btnHapus.Text = "Hapus";
            this.btnHapus.UseVisualStyleBackColor = true;
            this.btnHapus.Click += new System.EventHandler(this.btnHapus_Click);
            // 
            // btnTambah
            // 
            this.btnTambah.Font = new System.Drawing.Font("Times New Roman", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTambah.Location = new System.Drawing.Point(19, 314);
            this.btnTambah.Name = "btnTambah";
            this.btnTambah.Size = new System.Drawing.Size(98, 35);
            this.btnTambah.TabIndex = 28;
            this.btnTambah.Text = "Tambah";
            this.btnTambah.UseVisualStyleBackColor = true;
            this.btnTambah.Click += new System.EventHandler(this.btnTambah_Click);
            // 
            // txtEmail
            // 
            this.txtEmail.Location = new System.Drawing.Point(168, 205);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.Size = new System.Drawing.Size(280, 22);
            this.txtEmail.TabIndex = 27;
            // 
            // txtNama
            // 
            this.txtNama.Location = new System.Drawing.Point(168, 164);
            this.txtNama.Name = "txtNama";
            this.txtNama.Size = new System.Drawing.Size(280, 22);
            this.txtNama.TabIndex = 26;
            // 
            // txtIdPelanggan
            // 
            this.txtIdPelanggan.Location = new System.Drawing.Point(168, 124);
            this.txtIdPelanggan.Name = "txtIdPelanggan";
            this.txtIdPelanggan.Size = new System.Drawing.Size(280, 22);
            this.txtIdPelanggan.TabIndex = 25;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Times New Roman", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(9, 208);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(58, 19);
            this.label3.TabIndex = 24;
            this.label3.Text = "Email ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Times New Roman", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(9, 167);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 19);
            this.label2.TabIndex = 23;
            this.label2.Text = "Nama ";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Times New Roman", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 127);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(105, 19);
            this.label1.TabIndex = 22;
            this.label1.Text = "Id Pelanggan";
            // 
            // btnKEMBALI
            // 
            this.btnKEMBALI.Font = new System.Drawing.Font("Times New Roman", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnKEMBALI.Location = new System.Drawing.Point(12, 461);
            this.btnKEMBALI.Name = "btnKEMBALI";
            this.btnKEMBALI.Size = new System.Drawing.Size(120, 34);
            this.btnKEMBALI.TabIndex = 33;
            this.btnKEMBALI.Text = "KEMBALI";
            this.btnKEMBALI.UseVisualStyleBackColor = true;
            this.btnKEMBALI.Click += new System.EventHandler(this.btnKEMBALI_Click_1);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Sylfaen", 19.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(314, 18);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(341, 44);
            this.label4.TabIndex = 34;
            this.label4.Text = "DATA PELANGGAN";
            // 
            // import
            // 
            this.import.Font = new System.Drawing.Font("Times New Roman", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.import.Location = new System.Drawing.Point(119, 387);
            this.import.Name = "import";
            this.import.Size = new System.Drawing.Size(114, 35);
            this.import.TabIndex = 35;
            this.import.Text = "IMPORT";
            this.import.UseVisualStyleBackColor = true;
            this.import.Click += new System.EventHandler(this.import_Click_1);
            // 
            // Analisis
            // 
            this.Analisis.Font = new System.Drawing.Font("Times New Roman", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Analisis.Location = new System.Drawing.Point(250, 387);
            this.Analisis.Name = "Analisis";
            this.Analisis.Size = new System.Drawing.Size(114, 35);
            this.Analisis.TabIndex = 36;
            this.Analisis.Text = "ANALISIS";
            this.Analisis.UseVisualStyleBackColor = true;
            this.Analisis.Click += new System.EventHandler(this.Analisis_Click);
            // 
            // PELANGGAN
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Azure;
            this.ClientSize = new System.Drawing.Size(938, 504);
            this.Controls.Add(this.Analisis);
            this.Controls.Add(this.import);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btnKEMBALI);
            this.Controls.Add(this.dgvPelanggan);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.btnUpdate);
            this.Controls.Add(this.btnHapus);
            this.Controls.Add(this.btnTambah);
            this.Controls.Add(this.txtEmail);
            this.Controls.Add(this.txtNama);
            this.Controls.Add(this.txtIdPelanggan);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "PELANGGAN";
            this.Text = "PELANGGAN";
            this.Load += new System.EventHandler(this.PELANGGAN_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvPelanggan)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvPelanggan;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.Button btnHapus;
        private System.Windows.Forms.Button btnTambah;
        private System.Windows.Forms.TextBox txtEmail;
        private System.Windows.Forms.TextBox txtNama;
        private System.Windows.Forms.TextBox txtIdPelanggan;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnKEMBALI;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button import;
        private System.Windows.Forms.Button Analisis;
    }
}