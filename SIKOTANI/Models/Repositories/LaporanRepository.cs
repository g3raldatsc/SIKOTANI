using Npgsql;

namespace SIKOTANI.Models.Repositories;

public static class LaporanRepository
{
    public static List<RingkasanAnggota> AmbilAnggota()
    {
        const string sql = @"
            SELECT a.id_anggota, a.nama, COALESCE(a.no_telepon, ''), COALESCE(a.alamat, ''),
                   a.tanggal_gabung, a.luas_lahan_ha, a.status,
                   COALESCE((SELECT string_agg(k.nama, ', ' ORDER BY k.nama)
                             FROM keanggotaan_kelompok kk
                             JOIN kelompok_tani k ON k.id_kelompok = kk.id_kelompok
                             WHERE kk.id_anggota = a.id_anggota), '-'),
                   COALESCE((SELECT string_agg(c.nama, ', ' ORDER BY c.nama)
                             FROM anggota_komoditas ak
                             JOIN komoditas c ON c.id_komoditas = ak.id_komoditas
                             WHERE ak.id_anggota = a.id_anggota), '-')
            FROM anggota a
            ORDER BY a.status, a.nama";

        var hasil = new List<RingkasanAnggota>();
        using var koneksi = Database.BukaKoneksi();
        using var cmd = new NpgsqlCommand(sql, koneksi);
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            hasil.Add(new RingkasanAnggota(
                r.GetInt32(0), r.GetString(1), r.GetString(2), r.GetString(3),
                r.GetFieldValue<DateOnly>(4), r.GetDecimal(5), r.GetString(6),
                r.GetString(7), r.GetString(8)));
        }
        return hasil;
    }

    public static List<RingkasanDana> AmbilDana()
    {
        const string sql = @"
            SELECT a.nama, a.status,
                   COALESCE(SUM(d.saldo) FILTER (WHERE d.jenis_dana = 'Pokok'), 0),
                   COALESCE(SUM(d.saldo) FILTER (WHERE d.jenis_dana = 'Wajib'), 0),
                   COALESCE(SUM(d.saldo) FILTER (WHERE d.jenis_dana = 'Tabungan'), 0)
            FROM anggota a
            JOIN dana_anggota d ON d.id_anggota = a.id_anggota
            GROUP BY a.id_anggota, a.nama, a.status
            ORDER BY a.nama";

        var hasil = new List<RingkasanDana>();
        using var koneksi = Database.BukaKoneksi();
        using var cmd = new NpgsqlCommand(sql, koneksi);
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            hasil.Add(new RingkasanDana(
                r.GetString(0), r.GetString(1),
                r.GetDecimal(2), r.GetDecimal(3), r.GetDecimal(4)));
        }
        return hasil;
    }
}