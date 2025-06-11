using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Windows.Forms;

namespace HOMEPAGE
{
    public partial class PreviewDataAroma : Form
    {
        private string connectionString = "Data Source=LAPTOP-T1UUTAE0\\HIKMATYAR;Initial Catalog=Manajemen_Penjualan_Parfum;Integrated Security=True";

        public PreviewDataAroma(DataTable data)
        {
            InitializeComponent();
            dgvDataAroma.DataSource = data; // Assuming you have a DataGridView named dgvPreviewAroma
        }

        private void PreviewDataAroma_Load(object sender, EventArgs e)
        {
            dgvDataAroma.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Apakah Anda ingin mengimpor data aroma ini ke database?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                ImportDataToDatabase();
            }
        }

        /// <summary>
        /// Validates a single row of aroma data.
        /// </summary>
        /// <param name="row">The DataRow to validate.</param>
        /// <returns>True if the row is valid, false otherwise.</returns>
        private bool ValidateRow(DataRow row)
        {
            // Ensure column names match your Excel and database schema for Aroma_Parfum
            string idAroma = row["ID_Aroma"]?.ToString() ?? "";
            string namaAroma = row["Nama_Aroma"]?.ToString() ?? "";
            string deskripsi = row["Deskripsi"]?.ToString() ?? ""; // Deskripsi can be empty

            if (string.IsNullOrWhiteSpace(idAroma))
            {
                MessageBox.Show("ID Aroma tidak boleh kosong.", "Kesalahan Validasi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(namaAroma))
            {
                MessageBox.Show("Nama Aroma tidak boleh kosong.", "Kesalahan Validasi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // You can add more specific validation for ID_Aroma format if needed (e.g., length, specific characters)

            return true;
        }

        /// <summary>
        /// Imports data from the DataGridView to the Aroma_Parfum table in the database.
        /// </summary>
        private void ImportDataToDatabase()
        {
            int importedCount = 0;
            int skippedCount = 0;
            StringBuilder skippedRows = new StringBuilder();

            DataTable dt = (DataTable)dgvDataAroma.DataSource;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                foreach (DataRow row in dt.Rows)
                {
                    // Validate each row before attempting to import
                    if (!ValidateRow(row))
                    {
                        skippedRows.AppendLine($"ID Aroma: '{row["ID_Aroma"]?.ToString() ?? "N/A"}' - Gagal validasi.");
                        skippedCount++;
                        continue; // Skip this row and proceed to the next
                    }

                    try
                    {
                        // Check if the aroma already exists to avoid duplicates (based on ID_Aroma)
                        string checkQuery = "SELECT COUNT(*) FROM Aroma_Parfum WHERE ID_Aroma = @ID_Aroma";
                        using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                        {
                            checkCmd.Parameters.AddWithValue("@ID_Aroma", row["ID_Aroma"].ToString());
                            int existingCount = (int)checkCmd.ExecuteScalar();

                            if (existingCount > 0)
                            {
                                skippedRows.AppendLine($"ID Aroma: '{row["ID_Aroma"]}' - Sudah ada di database.");
                                skippedCount++;
                                continue; // Skip to the next row
                            }
                        }

                        // Use a stored procedure for adding aromas, similar to your AROMA_PARFUM form
                        using (SqlCommand cmd = new SqlCommand("AddAroma", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            // Ensure parameter names match your stored procedure 'AddAroma'
                            cmd.Parameters.AddWithValue("@ID_Aroma", row["ID_Aroma"]);
                            cmd.Parameters.AddWithValue("@Nama_Aroma", row["Nama_Aroma"]);
                            // Handle DBNull.Value for nullable Deskripsi column
                            cmd.Parameters.AddWithValue("@Deskripsi", row["Deskripsi"] == DBNull.Value ? (object)DBNull.Value : row["Deskripsi"].ToString());
                            cmd.ExecuteNonQuery();
                            importedCount++;
                        }
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 2627) // Primary key violation (duplicate ID)
                        {
                            skippedRows.AppendLine($"ID Aroma: '{row["ID_Aroma"]}' - Duplikasi ID.");
                        }
                        else if (ex.Number == 8152) // String or binary data would be truncated (data too long)
                        {
                            skippedRows.AppendLine($"ID Aroma: '{row["ID_Aroma"]}' - Data terlalu panjang untuk kolom.");
                        }
                        else
                        {
                            skippedRows.AppendLine($"ID Aroma: '{row["ID_Aroma"]}' - Error database: {ex.Message}");
                        }
                        skippedCount++;
                    }
                    catch (Exception ex)
                    {
                        skippedRows.AppendLine($"ID Aroma: '{row["ID_Aroma"]}' - Error umum: {ex.Message}");
                        skippedCount++;
                    }
                }
            }

            if (importedCount > 0)
            {
                MessageBox.Show($"Data aroma berhasil diimpor ke database. Total data diimpor: {importedCount}", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            if (skippedCount > 0)
            {
                MessageBox.Show($"Beberapa data aroma dilewati karena kesalahan atau duplikasi:\n\n{skippedRows.ToString()}", "Peringatan Impor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            this.DialogResult = DialogResult.OK; // Set DialogResult to OK if import was attempted
            this.Close(); // Close the preview form after import attempt
        }


        // Make sure you replace this with the actual DataGridView name in your PreviewDataAroma form
        // For example, if your DataGridView is named dgvAromaPreview, change this accordingly.
        private void dgvPreviewAroma_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Optional: You can add specific logic here if needed for cell content clicks.
            // For general row selection, dgvPreviewAroma_CellClick would be more appropriate.
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel; // Set DialogResult to Cancel if user cancels
            this.Close(); // Close the preview form without importing
        }

        private void Oke_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}