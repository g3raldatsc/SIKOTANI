using SIKOTANI.Models;
using SIKOTANI.Models.Repositories;

namespace SIKOTANI.Controllers;

public class PinjamanController : ControllerDasar
{
    // Data pinjaman boleh dilihat oleh peran yang punya akses pengajuan, persetujuan, atau pembayaran
    private static void PastikanBolehLihatPinjaman()
    {
        var pengguna = PenggunaSaatIni;
        if (!pengguna.BolehAkses(MenuAplikasi.PengajuanPinjaman) &&
            !pengguna.BolehAkses(MenuAplikasi.PersetujuanPinjaman) &&
            !pengguna.BolehAkses(MenuAplikasi.PembayaranAngsuran))
            throw new AturanBisnisException(
                $"Peran {pengguna.GetPeran()} tidak berhak melihat data pinjaman.");
    }

    public ParameterKoperasi MuatParameter()
    {
        PastikanBolehLihatPinjaman();
        return ParameterRepository.Muat();
    }

    public List<Pinjaman> Daftar(StatusPinjaman? status = null)
    {
        PastikanBolehLihatPinjaman();
        return PinjamanRepository.Daftar(status);
    }

    // ----- Pengajuan (Bendahara) -----
    public int AjukanTunai(int idAnggota, decimal jumlah, int tenorBulan, DateOnly tanggal)
    {
        PastikanBolehKelola(MenuAplikasi.PengajuanPinjaman);
        return PinjamanRepository.AjukanTunai(idAnggota, jumlah, tenorBulan, tanggal);
    }

    public int AjukanSaprodi(int idAnggota, IEnumerable<ItemSaprodi> daftarItem,
                             int tenorBulan, DateOnly tanggal)
    {
        PastikanBolehKelola(MenuAplikasi.PengajuanPinjaman);
        return PinjamanRepository.AjukanSaprodi(idAnggota, daftarItem, tenorBulan, tanggal);
    }

    // ----- Persetujuan (Ketua) -----
    public List<Angsuran> PratinjauJadwal(Pinjaman pinjaman, DateOnly tanggal)
    {
        PenggunaSaatIni.PastikanBolehAkses(MenuAplikasi.PersetujuanPinjaman);
        return pinjaman.BuatJadwalAngsuran(tanggal);   // hanya menghitung, tidak mengubah apa pun
    }

    public void Setujui(int idPinjaman, DateOnly tanggal)
    {
        PastikanBolehKelola(MenuAplikasi.PersetujuanPinjaman);
        PinjamanRepository.Setujui(idPinjaman, tanggal);
    }

    public void Tolak(int idPinjaman)
    {
        PastikanBolehKelola(MenuAplikasi.PersetujuanPinjaman);
        PinjamanRepository.Tolak(idPinjaman);
    }

    // ----- Pembayaran angsuran (Bendahara) -----
    public void BayarAngsuran(int idPinjaman, int noAngsuran, decimal jumlah, DateOnly tanggal)
    {
        PastikanBolehKelola(MenuAplikasi.PembayaranAngsuran);
        PinjamanRepository.Bayar(idPinjaman, noAngsuran, jumlah, tanggal);
    }
}