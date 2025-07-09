using CRUDRmas;
using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HOMEPAGE
{
    public partial class HalamanMenu : Form
    {
        private koneksi kn = new koneksi(); // Tambahkan ini
        SqlConnection conn; // Ubah ini
        public HalamanMenu()
        {
            InitializeComponent();
            conn = new SqlConnection(kn.connectionString()); // Tambahkan ini
        }

        private void btnPELANGGAN_Click(object sender, EventArgs e)
        {
            PELANGGAN pelangganForm = new PELANGGAN();
            pelangganForm.Show();
            this.Hide(); // Sembunyikan form ini
        }

        private void btnAROMAPARFUM_Click(object sender, EventArgs e)
        {
            AROMA_PARFUM aromaForm = new AROMA_PARFUM();
            aromaForm.Show();
            this.Hide(); // Sembunyikan form ini
        }

        private void btnPEGAWAI_Click(object sender, EventArgs e)
        {
            PEGAWAI pegawaiForm = new PEGAWAI();
            pegawaiForm.Show();
            this.Hide(); // Sembunyikan form ini
        }

        private void btnTRANSAKSI_Click(object sender, EventArgs e)
        {
            TRANSAKSI transaksiForm = new TRANSAKSI();
            transaksiForm.Show();
            this.Hide(); // Sembunyikan form ini
        }

        private void btnRACIKANPARFUM_Click(object sender, EventArgs e)
        {
            RACIKAN_PARFUM racikanForm = new RACIKAN_PARFUM();
            racikanForm.Show();
            this.Hide(); // Sembunyikan form ini
        }

        private void btnKEMBALI_Click(object sender, EventArgs e)
        {
            Form1 Form1Form = new Form1();
            Form1Form.Show();
            this.Hide();
        }

        private void report_Click(object sender, EventArgs e)
        {
            {
                FORMTRANSAKSI reportFORM = new FORMTRANSAKSI();

                // Tampilkan form Transaksi
                reportFORM.Show();

                // Opsional: Anda bisa menyembunyikan form saat ini jika Anda ingin
                this.Hide(); 
            }


        }

        private void HalamanMenu_Load(object sender, EventArgs e)
        {

        }
    }
}
