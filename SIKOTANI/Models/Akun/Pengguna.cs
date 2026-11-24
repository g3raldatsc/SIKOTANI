namespace SIKOTANI.Models;

public abstract class Pengguna
{
    private string _passwordHash; // private: enkapsulasi

    public int IdPengguna { get; internal set; }
    public string Username { get; }
    public Anggota Anggota { get; } // association (A7: pengurus adalah anggota)
    public bool Aktif { get; private set; }
    internal string PasswordHash => _passwordHash;

    // Polymorphism: tiap peran punya nama dan hak akses sendiri
    public abstract string GetPeran();
    public abstract HakAkses GetHakAkses(MenuAplikasi menu);

    protected Pengguna(int idPengguna, string username, Anggota anggota,
                       string passwordHash, bool aktif = true)
    {
        if (string.IsNullOrWhiteSpace(username) || username.Trim().Length < 3)
            throw new AturanBisnisException("Username minimal 3 karakter.");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new AturanBisnisException("Hash kata sandi wajib ada.");

        IdPengguna = idPengguna;
        Username = username.Trim().ToLowerInvariant();
        Anggota = anggota;
        _passwordHash = passwordHash;
        Aktif = aktif;
    }

    public bool BolehAkses(MenuAplikasi menu) => GetHakAkses(menu) != HakAkses.Tidak;
    public bool BolehKelola(MenuAplikasi menu) => GetHakAkses(menu) == HakAkses.Kelola;

    public void PastikanBolehAkses(MenuAplikasi menu)
    {
        if (!BolehAkses(menu))
            throw new AturanBisnisException(
                $"Peran {GetPeran()} tidak berhak membuka menu {menu}.");
    }

    // Akun hanya sah bila aktif dan anggotanya masih Aktif
    public bool VerifikasiKataSandi(string kataSandi)
        => Aktif && Anggota.Aktif && KataSandi.Verifikasi(kataSandi, _passwordHash);

    public void UbahKataSandi(string kataSandiBaru)
        => _passwordHash = KataSandi.Hash(kataSandiBaru);

    public void Nonaktifkan() => Aktif = false;
}