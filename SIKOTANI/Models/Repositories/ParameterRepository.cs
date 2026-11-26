using Npgsql;

namespace SIKOTANI.Models.Repositories;

public static class ParameterRepository
{
    public static ParameterKoperasi Muat()
    {
        const string sql = @"
            SELECT nilai_simpanan_pokok, bunga_tabungan_maks, bunga_pinjaman_maks,
                   bunga_tabungan_berlaku, bunga_pinjaman_berlaku
            FROM parameter_koperasi
            WHERE id_koperasi = 1";

        using var koneksi = Database.BukaKoneksi();
        using var cmd = new NpgsqlCommand(sql, koneksi);
        using var r = cmd.ExecuteReader();
        if (!r.Read())
            throw new AturanBisnisException("Parameter koperasi belum diisi.");

        // Constructor ParameterKoperasi memvalidasi ulang semua nilai
        return new ParameterKoperasi(r.GetDecimal(0), r.GetDecimal(1), r.GetDecimal(2),
                                     r.GetDecimal(3), r.GetDecimal(4));
    }

    public static void Simpan(ParameterKoperasi p)
    {
        const string sql = @"
            UPDATE parameter_koperasi
            SET nilai_simpanan_pokok = @pokok,
                bunga_tabungan_maks = @tabMaks, bunga_pinjaman_maks = @pinMaks,
                bunga_tabungan_berlaku = @tabBerlaku, bunga_pinjaman_berlaku = @pinBerlaku
            WHERE id_koperasi = 1";

        using var koneksi = Database.BukaKoneksi();
        using var cmd = new NpgsqlCommand(sql, koneksi);
        cmd.Parameters.AddWithValue("pokok", p.NilaiSimpananPokok);
        cmd.Parameters.AddWithValue("tabMaks", p.BungaTabunganMaks);
        cmd.Parameters.AddWithValue("pinMaks", p.BungaPinjamanMaks);
        cmd.Parameters.AddWithValue("tabBerlaku", p.BungaTabunganBerlaku);
        cmd.Parameters.AddWithValue("pinBerlaku", p.BungaPinjamanBerlaku);
        if (cmd.ExecuteNonQuery() == 0)
            throw new AturanBisnisException("Parameter koperasi belum diisi.");
    }
}