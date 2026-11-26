using SIKOTANI.Models;
using SIKOTANI.Models.Repositories;

namespace SIKOTANI.Controllers;

public class AnggotaController : ControllerDasar
{
    public List<Anggota> Cari(string kata, bool termasukKeluar)
    {
        PastikanBolehLihat(MenuAplikasi.Keanggotaan);
        return AnggotaRepository.Cari(kata, termasukKeluar);
    }

    public Anggota? CariById(int id)
    {
        PastikanBolehLihat(MenuAplikasi.Keanggotaan);
        return AnggotaRepository.CariById(id);
    }

    public int Tambah(string nama, string alamat, string noTelepon, DateOnly tanggalGabung, decimal luasLahanHa)
    {
        PastikanBolehKelola(MenuAplikasi.Keanggotaan);
        return AnggotaRepository.Tambah(nama, alamat, noTelepon, tanggalGabung, luasLahanHa);
    }

    public void Ubah(int id, string nama, string alamat, string noTelepon, decimal luasLahanHa)
    {
        PastikanBolehKelola(MenuAplikasi.Keanggotaan);
        AnggotaRepository.Ubah(id, nama, alamat, noTelepon, luasLahanHa);
    }

    public void Keluarkan(int id)
    {
        PastikanBolehKelola(MenuAplikasi.Keanggotaan);
        AnggotaRepository.Keluarkan(id);
    }
}