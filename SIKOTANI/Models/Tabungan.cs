namespace SIKOTANI.Models;

public class Tabungan : DanaAnggota
{
    public override string NamaJenis => "Tabungan";

    public Tabungan(int idDana, Anggota anggota, decimal saldoAwal = 0m)
        : base(idDana, anggota, saldoAwal) { }

    // Aturan A3 dan B1: boleh ditarik kapan saja selama saldo cukup
    public override void Tarik(decimal jumlah, DateOnly tanggal)
        => KurangiSaldo(jumlah, tanggal);

    // Imbalan sebulan = saldo x bunga tahunan / 12 (dibulatkan ke rupiah)
    public override decimal HitungImbalan(ParameterKoperasi parameter)
    {
        decimal imbalan = Saldo * (parameter.BungaTabunganBerlaku / 100m) / 12m;
        return Math.Round(imbalan, 0, MidpointRounding.AwayFromZero);
    }
}