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
using System.Text.RegularExpressions; // Tambahkan ini untuk Regex


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
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }
                string query = "SELECT * FROM Pegawai";
                SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                dgvPegawai.DataSource = dt;

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
            if (cachePegawai == null)
            {
                cachePegawai = new DataTable();
                try
                {
                    using (var newConn = new SqlConnection(connectionString))
                    {
                        newConn.Open();
                        using (var da = new SqlDataAdapter("SELECT * FROM Pegawai", newConn))
                        {
                            da.Fill(cachePegawai);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal memuat data cache: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            dgvPegawai.DataSource = cachePegawai;
        }

        private string connectionString = "Data Source=LAPTOP-T1UUTAE0\\HIKMATYAR;Initial Catalog=Manajemen_Penjualan_Parfum;Integrated Security=True";

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

            // Client-side validation for ID_Pegawai format: must start with '03' and be 3 or 4 characters long
            // Using Regex for more robust checking.
            // ^03 : start with '03'
            // .{1,2} : followed by 1 or 2 any characters
            // $ : end of string
            if (!Regex.IsMatch(txtIdPegawai.Text.Trim(), @"^03.{1,2}$"))
            {
                MessageBox.Show("ID Pegawai harus dimulai dengan '03' dan memiliki panjang total 3 atau 4 karakter.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtIdPegawai.Focus();
                return;
            }


            // Client-side validation for Nama_Pegawai (based on trigger)
            if (Regex.IsMatch(txtNama.Text, @"[^A-Za-z ]"))
            {
                MessageBox.Show("Nama Pegawai hanya boleh berisi huruf dan spasi.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNama.Focus();
                return;
            }


            using (var newConn = new SqlConnection(connectionString))
            {
                newConn.Open();
                using (var tran = newConn.BeginTransaction())
                {
                    try
                    {
                        // Check if ID already exists before attempting insert
                        // TRIM() is used here to handle CHAR(4) padding from DB during comparison.
                        SqlCommand checkCmd = new SqlCommand("SELECT COUNT(1) FROM Pegawai WHERE ID_Pegawai = @ID", newConn, tran);
                        checkCmd.Parameters.AddWithValue("@ID", txtIdPegawai.Text.Trim()); // Use Trim() for comparison
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
                        cmd.Parameters.AddWithValue("@ID", txtIdPegawai.Text.Trim()); // Kirim data dengan TRIM() agar tidak ada spasi sisa
                        cmd.Parameters.AddWithValue("@Nama", txtNama.Text);
                        cmd.ExecuteNonQuery();

                        tran.Commit();
                        MessageBox.Show("Berhasil Menambah Data", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        cachePegawai = null;
                        LoadCachedData();
                        ClearInputFields();
                    }
                    catch (SqlException ex)
                    {
                        tran.Rollback();
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
                            case 547: // This would still catch if the CHECK constraint on SQL side fires after trimming.
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
                using (var newConn = new SqlConnection(connectionString))
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
                            cmd.Parameters.AddWithValue("@ID", txtIdPegawai.Text.Trim()); // Use Trim() for consistency
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
                                cachePegawai = null;
                                LoadCachedData();
                                ClearInputFields();
                            }
                        }
                        catch (SqlException ex)
                        {
                            tran.Rollback();
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

            // Client-side validation for Nama_Pegawai (based on trigger)
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
                using (var newConn = new SqlConnection(connectionString))
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
                            cmd.Parameters.AddWithValue("@ID", txtIdPegawai.Text.Trim()); // Use Trim() for consistency
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
                                cachePegawai = null;
                                LoadCachedData();
                                ClearInputFields();
                            }
                        }
                        catch (SqlException ex)
                        {
                            tran.Rollback();
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
            cachePegawai = null;
            LoadCachedData();
            ClearInputFields();
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
                // When reading from CHAR(N) column, it might have trailing spaces. Trim it.
                txtIdPegawai.Text = row.Cells["ID_Pegawai"].Value.ToString().Trim();
                txtNama.Text = row.Cells["Nama_Pegawai"].Value.ToString();

                oldNamaPegawai = txtNama.Text;
            }
        }

        private void PEGAWAI_Load(object sender, EventArgs e)
        {
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

                    PreviewDataPelanggan preview = new PreviewDataPelanggan(dt);
                    preview.ShowDialog();
                    cachePegawai = null;
                    LoadCachedData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal membaca file Excel: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                using (SqlConnection newConn = new SqlConnection(connectionString))
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

                    string selectQuery = "SELECT * FROM Pegawai";
                    SqlCommand cmd2 = new SqlCommand(selectQuery, newConn);

                    var stopwatch = new System.Diagnostics.Stopwatch();
                    stopwatch.Start();
                    cmd2.ExecuteReader().Close();
                    stopwatch.Stop();
                    long execTimeMs = stopwatch.ElapsedMilliseconds;

                    string info = $"📌 Waktu Eksekusi Query: {execTimeMs} ms\n\n📦 Indeks:\n";

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
            }
        }
    }
}