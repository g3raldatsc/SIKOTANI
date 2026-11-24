namespace SIKOTANI.Models;

public class SimpananWajib : DanaAnggota
{
    public override string NamaJenis => "Simpanan Wajib";

    public SimpananWajib(int idDana, Anggota anggota, decimal saldoAwal = 0m)
        : base(idDana, anggota, saldoAwal) { }

    // Aturan A2
    public override void Tarik(decimal jumlah, DateOnly tanggal)
    {
        if (Anggota.Aktif)
            throw new AturanBisnisException(
                "Simpanan wajib tidak dapat diambil selama masih menjadi anggota.");
        KurangiSaldo(jumlah, tanggal);
    }

    public override decimal HitungImbalan(ParameterKoperasi parameter) => 0m;
}