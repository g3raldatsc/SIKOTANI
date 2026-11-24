namespace SIKOTANI.Models;

public class Sekretaris : Pengguna
{
    public Sekretaris(int idPengguna, string username, Anggota anggota,
                      string passwordHash, bool aktif = true)
        : base(idPengguna, username, anggota, passwordHash, aktif) { }

    public override string GetPeran() => "Sekretaris";

    public override HakAkses GetHakAkses(MenuAplikasi menu) => menu switch
    {
        MenuAplikasi.Keanggotaan => HakAkses.Kelola,
        MenuAplikasi.LaporanAnggota => HakAkses.Lihat,
        _ => HakAkses.Tidak
    };
}