namespace SIKOTANI.Models;

/// <summary>Unit usaha untuk dana anggota dan pinjaman (A8). Gak menyimpan setoran panen.</summary>
public class UnitSimpanPinjam
{
    // Composition: dana dan pinjaman dimiliki unit ini
    private readonly List<DanaAnggota> _dana = new();
    private readonly List<Pinjaman> _pinjaman = new();

    public string Nama { get; }
    public IReadOnlyList<DanaAnggota> DaftarDana => _dana;
    public IReadOnlyList<Pinjaman> DaftarPinjaman => _pinjaman;

    // internal: hanya Koperasi yang membuat unitnya
    internal UnitSimpanPinjam(string nama = "Unit Simpan Pinjam")
    {
        Nama = nama;
    }

    public void TambahDana(DanaAnggota dana)
    {
        // Cermin dari UNIQUE (id_anggota, jenis_dana) di database
        if (_dana.Any(d => d.Anggota.IdAnggota == dana.Anggota.IdAnggota
                           && d.GetType() == dana.GetType()))
            throw new AturanBisnisException(
                $"{dana.Anggota.Nama} sudah memiliki {dana.NamaJenis.ToLowerInvariant()}.");
        _dana.Add(dana);
    }

    public void TambahPinjaman(Pinjaman pinjaman)
    {
        if (_pinjaman.Contains(pinjaman))
            throw new AturanBisnisException("Pinjaman ini sudah tercatat di unit.");
        _pinjaman.Add(pinjaman);
    }

    public decimal TotalSaldoDana() => _dana.Sum(d => d.Saldo);

    public decimal TotalSisaPinjaman()
        => _pinjaman.Where(p => p.Status == StatusPinjaman.Aktif)
                    .Sum(p => p.GetSisaPokok());
}