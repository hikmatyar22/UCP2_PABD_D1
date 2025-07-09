using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Text.RegularExpressions; // Untuk Regex
using System.Text; // Untuk StringBuilder
using System.Diagnostics; // Untuk debugging dengan Debug.WriteLine
using NPOI.SS.UserModel; // Pastikan ini ada jika NPOI digunakan
using HOMEPAGE; // Tambahkan ini

namespace HOMEPAGE
{
    public partial class PreviewDataPegawai : Form
    {
        private koneksi kn = new koneksi(); // Membuat instans dari kelas koneksi Anda
        private string connectionString; // Deklarasi string koneksi, akan diinisialisasi di konstruktor

        public PreviewDataPegawai(DataTable data)
        {
            InitializeComponent();
            connectionString = kn.connectionString(); // Inisialisasi connectionString di sini
            dgvPreviewPegawai.DataSource = data;
            dgvPreviewPegawai.Refresh();
            dgvPreviewPegawai.Update(); // Update langsung ke DGV preview
            this.Text = "Preview Data Pegawai"; // Atur judul form agar jelas
        }

        private void PreviewDataPegawai_Load(object sender, EventArgs e)
        {
            dgvPreviewPegawai.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        }

        private void IMPORT_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Apakah Anda ingin mengimpor data pegawai ini ke database?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                ImportDataToDatabase();
            }
        }

        /// <summary>
        /// Melakukan validasi satu baris data dari DataTable yang berasal dari Excel.
        /// </summary>
        /// <param name="row">DataRow yang akan divalidasi.</param>
        /// <param name="rowIndex">Nomor indeks baris (untuk pesan error yang lebih spesifik).</param>
        /// <param name="errorMessage">Output string untuk pesan error validasi.</param>
        /// <returns>True jika baris valid, False jika tidak.</returns>
        private bool ValidateRow(DataRow row, int rowIndex, out string errorMessage)
        {
            errorMessage = "";

            // Pastikan kolom ada sebelum mengaksesnya
            // Nama kolom harus persis sama dengan header di Excel
            if (!row.Table.Columns.Contains("ID_Pegawai") || !row.Table.Columns.Contains("Nama_Pegawai"))
            {
                errorMessage = "Kolom 'ID_Pegawai' atau 'Nama_Pegawai' tidak ditemukan di header file Excel. Pastikan nama kolom benar.";
                Debug.WriteLine($"Validation failed for row {rowIndex}: {errorMessage}");
                return false;
            }

            // Ambil nilai dengan penanganan DBNull dan trim
            string id = row["ID_Pegawai"] != DBNull.Value ? row["ID_Pegawai"].ToString().Trim() : "";
            string nama = row["Nama_Pegawai"] != DBNull.Value ? row["Nama_Pegawai"].ToString().Trim() : "";

            // --- DEBUGGING TAMBAHAN ---
            Debug.WriteLine($"Validating row {rowIndex}: ID='{id}', Nama='{nama}'");
            // --- END DEBUGGING ---

            // Validasi ID_Pegawai
            if (string.IsNullOrWhiteSpace(id))
            {
                errorMessage = $"ID Pegawai tidak boleh kosong.";
                Debug.WriteLine($"Validation failed for row {rowIndex}: ID_Pegawai is empty or whitespace.");
                return false;
            }
            // Sesuai Regex di PEGAWAI.cs: harus dimulai dengan '03' dan diikuti 1-5 digit angka (total 3-7 karakter)
            if (!Regex.IsMatch(id, @"^03\d{1,5}$"))
            {
                errorMessage = $"ID Pegawai '{id}' tidak sesuai format. Harus dimulai dengan '03' dan diikuti 1 hingga 5 digit angka (total 3-7 karakter).";
                Debug.WriteLine($"Validation failed for row {rowIndex}: ID_Pegawai '{id}' does not match regex format.");
                return false;
            }

            // Validasi Nama_Pegawai
            if (string.IsNullOrWhiteSpace(nama))
            {
                errorMessage = $"Nama Pegawai tidak boleh kosong.";
                Debug.WriteLine($"Validation failed for row {rowIndex}: Nama_Pegawai is empty or whitespace.");
                return false;
            }
            if (Regex.IsMatch(nama, @"[^A-Za-z ]"))
            {
                errorMessage = $"Nama Pegawai '{nama}' hanya boleh berisi huruf dan spasi.";
                Debug.WriteLine($"Validation failed for row {rowIndex}: Nama_Pegawai '{nama}' contains invalid characters.");
                return false;
            }

            Debug.WriteLine($"Validation successful for row {rowIndex}.");
            return true;
        }


        private void ImportDataToDatabase()
        {
            Debug.WriteLine("Memulai ImportDataToDatabase()...");
            try
            {
                DataTable dt = (DataTable)dgvPreviewPegawai.DataSource;
                if (dt == null || dt.Rows.Count == 0)
                {
                    MessageBox.Show("Tidak ada data untuk diimpor.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.Cancel; // Tidak ada yang diimpor, batalkan
                    this.Close();
                    return;
                }

                int importedCount = 0;
                int failedCount = 0;
                StringBuilder errorDetails = new StringBuilder(); // Untuk menyimpan detail error per baris

                using (SqlConnection conn = new SqlConnection(connectionString)) // Menggunakan connectionString yang sudah diinisialisasi
                {
                    conn.Open();
                    // Tidak menggunakan transaction per baris karena bisa sangat lambat untuk banyak data.
                    // Gunakan transaction besar di sekitar loop jika konsistensi penuh diperlukan
                    // dan Anda siap untuk rollback semua jika ada satu saja yang gagal (lebih kompleks).

                    for (int i = 0; i < dt.Rows.Count; i++) // Iterasi dari baris 0 (indeks DataTable)
                    {
                        DataRow row = dt.Rows[i];
                        string currentId = "N/A"; // Default jika kolom tidak ditemukan

                        // Coba ambil ID_Pegawai untuk pesan error, bahkan jika kolomnya tidak ada
                        if (row.Table.Columns.Contains("ID_Pegawai"))
                        {
                            currentId = row["ID_Pegawai"] != DBNull.Value ? row["ID_Pegawai"].ToString().Trim() : "";
                        }

                        string validationError = "";
                        if (!ValidateRow(row, i + 2, out validationError)) // i+2 karena baris Excel dimulai dari 1 dan header di baris 1
                        {
                            failedCount++;
                            errorDetails.AppendLine($"Baris Excel {i + 2} (ID '{currentId}'): Validasi gagal - {validationError}");
                            Debug.WriteLine($"Validasi gagal untuk baris Excel {i + 2} (ID: {currentId}).");
                            continue; // Lanjutkan ke baris berikutnya jika validasi gagal
                        }

                        string namaPegawai = row.Table.Columns.Contains("Nama_Pegawai") ?
                                             (row["Nama_Pegawai"] != DBNull.Value ? row["Nama_Pegawai"].ToString().Trim() : "") : "";

                        // Periksa apakah ID sudah ada di database sebelum mencoba INSERT
                        try
                        {
                            // Gunakan SqlCommand baru untuk setiap iterasi dan pastikan param bersih
                            using (SqlCommand checkCmd = new SqlCommand("SELECT COUNT(1) FROM Pegawai WHERE ID_Pegawai = @ID", conn))
                            {
                                checkCmd.Parameters.AddWithValue("@ID", currentId);
                                if ((int)checkCmd.ExecuteScalar() > 0)
                                {
                                    failedCount++;
                                    errorDetails.AppendLine($"Baris Excel {i + 2} (ID '{currentId}'): ID sudah ada di database.");
                                    Debug.WriteLine($"ID {currentId} sudah ada.");
                                    continue; // Lanjutkan ke baris berikutnya
                                }
                            }

                            // Jika ID belum ada dan validasi lolos, lakukan INSERT
                            using (SqlCommand cmd = new SqlCommand("sp_TambahPegawai", conn))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@ID", currentId);
                                cmd.Parameters.AddWithValue("@Nama", namaPegawai);
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
                                case 2627: // Primary Key violation (duplicate ID_Pegawai)
                                    sqlErrorMessage = "ID sudah ada (kemungkinan race condition atau cache lama)."; // Pesan lebih jelas
                                    break;
                                case 2601: // Unique constraint violation (jika Nama_Pegawai unik)
                                    sqlErrorMessage = "Nama Pegawai sudah ada.";
                                    break;
                                case 8152: // String or binary data would be truncated (nama terlalu panjang)
                                    sqlErrorMessage = "Nama terlalu panjang.";
                                    break;
                                case 50000: // Custom error dari TRIGGER atau RAISERROR
                                    sqlErrorMessage = ex.Message;
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
                    } // End foreach (DataRow row in dt.Rows)
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

                // Ini sangat penting agar form PEGAWAI tahu bahwa operasi impor berhasil/selesai
                // Mengatur DialogResult.OK hanya jika setidaknya ada yang berhasil diimpor, atau tidak ada kegagalan.
                this.DialogResult = (importedCount > 0 && failedCount == 0) ? DialogResult.OK : DialogResult.OK; // Always OK for success, even partial. Adjust if strict success needed.
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Terjadi kesalahan fatal saat mengimpor data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine($"Fatal error di ImportDataToDatabase: {ex.Message}");
                this.DialogResult = DialogResult.Cancel; // Beri tahu PEGAWAI bahwa ada kegagalan besar
                this.Close();
            }
        }

        private void Oke_Click(object sender, EventArgs e)
        {
            // Tombol 'Oke' setelah melihat preview, biasanya ini yang akan trigger impor atau hanya menutup.
            // Sesuai diskusi, tombol 'IMPORT' yang akan melakukan import.
            // Jadi, tombol 'Oke' ini diasumsikan untuk menutup form preview tanpa melakukan impor.
            this.DialogResult = DialogResult.Cancel; // Set DialogResult.Cancel jika hanya menutup preview
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Ini mungkin tombol "Batal" atau "Kembali"
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void dgvPreviewPegawai_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Event ini seringkali tidak digunakan, CellClick lebih umum untuk memilih baris.
            // Biarkan kosong jika tidak ada fungsionalitas spesifik yang diperlukan.
        }
    }
}