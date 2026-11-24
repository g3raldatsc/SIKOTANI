using System.Security.Cryptography;

namespace SIKOTANI.Models;

/// <summary>Pembuat dan pemeriksa hash kata sandi (PBKDF2-SHA256 + salt acak).</summary>
public static class KataSandi
{
    private const int Iterasi = 600_000;
    private const int UkuranSalt = 16;
    private const int UkuranHash = 32;

    public static string Hash(string kataSandi)
    {
        if (string.IsNullOrWhiteSpace(kataSandi) || kataSandi.Length < 8)
            throw new AturanBisnisException("Kata sandi minimal 8 karakter.");

        byte[] salt = RandomNumberGenerator.GetBytes(UkuranSalt);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            kataSandi, salt, Iterasi, HashAlgorithmName.SHA256, UkuranHash);

        return $"pbkdf2-sha256${Iterasi}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public static bool Verifikasi(string kataSandi, string tersimpan)
    {
        try
        {
            var bagian = tersimpan.Split('$');
            if (bagian.Length != 4 || bagian[0] != "pbkdf2-sha256" ||
                !int.TryParse(bagian[1], out int iterasi))
                return false;

            byte[] salt = Convert.FromBase64String(bagian[2]);
            byte[] hashTersimpan = Convert.FromBase64String(bagian[3]);
            byte[] hashBaru = Rfc2898DeriveBytes.Pbkdf2(
                kataSandi, salt, iterasi, HashAlgorithmName.SHA256, hashTersimpan.Length);

            return CryptographicOperations.FixedTimeEquals(hashBaru, hashTersimpan);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}