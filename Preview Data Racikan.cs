using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Text.RegularExpressions; // Ditambahkan untuk Regex
using System.Text; // Ditambahkan untuk StringBuilder
using System.Diagnostics; // Ditambahkan untuk Debug.WriteLine
using NPOI.SS.UserModel; // Ditambahkan karena ini digunakan di PreviewDataRacikan
using HOMEPAGE; // Tambahkan ini

namespace HOMEPAGE
{
    public partial class PreviewDataRacikan : Form
    {
        private koneksi kn = new koneksi(); // Membuat instans dari kelas koneksi Anda
        private string connectionString; // Deklarasi string koneksi, akan diinisialisasi di konstruktor

        public PreviewDataRacikan(DataTable data)
        {
            InitializeComponent();
            connectionString = kn.connectionString(); // Inisialisasi connectionString di sini
            dgvPreviewRacikan.DataSource = data; // Assuming your DataGridView is named dgvDataAroma
            dgvPreviewRacikan.Refresh();
            dgvPreviewRacikan.Update();
            this.Text = "Preview Data Racikan Parfum";
        }

        private void PreviewDataRacikan_Load(object sender, EventArgs e)
        {
            dgvPreviewRacikan.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        }

        private void IMPORT_Click_1(object sender, EventArgs e) // Nama event handler asli dari Anda
        {
            DialogResult result = MessageBox.Show("Apakah Anda ingin mengimpor data racikan parfum ini ke database?", "Konfirmasi Impor", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                ImportDataToDatabase();
            }
        }

        /// <summary>
        /// Validates a single row of Racikan_Parfum data.
        /// </summary>
        /// <param name="row">The DataRow to validate.</param>
        /// <param name="rowIndex">The 0-based index of the row in the DataTable (for error messages).</param>
        /// <param name="errorMessage">Output string for validation error message.</param>
        /// <returns>True if the row is valid, false otherwise.</returns>
        private bool ValidateRow(DataRow row, int rowIndex, out string errorMessage)
        {
            errorMessage = "";

            // Pastikan semua kolom yang diperlukan ada di DataTable
            string[] requiredColumns = { "ID_Racikan", "ID_Pelanggan", "ID_Aroma", "ID_Pegawai", "Perbandingan", "Nama_Racikan", "Nama_Hasil_Parfum" };
            foreach (string colName in requiredColumns)
            {
                if (!row.Table.Columns.Contains(colName))
                {
                    errorMessage = $"Kolom '{colName}' tidak ditemukan di header file Excel. Pastikan semua kolom yang diperlukan ada dan namanya benar.";
                    Debug.WriteLine($"Validation failed for row {rowIndex}: {errorMessage}");
                    return false;
                }
            }

            // Ambil nilai dengan penanganan DBNull dan trim
            string idRacikan = row["ID_Racikan"] != DBNull.Value ? row["ID_Racikan"].ToString().Trim() : "";
            string idPelanggan = row["ID_Pelanggan"] != DBNull.Value ? row["ID_Pelanggan"].ToString().Trim() : "";
            string idAroma = row["ID_Aroma"] != DBNull.Value ? row["ID_Aroma"].ToString().Trim() : "";
            string idPegawai = row["ID_Pegawai"] != DBNull.Value ? row["ID_Pegawai"].ToString().Trim() : "";
            string perbandingan = row["Perbandingan"] != DBNull.Value ? row["Perbandingan"].ToString().Trim() : "";
            string namaRacikan = row["Nama_Racikan"] != DBNull.Value ? row["Nama_Racikan"].ToString().Trim() : "";
            string namaHasilParfum = row["Nama_Hasil_Parfum"] != DBNull.Value ? row["Nama_Hasil_Parfum"].ToString().Trim() : "";

            Debug.WriteLine($"Validating row {rowIndex}: ID_Racikan='{idRacikan}', ID_Pelanggan='{idPelanggan}', ID_Aroma='{idAroma}', ID_Pegawai='{idPegawai}', Perbandingan='{perbandingan}', Nama_Racikan='{namaRacikan}', Nama_Hasil_Parfum='{namaHasilParfum}'");

            // Validasi ID_Racikan
            if (string.IsNullOrWhiteSpace(idRacikan))
            {
                errorMessage = "ID Racikan tidak boleh kosong.";
                return false;
            }
            if (!Regex.IsMatch(idRacikan, @"^04\d{1,5}$"))
            {
                errorMessage = $"ID Racikan '{idRacikan}' tidak sesuai format. Harus dimulai dengan '04' dan diikuti 1 hingga 5 digit angka (total 3-7 karakter).";
                return false;
            }

            // Validasi ID_Pelanggan
            if (string.IsNullOrWhiteSpace(idPelanggan))
            {
                errorMessage = "ID Pelanggan tidak boleh kosong.";
                return false;
            }
            if (!Regex.IsMatch(idPelanggan, @"^01\d{1,5}$")) // Validasi format ID Pelanggan
            {
                errorMessage = $"ID Pelanggan '{idPelanggan}' tidak sesuai format. Harus dimulai dengan '01' dan diikuti 1 hingga 5 digit angka.";
                return false;
            }

            // Validasi ID_Aroma
            if (string.IsNullOrWhiteSpace(idAroma))
            {
                errorMessage = "ID Aroma tidak boleh kosong.";
                return false;
            }
            if (!Regex.IsMatch(idAroma, @"^02\d{1,5}$")) // Validasi format ID Aroma
            {
                errorMessage = $"ID Aroma '{idAroma}' tidak sesuai format. Harus dimulai dengan '02' dan diikuti 1 hingga 5 digit angka.";
                return false;
            }

            // Validasi ID_Pegawai
            if (string.IsNullOrWhiteSpace(idPegawai))
            {
                errorMessage = "ID Pegawai tidak boleh kosong.";
                return false;
            }
            if (!Regex.IsMatch(idPegawai, @"^03\d{1,5}$")) // Validasi format ID Pegawai
            {
                errorMessage = $"ID Pegawai '{idPegawai}' tidak sesuai format. Harus dimulai dengan '03' dan diikuti 1 hingga 5 digit angka.";
                return false;
            }

            // Validasi Perbandingan
            if (string.IsNullOrWhiteSpace(perbandingan))
            {
                errorMessage = "Perbandingan tidak boleh kosong.";
                return false;
            }
            if (!Regex.IsMatch(perbandingan, @"^[1-9]:[1-9]$"))
            {
                errorMessage = $"Format Perbandingan '{perbandingan}' tidak valid. Contoh: 1:3 (angka 1-9 diikuti ':' dan angka 1-9).";
                return false;
            }

            // Validasi Nama_Racikan
            if (string.IsNullOrWhiteSpace(namaRacikan))
            {
                errorMessage = "Nama Racikan tidak boleh kosong.";
                return false;
            }
            if (!Regex.IsMatch(namaRacikan, @"^[A-Za-z0-9 ]+$")) // Hanya huruf, angka, spasi
            {
                errorMessage = $"Nama Racikan '{namaRacikan}' hanya boleh berisi huruf, angka, dan spasi.";
                return false;
            }

            // Validasi Nama_Hasil_Parfum
            if (string.IsNullOrWhiteSpace(namaHasilParfum))
            {
                errorMessage = "Nama Hasil Parfum tidak boleh kosong.";
                return false;
            }
            if (!Regex.IsMatch(namaHasilParfum, @"^[A-Za-z0-9 ]+$")) // Hanya huruf, angka, spasi
            {
                errorMessage = $"Nama Hasil Parfum '{namaHasilParfum}' hanya boleh berisi huruf, angka, dan spasi.";
                return false;
            }

            Debug.WriteLine($"Validation successful for row {rowIndex}.");
            return true;
        }

