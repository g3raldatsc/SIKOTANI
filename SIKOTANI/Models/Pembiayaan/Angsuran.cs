namespace SIKOTANI.Models;

public class Angsuran
{
    private readonly List<PembayaranAngsuran> _pembayaran = new();   // composition

    public int IdAngsuran { get; internal set; }
    public int NoAngsuran { get; }
    public DateOnly JatuhTempo { get; }
    public decimal Pokok { get; }
    public decimal Bunga { get; }
    public StatusAngsuran Status { get; private set; } = StatusAngsuran.Belum;
    public IReadOnlyList<PembayaranAngsuran> Pembayaran => _pembayaran;

    public Angsuran(int noAngsuran, DateOnly jatuhTempo, decimal pokok, decimal bunga)
    {
        if (noAngsuran <= 0)
            throw new AturanBisnisException("Nomor angsuran harus lebih dari 0.");
        if (pokok < 0 || bunga < 0)
            throw new AturanBisnisException("Pokok dan bunga angsuran tidak boleh negatif.");

        NoAngsuran = noAngsuran;
        JatuhTempo = jatuhTempo;
        Pokok = pokok;
        Bunga = bunga;
    }

    public decimal HitungTagihan() => Pokok + Bunga;
    public decimal GetTerbayar() => _pembayaran.Sum(p => p.Jumlah);
    public decimal GetSisaTagihan() => HitungTagihan() - GetTerbayar();

    // B8: pembayaran sebagian dialokasikan ke bunga dahulu, baru ke pokok
    public decimal GetSisaPokok()
        => Pokok - Math.Max(0m, GetTerbayar() - Bunga);

    // Hanya penanda tampilan; tidak ada denda dan tagihan tidak berubah
    public bool Terlambat(DateOnly hariIni)
        => Status != StatusAngsuran.Lunas && JatuhTempo < hariIni;

    // Hanya dipanggil repository saat memuat jadwal dari database
    internal void PulihkanPembayaran(IEnumerable<PembayaranAngsuran> pembayaran)
    {
        _pembayaran.Clear();
        _pembayaran.AddRange(pembayaran);
        HitungUlangStatus();
    }

    // internal: hanya Pinjaman yang boleh mencatat pembayaran
    internal void CatatPembayaran(PembayaranAngsuran pembayaran)
    {
        if (Status == StatusAngsuran.Lunas)
            throw new AturanBisnisException("Angsuran ini sudah lunas.");
        if (pembayaran.Jumlah > GetSisaTagihan())
            throw new AturanBisnisException(
                $"Pembayaran melebihi sisa tagihan angsuran (Rp{GetSisaTagihan():N0}).");

        _pembayaran.Add(pembayaran);
        HitungUlangStatus();
    }

    private void HitungUlangStatus()
    {
        decimal terbayar = GetTerbayar();
        Status = terbayar <= 0m ? StatusAngsuran.Belum
               : terbayar >= HitungTagihan() ? StatusAngsuran.Lunas
               : StatusAngsuran.Sebagian;
    }
}