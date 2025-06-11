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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace HOMEPAGE
{
    public partial class PEGAWAI : Form
    {
        SqlConnection conn = new SqlConnection("Data Source=LAPTOP-T1UUTAE0\\HIKMATYAR;Initial Catalog=Manajemen_Penjualan_Parfum;Integrated Security=True");

        public PEGAWAI()
        {
            InitializeComponent();
            TampilData();
            LoadCachedData();
        }

        DataTable cachePegawai = null;
        private string oldNamaPegawai = "";


        void TampilData()
        {
            try
            {
                conn.Open();
                string query = "SELECT * FROM Pegawai";
                SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                dgvPegawai.DataSource = dt;
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menampilkan data: " + ex.Message);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
                
            }
        }

        private void LoadCachedData()
        {
            if (cachePegawai == null)
            {
                cachePegawai = new DataTable();
                using (var da = new SqlDataAdapter("SELECT * FROM Pegawai", conn))
                    da.Fill(cachePegawai);
            }
            dgvPegawai.DataSource = cachePegawai;
        }


        // Gantilah dengan connection string milikmu
        private string connectionString = "Data Source=LAPTOP-T1UUTAE0\\HIKMATYAR;Initial Catalog=Manajemen_Penjualan_Parfum;Integrated Security=True";

        private void btnTambah_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtIdPegawai.Text) || string.IsNullOrWhiteSpace(txtNama.Text))
            {
                MessageBox.Show("ID dan Nama harus diisi.");
                return;
            }

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        var cmd = new SqlCommand("sp_TambahPegawai", conn, tran)
                        {
                            CommandType = CommandType.StoredProcedure
                        };
                        cmd.Parameters.AddWithValue("@ID", txtIdPegawai.Text);
                        cmd.Parameters.AddWithValue("@Nama", txtNama.Text);
                        cmd.ExecuteNonQuery();

                        tran.Commit();
                        MessageBox.Show("Berhasil Menambah Data");
                        cachePegawai = null;
                        LoadCachedData();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        MessageBox.Show("Gagal Menambah Data: " + ex.Message);
                    }
                }
            }
        }

        private void btnHapus_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtIdPegawai.Text))
            {
                MessageBox.Show("ID harus diisi untuk menghapus.");
                return;
            }

            if (MessageBox.Show("Apakah Anda yakin ingin menghapus data ini?", "Konfirmasi", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            var cmd = new SqlCommand("sp_HapusPegawai", conn, tran)
                            {
                                CommandType = CommandType.StoredProcedure
                            };
                            cmd.Parameters.AddWithValue("@ID", txtIdPegawai.Text);
                            cmd.ExecuteNonQuery();

                            tran.Commit();
                            MessageBox.Show("Berhasil Menghapus Data");
                            cachePegawai = null;
                            LoadCachedData();
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            MessageBox.Show("Gagal Menghapus Data: " + ex.Message);
                        }
                    }
                }
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtIdPegawai.Text) || string.IsNullOrWhiteSpace(txtNama.Text))
            {
                MessageBox.Show("ID dan Nama harus diisi.");
                return;
            }

            if (txtNama.Text == oldNamaPegawai)
            {
                MessageBox.Show("Tidak Ada Perubahan");
                return;
            }

            if (MessageBox.Show("Apakah Anda yakin ingin mengupdate data ini?", "Konfirmasi", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            var cmd = new SqlCommand("sp_UpdatePegawai", conn, tran)
                            {
                                CommandType = CommandType.StoredProcedure
                            };
                            cmd.Parameters.AddWithValue("@ID", txtIdPegawai.Text);
                            cmd.Parameters.AddWithValue("@Nama", txtNama.Text);
                            cmd.ExecuteNonQuery();

                            tran.Commit();
                            MessageBox.Show("Berhasil Mengupdate Data");
                            cachePegawai = null;
                            LoadCachedData();
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            MessageBox.Show("Gagal Mengupdate Data: " + ex.Message);
                        }
                    }
                }
            }
        }


        private void btnRefresh_Click(object sender, EventArgs e)
        {
           TampilData();
        }

        private void btnKEMBALI_Click(object sender, EventArgs e)
        {
            HalamanMenu HalamanMenuForm = new HalamanMenu();
            HalamanMenuForm.Show();
            this.Hide();
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void dgvPegawai_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            
        }

        private void dgvPegawai_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvPegawai.Rows[e.RowIndex];
                txtIdPegawai.Text = row.Cells["ID_Pegawai"].Value.ToString();
                txtNama.Text = row.Cells["Nama_Pegawai"].Value.ToString();

                // Simpan data lama untuk pengecekan perubahan
                oldNamaPegawai = txtNama.Text;
            }
        }

        private void PEGAWAI_Load(object sender, EventArgs e)
        {

        }


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

        private void btnAnalisis_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // 1. Ambil informasi index dari sys.indexes dan sys.tables
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

                    SqlCommand cmd = new SqlCommand(indexQuery, conn);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dtIndex = new DataTable();
                    da.Fill(dtIndex);

                    // 2. Ukur waktu eksekusi query sederhana sebagai bentuk analisis performa
                    string selectQuery = "SELECT * FROM Pegawai";
                    SqlCommand cmd2 = new SqlCommand(selectQuery, conn);

                    var stopwatch = new System.Diagnostics.Stopwatch();
                    stopwatch.Start();
                    cmd2.ExecuteReader().Close();  // kita hanya ukur waktu, tidak perlu ambil data
                    stopwatch.Stop();
                    long execTimeMs = stopwatch.ElapsedMilliseconds;

                    // 3. Tampilkan hasil
                    string info = $"📌 Waktu Eksekusi Query: {execTimeMs} ms\n\n📦 Indeks:\n";

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
    }
}
