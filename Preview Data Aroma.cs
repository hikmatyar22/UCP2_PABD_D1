using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions; // Ditambahkan untuk Regex
using System.Diagnostics; // Ditambahkan untuk Debug.WriteLine
using NPOI.SS.UserModel; // Pastikan ini ada jika NPOI digunakan

namespace HOMEPAGE
{
    public partial class PreviewDataAroma : Form
    {

        private koneksi kn = new koneksi(); // Tambahkan ini
        private string connectionString; // Ubah ini

        public PreviewDataAroma(DataTable data)
        {
            InitializeComponent();
            dgvDataAroma.DataSource = data; // Assuming your DataGridView is named dgvDataAroma
            dgvDataAroma.Refresh();
            dgvDataAroma.Update();
            this.Text = "Preview Data Aroma Parfum"; // Atur judul form
            connectionString = kn.connectionString(); // Tambahkan ini
        }

        private void PreviewDataAroma_Load(object sender, EventArgs e)
        {
            dgvDataAroma.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        }

        private void IMPORT_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Apakah Anda ingin mengimpor data aroma ini ke database?", "Konfirmasi Impor", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                ImportDataToDatabase();
            }
        }

        /// <summary>
        /// Validates a single row of aroma data.
        /// </summary>
        /// <param name="row">The DataRow to validate.</param>
        /// <param name="rowIndex">The 0-based index of the row in the DataTable (for error messages).</param>
        /// <param name="errorMessage">Output string for validation error message.</param>
        /// <returns>True if the row is valid, false otherwise.</returns>
        private bool ValidateRow(DataRow row, int rowIndex, out string errorMessage)
        {
            errorMessage = "";

            // Pastikan kolom ada sebelum mengaksesnya (nama kolom harus persis sesuai header Excel)
            if (!row.Table.Columns.Contains("ID_Aroma") || !row.Table.Columns.Contains("Nama_Aroma") || !row.Table.Columns.Contains("Deskripsi"))
            {
                errorMessage = "Kolom 'ID_Aroma', 'Nama_Aroma', atau 'Deskripsi' tidak ditemukan di header file Excel. Pastikan nama kolom benar.";
                Debug.WriteLine($"Validation failed for row {rowIndex}: {errorMessage}");
                return false;
            }

            // Ambil nilai dengan penanganan DBNull dan trim
            string idAroma = row["ID_Aroma"] != DBNull.Value ? row["ID_Aroma"].ToString().Trim() : "";
            string namaAroma = row["Nama_Aroma"] != DBNull.Value ? row["Nama_Aroma"].ToString().Trim() : "";
            string deskripsi = row["Deskripsi"] != DBNull.Value ? row["Deskripsi"].ToString().Trim() : "";

            Debug.WriteLine($"Validating row {rowIndex}: ID='{idAroma}', Nama='{namaAroma}', Deskripsi='{deskripsi}'");

            // Validasi ID_Aroma: tidak boleh kosong, format '02' diikuti 1-5 digit (total 3-7 karakter)
            if (string.IsNullOrWhiteSpace(idAroma))
            {
                errorMessage = "ID Aroma tidak boleh kosong.";
                return false;
            }
            if (!Regex.IsMatch(idAroma, @"^02\d{1,5}$"))
            {
                errorMessage = $"ID Aroma '{idAroma}' tidak sesuai format. Harus dimulai dengan '02' dan diikuti 1 hingga 5 digit angka (total 3-7 karakter).";
                return false;
            }

            // Validasi Nama_Aroma: tidak boleh kosong, hanya huruf dan spasi
            if (string.IsNullOrWhiteSpace(namaAroma))
            {
                errorMessage = "Nama Aroma tidak boleh kosong.";
                return false;
            }
            if (Regex.IsMatch(namaAroma, @"[^A-Za-z ]"))
            {
                errorMessage = $"Nama Aroma '{namaAroma}' hanya boleh berisi huruf dan spasi.";
                return false;
            }

            // MODIFIKASI: Validasi Deskripsi - HANYA memeriksa apakah tidak kosong.
            // Tidak ada lagi pembatasan karakter menggunakan Regex.
            if (string.IsNullOrWhiteSpace(deskripsi))
            {
                errorMessage = "Deskripsi tidak boleh kosong.";
                return false;
            }
            // Baris regex sebelumnya di sini telah dihapus:
            // if (Regex.IsMatch(deskripsi, @"^[A-Za-z ]*$")) { ... }

            Debug.WriteLine($"Validation successful for row {rowIndex}.");
            return true;
        }

        /// <summary>
        /// Imports data from the DataGridView to the Aroma_Parfum table in the database.
        /// </summary>
        private void ImportDataToDatabase()
        {
            Debug.WriteLine("Memulai ImportDataToDatabase()...");
            try
            {
                DataTable dt = (DataTable)dgvDataAroma.DataSource; // Assuming DataGridView name is dgvDataAroma
                if (dt == null || dt.Rows.Count == 0)
                {
                    MessageBox.Show("Tidak ada data untuk diimpor.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                    return;
                }

                int importedCount = 0;
                int failedCount = 0;
                StringBuilder errorDetails = new StringBuilder(); // Untuk menyimpan detail error per baris

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // Tidak menggunakan transaction per baris. Untuk batch transaction, perlu perubahan arsitektur.

                    for (int i = 0; i < dt.Rows.Count; i++) // Iterasi dari baris 0 (indeks DataTable)
                    {
                        DataRow row = dt.Rows[i];
                        string currentId = "N/A"; // Default jika kolom tidak ditemukan

                        // Coba ambil ID_Aroma untuk pesan error
                        if (row.Table.Columns.Contains("ID_Aroma"))
                        {
                            currentId = row["ID_Aroma"] != DBNull.Value ? row["ID_Aroma"].ToString().Trim() : "N/A_EMPTY";
                        }

                        string validationError = "";
                        // i+2 karena baris Excel dimulai dari 1 dan header di baris 1
                        if (!ValidateRow(row, i + 2, out validationError))
                        {
                            failedCount++;
                            errorDetails.AppendLine($"Baris Excel {i + 2} (ID '{currentId}'): Validasi gagal - {validationError}");
                            Debug.WriteLine($"Validasi gagal untuk baris Excel {i + 2} (ID: {currentId}).");
                            continue; // Lanjutkan ke baris berikutnya jika validasi gagal
                        }

                        string namaAroma = row.Table.Columns.Contains("Nama_Aroma") ? (row["Nama_Aroma"] != DBNull.Value ? row["Nama_Aroma"].ToString().Trim() : "") : "";
                        string deskripsi = row.Table.Columns.Contains("Deskripsi") ? (row["Deskripsi"] != DBNull.Value ? row["Deskripsi"].ToString().Trim() : "") : "";

                        try
                        {
                            // Periksa apakah ID_Aroma sudah ada di database sebelum mencoba INSERT
                            using (SqlCommand checkCmd = new SqlCommand("SELECT COUNT(1) FROM Aroma_Parfum WHERE ID_Aroma = @ID_Aroma", conn))
                            {
                                checkCmd.Parameters.AddWithValue("@ID_Aroma", currentId);
                                if ((int)checkCmd.ExecuteScalar() > 0)
                                {
                                    failedCount++;
                                    errorDetails.AppendLine($"Baris Excel {i + 2} (ID '{currentId}'): ID sudah ada di database.");
                                    Debug.WriteLine($"ID {currentId} sudah ada.");
                                    continue; // Lanjutkan ke baris berikutnya
                                }
                            }

                            // Jika ID belum ada dan validasi lolos, lakukan INSERT
                            using (SqlCommand cmd = new SqlCommand("AddAroma", conn))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@ID_Aroma", currentId);
                                cmd.Parameters.AddWithValue("@Nama_Aroma", namaAroma);
                                cmd.Parameters.AddWithValue("@Deskripsi", deskripsi); // Deskripsi tidak lagi nullable
                                cmd.ExecuteNonQuery();
                                importedCount++;
                                Debug.WriteLine($"Berhasil mengimpor ID: {currentId} dari baris Excel {i + 2}.");
                            }
                        }
                        catch (SqlException ex)
                        {
                            failedCount++;
                            string sqlErrorMessage = "";
                            switch (ex.Number)
                            {
                                case 2627: // Primary Key violation (duplicate ID_Aroma)
                                    sqlErrorMessage = "ID sudah ada.";
                                    break;
                                case 2601: // Unique constraint violation (jika Nama_Aroma unik)
                                    sqlErrorMessage = "Nama Aroma sudah ada.";
                                    break;
                                case 8152: // String or binary data would be truncated (Nama atau Deskripsi terlalu panjang)
                                    sqlErrorMessage = "Nama atau Deskripsi terlalu panjang.";
                                    break;
                                case 50000: // Custom error dari TRIGGER atau RAISERROR
                                    sqlErrorMessage = ex.Message;
                                    break;
                                case 547: // CHECK constraint violation
                                    sqlErrorMessage = "Data tidak sesuai format database (misal: ID atau Deskripsi).";
                                    break;
                                default:
                                    sqlErrorMessage = $"Kesalahan database: {ex.Message}";
                                    break;
                            }
                            errorDetails.AppendLine($"Baris Excel {i + 2} (ID '{currentId}'): Gagal diimpor - {sqlErrorMessage}");
                            Debug.WriteLine($"SQL Error impor ID {currentId} (Baris Excel {i + 2}): {sqlErrorMessage}");
                        }
                        catch (Exception ex)
                        {
                            failedCount++;
                            errorDetails.AppendLine($"Baris Excel {i + 2} (ID '{currentId}'): Kesalahan tidak terduga - {ex.Message}");
                            Debug.WriteLine($"Error umum saat impor ID {currentId} (Baris Excel {i + 2}): {ex.Message}");
                        }
                    } // End for loop
                } // End using (SqlConnection conn)

                string summaryMessage = $"Proses impor selesai.\nBerhasil diimpor: {importedCount} baris.\nGagal diimpor: {failedCount} baris.";
                if (failedCount > 0)
                {
                    summaryMessage += "\n\nRincian Kegagalan:\n" + errorDetails.ToString();
                    MessageBox.Show(summaryMessage, "Impor Data - Hasil (Ada Kegagalan)", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show(summaryMessage, "Impor Data - Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                // Ini sangat penting agar form AROMA_PARFUM tahu bahwa operasi impor berhasil/selesai.
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Terjadi kesalahan fatal saat mengimpor data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine($"Fatal error di ImportDataToDatabase: {ex.Message}");
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e) // Mungkin tombol "Batal"
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void Oke_Click(object sender, EventArgs e) // Mungkin tombol "Oke" yang hanya untuk menutup preview
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // Hapus event handler duplikat jika ada di .Designer.cs
        // private void dgvPreviewAroma_CellContentClick(object sender, DataGridViewCellEventArgs e) { } 
    }
}