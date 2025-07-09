namespace HOMEPAGE
{
    partial class PreviewDataPelanggan
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
            this.dgvPreviewPelanggan = new System.Windows.Forms.DataGridView();
            this.OKE = new System.Windows.Forms.Button();
            this.IMPORT = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPreviewPelanggan)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvPreviewPelanggan
            // 
            this.dgvPreviewPelanggan.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPreviewPelanggan.Location = new System.Drawing.Point(47, 46);
            this.dgvPreviewPelanggan.Name = "dgvPreviewPelanggan";
            this.dgvPreviewPelanggan.RowHeadersWidth = 51;
            this.dgvPreviewPelanggan.RowTemplate.Height = 24;
            this.dgvPreviewPelanggan.Size = new System.Drawing.Size(710, 301);
            this.dgvPreviewPelanggan.TabIndex = 0;
            this.dgvPreviewPelanggan.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvPreviewPelanggan_CellContentClick);
            // 
            // OKE
            // 
            this.OKE.Font = new System.Drawing.Font("Times New Roman", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OKE.Location = new System.Drawing.Point(232, 369);
            this.OKE.Name = "OKE";
            this.OKE.Size = new System.Drawing.Size(127, 39);
            this.OKE.TabIndex = 1;
            this.OKE.Text = "OKE";
            this.OKE.UseVisualStyleBackColor = true;
            this.OKE.Click += new System.EventHandler(this.OKE_Click);
            // 
            // IMPORT
            // 
            this.IMPORT.Font = new System.Drawing.Font("Times New Roman", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IMPORT.Location = new System.Drawing.Point(377, 369);
            this.IMPORT.Name = "IMPORT";
            this.IMPORT.Size = new System.Drawing.Size(127, 39);
            this.IMPORT.TabIndex = 2;
            this.IMPORT.Text = "IMPORT";
            this.IMPORT.UseVisualStyleBackColor = true;
            this.IMPORT.Click += new System.EventHandler(this.IMPORT_Click);
            // 
            // PreviewDataPelanggan
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Azure;
            this.ClientSize = new System.Drawing.Size(800, 434);
            this.Controls.Add(this.IMPORT);
            this.Controls.Add(this.OKE);
            this.Controls.Add(this.dgvPreviewPelanggan);
            this.Name = "PreviewDataPelanggan";
            this.Text = "Preview_Data_Pelanggan";
            ((System.ComponentModel.ISupportInitialize)(this.dgvPreviewPelanggan)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvPreviewPelanggan;
        private System.Windows.Forms.Button OKE;
        private System.Windows.Forms.Button IMPORT;
    }
}