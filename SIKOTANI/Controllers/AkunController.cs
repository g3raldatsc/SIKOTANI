using SIKOTANI.Models;
using SIKOTANI.Models.Repositories;

namespace SIKOTANI.Controllers;

public class AkunController : ControllerDasar
{
    public List<Pengguna> DaftarAkun()
    {
        PastikanBolehLihat(MenuAplikasi.ParameterDanAkun);
        return PenggunaRepository.DaftarSemua();
    }

    public List<Anggota> AnggotaTanpaAkun()
    {
        PastikanBolehLihat(MenuAplikasi.ParameterDanAkun);
        return PenggunaRepository.AnggotaTanpaAkun();
    }

    public void BuatAkun(int idAnggota, string username, string kataSandi, string peran)
    {
        PastikanBolehKelola(MenuAplikasi.ParameterDanAkun);
        PenggunaRepository.BuatAkun(idAnggota, username, kataSandi, peran);
    }

    public void UbahStatus(int idPengguna, bool aktif)
    {
        PastikanBolehKelola(MenuAplikasi.ParameterDanAkun);
        if (!aktif && idPengguna == PenggunaSaatIni.IdPengguna)
            throw new AturanBisnisException("Anda tidak dapat menonaktifkan akun Anda sendiri.");
        PenggunaRepository.UbahStatusAktif(idPengguna, aktif);
    }

    public void AturUlangKataSandi(int idPengguna, string kataSandiBaru)
    {
        PastikanBolehKelola(MenuAplikasi.ParameterDanAkun);
        string hash = KataSandi.Hash(kataSandiBaru);   // validasi minimal 8 karakter
        PenggunaRepository.AturKataSandi(idPengguna, hash);
    }
}