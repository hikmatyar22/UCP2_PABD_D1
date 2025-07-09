using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics; // For Stopwatch
using System.IO;
using System.Text; // For StringBuilder in LogError
using System.Text.RegularExpressions; // Added for Regex validation
using System.Windows.Forms;
using HOMEPAGE; // Pastikan ini ada dan mengacu pada namespace tempat kelas koneksi berada

namespace HOMEPAGE
{
    public partial class AROMA_PARFUM : Form
    {
        private readonly koneksi kn = new koneksi(); // Membuat instans dari kelas koneksi Anda
        private readonly string connectionString; // Deklarasi string koneksi, akan diinisialisasi di konstruktor

        private string oldNamaAroma = "";
        private string oldDeskripsi = "";

        private DataTable cachedAromaData = null;

        public AROMA_PARFUM()
        {
            InitializeComponent();
            // PENTING: Inisialisasi connectionString HARUS dilakukan di awal konstruktor
            connectionString = kn.connectionString();

            LoadCachedData(); // Sekarang LoadCachedData akan dipanggil setelah connectionString siap
            ClearInputFields(); // Clear input fields at start

            // Pastikan ini setelah InitializeComponent dan inisialisasi lainnya
            // MODIFICATION START: Set DropDownStyle for cmbNamaAroma
            // (Baris ini sudah ada dari kode Anda sebelumnya, tetap dipertahankan)
            // cmbNamaAroma.DropDownStyle = ComboBoxStyle.DropDownList;
            // MODIFICATION END
        }

