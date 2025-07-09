namespace HOMEPAGE
{
    partial class PreviewDataRacikan
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
            this.dgvPreviewRacikan = new System.Windows.Forms.DataGridView();
            this.button1 = new System.Windows.Forms.Button();
            this.IMPORT = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPreviewRacikan)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvPreviewRacikan
            // 
            this.dgvPreviewRacikan.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPreviewRacikan.Location = new System.Drawing.Point(46, 42);
            this.dgvPreviewRacikan.Name = "dgvPreviewRacikan";
            this.dgvPreviewRacikan.RowHeadersWidth = 51;
            this.dgvPreviewRacikan.RowTemplate.Height = 24;
            this.dgvPreviewRacikan.Size = new System.Drawing.Size(710, 305);
            this.dgvPreviewRacikan.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Times New Roman", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(275, 369);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(120, 39);
            this.button1.TabIndex = 1;
            this.button1.Text = "OKE";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // IMPORT
            // 
            this.IMPORT.Location = new System.Drawing.Point(410, 369);
            this.IMPORT.Name = "IMPORT";
            this.IMPORT.Size = new System.Drawing.Size(120, 39);
            this.IMPORT.TabIndex = 2;
            this.IMPORT.Text = "IMPORT";
            this.IMPORT.UseVisualStyleBackColor = true;
            this.IMPORT.Click += new System.EventHandler(this.IMPORT_Click_1);
            // 
            // PreviewDataRacikan
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Azure;
            this.ClientSize = new System.Drawing.Size(800, 420);
            this.Controls.Add(this.IMPORT);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.dgvPreviewRacikan);
            this.Name = "PreviewDataRacikan";
            this.Text = "Preview_Data_Racikan";
            ((System.ComponentModel.ISupportInitialize)(this.dgvPreviewRacikan)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvPreviewRacikan;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button IMPORT;
    }
}