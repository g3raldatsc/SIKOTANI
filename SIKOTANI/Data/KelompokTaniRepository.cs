using Npgsql;
using SIKOTANI.Models;

namespace SIKOTANI.Data;

public static class KelompokTaniRepository
{
    public static List<KelompokTani> DaftarSemua()
    {
        var hasil = new List<KelompokTani>();
        using var koneksi = Database.BukaKoneksi();

        using (var cmd = new NpgsqlCommand(
            "SELECT id_kelompok, nama, desa FROM kelompok_tani ORDER BY nama", koneksi))
        using (var r = cmd.ExecuteReader())
        {
            while (r.Read())
                hasil.Add(new KelompokTani(r.GetInt32(0), r.GetString(1),
                                           r.IsDBNull(2) ? string.Empty : r.GetString(2)));
        }

        // Anggota tiap kelompok (hanya yang masih Aktif)
        string sqlAnggota = $@"
            SELECT kk.id_kelompok, {AnggotaRepository.KolomAnggota}
            FROM keanggotaan_kelompok kk
            JOIN anggota a ON a.id_anggota = kk.id_anggota
            WHERE a.status = 'Aktif'
            ORDER BY a.nama";

        using (var cmd2 = new NpgsqlCommand(sqlAnggota, koneksi))
        using (var r2 = cmd2.ExecuteReader())
        {
            while (r2.Read())
            {
                var kelompok = hasil.First(k => k.IdKelompok == r2.GetInt32(0));
                kelompok.TambahAnggota(AnggotaRepository.DariPembaca(r2, 1));
            }
        }
        return hasil;
    }

    public static KelompokTani? Muat(int idKelompok)
        => DaftarSemua().FirstOrDefault(k => k.IdKelompok == idKelompok);

    public static int Tambah(string nama, string desa)
    {
        var valid = new KelompokTani(0, nama, desa); // buat validasi domain
        using var koneksi = Database.BukaKoneksi();
        using var cmd = new NpgsqlCommand(
            "INSERT INTO kelompok_tani (nama, desa) VALUES (@nama, @desa) RETURNING id_kelompok",
            koneksi);
        cmd.Parameters.AddWithValue("nama", valid.Nama);
        cmd.Parameters.AddWithValue("desa", valid.Desa);
        return (int)cmd.ExecuteScalar()!;
    }

    public static void Ubah(int id, string nama, string desa)
    {
        var valid = new KelompokTani(id, nama, desa);
        using var koneksi = Database.BukaKoneksi();
        using var cmd = new NpgsqlCommand(
            "UPDATE kelompok_tani SET nama = @nama, desa = @desa WHERE id_kelompok = @id", koneksi);
        cmd.Parameters.AddWithValue("nama", valid.Nama);
        cmd.Parameters.AddWithValue("desa", valid.Desa);
        cmd.Parameters.AddWithValue("id", id);
        cmd.ExecuteNonQuery();
    }

    // Aggregation: yang terhapus kelompok dan tautannya (ON DELETE CASCADE), anggota tetap ada
    public static void Hapus(int id)
    {
        using var koneksi = Database.BukaKoneksi();
        using var cmd = new NpgsqlCommand(
            "DELETE FROM kelompok_tani WHERE id_kelompok = @id", koneksi);
        cmd.Parameters.AddWithValue("id", id);
        cmd.ExecuteNonQuery();
    }

    public static void TambahAnggota(int idKelompok, int idAnggota)
    {
        var kelompok = Muat(idKelompok)
            ?? throw new AturanBisnisException("Kelompok tani tidak ditemukan.");
        var anggota = AnggotaRepository.CariById(idAnggota)
            ?? throw new AturanBisnisException("Anggota tidak ditemukan.");

        kelompok.TambahAnggota(anggota); // validasi domain: harus Aktif, tidak ganda

        using var koneksi = Database.BukaKoneksi();
        using var cmd = new NpgsqlCommand(
            "INSERT INTO keanggotaan_kelompok (id_kelompok, id_anggota) VALUES (@k, @a)", koneksi);
        cmd.Parameters.AddWithValue("k", idKelompok);
        cmd.Parameters.AddWithValue("a", idAnggota);
        cmd.ExecuteNonQuery();
    }

    public static void KeluarkanAnggota(int idKelompok, int idAnggota)
    {
        var kelompok = Muat(idKelompok)
            ?? throw new AturanBisnisException("Kelompok tani tidak ditemukan.");
        var anggota = AnggotaRepository.CariById(idAnggota)
            ?? throw new AturanBisnisException("Anggota tidak ditemukan.");

        kelompok.KeluarkanAnggota(anggota); // validasi domain

        using var koneksi = Database.BukaKoneksi();
        using var cmd = new NpgsqlCommand(
            "DELETE FROM keanggotaan_kelompok WHERE id_kelompok = @k AND id_anggota = @a", koneksi);
        cmd.Parameters.AddWithValue("k", idKelompok);
        cmd.Parameters.AddWithValue("a", idAnggota);
        cmd.ExecuteNonQuery();
    }
}