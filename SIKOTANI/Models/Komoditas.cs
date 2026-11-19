namespace SIKOTANI.Models;

public class Komoditas
{
    public int IdKomoditas { get; internal set; }
    public string Nama { get; }
    public string Satuan { get; }

    public Komoditas(int idKomoditas, string nama, string satuan = "kg")
    {
        if (string.IsNullOrWhiteSpace(nama))
            throw new AturanBisnisException("Nama komoditas wajib diisi.");
        IdKomoditas = idKomoditas;
        Nama = nama.Trim();
        Satuan = string.IsNullOrWhiteSpace(satuan) ? "kg" : satuan.Trim();
    }
}