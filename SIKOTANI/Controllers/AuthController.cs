using SIKOTANI.Models;
using SIKOTANI.Models.Repositories;

namespace SIKOTANI.Controllers;

public class AuthController : ControllerDasar
{
    public bool BelumAdaPengguna() => PenggunaRepository.BelumAdaPengguna();

    /// <summary>Mengembalikan true bila berhasil masuk (sesi terisi).</summary>
    public bool Masuk(string username, string kataSandi)
    {
        var pengguna = PenggunaRepository.CariByUsername(username);
        if (pengguna is null || !pengguna.VerifikasiKataSandi(kataSandi))
            return false;

        Sesi.Masuk(pengguna);
        return true;
    }

    public void Keluar() => Sesi.Keluar();

    public void BuatKetuaAwal(string nama, string username, string kataSandi)
        => PenggunaRepository.BuatKetuaAwal(nama, username, kataSandi);

    // Sementara: dihapus pada Tahap 10 saat Form Parameter dan Akun jadi
    public string BuatAkunContoh()
    {
        PastikanBolehKelola(MenuAplikasi.ParameterDanAkun);

        var hasil = new System.Text.StringBuilder();
        var akun = new[]
        {
            ("Sekretaris Contoh", "sekretaris1", "Sekretaris"),
            ("Bendahara Contoh",  "bendahara1",  "Bendahara")
        };

        foreach (var (nama, username, peran) in akun)
        {
            try
            {
                PenggunaRepository.BuatPengguna(nama, username, "contoh1234", peran);
                hasil.AppendLine($"Dibuat: {username} ({peran})");
            }
            catch (AturanBisnisException ex)
            {
                hasil.AppendLine($"{username}: {ex.Message}");
            }
        }
        return hasil.ToString();
    }
}