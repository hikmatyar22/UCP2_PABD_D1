using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using HOMEPAGE; // Pastikan ini ada
using Microsoft.Reporting.WinForms;
using System.IO; // Tambahkan ini untuk Path.Combine

namespace CRUDRmas
{
    public partial class FORMTRANSAKSI : Form
    {
        private koneksi kn = new koneksi(); // Tambahkan ini
        private string connectionString; // Deklarasi string koneksi

        public FORMTRANSAKSI()
        {
            InitializeComponent();
            connectionString = kn.connectionString(); // Inisialisasi connectionString di konstruktor
        }

        private void FORMTRANSAKSI_Load_1(object sender, EventArgs e)
        {
            // Setup ReportViewer data
            SetupReportViewer();
            // Refresh report to display data (ini akan dipanggil lagi di SetupReportViewer, bisa dihapus di sini jika tidak perlu refresh ganda)
            // this.reportViewer1.RefreshReport(); // Opsional: bisa dihapus jika refresh di SetupReportViewer() sudah cukup
        }

        private void SetupReportViewer()
        {
            // Connection string to your database
            // Ganti baris ini: string connectionString = "Data Source=LAPTOP-T1UUTAE0\\HIKMATYAR;Initial Catalog=Manajemen_Penjualan_Parfum;Integrated Security=True";
            string connectionString = kn.connectionString(); // Gunakan connection string dinamis

            // SQL query to retrieve the required data from the database
            // MODIFIKASI PENTING: Tambahkan ORDER BY untuk pengurutan numerik ID_Transaksi
            string query = @"
SELECT
    t.ID_Transaksi,
    p.Nama AS Nama_Pelanggan,
    t.Tanggal_Transaksi,
    t.Total_Harga
FROM
    Transaksi t
JOIN
    Pelanggan p ON t.ID_Pelanggan = p.ID_Pelanggan
ORDER BY
    CAST(SUBSTRING(t.ID_Transaksi, 3, LEN(t.ID_Transaksi) - 2) AS INT)"; // Urutkan berdasarkan bagian numerik setelah '05'

            // Create a DataTable to store the data
            DataTable dt = new DataTable();

            // Use SqlDataAdapter to fill the DataTable with data from the database
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                da.Fill(dt);
            }

            // Create a ReportDataSource
            // IMPORTANT: Make sure "DataSet1" matches the name of your dataset in your RDLC file
            ReportDataSource rds = new ReportDataSource("DataSet1", dt);

            // Clear any existing data sources and add the new data source
            reportViewer1.LocalReport.DataSources.Clear();
            reportViewer1.LocalReport.DataSources.Add(rds);

            // Set the path to the report (.rdlc file)
            // Ubah path .rdlc agar dinamis, menggunakan Path.Combine dan BaseDirectory
            reportViewer1.LocalReport.ReportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ReportTransaksi.rdlc");

            // Refresh the ReportViewer to show the updated report
            reportViewer1.RefreshReport();
        }

        private void reportViewer1_Load(object sender, EventArgs e)
        {
            // Tidak ada implementasi spesifik yang diperlukan di sini.
        }

        private void btnOKE_Click(object sender, EventArgs e)
        {
            HalamanMenu HalamanMenuForm = new HalamanMenu();
            HalamanMenuForm.Show();
            this.Hide();
        }
    }
}