namespace SIKOTANI.Models;

public class Koperasi
{
    public string Nama { get; }
    public string Alamat { get; }
    public string NoBadanHukum { get; }

    // Composition: ketiganya dibuat dan dimiliki Koperasi
    public ParameterKoperasi Parameter { get; }
    public UnitSimpanPinjam Usp { get; }
    public UnitHasilPanen UnitPanen { get; }

    public Koperasi(string nama, string alamat, string noBadanHukum,
                    decimal nilaiSimpananPokok,
                    decimal bungaTabunganMaks, decimal bungaPinjamanMaks,
                    decimal bungaTabunganBerlaku, decimal bungaPinjamanBerlaku)
    {
        if (string.IsNullOrWhiteSpace(nama))
            throw new AturanBisnisException("Nama koperasi wajib diisi.");

        Nama = nama.Trim();
        Alamat = (alamat ?? string.Empty).Trim();
        NoBadanHukum = (noBadanHukum ?? string.Empty).Trim();

        Parameter = new ParameterKoperasi(nilaiSimpananPokok,
            bungaTabunganMaks, bungaPinjamanMaks,
            bungaTabunganBerlaku, bungaPinjamanBerlaku);
        Usp = new UnitSimpanPinjam();
        UnitPanen = new UnitHasilPanen();
    }
}