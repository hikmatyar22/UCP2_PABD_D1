namespace HOMEPAGE
{
    partial class PreviewDataTransaksi
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
            this.dgvDataTransaksi = new System.Windows.Forms.DataGridView();
            this.oke = new System.Windows.Forms.Button();
            this.IMPORT = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDataTransaksi)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvDataTransaksi
            // 
            this.dgvDataTransaksi.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDataTransaksi.Location = new System.Drawing.Point(47, 57);
            this.dgvDataTransaksi.Name = "dgvDataTransaksi";
            this.dgvDataTransaksi.RowHeadersWidth = 51;
            this.dgvDataTransaksi.RowTemplate.Height = 24;
            this.dgvDataTransaksi.Size = new System.Drawing.Size(707, 263);
            this.dgvDataTransaksi.TabIndex = 0;
            // 
            // oke
            // 
            this.oke.Font = new System.Drawing.Font("Times New Roman", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.oke.Location = new System.Drawing.Point(264, 351);
            this.oke.Name = "oke";
            this.oke.Size = new System.Drawing.Size(120, 36);
            this.oke.TabIndex = 1;
            this.oke.Text = "OKE";
            this.oke.UseVisualStyleBackColor = true;
            this.oke.Click += new System.EventHandler(this.oke_Click);
            // 
            // IMPORT
            // 
            this.IMPORT.Location = new System.Drawing.Point(408, 351);
            this.IMPORT.Name = "IMPORT";
            this.IMPORT.Size = new System.Drawing.Size(120, 36);
            this.IMPORT.TabIndex = 2;
            this.IMPORT.Text = "IMPORT";
            this.IMPORT.UseVisualStyleBackColor = true;
            this.IMPORT.Click += new System.EventHandler(this.IMPORT_Click);
            // 
            // PreviewDataTransaksi
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Azure;
            this.ClientSize = new System.Drawing.Size(800, 399);
            this.Controls.Add(this.IMPORT);
            this.Controls.Add(this.oke);
            this.Controls.Add(this.dgvDataTransaksi);
            this.Name = "PreviewDataTransaksi";
            this.Text = "Previe_Data_Transaksi";
            ((System.ComponentModel.ISupportInitialize)(this.dgvDataTransaksi)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvDataTransaksi;
        private System.Windows.Forms.Button oke;
        private System.Windows.Forms.Button IMPORT;
    }
}