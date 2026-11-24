using Npgsql;

namespace SIKOTANI.Models.Repositories;

public static class RingkasanRepository
{
    public static RingkasanDashboard Muat()
    {
        const string sql = @"
            SELECT
              (SELECT COUNT(*) FROM anggota WHERE status = 'Aktif'),
              (SELECT COUNT(*) FROM kelompok_tani),
              (SELECT COALESCE(SUM(saldo), 0) FROM dana_anggota),
              (SELECT COUNT(*) FROM pinjaman WHERE status = 'Aktif'),
              (SELECT COALESCE(SUM(berat_kg * harga_per_kg), 0) FROM setoran_panen)";

        using var koneksi = Database.BukaKoneksi();
        using var cmd = new NpgsqlCommand(sql, koneksi);
        using var r = cmd.ExecuteReader();
        r.Read();

        return new RingkasanDashboard(
            (int)r.GetInt64(0),
            (int)r.GetInt64(1),
            r.GetDecimal(2),
            (int)r.GetInt64(3),
            r.GetDecimal(4));
    }
}