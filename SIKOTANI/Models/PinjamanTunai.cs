namespace SIKOTANI.Models;

public class PinjamanTunai : Pinjaman
{
    public decimal JumlahPinjaman { get; }
    public override string NamaJenis => "Pinjaman Tunai";

    public PinjamanTunai(int idPinjaman, Anggota anggota, decimal jumlahPinjaman,
                         int tenorBulan, ParameterKoperasi parameter,
                         DateOnly tanggalPengajuan)
        : base(idPinjaman, anggota, tenorBulan, parameter, tanggalPengajuan)
    {
        if (jumlahPinjaman <= 0)
            throw new AturanBisnisException("Jumlah pinjaman harus lebih dari 0.");
        JumlahPinjaman = jumlahPinjaman;
    }

    public override decimal HitungPokokPinjaman() => JumlahPinjaman;

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