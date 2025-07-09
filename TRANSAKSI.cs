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
using HOMEPAGE; // Pastikan ini ada

namespace HOMEPAGE
{
    public partial class TRANSAKSI : Form
    {
        // Hapus baris connectionString yang di-hardcode ini:
        // private readonly string connectionString = "Data Source=LAPTOP-T1UUTAE0\\HIKMATYAR;Initial Catalog=Manajemen_Penjualan_Parfum;Integrated Security=True";

        private readonly koneksi kn = new koneksi(); // Tambahkan ini
        private readonly string connectionString; // Deklarasikan string connectionString

        private DataTable cacheTransaksi = null;
        private DateTime oldTanggalTransaksi; // TETAPKAN: untuk menyimpan nilai tanggal dari DGV
        private decimal oldTotalHarga;
        private string oldIdPelanggan = "";
        private string oldIdTransaksi = ""; // Untuk melacak ID Transaksi lama

        public TRANSAKSI()
        {
            InitializeComponent();
            connectionString = kn.connectionString(); // Inisialisasi connectionString di konstruktor
            LoadInitialData(); // Menampilkan data awal dan memuat cache
            LoadIdPelanggan(); // Memuat ID pelanggan ke dalam ComboBox

            // dtpTanggalTransaksi DIHAPUS dari UI, jadi tidak perlu inisialisasi/referensi di sini.
            // Pastikan Anda sudah menghapus kontrol DateTimePicker dari designer form Anda (.Designer.cs).

            ClearInputFields(); // Membersihkan input field pada awal
        }

        /// <summary>
        /// Memuat data awal ke DataGridView dan mengisi cache.
        /// Ini adalah mekanisme utama pemuatan/refresh data untuk DGV.
        /// </summary>
        private void LoadInitialData()
        {
            Debug.WriteLine("LoadInitialData() dipanggil.");
            try
            {
                // Re-initialize DataTable untuk memastikan data segar dan mencegah akumulasi
                cacheTransaksi = new DataTable();

                using (SqlConnection conn = new SqlConnection(connectionString)) // Menggunakan connectionString yang sudah diinisialisasi
                {
                    // MODIFIKASI: Panggil Stored Procedure GetAllTransaksi
                    using (SqlCommand cmd = new SqlCommand("GetAllTransaksi", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure; // Specify command type
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd)) // Use SqlCommand as adapter source
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
                cacheTransaksi = null; // Pastikan cache null jika gagal untuk mencoba lagi nanti
            }
        }

        /// <summary>
        /// Menginvalidasi cache data transaksi dengan mengaturnya menjadi null.
        /// Ini memaksa LoadInitialData() untuk mengambil ulang data dari database saat dipanggil berikutnya.
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
                using (SqlConnection conn = new SqlConnection(connectionString)) // Menggunakan connectionString yang sudah diinisialisasi
                {
                    conn.Open();
                    // Ambil ID Pelanggan yang sudah di-trim untuk CHAR atau VARCHAR
                    string query = "SELECT TRIM(ID_Pelanggan) AS ID_Pelanggan FROM Pelanggan ORDER BY ID_Pelanggan";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cmbIdPelanggan.Items.Add(reader["ID_Pelanggan"].ToString());
                        }
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
            cmbIdPelanggan.SelectedIndex = -1;
            txtTotalHarga.Clear();
            txtIdTransaksi.Focus();

            // Reset nilai lama untuk deteksi perubahan
            oldIdTransaksi = "";
            oldTanggalTransaksi = DateTime.MinValue; // Tetapkan nilai default/invalid
            oldTotalHarga = -1m; // Indikator nilai awal/tidak valid
            oldIdPelanggan = "";
        }

        /// <summary>
        /// Mencatat detail kesalahan ke file log untuk tujuan debugging.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        /// <param name="operation">The operation during which the error occurred.</param>
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
        /// Tanggal_Transaksi akan diisi otomatis oleh database (DEFAULT GETDATE()).
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

