namespace SIKOTANI.Models;

public class SetoranPanen
{
    private decimal _terpakai; // bagian nilai setoran yang sudah dipotong

    public int IdSetoran { get; internal set; }
    public Anggota Anggota { get; } // association
    public Komoditas Komoditas { get; } // association
    public DateOnly Tanggal { get; }
    public decimal BeratKg { get; }
    public decimal HargaPerKg { get; }

    public SetoranPanen(int idSetoran, Anggota anggota, Komoditas komoditas,
                        DateOnly tanggal, decimal beratKg, decimal hargaPerKg)
    {
        if (!anggota.Aktif)
            throw new AturanBisnisException("Anggota sudah keluar, setoran panen ditolak.");
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
        if (jumlah > HitungNilaiTersedia())
            throw new AturanBisnisException(
                "Jumlah potongan melebihi nilai setoran yang tersedia.");

        // Bila ini gagal (misalnya melebihi tagihan), _terpakai belum berubah.
        pinjaman.BayarAngsuran(noAngsuran, jumlah, tanggal, SumberPembayaran.PotongPanen, this);
        _terpakai += jumlah;
    }
}