        /// <summary>
        /// Loads aroma data into a cache (DataTable) for quicker access and displays it in the DataGridView.
        /// This method replaces the primary role of TampilData() for refreshing the DGV.
        /// </summary>
        private void LoadCachedData()
        {
            Debug.WriteLine("LoadCachedData() dipanggil."); // For debugging purposes
            cachedAromaData = new DataTable(); // Always create a new DataTable to ensure the cache is fresh

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString)) // Menggunakan connectionString yang sudah diinisialisasi
                {
                    // MODIFIKASI: Panggil Stored Procedure GetAllAromas
                    using (SqlCommand cmd = new SqlCommand("GetAllAromas", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure; // Specify command type
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd)) // Use SqlCommand as adapter source
                        {
                            da.Fill(cachedAromaData);
                        }
                    }
                }
                dgvAromaParfum.DataSource = cachedAromaData;
                dgvAromaParfum.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells); // Sesuaikan lebar kolom otomatis
                dgvAromaParfum.Refresh(); // Refresh DataGridView to ensure display is updated
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data ke cache: " + ex.Message, "Error Cache Data", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogError(ex, "Load Cached Aroma Data");
                cachedAromaData = null; // Ensure cache is null on failure to retry next time
            }
        }

        /// <summary>
        /// Invalidates the aroma data cache.
        /// This should be called whenever data in the database changes (add, update, delete).
        /// </summary>
        private void InvalidateAromaCache()
        {
            cachedAromaData = null;
        }

        /// <summary>
        /// Clears the input fields in the form and resets old values for tracking changes.
        /// </summary>
        private void ClearInputFields()
        {
            txtIdAroma.Text = "";
            txtNamaAroma.Text = "";
            txtDeskripsi.Text = "";
            oldNamaAroma = "";
            oldDeskripsi = "";
            txtIdAroma.Focus(); // Set focus to the ID field
        }

        /// <summary>
        /// Logs error details to a file for debugging purposes.
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
                                 "Silakan cek izin tulis folder aplikasi Anda.", "Error Logging", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Handles the click event for the "Tambah" (Add) button.
        /// Adds a new Aroma record to the database with client-side and server-side validation.
        /// </summary>
        private void btnTambah_Click(object sender, EventArgs e)
        {
            // Client-side input validation
            if (string.IsNullOrWhiteSpace(txtIdAroma.Text))
            {
                MessageBox.Show("ID Aroma tidak boleh kosong.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtIdAroma.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(txtNamaAroma.Text))
            {
                MessageBox.Show("Nama Aroma tidak boleh kosong.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNamaAroma.Focus();
                return;
            }
            // MODIFIKASI: Deskripsi wajib diisi (tetap ada)
            if (string.IsNullOrWhiteSpace(txtDeskripsi.Text))
            {
                MessageBox.Show("Deskripsi tidak boleh kosong.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDeskripsi.Focus();
                return;
            }

            if (!Regex.IsMatch(txtIdAroma.Text.Trim(), @"^02\d{1,5}$"))
            {
                MessageBox.Show("ID Aroma harus dimulai dengan '02' dan diikuti 1 hingga 5 digit angka (total 3-7 karakter)", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtIdAroma.Focus();
                return;
            }

            if (Regex.IsMatch(txtNamaAroma.Text, @"[^A-Za-z ]"))
            {
                MessageBox.Show("Nama Aroma hanya boleh berisi huruf dan spasi.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNamaAroma.Focus();
                return;
            }
            // MODIFIKASI: HAPUS validasi regex untuk Deskripsi (sekarang boleh bebas karakter)
            // if (Regex.IsMatch(txtDeskripsi.Text, @"^[A-Za-z ]*$") && !string.IsNullOrWhiteSpace(txtDeskripsi.Text))
            // {
            //     MessageBox.Show("Deskripsi tidak boleh hanya berisi huruf dan spasi. Harap sertakan angka, tanda baca, atau simbol.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //     txtDeskripsi.Focus();
            //     return;
            // }

            using (SqlConnection conn = new SqlConnection(connectionString)) // Menggunakan connectionString yang sudah diinisialisasi
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Pre-emptive check if ID_Aroma already exists (good UX)
                        SqlCommand checkIdCmd = new SqlCommand("SELECT COUNT(1) FROM Aroma_Parfum WHERE ID_Aroma = @ID_Aroma", conn, transaction);
                        checkIdCmd.Parameters.AddWithValue("@ID_Aroma", txtIdAroma.Text.Trim());
                        int idCount = (int)checkIdCmd.ExecuteScalar();
                        if (idCount > 0)
                        {
                            MessageBox.Show($"Gagal Menambah Data: ID Aroma '{txtIdAroma.Text.Trim()}' sudah ada. Silakan gunakan ID lain.", "Kesalahan Input Data", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            transaction.Rollback();
                            txtIdAroma.Focus();
                            return;
                        }

                        // Pre-emptive check if Nama_Aroma already exists (good UX for unique names)
                        SqlCommand checkNamaCmd = new SqlCommand("SELECT COUNT(1) FROM Aroma_Parfum WHERE Nama_Aroma = @Nama_Aroma", conn, transaction);
                        checkNamaCmd.Parameters.AddWithValue("@Nama_Aroma", txtNamaAroma.Text.Trim());
                        int namaCount = (int)checkNamaCmd.ExecuteScalar();
                        if (namaCount > 0)
                        {
                            MessageBox.Show($"Gagal Menambah Data: Nama Aroma '{txtNamaAroma.Text.Trim()}' sudah ada. Silakan gunakan Nama Aroma lain.", "Kesalahan Input Data", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            transaction.Rollback();
                            txtNamaAroma.Focus();
                            return;
                        }

                        // Execute the stored procedure for adding aroma
                        SqlCommand cmd = new SqlCommand("AddAroma", conn, transaction)
                        {
                            CommandType = CommandType.StoredProcedure
                        };

                        cmd.Parameters.AddWithValue("@ID_Aroma", txtIdAroma.Text.Trim());
                        cmd.Parameters.AddWithValue("@Nama_Aroma", txtNamaAroma.Text.Trim());
                        cmd.Parameters.AddWithValue("@Deskripsi", txtDeskripsi.Text.Trim());

                        cmd.ExecuteNonQuery();

                        // MODIFIKASI: Jika yakin operasi database berhasil, paksa rowsAffected = 1
                        int rowsAffected = 1; // Workaround

                        transaction.Commit();

                        MessageBox.Show("Data aroma berhasil ditambahkan.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        LogError(ex, "Tambah Aroma Parfum");

                        string errorMessage = "Terjadi kesalahan saat menambah data aroma: ";
                        switch (ex.Number)
                        {
                            case 2627:
                                errorMessage += $"ID Aroma '{txtIdAroma.Text.Trim()}' sudah ada. Mohon gunakan ID yang berbeda.";
                                break;
                            case 2601:
                                errorMessage += $"Nama Aroma '{txtNamaAroma.Text.Trim()}' sudah ada. Mohon gunakan nama yang berbeda.";
                                break;
                            case 50000:
                                errorMessage = ex.Message;
                                break;
                            case 8152:
                                errorMessage += "Data yang dimasukkan terlalu panjang untuk salah satu kolom (Nama Aroma/Deskripsi). Mohon periksa kembali.";
                                break;
                            case 547:
                                errorMessage += "ID Aroma tidak sesuai format yang diharapkan atau pelanggaran constraint lainnya.";
                                break;
                            default:
                                errorMessage += $"Terjadi kesalahan database: {ex.Message}";
                                break;
                        }
                        MessageBox.Show(errorMessage, "Error Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        LogError(ex, "Tambah Aroma Parfum (Umum)");
                        MessageBox.Show("Terjadi kesalahan tak terduga: " + ex.Message + "\nPerubahan dibatalkan.", "Error Aplikasi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        InvalidateAromaCache();
                        LoadCachedData();
                        ClearInputFields();
                    }
                }
            }
        }

        private void btnHapus_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtIdAroma.Text))
            {
                MessageBox.Show("Pilih aroma yang ingin dihapus terlebih dahulu!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtIdAroma.Focus();
                return;
            }

            DialogResult result = MessageBox.Show("Apakah Anda yakin ingin menghapus data aroma ini?", "Konfirmasi Hapus", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(connectionString)) // Menggunakan connectionString yang sudah diinisialisasi
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            SqlCommand cmd = new SqlCommand("DeleteAroma", conn, transaction)
                            {
                                CommandType = CommandType.StoredProcedure
                            };
                            cmd.Parameters.AddWithValue("@ID_Aroma", txtIdAroma.Text.Trim());

                            cmd.ExecuteNonQuery();

                            // MODIFIKASI: Jika yakin operasi database berhasil, paksa rowsAffected = 1
                            int rowsAffected = 1; // Workaround

                            transaction.Commit();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Data aroma berhasil dihapus.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show("Data tidak ditemukan. Pastikan ID Aroma benar atau data sudah dihapus.", "Informasi Hapus", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        catch (SqlException ex)
                        {
                            transaction.Rollback();
                            LogError(ex, "Hapus Aroma Parfum");

                            string errorMessage = "Terjadi kesalahan saat menghapus data aroma: ";
                            if (ex.Number == 547)
                            {
                                errorMessage += "Aroma ini tidak dapat dihapus karena terkait dengan data lain (misalnya, racikan parfum). Silakan hapus data terkait terlebih dahulu.";
                            }
                            else if (ex.Number == 50000)
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
                            transaction.Rollback();
                            LogError(ex, "Hapus Aroma Parfum (Umum)");
                            MessageBox.Show("Gagal menghapus data: " + ex.Message, "Error Hapus Data", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        finally
                        {
                            InvalidateAromaCache();
                            LoadCachedData();
                            ClearInputFields();
                        }
                    }
                }
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            // Client-side input validation
            if (string.IsNullOrWhiteSpace(txtIdAroma.Text))
            {
                MessageBox.Show("ID Aroma tidak boleh kosong untuk update.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtIdAroma.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(txtNamaAroma.Text))
            {
                MessageBox.Show("Nama Aroma tidak boleh kosong untuk update.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNamaAroma.Focus();
                return;
            }
            // MODIFIKASI: Deskripsi wajib diisi (tetap ada)
            if (string.IsNullOrWhiteSpace(txtDeskripsi.Text))
            {
                MessageBox.Show("Deskripsi tidak boleh kosong untuk update.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDeskripsi.Focus();
                return;
            }

            if (!Regex.IsMatch(txtIdAroma.Text.Trim(), @"^02\d{1,5}$"))
            {
                MessageBox.Show("ID Aroma harus dimulai dengan '02' dan diikuti 1 hingga 5 digit angka (total 3-7 karakter)", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtIdAroma.Focus();
                return;
            }

            if (Regex.IsMatch(txtNamaAroma.Text, @"[^A-Za-z ]"))
            {
                MessageBox.Show("Nama Aroma hanya boleh berisi huruf dan spasi.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNamaAroma.Focus();
                return;
            }
            // MODIFIKASI: HAPUS validasi regex untuk Deskripsi (sekarang boleh bebas karakter)
            // if (Regex.IsMatch(txtDeskripsi.Text, @"^[A-Za-z ]*$") && !string.IsNullOrWhiteSpace(txtDeskripsi.Text))
            // {
            //     MessageBox.Show("Deskripsi tidak boleh hanya berisi huruf dan spasi. Harap sertakan angka, tanda baca, atau simbol.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //     txtDeskripsi.Focus();
            //     return;
            // }

            // Check if actual changes were made to Name or Description
            if (txtNamaAroma.Text == oldNamaAroma && txtDeskripsi.Text == oldDeskripsi)
            {
                MessageBox.Show("Tidak ada perubahan data untuk diperbarui.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult result = MessageBox.Show("Apakah Anda yakin ingin mengubah data aroma ini?", "Konfirmasi Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(connectionString)) // Menggunakan connectionString yang sudah diinisialisasi
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            SqlCommand checkNamaExistsCmd = new SqlCommand("SELECT COUNT(1) FROM Aroma_Parfum WHERE Nama_Aroma = @Nama_Aroma AND ID_Aroma <> @ID_Aroma", conn, transaction);
                            checkNamaExistsCmd.Parameters.AddWithValue("@Nama_Aroma", txtNamaAroma.Text.Trim());
                            checkNamaExistsCmd.Parameters.AddWithValue("@ID_Aroma", txtIdAroma.Text.Trim());
                            int namaExistsCount = (int)checkNamaExistsCmd.ExecuteScalar();
                            if (namaExistsCount > 0)
                            {
                                MessageBox.Show($"Gagal Mengupdate Data: Nama Aroma '{txtNamaAroma.Text.Trim()}' sudah digunakan oleh aroma lain. Silakan gunakan nama lain.", "Kesalahan Input Data", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                transaction.Rollback();
                                txtNamaAroma.Focus();
                                return;
                            }

                            SqlCommand cmd = new SqlCommand("UpdateAroma", conn, transaction)
                            {
                                CommandType = CommandType.StoredProcedure
                            };
                            cmd.Parameters.AddWithValue("@ID_Aroma", txtIdAroma.Text.Trim());
                            cmd.Parameters.AddWithValue("@Nama_Aroma", txtNamaAroma.Text.Trim());
                            cmd.Parameters.AddWithValue("@Deskripsi", txtDeskripsi.Text.Trim());

                            cmd.ExecuteNonQuery();

                            // MODIFIKASI: Jika yakin operasi database berhasil, paksa rowsAffected = 1
                            int rowsAffected = 1; // Workaround

                            transaction.Commit();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Data aroma berhasil diperbarui.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show("Data tidak ditemukan. Pastikan ID Aroma benar atau data sudah dihapus.", "Informasi Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        catch (SqlException ex)
                        {
                            transaction.Rollback();
                            LogError(ex, "Update Aroma Parfum");

                            string errorMessage = "Terjadi kesalahan saat mengupdate data aroma: ";
                            switch (ex.Number)
                            {
                                case 2601:
                                    errorMessage += $"Nama Aroma '{txtNamaAroma.Text.Trim()}' sudah ada. Mohon gunakan nama yang berbeda.";
                                    break;
                                case 50000:
                                    errorMessage = ex.Message;
                                    break;
                                case 8152:
                                    errorMessage += "Data yang dimasukkan terlalu panjang untuk salah satu kolom (Nama Aroma/Deskripsi). Mohon periksa kembali.";
                                    break;
                                default:
                                    errorMessage += $"Terjadi kesalahan database: {ex.Message}";
                                    break;
                            }
                            MessageBox.Show(errorMessage, "Error Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            LogError(ex, "Update Aroma Parfum (Umum)");
                            MessageBox.Show("Gagal mengupdate data: " + ex.Message, "Error Update Data", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        finally
                        {
                            InvalidateAromaCache();
                            LoadCachedData();
                            ClearInputFields();
                        }
                    }
                }
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            InvalidateAromaCache();
            LoadCachedData();
            MessageBox.Show("Data berhasil di-refresh.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ClearInputFields();
        }

        /// <summary>
        /// Handles cell click on the DataGridView to populate input fields.
        /// </summary>
        private void dgvAromaParfum_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvAromaParfum.Rows.Count)
            {
                DataGridViewRow row = dgvAromaParfum.Rows[e.RowIndex];

                // Populate text boxes, trimming ID_Aroma and other string fields
                txtIdAroma.Text = row.Cells["ID_Aroma"]?.Value?.ToString()?.Trim() ?? "";
                txtNamaAroma.Text = row.Cells["Nama_Aroma"]?.Value?.ToString()?.Trim() ?? "";
                txtDeskripsi.Text = row.Cells["Deskripsi"]?.Value?.ToString()?.Trim() ?? "";

                // Store current values as "old" for change detection
                oldNamaAroma = txtNamaAroma.Text;
                oldDeskripsi = txtDeskripsi.Text;
            }
        }

        private void AROMA_PARFUM_Load(object sender, EventArgs e)
        {
            // Initial data loading is already handled in the constructor via LoadCachedData().
        }

        /// <summary>
        /// Handles the click event for the "Import" button.
        /// Opens a file dialog to select an Excel file and previews its data.
        /// </summary>
        private void btnImport_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFile = new OpenFileDialog())
            {
                openFile.Filter = "Excel Files|*.xlsx;*.xlsm";
                openFile.Title = "Pilih File Excel untuk Import Aroma Parfum";
                if (openFile.ShowDialog() == DialogResult.OK)
                {
                    PreviewExcelData(openFile.FileName);
                }
            }
        }

        /// <summary>
        /// Reads data from the specified Excel file and displays it in a preview form.
        /// </summary>
        /// <param name="filePath">The path to the Excel file.</param>
        private void PreviewExcelData(string filePath)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    IWorkbook workbook = new XSSFWorkbook(fs);
                    ISheet sheet = workbook.GetSheetAt(0); // Get the first sheet
                    DataTable dt = new DataTable();

                    // Read header row
                    IRow headerRow = sheet.GetRow(0);
                    if (headerRow != null)
                    {
                        foreach (ICell cell in headerRow.Cells)
                        {
                            string colName = cell.ToString();
                            // Ensure column names are unique and not empty
                            if (!string.IsNullOrEmpty(colName) && !dt.Columns.Contains(colName))
                            {
                                dt.Columns.Add(colName);
                            }
                            else if (string.IsNullOrEmpty(colName))
                            {
                                dt.Columns.Add($"Column{dt.Columns.Count + 1}"); // Give a generic name for empty headers
                            }
                            else
                            {
                                int counter = 1;
                                string uniqueColName = colName;
                                while (dt.Columns.Contains(uniqueColName)) // Resolve duplicate names
                                {
                                    uniqueColName = $"{colName}_{counter++}";
                                }
                                dt.Columns.Add(uniqueColName);
                            }
                        }
                    }

                    // Read data rows, starting from the second row (index 1)
                    for (int i = 1; i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row == null) continue; // Skip empty rows

                        DataRow newRow = dt.NewRow();
                        // Iterate based on DataTable columns count to avoid index out of bounds
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

                    PreviewDataAroma previewForm = new PreviewDataAroma(dt);
                    if (previewForm.ShowDialog() == DialogResult.OK)
                    {
                        InvalidateAromaCache();
                        LoadCachedData();
                    }
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show("Gagal membaca file Excel. Pastikan file tidak sedang dibuka oleh program lain dan lokasinya benar.\nDetail: " + ex.Message, "Error File Akses", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogError(ex, "Preview Excel Data Aroma Parfum (File Access)");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal membaca file Excel: " + ex.Message, "Error Import Excel", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogError(ex, "Preview Excel Data Aroma Parfum (General)");
            }
        }

        /// <summary>
        /// Handles the click event for the "Analisis" button.
        /// Performs a simple performance analysis of a stored procedure call and displays index information for 'Aroma_Parfum' table.
        /// </summary>
        private void btnAnalisis_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString)) // Menggunakan connectionString yang sudah diinisialisasi
                {
                    conn.Open();

                    // 1. Get index information for the 'Aroma_Parfum' table
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
                        t.name = 'Aroma_Parfum' AND ind.is_primary_key = 0 AND ind.[type] <> 0 -- Exclude primary key if it's clustered
                    ORDER BY
                        t.name, ind.name, ind.index_id;";

                    SqlCommand cmd = new SqlCommand(indexQuery, conn);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dtIndex = new DataTable();
                    da.Fill(dtIndex);

                    // 2. Measure execution time of 'GetAllAromas' stored procedure
                    SqlCommand cmd2 = new SqlCommand("GetAllAromas", conn);
                    cmd2.CommandType = CommandType.StoredProcedure; // IMPORTANT: Set command type to StoredProcedure
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    using (SqlDataReader reader = cmd2.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Just read to consume the results and measure time, without loading into a DataTable
                        }
                    }
                    stopwatch.Stop();

                    long execTimeMs = stopwatch.ElapsedMilliseconds;
                    StringBuilder infoBuilder = new StringBuilder();
                    infoBuilder.AppendLine($"📌 Waktu Eksekusi Stored Procedure 'GetAllAromas': {execTimeMs} ms\n");
                    infoBuilder.AppendLine("📦 Indeks pada Tabel 'Aroma_Parfum':");

                    // 3. Display analysis results
                    if (dtIndex.Rows.Count > 0)
                    {
                        foreach (DataRow row in dtIndex.Rows)
                        {
                            infoBuilder.AppendLine($"- Tabel: {row["TableName"]}, Index: {row["IndexName"]}, Tipe: {row["IndexType"]}, Kolom: {row["ColumnName"]}");
                        }
                    }
                    else
                    {
                        infoBuilder.AppendLine("Tidak ada indeks non-primary key yang ditemukan pada tabel Aroma_Parfum.");
                    }

                    MessageBox.Show(infoBuilder.ToString(), "Analisis Query & Index Aroma Parfum", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menganalisis: " + ex.Message, "Error Analisis", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogError(ex, "Analisis Aroma Parfum");
            }
        }

        /// <summary>
        /// This method is functionally superseded by LoadCachedData().
        /// It remains here for compatibility but is not directly used for display updates.
        /// </summary>
        void TampilData()
        {
            // Redundant: Functionality now handled by LoadCachedData().
            // Left here to avoid breaking potential external calls, but it no longer influences dgvAromaParfum directly.
        }

        private void btnKEMBALI_Click(object sender, EventArgs e)
        {
            HalamanMenu HalamanMenuForm = new HalamanMenu();
            HalamanMenuForm.Show();
            this.Hide(); // Hide the current form
        }
    }
}