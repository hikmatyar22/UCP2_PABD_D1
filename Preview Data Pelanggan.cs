using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Text.RegularExpressions; // Untuk Regex
using System.Text; // Untuk StringBuilder
using System.Diagnostics; // Untuk debugging dengan Debug.WriteLine


namespace HOMEPAGE
{
    public partial class PreviewDataPelanggan : Form
    {
        private koneksi kn = new koneksi(); // Tambahkan ini
        private string connectionString; // Ubah ini

        public PreviewDataPelanggan(DataTable data)
        {
            InitializeComponent();
            connectionString = kn.connectionString(); // Tambahkan ini
            dgvPreviewPelanggan.DataSource = data;
            dgvPreviewPelanggan.Refresh();
            dgvPreviewPelanggan.Update();
            this.Text = "Preview Data Pelanggan";
        }

        private void PreviewDataPelanggan_Load(object sender, EventArgs e)
        {
            dgvPreviewPelanggan.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        }

        private void IMPORT_Click(object sender, EventArgs e) // Menggantikan IMPORT_Click yang lama
        {
            DialogResult result = MessageBox.Show("Apakah Anda ingin mengimpor data pelanggan ini ke database?", "Konfirmasi Impor", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                ImportDataToDatabase();
            }
        }

        /// <summary>
        /// Validates a single row of customer data.
        /// </summary>
        /// <param name="row">The DataRow to validate.</param>
        /// <param name="rowIndex">The 0-based index of the row in the DataTable (for error messages).</param>
        /// <param name="errorMessage">Output string for validation error message.</param>
        /// <returns>True if the row is valid, false otherwise.</returns>
        private bool ValidateRow(DataRow row, int rowIndex, out string errorMessage)
        {
            errorMessage = "";

            // Pastikan kolom ada sebelum mengaksesnya (nama kolom harus persis sesuai header Excel)
            if (!row.Table.Columns.Contains("ID_Pelanggan") || !row.Table.Columns.Contains("Nama") || !row.Table.Columns.Contains("Email"))
            {
                errorMessage = "Kolom 'ID_Pelanggan', 'Nama', atau 'Email' tidak ditemukan di header file Excel. Pastikan nama kolom benar.";
                Debug.WriteLine($"Validation failed for row {rowIndex}: {errorMessage}");
                return false;
            }

            // Ambil nilai dengan penanganan DBNull dan trim
            string id = row["ID_Pelanggan"] != DBNull.Value ? row["ID_Pelanggan"].ToString().Trim() : "";
            string nama = row["Nama"] != DBNull.Value ? row["Nama"].ToString().Trim() : "";
            string email = row["Email"] != DBNull.Value ? row["Email"].ToString().Trim() : "";

            Debug.WriteLine($"Validating row {rowIndex}: ID='{id}', Nama='{nama}', Email='{email}'");

            // Validasi ID_Pelanggan: tidak boleh kosong, format '01' diikuti 1-5 digit (total 3-7 karakter)
            if (string.IsNullOrWhiteSpace(id))
            {
                errorMessage = "ID Pelanggan tidak boleh kosong.";
                return false;
            }
            if (!Regex.IsMatch(id, @"^01\d{1,5}$"))
            {
                errorMessage = $"ID Pelanggan '{id}' tidak sesuai format. Harus dimulai dengan '01' dan diikuti 1 hingga 5 digit angka (total 3-7 karakter).";
                return false;
            }

            // Validasi Nama Pelanggan: tidak boleh kosong, hanya huruf dan spasi
            if (string.IsNullOrWhiteSpace(nama))
            {
                errorMessage = "Nama Pelanggan tidak boleh kosong.";
                return false;
            }
            if (Regex.IsMatch(nama, @"[^A-Za-z ]"))
            {
                errorMessage = $"Nama Pelanggan '{nama}' hanya boleh berisi huruf dan spasi.";
                return false;
            }

            // Validasi Email: tidak boleh kosong, format email dasar
            if (string.IsNullOrWhiteSpace(email))
            {
                errorMessage = "Email tidak boleh kosong.";
                return false;
            }
            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                errorMessage = $"Format Email '{email}' tidak valid. Contoh: nama@example.com";
                return false;
            }

            Debug.WriteLine($"Validation successful for row {rowIndex}.");
            return true;
        }

