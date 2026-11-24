namespace SIKOTANI.Models;

public class PinjamanSaprodi : Pinjaman
{
    private readonly List<ItemSaprodi> _daftarItem = new(); // composition

    public IReadOnlyList<ItemSaprodi> DaftarItem => _daftarItem;
    public override string NamaJenis => "Pinjaman Saprodi";

    public PinjamanSaprodi(int idPinjaman, Anggota anggota, int tenorBulan,
                           ParameterKoperasi parameter, DateOnly tanggalPengajuan)
        : base(idPinjaman, anggota, tenorBulan, parameter, tanggalPengajuan) { }

    public void TambahItem(ItemSaprodi item)
    {
        if (Status != StatusPinjaman.Diajukan)
            throw new AturanBisnisException(
                "Item hanya dapat ditambah saat pinjaman berstatus Diajukan.");
        _daftarItem.Add(item);
    }

    // B6: pokok = jumlah seluruh subtotal item
    public override decimal HitungPokokPinjaman()
        => _daftarItem.Sum(i => i.HitungSubtotal());

    // B6: satu angsuran di akhir tenor; bunga = pokok x (suku bunga / 12) x tenor
    public override List<Angsuran> BuatJadwalAngsuran(DateOnly tanggalCair)
    {
        decimal pokok = HitungPokokPinjaman();
        decimal bunga = Math.Round(
            pokok * (SukuBungaTahunan / 100m) / 12m * TenorBulan, 0,
            MidpointRounding.AwayFromZero);

        return new List<Angsuran>
        {
            new Angsuran(1, tanggalCair.AddMonths(TenorBulan), pokok, bunga)
        };
    }
}