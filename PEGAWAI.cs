using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text; // Ditambahkan untuk StringBuilder dalam LogError
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions; // Tambahkan ini untuk Regex
using System.Diagnostics; // Untuk debugging dengan Debug.WriteLine
using HOMEPAGE; // Pastikan ini ada

namespace HOMEPAGE
{
    public partial class PEGAWAI : Form
    {
        // Gunakan connectionString yang sudah didefinisikan secara global
        // Hapus baris ini:
        // private string connectionString = "Data Source=LAPTOP-T1UUTAE0\\HIKMATYAR;Initial Catalog=Manajemen_Penjualan_Parfum;Integrated Security=True";

        private koneksi kn = new koneksi(); // Tambahkan ini 
        SqlConnection conn; // Deklarasi SqlConnection di sini 

        public PEGAWAI()
        {
            InitializeComponent();
            conn = new SqlConnection(kn.connectionString()); // Inisialisasi di konstruktor 
            LoadCachedData(); // Memuat data awal saat form dibuka
        }

        DataTable cachePegawai = null;
        private string oldNamaPegawai = "";

        // Tambahkan metode LogError untuk konsistensi
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

        // Metode TampilData() ini redundan karena LoadCachedData() sudah melakukan tugas serupa
        // Disarankan untuk menghapusnya jika tidak ada panggilan eksternal yang spesifik
        void TampilData()
        {
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }
                string query = "SELECT * FROM Pegawai"; // Query ini tidak akan terurut secara numerik ID
                SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menampilkan data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        private void LoadCachedData()
        {
            Debug.WriteLine("LoadCachedData() dipanggil.");
            // MODIFIKASI: Atur kursor loading saat mulai memuat data
            Cursor.Current = Cursors.WaitCursor; // Tampilkan kursor jam pasir

            cachePegawai = new DataTable(); // Selalu buat DataTable baru untuk memastikan data fresh

            try
            {
                // Gunakan kn.connectionString() di sini
                using (var newConn = new SqlConnection(kn.connectionString()))
                {
                    newConn.Open();
                    // MODIFIKASI: Gunakan Stored Procedure GetAllPegawai (untuk pengurutan numerik)
                    using (var cmd = new SqlCommand("GetAllPegawai", newConn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure; // Penting: Tentukan CommandType sebagai StoredProcedure
                        using (var da = new SqlDataAdapter(cmd)) // Gunakan SqlCommand sebagai argumen SqlDataAdapter
                        {
                            da.Fill(cachePegawai);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data cache: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogError(ex, "Load Cached Data Pegawai"); // Panggil LogError
                Debug.WriteLine($"Error di LoadCachedData: {ex.Message}");
            }
            finally
            {
                // MODIFIKASI: Kembalikan kursor ke default setelah selesai (baik sukses maupun gagal)
                Cursor.Current = Cursors.Default;
            }

            dgvPegawai.DataSource = cachePegawai;
            dgvPegawai.Refresh();
        }

        private void btnTambah_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtIdPegawai.Text))
            {
                MessageBox.Show("ID Pegawai tidak boleh kosong.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtIdPegawai.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNama.Text))
            {
                MessageBox.Show("Nama Pegawai tidak boleh kosong.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNama.Focus();
                return;
            }

            // Client-side validation for ID_Pegawai format
            if (!Regex.IsMatch(txtIdPegawai.Text.Trim(), @"^03\d{1,5}$"))
            {
                MessageBox.Show("ID Pegawai harus dimulai dengan '03' dan diikuti 1 hingga 5 digit angka (total 3-7 karakter)", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtIdPegawai.Focus();
                return;
            }

            // Client-side validation for Nama_Pegawai
            if (Regex.IsMatch(txtNama.Text, @"[^A-Za-z ]"))
            {
                MessageBox.Show("Nama Pegawai hanya boleh berisi huruf dan spasi.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNama.Focus();
                return;
            }

            // Gunakan kn.connectionString() di sini
            using (var newConn = new SqlConnection(kn.connectionString()))
            {
                newConn.Open();
                using (var tran = newConn.BeginTransaction())
                {
                    try
                    {
                        SqlCommand checkCmd = new SqlCommand("SELECT COUNT(1) FROM Pegawai WHERE ID_Pegawai = @ID", newConn, tran);
                        checkCmd.Parameters.AddWithValue("@ID", txtIdPegawai.Text.Trim());
                        int count = (int)checkCmd.ExecuteScalar();
                        if (count > 0)
                        {
                            MessageBox.Show($"Gagal Menambah Data: ID Pegawai '{txtIdPegawai.Text.Trim()}' sudah ada. Silakan gunakan ID lain.", "Kesalahan Input Data", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            tran.Rollback();
                            txtIdPegawai.Focus();
                            return;
                        }

                        var cmd = new SqlCommand("sp_TambahPegawai", newConn, tran)
                        {
                            CommandType = CommandType.StoredProcedure
                        };
                        cmd.Parameters.AddWithValue("@ID", txtIdPegawai.Text.Trim());
                        cmd.Parameters.AddWithValue("@Nama", txtNama.Text);
                        cmd.ExecuteNonQuery();

                        tran.Commit();
                        MessageBox.Show("Berhasil Menambah Data", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Perbarui DataGridView setelah operasi CRUD
                        LoadCachedData();
                        ClearInputFields();
                    }
                    catch (SqlException ex)
                    {
                        tran.Rollback();
                        LogError(ex, "Tambah Pegawai"); // Panggil LogError

                        string errorMessage = "Terjadi kesalahan saat menambah data pegawai: ";

                        switch (ex.Number)
                        {
                            case 2627:
                                errorMessage += $"ID Pegawai '{txtIdPegawai.Text.Trim()}' sudah ada. Silakan gunakan ID lain.";
                                break;
                            case 50000:
                                errorMessage += ex.Message;
                                break;
                            case 8152:
                                errorMessage += "Nama Pegawai terlalu panjang.";
                                break;
                            case 547:
                                errorMessage += "ID Pegawai tidak sesuai format (harus dimulai dengan '03' dan 3 atau 4 karakter).";
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

        private void btnHapus_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtIdPegawai.Text))
            {
                MessageBox.Show("ID Pegawai harus diisi untuk menghapus.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtIdPegawai.Focus();
                return;
            }

            if (MessageBox.Show("Apakah Anda yakin ingin menghapus data ini?", "Konfirmasi Hapus", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                // Gunakan kn.connectionString() di sini
                using (var newConn = new SqlConnection(kn.connectionString()))
                {
                    newConn.Open();
                    using (var tran = newConn.BeginTransaction())
                    {
                        try
                        {
                            var cmd = new SqlCommand("sp_HapusPegawai", newConn, tran)
                            {
                                CommandType = CommandType.StoredProcedure
                            };
                            cmd.Parameters.AddWithValue("@ID", txtIdPegawai.Text.Trim());
                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected == 0)
                            {
                                MessageBox.Show($"Data pegawai dengan ID '{txtIdPegawai.Text.Trim()}' tidak ditemukan.", "Hapus Gagal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                tran.Rollback();
                            }
                            else
                            {
                                tran.Commit();
                                MessageBox.Show("Berhasil Menghapus Data", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                // Perbarui DataGridView setelah operasi CRUD
                                LoadCachedData();
                                ClearInputFields();
                            }
                        }
                        catch (SqlException ex)
                        {
                            tran.Rollback();
                            LogError(ex, "Hapus Pegawai"); // Panggil LogError

                            string errorMessage = "Terjadi kesalahan saat menghapus data pegawai: ";
                            if (ex.Number == 547)
                            {
                                errorMessage += "Data pegawai ini tidak dapat dihapus karena terkait dengan data lain (misalnya, penjualan).";
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

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtIdPegawai.Text))
            {
                MessageBox.Show("ID Pegawai tidak boleh kosong untuk update.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtIdPegawai.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNama.Text))
            {
                MessageBox.Show("Nama Pegawai tidak boleh kosong untuk update.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNama.Focus();
                return;
            }

            // Client-side validation for Nama_Pegawai
            if (Regex.IsMatch(txtNama.Text, @"[^A-Za-z ]"))
            {
                MessageBox.Show("Nama Pegawai hanya boleh berisi huruf dan spasi.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNama.Focus();
                return;
            }

            if (txtNama.Text == oldNamaPegawai)
            {
                MessageBox.Show("Tidak Ada Perubahan yang Dilakukan.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("Apakah Anda yakin ingin mengupdate data ini?", "Konfirmasi Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                // Gunakan kn.connectionString() di sini
                using (var newConn = new SqlConnection(kn.connectionString()))
                {
                    newConn.Open();
                    using (var tran = newConn.BeginTransaction())
                    {
                        try
                        {
                            var cmd = new SqlCommand("sp_UpdatePegawai", newConn, tran)
                            {
                                CommandType = CommandType.StoredProcedure
                            };
                            cmd.Parameters.AddWithValue("@ID", txtIdPegawai.Text.Trim());
                            cmd.Parameters.AddWithValue("@Nama", txtNama.Text);

                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected == 0)
                            {
                                MessageBox.Show($"Data pegawai dengan ID '{txtIdPegawai.Text.Trim()}' tidak ditemukan untuk diupdate.", "Update Gagal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                tran.Rollback();
                            }
                            else
                            {
                                tran.Commit();
                                MessageBox.Show("Berhasil Mengupdate Data", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                // Perbarui DataGridView setelah operasi CRUD
                                LoadCachedData();
                                ClearInputFields();
                            }
                        }
                        catch (SqlException ex)
                        {
                            tran.Rollback();
                            LogError(ex, "Update Pegawai"); // Panggil LogError

                            string errorMessage = "Terjadi kesalahan saat mengupdate data pegawai: ";
                            switch (ex.Number)
                            {
                                case 50000:
                                    errorMessage += ex.Message;
                                    break;
                                case 8152:
                                    errorMessage += "Nama Pegawai terlalu panjang.";
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

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadCachedData(); // Cukup panggil LoadCachedData() untuk refresh
            ClearInputFields();
            // MODIFIKASI: Tambahkan pesan refresh
            MessageBox.Show("Data berhasil di-refresh.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnKEMBALI_Click(object sender, EventArgs e)
        {
            HalamanMenu HalamanMenuForm = new HalamanMenu();
            HalamanMenuForm.Show();
            this.Hide();
        }

        private void label3_Click(object sender, EventArgs e)
        {
            // Event handler ini kosong, mungkin tidak diperlukan.
        }

        private void dgvPegawai_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Biasanya dgv_CellClick lebih sering digunakan.
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvPegawai.Rows[e.RowIndex];
                txtIdPegawai.Text = row.Cells["ID_Pegawai"].Value.ToString().Trim();
                txtNama.Text = row.Cells["Nama_Pegawai"].Value.ToString();
                oldNamaPegawai = txtNama.Text;
            }
        }

        private void dgvPegawai_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvPegawai.Rows[e.RowIndex];
                txtIdPegawai.Text = row.Cells["ID_Pegawai"].Value.ToString().Trim();
                txtNama.Text = row.Cells["Nama_Pegawai"].Value.ToString();
                oldNamaPegawai = txtNama.Text;
            }
        }

        private void PEGAWAI_Load(object sender, EventArgs e)
        {
            // Event handler ini kosong, tidak ada kode yang perlu dieksekusi saat form dimuat selain yang di konstruktor.
        }

        private void ClearInputFields()
        {
            txtIdPegawai.Clear();
            txtNama.Clear();
            txtIdPegawai.Focus();
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
                            dt.Columns.Add(cell.ToString());
                        }
                    }

                    for (int i = 1; i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row == null) continue;

                        DataRow newRow = dt.NewRow();
                        for (int j = 0; j < dt.Columns.Count; j++)
                        {
                            newRow[j] = row.GetCell(j)?.ToString() ?? "";
                        }
                        dt.Rows.Add(newRow);
                    }

                    // Membuat instance form PreviewDataPegawai dan menampilkannya sebagai dialog
                    PreviewDataPegawai previewForm = new PreviewDataPegawai(dt);
                    Debug.WriteLine("Membuka PreviewDataPegawai. Menunggu hasil...");

                    // Menunggu form preview ditutup.
                    // Jika form preview ditutup dengan DialogResult.OK (berarti impor berhasil/selesai)
                    if (previewForm.ShowDialog() == DialogResult.OK)
                    {
                        Debug.WriteLine("PreviewDataPegawai ditutup dengan DialogResult.OK. Memuat ulang DataGridView utama.");
                        LoadCachedData(); // Panggil LoadCachedData untuk memperbarui dgvPegawai di form ini
                    }
                    else
                    {
                        Debug.WriteLine("PreviewDataPegawai ditutup, tapi bukan dengan DialogResult.OK (mungkin dibatalkan).");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal membaca file Excel: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine($"Error di PreviewData: {ex.Message}");
            }
        }

        private void import_Click_1(object sender, EventArgs e)
        {
            using (OpenFileDialog openFile = new OpenFileDialog())
            {
                openFile.Filter = "Excel Files|*.xlsx;*.xlsm";
                if (openFile.ShowDialog() == DialogResult.OK)
                {
                    PreviewData(openFile.FileName);
                }
            }
        }

        private void btnAnalisis_Click(object sender, EventArgs e)
        {
            try
            {
                // Gunakan kn.connectionString() di sini
                using (SqlConnection newConn = new SqlConnection(kn.connectionString()))
                {
                    newConn.Open();

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
                            t.name = 'Pegawai' AND ind.is_primary_key = 0 AND ind.[type] <> 0
                        ORDER BY
                            t.name, ind.name, ind.index_id;";

                    SqlCommand cmd = new SqlCommand(indexQuery, newConn);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dtIndex = new DataTable();
                    da.Fill(dtIndex);

                    // MODIFIKASI: Panggil GetAllPegawai Stored Procedure untuk analisis
                    SqlCommand cmd2 = new SqlCommand("GetAllPegawai", newConn);
                    cmd2.CommandType = CommandType.StoredProcedure; // Penting: Tentukan CommandType

                    var stopwatch = new System.Diagnostics.Stopwatch();
                    stopwatch.Start();
                    cmd2.ExecuteReader().Close(); // Hanya eksekusi dan tutup untuk mengukur waktu
                    stopwatch.Stop();
                    long execTimeMs = stopwatch.ElapsedMilliseconds;

                    string info = $"📌 Waktu Eksekusi Stored Procedure 'GetAllPegawai': {execTimeMs} ms\n\n📦 Indeks:\n";

                    if (dtIndex.Rows.Count == 0)
                    {
                        info += "Tidak ada indeks non-primary key yang ditemukan pada tabel Pegawai.\n";
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
                LogError(ex, "Analisis Pegawai"); // Panggil LogError
            }
        }
    }
}