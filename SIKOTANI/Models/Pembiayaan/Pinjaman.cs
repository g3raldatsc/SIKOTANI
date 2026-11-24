namespace SIKOTANI.Models;

public abstract class Pinjaman
{
    private readonly List<Angsuran> _angsuran = new(); // composition

    public int IdPinjaman { get; internal set; }
    public Anggota Anggota { get; } // association
    public int TenorBulan { get; }
    public decimal SukuBungaTahunan { get; }
    public DateOnly TanggalPengajuan { get; }
    public DateOnly? TanggalCair { get; private set; }
    public StatusPinjaman Status { get; private set; } = StatusPinjaman.Diajukan;
    public IReadOnlyList<Angsuran> JadwalAngsuran => _angsuran;
    public abstract string NamaJenis { get; }

    protected Pinjaman(int idPinjaman, Anggota anggota, int tenorBulan,
                       ParameterKoperasi parameter, DateOnly tanggalPengajuan)
    {
        if (!anggota.Aktif) // B2
            throw new AturanBisnisException(
                "Hanya anggota Aktif yang dapat mengajukan pinjaman.");
        if (tenorBulan <= 0)
            throw new AturanBisnisException("Tenor harus lebih dari 0 bulan.");

        IdPinjaman = idPinjaman;
        Anggota = anggota;
        TenorBulan = tenorBulan;
        TanggalPengajuan = tanggalPengajuan;
        SukuBungaTahunan = parameter.BungaPinjamanBerlaku; // B4 (sudah dijaga <= batas)
    }

    // Polymorphism: tiap jenis pinjaman menghitung pokok dan jadwal dengan caranya
    public abstract decimal HitungPokokPinjaman();
    public abstract List<Angsuran> BuatJadwalAngsuran(DateOnly tanggalCair);

    // Bunga sebulan dari sisa pokok (bunga efektif), dibulatkan ke rupiah
    protected decimal HitungBungaSebulan(decimal sisaPokok)
        => Math.Round(sisaPokok * (SukuBungaTahunan / 100m) / 12m, 0,
                      MidpointRounding.AwayFromZero);

    public void Setujui(DateOnly tanggal)
    {
        if (Status != StatusPinjaman.Diajukan)
            throw new AturanBisnisException(
                "Hanya pinjaman berstatus Diajukan yang dapat disetujui.");
        if (tanggal < TanggalPengajuan)
            throw new AturanBisnisException(
                "Tanggal persetujuan tidak boleh sebelum tanggal pengajuan.");
        if (HitungPokokPinjaman() <= 0)
            throw new AturanBisnisException(
                "Pokok pinjaman harus lebih dari 0 (isi jumlah atau tambahkan item saprodi).");

        var jadwal = BuatJadwalAngsuran(tanggal);
        _angsuran.AddRange(jadwal);
        TanggalCair = tanggal;
        Status = StatusPinjaman.Aktif;
    }

    public void Tolak()
    {
        if (Status != StatusPinjaman.Diajukan)
            throw new AturanBisnisException(
                "Hanya pinjaman berstatus Diajukan yang dapat ditolak.");
        Status = StatusPinjaman.Ditolak;
    }

    public void BayarAngsuran(int noAngsuran, decimal jumlah, DateOnly tanggal,
                              SumberPembayaran sumber, SetoranPanen? setoran = null)
    {
        if (Status != StatusPinjaman.Aktif)
            throw new AturanBisnisException("Pinjaman tidak berstatus Aktif.");

        var angsuran = _angsuran.FirstOrDefault(a => a.NoAngsuran == noAngsuran)
            ?? throw new AturanBisnisException($"Angsuran ke-{noAngsuran} tidak ditemukan.");

        angsuran.CatatPembayaran(new PembayaranAngsuran(tanggal, jumlah, sumber, setoran));
        CekLunas();
    }

    public decimal GetSisaPokok()
    {
        if (Status == StatusPinjaman.Ditolak) return 0m;
        if (_angsuran.Count == 0) return HitungPokokPinjaman();
        return _angsuran.Sum(a => a.GetSisaPokok());
    }

    public bool CekLunas()
    {
        if (Status == StatusPinjaman.Aktif && _angsuran.Count > 0 &&
            _angsuran.All(a => a.Status == StatusAngsuran.Lunas))
        {
            Status = StatusPinjaman.Lunas;
        }
        return Status == StatusPinjaman.Lunas;
    }
}