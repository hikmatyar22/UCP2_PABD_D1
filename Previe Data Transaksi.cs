using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Text.RegularExpressions; // Ditambahkan untuk Regex
using System.Text; // Ditambahkan untuk StringBuilder
using System.Diagnostics; // Ditambahkan untuk Debug.WriteLine
using NPOI.SS.UserModel; // Ditambahkan karena ini digunakan untuk membaca Excel
using HOMEPAGE; // Tambahkan ini

namespace HOMEPAGE
{
    public partial class PreviewDataTransaksi : Form
    {
        private koneksi kn = new koneksi(); // Membuat instans dari kelas koneksi Anda
        private string connectionString; // Deklarasi string koneksi, akan diinisialisasi di konstruktor

        public PreviewDataTransaksi(DataTable data)
        {
            InitializeComponent();
            connectionString = kn.connectionString(); // Inisialisasi connectionString di sini
            dgvDataTransaksi.DataSource = data; // Assuming your DataGridView is named dgvDataTransaksi
            dgvDataTransaksi.Refresh();
            dgvDataTransaksi.Update();
            this.Text = "Preview Data Transaksi"; // Atur judul form
        }

        private void PreviewDataTransaksi_Load(object sender, EventArgs e)
        {
            dgvDataTransaksi.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        }

        /// <summary>
        /// Handles the click event for the Import button.
        /// Confirms with the user before importing the displayed transaction data to the database.
        /// </summary>
        private void IMPORT_Click(object sender, EventArgs e) // Nama event handler asli dari Anda
        {
            DialogResult result = MessageBox.Show("Apakah Anda ingin mengimpor data transaksi ini ke database?", "Konfirmasi Impor", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                ImportDataToDatabase();
            }
        }

