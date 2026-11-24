namespace SIKOTANI.Models;

public class ItemSaprodi
{
    public int IdItem { get; internal set; }
    public string NamaItem { get; }
    public string Jenis { get; }
    public decimal Jumlah { get; }
    public string Satuan { get; }
    public decimal HargaSatuan { get; }

    public ItemSaprodi(string namaItem, string jenis, decimal jumlah,
                       string satuan, decimal hargaSatuan)
    {
        if (string.IsNullOrWhiteSpace(namaItem))
            throw new AturanBisnisException("Nama item saprodi wajib diisi.");
        if (jumlah <= 0)
            throw new AturanBisnisException("Jumlah item harus lebih dari 0.");
        if (hargaSatuan < 0)
            throw new AturanBisnisException("Harga satuan tidak boleh negatif.");

        NamaItem = namaItem.Trim();
        Jenis = (jenis ?? string.Empty).Trim();
        Jumlah = jumlah;
        Satuan = (satuan ?? string.Empty).Trim();
        HargaSatuan = hargaSatuan;
    }

    // B4: subtotal = jumlah x harga satuan
    public decimal HitungSubtotal()
        => Math.Round(Jumlah * HargaSatuan, 0, MidpointRounding.AwayFromZero);
}