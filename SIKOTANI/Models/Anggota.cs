namespace SIKOTANI.Models;

public class Anggota
{
    public int IdAnggota { get; internal set; }
    public string Nama { get; private set; } = string.Empty;
    public string Alamat { get; private set; } = string.Empty;
    public string NoTelepon { get; private set; } = string.Empty;
    public DateOnly TanggalGabung { get; private set; }
    public decimal LuasLahanHa { get; private set; }
    public StatusAnggota Status { get; private set; }

    public bool Aktif => Status == StatusAnggota.Aktif;

    public Anggota(int idAnggota, string nama, string alamat, string noTelepon,
                   DateOnly tanggalGabung, decimal luasLahanHa,
                   StatusAnggota status = StatusAnggota.Aktif)
    {
        IdAnggota = idAnggota;
        TanggalGabung = tanggalGabung;
        Status = status;
        UbahData(nama, alamat, noTelepon, luasLahanHa);
    }

    public void UbahData(string nama, string alamat, string noTelepon, decimal luasLahanHa)
    {
        if (string.IsNullOrWhiteSpace(nama))
            throw new AturanBisnisException("Nama anggota wajib diisi.");
        if (luasLahanHa < 0)
            throw new AturanBisnisException("Luas lahan tidak boleh negatif.");

        Nama = nama.Trim();
        Alamat = (alamat ?? string.Empty).Trim();
        NoTelepon = (noTelepon ?? string.Empty).Trim();
        LuasLahanHa = luasLahanHa;
    }

    public void Keluar()
    {
        if (!Aktif)
            throw new AturanBisnisException("Anggota sudah berstatus Keluar.");
        Status = StatusAnggota.Keluar;
    }

    public string TampilkanProfil()
        => $"{Nama} | {NoTelepon} | Lahan {LuasLahanHa:0.##} ha | {Status}";
}