using System.Text;

namespace SIKOTANI.Models;

/// <summary>Satu laporan berbentuk tabel. Nilai disimpan mentah; format tampilan dan CSV dibuat di sini.</summary>
public class TabelLaporan
{
    private readonly List<object?[]> _baris = new();

    public string Judul { get; }
    public IReadOnlyList<string> Kolom { get; }
    public IReadOnlyList<bool> KolomUang { get; }
    public IReadOnlyList<object?[]> Baris => _baris;
    public string Ringkasan { get; private set; } = string.Empty;

    public TabelLaporan(string judul, params (string Nama, bool Uang)[] kolom)
    {
        if (kolom.Length == 0)
            throw new AturanBisnisException("Laporan harus memiliki minimal satu kolom.");

        Judul = judul;
        Kolom = kolom.Select(k => k.Nama).ToList();
        KolomUang = kolom.Select(k => k.Uang).ToList();
    }

    public void TambahBaris(params object?[] nilai)
    {
        if (nilai.Length != Kolom.Count)
            throw new InvalidOperationException("Jumlah nilai tidak sama dengan jumlah kolom.");
        _baris.Add(nilai);
    }

    public void AturRingkasan(string teks) => Ringkasan = teks;

    /// <summary>Teks yang ditampilkan di tabel.</summary>
    public string Tampil(object? nilai, int kolom) => nilai switch
    {
        null => "-",
        decimal d when KolomUang[kolom] => FormatTeks.Rp(d),
        decimal d => FormatTeks.Angka(d),
        DateOnly t => FormatTeks.Tanggal(t),
        _ => nilai.ToString() ?? string.Empty
    };

    /// <summary>CSV dipisahkan oleh titik koma; angka ditulis mentah.</summary>
    public string KeCsv()
    {
        static string Esc(string s)
            => s.IndexOfAny(new[] { ';', '"', '\n', '\r' }) >= 0
                ? "\"" + s.Replace("\"", "\"\"") + "\""
                : s;

        static string Sel(object? nilai) => nilai switch
        {
            null => string.Empty,
            decimal d => d.ToString("0.##", FormatTeks.Budaya),
            DateOnly t => t.ToString("yyyy-MM-dd"),
            _ => nilai.ToString() ?? string.Empty
        };

        var sb = new StringBuilder();
        sb.AppendLine(string.Join(';', Kolom.Select(Esc)));
        foreach (var baris in _baris)
            sb.AppendLine(string.Join(';', baris.Select(n => Esc(Sel(n)))));
        return sb.ToString();
    }
}