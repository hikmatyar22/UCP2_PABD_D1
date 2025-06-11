using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace HOMEPAGE
{
    public partial class PELANGGAN : Form
    {
        // Use a single connection string for consistency
        private readonly string connectionString = "Data Source=LAPTOP-T1UUTAE0\\HIKMATYAR;Initial Catalog=Manajemen_Penjualan_Parfum;Integrated Security=True";

        // Cache for storing customer data to improve performance
        private DataTable cachePelanggan = null;
        // Stores the old customer name for change detection during updates
        private string oldNamaPelanggan = "";

        public PELANGGAN()
        {
            InitializeComponent();
            TampilData(); // Initial data display
            LoadCachedData(); // Load data into the cache
        }

        /// <summary>
        /// Displays customer data in the DataGridView.
        /// </summary>
        void TampilData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString)) // Use 'using' for proper resource management
                {
                    conn.Open();
                    string query = "SELECT * FROM Pelanggan";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgvPelanggan.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menampilkan data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Loads customer data into a cache (DataTable) for quicker access.
        /// </summary>
        private void LoadCachedData()
        {
            // Only load if the cache is empty
            if (cachePelanggan == null)
            {
                cachePelanggan = new DataTable();
                using (SqlConnection conn = new SqlConnection(connectionString)) // Use 'using' for proper resource management
                {
                    using (var da = new SqlDataAdapter("SELECT * FROM Pelanggan", conn))
                    {
                        da.Fill(cachePelanggan);
                    }
                }
            }
            dgvPelanggan.DataSource = cachePelanggan;
        }

        /// <summary>
        /// Handles the click event for the "Tambah" (Add) button.
        /// Adds a new customer record to the database.
        /// </summary>
        private void btnTambah_Click(object sender, EventArgs e)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(txtIdPelanggan.Text) || string.IsNullOrWhiteSpace(txtNama.Text) || string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("ID, Nama, dan Email harus diisi.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction()) // Use 'using' for transaction management
                {
                    try
                    {
                        SqlCommand cmd = new SqlCommand("sp_TambahPelanggan", conn, tran)
                        {
                            CommandType = CommandType.StoredProcedure
                        };
                        cmd.Parameters.AddWithValue("@ID_Pelanggan", txtIdPelanggan.Text);
                        cmd.Parameters.AddWithValue("@Nama", txtNama.Text);
                        cmd.Parameters.AddWithValue("@Email", txtEmail.Text);
                        cmd.ExecuteNonQuery();

                        tran.Commit(); // Commit the transaction on success
                        MessageBox.Show("Berhasil Menambah Data", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        cachePelanggan = null; // Invalidate the cache to reload fresh data
                        LoadCachedData(); // Reload cached data
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback(); // Rollback the transaction on error
                        MessageBox.Show("Gagal Menambah Data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the click event for the "Hapus" (Delete) button.
        /// Deletes a customer record from the database.
        /// </summary>
        private void btnHapus_Click(object sender, EventArgs e)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(txtIdPelanggan.Text))
            {
                MessageBox.Show("ID harus diisi untuk menghapus.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Confirmation dialog
            if (MessageBox.Show("Apakah Anda yakin ingin menghapus data ini?", "Konfirmasi Hapus", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
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
                            // FIX APPLIED HERE: Changed "@ID" to "@ID_Pelanggan" to match the stored procedure's expected parameter name
                            cmd.Parameters.AddWithValue("@ID_Pelanggan", txtIdPelanggan.Text);
                            cmd.ExecuteNonQuery();

                            tran.Commit();
                            MessageBox.Show("Berhasil Menghapus Data", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            cachePelanggan = null;
                            LoadCachedData();
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
        /// Updates an existing customer record in the database.
        /// </summary>
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(txtIdPelanggan.Text) || string.IsNullOrWhiteSpace(txtNama.Text) || string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("ID, Nama, dan Email harus diisi.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check for actual changes
            if (txtNama.Text == oldNamaPelanggan)
            {
                MessageBox.Show("Tidak ada perubahan yang terdeteksi.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Confirmation dialog
            if (MessageBox.Show("Apakah Anda yakin ingin mengupdate data ini?", "Konfirmasi Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlTransaction tran = conn.BeginTransaction())
                    {
                        try
                        {
                            SqlCommand cmd = new SqlCommand("sp_UpdatePelanggan", conn, tran)
                            {
                                CommandType = CommandType.StoredProcedure
                            };
                            cmd.Parameters.AddWithValue("@ID_Pelanggan", txtIdPelanggan.Text);
                            cmd.Parameters.AddWithValue("@Nama", txtNama.Text);
                            cmd.Parameters.AddWithValue("@Email", txtEmail.Text);
                            cmd.ExecuteNonQuery();

                            tran.Commit();
                            MessageBox.Show("Berhasil Mengupdate Data", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            cachePelanggan = null;
                            LoadCachedData();
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
        /// Reloads the data from the database.
        /// </summary>
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            cachePelanggan = null; // Clear the cache to force a fresh load
            TampilData();
            LoadCachedData(); // Re-populate the cache
        }

        /// <summary>
        /// Handles the CellClick event of the DataGridView.
        /// Populates the text boxes with the data from the selected row.
        /// </summary>
        private void dgvPelanggan_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0 && dgvPelanggan.Rows[e.RowIndex].Cells.Count > 0)
                {
                    DataGridViewRow row = dgvPelanggan.Rows[e.RowIndex];

                    // CORRECTED: Changed "Nama_Pelanggan" to "Nama" based on DataGridView column headers
                    txtIdPelanggan.Text = row.Cells["ID_Pelanggan"]?.Value?.ToString() ?? "";
                    txtNama.Text = row.Cells["Nama"]?.Value?.ToString() ?? "";
                    txtEmail.Text = row.Cells["Email"]?.Value?.ToString() ?? "";

                    // Store the old name for update change detection
                    oldNamaPelanggan = txtNama.Text;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memilih data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles the click event for the "Import" button.
        /// Opens a file dialog to select an Excel file and previews its data.
        /// </summary>
        private void import_Click_1(object sender, EventArgs e)
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
                            dt.Columns.Add(cell.ToString());
                        }
                    }

                    // Read data rows
                    for (int i = 1; i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row == null) continue; // Skip empty rows

                        DataRow newRow = dt.NewRow();
                        for (int j = 0; j < dt.Columns.Count; j++) // Iterate based on DataTable columns, not header cells
                        {
                            newRow[j] = row.GetCell(j)?.ToString() ?? ""; // Handle null cells gracefully
                        }
                        dt.Rows.Add(newRow);
                    }

                    // Assuming you have a PreviewDataPelanggan form for displaying imported data
                    PreviewDataPelanggan preview = new PreviewDataPelanggan(dt);
                    preview.ShowDialog();
                    TampilData(); // Refresh data after import preview
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal membaca file Excel: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // 1. Get index information
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
                        t.name = 'Pelanggan' AND ind.is_primary_key = 0 AND ind.[type] <> 0
                    ORDER BY
                        t.name, ind.name, ind.index_id;";

                    SqlCommand cmd = new SqlCommand(indexQuery, conn);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dtIndex = new DataTable();
                    da.Fill(dtIndex);

                    // 2. Measure query execution time
                    string selectQuery = "SELECT * FROM Pelanggan";
                    SqlCommand cmd2 = new SqlCommand(selectQuery, conn);
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    cmd2.ExecuteReader().Close(); // Just execute and close to measure time
                    stopwatch.Stop();

                    long execTimeMs = stopwatch.ElapsedMilliseconds;
                    string info = $"📌 Waktu Eksekusi Query: {execTimeMs} ms\n\n📦 Indeks:\n";

                    // 3. Display results
                    foreach (DataRow row in dtIndex.Rows)
                    {
                        info += $"- Tabel: {row["TableName"]}, Index: {row["IndexName"]}, Tipe: {row["IndexType"]}, Kolom: {row["ColumnName"]}\n";
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
            this.Hide();
            
        }

        // This event handler is empty and can be removed if not needed.
        private void dgvPelanggan_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // No implementation needed based on the pegawai code, can be removed if not used.
        }

        private void PELANGGAN_Load(object sender, EventArgs e)
        {

        }
    }
}