using System.Globalization;

namespace SIKOTANI.Models;

/// <summary>ini hanya untuk format teks yang seragam, tidak bergantung pengaturan Windows.</summary>
public static class FormatTeks
{
    public static CultureInfo Budaya { get; } = CultureInfo.GetCultureInfo("id-ID");

    public static string Rp(decimal nilai) => "Rp" + nilai.ToString("N0", Budaya);
    public static string Angka(decimal nilai) => nilai.ToString("N2", Budaya);
    public static string Tanggal(DateOnly tanggal) => tanggal.ToString("dd-MM-yyyy", Budaya);
}