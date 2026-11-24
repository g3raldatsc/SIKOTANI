namespace SIKOTANI.Models;

public class PinjamanSaprodi : Pinjaman
{
    private readonly List<ItemSaprodi> _daftarItem = new();   // composition

    public IReadOnlyList<ItemSaprodi> DaftarItem => _daftarItem;
    public override string NamaJenis => "Pinjaman Saprodi";

    // Pengajuan baru
    public PinjamanSaprodi(int idPinjaman, Anggota anggota, int tenorBulan,
                           ParameterKoperasi parameter, DateOnly tanggalPengajuan)
        : base(idPinjaman, anggota, tenorBulan, parameter, tanggalPengajuan) { }

    // Pemulihan dari database
    internal PinjamanSaprodi(int idPinjaman, Anggota anggota, int tenorBulan,
                             decimal sukuBungaTahunan, DateOnly tanggalPengajuan)
        : base(idPinjaman, anggota, tenorBulan, sukuBungaTahunan, tanggalPengajuan) { }

    public void TambahItem(ItemSaprodi item)
    {
        if (Status != StatusPinjaman.Diajukan)
            throw new AturanBisnisException(
                "Item hanya dapat ditambah saat pinjaman berstatus Diajukan.");
        _daftarItem.Add(item);
    }

    // Dipanggil sebelum pengajuan disimpan
    public void PastikanSiapDiajukan()
    {
        if (_daftarItem.Count == 0)
            throw new AturanBisnisException(
                "Pinjaman saprodi harus memiliki minimal satu item.");
    }

    // B6: pokok = jumlah seluruh subtotal item
    public override decimal HitungPokokPinjaman()
        => _daftarItem.Sum(i => i.HitungSubtotal());

    public override string Rincian()
        => "Pinjaman uang untuk saprodi:\n" + string.Join("\n",
            _daftarItem.Select(i =>
                $"- {i.NamaItem}: {i.Jumlah:0.##} {i.Satuan} x Rp{i.HargaSatuan:N0}"));

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