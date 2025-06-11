using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics; // For Stopwatch
using System.IO;
using System.Linq; // For .Cast<T>().FirstOrDefault()
using System.Text; // For StringBuilder in LogError
using System.Text.RegularExpressions; // Added for Regex validation
using System.Windows.Forms;

namespace HOMEPAGE
{
    public partial class TRANSAKSI : Form
    {
        // Gunakan satu connection string untuk konsistensi dan manajemen sumber daya yang tepat
        private readonly string connectionString = "Data Source=LAPTOP-T1UUTAE0\\HIKMATYAR;Initial Catalog=Manajemen_Penjualan_Parfum;Integrated Security=True";

        // Cache untuk menyimpan data transaksi guna meningkatkan kinerja
        private DataTable cacheTransaksi = null;
        // Menyimpan nilai lama untuk deteksi perubahan saat update
        private DateTime oldTanggalTransaksi;
        private decimal oldTotalHarga;
        private string oldIdPelanggan = ""; // Keep track of old ID Pelanggan for update comparison

        public TRANSAKSI()
        {
            InitializeComponent();
            LoadInitialData(); // Menampilkan data awal dan memuat cache
            LoadIdPelanggan(); // Memuat ID pelanggan ke dalam ComboBox

            // Set format DateTimePicker saat inisialisasi
            dtpTanggalTransaksi.Format = DateTimePickerFormat.Custom;
            dtpTanggalTransaksi.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            dtpTanggalTransaksi.ShowUpDown = true; // Opsi untuk menampilkan tombol naik/turun untuk waktu

            ClearInputFields(); // Membersihkan input field pada awal
        }

