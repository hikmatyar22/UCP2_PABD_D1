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
    public partial class RACIKAN_PARFUM : Form
    {
        // Use a single connection string for consistency
        private readonly string connectionString = "Data Source=LAPTOP-T1UUTAE0\\HIKMATYAR;Initial Catalog=Manajemen_Penjualan_Parfum;Integrated Security=True";

        // Cache for storing Racikan_Parfum data to improve performance
        private DataTable cacheRacikanParfum = null;
        // Stores the old Racikan ID for change detection during updates (though ID is usually fixed)
        private string oldIdRacikan = "";

        public RACIKAN_PARFUM()
        {
            InitializeComponent();
            LoadInitialData(); // Initial data display and cache loading
            LoadComboBoxes(); // Load data for ComboBoxes
        }

        /// <summary>
        /// Loads initial data into DataGridView and populates the cache.
        /// </summary>
        private void LoadInitialData()
        {
            try
            {
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
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data awal: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Populates all ComboBoxes (ID_Pelanggan, ID_Aroma, ID_Pegawai, Nama_Aroma, Perbandingan).
        /// </summary>
        private void LoadComboBoxes()
        {
            LoadIdPelanggan();
            LoadIdAroma();
            LoadIdPegawai();
            LoadNamaAroma();
            LoadPerbandingan();
        }

        private void LoadIdPelanggan()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT ID_Pelanggan FROM Pelanggan";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        cmbIdPelanggan.Items.Clear();
                        while (reader.Read())
                        {
                            cmbIdPelanggan.Items.Add(reader["ID_Pelanggan"].ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal memuat ID pelanggan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LoadIdAroma()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT ID_Aroma FROM Aroma_Parfum";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        cmbIdAroma.Items.Clear();
                        while (reader.Read())
                        {
                            cmbIdAroma.Items.Add(reader["ID_Aroma"].ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal memuat ID aroma: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LoadIdPegawai()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT ID_Pegawai FROM Pegawai";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        cmbIdPegawai.Items.Clear();
                        while (reader.Read())
                        {
                            cmbIdPegawai.Items.Add(reader["ID_Pegawai"].ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal memuat ID pegawai: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LoadNamaAroma()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT Nama_Aroma FROM Aroma_Parfum";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        cmbNamaAroma.Items.Clear();
                        while (reader.Read())
                        {
                            cmbNamaAroma.Items.Add(reader["Nama_Aroma"].ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal memuat Nama Aroma: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LoadPerbandingan()
        {
            cmbPerbandingan.Items.Clear();
            cmbPerbandingan.Items.Add("1:1");
            cmbPerbandingan.Items.Add("1:2");
            cmbPerbandingan.Items.Add("1:3");
            cmbPerbandingan.Items.Add("2:2");
            cmbPerbandingan.Items.Add("2:3");
        }

        /// <summary>
        /// Handles the click event for the "Tambah" (Add) button.
        /// Adds a new Racikan_Parfum record to the database using a stored procedure.
        /// </summary>
        private void btnTambah_Click(object sender, EventArgs e)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(txtIdRacikan.Text) ||
                cmbIdPelanggan.SelectedItem == null ||
                cmbIdAroma.SelectedItem == null ||
                cmbIdPegawai.SelectedItem == null ||
                cmbPerbandingan.SelectedItem == null ||
                string.IsNullOrWhiteSpace(cmbNamaAroma.Text) ||
                string.IsNullOrWhiteSpace(txtNamaHasilParfum.Text))
            {
                MessageBox.Show("Semua kolom harus diisi.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction()) // Use transaction for atomicity
                {
                    try
                    {
                        SqlCommand cmd = new SqlCommand("sp_TambahRacikanParfum", conn, tran)
                        {
                            CommandType = CommandType.StoredProcedure
                        };
                        cmd.Parameters.AddWithValue("@ID_Pelanggan", cmbIdPelanggan.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@ID_Aroma", cmbIdAroma.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@ID_Pegawai", cmbIdPegawai.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@ID_Racikan", txtIdRacikan.Text);
                        cmd.Parameters.AddWithValue("@Perbandingan", cmbPerbandingan.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@Nama_Racikan", cmbNamaAroma.Text); // Use Text for user input or selected item
                        cmd.Parameters.AddWithValue("@Nama_Hasil_Parfum", txtNamaHasilParfum.Text);
                        cmd.ExecuteNonQuery();

                        tran.Commit(); // Commit the transaction on success
                        MessageBox.Show("Berhasil Menambah Data Racikan Parfum", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        cacheRacikanParfum = null; // Invalidate the cache to reload fresh data
                        LoadInitialData(); // Reload cached data
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback(); // Rollback the transaction on error
                        MessageBox.Show("Gagal Menambah Data Racikan Parfum: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            // Input validation
            if (string.IsNullOrWhiteSpace(txtIdRacikan.Text))
            {
                MessageBox.Show("ID Racikan harus diisi untuk menghapus.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Confirmation dialog
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
                            cmd.Parameters.AddWithValue("@ID_Racikan", txtIdRacikan.Text);
                            cmd.ExecuteNonQuery();

                            tran.Commit();
                            MessageBox.Show("Berhasil Menghapus Data Racikan Parfum", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            cacheRacikanParfum = null;
                            LoadInitialData();
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            MessageBox.Show("Gagal Menghapus Data Racikan Parfum: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            // Input validation
            if (string.IsNullOrWhiteSpace(txtIdRacikan.Text) ||
                cmbIdPelanggan.SelectedItem == null ||
                cmbIdAroma.SelectedItem == null ||
                cmbIdPegawai.SelectedItem == null ||
                cmbPerbandingan.SelectedItem == null ||
                string.IsNullOrWhiteSpace(cmbNamaAroma.Text) ||
                string.IsNullOrWhiteSpace(txtNamaHasilParfum.Text))
            {
                MessageBox.Show("Semua kolom harus diisi.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Confirmation dialog
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
                            cmd.Parameters.AddWithValue("@ID_Racikan", txtIdRacikan.Text);
                            cmd.Parameters.AddWithValue("@ID_Pelanggan", cmbIdPelanggan.SelectedItem.ToString());
                            cmd.Parameters.AddWithValue("@ID_Aroma", cmbIdAroma.SelectedItem.ToString());
                            cmd.Parameters.AddWithValue("@ID_Pegawai", cmbIdPegawai.SelectedItem.ToString());
                            cmd.Parameters.AddWithValue("@Perbandingan", cmbPerbandingan.SelectedItem.ToString());
                            cmd.Parameters.AddWithValue("@Nama_Racikan", cmbNamaAroma.Text);
                            cmd.Parameters.AddWithValue("@Nama_Hasil_Parfum", txtNamaHasilParfum.Text);
                            cmd.ExecuteNonQuery();

                            tran.Commit();
                            MessageBox.Show("Berhasil Mengupdate Data Racikan Parfum", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            cacheRacikanParfum = null;
                            LoadInitialData();
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            MessageBox.Show("Gagal Mengupdate Data Racikan Parfum: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            cacheRacikanParfum = null; // Clear the cache to force a fresh load
            LoadInitialData(); // Re-populate the cache and display data
            LoadComboBoxes(); // Also refresh dropdowns if needed (e.g., new customers/aromas added)
        }

        /// <summary>
        /// Handles the CellClick event of the DataGridView.
        /// Populates the text boxes and combo boxes with the data from the selected row.
        /// </summary>
        private void dgvRacikanParfum_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0 && dgvRacikanParfum.Rows[e.RowIndex].Cells.Count > 0)
                {
                    DataGridViewRow row = dgvRacikanParfum.Rows[e.RowIndex];

                    txtIdRacikan.Text = row.Cells["ID_Racikan"]?.Value?.ToString() ?? "";
                    // Set selected item for ComboBoxes
                    cmbIdPelanggan.SelectedItem = row.Cells["ID_Pelanggan"]?.Value?.ToString();
                    cmbIdAroma.SelectedItem = row.Cells["ID_Aroma"]?.Value?.ToString();
                    cmbIdPegawai.SelectedItem = row.Cells["ID_Pegawai"]?.Value?.ToString();
                    cmbPerbandingan.SelectedItem = row.Cells["Perbandingan"]?.Value?.ToString();
                    cmbNamaAroma.Text = row.Cells["Nama_Racikan"]?.Value?.ToString() ?? ""; // Use Text for ComboBox with editable style
                    txtNamaHasilParfum.Text = row.Cells["Nama_Hasil_Parfum"]?.Value?.ToString() ?? "";

                    // Store the old ID for update change detection (if needed, though ID is usually primary key)
                    oldIdRacikan = txtIdRacikan.Text;
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
        private void Import_Click(object sender, EventArgs e)
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
                        if (row == null) continue;

                        DataRow newRow = dt.NewRow();
                        for (int j = 0; j < dt.Columns.Count; j++)
                        {
                            newRow[j] = row.GetCell(j)?.ToString() ?? "";
                        }
                        dt.Rows.Add(newRow);
                    }

                    // Assuming you have a PreviewDataRacikan form for displaying imported data
                    // You might need to create this form similarly to PreviewDataPelanggan
                    // Make sure PreviewDataRacikan constructor accepts a DataTable
                    PreviewDataRacikan preview = new PreviewDataRacikan(dt);
                    preview.ShowDialog();
                    cacheRacikanParfum = null; // Invalidate cache after potential import
                    LoadInitialData(); // Refresh data after import preview
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal membaca file Excel: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                        t.name = 'Racikan_Parfum' AND ind.is_primary_key = 0 AND ind.[type] <> 0
                    ORDER BY
                        t.name, ind.name, ind.index_id;";

                    SqlCommand cmd = new SqlCommand(indexQuery, conn);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dtIndex = new DataTable();
                    da.Fill(dtIndex);

                    // 2. Measure query execution time
                    string selectQuery = "SELECT * FROM Racikan_Parfum";
                    SqlCommand cmd2 = new SqlCommand(selectQuery, conn);
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    cmd2.ExecuteReader().Close(); // Just execute and close to measure time
                    stopwatch.Stop();

                    long execTimeMs = stopwatch.ElapsedMilliseconds;
                    string info = $"📌 Waktu Eksekusi Query: {execTimeMs} ms\n\n📦 Indeks:\n";

                    // 3. Display results
                    if (dtIndex.Rows.Count > 0)
                    {
                        foreach (DataRow row in dtIndex.Rows)
                        {
                            info += $"- Tabel: {row["TableName"]}, Index: {row["IndexName"]}, Tipe: {row["IndexType"]}, Kolom: {row["ColumnName"]}\n";
                        }
                    }
                    else
                    {
                        info += "Tidak ada indeks non-primary key ditemukan untuk tabel Racikan_Parfum.\n";
                    }

                    MessageBox.Show(info, "Analisis Query & Index Racikan Parfum", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        private void btnKEMBALI_Click(object sender, EventArgs e)
        {
            HalamanMenu HalamanMenuForm = new HalamanMenu();
            HalamanMenuForm.Show();
            this.Hide();
        }

        // --- Event handlers from your previous code (consider if still needed) ---
        private void dgvRacikanParfum_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Empty, can be removed.
        }

        private void RACIKAN_PARFUM_Load(object sender, EventArgs e)
        {
            // Empty, can be removed if no specific load logic is needed.
        }

        private void label2_Click(object sender, EventArgs e)
        {
            // Empty, can be removed.
        }

        private void cmbPerbandingan_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Empty, can be removed if no specific logic needed on selection change.
        }
    }
}