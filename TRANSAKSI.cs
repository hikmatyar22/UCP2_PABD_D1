using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Linq; // Pastikan ini ada untuk metode .Cast<T>().FirstOrDefault()

namespace HOMEPAGE
{
    public partial class TRANSAKSI : Form
    {
        // Gunakan satu connection string untuk konsistensi dan manajemen sumber daya yang tepat
        private readonly string connectionString = "Data Source=LAPTOP-T1UUTAE0\\HIKMATYAR;Initial Catalog=Manajemen_Penjualan_Parfum;Integrated Security=True";

        // Cache untuk menyimpan data transaksi guna meningkatkan kinerja
        private DataTable cacheTransaksi = null;
        // Menyimpan tanggal dan total harga lama untuk deteksi perubahan saat update
        private DateTime oldTanggalTransaksi;
        private decimal oldTotalHarga;

        public TRANSAKSI()
        {
            InitializeComponent();
            TampilData(); // Menampilkan data awal
            LoadCachedData(); // Memuat data ke dalam cache
            LoadIdPelanggan(); // Memuat ID pelanggan ke dalam ComboBox

            // Set format DateTimePicker saat inisialisasi
            dtpTanggalTransaksi.Format = DateTimePickerFormat.Custom;
            dtpTanggalTransaksi.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            dtpTanggalTransaksi.ShowUpDown = true; // Opsi untuk menampilkan tombol naik/turun untuk waktu
        }

