namespace SIKOTANI.Models;

public class PembayaranAngsuran
{
    public int IdPembayaran { get; internal set; }
    public DateOnly Tanggal { get; }
    public decimal Jumlah { get; }
    public SumberPembayaran Sumber { get; }
    public SetoranPanen? Setoran { get; } // association, hanya untuk PotongPanen

    public PembayaranAngsuran(DateOnly tanggal, decimal jumlah,
                              SumberPembayaran sumber, SetoranPanen? setoran = null)
    {
        if (jumlah <= 0)
            throw new AturanBisnisException("Jumlah pembayaran harus lebih dari 0.");
        if (sumber == SumberPembayaran.PotongPanen && setoran is null)
            throw new AturanBisnisException(
                "Pembayaran potong panen harus terhubung ke setoran panen.");
        if (sumber == SumberPembayaran.Tunai && setoran is not null)
            throw new AturanBisnisException(
                "Pembayaran tunai tidak boleh terhubung ke setoran panen.");

        Tanggal = tanggal;
        Jumlah = jumlah;
        Sumber = sumber;
        Setoran = setoran;
    }
}