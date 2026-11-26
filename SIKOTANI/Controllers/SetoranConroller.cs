using SIKOTANI.Models;
using SIKOTANI.Models.Repositories;

namespace SIKOTANI.Controllers;

public class SetoranController : ControllerDasar
{
    public List<SetoranPanen> Daftar()
    {
        PastikanBolehLihat(MenuAplikasi.SetoranPanen);
        return SetoranRepository.Daftar();
    }

    public List<RiwayatPotongan> Riwayat(int idSetoran)
    {
        PastikanBolehLihat(MenuAplikasi.SetoranPanen);
        return SetoranRepository.Riwayat(idSetoran);
    }

    public int Catat(int idAnggota, int idKomoditas, DateOnly tanggal,
                     decimal beratKg, decimal hargaPerKg)
    {
        PastikanBolehKelola(MenuAplikasi.SetoranPanen);
        return SetoranRepository.Catat(idAnggota, idKomoditas, tanggal, beratKg, hargaPerKg);
    }

    // Potong panen menyentuh dua unit usaha, jadi peran harus berhak mengelola keduanya
    public void PotongAngsuran(int idSetoran, int idPinjaman, int noAngsuran,
                               decimal jumlah, DateOnly tanggal, bool simulasiGagal = false)
    {
        PastikanBolehKelola(MenuAplikasi.SetoranPanen);
        PastikanBolehKelola(MenuAplikasi.PembayaranAngsuran);
        SetoranRepository.PotongUntukAngsuran(idSetoran, idPinjaman, noAngsuran,
                                              jumlah, tanggal, simulasiGagal);
    }
}