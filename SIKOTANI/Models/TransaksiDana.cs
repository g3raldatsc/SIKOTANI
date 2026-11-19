namespace SIKOTANI.Models;

public class TransaksiDana
{
    public int IdTransaksi { get; internal set; }
    public DateOnly Tanggal { get; }
    public JenisTransaksi Jenis { get; }
    public decimal Jumlah { get; }
    public decimal SaldoSetelah { get; }

    public TransaksiDana(DateOnly tanggal, JenisTransaksi jenis, decimal jumlah, decimal saldoSetelah)
    {
        Tanggal = tanggal;
        Jenis = jenis;
        Jumlah = jumlah;
        SaldoSetelah = saldoSetelah;
    }
}