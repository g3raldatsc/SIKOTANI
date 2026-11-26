using Npgsql;

namespace SIKOTANI.Models.Repositories;

public static class SetoranRepository
{
    // ================= Membaca =================
    public static List<SetoranPanen> Daftar() => Muat(string.Empty);

    public static SetoranPanen? CariById(int id)
        => Muat("WHERE s.id_setoran = @id",
                c => c.Parameters.AddWithValue("id", id)).FirstOrDefault();

    private static List<SetoranPanen> Muat(string kondisi, Action<NpgsqlCommand>? isiParameter = null)
    {
        // Nilai terpakai dihitung dari pembayaran angsuran yang menunjuk setoran ini
        string sql = $@"
            SELECT s.id_setoran, s.tanggal, s.berat_kg, s.harga_per_kg,
                   k.id_komoditas, k.nama, k.satuan,
                   COALESCE((SELECT SUM(pb.jumlah) FROM pembayaran_angsuran pb
                             WHERE pb.id_setoran = s.id_setoran), 0),
                   {AnggotaRepository.KolomAnggota}
            FROM setoran_panen s
            JOIN komoditas k ON k.id_komoditas = s.id_komoditas
            JOIN anggota a ON a.id_anggota = s.id_anggota
            {kondisi}
            ORDER BY s.id_setoran DESC";

        var hasil = new List<SetoranPanen>();
        using var koneksi = Database.BukaKoneksi();
        using var cmd = new NpgsqlCommand(sql, koneksi);
        isiParameter?.Invoke(cmd);
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            var komoditas = new Komoditas(r.GetInt32(4), r.GetString(5), r.GetString(6));
            var anggota = AnggotaRepository.DariPembaca(r, 8);
            hasil.Add(new SetoranPanen(
                r.GetInt32(0), anggota, komoditas,
                r.GetFieldValue<DateOnly>(1), r.GetDecimal(2), r.GetDecimal(3),
                r.GetDecimal(7)));
        }
        return hasil;
    }

    public static List<RiwayatPotongan> Riwayat(int idSetoran)
    {
        const string sql = @"
            SELECT pb.tanggal, pb.jumlah, an.id_pinjaman, an.no_angsuran
            FROM pembayaran_angsuran pb
            JOIN angsuran an ON an.id_angsuran = pb.id_angsuran
            WHERE pb.id_setoran = @id
            ORDER BY pb.id_pembayaran DESC";

        var hasil = new List<RiwayatPotongan>();
        using var koneksi = Database.BukaKoneksi();
        using var cmd = new NpgsqlCommand(sql, koneksi);
        cmd.Parameters.AddWithValue("id", idSetoran);
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            hasil.Add(new RiwayatPotongan(
                r.GetFieldValue<DateOnly>(0), r.GetDecimal(1), r.GetInt32(2), r.GetInt32(3)));
        }
        return hasil;
    }

    // ================= Setoran baru (Unit Hasil Panen) =================
    public static int Catat(int idAnggota, int idKomoditas, DateOnly tanggal,
                            decimal beratKg, decimal hargaPerKg)
    {
        if (tanggal > DateOnly.FromDateTime(DateTime.Today))
            throw new AturanBisnisException("Tanggal setoran tidak boleh lebih dari hari ini.");

        var anggota = AnggotaRepository.CariById(idAnggota)
            ?? throw new AturanBisnisException("Data anggota tidak ditemukan.");
        var komoditas = KomoditasRepository.DaftarSemua()
            .FirstOrDefault(k => k.IdKomoditas == idKomoditas)
            ?? throw new AturanBisnisException("Komoditas tidak ditemukan.");

        // Validasi dilakukan class domain (anggota Aktif, berat, harga)
        var valid = new SetoranPanen(0, anggota, komoditas, tanggal, beratKg, hargaPerKg);

        const string sql = @"
            INSERT INTO setoran_panen (id_anggota, id_komoditas, tanggal, berat_kg, harga_per_kg)
            VALUES (@a, @k, @tgl, @berat, @harga)
            RETURNING id_setoran";

        using var koneksi = Database.BukaKoneksi();
        using var cmd = new NpgsqlCommand(sql, koneksi);
        cmd.Parameters.AddWithValue("a", idAnggota);
        cmd.Parameters.AddWithValue("k", idKomoditas);
        cmd.Parameters.AddWithValue("tgl", valid.Tanggal);
        cmd.Parameters.AddWithValue("berat", valid.BeratKg);
        cmd.Parameters.AddWithValue("harga", valid.HargaPerKg);
        return (int)cmd.ExecuteScalar()!;
    }

    // ================= Potong panen =================
    /// <summary>
    /// Dua catatan terpisah dalam SATU transaksi: setoran tetap di Unit Hasil Panen,
    /// sedangkan pembayarannya tercatat di angsuran (USP) dengan sumber PotongPanen.
    /// </summary>
    public static void PotongUntukAngsuran(int idSetoran, int idPinjaman, int noAngsuran,
                                           decimal jumlah, DateOnly tanggal,
                                           bool simulasiGagal = false)
    {
        if (tanggal > DateOnly.FromDateTime(DateTime.Today))
            throw new AturanBisnisException("Tanggal pemotongan tidak boleh lebih dari hari ini.");

        using var koneksi = Database.BukaKoneksi();
        using var transaksi = koneksi.BeginTransaction();

        // Urutan kunci selalu sama (setoran dahulu, lalu pinjaman) agar tidak saling menunggu
        KunciSetoran(koneksi, transaksi, idSetoran);
        KunciPinjaman(koneksi, transaksi, idPinjaman);

        var setoran = CariById(idSetoran)
            ?? throw new AturanBisnisException("Data setoran tidak ditemukan.");
        var pinjaman = PinjamanRepository.CariById(idPinjaman)
            ?? throw new AturanBisnisException("Data pinjaman tidak ditemukan.");

        // Aturan domain: nilai tersedia, anggota sama, tanggal, pinjaman Aktif, sisa tagihan
        setoran.PotongUntukAngsuran(pinjaman, noAngsuran, jumlah, tanggal);

        var angsuran = pinjaman.JadwalAngsuran.First(a => a.NoAngsuran == noAngsuran);
        var catatan = angsuran.Pembayaran.Last();

        // Catatan 1: pembayaran angsuran di USP, bersumber dari setoran ini
        using (var cmd = new NpgsqlCommand(@"
            INSERT INTO pembayaran_angsuran (id_angsuran, tanggal, jumlah, sumber, id_setoran)
            VALUES (@ang, @tgl, @jumlah, 'PotongPanen', @setoran)", koneksi, transaksi))
        {
            cmd.Parameters.AddWithValue("ang", angsuran.IdAngsuran);
            cmd.Parameters.AddWithValue("tgl", catatan.Tanggal);
            cmd.Parameters.AddWithValue("jumlah", catatan.Jumlah);
            cmd.Parameters.AddWithValue("setoran", idSetoran);
            cmd.ExecuteNonQuery();
        }

        // Khusus uji T10: gagal setelah pembayaran tersimpan, SEBELUM status angsuran diperbarui.
        // Karena transaksi tidak di-Commit, pembayaran tadi ikut dibatalkan (rollback).
        if (simulasiGagal)
            throw new InvalidOperationException(
                "Simulasi kegagalan di tengah transaksi (uji T10).");

        using (var cmd = new NpgsqlCommand(
            "UPDATE angsuran SET status = @status WHERE id_angsuran = @ang", koneksi, transaksi))
        {
            cmd.Parameters.AddWithValue("status", angsuran.Status.ToString());
            cmd.Parameters.AddWithValue("ang", angsuran.IdAngsuran);
            cmd.ExecuteNonQuery();
        }

        if (pinjaman.Status == StatusPinjaman.Lunas)
        {
            using var cmd = new NpgsqlCommand(
                "UPDATE pinjaman SET status = 'Lunas' WHERE id_pinjaman = @id", koneksi, transaksi);
            cmd.Parameters.AddWithValue("id", idPinjaman);
            cmd.ExecuteNonQuery();
        }

        transaksi.Commit();   // pembayaran, status angsuran, dan status pinjaman: satu kesatuan
    }

    // ================= Pembantu =================
    private static void KunciSetoran(NpgsqlConnection koneksi, NpgsqlTransaction transaksi, int id)
    {
        using var cmd = new NpgsqlCommand(
            "SELECT 1 FROM setoran_panen WHERE id_setoran = @id FOR UPDATE", koneksi, transaksi);
        cmd.Parameters.AddWithValue("id", id);
        if (cmd.ExecuteScalar() is null)
            throw new AturanBisnisException("Data setoran tidak ditemukan.");
    }

    private static void KunciPinjaman(NpgsqlConnection koneksi, NpgsqlTransaction transaksi, int id)
    {
        using var cmd = new NpgsqlCommand(
            "SELECT 1 FROM pinjaman WHERE id_pinjaman = @id FOR UPDATE", koneksi, transaksi);
        cmd.Parameters.AddWithValue("id", id);
        if (cmd.ExecuteScalar() is null)
            throw new AturanBisnisException("Data pinjaman tidak ditemukan.");
    }
}