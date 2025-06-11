namespace HOMEPAGE
{
    partial class PreviewDataAroma
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
            this.dgvDataAroma = new System.Windows.Forms.DataGridView();
            this.Oke = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDataAroma)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvDataAroma
            // 
            this.dgvDataAroma.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDataAroma.Location = new System.Drawing.Point(65, 58);
            this.dgvDataAroma.Name = "dgvDataAroma";
            this.dgvDataAroma.RowHeadersWidth = 51;
            this.dgvDataAroma.RowTemplate.Height = 24;
            this.dgvDataAroma.Size = new System.Drawing.Size(676, 232);
            this.dgvDataAroma.TabIndex = 0;
            // 
            // Oke
            // 
            this.Oke.Font = new System.Drawing.Font("Times New Roman", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Oke.Location = new System.Drawing.Point(334, 318);
            this.Oke.Name = "Oke";
            this.Oke.Size = new System.Drawing.Size(135, 34);
            this.Oke.TabIndex = 1;
            this.Oke.Text = "OKE";
            this.Oke.UseVisualStyleBackColor = true;
            this.Oke.Click += new System.EventHandler(this.Oke_Click);
            // 
            // PreviewDataAroma
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Azure;
            this.ClientSize = new System.Drawing.Size(800, 382);
            this.Controls.Add(this.Oke);
            this.Controls.Add(this.dgvDataAroma);
            this.Name = "PreviewDataAroma";
            this.Text = "Preview_Data_Aroma";
            ((System.ComponentModel.ISupportInitialize)(this.dgvDataAroma)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvDataAroma;
        private System.Windows.Forms.Button Oke;
    }
}