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

namespace HOMEPAGE
{
    public partial class RACIKAN_PARFUM : Form
    {
        private readonly string connectionString = "Data Source=LAPTOP-T1UUTAE0\\HIKMATYAR;Initial Catalog=Manajemen_Penjualan_Parfum;Integrated Security=True";

        // Cache for storing Racikan_Parfum data to improve performance
        private DataTable cacheRacikanParfum = null;
        // Stores old values for change detection during updates
        private string oldIdRacikan = "";
        private string oldIdPelanggan = "";
        private string oldIdAroma = "";
        private string oldIdPegawai = "";
        private string oldPerbandingan = "";
        private string oldNamaRacikan = "";
        private string oldNamaHasilParfum = "";

        public RACIKAN_PARFUM()
        {
            InitializeComponent();
            LoadInitialData(); // Initial data display and cache loading
            LoadComboBoxes();  // Load data for ComboBoxes
            ClearInputFields(); // Clear fields on start
        }

        /// <summary>
        /// Loads initial data into DataGridView and populates the cache.
        /// </summary>
        private void LoadInitialData()
        {
            try
            {
                // Ensure DataGridView columns are reset before loading new data
                // This prevents issues if the schema changes or data is reloaded multiple times
                dgvRacikanParfum.DataSource = null; // Clear current binding
                dgvRacikanParfum.Columns.Clear();   // Clear existing columns

                // If cache is null, load from DB. Otherwise, use cached data.
                if (cacheRacikanParfum == null)
                {
                    cacheRacikanParfum = new DataTable();
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM Racikan_Parfum", conn))
                        {
                            adapter.Fill(cacheRacikanParfum);
                        }
                    }
                }
                dgvRacikanParfum.DataSource = cacheRacikanParfum;

                // Auto-size columns for better readability
                dgvRacikanParfum.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data awal: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogError(ex, "Load Initial Racikan Data");
            }
        }

        /// <summary>
        /// Invalidates the Racikan_Parfum data cache.
        /// This should be called whenever data in the database changes (add, update, delete).
        /// </summary>
        private void InvalidateRacikanCache()
        {
            cacheRacikanParfum = null;
        }

        /// <summary>
        /// Clears all input fields and resets old values for change detection.
        /// </summary>
        private void ClearInputFields()
        {
            txtIdRacikan.Clear();
            cmbIdPelanggan.SelectedIndex = -1; // Deselect
            cmbIdAroma.SelectedIndex = -1;
            cmbIdPegawai.SelectedIndex = -1;
            cmbPerbandingan.SelectedIndex = -1;
            cmbNamaAroma.Text = ""; // Clear text for editable combobox
            txtNamaHasilParfum.Clear();

            txtIdRacikan.Focus(); // Set focus to the ID field

            // Reset old values
            oldIdRacikan = "";
            oldIdPelanggan = "";
            oldIdAroma = "";
            oldIdPegawai = "";
            oldPerbandingan = "";
            oldNamaRacikan = "";
            oldNamaHasilParfum = "";
        }

        /// <summary>
        /// Populates all ComboBoxes (ID_Pelanggan, ID_Aroma, ID_Pegawai, Nama_Aroma, Perbandingan).
        /// </summary>
        private void LoadComboBoxes()
        {
            LoadIdPelanggan();
            LoadIdAroma();
            LoadIdPegawai();
            LoadNamaAroma(); // For cmbNamaAroma
            LoadPerbandingan();
        }

