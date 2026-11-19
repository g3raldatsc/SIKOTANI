namespace SIKOTANI.Models;

public class SimpananPokok : DanaAnggota
{
    private readonly decimal _nilaiWajib;

    public override string NamaJenis => "Simpanan Pokok";

    public SimpananPokok(int idDana, Anggota anggota, decimal nilaiWajib,
                         decimal saldoAwal = 0m)
        : base(idDana, anggota, saldoAwal)
    {
        _nilaiWajib = nilaiWajib;
    }

    // Aturan B3: dibayar sekali, jumlahnya sama dengan parameter
    public override void Setor(decimal jumlah, DateOnly tanggal)
    {
        if (Saldo > 0)
            throw new AturanBisnisException("Simpanan pokok sudah dibayar.");
        if (jumlah != _nilaiWajib)
            throw new AturanBisnisException(
                $"Simpanan pokok harus sebesar Rp{_nilaiWajib:N0}.");
        base.Setor(jumlah, tanggal);
    }

    // Aturan A1
    public override void Tarik(decimal jumlah, DateOnly tanggal)
    {
        if (Anggota.Aktif)
            throw new AturanBisnisException(
                "Simpanan pokok tidak dapat diambil selama masih menjadi anggota.");
        KurangiSaldo(jumlah, tanggal);
    }

    public override decimal HitungImbalan(ParameterKoperasi parameter) => 0m;
}