        /// <summary>
        /// Validates a single row of transaction data from the Excel preview.
        /// </summary>
        /// <param name="row">The DataRow to validate.</param>
        /// <param name="rowIndex">The 0-based index of the row in the DataTable (for error messages).</param>
        /// <param name="errorMessage">Output string for validation error message.</param>
        /// <returns>True if the row is valid, false otherwise.</returns>
        private bool ValidateRow(DataRow row, int rowIndex, out string errorMessage)
        {
            errorMessage = "";

            // Pastikan semua kolom yang diperlukan ada di DataTable
            string[] requiredColumns = { "ID_Transaksi", "ID_Pelanggan", "Tanggal_Transaksi", "Total_Harga" };
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
            string idTransaksi = row["ID_Transaksi"] != DBNull.Value ? row["ID_Transaksi"].ToString().Trim() : "";
            string idPelanggan = row["ID_Pelanggan"] != DBNull.Value ? row["ID_Pelanggan"].ToString().Trim() : "";
            string tanggalTransaksiStr = row["Tanggal_Transaksi"] != DBNull.Value ? row["Tanggal_Transaksi"].ToString().Trim() : "";
            string totalHargaStr = row["Total_Harga"] != DBNull.Value ? row["Total_Harga"].ToString().Trim() : "";

            Debug.WriteLine($"Validating row {rowIndex}: ID_Transaksi='{idTransaksi}', ID_Pelanggan='{idPelanggan}', Tanggal='{tanggalTransaksiStr}', Total='{totalHargaStr}'");

            // Validasi ID_Transaksi
            if (string.IsNullOrWhiteSpace(idTransaksi))
            {
                errorMessage = "ID Transaksi tidak boleh kosong.";
                return false;
            }
            if (!Regex.IsMatch(idTransaksi, @"^05\d{1,5}$")) // Validasi format ID Transaksi
            {
                errorMessage = $"ID Transaksi '{idTransaksi}' tidak sesuai format. Harus dimulai dengan '05' dan diikuti 1 hingga 5 digit angka (total 3-7 karakter).";
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

            // Validasi Tanggal_Transaksi
            if (string.IsNullOrWhiteSpace(tanggalTransaksiStr))
            {
                errorMessage = "Tanggal Transaksi tidak boleh kosong.";
                return false;
            }
            if (!DateTime.TryParse(tanggalTransaksiStr, out DateTime tanggalTransaksi))
            {
                errorMessage = $"Tanggal Transaksi '{tanggalTransaksiStr}' tidak valid. Gunakan format yang dikenali tanggal/waktu (misal: yyyy-MM-dd HH:mm:ss).";
                return false;
            }

            // Validasi Total_Harga
            if (string.IsNullOrWhiteSpace(totalHargaStr))
            {
                errorMessage = "Total Harga tidak boleh kosong.";
                return false;
            }
            if (!decimal.TryParse(totalHargaStr, out decimal totalHarga) || totalHarga < 0)
            {
                errorMessage = $"Total Harga '{totalHargaStr}' tidak valid. Harus berupa angka non-negatif yang valid.";
                return false;
            }

            Debug.WriteLine($"Validation successful for row {rowIndex}.");
            return true;
        }

        /// <summary>
        /// Imports data from the DataGridView to the Transaksi table in the database.
        /// </summary>
        private void ImportDataToDatabase()
        {
            Debug.WriteLine("Memulai ImportDataToDatabase()...");
            try
            {
                DataTable dt = (DataTable)dgvDataTransaksi.DataSource;
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
                        string currentIdTransaksi = "N/A_ID_MISSING";

                        if (row.Table.Columns.Contains("ID_Transaksi"))
                        {
                            currentIdTransaksi = row["ID_Transaksi"] != DBNull.Value ? row["ID_Transaksi"].ToString().Trim() : "N/A_EMPTY";
                        }

                        string validationError = "";
                        // i+2 karena baris Excel dimulai dari 1 dan header di baris 1
                        if (!ValidateRow(row, i + 2, out validationError))
                        {
                            failedCount++;
                            errorDetails.AppendLine($"Baris Excel {i + 2} (ID Transaksi '{currentIdTransaksi}'): Validasi gagal - {validationError}");
                            Debug.WriteLine($"Validasi gagal untuk baris Excel {i + 2} (ID Transaksi: {currentIdTransaksi}).");
                            continue; // Lanjutkan ke baris berikutnya jika validasi gagal
                        }

                        // Ambil semua data (pastikan kolom ada dan null-safe)
                        string idPelanggan = row.Table.Columns.Contains("ID_Pelanggan") ? (row["ID_Pelanggan"] != DBNull.Value ? row["ID_Pelanggan"].ToString().Trim() : "") : "";
                        DateTime tanggalTransaksi = DateTime.Parse(row["Tanggal_Transaksi"].ToString()); // Sudah divalidasi dan di-parse di ValidateRow
                        decimal totalHarga = decimal.Parse(row["Total_Harga"].ToString()); // Sudah divalidasi dan di-parse di ValidateRow

                        try
                        {
                            // Periksa apakah ID_Transaksi sudah ada di database sebelum mencoba INSERT
                            using (SqlCommand checkCmd = new SqlCommand("SELECT COUNT(1) FROM Transaksi WHERE ID_Transaksi = @ID_Transaksi", conn))
                            {
                                checkCmd.Parameters.AddWithValue("@ID_Transaksi", currentIdTransaksi);
                                if ((int)checkCmd.ExecuteScalar() > 0)
                                {
                                    failedCount++;
                                    errorDetails.AppendLine($"Baris Excel {i + 2} (ID Transaksi '{currentIdTransaksi}'): ID sudah ada di database.");
                                    Debug.WriteLine($"ID Transaksi {currentIdTransaksi} sudah ada.");
                                    continue; // Lanjutkan ke baris berikutnya
                                }
                            }

                            // Jika ID_Transaksi belum ada dan validasi lolos, lakukan INSERT
                            using (SqlCommand cmd = new SqlCommand("InsertTransaksi", conn))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@ID_Transaksi", currentIdTransaksi);
                                cmd.Parameters.AddWithValue("@ID_Pelanggan", idPelanggan);
                                cmd.Parameters.AddWithValue("@Tanggal_Transaksi", tanggalTransaksi);
                                cmd.Parameters.AddWithValue("@Total_Harga", totalHarga);

                                cmd.ExecuteNonQuery();
                                importedCount++;
                                Debug.WriteLine($"Berhasil mengimpor ID Transaksi: {currentIdTransaksi} dari baris Excel {i + 2}.");
                            }
                        }
                        catch (SqlException ex)
                        {
                            failedCount++;
                            string sqlErrorMessage = "";
                            switch (ex.Number)
                            {
                                case 2627: // Primary Key violation (duplicate ID_Transaksi)
                                    sqlErrorMessage = "ID sudah ada.";
                                    break;
                                case 547: // Foreign key or CHECK constraint violation
                                    if (ex.Message.Contains("FK_Transaksi_Pelanggan")) sqlErrorMessage = "ID Pelanggan tidak valid atau tidak ditemukan.";
                                    else if (ex.Message.Contains("CK_Transaksi_IDFormat")) sqlErrorMessage = "Format ID Transaksi tidak valid.";
                                    else if (ex.Message.Contains("CK_Transaksi_TotalHarga")) sqlErrorMessage = "Total Harga tidak valid.";
                                    else sqlErrorMessage = $"Pelanggaran constraint database: {ex.Message}";
                                    break;
                                case 8152: // String or binary data would be truncated
                                    sqlErrorMessage = "Data terlalu panjang untuk kolom ID, ID Pelanggan, atau Total Harga.";
                                    break;
                                case 50000: // Custom error dari TRIGGER atau RAISERROR
                                    sqlErrorMessage = ex.Message;
                                    break;
                                default:
                                    sqlErrorMessage = $"Kesalahan database: {ex.Message}";
                                    break;
                            }
                            errorDetails.AppendLine($"Baris Excel {i + 2} (ID Transaksi '{currentIdTransaksi}'): Gagal diimpor - {sqlErrorMessage}");
                            Debug.WriteLine($"SQL Error impor ID Transaksi {currentIdTransaksi} (Baris Excel {i + 2}): {sqlErrorMessage}");
                        }
                        catch (Exception ex)
                        {
                            failedCount++;
                            errorDetails.AppendLine($"Baris Excel {i + 2} (ID Transaksi '{currentIdTransaksi}'): Kesalahan tidak terduga - {ex.Message}");
                            Debug.WriteLine($"Error umum saat impor ID Transaksi {currentIdTransaksi} (Baris Excel {i + 2}): {ex.Message}");
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

                // Ini sangat penting agar form TRANSAKSI tahu bahwa operasi impor berhasil/selesai.
                this.DialogResult = DialogResult.OK; // Sinyalkan ke TRANSAKSI untuk merefresh
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

        /// <summary>
        /// Handles the click event for the Cancel button.
        /// Closes the preview form without importing any data.
        /// </summary>
        private void btnCancel_Click(object sender, EventArgs e) // Asumsikan tombol batal bernama btnCancel
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void oke_Click(object sender, EventArgs e) // Asumsikan tombol "Oke" yang hanya untuk menutup preview
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // Anda mungkin perlu menambahkan DataGridView ke form Anda bernama 'dgvDataTransaksi'
        // dan tombol bernama 'btnCancel' di desainer untuk kode ini bekerja.
        // Hapus event handler yang tidak digunakan jika ada duplikat di .Designer.cs
        // private void dgvPreviewTransaksi_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
    }
}