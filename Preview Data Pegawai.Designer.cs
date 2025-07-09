namespace HOMEPAGE
{
    partial class PreviewDataPegawai
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
            this.dgvPreviewPegawai = new System.Windows.Forms.DataGridView();
            this.Oke = new System.Windows.Forms.Button();
            this.IMPORT = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPreviewPegawai)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvPreviewPegawai
            // 
            this.dgvPreviewPegawai.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPreviewPegawai.Location = new System.Drawing.Point(56, 70);
            this.dgvPreviewPegawai.Name = "dgvPreviewPegawai";
            this.dgvPreviewPegawai.RowHeadersWidth = 51;
            this.dgvPreviewPegawai.RowTemplate.Height = 24;
            this.dgvPreviewPegawai.Size = new System.Drawing.Size(686, 291);
            this.dgvPreviewPegawai.TabIndex = 0;
            this.dgvPreviewPegawai.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvPreviewPegawai_CellContentClick);
            // 
            // Oke
            // 
            this.Oke.Font = new System.Drawing.Font("Times New Roman", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Oke.Location = new System.Drawing.Point(258, 386);
            this.Oke.Name = "Oke";
            this.Oke.Size = new System.Drawing.Size(121, 36);
            this.Oke.TabIndex = 1;
            this.Oke.Text = "OKE";
            this.Oke.UseVisualStyleBackColor = true;
            this.Oke.Click += new System.EventHandler(this.Oke_Click);
            // 
            // IMPORT
            // 
            this.IMPORT.Font = new System.Drawing.Font("Times New Roman", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IMPORT.Location = new System.Drawing.Point(397, 386);
            this.IMPORT.Name = "IMPORT";
            this.IMPORT.Size = new System.Drawing.Size(121, 36);
            this.IMPORT.TabIndex = 2;
            this.IMPORT.Text = "IMPORT";
            this.IMPORT.UseVisualStyleBackColor = true;
            this.IMPORT.Click += new System.EventHandler(this.IMPORT_Click);
            // 
            // PreviewDataPegawai
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Azure;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.IMPORT);
            this.Controls.Add(this.Oke);
            this.Controls.Add(this.dgvPreviewPegawai);
            this.Name = "PreviewDataPegawai";
            this.Text = "Preview_Data_Pegawai";
            ((System.ComponentModel.ISupportInitialize)(this.dgvPreviewPegawai)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvPreviewPegawai;
        private System.Windows.Forms.Button Oke;
        private System.Windows.Forms.Button IMPORT;
    }
}