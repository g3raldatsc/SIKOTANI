using Npgsql;

namespace SIKOTANI.Models.Repositories;

/// <summary>Satu-satunya tempat SQL untuk tabel pengguna.</summary>
public static class PenggunaRepository
{
    private const string KolomPengguna =
        "p.id_pengguna, p.username, p.password_hash, p.peran, p.aktif, " + AnggotaRepository.KolomAnggota;

    private const string DariPengguna =
        " FROM pengguna p JOIN anggota a ON a.id_anggota = p.id_anggota ";

    // buat READ
    public static bool BelumAdaPengguna()
    {
        using var koneksi = Database.BukaKoneksi();
        using var perintah = new NpgsqlCommand("SELECT COUNT(*) FROM pengguna", koneksi);
        return (long)perintah.ExecuteScalar()! == 0;
    }

    public static Pengguna? CariByUsername(string username)
        => Cari("WHERE p.username = @nilai", username.Trim().ToLowerInvariant()).FirstOrDefault();

    public static Pengguna? CariById(int id)
        => Cari("WHERE p.id_pengguna = @nilai", id).FirstOrDefault();

    public static List<Pengguna> DaftarSemua() => Cari(string.Empty, null);

    private static List<Pengguna> Cari(string kondisi, object? nilai)
    {
        string sql = $"SELECT {KolomPengguna}{DariPengguna}{kondisi} ORDER BY p.id_pengguna";

        var hasil = new List<Pengguna>();
        using var koneksi = Database.BukaKoneksi();
        using var cmd = new NpgsqlCommand(sql, koneksi);
        if (nilai is not null) cmd.Parameters.AddWithValue("nilai", nilai);
        using var r = cmd.ExecuteReader();
        while (r.Read()) hasil.Add(Bentuk(r));
        return hasil;
    }

    // Polymorphism: peran di database menentukan class turunan yang dibuat
    private static Pengguna Bentuk(NpgsqlDataReader r)
    {
        var anggota = AnggotaRepository.DariPembaca(r, 5);
        int id = r.GetInt32(0);
        string user = r.GetString(1);
        string hash = r.GetString(2);
        bool aktif = r.GetBoolean(4);

        return r.GetString(3) switch
        {
            "Ketua" => new Ketua(id, user, anggota, hash, aktif),
            "Sekretaris" => new Sekretaris(id, user, anggota, hash, aktif),
            "Bendahara" => new Bendahara(id, user, anggota, hash, aktif),
            _ => throw new InvalidOperationException("Peran di database tidak dikenal.")
        };
    }

    // Anggota Aktif yang belum memiliki akun (calon pengurus)
    public static List<Anggota> AnggotaTanpaAkun()
    {
        string sql = $@"
            SELECT {AnggotaRepository.KolomAnggota}
            FROM anggota a
            WHERE a.status = 'Aktif'
              AND NOT EXISTS (SELECT 1 FROM pengguna p WHERE p.id_anggota = a.id_anggota)
            ORDER BY a.nama";

        var hasil = new List<Anggota>();
        using var koneksi = Database.BukaKoneksi();
        using var cmd = new NpgsqlCommand(sql, koneksi);
        using var r = cmd.ExecuteReader();
        while (r.Read()) hasil.Add(AnggotaRepository.DariPembaca(r));
        return hasil;
    }

    // buat nge-CREATE akun
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
        username = PastikanUsernameValid(username);
        PastikanPeranValid(peran);
        string hash = KataSandi.Hash(kataSandi);   // ini bakal nolak sandi < 8 karakter

        using var koneksi = Database.BukaKoneksi();
        using var transaksi = koneksi.BeginTransaction();   // dua INSERT: semua atau tidak sama sekali

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

