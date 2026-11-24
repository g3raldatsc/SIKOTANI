using SIKOTANI.Models;

namespace SIKOTANI.Controllers;

/// <summary>Kerangka semua controller: pemeriksaan sesi dan hak akses.</summary>
public abstract class ControllerDasar
{
    protected static Pengguna PenggunaSaatIni
        => Sesi.PenggunaAktif
           ?? throw new AturanBisnisException(
               "Sesi masuk tidak ditemukan. Silakan masuk kembali.");

    // Untuk operasi baca: peran harus punya hak Lihat atau Kelola
    protected static void PastikanBolehLihat(MenuAplikasi menu)
        => PenggunaSaatIni.PastikanBolehAkses(menu);

    // Untuk operasi tulis: peran harus punya hak Kelola
    protected static void PastikanBolehKelola(MenuAplikasi menu)
    {
        if (!PenggunaSaatIni.BolehKelola(menu))
            throw new AturanBisnisException(
                $"Peran {PenggunaSaatIni.GetPeran()} tidak berhak mengubah data pada menu {menu}.");
    }
}