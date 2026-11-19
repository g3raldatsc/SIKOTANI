using Npgsql;
using SIKOTANI.Models;

namespace SIKOTANI.Data;

public static class AnggotaRepository
{
    // Urutan kolom ini harus sama dengan DariPembaca oicfonwpebugewqi
    internal const string KolomAnggota =
        "a.id_anggota, a.nama, a.alamat, a.no_telepon, a.tanggal_gabung, a.luas_lahan_ha, a.status";

    internal static Anggota DariPembaca(NpgsqlDataReader r, int awal = 0)
    {
        return new Anggota(
            r.GetInt32(awal),
            r.GetString(awal + 1),
            r.IsDBNull(awal + 2) ? string.Empty : r.GetString(awal + 2),
            r.IsDBNull(awal + 3) ? string.Empty : r.GetString(awal + 3),
            r.GetFieldValue<DateOnly>(awal + 4),
            r.GetDecimal(awal + 5),
            Enum.Parse<StatusAnggota>(r.GetString(awal + 6)));
    }

    public static List<Anggota> Cari(string kata, bool termasukKeluar)
    {
        kata = (kata ?? string.Empty).Trim();
        string sql = $@"
            SELECT {KolomAnggota}
            FROM anggota a
            WHERE (@kata = '' OR a.nama ILIKE @pola OR COALESCE(a.no_telepon, '') ILIKE @pola)
              AND (@semua OR a.status = 'Aktif')
            ORDER BY a.nama";

        var hasil = new List<Anggota>();
        using var koneksi = Database.BukaKoneksi();
        using var cmd = new NpgsqlCommand(sql, koneksi);
        cmd.Parameters.AddWithValue("kata", kata);
        cmd.Parameters.AddWithValue("pola", "%" + kata + "%");
        cmd.Parameters.AddWithValue("semua", termasukKeluar);
        using var r = cmd.ExecuteReader();
        while (r.Read()) hasil.Add(DariPembaca(r));
        return hasil;
    }

    public static Anggota? CariById(int id)
    {
        string sql = $"SELECT {KolomAnggota} FROM anggota a WHERE a.id_anggota = @id";
        using var koneksi = Database.BukaKoneksi();
        using var cmd = new NpgsqlCommand(sql, koneksi);
        cmd.Parameters.AddWithValue("id", id);
        using var r = cmd.ExecuteReader();
        return r.Read() ? DariPembaca(r) : null;
    }

    public static int Tambah(string nama, string alamat, string noTelepon,
                             DateOnly tanggalGabung, decimal luasLahanHa)
    {
        // Validasi dilakukan class domain Anggota, bukan form
        var valid = new Anggota(0, nama, alamat, noTelepon, tanggalGabung, luasLahanHa);

        const string sql = @"
            INSERT INTO anggota (nama, alamat, no_telepon, tanggal_gabung, luas_lahan_ha)
            VALUES (@nama, @alamat, @telp, @tgl, @luas)
            RETURNING id_anggota";

        using var koneksi = Database.BukaKoneksi();
        using var cmd = new NpgsqlCommand(sql, koneksi);
        cmd.Parameters.AddWithValue("nama", valid.Nama);
        cmd.Parameters.AddWithValue("alamat", valid.Alamat);
        cmd.Parameters.AddWithValue("telp", valid.NoTelepon);
        cmd.Parameters.AddWithValue("tgl", valid.TanggalGabung);
        cmd.Parameters.AddWithValue("luas", valid.LuasLahanHa);
        return (int)cmd.ExecuteScalar()!;
    }

    public static void Ubah(int id, string nama, string alamat, string noTelepon, decimal luasLahanHa)
    {
        var anggota = CariById(id)
            ?? throw new AturanBisnisException("Data anggota tidak ditemukan.");
        anggota.UbahData(nama, alamat, noTelepon, luasLahanHa);   // validasi domain

        const string sql = @"
            UPDATE anggota
            SET nama = @nama, alamat = @alamat, no_telepon = @telp, luas_lahan_ha = @luas
            WHERE id_anggota = @id";

        using var koneksi = Database.BukaKoneksi();
        using var cmd = new NpgsqlCommand(sql, koneksi);
        cmd.Parameters.AddWithValue("nama", anggota.Nama);
        cmd.Parameters.AddWithValue("alamat", anggota.Alamat);
        cmd.Parameters.AddWithValue("telp", anggota.NoTelepon);
        cmd.Parameters.AddWithValue("luas", anggota.LuasLahanHa);
        cmd.Parameters.AddWithValue("id", id);
        cmd.ExecuteNonQuery();
    }

    // Anggota tidak dihapus, hanya berstatus Keluar, agar riwayat simpanan dan pinjaman tetap utuh
    public static void Keluarkan(int id)
    {
        var anggota = CariById(id)
            ?? throw new AturanBisnisException("Data anggota tidak ditemukan.");
        anggota.Keluar(); // domain menolak bila sudah Keluar

        using var koneksi = Database.BukaKoneksi();
        using var transaksi = koneksi.BeginTransaction();

        long Hitung(string sql)
        {
            using var cmdHitung = new NpgsqlCommand(sql, koneksi, transaksi);
            cmdHitung.Parameters.AddWithValue("id", id);
            return (long)cmdHitung.ExecuteScalar()!;
        }

        if (Hitung("SELECT COUNT(*) FROM pinjaman WHERE id_anggota = @id " +
                   "AND status IN ('Diajukan', 'Aktif')") > 0)
            throw new AturanBisnisException(
                "Anggota masih memiliki pinjaman Diajukan atau Aktif, belum dapat dikeluarkan.");

        if (Hitung("SELECT COUNT(*) FROM pengguna WHERE id_anggota = @id AND aktif") > 0)
            throw new AturanBisnisException(
                "Anggota ini memiliki akun pengurus yang masih aktif, belum dapat dikeluarkan.");

        using (var cmdStatus = new NpgsqlCommand(
            "UPDATE anggota SET status = 'Keluar' WHERE id_anggota = @id", koneksi, transaksi))
        {
            cmdStatus.Parameters.AddWithValue("id", id);
            cmdStatus.ExecuteNonQuery();
        }

        // Anggota yang keluar otomatis dilepas dari semua kelompok tani
        using (var cmdLepas = new NpgsqlCommand(
            "DELETE FROM keanggotaan_kelompok WHERE id_anggota = @id", koneksi, transaksi))
        {
            cmdLepas.Parameters.AddWithValue("id", id);
            cmdLepas.ExecuteNonQuery();
        }

        transaksi.Commit();
    }
}