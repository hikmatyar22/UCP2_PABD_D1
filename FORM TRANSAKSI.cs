using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using HOMEPAGE;
using Microsoft.Reporting.WinForms;

namespace CRUDRmas
{
    public partial class FORMTRANSAKSI : Form
    {
        public FORMTRANSAKSI()
        {
            InitializeComponent();
        }

        private void FORMTRANSAKSI_Load_1(object sender, EventArgs e)
        {
            // Setup ReportViewer data
            SetupReportViewer();
            // Refresh report to display data
            this.reportViewer1.RefreshReport();
        }

        private void SetupReportViewer()
        {
            // Connection string to your database
            string connectionString = "Data Source=LAPTOP-T1UUTAE0\\HIKMATYAR;Initial Catalog=Manajemen_Penjualan_Parfum;Integrated Security=True";

            // SQL query to retrieve the required data from the database
            string query = @"
SELECT
    t.ID_Transaksi,
    p.Nama AS Nama_Pelanggan,
    t.Tanggal_Transaksi,
    t.Total_Harga
FROM
    Transaksi t
JOIN
    Pelanggan p ON t.ID_Pelanggan = p.ID_Pelanggan";

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
            // Change this to the actual path of your RDLC file
            reportViewer1.LocalReport.ReportPath = @"C:\Users\Lenovo\Pictures\HOMEPAGE\HOMEPAGE\ReportTransaksi.rdlc";

            // Refresh the ReportViewer to show the updated report
            reportViewer1.RefreshReport();
        }

        private void reportViewer1_Load(object sender, EventArgs e)
        {

        }

        private void btnOKE_Click(object sender, EventArgs e)
        {
            HalamanMenu HalamanMenuForm = new HalamanMenu();
            HalamanMenuForm.Show();
            this.Hide();
        }
    }
}