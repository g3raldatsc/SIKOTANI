using Npgsql;
using SIKOTANI.Models;

namespace SIKOTANI.Data;

/// <summary>Satu-satunya tempat SQL untuk tabel pengguna.</summary>
public static class PenggunaRepository
{
    public static bool BelumAdaPengguna()
    {
        using var koneksi = Database.BukaKoneksi();
        using var perintah = new NpgsqlCommand("SELECT COUNT(*) FROM pengguna", koneksi);
        return (long)perintah.ExecuteScalar()! == 0;
    }

    public static Pengguna? CariByUsername(string username)
    {
        const string sql = @"
            SELECT p.id_pengguna, p.username, p.password_hash, p.peran, p.aktif,
                   a.id_anggota, a.nama, a.alamat, a.no_telepon, a.tanggal_gabung,
                   a.luas_lahan_ha, a.status
            FROM pengguna p
            JOIN anggota a ON a.id_anggota = p.id_anggota
            WHERE p.username = @username";

        using var koneksi = Database.BukaKoneksi();
        using var perintah = new NpgsqlCommand(sql, koneksi);
        perintah.Parameters.AddWithValue("username", username.Trim().ToLowerInvariant());

        using var pembaca = perintah.ExecuteReader();
        if (!pembaca.Read()) return null;

        var anggota = new Anggota(
            pembaca.GetInt32(5),
            pembaca.GetString(6),
            pembaca.IsDBNull(7) ? string.Empty : pembaca.GetString(7),
            pembaca.IsDBNull(8) ? string.Empty : pembaca.GetString(8),
            pembaca.GetFieldValue<DateOnly>(9),
            pembaca.GetDecimal(10),
            Enum.Parse<StatusAnggota>(pembaca.GetString(11)));

        int id = pembaca.GetInt32(0);
        string user = pembaca.GetString(1);
        string hash = pembaca.GetString(2);
        bool aktif = pembaca.GetBoolean(4);

        // Polymorphism: peran di database menentukan class turunan yang dibuat
        return pembaca.GetString(3) switch
        {
            "Ketua" => new Ketua(id, user, anggota, hash, aktif),
            "Sekretaris" => new Sekretaris(id, user, anggota, hash, aktif),
            "Bendahara" => new Bendahara(id, user, anggota, hash, aktif),
            _ => throw new InvalidOperationException("Peran di database tidak dikenal.")
        };
    }

    // Hanya boleh dipakai saat tabel pengguna masih kosong
    public static void BuatKetuaAwal(string nama, string username, string kataSandi)
    {
        if (!BelumAdaPengguna())
            throw new AturanBisnisException("Akun Ketua awal sudah pernah dibuat.");
        BuatPengguna(nama, username, kataSandi, "Ketua");
    }

    // A7: setiap pengguna adalah anggota, jadi data anggota dan akun dibuat bersamaan
    public static void BuatPengguna(string nama, string username, string kataSandi, string peran)
    {
        if (string.IsNullOrWhiteSpace(nama))
            throw new AturanBisnisException("Nama wajib diisi.");

        username = (username ?? string.Empty).Trim().ToLowerInvariant();
        if (username.Length < 3 || username.Any(char.IsWhiteSpace))
            throw new AturanBisnisException(
                "Username minimal 3 karakter dan tidak boleh mengandung spasi.");
        if (peran is not ("Ketua" or "Sekretaris" or "Bendahara"))
            throw new AturanBisnisException("Peran tidak valid.");

        string hash = KataSandi.Hash(kataSandi); // menolak sandi < 8 karakter

        using var koneksi = Database.BukaKoneksi();
        using var transaksi = koneksi.BeginTransaction(); // dua INSERT: semua atau tidak sama sekali

        try
        {
            int idAnggota;
            using (var cmdAnggota = new NpgsqlCommand(
                "INSERT INTO anggota (nama, tanggal_gabung) VALUES (@nama, @tgl) " +
                "RETURNING id_anggota", koneksi, transaksi))
            {
                cmdAnggota.Parameters.AddWithValue("nama", nama.Trim());
                cmdAnggota.Parameters.AddWithValue("tgl", DateOnly.FromDateTime(DateTime.Today));
                idAnggota = (int)cmdAnggota.ExecuteScalar()!;
            }

            using (var cmdPengguna = new NpgsqlCommand(
                "INSERT INTO pengguna (id_anggota, username, password_hash, peran) " +
                "VALUES (@id, @user, @hash, @peran)", koneksi, transaksi))
            {
                cmdPengguna.Parameters.AddWithValue("id", idAnggota);
                cmdPengguna.Parameters.AddWithValue("user", username);
                cmdPengguna.Parameters.AddWithValue("hash", hash);
                cmdPengguna.Parameters.AddWithValue("peran", peran);
                cmdPengguna.ExecuteNonQuery();
            }

            transaksi.Commit();
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            // transaksi dibatalkan otomatis saat di-dispose tanpa Commit
            throw new AturanBisnisException("Username sudah dipakai.");
        }
    }
}