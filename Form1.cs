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
        SqlConnection conn = new SqlConnection("Data Source=LAPTOP-T1UUTAE0\\HIKMATYAR;Initial Catalog=Manajemen_Penjualan_Parfum;Integrated Security=True");
        public Form1()
        {
            InitializeComponent();
        }

        private void btnMENU_Click(object sender, EventArgs e)
        {
            // Misalnya membuka form menu utama
            HalamanMenu menu = new HalamanMenu();
            menu.Show();
            this.Hide(); // Sembunyikan form ini

        }

        private void btnEXIT_Click(object sender, EventArgs e)
        {
            // Tutup aplikasi
            Application.Exit();

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        
    }
}
