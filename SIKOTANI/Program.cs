using SIKOTANI.Data;

namespace SIKOTANI;

// im not gay
internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        try
        {
            if (PenggunaRepository.BelumAdaPengguna())
            {
                using var setup = new FormSetupAwal();
                if (setup.ShowDialog() != DialogResult.OK) return;
            }

            while (true)
            {
                using (var login = new FormLogin())
                {
                    if (login.ShowDialog() != DialogResult.OK) return;
                }

                using var utama = new FormUtama();
                utama.ShowDialog();

                // Tombol Keluar mengosongkan sesi, lalu login muncul lagi.
                // Bila jendela ditutup dengan tombol X (sesi masih ada), aplikasi selesai.
                if (Sesi.SudahMasuk) return;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Terjadi kesalahan:\n" + ex.Message,
                "SIKOTANI", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}