        private void LoadIdPelanggan()
        {
            try
            {
                cmbIdPelanggan.Items.Clear();
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // Use a query that fetches trimmed IDs for CHAR columns
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

        private void LoadIdAroma()
        {
            try
            {
                cmbIdAroma.Items.Clear();
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // Use a query that fetches trimmed IDs for CHAR columns
                    string query = "SELECT TRIM(ID_Aroma) AS ID_Aroma FROM Aroma_Parfum ORDER BY ID_Aroma";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cmbIdAroma.Items.Add(reader["ID_Aroma"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat ID aroma: " + ex.Message, "Error Data Load", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogError(ex, "Load ID Aroma ComboBox");
            }
        }

        private void LoadIdPegawai()
        {
            try
            {
                cmbIdPegawai.Items.Clear();
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // Use a query that fetches trimmed IDs for CHAR columns
                    string query = "SELECT TRIM(ID_Pegawai) AS ID_Pegawai FROM Pegawai ORDER BY ID_Pegawai";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cmbIdPegawai.Items.Add(reader["ID_Pegawai"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat ID pegawai: " + ex.Message, "Error Data Load", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogError(ex, "Load ID Pegawai ComboBox");
            }
        }

        private void LoadNamaAroma()
        {
            try
            {
                cmbNamaAroma.Items.Clear();
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // Load actual aroma names from Aroma_Parfum for the combobox
                    string query = "SELECT Nama_Aroma FROM Aroma_Parfum ORDER BY Nama_Aroma";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cmbNamaAroma.Items.Add(reader["Nama_Aroma"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat Nama Aroma: " + ex.Message, "Error Data Load", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogError(ex, "Load Nama Aroma ComboBox");
            }
        }

        private void LoadPerbandingan()
        {
            cmbPerbandingan.Items.Clear();
            // Populate with predefined comparison values based on your CHECK constraint
            cmbPerbandingan.Items.Add("1:1");
            cmbPerbandingan.Items.Add("1:2");
            cmbPerbandingan.Items.Add("1:3");
            cmbPerbandingan.Items.Add("2:1");
            cmbPerbandingan.Items.Add("2:2");
            cmbPerbandingan.Items.Add("2:3");
            cmbPerbandingan.Items.Add("3:1");
            cmbPerbandingan.Items.Add("3:2");
            cmbPerbandingan.Items.Add("3:3");
            // Add more as per your business needs, e.g., "4:1", "1:4" etc.
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
                                 "Silakan cek izin tulis folder aplikasi Anda.", "Error Logging Critical", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

  

        /// <summary>
        /// Handles the click event for the "Tambah" (Add) button.
        /// Adds a new Racikan_Parfum record to the database using a stored procedure with validation.
        /// </summary>
        private void btnTambah_Click(object sender, EventArgs e)
        {
            // Client-side Input Validation: Check for empty fields
            if (string.IsNullOrWhiteSpace(txtIdRacikan.Text) ||
                cmbIdPelanggan.SelectedItem == null ||
                cmbIdAroma.SelectedItem == null ||
                cmbIdPegawai.SelectedItem == null ||
                cmbPerbandingan.SelectedItem == null ||
                string.IsNullOrWhiteSpace(cmbNamaAroma.Text) || // For editable combobox, check Text property
                string.IsNullOrWhiteSpace(txtNamaHasilParfum.Text))
            {
                MessageBox.Show("Semua kolom harus diisi sebelum menambah data racikan parfum.", "Peringatan Input Kosong", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Client-side Validation: ID_Racikan format
            // Must start with '04' and be 3 or 4 characters long (alphanumeric).
            if (!Regex.IsMatch(txtIdRacikan.Text.Trim(), @"^04[A-Za-z0-9]{1,2}$"))
            {
                MessageBox.Show("ID Racikan harus dimulai dengan '04' dan memiliki panjang total 3 atau 4 karakter.", "Input Error: ID Racikan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtIdRacikan.Focus();
                return;
            }

            // Client-side Validation: Nama_Racikan and Nama_Hasil_Parfum (alphanumeric and spaces only)
            if (!Regex.IsMatch(cmbNamaAroma.Text.Trim(), @"^[A-Za-z0-9 ]+$"))
            {
                MessageBox.Show("Nama Racikan hanya boleh berisi huruf, angka, dan spasi.", "Input Error: Nama Racikan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbNamaAroma.Focus();
                return;
            }
            if (!Regex.IsMatch(txtNamaHasilParfum.Text.Trim(), @"^[A-Za-z0-9 ]+$"))
            {
                MessageBox.Show("Nama Hasil Parfum hanya boleh berisi huruf, angka, dan spasi.", "Input Error: Nama Hasil Parfum", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNamaHasilParfum.Focus();
                return;
            }

            // Client-side Validation: Perbandingan format (e.g., "1:3" with digits 1-9)
            if (!Regex.IsMatch(cmbPerbandingan.SelectedItem.ToString(), @"^[1-9]:[1-9]$"))
            {
                MessageBox.Show("Format Perbandingan tidak valid. Contoh: 1:3 (Angka 1-9 diikuti ':' dan angka 1-9).", "Input Error: Perbandingan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbPerbandingan.Focus();
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        // Pre-emptive check if ID_Racikan already exists for better UX
                        SqlCommand checkIdCmd = new SqlCommand("SELECT COUNT(1) FROM Racikan_Parfum WHERE TRIM(ID_Racikan) = @ID_Racikan", conn, tran);
                        checkIdCmd.Parameters.AddWithValue("@ID_Racikan", txtIdRacikan.Text.Trim());
                        int idCount = (int)checkIdCmd.ExecuteScalar();
                        if (idCount > 0)
                        {
                            MessageBox.Show($"Gagal Menambah Data: ID Racikan '{txtIdRacikan.Text.Trim()}' sudah ada. Silakan gunakan ID lain.", "Kesalahan Input Data", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            tran.Rollback();
                            txtIdRacikan.Focus();
                            return;
                        }

                        // Call the stored procedure to add data
                        SqlCommand cmd = new SqlCommand("sp_TambahRacikanParfum", conn, tran)
                        {
                            CommandType = CommandType.StoredProcedure
                        };

                        // Add parameters, trim all string values for cleanliness and CHAR column compatibility
                        cmd.Parameters.AddWithValue("@ID_Pelanggan", cmbIdPelanggan.SelectedItem.ToString().Trim());
                        cmd.Parameters.AddWithValue("@ID_Aroma", cmbIdAroma.SelectedItem.ToString().Trim());
                        cmd.Parameters.AddWithValue("@ID_Pegawai", cmbIdPegawai.SelectedItem.ToString().Trim());
                        cmd.Parameters.AddWithValue("@ID_Racikan", txtIdRacikan.Text.Trim());
                        cmd.Parameters.AddWithValue("@Perbandingan", cmbPerbandingan.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@Nama_Racikan", cmbNamaAroma.Text.Trim());
                        cmd.Parameters.AddWithValue("@Nama_Hasil_Parfum", txtNamaHasilParfum.Text.Trim());
                        cmd.ExecuteNonQuery();

                        tran.Commit();
                        MessageBox.Show("Berhasil Menambah Data Racikan Parfum.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (SqlException ex)
                    {
                        tran.Rollback();
                        LogError(ex, "Tambah Racikan Parfum"); // Log the detailed SQL error

                        string errorMessage = "Terjadi kesalahan saat menambah data racikan parfum: ";
                        switch (ex.Number)
                        {
                            case 2627: // Primary key violation (duplicate ID_Racikan)
                                errorMessage += $"ID Racikan '{txtIdRacikan.Text.Trim()}' sudah ada. Silakan gunakan ID lain.";
                                break;
                            case 547: // Foreign key or CHECK constraint violation
                                if (ex.Message.Contains("FK__Racikan_P__ID_Pelanggan"))
                                {
                                    errorMessage += "ID Pelanggan tidak valid atau tidak ditemukan.";
                                }
                                else if (ex.Message.Contains("FK__Racikan_P__ID_Aroma"))
                                {
                                    errorMessage += "ID Aroma tidak valid atau tidak ditemukan.";
                                }
                                else if (ex.Message.Contains("FK__Racikan_P__ID_Pegawai"))
                                {
                                    errorMessage += "ID Pegawai tidak valid atau tidak ditemukan.";
                                }
                                else if (ex.Message.Contains("CK__Racikan_P__Perbandingan")) // Based on your CHECK constraint name
                                {
                                    errorMessage += "Format Perbandingan tidak valid (harus seperti '1:3' dengan angka 1-9).";
                                }
                                else if (ex.Message.Contains("CK__Racikan_P__ID_Racikan")) // Based on your CHECK constraint name for ID format
                                {
                                    errorMessage += "ID Racikan tidak sesuai format (harus dimulai dengan '04' dan 3 atau 4 karakter).";
                                }
                                else
                                {
                                    errorMessage += $"Terdapat pelanggaran constraint database: {ex.Message}";
                                }
                                break;
                            case 8152: // String or binary data would be truncated (e.g., Nama_Racikan or Nama_Hasil_Parfum too long)
                                errorMessage += "Panjang data Nama Racikan atau Nama Hasil Parfum terlalu panjang. Mohon periksa kembali.";
                                break;
                            case 50000: // Custom RAISERROR from triggers
                                errorMessage = ex.Message; // Directly use the specific message from the trigger
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
                        LogError(ex, "Tambah Racikan Parfum (Umum)"); // Log general application errors
                        MessageBox.Show("Terjadi kesalahan tak terduga: " + ex.Message + "\nPerubahan dibatalkan.", "Error Aplikasi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        // Always refresh data and clear fields regardless of success or failure
                        InvalidateRacikanCache();
                        LoadInitialData();
                        ClearInputFields();
                    }
                }
            }
        }

        /// <summary>
        /// Handles the click event for the "Hapus" (Delete) button.
        /// Deletes a Racikan_Parfum record from the database using a stored procedure.
        /// </summary>
        private void btnHapus_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtIdRacikan.Text))
            {
                MessageBox.Show("ID Racikan harus diisi atau dipilih dari tabel untuk menghapus.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtIdRacikan.Focus();
                return;
            }

            if (MessageBox.Show("Apakah Anda yakin ingin menghapus data racikan parfum ini?", "Konfirmasi Hapus", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlTransaction tran = conn.BeginTransaction())
                    {
                        try
                        {
                            SqlCommand cmd = new SqlCommand("sp_HapusRacikanParfum", conn, tran)
                            {
                                CommandType = CommandType.StoredProcedure
                            };
                            cmd.Parameters.AddWithValue("@ID_Racikan", txtIdRacikan.Text.Trim()); // Trim ID for consistency

                            int rowsAffected = cmd.ExecuteNonQuery();
                            tran.Commit();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Berhasil Menghapus Data Racikan Parfum.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show($"Data racikan parfum dengan ID '{txtIdRacikan.Text.Trim()}' tidak ditemukan.", "Hapus Gagal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                        catch (SqlException ex)
                        {
                            tran.Rollback();
                            LogError(ex, "Hapus Racikan Parfum"); // Log the detailed SQL error

                            string errorMessage = "Terjadi kesalahan saat menghapus data racikan parfum: ";
                            if (ex.Number == 547) // Foreign key constraint violation (should not happen if Racikan_Parfum is leaf table, but good practice)
                            {
                                errorMessage += "Data racikan ini tidak dapat dihapus karena terkait dengan data lain. Mohon periksa integritas data.";
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
                            LogError(ex, "Hapus Racikan Parfum (Umum)");
                            MessageBox.Show("Gagal Menghapus Data Racikan Parfum: " + ex.Message, "Error Aplikasi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        finally
                        {
                            InvalidateRacikanCache();
                            LoadInitialData();
                            ClearInputFields();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the click event for the "Update" button.
        /// Updates an existing Racikan_Parfum record in the database using a stored procedure.
        /// </summary>
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            // Client-side Input Validation: Check for empty fields
            if (string.IsNullOrWhiteSpace(txtIdRacikan.Text) ||
                cmbIdPelanggan.SelectedItem == null ||
                cmbIdAroma.SelectedItem == null ||
                cmbIdPegawai.SelectedItem == null ||
                cmbPerbandingan.SelectedItem == null ||
                string.IsNullOrWhiteSpace(cmbNamaAroma.Text) ||
                string.IsNullOrWhiteSpace(txtNamaHasilParfum.Text))
            {
                MessageBox.Show("Semua kolom harus diisi sebelum mengupdate data racikan parfum.", "Peringatan Input Kosong", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Client-side Validation: ID_Racikan format (should match existing, but re-validate for safety)
            if (!Regex.IsMatch(txtIdRacikan.Text.Trim(), @"^04[A-Za-z0-9]{1,2}$"))
            {
                MessageBox.Show("ID Racikan harus dimulai dengan '04' dan memiliki panjang total 3 atau 4 karakter (contoh: 04A, 04AB).", "Input Error: ID Racikan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtIdRacikan.Focus();
                return;
            }

            // Client-side Validation: Nama_Racikan and Nama_Hasil_Parfum
            if (!Regex.IsMatch(cmbNamaAroma.Text.Trim(), @"^[A-Za-z0-9 ]+$"))
            {
                MessageBox.Show("Nama Racikan hanya boleh berisi huruf, angka, dan spasi.", "Input Error: Nama Racikan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbNamaAroma.Focus();
                return;
            }
            if (!Regex.IsMatch(txtNamaHasilParfum.Text.Trim(), @"^[A-Za-z0-9 ]+$"))
            {
                MessageBox.Show("Nama Hasil Parfum hanya boleh berisi huruf, angka, dan spasi.", "Input Error: Nama Hasil Parfum", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNamaHasilParfum.Focus();
                return;
            }

            // Client-side Validation: Perbandingan format
            if (!Regex.IsMatch(cmbPerbandingan.SelectedItem.ToString(), @"^[1-9]:[1-9]$"))
            {
                MessageBox.Show("Format Perbandingan tidak valid. Contoh: 1:3 (Angka 1-9 diikuti ':' dan angka 1-9).", "Input Error: Perbandingan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbPerbandingan.Focus();
                return;
            }

            // Check for actual changes before attempting update
            bool hasChanges = false;
            // Compare trimmed current values with trimmed old values
            if (txtIdRacikan.Text.Trim() != oldIdRacikan.Trim() ||
                (cmbIdPelanggan.SelectedItem?.ToString()?.Trim() ?? "") != oldIdPelanggan.Trim() ||
                (cmbIdAroma.SelectedItem?.ToString()?.Trim() ?? "") != oldIdAroma.Trim() ||
                (cmbIdPegawai.SelectedItem?.ToString()?.Trim() ?? "") != oldIdPegawai.Trim() ||
                (cmbPerbandingan.SelectedItem?.ToString() ?? "") != oldPerbandingan ||
                cmbNamaAroma.Text.Trim() != oldNamaRacikan.Trim() ||
                txtNamaHasilParfum.Text.Trim() != oldNamaHasilParfum.Trim())
            {
                hasChanges = true;
            }

            if (!hasChanges)
            {
                MessageBox.Show("Tidak ada perubahan data untuk diperbarui.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("Apakah Anda yakin ingin mengupdate data racikan parfum ini?", "Konfirmasi Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlTransaction tran = conn.BeginTransaction())
                    {
                        try
                        {
                            SqlCommand cmd = new SqlCommand("sp_UpdateRacikanParfum", conn, tran)
                            {
                                CommandType = CommandType.StoredProcedure
                            };
                            // Add parameters, trim all string values
                            cmd.Parameters.AddWithValue("@ID_Racikan", txtIdRacikan.Text.Trim());
                            cmd.Parameters.AddWithValue("@ID_Pelanggan", cmbIdPelanggan.SelectedItem.ToString().Trim());
                            cmd.Parameters.AddWithValue("@ID_Aroma", cmbIdAroma.SelectedItem.ToString().Trim());
                            cmd.Parameters.AddWithValue("@ID_Pegawai", cmbIdPegawai.SelectedItem.ToString().Trim());
                            cmd.Parameters.AddWithValue("@Perbandingan", cmbPerbandingan.SelectedItem.ToString());
                            cmd.Parameters.AddWithValue("@Nama_Racikan", cmbNamaAroma.Text.Trim());
                            cmd.Parameters.AddWithValue("@Nama_Hasil_Parfum", txtNamaHasilParfum.Text.Trim());

                            int rowsAffected = cmd.ExecuteNonQuery();
                            tran.Commit();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Berhasil Mengupdate Data Racikan Parfum.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show($"Data racikan parfum dengan ID '{txtIdRacikan.Text.Trim()}' tidak ditemukan untuk diupdate.", "Update Gagal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                        catch (SqlException ex)
                        {
                            tran.Rollback();
                            LogError(ex, "Update Racikan Parfum"); // Log the detailed SQL error

                            string errorMessage = "Terjadi kesalahan saat mengupdate data racikan parfum: ";
                            switch (ex.Number)
                            {
                                case 547: // Foreign key or CHECK constraint violation
                                    if (ex.Message.Contains("FK__Racikan_P__ID_Pelanggan"))
                                    {
                                        errorMessage += "ID Pelanggan tidak valid atau tidak ditemukan.";
                                    }
                                    else if (ex.Message.Contains("FK__Racikan_P__ID_Aroma"))
                                    {
                                        errorMessage += "ID Aroma tidak valid atau tidak ditemukan.";
                                    }
                                    else if (ex.Message.Contains("FK__Racikan_P__ID_Pegawai"))
                                    {
                                        errorMessage += "ID Pegawai tidak valid atau tidak ditemukan.";
                                    }
                                    else if (ex.Message.Contains("CK__Racikan_P__Perbandingan")) // Based on your CHECK constraint name
                                    {
                                        errorMessage += "Format Perbandingan tidak valid (harus seperti '1:3' dengan angka 1-9).";
                                    }
                                    else if (ex.Message.Contains("CK__Racikan_P__ID_Racikan")) // Based on your CHECK constraint name for ID_Racikan
                                    {
                                        errorMessage += "ID Racikan tidak sesuai format (harus dimulai dengan '04' dan 3 atau 4 karakter).";
                                    }
                                    else
                                    {
                                        errorMessage += $"Terdapat pelanggaran constraint database: {ex.Message}";
                                    }
                                    break;
                                case 8152: // String or binary data would be truncated
                                    errorMessage += "Panjang data Nama Racikan atau Nama Hasil Parfum terlalu panjang. Mohon periksa kembali.";
                                    break;
                                case 50000: // Custom RAISERROR
                                    errorMessage = ex.Message; // Use the specific message from the trigger
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
                            LogError(ex, "Update Racikan Parfum (Umum)");
                            MessageBox.Show("Gagal Mengupdate Data Racikan Parfum: " + ex.Message, "Error Aplikasi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        finally
                        {
                            InvalidateRacikanCache();
                            LoadInitialData();
                            ClearInputFields();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the click event for the "Refresh" button.
        /// Reloads the data from the database and refreshes comboboxes.
        /// </summary>
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            InvalidateRacikanCache(); // Clear the cache
            LoadInitialData();        // Re-populate the cache and display data
            LoadComboBoxes();         // Also refresh dropdowns as underlying data might have changed
            ClearInputFields();       // Clear input fields
            MessageBox.Show("Data berhasil di-refresh.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Handles the CellClick event of the DataGridView.
        /// Populates the text boxes and combo boxes with the data from the selected row.
        /// </summary>
        private void dgvRacikanParfum_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0 && e.RowIndex < dgvRacikanParfum.Rows.Count)
                {
                    DataGridViewRow row = dgvRacikanParfum.Rows[e.RowIndex];

                    txtIdRacikan.Text = row.Cells["ID_Racikan"]?.Value?.ToString()?.Trim() ?? ""; // Trim for CHAR(4)

                    // Set selected item for ComboBoxes, trimming values for comparison against items
                    // Use a temporary variable to hold trimmed value from grid
                    string idPelanggan = row.Cells["ID_Pelanggan"]?.Value?.ToString()?.Trim() ?? "";
                    if (cmbIdPelanggan.Items.Contains(idPelanggan)) cmbIdPelanggan.SelectedItem = idPelanggan;
                    else cmbIdPelanggan.SelectedIndex = -1; // Clear selection if not found

                    string idAroma = row.Cells["ID_Aroma"]?.Value?.ToString()?.Trim() ?? "";
                    if (cmbIdAroma.Items.Contains(idAroma)) cmbIdAroma.SelectedItem = idAroma;
                    else cmbIdAroma.SelectedIndex = -1;

                    string idPegawai = row.Cells["ID_Pegawai"]?.Value?.ToString()?.Trim() ?? "";
                    if (cmbIdPegawai.Items.Contains(idPegawai)) cmbIdPegawai.SelectedItem = idPegawai;
                    else cmbIdPegawai.SelectedIndex = -1;

                    string perbandingan = row.Cells["Perbandingan"]?.Value?.ToString() ?? "";
                    if (cmbPerbandingan.Items.Contains(perbandingan)) cmbPerbandingan.SelectedItem = perbandingan;
                    else cmbPerbandingan.SelectedIndex = -1;

                    cmbNamaAroma.Text = row.Cells["Nama_Racikan"]?.Value?.ToString()?.Trim() ?? "";
                    txtNamaHasilParfum.Text = row.Cells["Nama_Hasil_Parfum"]?.Value?.ToString()?.Trim() ?? "";

                    // Store the old values for change detection, ensure they are trimmed
                    oldIdRacikan = txtIdRacikan.Text.Trim();
                    oldIdPelanggan = idPelanggan;
                    oldIdAroma = idAroma;
                    oldIdPegawai = idPegawai;
                    oldPerbandingan = perbandingan;
                    oldNamaRacikan = cmbNamaAroma.Text.Trim();
                    oldNamaHasilParfum = txtNamaHasilParfum.Text.Trim();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memilih data dari tabel: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogError(ex, "DataGridView CellClick");
            }
        }

 

        /// <summary>
        /// Handles the click event for the "Import" button.
        /// Opens a file dialog to select an Excel file and previews its data.
        /// </summary>
        private void Import_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFile = new OpenFileDialog())
            {
                openFile.Filter = "Excel Files|*.xlsx;*.xlsm";
                openFile.Title = "Pilih File Excel untuk Import Racikan Parfum";
                if (openFile.ShowDialog() == DialogResult.OK)
                {
                    PreviewData(openFile.FileName);
                }
            }
        }

        /// <summary>
        /// Reads data from the specified Excel file and displays it in a preview form.
        /// This method includes the fix for correctly reading formula cell values.
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

                    // Read header row and add columns to DataTable
                    IRow headerRow = sheet.GetRow(0);
                    if (headerRow != null)
                    {
                        foreach (ICell cell in headerRow.Cells)
                        {
                            string columnName = cell.ToString();
                            // Ensure column names are unique and not empty
                            if (!string.IsNullOrEmpty(columnName) && !dt.Columns.Contains(columnName))
                            {
                                dt.Columns.Add(columnName);
                            }
                            else if (string.IsNullOrEmpty(columnName))
                            {
                                dt.Columns.Add($"Column{dt.Columns.Count + 1}"); // Assign generic name for empty headers
                            }
                            else
                            {
                                int i = 1;
                                while (dt.Columns.Contains(columnName + "_" + i)) // Resolve duplicate names
                                {
                                    i++;
                                }
                                dt.Columns.Add(columnName + "_" + i);
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
                                // CORRECTED LOGIC FOR HANDLING FORMULA CELLS
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
                                        // Get the cached result type and then retrieve the value
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
                                                // Fallback for other formula result types or if evaluation is not cached
                                                newRow[j] = cell.ToString();
                                                break;
                                        }
                                        break;
                                    default:
                                        newRow[j] = cell.ToString(); // Handles CellType.Blank, etc.
                                        break;
                                }
                            }
                            else
                            {
                                newRow[j] = DBNull.Value; // Assign DBNull.Value for empty cells
                            }
                        }
                        dt.Rows.Add(newRow);
                    }

                    // Ensure you have a form named 'PreviewDataRacikan' that accepts a DataTable in its constructor
                    using (PreviewDataRacikan previewForm = new PreviewDataRacikan(dt))
                    {
                        previewForm.ShowDialog();
                    }
                    InvalidateRacikanCache(); // Invalidate cache after potential import
                    LoadInitialData();      // Reload data
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show("Gagal membaca file Excel. Pastikan file tidak sedang dibuka oleh program lain dan lokasinya benar.\nDetail: " + ex.Message, "Error File Akses", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogError(ex, "Preview Excel Data Racikan Parfum (File Access)");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal membaca file Excel: " + ex.Message, "Error Import Excel", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogError(ex, "Preview Excel Data Racikan Parfum (General)");
            }
        }


        /// <summary>
        /// Handles the click event for the "Analisis" button.
        /// Performs a simple performance analysis of a SELECT query and displays index information for the 'Racikan_Parfum' table.
        /// </summary>
        private void Analisis_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // 1. Get index information for the 'Racikan_Parfum' table
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
                        t.name = 'Racikan_Parfum' AND ind.is_primary_key = 0 AND ind.[type] <> 0
                    ORDER BY
                        t.name, ind.name, ind.index_id;";

                    SqlCommand cmd = new SqlCommand(indexQuery, conn);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dtIndex = new DataTable();
                    da.Fill(dtIndex);

                    // 2. Measure query execution time for a simple SELECT on 'Racikan_Parfum'
                    string selectQuery = "SELECT ID_Racikan, ID_Pelanggan, ID_Aroma, ID_Pegawai, Perbandingan, Nama_Racikan, Nama_Hasil_Parfum FROM Racikan_Parfum";
                    SqlCommand cmd2 = new SqlCommand(selectQuery, conn);
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    using (SqlDataReader reader = cmd2.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Just read to consume the results and measure time
                        }
                    }
                    stopwatch.Stop();

                    long execTimeMs = stopwatch.ElapsedMilliseconds;
                    StringBuilder infoBuilder = new StringBuilder();
                    infoBuilder.AppendLine($"📌 Waktu Eksekusi Query 'SELECT ... FROM Racikan_Parfum': {execTimeMs} ms\n");
                    infoBuilder.AppendLine("📦 Indeks pada Tabel 'Racikan_Parfum':");

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
                        infoBuilder.AppendLine("Tidak ada indeks non-primary key ditemukan untuk tabel Racikan_Parfum.");
                    }

                    MessageBox.Show(infoBuilder.ToString(), "Analisis Query & Index Racikan Parfum", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menganalisis: " + ex.Message, "Error Analisis", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogError(ex, "Analisis Racikan Parfum");
            }
        }

        /// <summary>
        /// Handles the click event for the "Kembali" (Back) button.
        /// Closes the current form and opens the HalamanMenu form.
        /// </summary>
        private void btnKEMBALI_Click(object sender, EventArgs e)
        {
            HalamanMenu HalamanMenuForm = new HalamanMenu();
            HalamanMenuForm.Show();
            this.Hide();
        }

        // --- Unused Event Handlers (Can be removed if not needed in your form designer) ---
        private void dgvRacikanParfum_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Empty, can be removed if not used.
        }

        private void RACIKAN_PARFUM_Load(object sender, EventArgs e)
        {
            // Empty, can be removed if no specific load logic is needed.
        }

        private void label2_Click(object sender, EventArgs e)
        {
            // Empty, can be removed if not used.
        }

        private void cmbPerbandingan_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Empty, can be removed if no specific logic needed on selection change.
        }
    }
}