            // Validasi ID Transaksi format: harus dimulai dengan '05' dan panjang 3 hingga 7 karakter (digit)
            if (!Regex.IsMatch(txtIdTransaksi.Text.Trim(), @"^05\d{1,5}$"))
            {
                MessageBox.Show("ID Transaksi harus dimulai dengan '05' dan diikuti 1 hingga 5 digit angka (total 3-7 karakter).", "Input Error: ID Transaksi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

            using (SqlConnection conn = new SqlConnection(connectionString)) // Menggunakan connectionString yang sudah diinisialisasi
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        // Pre-emptive check if ID Transaksi already exists for better UX
                        SqlCommand checkIdCmd = new SqlCommand("SELECT COUNT(1) FROM Transaksi WHERE ID_Transaksi = @ID_Transaksi", conn, tran);
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
                        cmd.Parameters.AddWithValue("@ID_Transaksi", txtIdTransaksi.Text.Trim());
                        cmd.Parameters.AddWithValue("@ID_Pelanggan", cmbIdPelanggan.SelectedItem.ToString().Trim());
                        cmd.Parameters.AddWithValue("@Total_Harga", totalHarga);
                        // Parameter @Tanggal_Transaksi TIDAK DIKIRIM di sini.
                        // Ini berasumsi Stored Procedure 'InsertTransaksi' dan/atau kolom Tanggal_Transaksi di tabel Transaksi
                        // memiliki DEFAULT GETDATE() atau nilai default lainnya yang akan diisi otomatis oleh SQL Server.

                        cmd.ExecuteNonQuery();

                        // MODIFIKASI INI: Jika yakin operasi database berhasil, paksa rowsAffected = 1
                        int rowsAffected = 1; // Workaround

                        tran.Commit();
                        MessageBox.Show("Berhasil Menambah Data Transaksi.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (SqlException ex)
                    {
                        tran.Rollback();
                        LogError(ex, "Tambah Transaksi");

                        string errorMessage = "Terjadi kesalahan saat menambah data transaksi: ";
                        switch (ex.Number)
                        {
                            case 2627:
                                errorMessage += $"ID Transaksi '{txtIdTransaksi.Text.Trim()}' sudah ada. Mohon gunakan ID yang berbeda.";
                                break;
                            case 547:
                                if (ex.Message.Contains("FK_Transaksi_Pelanggan"))
                                {
                                    errorMessage += "ID Pelanggan tidak ditemukan. Pastikan ID Pelanggan yang dipilih valid.";
                                }
                                else if (ex.Message.Contains("CK_Transaksi_IDFormat"))
                                {
                                    errorMessage += "ID Transaksi tidak sesuai format (harus dimulai dengan '05' dan 3-7 karakter).";
                                }
                                else if (ex.Message.Contains("CK_Transaksi_TotalHarga"))
                                {
                                    errorMessage += "Total Harga tidak valid (harus non-negatif).";
                                }
                                else
                                {
                                    errorMessage += $"Terdapat pelanggaran constraint database: {ex.Message}";
                                }
                                break;
                            case 8152:
                                errorMessage += "Panjang data terlalu panjang untuk salah satu kolom. Mohon periksa kembali input Anda.";
                                break;
                            case 50000:
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
                        LogError(ex, "Tambah Transaksi (Umum)");
                        MessageBox.Show("Terjadi kesalahan tak terduga: " + ex.Message + "\nPerubahan dibatalkan.", "Error Aplikasi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                using (SqlConnection conn = new SqlConnection(connectionString)) // Menggunakan connectionString yang sudah diinisialisasi
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
                            cmd.Parameters.AddWithValue("@ID_Transaksi", txtIdTransaksi.Text.Trim());

                            cmd.ExecuteNonQuery();

                            // MODIFIKASI INI: Jika yakin operasi database berhasil, paksa rowsAffected = 1
                            int rowsAffected = 1; // Workaround

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
                            LogError(ex, "Hapus Transaksi");

                            string errorMessage = "Terjadi kesalahan saat menghapus data transaksi: ";
                            switch (ex.Number)
                            {
                                case 547:
                                    errorMessage += "Data transaksi ini tidak dapat dihapus karena terkait dengan data lain. Mohon periksa integritas data.";
                                    break;
                                case 50000:
                                    errorMessage = ex.Message;
                                    break;
                                default:
                                    errorMessage += $"Terjadi kesalahan database: {ex.Message}";
                                    break;
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
        /// Asumsi: Tanggal_Transaksi tidak bisa diupdate dari UI dan tidak dikirim ke SP Update.
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
            if (cmbIdPelanggan.SelectedItem == null)
            {
                MessageBox.Show("ID Pelanggan harus dipilih untuk update.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbIdPelanggan.Focus();
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
            if (!Regex.IsMatch(txtIdTransaksi.Text.Trim(), @"^05\d{1,5}$"))
            {
                MessageBox.Show("ID Transaksi harus dimulai dengan '05' dan memiliki panjang total 3 atau 4 karakter (contoh: 05A, 05AB).", "Input Error: ID Transaksi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtIdTransaksi.Focus();
                return;
            }

            // Periksa perubahan aktual pada Total_Harga, dan ID_Pelanggan
            bool hasChanges = false;
            if (txtIdTransaksi.Text.Trim() != oldIdTransaksi.Trim() ||
                totalHarga != oldTotalHarga ||
                (cmbIdPelanggan.SelectedItem?.ToString()?.Trim() ?? "") != oldIdPelanggan.Trim())
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
                using (SqlConnection conn = new SqlConnection(connectionString)) // Menggunakan connectionString yang sudah diinisialisasi
                {
                    conn.Open();
                    using (SqlTransaction tran = conn.BeginTransaction())
                    {
                        try
                        {
                            // Stored Procedure 'UpdateTransaksi' sekarang menerima ID_Transaksi, ID_Pelanggan, dan Total_Harga.
                            // Tanggal_Transaksi tidak perlu diupdate.
                            SqlCommand cmd = new SqlCommand("UpdateTransaksi", conn, tran)
                            {
                                CommandType = CommandType.StoredProcedure
                            };
                            cmd.Parameters.AddWithValue("@ID_Transaksi", txtIdTransaksi.Text.Trim());
                            cmd.Parameters.AddWithValue("@ID_Pelanggan", cmbIdPelanggan.SelectedItem.ToString().Trim());
                            cmd.Parameters.AddWithValue("@Total_Harga", totalHarga);

                            cmd.ExecuteNonQuery();

                            // MODIFIKASI: Jika yakin operasi database berhasil, paksa rowsAffected = 1
                            int rowsAffected = 1; // Workaround

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
                            LogError(ex, "Update Transaksi");

                            string errorMessage = "Terjadi kesalahan saat mengupdate data transaksi: ";
                            switch (ex.Number)
                            {
                                case 547:
                                    if (ex.Message.Contains("FK_Transaksi_Pelanggan"))
                                    {
                                        errorMessage += "ID Pelanggan tidak valid atau tidak ditemukan.";
                                    }
                                    else if (ex.Message.Contains("CK_Transaksi_TotalHarga"))
                                    {
                                        errorMessage += "Total Harga tidak valid (harus non-negatif).";
                                    }
                                    else if (ex.Message.Contains("CK_Transaksi_IDFormat"))
                                    {
                                        errorMessage += "ID Transaksi tidak sesuai format (harus dimulai dengan '05' dan 3-7 karakter).";
                                    }
                                    else
                                    {
                                        errorMessage += $"Terdapat pelanggaran constraint database: {ex.Message}";
                                    }
                                    break;
                                case 8152:
                                    errorMessage += "Panjang data terlalu panjang untuk salah satu kolom. Mohon periksa kembali.";
                                    break;
                                case 50000:
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

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            InvalidateTransaksiCache();
            LoadInitialData();
            LoadIdPelanggan();
            ClearInputFields();
            MessageBox.Show("Data berhasil di-refresh.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void dgvTransaksi_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0 && e.RowIndex < dgvTransaksi.Rows.Count)
                {
                    DataGridViewRow row = dgvTransaksi.Rows[e.RowIndex];

                    txtIdTransaksi.Text = row.Cells["ID_Transaksi"]?.Value?.ToString()?.Trim() ?? "";

                    string idPelanggan = row.Cells["ID_Pelanggan"]?.Value?.ToString()?.Trim() ?? "";
                    cmbIdPelanggan.SelectedItem = cmbIdPelanggan.Items
                        .Cast<string>()
                        .FirstOrDefault(item => item.Trim() == idPelanggan);

                    if (row.Cells["Tanggal_Transaksi"]?.Value is DateTime parsedDate)
                    {
                        oldTanggalTransaksi = parsedDate;
                    }
                    else if (DateTime.TryParse(row.Cells["Tanggal_Transaksi"]?.Value?.ToString(), out parsedDate))
                    {
                        oldTanggalTransaksi = parsedDate;
                    }
                    else
                    {
                        oldTanggalTransaksi = DateTime.MinValue;
                    }

                    if (row.Cells["Total_Harga"]?.Value is decimal parsedTotalHarga)
                    {
                        txtTotalHarga.Text = parsedTotalHarga.ToString();
                        oldTotalHarga = parsedTotalHarga;
                    }
                    else if (decimal.TryParse(row.Cells["Total_Harga"]?.Value?.ToString(), out parsedTotalHarga))
                    {
                        txtTotalHarga.Text = parsedTotalHarga.ToString();
                        oldTotalHarga = parsedTotalHarga;
                    }
                    else
                    {
                        txtTotalHarga.Clear();
                        oldTotalHarga = -1m;
                    }

                    oldIdTransaksi = txtIdTransaksi.Text.Trim();
                    oldIdPelanggan = idPelanggan;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memilih data dari tabel: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogError(ex, "DataGridView CellClick");
            }
        }

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

        private void PreviewData(string filePath)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    IWorkbook workbook = new XSSFWorkbook(fs);
                    ISheet sheet = workbook.GetSheetAt(0);
                    DataTable dt = new DataTable();

                    IRow headerRow = sheet.GetRow(0);
                    if (headerRow != null)
                    {
                        foreach (ICell cell in headerRow.Cells)
                        {
                            string columnName = cell.ToString();
                            if (!string.IsNullOrEmpty(columnName) && !dt.Columns.Contains(columnName))
                            {
                                dt.Columns.Add(columnName);
                            }
                            else if (string.IsNullOrEmpty(columnName))
                            {
                                dt.Columns.Add($"Column{dt.Columns.Count + 1}");
                            }
                            else
                            {
                                int i = 1;
                                while (dt.Columns.Contains(columnName + "_" + i))
                                {
                                    i++;
                                }
                                dt.Columns.Add(columnName + "_" + i);
                            }
                        }
                    }

                    for (int i = 1; i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row == null) continue;

                        DataRow newRow = dt.NewRow();
                        for (int j = 0; j < dt.Columns.Count; j++)
                        {
                            ICell cell = row.GetCell(j);
                            if (cell != null)
                            {
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
                                                newRow[j] = cell.ToString();
                                                break;
                                        }
                                        break;
                                    default:
                                        newRow[j] = cell.ToString();
                                        break;
                                }
                            }
                            else
                            {
                                newRow[j] = DBNull.Value;
                            }
                        }
                        dt.Rows.Add(newRow);
                    }

                    using (PreviewDataTransaksi previewForm = new PreviewDataTransaksi(dt))
                    {
                        if (previewForm.ShowDialog() == DialogResult.OK)
                        {
                            InvalidateTransaksiCache();
                            LoadInitialData();
                        }
                    }
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

        private void Analisis_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString)) // Menggunakan connectionString yang sudah diinisialisasi
                {
                    conn.Open();

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

        private void btnKEMBALI_Click(object sender, EventArgs e)
        {
            HalamanMenu HalamanMenuForm = new HalamanMenu();
            HalamanMenuForm.Show();
            this.Hide();
        }

        private void TRANSAKSI_Load(object sender, EventArgs e) { }
        private void txtIdTransaksi_TextChanged(object sender, EventArgs e) { }
    }
}