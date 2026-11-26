namespace SIKOTANI.Models;

/// <summary>Satu baris riwayat: setoran dipotong untuk angsuran tertentu.</summary>
public record RiwayatPotongan(DateOnly Tanggal, decimal Jumlah, int IdPinjaman, int NoAngsuran);