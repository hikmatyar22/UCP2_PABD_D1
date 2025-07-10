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
    public partial class Form1 : Form
    {
        private koneksi kn = new koneksi();
        SqlConnection conn;

        public Form1()
        {
            InitializeComponent();
            conn = new SqlConnection(kn.connectionString());
        }

        private void btnMENU_Click(object sender, EventArgs e)
        {
            // Misalnya membuka form menu utama
            HalamanMenu menu = new HalamanMenu();
            menu.Show();
            this.Hide(); // Sembunyikan form ini
        }

        private void btnEXIT_Click(object sender, EventArgs e)
        {
            // Tutup aplikasi
            Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                // Buka koneksi
                conn.Open();
                // Jika koneksi berhasil, tampilkan pesan sukses
                lblStatusKoneksi.Text = "STATUS : KONEKSI BERHASIL";
                lblStatusKoneksi.ForeColor = Color.Green; // Atur warna menjadi hijau
            }
            catch (Exception ex)
            {
                // Jika koneksi gagal, tampilkan pesan kesalahan
                lblStatusKoneksi.Text = " STATUS : KONEKSI GAGAL: " + ex.Message;
                lblStatusKoneksi.ForeColor = Color.Red; // Atur warna menjadi merah
                MessageBox.Show("Gagal terhubung ke database:\n" + ex.Message, "Kesalahan Koneksi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Selalu tutup koneksi jika dibuka di blok ini
                // (meskipun untuk sekadar memeriksa koneksi, Anda mungkin tidak ingin menyimpannya tetap terbuka di sini)
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }
    }
}