        /// <summary>
        /// Memuat data awal ke DataGridView dan mengisi cache.
        /// </summary>
        private void LoadInitialData()
        {
            try
            {
                // Bersihkan kolom DataGridView yang ada untuk mencegah duplikasi jika metode dipanggil berulang
                dgvTransaksi.DataSource = null;
                dgvTransaksi.Columns.Clear();

                // Muat data dari DB jika cache kosong, jika tidak gunakan cache
                if (cacheTransaksi == null)
                {
                    cacheTransaksi = new DataTable();
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter("SELECT ID_Transaksi, ID_Pelanggan, Tanggal_Transaksi, Total_Harga FROM Transaksi", conn))
                        {
                            adapter.Fill(cacheTransaksi);
                        }
                    }
                }
                dgvTransaksi.DataSource = cacheTransaksi;
                dgvTransaksi.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells); // Sesuaikan lebar kolom otomatis
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data awal: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogError(ex, "Load Initial Transaksi Data");
            }
        }

        /// <summary>
        /// Menginvalidasi cache data transaksi.
        /// Ini harus dipanggil setiap kali data di database berubah (tambah, update, hapus).
        /// </summary>
        private void InvalidateTransaksiCache()
        {
            cacheTransaksi = null;
        }

        /// <summary>
        /// Memuat ID_Pelanggan ke dalam ComboBox.
        /// </summary>
        private void LoadIdPelanggan()
        {
            try
            {
                cmbIdPelanggan.Items.Clear();
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // Ambil ID Pelanggan yang sudah di-trim untuk CHAR(4)
                    string query = "SELECT TRIM(ID_Pelanggan) AS ID_Pelanggan FROM Pelanggan ORDER BY ID_Pelanggan";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        cmbIdPelanggan.Items.Add(reader["ID_Pelanggan"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat ID pelanggan: " + ex.Message, "Error Data Load", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogError(ex, "Load ID Pelanggan ComboBox");
            }
        }

        /// <summary>
        /// Membersihkan semua field input pada form.
        /// </summary>
        private void ClearInputFields()
        {
            txtIdTransaksi.Clear();
            cmbIdPelanggan.SelectedIndex = -1; // Batalkan pilihan item yang dipilih
            dtpTanggalTransaksi.Value = DateTime.Now; // Set DateTimePicker ke tanggal/waktu saat ini
            txtTotalHarga.Clear();
            txtIdTransaksi.Focus(); // Set fokus kembali ke field ID

            // Reset nilai lama untuk deteksi perubahan
            oldTanggalTransaksi = DateTime.MinValue; // Indikator nilai awal/tidak valid
            oldTotalHarga = -1m; // Indikator nilai awal/tidak valid
            oldIdPelanggan = "";
        }

        /// <summary>
        /// Mencatat detail kesalahan ke file log untuk tujuan debugging.
        /// </summary>
        /// <param name="ex">Exception yang akan dicatat.</param>
        /// <param name="operation">Operasi saat kesalahan terjadi.</param>
        private void LogError(Exception ex, string operation)
        {
            string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
            string logFilePath = Path.Combine(logDirectory, "application_error.log");

            StringBuilder logEntry = new StringBuilder();
            logEntry.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Operation: {operation}");
            logEntry.AppendLine($"Error Message: {ex.Message}");
            logEntry.AppendLine($"Stack Trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                logEntry.AppendLine($"Inner Exception: {ex.InnerException.Message}");
                logEntry.AppendLine($"Inner Stack Trace: {ex.InnerException.StackTrace}");
            }
            logEntry.AppendLine("--------------------------------------------------\n");

            try
            {
                File.AppendAllText(logFilePath, logEntry.ToString());
            }
            catch (Exception logEx)
            {
                MessageBox.Show($"Terjadi kesalahan fatal saat mencatat log: {logEx.Message}\n" +
                                 "Silakan cek izin tulis folder aplikasi Anda.", "Error Logging Critical", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Menangani event klik untuk tombol "Tambah".
        /// Menambahkan catatan transaksi baru ke database dengan validasi.
        /// </summary>
        private void btnTambah_Click(object sender, EventArgs e)
        {
            // Validasi Input: Pastikan semua field tidak kosong
            if (string.IsNullOrWhiteSpace(txtIdTransaksi.Text))
            {
                MessageBox.Show("ID Transaksi tidak boleh kosong.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtIdTransaksi.Focus();
                return;
            }
            if (cmbIdPelanggan.SelectedItem == null)
            {
                MessageBox.Show("ID Pelanggan harus dipilih.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbIdPelanggan.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(txtTotalHarga.Text))
            {
                MessageBox.Show("Total Harga tidak boleh kosong.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTotalHarga.Focus();
                return;
            }

            // Validasi ID Transaksi format: harus dimulai dengan '05' dan panjang 3 atau 4 karakter (alphanumeric)
            // Regex: ^05[A-Za-z0-9]{1,2}$
            if (!Regex.IsMatch(txtIdTransaksi.Text.Trim(), @"^05[A-Za-z0-9]{1,2}$"))
            {
                MessageBox.Show("ID Transaksi harus dimulai dengan '05' dan memiliki panjang total 3 atau 4 karakter.", "Input Error: ID Transaksi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtIdTransaksi.Focus();
                return;
            }

            // Validasi Total Harga sebagai angka desimal yang valid dan non-negatif
            if (!decimal.TryParse(txtTotalHarga.Text, out decimal totalHarga) || totalHarga < 0)
            {
                MessageBox.Show("Total Harga harus berupa angka non-negatif yang valid.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTotalHarga.Focus();
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        // Pre-emptive check if ID Transaksi already exists for better UX
                        SqlCommand checkIdCmd = new SqlCommand("SELECT COUNT(1) FROM Transaksi WHERE TRIM(ID_Transaksi) = @ID_Transaksi", conn, tran);
                        checkIdCmd.Parameters.AddWithValue("@ID_Transaksi", txtIdTransaksi.Text.Trim());
                        int idCount = (int)checkIdCmd.ExecuteScalar();
                        if (idCount > 0)
                        {
                            MessageBox.Show($"Gagal Menambah Data: ID Transaksi '{txtIdTransaksi.Text.Trim()}' sudah ada. Silakan gunakan ID lain.", "Kesalahan Input Data", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            tran.Rollback();
                            txtIdTransaksi.Focus();
                            return;
                        }

                        SqlCommand cmd = new SqlCommand("InsertTransaksi", conn, tran)
                        {
                            CommandType = CommandType.StoredProcedure
                        };
                        cmd.Parameters.AddWithValue("@ID_Transaksi", txtIdTransaksi.Text.Trim()); // Trim ID for CHAR(4)
                        cmd.Parameters.AddWithValue("@ID_Pelanggan", cmbIdPelanggan.SelectedItem.ToString().Trim()); // Trim ID Pelanggan
                        cmd.Parameters.AddWithValue("@Total_Harga", totalHarga);
                        // Tanggal_Transaksi memiliki DEFAULT GETDATE() di SQL, jadi tidak perlu dikirim jika ingin pakai default
                        // Jika ingin mengirim tanggal dari DateTimePicker, SP InsertTransaksi perlu diubah
                        // cmd.Parameters.AddWithValue("@Tanggal_Transaksi", dtpTanggalTransaksi.Value); 

                        cmd.ExecuteNonQuery();

                        tran.Commit();
                        MessageBox.Show("Berhasil Menambah Data Transaksi.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        InvalidateTransaksiCache(); // Invalidasi cache
                        LoadInitialData();          // Muat ulang data
                        ClearInputFields();         // Bersihkan field input
                    }
                    catch (SqlException ex)
                    {
                        tran.Rollback();
                        LogError(ex, "Tambah Transaksi"); // Log kesalahan

                        string errorMessage = "Terjadi kesalahan saat menambah data transaksi: ";
                        switch (ex.Number)
                        {
                            case 2627: // Primary key violation (duplikasi ID_Transaksi)
                                errorMessage += $"ID Transaksi '{txtIdTransaksi.Text.Trim()}' sudah ada. Mohon gunakan ID yang berbeda.";
                                break;
                            case 547: // Foreign key violation (ID Pelanggan tidak ada) atau CHECK constraint violation
                                if (ex.Message.Contains("FK__Transaksi__ID_Pelanggan")) // Asumsi nama FK constraint
                                {
                                    errorMessage += "ID Pelanggan tidak ditemukan. Pastikan ID Pelanggan yang dipilih valid.";
                                }
                                else if (ex.Message.Contains("CK__Transaksi__ID_Trans")) // Asumsi nama CHECK constraint ID Transaksi
                                {
                                    errorMessage += "ID Transaksi tidak sesuai format (harus dimulai dengan '05' dan 3 atau 4 karakter).";
                                }
                                else if (ex.Message.Contains("CK__Transaksi__Total_Harga")) // Asumsi nama CHECK constraint Total Harga
                                {
                                    errorMessage += "Total Harga tidak valid (harus non-negatif).";
                                }
                                else
                                {
                                    errorMessage += $"Terdapat pelanggaran constraint database: {ex.Message}";
                                }
                                break;
                            case 8152: // String or binary data would be truncated (e.g., ID_Transaksi terlalu panjang jika tanpa TRIM)
                                errorMessage += "Panjang data terlalu panjang untuk salah satu kolom. Mohon periksa kembali input Anda.";
                                break;
                            case 50000: // Custom RAISERROR dari trigger (jika ada)
                                errorMessage = ex.Message; // Gunakan pesan dari trigger
                                break;
                            default:
                                errorMessage += $"Terjadi kesalahan database tak terduga: {ex.Message}";
                                break;
                        }
                        MessageBox.Show(errorMessage, "Error Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        LogError(ex, "Tambah Transaksi (Umum)");
                        MessageBox.Show("Terjadi kesalahan tak terduga: " + ex.Message + "\nPerubahan dibatalkan.", "Error Aplikasi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Menangani event klik untuk tombol "Hapus".
        /// Menghapus catatan transaksi dari database.
        /// </summary>
        private void btnHapus_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtIdTransaksi.Text))
            {
                MessageBox.Show("ID Transaksi harus diisi atau dipilih dari tabel untuk menghapus.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtIdTransaksi.Focus();
                return;
            }

            if (MessageBox.Show("Apakah Anda yakin ingin menghapus data transaksi ini?", "Konfirmasi Hapus", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlTransaction tran = conn.BeginTransaction())
                    {
                        try
                        {
                            SqlCommand cmd = new SqlCommand("DeleteTransaksi", conn, tran)
                            {
                                CommandType = CommandType.StoredProcedure
                            };
                            cmd.Parameters.AddWithValue("@ID_Transaksi", txtIdTransaksi.Text.Trim()); // Trim ID

                            int rowsAffected = cmd.ExecuteNonQuery();
                            tran.Commit();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Berhasil Menghapus Data Transaksi.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show($"Data transaksi dengan ID '{txtIdTransaksi.Text.Trim()}' tidak ditemukan.", "Hapus Gagal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                        catch (SqlException ex)
                        {
                            tran.Rollback();
                            LogError(ex, "Hapus Transaksi"); // Log kesalahan

                            string errorMessage = "Terjadi kesalahan saat menghapus data transaksi: ";
                            if (ex.Number == 547) // Foreign key violation (jika Transaksi direferensikan oleh tabel lain)
                            {
                                errorMessage += "Data transaksi ini tidak dapat dihapus karena terkait dengan data lain. Mohon periksa integritas data.";
                            }
                            else if (ex.Number == 50000) // Custom RAISERROR
                            {
                                errorMessage = ex.Message;
                            }
                            else
                            {
                                errorMessage += $"Terjadi kesalahan database: {ex.Message}";
                            }
                            MessageBox.Show(errorMessage, "Error Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            LogError(ex, "Hapus Transaksi (Umum)");
                            MessageBox.Show("Gagal Menghapus Data Transaksi: " + ex.Message, "Error Aplikasi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        finally
                        {
                            InvalidateTransaksiCache();
                            LoadInitialData();
                            ClearInputFields();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Menangani event klik untuk tombol "Update".
        /// Memperbarui catatan transaksi yang ada di database.
        /// </summary>
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            // Validasi Input: Pastikan semua field tidak kosong
            if (string.IsNullOrWhiteSpace(txtIdTransaksi.Text))
            {
                MessageBox.Show("ID Transaksi tidak boleh kosong untuk update.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtIdTransaksi.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(txtTotalHarga.Text))
            {
                MessageBox.Show("Total Harga tidak boleh kosong untuk update.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTotalHarga.Focus();
                return;
            }

            // Validasi Total Harga sebagai angka desimal yang valid dan non-negatif
            if (!decimal.TryParse(txtTotalHarga.Text, out decimal totalHarga) || totalHarga < 0)
            {
                MessageBox.Show("Total Harga harus berupa angka non-negatif yang valid.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTotalHarga.Focus();
                return;
            }

            // Validasi ID Transaksi format (jika memungkinkan diubah atau penting untuk konsistensi)
            if (!Regex.IsMatch(txtIdTransaksi.Text.Trim(), @"^05[A-Za-z0-9]{1,2}$"))
            {
                MessageBox.Show("ID Transaksi harus dimulai dengan '05' dan memiliki panjang total 3 atau 4 karakter (contoh: 05A, 05AB).", "Input Error: ID Transaksi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtIdTransaksi.Focus();
                return;
            }

            // Periksa perubahan aktual pada Tanggal_Transaksi, Total_Harga, dan ID_Pelanggan (jika ID Pelanggan diizinkan untuk diupdate)
            bool hasChanges = false;
            if (dtpTanggalTransaksi.Value.Date != oldTanggalTransaksi.Date || // Compare Date part to ignore time if not relevant
                dtpTanggalTransaksi.Value.TimeOfDay != oldTanggalTransaksi.TimeOfDay || // Compare Time part if relevant
                totalHarga != oldTotalHarga ||
                (cmbIdPelanggan.SelectedItem?.ToString()?.Trim() ?? "") != oldIdPelanggan.Trim()) // Check ID Pelanggan change
            {
                hasChanges = true;
            }

            if (!hasChanges)
            {
                MessageBox.Show("Tidak ada perubahan yang terdeteksi untuk diperbarui.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("Apakah Anda yakin ingin mengupdate data transaksi ini?", "Konfirmasi Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlTransaction tran = conn.BeginTransaction())
                    {
                        try
                        {
                            SqlCommand cmd = new SqlCommand("UpdateTransaksi", conn, tran)
                            {
                                CommandType = CommandType.StoredProcedure
                            };
                            cmd.Parameters.AddWithValue("@ID_Transaksi", txtIdTransaksi.Text.Trim()); // Trim ID Transaksi
                            cmd.Parameters.AddWithValue("@Tanggal_Transaksi", dtpTanggalTransaksi.Value); // Ambil nilai dari DateTimePicker
                            cmd.Parameters.AddWithValue("@Total_Harga", totalHarga);

                            // Penting: Stored Procedure UpdateTransaksi Anda saat ini tidak memiliki parameter @ID_Pelanggan.
                            // Jika Anda ingin mengizinkan ID_Pelanggan diubah, Anda harus memodifikasi SP UpdateTransaksi di SQL.
                            // Contoh penambahan parameter jika SP diubah:
                            // cmd.Parameters.AddWithValue("@ID_Pelanggan", cmbIdPelanggan.SelectedItem.ToString().Trim());

                            int rowsAffected = cmd.ExecuteNonQuery();
                            tran.Commit();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Berhasil Mengupdate Data Transaksi.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show($"Data transaksi dengan ID '{txtIdTransaksi.Text.Trim()}' tidak ditemukan untuk diupdate.", "Update Gagal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                        catch (SqlException ex)
                        {
                            tran.Rollback();
                            LogError(ex, "Update Transaksi"); // Log kesalahan

                            string errorMessage = "Terjadi kesalahan saat mengupdate data transaksi: ";
                            switch (ex.Number)
                            {
                                case 547: // Foreign key violation (jika ID Pelanggan diupdate dan tidak valid) atau CHECK constraint
                                    if (ex.Message.Contains("FK__Transaksi__ID_Pelanggan")) // Jika SP diubah dan FK dilanggar
                                    {
                                        errorMessage += "ID Pelanggan tidak valid atau tidak ditemukan.";
                                    }
                                    else if (ex.Message.Contains("CK__Transaksi__Total_Harga"))
                                    {
                                        errorMessage += "Total Harga tidak valid (harus non-negatif).";
                                    }
                                    else if (ex.Message.Contains("CK__Transaksi__ID_Trans")) // Asumsi nama CHECK constraint ID Transaksi
                                    {
                                        errorMessage += "ID Transaksi tidak sesuai format (harus dimulai dengan '05' dan 3 atau 4 karakter).";
                                    }
                                    else
                                    {
                                        errorMessage += $"Terdapat pelanggaran constraint database: {ex.Message}";
                                    }
                                    break;
                                case 8152: // String or binary data would be truncated
                                    errorMessage += "Panjang data terlalu panjang untuk salah satu kolom. Mohon periksa kembali.";
                                    break;
                                case 50000: // Custom RAISERROR
                                    errorMessage = ex.Message;
                                    break;
                                default:
                                    errorMessage += $"Terjadi kesalahan database tak terduga: {ex.Message}";
                                    break;
                            }
                            MessageBox.Show(errorMessage, "Error Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            LogError(ex, "Update Transaksi (Umum)");
                            MessageBox.Show("Gagal Mengupdate Data Transaksi: " + ex.Message, "Error Aplikasi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        finally
                        {
                            InvalidateTransaksiCache();
                            LoadInitialData();
                            ClearInputFields();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Menangani event klik untuk tombol "Refresh".
        /// Memuat ulang data dari database dan membersihkan input fields.
        /// </summary>
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            InvalidateTransaksiCache(); // Bersihkan cache
            LoadInitialData();          // Muat ulang data ke DGV dan cache
            LoadIdPelanggan();          // Refresh ComboBox ID Pelanggan
            ClearInputFields();         // Bersihkan field input
            MessageBox.Show("Data berhasil di-refresh.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Menangani event CellClick dari DataGridView.
        /// Mengisi textbox dan combobox dengan data dari baris yang dipilih.
        /// </summary>
        private void dgvTransaksi_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0 && e.RowIndex < dgvTransaksi.Rows.Count)
                {
                    DataGridViewRow row = dgvTransaksi.Rows[e.RowIndex];

                    txtIdTransaksi.Text = row.Cells["ID_Transaksi"]?.Value?.ToString()?.Trim() ?? "";

                    string idPelanggan = row.Cells["ID_Pelanggan"]?.Value?.ToString()?.Trim() ?? "";
                    // Pilih item di ComboBox
                    cmbIdPelanggan.SelectedItem = cmbIdPelanggan.Items
                        .Cast<string>()
                        .FirstOrDefault(item => item.Trim() == idPelanggan); // Gunakan Trim() untuk item ComboBox juga

                    // Ambil nilai tanggal dari DataGridView dan set ke DateTimePicker
                    if (row.Cells["Tanggal_Transaksi"]?.Value is DateTime parsedDate) // Direct cast if already DateTime
                    {
                        dtpTanggalTransaksi.Value = parsedDate;
                        oldTanggalTransaksi = parsedDate; // Simpan untuk deteksi perubahan
                    }
                    else if (DateTime.TryParse(row.Cells["Tanggal_Transaksi"]?.Value?.ToString(), out parsedDate)) // Try parse if not direct DateTime
                    {
                        dtpTanggalTransaksi.Value = parsedDate;
                        oldTanggalTransaksi = parsedDate; // Simpan untuk deteksi perubahan
                    }
                    else
                    {
                        dtpTanggalTransaksi.Value = DateTime.Now; // Default jika gagal parse
                        oldTanggalTransaksi = DateTime.MinValue; // Tandai bahwa nilai lama tidak valid
                    }

                    // Ambil Total_Harga dari DGV dan simpan nilai lama
                    if (row.Cells["Total_Harga"]?.Value is decimal parsedTotalHarga) // Direct cast if already decimal
                    {
                        txtTotalHarga.Text = parsedTotalHarga.ToString();
                        oldTotalHarga = parsedTotalHarga;
                    }
                    else if (decimal.TryParse(row.Cells["Total_Harga"]?.Value?.ToString(), out parsedTotalHarga)) // Try parse if not direct decimal
                    {
                        txtTotalHarga.Text = parsedTotalHarga.ToString();
                        oldTotalHarga = parsedTotalHarga;
                    }
                    else
                    {
                        txtTotalHarga.Clear();
                        oldTotalHarga = -1m; // Default jika gagal parse
                    }

                    // Simpan oldIdPelanggan untuk deteksi perubahan pada update
                    oldIdPelanggan = idPelanggan;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memilih data dari tabel: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogError(ex, "DataGridView CellClick");
            }
        }

        /// <summary>
        /// Menangani event klik untuk tombol "Import".
        /// Membuka dialog file untuk memilih file Excel dan menampilkan pratinjaunya.
        /// </summary>
        private void btnImport_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFile = new OpenFileDialog())
            {
                openFile.Filter = "Excel Files|*.xlsx;*.xlsm";
                openFile.Title = "Pilih File Excel untuk Import Data Transaksi";
                if (openFile.ShowDialog() == DialogResult.OK)
                {
                    PreviewData(openFile.FileName);
                }
            }
        }

        /// <summary>
        /// Membaca data dari file Excel yang ditentukan dan menampilkannya dalam formulir pratinjau.
        /// Ini mencakup perbaikan untuk membaca nilai sel formula dengan benar.
        /// </summary>
        /// <param name="filePath">Path ke file Excel.</param>
        private void PreviewData(string filePath)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    IWorkbook workbook = new XSSFWorkbook(fs);
                    ISheet sheet = workbook.GetSheetAt(0); // Ambil sheet pertama
                    DataTable dt = new DataTable();

                    // Baca baris header dan tambahkan kolom ke DataTable
                    IRow headerRow = sheet.GetRow(0);
                    if (headerRow != null)
                    {
                        foreach (ICell cell in headerRow.Cells)
                        {
                            string columnName = cell.ToString();
                            // Pastikan nama kolom unik dan tidak kosong
                            if (!string.IsNullOrEmpty(columnName) && !dt.Columns.Contains(columnName))
                            {
                                dt.Columns.Add(columnName);
                            }
                            else if (string.IsNullOrEmpty(columnName))
                            {
                                dt.Columns.Add($"Column{dt.Columns.Count + 1}"); // Beri nama generik untuk header kosong
                            }
                            else
                            {
                                int i = 1;
                                while (dt.Columns.Contains(columnName + "_" + i)) // Atasi nama duplikat
                                {
                                    i++;
                                }
                                dt.Columns.Add(columnName + "_" + i);
                            }
                        }
                    }

                    // Baca baris data, mulai dari baris kedua (indeks 1)
                    for (int i = 1; i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row == null) continue; // Lewati baris kosong

                        DataRow newRow = dt.NewRow();
                        // Iterasi berdasarkan jumlah kolom DataTable untuk menghindari indeks di luar batas
                        for (int j = 0; j < dt.Columns.Count; j++)
                        {
                            ICell cell = row.GetCell(j);
                            if (cell != null)
                            {
                                // LOGIKA DIPERBAIKI UNTUK MENANGANI SEL FORMULA
                                switch (cell.CellType)
                                {
                                    case CellType.String:
                                        newRow[j] = cell.StringCellValue;
                                        break;
                                    case CellType.Numeric:
                                        if (DateUtil.IsCellDateFormatted(cell))
                                        {
                                            newRow[j] = cell.DateCellValue;
                                        }
                                        else
                                        {
                                            newRow[j] = cell.NumericCellValue;
                                        }
                                        break;
                                    case CellType.Boolean:
                                        newRow[j] = cell.BooleanCellValue;
                                        break;
                                    case CellType.Formula:
                                        // Ambil tipe hasil cache dan kemudian ambil nilainya
                                        switch (cell.CachedFormulaResultType)
                                        {
                                            case CellType.String:
                                                newRow[j] = cell.StringCellValue;
                                                break;
                                            case CellType.Numeric:
                                                if (DateUtil.IsCellDateFormatted(cell))
                                                {
                                                    newRow[j] = cell.DateCellValue;
                                                }
                                                else
                                                {
                                                    newRow[j] = cell.NumericCellValue;
                                                }
                                                break;
                                            case CellType.Boolean:
                                                newRow[j] = cell.BooleanCellValue;
                                                break;
                                            case CellType.Error:
                                                newRow[j] = FormulaError.ForInt((int)cell.ErrorCellValue).String;
                                                break;
                                            default:
                                                // Fallback untuk tipe hasil formula lain atau jika evaluasi tidak di-cache
                                                newRow[j] = cell.ToString();
                                                break;
                                        }
                                        break;
                                    default:
                                        newRow[j] = cell.ToString(); // Menangani CellType.Blank, dll.
                                        break;
                                }
                            }
                            else
                            {
                                newRow[j] = DBNull.Value; // Tetapkan DBNull.Value untuk sel kosong
                            }
                        }
                        dt.Rows.Add(newRow);
                    }

                    // Pastikan Anda memiliki formulir bernama 'PreviewDataTransaksi' yang menerima DataTable di konstruktornya
                    using (PreviewDataTransaksi previewForm = new PreviewDataTransaksi(dt))
                    {
                        previewForm.ShowDialog();
                    }
                    InvalidateTransaksiCache(); // Invalidasi cache setelah potensi impor
                    LoadInitialData();          // Muat ulang data
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show("Gagal membaca file Excel. Pastikan file tidak sedang dibuka oleh program lain dan lokasinya benar.\nDetail: " + ex.Message, "Error File Akses", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogError(ex, "Preview Excel Data Transaksi (Akses File)");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal membaca file Excel: " + ex.Message, "Error Impor Excel", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogError(ex, "Preview Excel Data Transaksi (Umum)");
            }
        }


        /// <summary>
        /// Menangani event klik untuk tombol "Analisis".
        /// Melakukan analisis kinerja sederhana dari query SELECT dan menampilkan informasi indeks untuk tabel 'Transaksi'.
        /// </summary>
        private void Analisis_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // 1. Dapatkan informasi indeks untuk tabel 'Transaksi'
                    string indexQuery = @"
                    SELECT
                        t.name AS TableName,
                        ind.name AS IndexName,
                        ind.type_desc AS IndexType,
                        col.name AS ColumnName
                    FROM
                        sys.indexes ind
                    INNER JOIN
                        sys.index_columns ic ON ind.object_id = ic.object_id AND ind.index_id = ic.index_id
                    INNER JOIN
                        sys.columns col ON ic.object_id = col.object_id AND ic.column_id = col.column_id
                    INNER JOIN
                        sys.tables t ON ind.object_id = t.object_id
                    WHERE
                        t.name = 'Transaksi' AND ind.is_primary_key = 0 AND ind.[type] <> 0
                    ORDER BY
                        t.name, ind.name, ind.index_id;";

                    SqlCommand cmd = new SqlCommand(indexQuery, conn);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dtIndex = new DataTable();
                    da.Fill(dtIndex);

                    // 2. Ukur waktu eksekusi query SELECT pada 'Transaksi'
                    string selectQuery = "SELECT ID_Transaksi, ID_Pelanggan, Tanggal_Transaksi, Total_Harga FROM Transaksi";
                    SqlCommand cmd2 = new SqlCommand(selectQuery, conn);
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    using (SqlDataReader reader = cmd2.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Hanya baca untuk mengonsumsi hasil dan mengukur waktu
                        }
                    }
                    stopwatch.Stop();

                    long execTimeMs = stopwatch.ElapsedMilliseconds;
                    StringBuilder infoBuilder = new StringBuilder();
                    infoBuilder.AppendLine($"📌 Waktu Eksekusi Query 'SELECT ... FROM Transaksi': {execTimeMs} ms\n");
                    infoBuilder.AppendLine("📦 Indeks pada Tabel 'Transaksi':");

                    // 3. Tampilkan hasil analisis
                    if (dtIndex.Rows.Count > 0)
                    {
                        foreach (DataRow row in dtIndex.Rows)
                        {
                            infoBuilder.AppendLine($"- Tabel: {row["TableName"]}, Index: {row["IndexName"]}, Tipe: {row["IndexType"]}, Kolom: {row["ColumnName"]}");
                        }
                    }
                    else
                    {
                        infoBuilder.AppendLine("Tidak ada indeks non-primary key yang ditemukan untuk tabel Transaksi.");
                    }

                    MessageBox.Show(infoBuilder.ToString(), "Analisis Query & Index Transaksi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menganalisis: " + ex.Message, "Error Analisis", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogError(ex, "Analisis Transaksi");
            }
        }

        /// <summary>
        /// Menangani event klik untuk tombol "Kembali".
        /// Menutup form saat ini dan membuka form HalamanMenu.
        /// </summary>
        private void btnKEMBALI_Click(object sender, EventArgs e)
        {
            HalamanMenu HalamanMenuForm = new HalamanMenu();
            HalamanMenuForm.Show();
            this.Hide();
        }

        // Event handler kosong yang bisa dihapus jika tidak digunakan di desainer form
        private void TRANSAKSI_Load(object sender, EventArgs e) { /* Tidak ada implementasi yang diperlukan */ }
        private void dtpTanggalTransaksi_ValueChanged(object sender, EventArgs e) { /* Tidak ada implementasi yang diperlukan */ }
    }
}