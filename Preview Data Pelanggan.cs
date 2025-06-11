using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace HOMEPAGE
{
    public partial class PreviewDataPelanggan : Form
    {
        private string connectionString = "Data Source=LAPTOP-T1UUTAE0\\HIKMATYAR;Initial Catalog=Manajemen_Penjualan_Parfum;Integrated Security=True";

        public PreviewDataPelanggan(DataTable data)
        {
            InitializeComponent();
            dgvPreviewPelanggan.DataSource = data;
        }

        private void PreviewDataPelanggan_Load(object sender, EventArgs e)
        {
            dgvPreviewPelanggan.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Apakah Anda ingin mengimpor data pelanggan ini ke database?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                ImportDataToDatabase();
            }
        }

        /// <summary>
        /// Validates a single row of customer data.
        /// </summary>
        /// <param name="row">The DataRow to validate.</param>
        /// <returns>True if the row is valid, false otherwise.</returns>
        private bool ValidateRow(DataRow row)
        {
            // Ensure column names match your Excel and database schema for Pelanggan
            string id = row["ID_Pelanggan"]?.ToString() ?? "";
            string nama = row["Nama"]?.ToString() ?? "";
            string email = row["Email"]?.ToString() ?? "";

            if (string.IsNullOrWhiteSpace(id) || id.Length != 4) // Assuming ID_Pelanggan should also be 4 characters
            {
                MessageBox.Show("ID Pelanggan harus terdiri dari 4 karakter dan tidak boleh kosong.", "Kesalahan Validasi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(nama))
            {
                MessageBox.Show("Nama Pelanggan tidak boleh kosong.", "Kesalahan Validasi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Email Pelanggan tidak boleh kosong.", "Kesalahan Validasi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // You can add more validation logic here, e.g., email format validation

            return true;
        }

        /// <summary>
        /// Imports data from the DataGridView to the Pelanggan table in the database.
        /// </summary>
        private void ImportDataToDatabase()
        {
            try
            {
                DataTable dt = (DataTable)dgvPreviewPelanggan.DataSource;
                int importedCount = 0;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    foreach (DataRow row in dt.Rows)
                    {
                        // Validate each row before attempting to import
                        if (!ValidateRow(row))
                        {
                            // Optionally log which row failed validation
                            continue; // Skip this row and proceed to the next
                        }

                        // Check if the customer already exists to avoid duplicates (based on ID_Pelanggan)
                        string checkQuery = "SELECT COUNT(*) FROM Pelanggan WHERE ID_Pelanggan = @ID_Pelanggan";
                        using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                        {
                            checkCmd.Parameters.AddWithValue("@ID_Pelanggan", row["ID_Pelanggan"].ToString());
                            int existingCount = (int)checkCmd.ExecuteScalar();

                            if (existingCount > 0)
                            {
                                MessageBox.Show($"Pelanggan dengan ID '{row["ID_Pelanggan"]}' sudah ada. Data ini akan dilewati.", "Peringatan Duplikasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                continue; // Skip to the next row
                            }
                        }

                        // Use a stored procedure for adding customers, similar to your PELANGGAN form
                        using (SqlCommand cmd = new SqlCommand("sp_TambahPelanggan", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            // Ensure parameter names match your stored procedure 'sp_TambahPelanggan'
                            cmd.Parameters.AddWithValue("@ID_Pelanggan", row["ID_Pelanggan"]);
                            cmd.Parameters.AddWithValue("@Nama", row["Nama"]);
                            cmd.Parameters.AddWithValue("@Email", row["Email"]);
                            cmd.ExecuteNonQuery();
                            importedCount++;
                        }
                    }
                }

                MessageBox.Show($"Data pelanggan berhasil diimpor ke database. Total data diimpor: {importedCount}", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close(); // Close the preview form after successful import
            }
            catch (Exception ex)
            {
                MessageBox.Show("Terjadi kesalahan saat mengimpor data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // This event handler was named dgvPreview_CellContentClick and was renamed to dgvPreviewPelanggan_CellContentClick
        private void dgvPreviewPelanggan_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Optional: You can add specific logic here if needed for cell content clicks.
            // For general row selection, dgvPreviewPelanggan_CellClick would be more appropriate.
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close(); // Close the preview form without importing
        }

        private void OKE_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}