        /// <summary>
        /// Imports data from the DataGridView to the Pelanggan table in the database.
        /// </summary>
        private void ImportDataToDatabase()
        {
            Debug.WriteLine("Memulai ImportDataToDatabase()...");
            try
            {
                DataTable dt = (DataTable)dgvPreviewPelanggan.DataSource;
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
                    // Pertimbangkan untuk menggunakan SqlTransaction tunggal di sekitar loop
                    // agar semua impor bersifat atomik (semua berhasil atau semua batal).
                    // Namun, ini membuat error handling per baris lebih kompleks karena
                    // Anda harus menangani duplikasi ID Pelanggan di sini daripada membiarkannya ke DB.
                    // Untuk saat ini, kita akan lakukan per baris tanpa transaction,
                    // dan mengandalkan validasi awal + penanganan error SQL.

                    for (int i = 0; i < dt.Rows.Count; i++) // Iterasi dari baris 0 (indeks DataTable)
                    {
                        DataRow row = dt.Rows[i];
                        string currentId = "N/A"; // Default jika kolom tidak ditemukan

                        // Coba ambil ID_Pelanggan untuk pesan error, bahkan jika kolomnya tidak ada
                        if (row.Table.Columns.Contains("ID_Pelanggan"))
                        {
                            currentId = row["ID_Pelanggan"] != DBNull.Value ? row["ID_Pelanggan"].ToString().Trim() : "N/A_EMPTY";
                        }

                        string validationError = "";
                        if (!ValidateRow(row, i + 2, out validationError)) // i+2 karena baris Excel dimulai dari 1 dan header di baris 1
                        {
                            failedCount++;
                            errorDetails.AppendLine($"Baris Excel {i + 2} (ID '{currentId}'): Validasi gagal - {validationError}");
                            Debug.WriteLine($"Validasi gagal untuk baris Excel {i + 2} (ID: {currentId}).");
                            continue; // Lanjutkan ke baris berikutnya jika validasi gagal
                        }

                        string nama = row.Table.Columns.Contains("Nama") ? (row["Nama"] != DBNull.Value ? row["Nama"].ToString().Trim() : "") : "";
                        string email = row.Table.Columns.Contains("Email") ? (row["Email"] != DBNull.Value ? row["Email"].ToString().Trim() : "") : "";

                        try
                        {
                            // Periksa apakah ID_Pelanggan sudah ada di database sebelum mencoba INSERT
                            using (SqlCommand checkCmd = new SqlCommand("SELECT COUNT(1) FROM Pelanggan WHERE ID_Pelanggan = @ID_Pelanggan", conn))
                            {
                                checkCmd.Parameters.AddWithValue("@ID_Pelanggan", currentId); // Menggunakan ID yang sudah di-trim
                                if ((int)checkCmd.ExecuteScalar() > 0)
                                {
                                    failedCount++;
                                    errorDetails.AppendLine($"Baris Excel {i + 2} (ID '{currentId}'): ID sudah ada di database.");
                                    Debug.WriteLine($"ID {currentId} sudah ada.");
                                    continue; // Lanjutkan ke baris berikutnya
                                }
                            }

                            // Periksa apakah Email sudah ada di database untuk ID yang berbeda
                            using (SqlCommand checkEmailCmd = new SqlCommand("SELECT COUNT(1) FROM Pelanggan WHERE Email = @Email AND ID_Pelanggan <> @ID_Pelanggan", conn))
                            {
                                checkEmailCmd.Parameters.AddWithValue("@Email", email);
                                checkEmailCmd.Parameters.AddWithValue("@ID_Pelanggan", currentId); // Menggunakan ID yang sudah di-trim
                                if ((int)checkEmailCmd.ExecuteScalar() > 0)
                                {
                                    failedCount++;
                                    errorDetails.AppendLine($"Baris Excel {i + 2} (ID '{currentId}'): Email '{email}' sudah digunakan oleh pelanggan lain.");
                                    Debug.WriteLine($"Email {email} sudah ada untuk ID lain.");
                                    continue; // Lanjutkan ke baris berikutnya
                                }
                            }


                            // Jika ID belum ada dan validasi lolos, lakukan INSERT
                            using (SqlCommand cmd = new SqlCommand("sp_TambahPelanggan", conn))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@ID_Pelanggan", currentId);
                                cmd.Parameters.AddWithValue("@Nama", nama);
                                cmd.Parameters.AddWithValue("@Email", email);
                                cmd.ExecuteNonQuery(); // Eksekusi, abaikan return value karena workaround akan diterapkan
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
                                case 2627: // Primary Key violation (duplicate ID_Pelanggan)
                                    sqlErrorMessage = "ID sudah ada.";
                                    break;
                                case 2601: // Unique constraint violation (duplicate Email)
                                    sqlErrorMessage = "Email sudah digunakan oleh pelanggan lain.";
                                    break;
                                case 8152: // String or binary data would be truncated (Nama atau Email terlalu panjang)
                                    sqlErrorMessage = "Nama atau Email terlalu panjang.";
                                    break;
                                case 50000: // Custom error dari TRIGGER atau RAISERROR
                                    sqlErrorMessage = ex.Message;
                                    break;
                                case 547: // CHECK constraint violation
                                    sqlErrorMessage = "Data tidak sesuai format database (misal: ID atau Email).";
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

                // Ini sangat penting agar form PELANGGAN tahu bahwa operasi impor berhasil/selesai.
                // Mengatur DialogResult.OK jika setidaknya ada yang berhasil diimpor atau tidak ada kegagalan.
                this.DialogResult = DialogResult.OK; // Sinyalkan ke PELANGGAN untuk merefresh
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


        private void OKE_Click(object sender, EventArgs e) // Mungkin tombol "Oke" yang hanya untuk menutup preview
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void dgvPreviewPelanggan_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }


        // Hapus event handler duplikat jika ada di .Designer.cs
        // private void IMPORT_Click(object sender, EventArgs e) { } 
        // private void dgvPreview_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        // private void dgvPreviewPelanggan_CellContentClick(object sender, DataGridViewCellEventArgs e) { } // Keep this one if used in designer, or remove if CellClick is primary
    }
}