using Npgsql;
using SIKOTANI.Models;

namespace SIKOTANI.Data;

public static class KomoditasRepository
{
    public static List<Komoditas> DaftarSemua()
    {
        var hasil = new List<Komoditas>();
        using var koneksi = Database.BukaKoneksi();
        using var cmd = new NpgsqlCommand(
            "SELECT id_komoditas, nama, satuan FROM komoditas ORDER BY nama", koneksi);
        using var r = cmd.ExecuteReader();
        while (r.Read())
            hasil.Add(new Komoditas(r.GetInt32(0), r.GetString(1), r.GetString(2)));
        return hasil;
    }

    public static void Tambah(string nama, string satuan)
    {
        var valid = new Komoditas(0, nama, satuan); // validasi domain
        try
        {
            using var koneksi = Database.BukaKoneksi();
            using var cmd = new NpgsqlCommand(
                "INSERT INTO komoditas (nama, satuan) VALUES (@nama, @satuan)", koneksi);
            cmd.Parameters.AddWithValue("nama", valid.Nama);
            cmd.Parameters.AddWithValue("satuan", valid.Satuan);
            cmd.ExecuteNonQuery();
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new AturanBisnisException("Komoditas dengan nama itu sudah ada.");
        }
    }

    public static void Hapus(int id)
    {
        try
        {
            using var koneksi = Database.BukaKoneksi();
            using var cmd = new NpgsqlCommand(
                "DELETE FROM komoditas WHERE id_komoditas = @id", koneksi);
            cmd.Parameters.AddWithValue("id", id);
            cmd.ExecuteNonQuery();
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            throw new AturanBisnisException(
                "Komoditas sudah dipakai pada setoran panen, tidak dapat dihapus.");
        }
    }

    // Association many to many aruwyg: Anggota dengan Komoditas
    public static List<int> IdKomoditasAnggota(int idAnggota)
    {
        var hasil = new List<int>();
        using var koneksi = Database.BukaKoneksi();
        using var cmd = new NpgsqlCommand(
            "SELECT id_komoditas FROM anggota_komoditas WHERE id_anggota = @id", koneksi);
        cmd.Parameters.AddWithValue("id", idAnggota);
        using var r = cmd.ExecuteReader();
        while (r.Read()) hasil.Add(r.GetInt32(0));
        return hasil;
    }

    public static void SetKomoditasAnggota(int idAnggota, IEnumerable<int> idKomoditas)
    {
        using var koneksi = Database.BukaKoneksi();
        using var transaksi = koneksi.BeginTransaction();

        using (var hapus = new NpgsqlCommand(
            "DELETE FROM anggota_komoditas WHERE id_anggota = @id", koneksi, transaksi))
        {
            hapus.Parameters.AddWithValue("id", idAnggota);
            hapus.ExecuteNonQuery();
        }

        foreach (int idK in idKomoditas.Distinct())
        {
            using var tambah = new NpgsqlCommand(
                "INSERT INTO anggota_komoditas (id_anggota, id_komoditas) VALUES (@a, @k)",
                koneksi, transaksi);
            tambah.Parameters.AddWithValue("a", idAnggota);
            tambah.Parameters.AddWithValue("k", idK);
            tambah.ExecuteNonQuery();
        }

        transaksi.Commit();
    }
}