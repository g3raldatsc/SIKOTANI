using SIKOTANI.Models;
using SIKOTANI.Models.Repositories;

namespace SIKOTANI.Controllers;

public class KelompokTaniController : ControllerDasar
{
    public List<KelompokTani> DaftarSemua()
    {
        PastikanBolehLihat(MenuAplikasi.Keanggotaan);
        return KelompokTaniRepository.DaftarSemua();
    }

    public int Tambah(string nama, string desa)
    {
        PastikanBolehKelola(MenuAplikasi.Keanggotaan);
        return KelompokTaniRepository.Tambah(nama, desa);
    }

    public void Ubah(int id, string nama, string desa)
    {
        PastikanBolehKelola(MenuAplikasi.Keanggotaan);
        KelompokTaniRepository.Ubah(id, nama, desa);
    }

    public void Hapus(int id)
    {
        PastikanBolehKelola(MenuAplikasi.Keanggotaan);
        KelompokTaniRepository.Hapus(id);
    }

    public void TambahAnggota(int idKelompok, int idAnggota)
    {
        PastikanBolehKelola(MenuAplikasi.Keanggotaan);
        KelompokTaniRepository.TambahAnggota(idKelompok, idAnggota);
    }

    public void KeluarkanAnggota(int idKelompok, int idAnggota)
    {
        PastikanBolehKelola(MenuAplikasi.Keanggotaan);
        KelompokTaniRepository.KeluarkanAnggota(idKelompok, idAnggota);
    }
}