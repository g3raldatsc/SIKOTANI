namespace SIKOTANI.Models;

/// <summary>Unit usaha untuk setoran panen (A8). Dicatat terpisah dari USP.</summary>
public class UnitHasilPanen
{
    // Composition: setoran dimiliki unit ini
    private readonly List<SetoranPanen> _setoran = new();

    public string Nama { get; }
    public IReadOnlyList<SetoranPanen> DaftarSetoran => _setoran;

    internal UnitHasilPanen(string nama = "Unit Hasil Panen")
    {
        Nama = nama;
    }

    public void TambahSetoran(SetoranPanen setoran)
    {
        if (_setoran.Contains(setoran))
            throw new AturanBisnisException("Setoran ini sudah tercatat di unit.");
        _setoran.Add(setoran);
    }

    public decimal TotalNilaiSetoran() => _setoran.Sum(s => s.HitungNilai());

    public decimal TotalNilaiTersedia() => _setoran.Sum(s => s.HitungNilaiTersedia());
}