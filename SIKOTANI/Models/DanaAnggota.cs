namespace SIKOTANI.Models;

public abstract class DanaAnggota
{
    private decimal _saldo; // private: enkapsulasi
    private readonly List<TransaksiDana> _transaksi = new(); // composition

    public int IdDana { get; internal set; }
    public Anggota Anggota { get; } // association
    public decimal Saldo => _saldo;
    public IReadOnlyList<TransaksiDana> Transaksi => _transaksi;
    public abstract string NamaJenis { get; }

    protected DanaAnggota(int idDana, Anggota anggota, decimal saldoAwal = 0m)
    {
        if (saldoAwal < 0)
            throw new AturanBisnisException("Saldo awal tidak boleh negatif.");
        IdDana = idDana;
        Anggota = anggota;
        _saldo = saldoAwal;
    }

    // virtual: boleh di-override oleh class turunan
    public virtual void Setor(decimal jumlah, DateOnly tanggal)
    {
        PastikanJumlahPositif(jumlah);
        if (!Anggota.Aktif)
            throw new AturanBisnisException("Anggota sudah keluar, setoran ditolak.");

        _saldo += jumlah;
        _transaksi.Add(new TransaksiDana(tanggal, JenisTransaksi.Setor, jumlah, _saldo));
    }

    // abstract: tiap jenis dana wajib punya aturan penarikan sendiri
    public abstract void Tarik(decimal jumlah, DateOnly tanggal);

    public abstract decimal HitungImbalan(ParameterKoperasi parameter);

    // Satu-satunya jalan mengurangi saldo; dipakai class turunan.
    protected void KurangiSaldo(decimal jumlah, DateOnly tanggal)
    {
        PastikanJumlahPositif(jumlah);
        if (jumlah > _saldo)
            throw new AturanBisnisException(
                $"Saldo {NamaJenis.ToLowerInvariant()} tidak mencukupi.");

        _saldo -= jumlah;
        _transaksi.Add(new TransaksiDana(tanggal, JenisTransaksi.Tarik, jumlah, _saldo));
    }

    protected static void PastikanJumlahPositif(decimal jumlah)
    {
        if (jumlah <= 0)
            throw new AturanBisnisException("Jumlah harus lebih dari 0.");
    }
}