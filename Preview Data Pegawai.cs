using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace HOMEPAGE
{
    public partial class PreviewDataPegawai : Form
    {
        private string connectionString = "Data Source=LAPTOP-T1UUTAE0\\HIKMATYAR;Initial Catalog=Manajemen_Penjualan_Parfum;Integrated Security=True";

        public PreviewDataPegawai(DataTable data)
        {
            InitializeComponent();
            dgvPreviewPegawai.DataSource = data;
        }

        private void PreviewDataPegawai_Load(object sender, EventArgs e)
        {
            dgvPreviewPegawai.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Apakah Anda ingin mengimpor data pegawai ini ke database?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                ImportDataToDatabase();
            }
        }

        private bool ValidateRow(DataRow row)
        {
            string id = row["ID_Pegawai"].ToString();
            string nama = row["Nama_Pegawai"].ToString();

            if (string.IsNullOrWhiteSpace(id) || id.Length != 4)
            {
                MessageBox.Show("ID Pegawai harus terdiri dari 4 karakter.", "Kesalahan Validasi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(nama))
            {
                MessageBox.Show("Nama Pegawai tidak boleh kosong.", "Kesalahan Validasi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private void ImportDataToDatabase()
        {
            try
            {
                DataTable dt = (DataTable)dgvPreviewPegawai.DataSource;

                foreach (DataRow row in dt.Rows)
                {
                    if (!ValidateRow(row))
                        continue;

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();

                        using (SqlCommand cmd = new SqlCommand("TambahPegawai", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@ID", row["ID_Pegawai"]);
                            cmd.Parameters.AddWithValue("@Nama", row["Nama_Pegawai"]);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                MessageBox.Show("Data pegawai berhasil diimpor ke database.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Terjadi kesalahan saat mengimpor data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvPreview_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Optional: fungsi klik cell
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Oke_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}