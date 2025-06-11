using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace HOMEPAGE
{
    public partial class AROMA_PARFUM : Form
    {
        private readonly string connectionString = "Data Source=LAPTOP-T1UUTAE0\\HIKMATYAR;Initial Catalog=Manajemen_Penjualan_Parfum;Integrated Security=True";

        private string oldNamaAroma = "";
        private string oldDeskripsi = "";

        private DataTable cachedAromaData = null;

        public AROMA_PARFUM()
        {
            InitializeComponent();
            LoadCachedData(); // Load data into the cache on form initialization
        }

        /// <summary>
        /// Displays aroma data in the DataGridView.
        /// This method primarily relies on the cached data.
        /// </summary>
        void TampilData()
        {
            if (cachedAromaData != null)
            {
                dgvAromaParfum.DataSource = cachedAromaData;
                return;
            }

            // Fallback: If cache is empty, try to load it from the database
            LoadCachedData();
        }

        /// <summary>
        /// Loads aroma data into a cache (DataTable) for quicker access.
        /// </summary>
        private void LoadCachedData()
        {
            if (cachedAromaData == null)
            {
                cachedAromaData = new DataTable();
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        using (var da = new SqlDataAdapter("GetAllAromas", conn))
                        {
                            da.Fill(cachedAromaData);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal memuat data ke cache: " + ex.Message, "Error Cache Data", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LogError(ex, "Load Cached Aroma Data");
                    cachedAromaData = null;
                }
            }
            dgvAromaParfum.DataSource = cachedAromaData;
        }

        /// <summary>
        /// Invalidates the aroma data cache.
        /// This should be called whenever data in the database changes (add, update, delete).
        /// </summary>
        private void InvalidateAromaCache()
        {
            cachedAromaData = null;
        }

        private void btnTambah_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtIdAroma.Text) ||
                string.IsNullOrWhiteSpace(txtNamaAroma.Text))
            {
                MessageBox.Show("ID Aroma dan Nama Aroma tidak boleh kosong!", "Peringatan Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        SqlCommand cmd = new SqlCommand("AddAroma", conn, transaction);
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@ID_Aroma", txtIdAroma.Text);
                        cmd.Parameters.AddWithValue("@Nama_Aroma", txtNamaAroma.Text);
                        cmd.Parameters.AddWithValue("@Deskripsi", txtDeskripsi.Text ?? (object)DBNull.Value);

                        cmd.ExecuteNonQuery();
                        transaction.Commit();

                        MessageBox.Show("Data aroma berhasil ditambahkan.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        LogError(ex, "Tambah Aroma Parfum");

                        if (ex.Number == 2627)
                        {
                            MessageBox.Show("ID Aroma sudah ada. Mohon gunakan ID yang berbeda.", "Error Duplikasi ID", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else if (ex.Number == 8152)
                        {
                            MessageBox.Show("Data yang dimasukkan terlalu panjang untuk salah satu kolom (Nama Aroma/Deskripsi). Mohon periksa kembali.", "Error Input Data", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            MessageBox.Show("Gagal menambah data: " + ex.Message + "\nPerubahan dibatalkan.", "Error Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
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
                return;
            }

            DialogResult result = MessageBox.Show("Apakah Anda yakin ingin menghapus data ini?", "Konfirmasi Hapus", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            SqlCommand cmd = new SqlCommand("DeleteAroma", conn, transaction);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@ID_Aroma", txtIdAroma.Text);

                            int rowsAffected = cmd.ExecuteNonQuery();
                            transaction.Commit();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Data berhasil dihapus.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show("Data tidak ditemukan. Pastikan ID benar.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        catch (SqlException ex)
                        {
                            transaction.Rollback();
                            LogError(ex, "Hapus Aroma Parfum");
                            if (ex.Number == 547)
                            {
                                MessageBox.Show("Gagal menghapus data: Aroma ini terkait dengan data lain (misalnya, produk parfum). Silakan hapus data terkait terlebih dahulu.", "Error Integritas Data", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            else
                            {
                                MessageBox.Show("Gagal menghapus data: " + ex.Message, "Error Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show("Gagal menghapus data: " + ex.Message, "Error Hapus Data", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            LogError(ex, "Hapus Aroma Parfum (Umum)");
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
            if (string.IsNullOrWhiteSpace(txtIdAroma.Text) ||
                string.IsNullOrWhiteSpace(txtNamaAroma.Text))
            {
                MessageBox.Show("ID Aroma dan Nama Aroma tidak boleh kosong!", "Peringatan Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtNamaAroma.Text == oldNamaAroma && txtDeskripsi.Text == oldDeskripsi)
            {
                MessageBox.Show("Tidak ada perubahan data untuk diperbarui.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult result = MessageBox.Show("Apakah Anda yakin ingin mengubah data ini?", "Konfirmasi Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            SqlCommand cmd = new SqlCommand("UpdateAroma", conn, transaction);
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.AddWithValue("@ID_Aroma", txtIdAroma.Text);
                            cmd.Parameters.AddWithValue("@Nama_Aroma", txtNamaAroma.Text);
                            cmd.Parameters.AddWithValue("@Deskripsi", txtDeskripsi.Text ?? (object)DBNull.Value);

                            int rowsAffected = cmd.ExecuteNonQuery();
                            transaction.Commit();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Data berhasil diperbarui.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show("Data tidak ditemukan. Pastikan ID benar.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        catch (SqlException ex)
                        {
                            transaction.Rollback();
                            LogError(ex, "Update Aroma Parfum");
                            if (ex.Number == 8152)
                            {
                                MessageBox.Show("Data yang dimasukkan terlalu panjang untuk salah satu kolom (Nama Aroma/Deskripsi). Mohon periksa kembali.", "Error Input Data", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            else
                            {
                                MessageBox.Show("Gagal mengupdate data: " + ex.Message, "Error Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show("Gagal mengupdate data: " + ex.Message, "Error Update Data", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            LogError(ex, "Update Aroma Parfum (Umum)");
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

        private void ClearInputFields()
        {
            txtIdAroma.Text = "";
            txtNamaAroma.Text = "";
            txtDeskripsi.Text = "";
            oldNamaAroma = "";
            oldDeskripsi = "";
        }

        private void btnKEMBALI_Click(object sender, EventArgs e)
        {
            HalamanMenu HalamanMenuForm = new HalamanMenu();
            HalamanMenuForm.Show();
            this.Hide();
            
        }

        private void dgvAromaParfum_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvAromaParfum.Rows.Count)
            {
                DataGridViewRow row = dgvAromaParfum.Rows[e.RowIndex];

                txtIdAroma.Text = row.Cells["ID_Aroma"]?.Value?.ToString() ?? "";
                txtNamaAroma.Text = row.Cells["Nama_Aroma"]?.Value?.ToString() ?? "";
                txtDeskripsi.Text = row.Cells["Deskripsi"]?.Value?.ToString() ?? "";

                oldNamaAroma = txtNamaAroma.Text;
                oldDeskripsi = txtDeskripsi.Text;
            }
        }

        private void AROMA_PARFUM_Load(object sender, EventArgs e)
        {
            // Any additional setup when the form loads
        }

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

        private void PreviewExcelData(string filePath)
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
                            if (dt.Columns.Contains(columnName))
                            {
                                int i = 1;
                                while (dt.Columns.Contains(columnName + "_" + i))
                                {
                                    i++;
                                }
                                columnName += "_" + i;
                            }
                            dt.Columns.Add(columnName);
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
                                        newRow[j] = cell.ToString();
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

                    // Pastikan Anda memiliki form bernama 'PreviewDataAroma' untuk menampilkan data ini
                    // Jika belum ada, Anda bisa membuat form baru atau menggunakan form generik
                    using (PreviewDataAroma previewForm = new PreviewDataAroma(dt))
                    {
                        previewForm.ShowDialog();
                    }
                    InvalidateAromaCache();
                    LoadCachedData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal membaca file Excel: " + ex.Message, "Error Import Excel", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogError(ex, "Preview Excel Data Aroma Parfum");
            }
        }

        private void btnAnalisis_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
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
                        t.name = 'Aroma_Parfum' AND ind.is_primary_key = 0 AND ind.[type] <> 0
                    ORDER BY
                        t.name, ind.name, ind.index_id;";

                    SqlCommand cmd = new SqlCommand(indexQuery, conn);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dtIndex = new DataTable();
                    da.Fill(dtIndex);

                    // *** PERUBAHAN UTAMA DI SINI ***
                    // Menggunakan Stored Procedure GetAllAromas untuk pengukuran waktu eksekusi
                    SqlCommand cmd2 = new SqlCommand("GetAllAromas", conn);
                    cmd2.CommandType = CommandType.StoredProcedure; // Penting: Menentukan CommandType

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
                    infoBuilder.AppendLine($"📌 Waktu Eksekusi Query (GetAllAromas): {execTimeMs} ms\n");
                    infoBuilder.AppendLine("📦 Indeks pada Aroma_Parfum:");

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
    }
}