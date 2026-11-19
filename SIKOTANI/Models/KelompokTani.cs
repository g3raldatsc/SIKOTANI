namespace SIKOTANI.Models;

public class KelompokTani
{
    // Aggregation: anggota hidup di luar kelompok, kelompok hanya merujuk
    private readonly List<Anggota> _daftarAnggota = new();

    public int IdKelompok { get; internal set; }
    public string Nama { get; }
    public string Desa { get; }
    public IReadOnlyList<Anggota> DaftarAnggota => _daftarAnggota;

    public KelompokTani(int idKelompok, string nama, string desa)
    {
        if (string.IsNullOrWhiteSpace(nama))
            throw new AturanBisnisException("Nama kelompok tani wajib diisi.");
        IdKelompok = idKelompok;
        Nama = nama.Trim();
        Desa = (desa ?? string.Empty).Trim();
    }

    public void TambahAnggota(Anggota anggota)
    {
        if (!anggota.Aktif)
            throw new AturanBisnisException(
                $"{anggota.Nama} sudah keluar dari koperasi, tidak dapat bergabung ke kelompok.");
        if (_daftarAnggota.Any(a => a.IdAnggota == anggota.IdAnggota))
            throw new AturanBisnisException(
                $"{anggota.Nama} sudah menjadi anggota kelompok {Nama}.");
        _daftarAnggota.Add(anggota);
    }

    public void KeluarkanAnggota(Anggota anggota)
    {
        var ada = _daftarAnggota.FirstOrDefault(a => a.IdAnggota == anggota.IdAnggota)
            ?? throw new AturanBisnisException(
                $"{anggota.Nama} bukan anggota kelompok {Nama}.");
        _daftarAnggota.Remove(ada);   // hanya tautan yang lepas, objek Anggota tetap ada
    }

    public int HitungJumlahAnggota() => _daftarAnggota.Count;
}