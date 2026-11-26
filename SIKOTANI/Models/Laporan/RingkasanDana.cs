namespace SIKOTANI.Models;

public record RingkasanDana(string Nama, string Status, decimal Pokok, decimal Wajib, decimal Tabungan)
{
    public decimal Total => Pokok + Wajib + Tabungan;
}