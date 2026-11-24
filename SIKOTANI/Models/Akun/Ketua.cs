namespace SIKOTANI.Models;

public class Ketua : Pengguna
{
    public Ketua(int idPengguna, string username, Anggota anggota,
                 string passwordHash, bool aktif = true)
        : base(idPengguna, username, anggota, passwordHash, aktif) { }

    public override string GetPeran() => "Ketua";   // ngeubah alur methodnya dari pengguna.cs

    public override HakAkses GetHakAkses(MenuAplikasi menu) => menu switch
    {
        MenuAplikasi.PersetujuanPinjaman or
        MenuAplikasi.ParameterDanAkun => HakAkses.Kelola,
        _ => HakAkses.Lihat
    };
}