using Npgsql;
using SIKOTANI.Models;

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
        return new ParameterKoperasi(r.GetDecimal(0), r.GetDecimal(1), r.GetDecimal(2), r.GetDecimal(3), r.GetDecimal(4));
    }
}