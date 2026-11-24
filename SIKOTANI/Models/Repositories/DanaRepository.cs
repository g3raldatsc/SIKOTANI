using Npgsql;
using SIKOTANI.Models;

namespace SIKOTANI.Models.Repositories;

public static class DanaRepository
{
    // Polymorphism: jenis di database menentukan class turunan yang dibuat
    private static DanaAnggota Bentuk(int id, string jenis, decimal saldo, Anggota anggota, ParameterKoperasi parameter)
    {
        return jenis switch
        {
            "Pokok" => new SimpananPokok(id, anggota, parameter.NilaiSimpananPokok, saldo),
            "Wajib" => new SimpananWajib(id, anggota, saldo),
            "Tabungan" => new Tabungan(id, anggota, saldo),
            _ => throw new InvalidOperationException("Jenis dana di database tidak dikenal.")
        };
    }

    public static List<DanaAnggota> DaftarDana(Anggota anggota)
    {
        var parameter = ParameterRepository.Muat();
        var hasil = new List<DanaAnggota>();

        using var koneksi = Database.BukaKoneksi();
        using var cmd = new NpgsqlCommand(
            "SELECT id_dana, jenis_dana, saldo FROM dana_anggota " +
            "WHERE id_anggota = @id ORDER BY id_dana", koneksi);
        cmd.Parameters.AddWithValue("id", anggota.IdAnggota);

        using var r = cmd.ExecuteReader();
        while (r.Read())
            hasil.Add(Bentuk(r.GetInt32(0), r.GetString(1), r.GetDecimal(2), anggota, parameter));
        return hasil;
    }

    public static List<TransaksiDana> Riwayat(int idDana)
    {
        var hasil = new List<TransaksiDana>();

        using var koneksi = Database.BukaKoneksi();
        using var cmd = new NpgsqlCommand(
            "SELECT id_transaksi, tanggal, jenis, jumlah, saldo_setelah " +
            "FROM transaksi_dana WHERE id_dana = @id ORDER BY id_transaksi DESC", koneksi);
        cmd.Parameters.AddWithValue("id", idDana);

        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            hasil.Add(new TransaksiDana(
                r.GetFieldValue<DateOnly>(1),
                Enum.Parse<JenisTransaksi>(r.GetString(2)),
                r.GetDecimal(3),
                r.GetDecimal(4))
            { IdTransaksi = r.GetInt32(0) });
        }
        return hasil;
    }

    public static int BukaDana(int idAnggota, string jenis)
    {
        var anggota = AnggotaRepository.CariById(idAnggota)
            ?? throw new AturanBisnisException("Data anggota tidak ditemukan.");
        if (!anggota.Aktif)
            throw new AturanBisnisException(
                "Anggota sudah keluar, tidak dapat membuka rekening dana baru.");
        if (jenis is not ("Pokok" or "Wajib" or "Tabungan"))
            throw new AturanBisnisException("Jenis dana tidak valid.");

        try
        {
            using var koneksi = Database.BukaKoneksi();
            using var cmd = new NpgsqlCommand(
                "INSERT INTO dana_anggota (id_anggota, jenis_dana) VALUES (@a, @j) " +
                "RETURNING id_dana", koneksi);
            cmd.Parameters.AddWithValue("a", idAnggota);
            cmd.Parameters.AddWithValue("j", jenis);
            return (int)cmd.ExecuteScalar()!;
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new AturanBisnisException(
                "Anggota ini sudah memiliki rekening dana jenis tersebut.");
        }
    }

    /// <summary>
    /// Setor atau tarik. Aturan bisnis dijalankan oleh class domain (DanaAnggota),
    /// lalu saldo dan riwayat disimpan dalam SATU transaksi database.
    /// </summary>
    public static void Catat(int idDana, JenisTransaksi jenis, decimal jumlah, DateOnly tanggal)
    {
        if (tanggal > DateOnly.FromDateTime(DateTime.Today))
            throw new AturanBisnisException("Tanggal transaksi tidak boleh lebih dari hari ini.");

        var parameter = ParameterRepository.Muat();

        using var koneksi = Database.BukaKoneksi();
        using var transaksi = koneksi.BeginTransaction();

        // FOR UPDATE: baris dana dikunci sampai transaksi selesai
        const string sqlDana = @"
            SELECT d.jenis_dana, d.saldo,
                   a.id_anggota, a.nama, a.alamat, a.no_telepon,
                   a.tanggal_gabung, a.luas_lahan_ha, a.status
            FROM dana_anggota d
            JOIN anggota a ON a.id_anggota = d.id_anggota
            WHERE d.id_dana = @id
            FOR UPDATE OF d";

        DanaAnggota dana;
        using (var cmd = new NpgsqlCommand(sqlDana, koneksi, transaksi))
        {
            cmd.Parameters.AddWithValue("id", idDana);
            using var r = cmd.ExecuteReader();
            if (!r.Read())
                throw new AturanBisnisException("Rekening dana tidak ditemukan.");

            var anggota = AnggotaRepository.DariPembaca(r, 2);
            dana = Bentuk(idDana, r.GetString(0), r.GetDecimal(1), anggota, parameter);
        }

        // Class domain menolak bila aturan dilanggar (exception membatalkan transaksi)
        if (jenis == JenisTransaksi.Setor) dana.Setor(jumlah, tanggal);
        else dana.Tarik(jumlah, tanggal);

        var catatan = dana.Transaksi.Last();

        using (var cmdSaldo = new NpgsqlCommand(
            "UPDATE dana_anggota SET saldo = @saldo WHERE id_dana = @id", koneksi, transaksi))
        {
            cmdSaldo.Parameters.AddWithValue("saldo", dana.Saldo);
            cmdSaldo.Parameters.AddWithValue("id", idDana);
            cmdSaldo.ExecuteNonQuery();
        }

        using (var cmdCatat = new NpgsqlCommand(
            "INSERT INTO transaksi_dana (id_dana, tanggal, jenis, jumlah, saldo_setelah) " +
            "VALUES (@id, @tgl, @jenis, @jumlah, @sisa)", koneksi, transaksi))
        {
            cmdCatat.Parameters.AddWithValue("id", idDana);
            cmdCatat.Parameters.AddWithValue("tgl", catatan.Tanggal);
            cmdCatat.Parameters.AddWithValue("jenis", catatan.Jenis.ToString());
            cmdCatat.Parameters.AddWithValue("jumlah", catatan.Jumlah);
            cmdCatat.Parameters.AddWithValue("sisa", catatan.SaldoSetelah);
            cmdCatat.ExecuteNonQuery();
        }

        transaksi.Commit();
    }
}