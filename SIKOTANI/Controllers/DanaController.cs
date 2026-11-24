using SIKOTANI.Models;
using SIKOTANI.Models.Repositories;

namespace SIKOTANI.Controllers;

public class DanaController : ControllerDasar
{
    public ParameterKoperasi MuatParameter()
    {
        PastikanBolehLihat(MenuAplikasi.DanaAnggota);
        return ParameterRepository.Muat();
    }

    public List<DanaAnggota> DaftarDana(Anggota anggota)
    {
        PastikanBolehLihat(MenuAplikasi.DanaAnggota);
        return DanaRepository.DaftarDana(anggota);
    }

    public List<TransaksiDana> Riwayat(int idDana)
    {
        PastikanBolehLihat(MenuAplikasi.DanaAnggota);
        return DanaRepository.Riwayat(idDana);
    }

    public int BukaDana(int idAnggota, string jenis)
    {
        PastikanBolehKelola(MenuAplikasi.DanaAnggota);
        return DanaRepository.BukaDana(idAnggota, jenis);
    }

    public void Catat(int idDana, JenisTransaksi jenis, decimal jumlah, DateOnly tanggal)
    {
        PastikanBolehKelola(MenuAplikasi.DanaAnggota);
        DanaRepository.Catat(idDana, jenis, jumlah, tanggal);
    }
}