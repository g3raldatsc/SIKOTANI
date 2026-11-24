using SIKOTANI.Models;
using SIKOTANI.Models.Repositories;

namespace SIKOTANI.Controllers;

public class DashboardController : ControllerDasar
{
    /// <summary>Angka yang tidak boleh dilihat peran ini diganti null.</summary>
    public RingkasanDashboard Muat()
    {
        var pengguna = PenggunaSaatIni;
        var data = RingkasanRepository.Muat();

        return new RingkasanDashboard(
            data.AnggotaAktif,
            data.KelompokTani,
            pengguna.BolehAkses(MenuAplikasi.DanaAnggota) ? data.TotalSaldoDana : null,
            pengguna.BolehAkses(MenuAplikasi.PengajuanPinjaman) ? data.PinjamanAktif : null,
            pengguna.BolehAkses(MenuAplikasi.SetoranPanen) ? data.NilaiSetoran : null);
    }
}