    // Akun untuk anggota yang sudah ada (A7: pengurus adalah anggota Aktif)
    public static void BuatAkun(int idAnggota, string username, string kataSandi, string peran)
    {
        var anggota = AnggotaRepository.CariById(idAnggota)
            ?? throw new AturanBisnisException("Data anggota tidak ditemukan.");
        if (!anggota.Aktif)
            throw new AturanBisnisException("Hanya anggota Aktif yang dapat memiliki akun pengurus.");

        username = PastikanUsernameValid(username);
        PastikanPeranValid(peran);
        string hash = KataSandi.Hash(kataSandi);   // menolak sandi < 8 karakter

        try
        {
            using var koneksi = Database.BukaKoneksi();
            using var cmd = new NpgsqlCommand(
                "INSERT INTO pengguna (id_anggota, username, password_hash, peran) " +
                "VALUES (@id, @user, @hash, @peran)", koneksi);
            cmd.Parameters.AddWithValue("id", idAnggota);
            cmd.Parameters.AddWithValue("user", username);
            cmd.Parameters.AddWithValue("hash", hash);
            cmd.Parameters.AddWithValue("peran", peran);
            cmd.ExecuteNonQuery();
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new AturanBisnisException(
                (ex.ConstraintName ?? string.Empty).Contains("username")
                    ? "Username sudah dipakai."
                    : "Anggota ini sudah memiliki akun pengurus.");
        }
    }

    // buat nge-UPDATE akun
    public static void UbahStatusAktif(int idPengguna, bool aktif)
    {
        using var koneksi = Database.BukaKoneksi();
        using var transaksi = koneksi.BeginTransaction();

        string peran;
        bool aktifSekarang;
        string statusAnggota;
        using (var cmd = new NpgsqlCommand(
            "SELECT p.peran, p.aktif, a.status FROM pengguna p " +
            "JOIN anggota a ON a.id_anggota = p.id_anggota " +
            "WHERE p.id_pengguna = @id FOR UPDATE OF p", koneksi, transaksi))
        {
            cmd.Parameters.AddWithValue("id", idPengguna);
            using var r = cmd.ExecuteReader();
            if (!r.Read()) throw new AturanBisnisException("Akun tidak ditemukan.");
            peran = r.GetString(0);
            aktifSekarang = r.GetBoolean(1);
            statusAnggota = r.GetString(2);
        }

        if (aktif == aktifSekarang)
            throw new AturanBisnisException(aktif ? "Akun sudah aktif." : "Akun sudah nonaktif.");
        if (aktif && statusAnggota != "Aktif")
            throw new AturanBisnisException(
                "Anggota pemilik akun sudah keluar, akun tidak dapat diaktifkan.");

        if (!aktif && peran == "Ketua")
        {
            long jumlahKetua;
            using (var cmd = new NpgsqlCommand(
                "SELECT COUNT(*) FROM pengguna WHERE peran = 'Ketua' AND aktif", koneksi, transaksi))
                jumlahKetua = (long)cmd.ExecuteScalar()!;
            if (jumlahKetua <= 1)
                throw new AturanBisnisException("Ketua aktif terakhir tidak dapat dinonaktifkan.");
        }

        using (var cmd = new NpgsqlCommand(
            "UPDATE pengguna SET aktif = @aktif WHERE id_pengguna = @id", koneksi, transaksi))
        {
            cmd.Parameters.AddWithValue("aktif", aktif);
            cmd.Parameters.AddWithValue("id", idPengguna);
            cmd.ExecuteNonQuery();
        }
        transaksi.Commit();
    }

    public static void AturKataSandi(int idPengguna, string passwordHash)
    {
        using var koneksi = Database.BukaKoneksi();
        using var cmd = new NpgsqlCommand(
            "UPDATE pengguna SET password_hash = @hash WHERE id_pengguna = @id", koneksi);
        cmd.Parameters.AddWithValue("hash", passwordHash);
        cmd.Parameters.AddWithValue("id", idPengguna);
        if (cmd.ExecuteNonQuery() == 0)
            throw new AturanBisnisException("Akun tidak ditemukan.");
    }

    // tAMBAHAN
    private static string PastikanUsernameValid(string username)
    {
        username = (username ?? string.Empty).Trim().ToLowerInvariant();
        if (username.Length < 3 || username.Any(char.IsWhiteSpace))
            throw new AturanBisnisException(
                "Username minimal 3 karakter dan tidak boleh mengandung spasi.");
        return username;
    }

    private static void PastikanPeranValid(string peran)
    {
        if (peran is not ("Ketua" or "Sekretaris" or "Bendahara"))
            throw new AturanBisnisException("Peran tidak valid.");
    }
}