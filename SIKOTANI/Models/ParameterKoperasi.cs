namespace SIKOTANI.Models;

public class ParameterKoperasi
{
    // Enkapsulasi: nilai hanya bisa diubah lewat method yang memvalidasi.
    public decimal NilaiSimpananPokok { get; private set; }
    public decimal BungaTabunganMaks { get; private set; }
    public decimal BungaPinjamanMaks { get; private set; }
    public decimal BungaTabunganBerlaku { get; private set; }
    public decimal BungaPinjamanBerlaku { get; private set; }

    public ParameterKoperasi(decimal nilaiSimpananPokok,
        decimal bungaTabunganMaks, decimal bungaPinjamanMaks,
        decimal bungaTabunganBerlaku, decimal bungaPinjamanBerlaku)
    {
        if (bungaTabunganMaks < 0 || bungaPinjamanMaks < 0)
            throw new AturanBisnisException("Batas maksimum bunga tidak boleh negatif.");

        BungaTabunganMaks = bungaTabunganMaks;
        BungaPinjamanMaks = bungaPinjamanMaks;
        AturSimpananPokok(nilaiSimpananPokok);
        AturBungaTabungan(bungaTabunganBerlaku);
        AturBungaPinjaman(bungaPinjamanBerlaku);
    }

    public void AturSimpananPokok(decimal nilai)
    {
        if (nilai <= 0)
            throw new AturanBisnisException("Nilai simpanan pokok harus lebih dari 0.");
        NilaiSimpananPokok = nilai;
    }

    public void AturBungaTabungan(decimal persen)
    {
        if (persen < 0 || persen > BungaTabunganMaks)
            throw new AturanBisnisException(
                $"Bunga tabungan harus antara 0% dan batas maksimum {BungaTabunganMaks:0.##}%.");
        BungaTabunganBerlaku = persen;
    }

    public void AturBungaPinjaman(decimal persen)
    {
        if (persen < 0 || persen > BungaPinjamanMaks)
            throw new AturanBisnisException(
                $"Bunga pinjaman harus antara 0% dan batas maksimum {BungaPinjamanMaks:0.##}%.");
        BungaPinjamanBerlaku = persen;
    }

    // Aturan A6: batas maksimum dapat diubah Menteri, maka tidak ditulis mati di kode.
    // gacor wokkk
    public void UbahBatasMaks(decimal tabunganMaks, decimal pinjamanMaks)
    {
        if (tabunganMaks < BungaTabunganBerlaku || pinjamanMaks < BungaPinjamanBerlaku)
            throw new AturanBisnisException(
                "Batas maksimum baru lebih kecil dari bunga yang berlaku. Turunkan bunga berlaku dahulu.");
        BungaTabunganMaks = tabunganMaks;
        BungaPinjamanMaks = pinjamanMaks;
    }
}