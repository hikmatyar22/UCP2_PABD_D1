namespace CRUDRmas
{
    partial class FORMTRANSAKSI
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
            this.reportViewer1 = new Microsoft.Reporting.WinForms.ReportViewer();
            this.btnOKE = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // reportViewer1
            // 
            this.reportViewer1.Location = new System.Drawing.Point(51, 63);
            this.reportViewer1.Name = "reportViewer1";
            this.reportViewer1.Size = new System.Drawing.Size(696, 280);
            this.reportViewer1.TabIndex = 0;
            this.reportViewer1.Load += new System.EventHandler(this.reportViewer1_Load);
            // 
            // btnOKE
            // 
            this.btnOKE.Font = new System.Drawing.Font("Times New Roman", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOKE.Location = new System.Drawing.Point(325, 384);
            this.btnOKE.Name = "btnOKE";
            this.btnOKE.Size = new System.Drawing.Size(127, 32);
            this.btnOKE.TabIndex = 1;
            this.btnOKE.Text = "OKE";
            this.btnOKE.UseVisualStyleBackColor = true;
            this.btnOKE.Click += new System.EventHandler(this.btnOKE_Click);
            // 
            // FORMTRANSAKSI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Azure;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnOKE);
            this.Controls.Add(this.reportViewer1);
            this.Name = "FORMTRANSAKSI";
            this.Text = "FORM_TRANSAKSI";
            this.Load += new System.EventHandler(this.FORMTRANSAKSI_Load_1);
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.Reporting.WinForms.ReportViewer reportViewer1;
        private System.Windows.Forms.Button btnOKE;
    }
}