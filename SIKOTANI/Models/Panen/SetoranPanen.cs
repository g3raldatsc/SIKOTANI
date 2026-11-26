namespace SIKOTANI.Models;

public class SetoranPanen
{
    private decimal _terpakai;   // bagian nilai setoran yang sudah dipotong

    public int IdSetoran { get; internal set; }
    public Anggota Anggota { get; }         // association
    public Komoditas Komoditas { get; }     // association
    public DateOnly Tanggal { get; }
    public decimal BeratKg { get; }
    public decimal HargaPerKg { get; }
    public decimal Terpakai => _terpakai;

    // Setoran baru: anggota harus Aktif
    public SetoranPanen(int idSetoran, Anggota anggota, Komoditas komoditas,
                        DateOnly tanggal, decimal beratKg, decimal hargaPerKg)
        : this(idSetoran, anggota, komoditas, tanggal, beratKg, hargaPerKg, 0m)
    {
        if (!anggota.Aktif)
            throw new AturanBisnisException("Anggota sudah keluar, setoran panen ditolak.");
    }

    // Pemulihan dari database: anggota yang kini sudah keluar tetap memiliki riwayat setorannya
    internal SetoranPanen(int idSetoran, Anggota anggota, Komoditas komoditas,
                          DateOnly tanggal, decimal beratKg, decimal hargaPerKg,
                          decimal terpakai)
    {
        if (beratKg <= 0)
            throw new AturanBisnisException("Berat setoran harus lebih dari 0 kg.");
        if (hargaPerKg < 0)
            throw new AturanBisnisException("Harga per kg tidak boleh negatif.");

        IdSetoran = idSetoran;
        Anggota = anggota;
        Komoditas = komoditas;
        Tanggal = tanggal;
        BeratKg = beratKg;
        HargaPerKg = hargaPerKg;

        if (terpakai < 0 || terpakai > HitungNilai())
            throw new AturanBisnisException("Nilai terpakai setoran tidak valid.");
        _terpakai = terpakai;
    }

    // B9: nilai = berat x harga per kg
    public decimal HitungNilai()
        => Math.Round(BeratKg * HargaPerKg, 0, MidpointRounding.AwayFromZero);

    public decimal HitungNilaiTersedia() => HitungNilai() - _terpakai;

    // B10: potong panen tidak boleh melebihi nilai setoran maupun sisa tagihan
    public void PotongUntukAngsuran(Pinjaman pinjaman, int noAngsuran,
                                    decimal jumlah, DateOnly tanggal)
    {
        if (jumlah <= 0)
            throw new AturanBisnisException("Jumlah potongan harus lebih dari 0.");
        if (pinjaman.Anggota.IdAnggota != Anggota.IdAnggota)
            throw new AturanBisnisException(
                "Setoran hanya dapat dipotong untuk pinjaman milik anggota yang sama.");
        if (tanggal < Tanggal)
            throw new AturanBisnisException(
                "Tanggal pemotongan tidak boleh sebelum tanggal setoran.");
        if (jumlah > HitungNilaiTersedia())
            throw new AturanBisnisException(
                "Jumlah potongan melebihi nilai setoran yang tersedia.");

        // Bila ini gagal (misalnya melebihi tagihan), _terpakai belum berubah.
        pinjaman.BayarAngsuran(noAngsuran, jumlah, tanggal,
                               SumberPembayaran.PotongPanen, this);
        _terpakai += jumlah;
    }
}