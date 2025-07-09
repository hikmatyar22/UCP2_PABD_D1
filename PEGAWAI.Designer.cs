namespace HOMEPAGE
{
    partial class PEGAWAI
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
            this.dgvPegawai = new System.Windows.Forms.DataGridView();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.btnHapus = new System.Windows.Forms.Button();
            this.btnTambah = new System.Windows.Forms.Button();
            this.txtNama = new System.Windows.Forms.TextBox();
            this.txtIdPegawai = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnKEMBALI = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.import = new System.Windows.Forms.Button();
            this.btnAnalisis = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPegawai)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvPegawai
            // 
            this.dgvPegawai.BackgroundColor = System.Drawing.Color.White;
            this.dgvPegawai.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPegawai.GridColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.dgvPegawai.Location = new System.Drawing.Point(462, 126);
            this.dgvPegawai.Name = "dgvPegawai";
            this.dgvPegawai.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.dgvPegawai.RowHeadersWidth = 51;
            this.dgvPegawai.RowTemplate.Height = 24;
            this.dgvPegawai.Size = new System.Drawing.Size(341, 388);
            this.dgvPegawai.TabIndex = 32;
            this.dgvPegawai.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvPegawai_CellClick);
            this.dgvPegawai.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvPegawai_CellContentClick);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Font = new System.Drawing.Font("Times New Roman", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRefresh.Location = new System.Drawing.Point(345, 310);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(105, 38);
            this.btnRefresh.TabIndex = 31;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnUpdate
            // 
            this.btnUpdate.Font = new System.Drawing.Font("Times New Roman", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnUpdate.Location = new System.Drawing.Point(123, 310);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(105, 38);
            this.btnUpdate.TabIndex = 30;
            this.btnUpdate.Text = "Update";
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // btnHapus
            // 
            this.btnHapus.Font = new System.Drawing.Font("Times New Roman", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnHapus.Location = new System.Drawing.Point(234, 310);
            this.btnHapus.Name = "btnHapus";
            this.btnHapus.Size = new System.Drawing.Size(105, 38);
            this.btnHapus.TabIndex = 29;
            this.btnHapus.Text = "Hapus";
            this.btnHapus.UseVisualStyleBackColor = true;
            this.btnHapus.Click += new System.EventHandler(this.btnHapus_Click);
            // 
            // btnTambah
            // 
            this.btnTambah.Font = new System.Drawing.Font("Times New Roman", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTambah.Location = new System.Drawing.Point(12, 310);
            this.btnTambah.Name = "btnTambah";
            this.btnTambah.Size = new System.Drawing.Size(105, 38);
            this.btnTambah.TabIndex = 28;
            this.btnTambah.Text = "Tambah";
            this.btnTambah.UseVisualStyleBackColor = true;
            this.btnTambah.Click += new System.EventHandler(this.btnTambah_Click);
            // 
            // txtNama
            // 
            this.txtNama.Location = new System.Drawing.Point(123, 214);
            this.txtNama.Name = "txtNama";
            this.txtNama.Size = new System.Drawing.Size(305, 22);
            this.txtNama.TabIndex = 26;
            // 
            // txtIdPegawai
            // 
            this.txtIdPegawai.Location = new System.Drawing.Point(123, 168);
            this.txtIdPegawai.MaxLength = 0;
            this.txtIdPegawai.Name = "txtIdPegawai";
            this.txtIdPegawai.Size = new System.Drawing.Size(305, 22);
            this.txtIdPegawai.TabIndex = 25;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Times New Roman", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 214);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 19);
            this.label2.TabIndex = 23;
            this.label2.Text = "Nama ";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Times New Roman", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 168);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 19);
            this.label1.TabIndex = 22;
            this.label1.Text = "Id Pegawai";
            // 
            // btnKEMBALI
            // 
            this.btnKEMBALI.Font = new System.Drawing.Font("Times New Roman", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnKEMBALI.Location = new System.Drawing.Point(12, 477);
            this.btnKEMBALI.Name = "btnKEMBALI";
            this.btnKEMBALI.Size = new System.Drawing.Size(130, 37);
            this.btnKEMBALI.TabIndex = 44;
            this.btnKEMBALI.Text = "KEMBALI";
            this.btnKEMBALI.UseVisualStyleBackColor = true;
            this.btnKEMBALI.Click += new System.EventHandler(this.btnKEMBALI_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Sylfaen", 19.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(277, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(281, 43);
            this.label3.TabIndex = 45;
            this.label3.Text = "DATA PEGAWAI";
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // import
            // 
            this.import.Font = new System.Drawing.Font("Times New Roman", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.import.Location = new System.Drawing.Point(114, 392);
            this.import.Name = "import";
            this.import.Size = new System.Drawing.Size(114, 38);
            this.import.TabIndex = 46;
            this.import.Text = "IMPORT";
            this.import.UseVisualStyleBackColor = true;
            this.import.Click += new System.EventHandler(this.import_Click_1);
            // 
            // btnAnalisis
            // 
            this.btnAnalisis.Font = new System.Drawing.Font("Times New Roman", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAnalisis.Location = new System.Drawing.Point(234, 392);
            this.btnAnalisis.Name = "btnAnalisis";
            this.btnAnalisis.Size = new System.Drawing.Size(114, 38);
            this.btnAnalisis.TabIndex = 47;
            this.btnAnalisis.Text = "ANALISIS";
            this.btnAnalisis.UseVisualStyleBackColor = true;
            this.btnAnalisis.Click += new System.EventHandler(this.btnAnalisis_Click);
            // 
            // PEGAWAI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Azure;
            this.ClientSize = new System.Drawing.Size(815, 526);
            this.Controls.Add(this.btnAnalisis);
            this.Controls.Add(this.import);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnKEMBALI);
            this.Controls.Add(this.dgvPegawai);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.btnUpdate);
            this.Controls.Add(this.btnHapus);
            this.Controls.Add(this.btnTambah);
            this.Controls.Add(this.txtNama);
            this.Controls.Add(this.txtIdPegawai);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "PEGAWAI";
            this.Text = "PEGAWAI";
            this.Load += new System.EventHandler(this.PEGAWAI_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvPegawai)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvPegawai;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.Button btnHapus;
        private System.Windows.Forms.Button btnTambah;
        private System.Windows.Forms.TextBox txtNama;
        private System.Windows.Forms.TextBox txtIdPegawai;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnKEMBALI;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button import;
        private System.Windows.Forms.Button btnAnalisis;
    }
}