        /// <summary>
        /// Imports data from the DataGridView to the Racikan_Parfum table in the database.
        /// </summary>
        private void ImportDataToDatabase()
        {
            Debug.WriteLine("Memulai ImportDataToDatabase()...");
            try
            {
                DataTable dt = (DataTable)dgvPreviewRacikan.DataSource;
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

                using (SqlConnection conn = new SqlConnection(connectionString)) // Menggunakan connectionString yang sudah diinisialisasi
                {
                    conn.Open();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow row = dt.Rows[i];
                        string currentIdRacikan = "N/A_ID_MISSING";

                        if (row.Table.Columns.Contains("ID_Racikan"))
                        {
                            currentIdRacikan = row["ID_Racikan"] != DBNull.Value ? row["ID_Racikan"].ToString().Trim() : "N/A_EMPTY";
                        }

                        string validationError = "";
                        // i+2 karena baris Excel dimulai dari 1 dan header di baris 1
                        if (!ValidateRow(row, i + 2, out validationError))
                        {
                            failedCount++;
                            errorDetails.AppendLine($"Baris Excel {i + 2} (ID Racikan '{currentIdRacikan}'): Validasi gagal - {validationError}");
                            Debug.WriteLine($"Validasi gagal untuk baris Excel {i + 2} (ID Racikan: {currentIdRacikan}).");
                            continue; // Lanjutkan ke baris berikutnya jika validasi gagal
                        }

                        // Ambil semua data (pastikan kolom ada dan null-safe)
                        string idPelanggan = row.Table.Columns.Contains("ID_Pelanggan") ? (row["ID_Pelanggan"] != DBNull.Value ? row["ID_Pelanggan"].ToString().Trim() : "") : "";
                        string idAroma = row.Table.Columns.Contains("ID_Aroma") ? (row["ID_Aroma"] != DBNull.Value ? row["ID_Aroma"].ToString().Trim() : "") : "";
                        string idPegawai = row.Table.Columns.Contains("ID_Pegawai") ? (row["ID_Pegawai"] != DBNull.Value ? row["ID_Pegawai"].ToString().Trim() : "") : "";
                        string perbandingan = row.Table.Columns.Contains("Perbandingan") ? (row["Perbandingan"] != DBNull.Value ? row["Perbandingan"].ToString().Trim() : "") : "";
                        string namaRacikan = row.Table.Columns.Contains("Nama_Racikan") ? (row["Nama_Racikan"] != DBNull.Value ? row["Nama_Racikan"].ToString().Trim() : "") : "";
                        string namaHasilParfum = row.Table.Columns.Contains("Nama_Hasil_Parfum") ? (row["Nama_Hasil_Parfum"] != DBNull.Value ? row["Nama_Hasil_Parfum"].ToString().Trim() : "") : "";


                        try
                        {
                            // Periksa apakah ID_Racikan sudah ada di database sebelum mencoba INSERT
                            using (SqlCommand checkCmd = new SqlCommand("SELECT COUNT(1) FROM Racikan_Parfum WHERE ID_Racikan = @ID_Racikan", conn))
                            {
                                checkCmd.Parameters.AddWithValue("@ID_Racikan", currentIdRacikan);
                                if ((int)checkCmd.ExecuteScalar() > 0)
                                {
                                    failedCount++;
                                    errorDetails.AppendLine($"Baris Excel {i + 2} (ID Racikan '{currentIdRacikan}'): ID sudah ada di database.");
                                    Debug.WriteLine($"ID Racikan {currentIdRacikan} sudah ada.");
                                    continue; // Lanjutkan ke baris berikutnya
                                }
                            }

                            // Jika ID_Racikan belum ada dan validasi lolos, lakukan INSERT
                            using (SqlCommand cmd = new SqlCommand("sp_TambahRacikanParfum", conn))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@ID_Pelanggan", idPelanggan);
                                cmd.Parameters.AddWithValue("@ID_Aroma", idAroma);
                                cmd.Parameters.AddWithValue("@ID_Pegawai", idPegawai);
                                cmd.Parameters.AddWithValue("@ID_Racikan", currentIdRacikan);
                                cmd.Parameters.AddWithValue("@Perbandingan", perbandingan);
                                cmd.Parameters.AddWithValue("@Nama_Racikan", namaRacikan);
                                cmd.Parameters.AddWithValue("@Nama_Hasil_Parfum", namaHasilParfum);

                                cmd.ExecuteNonQuery();
                                importedCount++;
                                Debug.WriteLine($"Berhasil mengimpor ID Racikan: {currentIdRacikan} dari baris Excel {i + 2}.");
                            }
                        }
                        catch (SqlException ex)
                        {
                            failedCount++;
                            string sqlErrorMessage = "";
                            switch (ex.Number)
                            {
                                case 2627: // Primary Key violation (duplicate ID_Racikan)
                                    sqlErrorMessage = "ID sudah ada.";
                                    break;
                                case 547: // Foreign key or CHECK constraint violation
                                    if (ex.Message.Contains("FK__Racikan_P__ID_Pelanggan")) sqlErrorMessage = "ID Pelanggan tidak valid atau tidak ditemukan.";
                                    else if (ex.Message.Contains("FK__Racikan_P__ID_Aroma")) sqlErrorMessage = "ID Aroma tidak valid atau tidak ditemukan.";
                                    else if (ex.Message.Contains("FK__Racikan_P__ID_Pegawai")) sqlErrorMessage = "ID Pegawai tidak valid atau tidak ditemukan.";
                                    else if (ex.Message.Contains("CK__Racikan_P__Perbandingan")) sqlErrorMessage = "Format Perbandingan tidak valid.";
                                    else if (ex.Message.Contains("CK__Racikan_P__ID_Racikan")) sqlErrorMessage = "Format ID Racikan tidak valid.";
                                    else sqlErrorMessage = $"Pelanggaran constraint database: {ex.Message}";
                                    break;
                                case 8152: // String or binary data would be truncated
                                    sqlErrorMessage = "Data terlalu panjang untuk kolom Nama Racikan, Nama Hasil Parfum, atau ID.";
                                    break;
                                case 50000: // Custom error dari TRIGGER atau RAISERROR
                                    sqlErrorMessage = ex.Message;
                                    break;
                                default:
                                    sqlErrorMessage = $"Kesalahan database: {ex.Message}";
                                    break;
                            }
                            errorDetails.AppendLine($"Baris Excel {i + 2} (ID Racikan '{currentIdRacikan}'): Gagal diimpor - {sqlErrorMessage}");
                            Debug.WriteLine($"SQL Error impor ID Racikan {currentIdRacikan} (Baris Excel {i + 2}): {sqlErrorMessage}");
                        }
                        catch (Exception ex)
                        {
                            failedCount++;
                            errorDetails.AppendLine($"Baris Excel {i + 2} (ID Racikan '{currentIdRacikan}'): Kesalahan tidak terduga - {ex.Message}");
                            Debug.WriteLine($"Error umum saat impor ID Racikan {currentIdRacikan} (Baris Excel {i + 2}): {ex.Message}");
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

                // Ini sangat penting agar form RACIKAN_PARFUM tahu bahwa operasi impor berhasil/selesai.
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

        private void dgvPreview_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Tidak perlu ada kode di sini jika hanya untuk menampilkan data
        }

        private void button1_Click_1(object sender, EventArgs e) // Mungkin tombol "Batal"
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}