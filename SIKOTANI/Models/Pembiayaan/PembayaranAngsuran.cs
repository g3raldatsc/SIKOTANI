namespace SIKOTANI.Models;

public class PembayaranAngsuran
{
    public int IdPembayaran { get; internal set; }
    public DateOnly Tanggal { get; }
    public decimal Jumlah { get; }
    public SumberPembayaran Sumber { get; }
    public SetoranPanen? Setoran { get; }   // association, hanya untuk PotongPanen
    public int? IdSetoran { get; }          // tetap diketahui walau objek setoran belum dimuat

    // Pembayaran baru
    public PembayaranAngsuran(DateOnly tanggal, decimal jumlah,
                              SumberPembayaran sumber, SetoranPanen? setoran = null)
        : this(tanggal, jumlah, sumber, setoran, setoran?.IdSetoran) { }

    // Pemulihan dari database: hanya ID setoran yang diketahui
    internal PembayaranAngsuran(DateOnly tanggal, decimal jumlah,
                                SumberPembayaran sumber, int? idSetoran)
        : this(tanggal, jumlah, sumber, null, idSetoran) { }

    private PembayaranAngsuran(DateOnly tanggal, decimal jumlah, SumberPembayaran sumber,
                               SetoranPanen? setoran, int? idSetoran)
    {
        if (jumlah <= 0)
            throw new AturanBisnisException("Jumlah pembayaran harus lebih dari 0.");

        bool adaSetoran = setoran is not null || idSetoran is not null;
        if (sumber == SumberPembayaran.PotongPanen && !adaSetoran)
            throw new AturanBisnisException(
                "Pembayaran potong panen harus terhubung ke setoran panen.");
        if (sumber == SumberPembayaran.Tunai && adaSetoran)
            throw new AturanBisnisException(
                "Pembayaran tunai tidak boleh terhubung ke setoran panen.");

        Tanggal = tanggal;
        Jumlah = jumlah;
        Sumber = sumber;
        Setoran = setoran;
        IdSetoran = idSetoran;
    }
}