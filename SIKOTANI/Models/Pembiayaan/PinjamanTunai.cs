namespace SIKOTANI.Models;

public class PinjamanTunai : Pinjaman
{
    public decimal JumlahPinjaman { get; }
    public override string NamaJenis => "Pinjaman Tunai";

    // Pengajuan baru
    public PinjamanTunai(int idPinjaman, Anggota anggota, decimal jumlahPinjaman,
                         int tenorBulan, ParameterKoperasi parameter,
                         DateOnly tanggalPengajuan)
        : base(idPinjaman, anggota, tenorBulan, parameter, tanggalPengajuan)
    {
        PastikanJumlahValid(jumlahPinjaman);
        JumlahPinjaman = jumlahPinjaman;
    }

    // Pemulihan dari database
    internal PinjamanTunai(int idPinjaman, Anggota anggota, decimal jumlahPinjaman,
                           int tenorBulan, decimal sukuBungaTahunan,
                           DateOnly tanggalPengajuan)
        : base(idPinjaman, anggota, tenorBulan, sukuBungaTahunan, tanggalPengajuan)
    {
        PastikanJumlahValid(jumlahPinjaman);
        JumlahPinjaman = jumlahPinjaman;
    }

    private static void PastikanJumlahValid(decimal jumlah)
    {
        if (jumlah <= 0)
            throw new AturanBisnisException("Jumlah pinjaman harus lebih dari 0.");
    }

    public override decimal HitungPokokPinjaman() => JumlahPinjaman;

    public override string Rincian() => $"Pinjaman uang tunai sebesar Rp{JumlahPinjaman:N0}";

    // B5: angsuran pokok tetap tiap bulan, bunga dari sisa pokok (menurun)
    public override List<Angsuran> BuatJadwalAngsuran(DateOnly tanggalCair)
    {
        decimal pokok = HitungPokokPinjaman();
        decimal pokokBulanan = Math.Floor(pokok / TenorBulan);
        decimal sisa = pokok;
        var jadwal = new List<Angsuran>();

        for (int i = 1; i <= TenorBulan; i++)
        {
            decimal bunga = HitungBungaSebulan(sisa);
            // B8: selisih pembulatan ditampung angsuran terakhir
            decimal angsuranPokok = (i == TenorBulan) ? sisa : pokokBulanan;

            jadwal.Add(new Angsuran(i, tanggalCair.AddMonths(i), angsuranPokok, bunga));
            sisa -= angsuranPokok;
        }
        return jadwal;
    }
}