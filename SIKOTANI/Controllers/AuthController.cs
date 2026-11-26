using SIKOTANI.Models;
using SIKOTANI.Models.Repositories;

namespace SIKOTANI.Controllers;

public class AuthController : ControllerDasar
{
    public bool BelumAdaPengguna() => PenggunaRepository.BelumAdaPengguna();

    /// <summary>ini bakal ngembalikan true bila berhasil masuk (sesi terisi).</summary>
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

    /// <summary>Setiap pengguna dapat mengubah kata sandinya sendiri.</summary>
    public void UbahKataSandiSaya(string kataSandiLama, string kataSandiBaru)
    {
        var saya = PenggunaSaatIni;
        if (!saya.VerifikasiKataSandi(kataSandiLama))
            throw new AturanBisnisException("Kata sandi lama salah.");

        string hashBaru = KataSandi.Hash(kataSandiBaru);   // validasi minimal 8 karakter
        PenggunaRepository.AturKataSandi(saya.IdPengguna, hashBaru);

        // Muat ulang agar objek sesi memakai hash terbaru
        var terbaru = PenggunaRepository.CariById(saya.IdPengguna);
        if (terbaru is not null) Sesi.Masuk(terbaru);
    }
}