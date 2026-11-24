namespace SIKOTANI.Models;

/// <summary>Angka ringkasan untuk dashboard. Nilai null berarti peran tidak berhak melihatnya.</summary>
public record RingkasanDashboard(
    int AnggotaAktif,
    int KelompokTani,
    decimal? TotalSaldoDana,
    int? PinjamanAktif,
    decimal? NilaiSetoran);