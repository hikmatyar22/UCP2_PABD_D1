using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace HOMEPAGE
{
    public partial class PreviewDataRacikan : Form
    {
        private string connectionString = "Data Source=LAPTOP-T1UUTAE0\\HIKMATYAR;Initial Catalog=Manajemen_Penjualan_Parfum;Integrated Security=True";

        public PreviewDataRacikan(DataTable data)
        {
            InitializeComponent();
            dgvPreviewRacikan.DataSource = data;
        }

        private void PreviewDataRacikan_Load(object sender, EventArgs e)
        {
            dgvPreviewRacikan.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Apakah Anda ingin mengimpor data racikan parfum ini ke database?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                ImportDataToDatabase();
            }
        }

        /// <summary>
        /// Validates a single row of Racikan_Parfum data.
        /// </summary>
        /// <param name="row">The DataRow to validate.</param>
        /// <returns>True if the row is valid, false otherwise.</returns>
        private bool ValidateRow(DataRow row)
        {
            // Ensure column names match your Excel and database schema for Racikan_Parfum
            string idRacikan = row["ID_Racikan"]?.ToString() ?? "";
            string idPelanggan = row["ID_Pelanggan"]?.ToString() ?? "";
            string idAroma = row["ID_Aroma"]?.ToString() ?? "";
            string idPegawai = row["ID_Pegawai"]?.ToString() ?? "";
            string perbandingan = row["Perbandingan"]?.ToString() ?? "";
            string namaRacikan = row["Nama_Racikan"]?.ToString() ?? "";
            string namaHasilParfum = row["Nama_Hasil_Parfum"]?.ToString() ?? "";

            if (string.IsNullOrWhiteSpace(idRacikan) || idRacikan.Length != 4) // Assuming ID_Racikan should be 4 characters
            {
                MessageBox.Show("ID Racikan harus terdiri dari 4 karakter dan tidak boleh kosong.", "Kesalahan Validasi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(idPelanggan))
            {
                MessageBox.Show("ID Pelanggan tidak boleh kosong.", "Kesalahan Validasi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(idAroma))
            {
                MessageBox.Show("ID Aroma tidak boleh kosong.", "Kesalahan Validasi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(idPegawai))
            {
                MessageBox.Show("ID Pegawai tidak boleh kosong.", "Kesalahan Validasi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(perbandingan))
            {
                MessageBox.Show("Perbandingan tidak boleh kosong.", "Kesalahan Validasi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(namaRacikan))
            {
                MessageBox.Show("Nama Racikan tidak boleh kosong.", "Kesalahan Validasi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(namaHasilParfum))
            {
                MessageBox.Show("Nama Hasil Parfum tidak boleh kosong.", "Kesalahan Validasi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Imports data from the DataGridView to the Racikan_Parfum table in the database.
        /// </summary>
        private void ImportDataToDatabase()
        {
            try
            {
                DataTable dt = (DataTable)dgvPreviewRacikan.DataSource;
                int importedCount = 0;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    foreach (DataRow row in dt.Rows)
                    {
                        // Validate each row before attempting to import
                        if (!ValidateRow(row))
                        {
                            continue; // Skip this row and proceed to the next
                        }

                        // Check if the Racikan_Parfum already exists to avoid duplicates (based on ID_Racikan)
                        string checkQuery = "SELECT COUNT(*) FROM Racikan_Parfum WHERE ID_Racikan = @ID_Racikan";
                        using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                        {
                            checkCmd.Parameters.AddWithValue("@ID_Racikan", row["ID_Racikan"].ToString());
                            int existingCount = (int)checkCmd.ExecuteScalar();

                            if (existingCount > 0)
                            {
                                MessageBox.Show($"Racikan Parfum dengan ID '{row["ID_Racikan"]}' sudah ada. Data ini akan dilewati.", "Peringatan Duplikasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                continue; // Skip to the next row
                            }
                        }

                        // Use a stored procedure for adding Racikan_Parfum, similar to your RACIKAN_PARFUM form
                        using (SqlCommand cmd = new SqlCommand("sp_TambahRacikanParfum", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            // Ensure parameter names match your stored procedure 'sp_TambahRacikanParfum'
                            cmd.Parameters.AddWithValue("@ID_Pelanggan", row["ID_Pelanggan"]);
                            cmd.Parameters.AddWithValue("@ID_Aroma", row["ID_Aroma"]);
                            cmd.Parameters.AddWithValue("@ID_Pegawai", row["ID_Pegawai"]);
                            cmd.Parameters.AddWithValue("@ID_Racikan", row["ID_Racikan"]);
                            cmd.Parameters.AddWithValue("@Perbandingan", row["Perbandingan"]);
                            cmd.Parameters.AddWithValue("@Nama_Racikan", row["Nama_Racikan"]);
                            cmd.Parameters.AddWithValue("@Nama_Hasil_Parfum", row["Nama_Hasil_Parfum"]);
                            cmd.ExecuteNonQuery();
                            importedCount++;
                        }
                    }
                }

                MessageBox.Show($"Data racikan parfum berhasil diimpor ke database. Total data diimpor: {importedCount}", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close(); // Close the preview form after successful import
            }
            catch (Exception ex)
            {
                MessageBox.Show("Terjadi kesalahan saat mengimpor data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvPreview_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Optional: You can add specific logic here if needed for cell content clicks.
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close(); // Close the preview form without importing
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}