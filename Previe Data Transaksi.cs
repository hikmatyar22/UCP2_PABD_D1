using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace HOMEPAGE
{
    public partial class PreviewDataTransaksi : Form
    {
        private string connectionString = "Data Source=LAPTOP-T1UUTAE0\\HIKMATYAR;Initial Catalog=Manajemen_Penjualan_Parfum;Integrated Security=True";

        public PreviewDataTransaksi(DataTable data)
        {
            InitializeComponent();
            dgvDataTransaksi.DataSource = data; // Assuming your DataGridView is named dgvPreviewTransaksi
        }

        private void PreviewDataTransaksi_Load(object sender, EventArgs e)
        {
            dgvDataTransaksi.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        }

        /// <summary>
        /// Handles the click event for the Import button.
        /// Confirms with the user before importing the displayed transaction data to the database.
        /// </summary>
        private void btnImport_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Apakah Anda ingin mengimpor data transaksi ini ke database?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                ImportDataToDatabase();
            }
        }

        /// <summary>
        /// Validates a single row of transaction data from the Excel preview.
        /// </summary>
        /// <param name="row">The DataRow to validate.</param>
        /// <returns>True if the row is valid, false otherwise.</returns>
        private bool ValidateRow(DataRow row)
        {
            // Ensure column names match your Excel and database schema for Transaksi
            string idTransaksi = row["ID_Transaksi"]?.ToString() ?? "";
            string idPelanggan = row["ID_Pelanggan"]?.ToString() ?? "";
            string tanggalTransaksiStr = row["Tanggal_Transaksi"]?.ToString() ?? "";
            string totalHargaStr = row["Total_Harga"]?.ToString() ?? "";

            if (string.IsNullOrWhiteSpace(idTransaksi))
            {
                MessageBox.Show("ID Transaksi tidak boleh kosong.", "Kesalahan Validasi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(idPelanggan))
            {
                MessageBox.Show("ID Pelanggan tidak boleh kosong.", "Kesalahan Validasi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!DateTime.TryParse(tanggalTransaksiStr, out DateTime tanggalTransaksi))
            {
                MessageBox.Show($"Tanggal Transaksi '{tanggalTransaksiStr}' tidak valid. Gunakan format 'yyyy-MM-dd HH:mm:ss'.", "Kesalahan Validasi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!decimal.TryParse(totalHargaStr, out decimal totalHarga))
            {
                MessageBox.Show($"Total Harga '{totalHargaStr}' tidak valid. Harus berupa angka.", "Kesalahan Validasi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Imports data from the DataGridView to the Transaksi table in the database.
        /// </summary>
        private void ImportDataToDatabase()
        {
            try
            {
                DataTable dt = (DataTable)dgvDataTransaksi.DataSource;
                int importedCount = 0;
                int skippedCount = 0;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    foreach (DataRow row in dt.Rows)
                    {
                        // Validate each row before attempting to import
                        if (!ValidateRow(row))
                        {
                            skippedCount++;
                            continue; // Skip this row and proceed to the next
                        }

                        // Extract data with proper casting/parsing
                        string idTransaksi = row["ID_Transaksi"].ToString();
                        string idPelanggan = row["ID_Pelanggan"].ToString();
                        DateTime tanggalTransaksi = DateTime.Parse(row["Tanggal_Transaksi"].ToString()); // Already validated
                        decimal totalHarga = decimal.Parse(row["Total_Harga"].ToString()); // Already validated

                        // Check if the transaction already exists to avoid duplicates (based on ID_Transaksi)
                        string checkQuery = "SELECT COUNT(*) FROM Transaksi WHERE ID_Transaksi = @ID_Transaksi";
                        using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                        {
                            checkCmd.Parameters.AddWithValue("@ID_Transaksi", idTransaksi);
                            int existingCount = (int)checkCmd.ExecuteScalar();

                            if (existingCount > 0)
                            {
                                MessageBox.Show($"Transaksi dengan ID '{idTransaksi}' sudah ada. Data ini akan dilewati.", "Peringatan Duplikasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                skippedCount++;
                                continue; // Skip to the next row
                            }
                        }

                        // Use a stored procedure for adding transactions, similar to your TRANSAKSI form
                        // Make sure 'InsertTransaksi' SP in your database can accept Tanggal_Transaksi as a parameter
                        // or modify it if you intend to use GETDATE() by default.
                        // For this import, we assume it accepts Tanggal_Transaksi as a parameter.
                        using (SqlCommand cmd = new SqlCommand("InsertTransaksi", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@ID_Transaksi", idTransaksi);
                            cmd.Parameters.AddWithValue("@ID_Pelanggan", idPelanggan);
                            cmd.Parameters.AddWithValue("@Tanggal_Transaksi", tanggalTransaksi); // Add this parameter
                            cmd.Parameters.AddWithValue("@Total_Harga", totalHarga);
                            cmd.ExecuteNonQuery();
                            importedCount++;
                        }
                    }
                }

                MessageBox.Show($"Data transaksi berhasil diimpor ke database. Total data diimpor: {importedCount}. Data dilewati: {skippedCount}.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK; // Set DialogResult to OK so parent form can refresh
                this.Close(); // Close the preview form after successful import
            }
            catch (Exception ex)
            {
                MessageBox.Show("Terjadi kesalahan saat mengimpor data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles the click event for the Cancel button.
        /// Closes the preview form without importing any data.
        /// </summary>
        private void btnCancel_Click(object sender, EventArgs e) // Assuming your cancel button is named btnCancel
        {
            this.Close(); // Close the preview form without importing
        }

        private void oke_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // You might need to add a DataGridView to your form named 'dgvPreviewTransaksi'
        // and a button named 'btnCancel' in the designer for this code to work.
    }
}