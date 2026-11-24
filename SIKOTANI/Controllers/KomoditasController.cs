using SIKOTANI.Models;
using SIKOTANI.Models.Repositories;

namespace SIKOTANI.Controllers;

public class KomoditasController : ControllerDasar
{
    public List<Komoditas> DaftarSemua()
    {
        PastikanBolehLihat(MenuAplikasi.Keanggotaan);
        return KomoditasRepository.DaftarSemua();
    }

    public void Tambah(string nama, string satuan)
    {
        PastikanBolehKelola(MenuAplikasi.Keanggotaan);
        KomoditasRepository.Tambah(nama, satuan);
    }

    public void Hapus(int id)
    {
        PastikanBolehKelola(MenuAplikasi.Keanggotaan);
        KomoditasRepository.Hapus(id);
    }

    public List<int> IdKomoditasAnggota(int idAnggota)
    {
        PastikanBolehLihat(MenuAplikasi.Keanggotaan);
        return KomoditasRepository.IdKomoditasAnggota(idAnggota);
    }

    public void SetKomoditasAnggota(int idAnggota, IEnumerable<int> idKomoditas)
    {
        PastikanBolehKelola(MenuAplikasi.Keanggotaan);
        KomoditasRepository.SetKomoditasAnggota(idAnggota, idKomoditas);
    }
}