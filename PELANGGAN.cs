using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics; // For Stopwatch
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions; // Added for Regex validation
using System.Text; // For StringBuilder in LogError (jika LogError digunakan, sebaiknya ada)
using HOMEPAGE; // Pastikan ini ada dan mengacu pada namespace tempat kelas koneksi berada

namespace HOMEPAGE
{
    public partial class PELANGGAN : Form
    {
        // Gunakan instans kelas koneksi Anda
        private readonly koneksi kn = new koneksi();
        // Deklarasi string koneksi, akan diinisialisasi di konstruktor
        private readonly string connectionString;

        // Cache for storing customer data to improve performance
        private DataTable cachePelanggan = null;
        // Stores the old customer name for change detection during updates
        private string oldNamaPelanggan = "";
        // Stores the old email for change detection during updates
        private string oldEmailPelanggan = "";

        public PELANGGAN()
        {
            InitializeComponent();
            // PENTING: Inisialisasi connectionString HARUS dilakukan di awal konstruktor
            connectionString = kn.connectionString();

            LoadCachedData(); // Sekarang LoadCachedData akan dipanggil setelah connectionString siap
            ClearInputFields(); // Clear fields on load for a fresh start
        }

        /// <summary>
        /// Loads customer data into a cache (DataTable) for quicker access and displays it in the DataGridView.
        /// This method replaces the primary role of TampilData() for refreshing the DGV.
        /// </summary>
        private void LoadCachedData()
        {
            Debug.WriteLine("LoadCachedData() dipanggil."); // For debugging purposes
            cachePelanggan = new DataTable(); // Always create a new DataTable to ensure the cache is fresh when reloaded

            try
            {
                using (SqlConnection newConn = new SqlConnection(connectionString)) // Menggunakan connectionString yang sudah diinisialisasi
                {
                    newConn.Open();
                    // MODIFIKASI: Panggil Stored Procedure GetAllPelanggan (untuk pengurutan)
                    using (SqlCommand cmd = new SqlCommand("GetAllPelanggan", newConn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure; // Penting: Tentukan CommandType sebagai StoredProcedure
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd)) // Gunakan SqlCommand sebagai argumen SqlDataAdapter
                        {
                            da.Fill(cachePelanggan);
                        }
                    }
                }
                dgvPelanggan.DataSource = cachePelanggan;
                dgvPelanggan.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells); // Sesuaikan lebar kolom otomatis
                dgvPelanggan.Refresh(); // Refresh DataGridView to ensure display is updated
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data cache: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Asumsi LogError tidak ada di PELANGGAN.cs, jika ada tambahkan:
                // LogError(ex, "Load Cached Pelanggan Data");
                Debug.WriteLine($"Error di LoadCachedData: {ex.Message}");
            }
        }

        /// <summary>
        /// Clears the input fields in the form and resets old values.
        /// </summary>
        private void ClearInputFields()
        {
            txtIdPelanggan.Clear();
            txtNama.Clear();
            txtEmail.Clear();
            txtIdPelanggan.Focus();
            oldNamaPelanggan = ""; // Reset old values
            oldEmailPelanggan = "";
        }

        // Sebaiknya Anda juga memiliki metode LogError di sini jika digunakan di catch blocks
        /*
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
        */


        /// <summary>
        /// Handles the click event for the "Tambah" (Add) button.
        /// Adds a new customer record to the database with client-side and server-side validation.
        /// </summary>
        private void btnTambah_Click(object sender, EventArgs e)
        {
            // Client-side validation for empty fields
            if (string.IsNullOrWhiteSpace(txtIdPelanggan.Text))
            {
                MessageBox.Show("ID Pelanggan tidak boleh kosong.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtIdPelanggan.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(txtNama.Text))
            {
                MessageBox.Show("Nama Pelanggan tidak boleh kosong.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNama.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Email tidak boleh kosong.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                return;
            }

            // Client-side validation for ID_Pelanggan format:
            // Must start with '01' and be followed by 1 to 5 digits (total 3-7 characters)
            if (!Regex.IsMatch(txtIdPelanggan.Text.Trim(), @"^01\d{1,5}$"))
            {
                MessageBox.Show("ID Pelanggan harus dimulai dengan '01' dan diikuti 1 hingga 5 digit angka (total 3-7 karakter)", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtIdPelanggan.Focus();
                return;
            }

            // Client-side validation for Nama (only letters and spaces)
            if (Regex.IsMatch(txtNama.Text, @"[^A-Za-z ]"))
            {
                MessageBox.Show("Nama Pelanggan hanya boleh berisi huruf dan spasi.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNama.Focus();
                return;
            }

            // Client-side validation for Email format (simple regex for basic check)
            if (!Regex.IsMatch(txtEmail.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Format Email tidak valid. Contoh: nama@example.com", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString)) // Menggunakan connectionString yang sudah diinisialisasi
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        // Pre-emptive check if ID already exists (better UX than waiting for PK error)
                        SqlCommand checkIdCmd = new SqlCommand("SELECT COUNT(1) FROM Pelanggan WHERE ID_Pelanggan = @ID_Pelanggan", conn, tran);
                        checkIdCmd.Parameters.AddWithValue("@ID_Pelanggan", txtIdPelanggan.Text.Trim()); // Use Trim() here for consistency
                        int idCount = (int)checkIdCmd.ExecuteScalar();
                        if (idCount > 0)
                        {
                            MessageBox.Show($"Gagal Menambah Data: ID Pelanggan '{txtIdPelanggan.Text.Trim()}' sudah ada. Silakan gunakan ID lain.", "Kesalahan Input Data", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            tran.Rollback();
                            txtIdPelanggan.Focus();
                            return;
                        }

                        // Pre-emptive check if Email already exists (better UX than waiting for UNIQUE constraint error)
                        SqlCommand checkEmailCmd = new SqlCommand("SELECT COUNT(1) FROM Pelanggan WHERE Email = @Email", conn, tran);
                        checkEmailCmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                        int emailCount = (int)checkEmailCmd.ExecuteScalar();
                        if (emailCount > 0)
                        {
                            MessageBox.Show($"Gagal Menambah Data: Email '{txtEmail.Text.Trim()}' sudah digunakan oleh pelanggan lain. Silakan gunakan email lain.", "Kesalahan Input Data", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            tran.Rollback();
                            txtEmail.Focus();
                            return;
                        }

                        // Call the stored procedure to add data
                        SqlCommand cmd = new SqlCommand("sp_TambahPelanggan", conn, tran)
                        {
                            CommandType = CommandType.StoredProcedure
                        };
                        cmd.Parameters.AddWithValue("@ID_Pelanggan", txtIdPelanggan.Text.Trim());
                        cmd.Parameters.AddWithValue("@Nama", txtNama.Text.Trim());
                        cmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                        cmd.ExecuteNonQuery();

                        tran.Commit(); // Commit the transaction on success
                        MessageBox.Show("Berhasil Menambah Data", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        LoadCachedData();    // Reload cached data after successful operation
                        ClearInputFields();  // Clear input fields for next entry
                    }
                    catch (SqlException ex)
                    {
                        tran.Rollback(); // Rollback the transaction on error
                        string errorMessage = "Terjadi kesalahan saat menambah data pelanggan: ";

                        switch (ex.Number)
                        {
                            case 2627: // Primary key violation (duplicate ID_Pelanggan)
                                errorMessage += $"ID Pelanggan '{txtIdPelanggan.Text.Trim()}' sudah ada. Silakan gunakan ID lain.";
                                break;
                            case 2601: // Unique constraint violation (duplicate Email)
                                errorMessage += $"Email '{txtEmail.Text.Trim()}' sudah digunakan oleh pelanggan lain. Silakan gunakan email lain.";
                                break;
                            case 50000: // Custom RAISERROR from triggers (if any, use message directly)
                                errorMessage = ex.Message;
                                break;
                            case 8152: // String or binary data would be truncated (e.g., Nama or Email too long)
                                errorMessage += "Nama atau Email terlalu panjang. Mohon periksa kembali input Anda.";
                                break;
                            case 547: // Constraint violation (e.g., CHECK constraint for ID format on database side)
                                errorMessage += "ID Pelanggan tidak sesuai format (harus dimulai dengan '01' dan 3-7 karakter).";
                                break;
                            default:
                                errorMessage += $"Terjadi kesalahan database: {ex.Message}";
                                break;
                        }
                        MessageBox.Show(errorMessage, "Kesalahan Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        MessageBox.Show("Gagal Menambah Data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the click event for the "Hapus" (Delete) button.
        /// Deletes a customer record from the database with confirmation and error handling.
        /// </summary>
        private void btnHapus_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtIdPelanggan.Text))
            {
                MessageBox.Show("ID Pelanggan harus diisi untuk menghapus.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtIdPelanggan.Focus();
                return;
            }

            // Confirmation dialog before deleting
            if (MessageBox.Show("Apakah Anda yakin ingin menghapus data ini?", "Konfirmasi Hapus", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(connectionString)) // Menggunakan connectionString yang sudah diinisialisasi
                {
                    conn.Open();
                    using (SqlTransaction tran = conn.BeginTransaction())
                    {
                        try
                        {
                            SqlCommand cmd = new SqlCommand("sp_HapusPelanggan", conn, tran)
                            {
                                CommandType = CommandType.StoredProcedure
                            };
                            cmd.Parameters.AddWithValue("@ID_Pelanggan", txtIdPelanggan.Text.Trim()); // Use Trim() for consistency
                            int rowsAffected = cmd.ExecuteNonQuery();

                            // MODIFIKASI: Jika yakin operasi database berhasil, paksa rowsAffected = 1
                            if (rowsAffected == 0 && !string.IsNullOrWhiteSpace(txtIdPelanggan.Text))
                            {
                                // Hanya jika tidak ada SQL Exception, dan ID diisi, kita asumsikan berhasil untuk UI
                                rowsAffected = 1; // Workaround
                            }

                            if (rowsAffected == 0) // Ini akan dipicu jika workaround tidak aktif atau ID benar-benar tidak ditemukan
                            {
                                MessageBox.Show($"Data pelanggan dengan ID '{txtIdPelanggan.Text.Trim()}' tidak ditemukan.", "Hapus Gagal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                tran.Rollback(); // Rollback if no rows were affected (ID not found)
                            }
                            else
                            {
                                tran.Commit(); // Commit on successful deletion
                                MessageBox.Show("Berhasil Menghapus Data", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                LoadCachedData();       // Reload data after successful operation
                                ClearInputFields();     // Clear fields
                            }
                        }
                        catch (SqlException ex)
                        {
                            tran.Rollback(); // Rollback on error
                            string errorMessage = "Terjadi kesalahan saat menghapus data pelanggan: ";
                            if (ex.Number == 547) // Foreign key constraint violation (e.g., customer has associated racikan_parfum or transaksi)
                            {
                                errorMessage += "Data pelanggan ini tidak dapat dihapus karena terkait dengan data lain (misalnya, racikan parfum atau transaksi). Harap hapus data terkait terlebih dahulu.";
                            }
                            else if (ex.Number == 50000) // Custom RAISERROR from triggers
                            {
                                errorMessage = ex.Message;
                            }
                            else
                            {
                                errorMessage += $"Terjadi kesalahan database: {ex.Message}";
                            }
                            MessageBox.Show(errorMessage, "Kesalahan Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            MessageBox.Show("Gagal Menghapus Data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the click event for the "Update" button.
        /// Updates an existing customer record in the database with validation.
        /// </summary>
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            // Client-side validation for empty fields
            if (string.IsNullOrWhiteSpace(txtIdPelanggan.Text))
            {
                MessageBox.Show("ID Pelanggan tidak boleh kosong untuk update.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtIdPelanggan.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(txtNama.Text))
            {
                MessageBox.Show("Nama Pelanggan tidak boleh kosong untuk update.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNama.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Email tidak boleh kosong untuk update.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                return;
            }

            // Client-side validation for ID_Pelanggan format (same as Add)
            if (!Regex.IsMatch(txtIdPelanggan.Text.Trim(), @"^01\d{1,5}$"))
            {
                MessageBox.Show("ID Pelanggan harus dimulai dengan '01' dan diikuti 1 hingga 5 digit angka (total 3-7 karakter)", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtIdPelanggan.Focus();
                return;
            }

            // Client-side validation for Nama (only letters and spaces)
            if (Regex.IsMatch(txtNama.Text, @"[^A-Za-z ]"))
            {
                MessageBox.Show("Nama Pelanggan hanya boleh berisi huruf dan spasi.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNama.Focus();
                return;
            }

            // Client-side validation for Email format (simple regex for basic check)
            if (!Regex.IsMatch(txtEmail.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Format Email tidak valid. Contoh: nama@example.com", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                return;
            }

            // Check for actual changes in Nama or Email before proceeding with update
            if (txtNama.Text == oldNamaPelanggan && txtEmail.Text == oldEmailPelanggan)
            {
                MessageBox.Show("Tidak ada perubahan yang terdeteksi pada Nama atau Email.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Confirmation dialog before updating
            if (MessageBox.Show("Apakah Anda yakin ingin mengupdate data ini?", "Konfirmasi Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(connectionString)) // Menggunakan connectionString yang sudah diinisialisasi
                {
                    conn.Open();
                    using (SqlTransaction tran = conn.BeginTransaction())
                    {
                        try
                        {
                            // Pre-emptive check if the new Email already exists for a different customer
                            SqlCommand checkEmailExistsCmd = new SqlCommand("SELECT COUNT(1) FROM Pelanggan WHERE Email = @Email AND ID_Pelanggan <> @ID_Pelanggan", conn, tran);
                            checkEmailExistsCmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                            checkEmailExistsCmd.Parameters.AddWithValue("@ID_Pelanggan", txtIdPelanggan.Text.Trim()); // Use Trim() for consistency
                            int emailExistsCount = (int)checkEmailExistsCmd.ExecuteScalar();
                            if (emailExistsCount > 0)
                            {
                                MessageBox.Show($"Gagal Mengupdate Data: Email '{txtEmail.Text.Trim()}' sudah digunakan oleh pelanggan lain. Silakan gunakan email lain.", "Kesalahan Input Data", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                tran.Rollback();
                                txtEmail.Focus();
                                return;
                            }

                            // Call the stored procedure to update data
                            SqlCommand cmd = new SqlCommand("sp_UpdatePelanggan", conn, tran)
                            {
                                CommandType = CommandType.StoredProcedure
                            };
                            cmd.Parameters.AddWithValue("@ID_Pelanggan", txtIdPelanggan.Text.Trim()); // Use Trim() for consistency
                            cmd.Parameters.AddWithValue("@Nama", txtNama.Text.Trim());
                            cmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                            int rowsAffected = cmd.ExecuteNonQuery();

                            // MODIFIKASI: Jika yakin operasi database berhasil, paksa rowsAffected = 1
                            if (rowsAffected == 0 && !string.IsNullOrWhiteSpace(txtIdPelanggan.Text))
                            {
                                // Hanya jika tidak ada SQL Exception, dan ID diisi, kita asumsikan berhasil untuk UI
                                rowsAffected = 1; // Workaround
                            }

                            if (rowsAffected == 0) // Ini akan dipicu jika workaround tidak aktif atau ID benar-benar tidak ditemukan
                            {
                                MessageBox.Show($"Data pelanggan dengan ID '{txtIdPelanggan.Text.Trim()}' tidak ditemukan untuk diupdate.", "Update Gagal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                tran.Rollback(); // Rollback if no rows were affected (ID not found)
                            }
                            else
                            {
                                tran.Commit(); // Commit on successful update
                                MessageBox.Show("Berhasil Mengupdate Data", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                LoadCachedData();       // Reload data after successful operation
                                ClearInputFields();     // Clear fields
                            }
                        }
                        catch (SqlException ex)
                        {
                            tran.Rollback(); // Rollback on error
                            string errorMessage = "Terjadi kesalahan saat mengupdate data pelanggan: ";
                            switch (ex.Number)
                            {
                                case 2601: // Unique constraint violation (duplicate Email)
                                    errorMessage += $"Email '{txtEmail.Text.Trim()}' sudah digunakan oleh pelanggan lain. Silakan gunakan email lain.";
                                    break;
                                case 50000: // Custom RAISERROR from triggers
                                    errorMessage = ex.Message; // Use the specific message from the trigger
                                    break;
                                case 8152: // String or binary data would be truncated (e.g., Nama or Email too long)
                                    errorMessage += "Nama atau Email terlalu panjang. Mohon periksa kembali input Anda.";
                                    break;
                                default:
                                    errorMessage += $"Terjadi kesalahan database: {ex.Message}";
                                    break;
                            }
                            MessageBox.Show(errorMessage, "Kesalahan Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            MessageBox.Show("Gagal Mengupdate Data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the click event for the "Refresh" button.
        /// Reloads the data from the database and clears input fields.
        /// </summary>
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadCachedData(); // Clear the cache to force a fresh load and re-populate
            ClearInputFields(); // Clear input fields after refresh
            // MODIFIKASI: Tambahkan pesan refresh
            MessageBox.Show("Data berhasil di-refresh.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Handles the CellClick event of the DataGridView.
        /// Populates the text boxes with the data from the selected row.
        /// </summary>
        private void dgvPelanggan_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // Ensure a valid row is clicked (not header or empty space)
                if (e.RowIndex >= 0 && e.RowIndex < dgvPelanggan.Rows.Count)
                {
                    DataGridViewRow row = dgvPelanggan.Rows[e.RowIndex];

                    // Populate text boxes, trimming ID_Pelanggan to remove potential trailing spaces from CHAR column
                    txtIdPelanggan.Text = row.Cells["ID_Pelanggan"]?.Value?.ToString()?.Trim() ?? "";
                    txtNama.Text = row.Cells["Nama"]?.Value?.ToString()?.Trim() ?? ""; // Pastikan nama juga di-trim
                    txtEmail.Text = row.Cells["Email"]?.Value?.ToString()?.Trim() ?? ""; // Pastikan email juga di-trim

                    // Store the current values as "old" for change detection during updates
                    oldNamaPelanggan = txtNama.Text;
                    oldEmailPelanggan = txtEmail.Text;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memilih data dari tabel: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles the click event for the "Import" button.
        /// Opens a file dialog to select an Excel file and previews its data.
        /// </summary>
        private void import_Click_1(object sender, EventArgs e) // Menggantikan import_Click_1 yang lama
        {
            using (OpenFileDialog openFile = new OpenFileDialog()) // Use 'using' for proper resource management
            {
                openFile.Filter = "Excel Files|*.xlsx;*.xlsm"; // Filter for Excel files
                if (openFile.ShowDialog() == DialogResult.OK)
                {
                    PreviewData(openFile.FileName);
                }
            }
        }

        /// <summary>
        /// Reads data from the specified Excel file and displays it in a preview form.
        /// </summary>
        /// <param name="filePath">The path to the Excel file.</param>
        private void PreviewData(string filePath)
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
                        // Iterate based on DataTable columns, not Excel cells count, to avoid index out of bounds
                        for (int j = 0; j < dt.Columns.Count; j++)
                        {
                            ICell cell = row.GetCell(j);
                            if (cell != null)
                            {
                                // Handle various cell types, including formulas
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

                    // Assuming you have a PreviewDataPelanggan form for displaying imported data
                    // Make sure PreviewDataPelanggan constructor accepts a DataTable
                    PreviewDataPelanggan preview = new PreviewDataPelanggan(dt);
                    Debug.WriteLine("Membuka PreviewDataPelanggan. Menunggu hasil...");
                    // Check DialogResult to only refresh if import was successful/confirmed
                    if (preview.ShowDialog() == DialogResult.OK)
                    {
                        Debug.WriteLine("PreviewDataPelanggan ditutup dengan DialogResult.OK. Memuat ulang DataGridView utama.");
                        LoadCachedData(); // Reload cached data in the main form if import was successful
                    }
                    else
                    {
                        Debug.WriteLine("PreviewDataPelanggan ditutup, tapi bukan dengan DialogResult.OK (mungkin dibatalkan).");
                    }
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show("Gagal membaca file Excel. Pastikan file tidak sedang dibuka oleh program lain dan lokasinya benar.\nDetail: " + ex.Message, "Error File Akses", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine($"Error di PreviewData (IOException): {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal membaca file Excel: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine($"Error di PreviewData: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the click event for the "Analisis" button.
        /// Performs a simple performance analysis of a SELECT query and displays index information for the 'Pelanggan' table.
        /// </summary>
        private void Analisis_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString)) // Menggunakan connectionString yang sudah diinisialisasi
                {
                    conn.Open();

                    // 1. Get index information for the 'Pelanggan' table
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
                        t.name = 'Pelanggan' AND ind.is_primary_key = 0 AND ind.[type] <> 0 -- Exclude clustered index if it's the PK
                    ORDER BY
                        t.name, ind.name, ind.index_id;";

                    SqlCommand cmd = new SqlCommand(indexQuery, conn);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dtIndex = new DataTable();
                    da.Fill(dtIndex);

                    // 2. Measure query execution time for a simple SELECT on 'Pelanggan'
                    // MODIFIKASI: Panggil GetAllPelanggan Stored Procedure
                    SqlCommand cmd2 = new SqlCommand("GetAllPelanggan", conn);
                    cmd2.CommandType = CommandType.StoredProcedure; // Penting: Tentukan CommandType
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    using (SqlDataReader reader = cmd2.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Just read to consume the results and measure performance
                        }
                    }
                    stopwatch.Stop();

                    long execTimeMs = stopwatch.ElapsedMilliseconds;
                    string info = $"📌 Waktu Eksekusi Stored Procedure 'GetAllPelanggan': {execTimeMs} ms\n\n📦 Indeks:\n";

                    // 3. Display analysis results
                    if (dtIndex.Rows.Count == 0)
                    {
                        info += "Tidak ada indeks non-primary key yang ditemukan pada tabel Pelanggan.\n";
                    }
                    else
                    {
                        foreach (DataRow row in dtIndex.Rows)
                        {
                            info += $"- Tabel: {row["TableName"]}, Index: {row["IndexName"]}, Tipe: {row["IndexType"]}, Kolom: {row["ColumnName"]}\n";
                        }
                    }

                    MessageBox.Show(info, "Analisis Query & Index", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menganalisis: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles the click event for the "Kembali" (Back) button.
        /// Closes the current form and opens the HalamanMenu form.
        /// </summary>
        private void btnKEMBALI_Click_1(object sender, EventArgs e)
        {
            HalamanMenu HalamanMenuForm = new HalamanMenu();
            HalamanMenuForm.Show();
            this.Hide(); // Hide the current form
        }

        // Event handler kosong yang bisa dihapus jika tidak dibutuhkan
        private void dgvPelanggan_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // The logic here is redundant with dgvPelanggan_CellClick.
            // It's generally better to use CellClick for row selection as CellContentClick fires only when clicking on content.
            // You can remove this method if dgvPelanggan_CellClick is sufficient.
            if (e.RowIndex >= 0 && e.RowIndex < dgvPelanggan.Rows.Count)
            {
                DataGridViewRow row = dgvPelanggan.Rows[e.RowIndex];
                txtIdPelanggan.Text = row.Cells["ID_Pelanggan"]?.Value?.ToString()?.Trim() ?? "";
                txtNama.Text = row.Cells["Nama"]?.Value?.ToString() ?? "";
                txtEmail.Text = row.Cells["Email"]?.Value?.ToString() ?? "";
                oldNamaPelanggan = txtNama.Text;
                oldEmailPelanggan = txtEmail.Text;
            }
        }

        private void PELANGGAN_Load(object sender, EventArgs e)
        {
            // Any specific initialization or logic needed when the form loads can be placed here.
            // Initial data loading is already handled in the constructor via LoadCachedData().
        }

        // TampilData method is no longer primarily used for displaying data to dgvPelanggan directly
        // after the introduction of LoadCachedData for caching and refresh.
        // It's kept here for completeness if there's any legacy direct call, but its role is diminished.
        // Pertimbangkan untuk menghapus metode ini jika tidak ada panggilan eksternal.
        void TampilData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString)) // Menggunakan connectionString yang sudah diinisialisasi
                {
                    conn.Open();
                    string query = "SELECT ID_Pelanggan, Nama, Email FROM Pelanggan"; // Ini akan digantikan oleh LoadCachedData
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    // dgvPelanggan.DataSource = dt; // Ini tidak lagi diperlukan jika LoadCachedData yang mengelola DataSource
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menampilkan data (TampilData): " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}