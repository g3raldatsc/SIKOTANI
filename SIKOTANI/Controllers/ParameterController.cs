using SIKOTANI.Models;
using SIKOTANI.Models.Repositories;

namespace SIKOTANI.Controllers;

public class ParameterController : ControllerDasar
{
    public ParameterKoperasi Muat()
    {
        PastikanBolehLihat(MenuAplikasi.ParameterDanAkun);
        return ParameterRepository.Muat();
    }

    /// <summary>Batas maksimum bunga sengaja gak dapat diubah dari sini (A4, A5, A6).</summary>
    public void Simpan(decimal nilaiSimpananPokok, decimal bungaTabungan, decimal bungaPinjaman)
    {
        PastikanBolehKelola(MenuAplikasi.ParameterDanAkun);

        var parameter = ParameterRepository.Muat();
        parameter.AturSimpananPokok(nilaiSimpananPokok);   // validasi dilakukan class domain
        parameter.AturBungaTabungan(bungaTabungan);
        parameter.AturBungaPinjaman(bungaPinjaman);
        ParameterRepository.Simpan(parameter);
    }
}