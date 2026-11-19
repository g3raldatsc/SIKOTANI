namespace SIKOTANI.Models;

public class Bendahara : Pengguna
{
    public Bendahara(int idPengguna, string username, Anggota anggota,
                     string passwordHash, bool aktif = true)
        : base(idPengguna, username, anggota, passwordHash, aktif) { }

    public override string GetPeran() => "Bendahara";

    public override HakAkses GetHakAkses(MenuAplikasi menu) => menu switch
    {
        MenuAplikasi.DanaAnggota or
        MenuAplikasi.PengajuanPinjaman or
        MenuAplikasi.PembayaranAngsuran or
        MenuAplikasi.SetoranPanen => HakAkses.Kelola,
        MenuAplikasi.Keanggotaan or
        MenuAplikasi.LaporanKeuangan => HakAkses.Lihat,
        _ => HakAkses.Tidak
    };
}