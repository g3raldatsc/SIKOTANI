using SIKOTANI.Models;

namespace SIKOTANI.Controllers;

/// <summary>Menyimpan siapa yang sedang masuk. Bertipe Pengguna (abstrak), jadi tidak peduli perannya.</summary>
public static class Sesi
{
    public static Pengguna? PenggunaAktif { get; private set; }
    public static bool SudahMasuk => PenggunaAktif is not null;

    public static void Masuk(Pengguna pengguna) => PenggunaAktif = pengguna;
    public static void Keluar() => PenggunaAktif = null;
}