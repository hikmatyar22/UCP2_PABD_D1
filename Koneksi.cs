using System;
using System.Net;
using System.Net.Sockets;

namespace HOMEPAGE // Pastikan ini adalah namespace utama aplikasi Anda
{
    internal class koneksi
    {
        public string connectionString() // Ini adalah metode yang akan mengembalikan string koneksi
        {
            string connectStr = "";
            try
            {
                // Mendapatkan alamat IP lokal komputer.
                // Untuk SQL Server yang berjalan di mesin yang sama (misalnya SQL Server Express),
                // ini bisa menjadi cara untuk menghubungkan tanpa hardcode nama server.
                string localIP = GetLocalIPAddress();

                // Perbarui 'Manajemen_Penjualan_Parfum' jika nama database Anda berbeda.
                // 'Integrated Security=True' berarti menggunakan otentikasi Windows.
                connectStr = $"Server={localIP};Initial Catalog=Manajemen_Penjualan_Parfum;Integrated Security=True;";
                return connectStr;
            }
            catch (Exception ex)
            {
                // Menampilkan pesan error di konsol jika terjadi masalah
                Console.WriteLine(ex.Message);
                return string.Empty; // Mengembalikan string kosong jika gagal
            }
        }

        public static string GetLocalIPAddress() // Metode untuk mendapatkan IP address lokal
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Tidak ada alamat IP yang ditemukan.");
        }
    }
}