        /// <summary>
        /// Menampilkan data transaksi di DataGridView.
        /// Metode ini terutama untuk pemuatan awal dan refresh penuh jika diperlukan.
        /// Untuk data yang di-cache, LoadCachedData() lebih disukai.
        /// </summary>
        void TampilData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString)) // Gunakan 'using' untuk manajemen sumber daya yang tepat
                {
                    conn.Open();
                    string query = "SELECT ID_Transaksi, ID_Pelanggan, Tanggal_Transaksi, Total_Harga FROM Transaksi";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgvTransaksi.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menampilkan data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Memuat data transaksi ke dalam cache (DataTable) untuk akses yang lebih cepat.
        /// </summary>
        private void LoadCachedData()
        {
            // Hanya muat jika cache kosong atau perlu di-refresh
            if (cacheTransaksi == null)
            {
                cacheTransaksi = new DataTable();
                using (SqlConnection conn = new SqlConnection(connectionString)) // Gunakan 'using' untuk manajemen sumber daya yang tepat
                {
                    using (var da = new SqlDataAdapter("SELECT ID_Transaksi, ID_Pelanggan, Tanggal_Transaksi, Total_Harga FROM Transaksi", conn))
                    {
                        da.Fill(cacheTransaksi);
                    }
                }
            }
            dgvTransaksi.DataSource = cacheTransaksi;
        }

        /// <summary>
        /// Memuat ID_Pelanggan ke dalam ComboBox.
        /// </summary>
        private void LoadIdPelanggan()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString)) // Gunakan 'using' untuk manajemen sumber daya yang tepat
                {
                    conn.Open();
                    string query = "SELECT ID_Pelanggan FROM Pelanggan ORDER BY ID_Pelanggan"; // Tambahkan ORDER BY untuk tampilan yang rapi
                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    cmbIdPelanggan.Items.Clear();
                    while (reader.Read())
                    {
                        cmbIdPelanggan.Items.Add(reader["ID_Pelanggan"].ToString());
                    }
                } // Koneksi otomatis ditutup oleh 'using'
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat ID pelanggan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Menangani event klik untuk tombol "Tambah".
        /// Menambahkan catatan transaksi baru ke database.
        /// </summary>
        private void btnTambah_Click(object sender, EventArgs e)
        {
            // Validasi input
            if (string.IsNullOrWhiteSpace(txtIdTransaksi.Text) ||
                cmbIdPelanggan.SelectedItem == null ||
                string.IsNullOrWhiteSpace(txtTotalHarga.Text))
            {
                MessageBox.Show("ID Transaksi, ID Pelanggan, dan Total Harga harus diisi.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validasi Total Harga sebagai angka desimal yang valid
            if (!decimal.TryParse(txtTotalHarga.Text, out decimal totalHarga))
            {
                MessageBox.Show("Total Harga harus berupa angka yang valid.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction()) // Gunakan 'using' untuk manajemen transaksi
                {
                    try
                    {
                        SqlCommand cmd = new SqlCommand("InsertTransaksi", conn, tran)
                        {
                            CommandType = CommandType.StoredProcedure
                        };
                        cmd.Parameters.AddWithValue("@ID_Transaksi", txtIdTransaksi.Text);
                        cmd.Parameters.AddWithValue("@ID_Pelanggan", cmbIdPelanggan.SelectedItem.ToString());
                        // Karena Tanggal_Transaksi memiliki DEFAULT GETDATE() di SQL,
                        // kita tidak perlu mengirimkannya sebagai parameter jika ingin menggunakan tanggal saat ini.
                        // Jika Anda ingin mengizinkan pengguna memilih tanggal saat tambah,
                        // Anda perlu memodifikasi SP InsertTransaksi untuk menerima @Tanggal_Transaksi.
                        // Untuk saat ini, kita mengandalkan DEFAULT GETDATE() dari SQL.
                        cmd.Parameters.AddWithValue("@Total_Harga", totalHarga);
                        cmd.ExecuteNonQuery();

                        tran.Commit(); // Commit transaksi saat berhasil
                        MessageBox.Show("Berhasil Menambah Data Transaksi", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        cacheTransaksi = null; // Invalidasi cache untuk memuat ulang data baru
                        LoadCachedData(); // Muat ulang data yang di-cache
                        ClearInputFields(); // Bersihkan textbox setelah berhasil menambah
                    }
                    catch (SqlException ex)
                    {
                        tran.Rollback(); // Rollback transaksi saat terjadi kesalahan
                        if (ex.Number == 2627) // Pelanggaran primary key (duplikasi ID)
                        {
                            MessageBox.Show("ID Transaksi sudah ada. Mohon gunakan ID yang berbeda.", "Error Duplikasi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else if (ex.Number == 547) // Foreign key violation (ID Pelanggan tidak ada)
                        {
                            MessageBox.Show("ID Pelanggan tidak ditemukan. Pastikan ID Pelanggan yang dipilih valid.", "Error Data Terkait", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            MessageBox.Show("Gagal Menambah Data Transaksi: " + ex.Message, "Error Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback(); // Rollback transaksi saat terjadi kesalahan
                        MessageBox.Show("Gagal Menambah Data Transaksi: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            // Validasi input
            if (string.IsNullOrWhiteSpace(txtIdTransaksi.Text))
            {
                MessageBox.Show("ID Transaksi harus diisi untuk menghapus.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Dialog konfirmasi
            if (MessageBox.Show("Apakah Anda yakin ingin menghapus data transaksi ini?", "Konfirmasi Hapus", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
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
                            cmd.Parameters.AddWithValue("@ID_Transaksi", txtIdTransaksi.Text);
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                tran.Commit();
                                MessageBox.Show("Berhasil Menghapus Data Transaksi", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                cacheTransaksi = null;
                                LoadCachedData();
                                ClearInputFields();
                            }
                            else
                            {
                                tran.Rollback(); // Rollback jika tidak ada baris yang terpengaruh
                                MessageBox.Show("Data transaksi dengan ID tersebut tidak ditemukan.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            MessageBox.Show("Gagal Menghapus Data Transaksi: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Menangani event klik untuk tombol "Update".
        /// Memperbarui catatan transaksi yang ada di database.
        /// </summary>
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            // Validasi input
            if (string.IsNullOrWhiteSpace(txtIdTransaksi.Text) ||
                string.IsNullOrWhiteSpace(txtTotalHarga.Text)) // Tanggal_Transaksi sekarang dari DateTimePicker
            {
                MessageBox.Show("ID Transaksi dan Total Harga harus diisi.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Tanggal_Transaksi langsung diambil dari DateTimePicker
            DateTime tanggalTransaksi = dtpTanggalTransaksi.Value;

            // Validasi Total Harga sebagai angka desimal yang valid
            if (!decimal.TryParse(txtTotalHarga.Text, out decimal totalHarga))
            {
                MessageBox.Show("Total Harga harus berupa angka yang valid.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Periksa perubahan aktual
            // Bandingkan dengan oldTanggalTransaksi dan oldTotalHarga
            if (tanggalTransaksi == oldTanggalTransaksi && totalHarga == oldTotalHarga)
            {
                MessageBox.Show("Tidak ada perubahan yang terdeteksi.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Dialog konfirmasi
            if (MessageBox.Show("Apakah Anda yakin ingin mengupdate data transaksi ini?", "Konfirmasi Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlTransaction tran = conn.BeginTransaction())
                    {
                        try
                        {
                            SqlCommand cmd = new SqlCommand("UpdateTransaksi", conn, tran)
                            {
                                CommandType = CommandType.StoredProcedure
                            };
                            cmd.Parameters.AddWithValue("@ID_Transaksi", txtIdTransaksi.Text);
                            cmd.Parameters.AddWithValue("@Tanggal_Transaksi", tanggalTransaksi); // Ambil nilai dari DateTimePicker
                            cmd.Parameters.AddWithValue("@Total_Harga", totalHarga);
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                tran.Commit();
                                MessageBox.Show("Berhasil Mengupdate Data Transaksi", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                cacheTransaksi = null;
                                LoadCachedData();
                                ClearInputFields();
                            }
                            else
                            {
                                tran.Rollback(); // Rollback jika tidak ada baris yang terpengaruh
                                MessageBox.Show("Data transaksi dengan ID tersebut tidak ditemukan.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            MessageBox.Show("Gagal Mengupdate Data Transaksi: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Menangani event klik untuk tombol "Refresh".
        /// Memuat ulang data dari database.
        /// </summary>
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            cacheTransaksi = null; // Bersihkan cache untuk memaksa pemuatan ulang
            TampilData(); // Bisa juga langsung LoadCachedData() jika ingin mengandalkan cache
            LoadCachedData(); // Muat ulang data ke DGV dan cache
            ClearInputFields(); // Bersihkan field input saat refresh
        }

        /// <summary>
        /// Menangani event CellClick dari DataGridView.
        /// Mengisi textbox dengan data dari baris yang dipilih.
        /// </summary>
        private void dgvTransaksi_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0 && dgvTransaksi.Rows[e.RowIndex].Cells.Count > 0)
                {
                    DataGridViewRow row = dgvTransaksi.Rows[e.RowIndex];

                    txtIdTransaksi.Text = row.Cells["ID_Transaksi"]?.Value?.ToString() ?? "";

                    string idPelanggan = row.Cells["ID_Pelanggan"]?.Value?.ToString() ?? "";
                    // Pilih item di ComboBox
                    cmbIdPelanggan.SelectedItem = cmbIdPelanggan.Items
                        .Cast<string>()
                        .FirstOrDefault(item => item == idPelanggan);

                    // Ambil nilai tanggal dari DataGridView dan set ke DateTimePicker
                    // Pastikan nilai Tanggal_Transaksi dari DGV dapat di-parse ke DateTime
                    if (DateTime.TryParse(row.Cells["Tanggal_Transaksi"]?.Value?.ToString(), out DateTime parsedDate))
                    {
                        dtpTanggalTransaksi.Value = parsedDate;
                        oldTanggalTransaksi = parsedDate; // Simpan untuk deteksi perubahan
                    }
                    else
                    {
                        dtpTanggalTransaksi.Value = DateTime.Now; // Default jika gagal parse
                        oldTanggalTransaksi = DateTime.MinValue; // Tandai bahwa nilai lama tidak valid
                    }

                    txtTotalHarga.Text = row.Cells["Total_Harga"]?.Value?.ToString() ?? "";

                    // Simpan nilai lama Total_Harga untuk deteksi perubahan
                    if (decimal.TryParse(txtTotalHarga.Text, out oldTotalHarga))
                    {
                        // Nilai berhasil di-parse dan disimpan
                    }
                    else
                    {
                        oldTotalHarga = 0; // Default jika gagal parse
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memilih data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Menangani event klik untuk tombol "Import".
        /// Membuka dialog file untuk memilih file Excel dan menampilkan pratinjaunya.
        /// </summary>
        private void btnImport_Click(object sender, EventArgs e)
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
        /// Membaca data dari file Excel yang ditentukan dan menampilkannya dalam formulir pratinjau.
        /// </summary>
        /// <param name="filePath">Path ke file Excel.</param>
        private void PreviewData(string filePath)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    IWorkbook workbook = new XSSFWorkbook(fs);
                    ISheet sheet = workbook.GetSheetAt(0); // Ambil sheet pertama
                    DataTable dt = new DataTable();

                    // Baca baris header
                    IRow headerRow = sheet.GetRow(0);
                    if (headerRow != null)
                    {
                        foreach (ICell cell in headerRow.Cells)
                        {
                            dt.Columns.Add(cell.ToString());
                        }
                    }

                    // Baca baris data
                    for (int i = 1; i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row == null) continue; // Lewati baris kosong

                        DataRow newRow = dt.NewRow();
                        for (int j = 0; j < dt.Columns.Count; j++)
                        {
                            newRow[j] = row.GetCell(j)?.ToString() ?? ""; // Tangani sel null dengan anggun
                        }
                        dt.Rows.Add(newRow);
                    }

                    // FIX APPLIED HERE: Ensure the class name matches its definition
                    // If your PreviewDataTransaksi.cs file declares 'public partial class PreviewDataTransaki',
                    // then you should change this line to 'PreviewDataTransaki preview = new PreviewDataTransaki(dt);'
                    // However, it's generally recommended to correct the typo in the class declaration itself.
                    PreviewDataTransaksi preview = new PreviewDataTransaksi(dt);
                    preview.ShowDialog();

                    // Setelah form pratinjau ditutup, refresh data utama
                    cacheTransaksi = null; // Invalidasi cache untuk memaksa pemuatan ulang
                    LoadCachedData(); // Muat ulang data ke DGV dan cache
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal membaca file Excel: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Menangani event klik untuk tombol "Analisis".
        /// Melakukan analisis kinerja sederhana dari query SELECT dan menampilkan informasi indeks untuk tabel 'Transaksi'.
        /// </summary>
        private void Analisis_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // 1. Dapatkan informasi indeks
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

                    // 2. Ukur waktu eksekusi query
                    string selectQuery = "SELECT ID_Transaksi, ID_Pelanggan, Tanggal_Transaksi, Total_Harga FROM Transaksi";
                    SqlCommand cmd2 = new SqlCommand(selectQuery, conn);
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    cmd2.ExecuteReader().Close(); // Hanya eksekusi dan tutup untuk mengukur waktu
                    stopwatch.Stop();

                    long execTimeMs = stopwatch.ElapsedMilliseconds;
                    string info = $"📌 Waktu Eksekusi Query: {execTimeMs} ms\n\n📦 Indeks:\n";

                    // 3. Tampilkan hasil
                    if (dtIndex.Rows.Count > 0)
                    {
                        foreach (DataRow row in dtIndex.Rows)
                        {
                            info += $"- Tabel: {row["TableName"]}, Index: {row["IndexName"]}, Tipe: {row["IndexType"]}, Kolom: {row["ColumnName"]}\n";
                        }
                    }
                    else
                    {
                        info += "Tidak ada indeks non-primary key yang ditemukan untuk tabel Transaksi.\n";
                    }

                    MessageBox.Show(info, "Analisis Query & Index Transaksi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menganalisis: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Membersihkan semua field input pada form.
        /// </summary>
        private void ClearInputFields()
        {
            txtIdTransaksi.Clear();
            cmbIdPelanggan.SelectedIndex = -1; // Batalkan pilihan item yang dipilih
            dtpTanggalTransaksi.Value = DateTime.Now; // Set DateTimePicker ke tanggal/waktu saat ini
            txtTotalHarga.Clear();
            txtIdTransaksi.Focus(); // Set fokus kembali ke field ID
        }

        /// <summary>
        /// Menangani event klik untuk tombol "Kembali".
        /// Menutup form saat ini dan membuka form HalamanMenu.
        /// </summary>
        private void btnKEMBALI_Click(object sender, EventArgs e)
        {
            HalamanMenu HalamanMenuForm = new HalamanMenu();
            HalamanMenuForm.Show();
            this.Hide();
        }

        // Event handler kosong yang bisa dihapus jika tidak digunakan
        private void TRANSAKSI_Load(object sender, EventArgs e) { /* Tidak ada implementasi yang diperlukan */ }
        private void dtpTanggalTransaksi_ValueChanged(object sender, EventArgs e) { /* Tidak ada implementasi yang diperlukan */ }
    }
}