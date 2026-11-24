using Npgsql;

namespace SIKOTANI.Models.Repositories;

public static class PinjamanRepository
{
    // ================= Membaca =================
    public static List<Pinjaman> Daftar(StatusPinjaman? status = null)
    {
        if (status is null) return Muat(string.Empty);
        return Muat("WHERE p.status = @status",
                    c => c.Parameters.AddWithValue("status", status.Value.ToString()));
    }

    public static Pinjaman? CariById(int id)
        => Muat("WHERE p.id_pinjaman = @id",
                c => c.Parameters.AddWithValue("id", id)).FirstOrDefault();

    private static List<Pinjaman> Muat(string kondisi, Action<NpgsqlCommand>? isiParameter = null)
    {
        string sql = $@"
            SELECT p.id_pinjaman, p.jenis_pinjaman, p.jumlah_pinjaman, p.tenor_bulan,
                   p.suku_bunga_tahunan, p.tanggal_pengajuan, p.tanggal_cair, p.status,
                   {AnggotaRepository.KolomAnggota}
            FROM pinjaman p
            JOIN anggota a ON a.id_anggota = p.id_anggota
            {kondisi}
            ORDER BY p.id_pinjaman DESC";

        var hasil = new List<Pinjaman>();
        var keadaan = new Dictionary<int, (StatusPinjaman Status, DateOnly? TanggalCair)>();

        using var koneksi = Database.BukaKoneksi();

        // 1. Pinjaman dan anggotanya
        using (var cmd = new NpgsqlCommand(sql, koneksi))
        {
            isiParameter?.Invoke(cmd);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                int id = r.GetInt32(0);
                string jenis = r.GetString(1);
                int tenor = r.GetInt32(3);
                decimal suku = r.GetDecimal(4);
                var tglPengajuan = r.GetFieldValue<DateOnly>(5);
                DateOnly? tglCair = r.IsDBNull(6) ? null : r.GetFieldValue<DateOnly>(6);
                var status = Enum.Parse<StatusPinjaman>(r.GetString(7));
                var anggota = AnggotaRepository.DariPembaca(r, 8);

                // Polymorphism: jenis di database menentukan class turunan
                Pinjaman pj = jenis == "Tunai"
                    ? new PinjamanTunai(id, anggota, r.GetDecimal(2), tenor, suku, tglPengajuan)
                    : new PinjamanSaprodi(id, anggota, tenor, suku, tglPengajuan);

                hasil.Add(pj);
                keadaan[id] = (status, tglCair);
            }
        }
        if (hasil.Count == 0) return hasil;

        var ids = hasil.Select(x => x.IdPinjaman).ToArray();
        var peta = hasil.ToDictionary(x => x.IdPinjaman);

        // 2. Item saprodi (ditambahkan selagi status masih Diajukan, sebelum dipulihkan)
        using (var cmdItem = new NpgsqlCommand(
            "SELECT id_pinjaman, id_item, nama_item, jenis, jumlah, satuan, harga_satuan " +
            "FROM item_saprodi WHERE id_pinjaman = ANY(@ids) ORDER BY id_item", koneksi))
        {
            cmdItem.Parameters.AddWithValue("ids", ids);
            using var r = cmdItem.ExecuteReader();
            while (r.Read())
            {
                var saprodi = (PinjamanSaprodi)peta[r.GetInt32(0)];
                saprodi.TambahItem(new ItemSaprodi(
                    r.GetString(2),
                    r.IsDBNull(3) ? string.Empty : r.GetString(3),
                    r.GetDecimal(4),
                    r.IsDBNull(5) ? string.Empty : r.GetString(5),
                    r.GetDecimal(6))
                { IdItem = r.GetInt32(1) });
            }
        }

        // 3. Jadwal angsuran (pembayaran dimuat pada Tahap 8B)
        var jadwal = ids.ToDictionary(x => x, _ => new List<Angsuran>());
        using (var cmdAng = new NpgsqlCommand(
            "SELECT id_pinjaman, id_angsuran, no_angsuran, jatuh_tempo, pokok, bunga " +
            "FROM angsuran WHERE id_pinjaman = ANY(@ids) " +
            "ORDER BY id_pinjaman, no_angsuran", koneksi))
        {
            cmdAng.Parameters.AddWithValue("ids", ids);
            using var r = cmdAng.ExecuteReader();
            while (r.Read())
            {
                jadwal[r.GetInt32(0)].Add(new Angsuran(
                    r.GetInt32(2), r.GetFieldValue<DateOnly>(3),
                    r.GetDecimal(4), r.GetDecimal(5))
                { IdAngsuran = r.GetInt32(1) });
            }
        }

        // 4. Pulihkan status, tanggal cair, dan jadwal
        foreach (var pj in hasil)
        {
            var (status, tglCair) = keadaan[pj.IdPinjaman];
            pj.Pulihkan(status, tglCair, jadwal[pj.IdPinjaman]);
        }
        return hasil;
    }

    // ================= Pengajuan =================
    public static int AjukanTunai(int idAnggota, decimal jumlah, int tenorBulan, DateOnly tanggal)
    {
        PastikanBukanMasaDepan(tanggal, "pengajuan");
        var anggota = AnggotaRepository.CariById(idAnggota)
            ?? throw new AturanBisnisException("Data anggota tidak ditemukan.");
        var parameter = ParameterRepository.Muat();

        // Validasi dilakukan class domain (anggota Aktif, jumlah, tenor)
        var pinjaman = new PinjamanTunai(0, anggota, jumlah, tenorBulan, parameter, tanggal);

        const string sql = @"
            INSERT INTO pinjaman (id_anggota, jenis_pinjaman, jumlah_pinjaman, tenor_bulan,
                                  suku_bunga_tahunan, tanggal_pengajuan, status)
            VALUES (@a, 'Tunai', @jumlah, @tenor, @suku, @tgl, 'Diajukan')
            RETURNING id_pinjaman";

        using var koneksi = Database.BukaKoneksi();
        using var cmd = new NpgsqlCommand(sql, koneksi);
        cmd.Parameters.AddWithValue("a", idAnggota);
        cmd.Parameters.AddWithValue("jumlah", pinjaman.JumlahPinjaman);
        cmd.Parameters.AddWithValue("tenor", pinjaman.TenorBulan);
        cmd.Parameters.AddWithValue("suku", pinjaman.SukuBungaTahunan);
        cmd.Parameters.AddWithValue("tgl", pinjaman.TanggalPengajuan);
        return (int)cmd.ExecuteScalar()!;
    }

    public static int AjukanSaprodi(int idAnggota, IEnumerable<ItemSaprodi> daftarItem,
                                    int tenorBulan, DateOnly tanggal)
    {
        PastikanBukanMasaDepan(tanggal, "pengajuan");
        var anggota = AnggotaRepository.CariById(idAnggota)
            ?? throw new AturanBisnisException("Data anggota tidak ditemukan.");
        var parameter = ParameterRepository.Muat();

        var pinjaman = new PinjamanSaprodi(0, anggota, tenorBulan, parameter, tanggal);
        foreach (var item in daftarItem) pinjaman.TambahItem(item);
        pinjaman.PastikanSiapDiajukan();

        using var koneksi = Database.BukaKoneksi();
        using var transaksi = koneksi.BeginTransaction();   // pinjaman dan itemnya: satu kesatuan

        int id;
        using (var cmd = new NpgsqlCommand(@"
            INSERT INTO pinjaman (id_anggota, jenis_pinjaman, jumlah_pinjaman, tenor_bulan,
                                  suku_bunga_tahunan, tanggal_pengajuan, status)
            VALUES (@a, 'Saprodi', NULL, @tenor, @suku, @tgl, 'Diajukan')
            RETURNING id_pinjaman", koneksi, transaksi))
        {
            cmd.Parameters.AddWithValue("a", idAnggota);
            cmd.Parameters.AddWithValue("tenor", pinjaman.TenorBulan);
            cmd.Parameters.AddWithValue("suku", pinjaman.SukuBungaTahunan);
            cmd.Parameters.AddWithValue("tgl", pinjaman.TanggalPengajuan);
            id = (int)cmd.ExecuteScalar()!;
        }

        foreach (var item in pinjaman.DaftarItem)
        {
            using var cmdItem = new NpgsqlCommand(@"
                INSERT INTO item_saprodi (id_pinjaman, nama_item, jenis, jumlah, satuan, harga_satuan)
                VALUES (@id, @nama, @jenis, @jumlah, @satuan, @harga)", koneksi, transaksi);
            cmdItem.Parameters.AddWithValue("id", id);
            cmdItem.Parameters.AddWithValue("nama", item.NamaItem);
            cmdItem.Parameters.AddWithValue("jenis", item.Jenis);
            cmdItem.Parameters.AddWithValue("jumlah", item.Jumlah);
            cmdItem.Parameters.AddWithValue("satuan", item.Satuan);
            cmdItem.Parameters.AddWithValue("harga", item.HargaSatuan);
            cmdItem.ExecuteNonQuery();
        }

        transaksi.Commit();
        return id;
    }

    // ================= Persetujuan =================
    public static void Setujui(int idPinjaman, DateOnly tanggal)
    {
        PastikanBukanMasaDepan(tanggal, "persetujuan");

        using var koneksi = Database.BukaKoneksi();
        using var transaksi = koneksi.BeginTransaction();
        Kunci(koneksi, transaksi, idPinjaman);

        var pinjaman = CariById(idPinjaman)
            ?? throw new AturanBisnisException("Data pinjaman tidak ditemukan.");

        pinjaman.Setujui(tanggal);   // aturan domain; membuat jadwal angsuran

        using (var cmd = new NpgsqlCommand(
            "UPDATE pinjaman SET status = 'Aktif', tanggal_cair = @tgl " +
            "WHERE id_pinjaman = @id", koneksi, transaksi))
        {
            cmd.Parameters.AddWithValue("tgl", tanggal);
            cmd.Parameters.AddWithValue("id", idPinjaman);
            cmd.ExecuteNonQuery();
        }

        foreach (var a in pinjaman.JadwalAngsuran)
        {
            using var cmdAng = new NpgsqlCommand(@"
                INSERT INTO angsuran (id_pinjaman, no_angsuran, jatuh_tempo, pokok, bunga)
                VALUES (@id, @no, @tempo, @pokok, @bunga)", koneksi, transaksi);
            cmdAng.Parameters.AddWithValue("id", idPinjaman);
            cmdAng.Parameters.AddWithValue("no", a.NoAngsuran);
            cmdAng.Parameters.AddWithValue("tempo", a.JatuhTempo);
            cmdAng.Parameters.AddWithValue("pokok", a.Pokok);
            cmdAng.Parameters.AddWithValue("bunga", a.Bunga);
            cmdAng.ExecuteNonQuery();
        }

        transaksi.Commit();   // status dan jadwal tersimpan bersama
    }

    public static void Tolak(int idPinjaman)
    {
        using var koneksi = Database.BukaKoneksi();
        using var transaksi = koneksi.BeginTransaction();
        Kunci(koneksi, transaksi, idPinjaman);

        var pinjaman = CariById(idPinjaman)
            ?? throw new AturanBisnisException("Data pinjaman tidak ditemukan.");
        pinjaman.Tolak();

        using (var cmd = new NpgsqlCommand(
            "UPDATE pinjaman SET status = 'Ditolak' WHERE id_pinjaman = @id", koneksi, transaksi))
        {
            cmd.Parameters.AddWithValue("id", idPinjaman);
            cmd.ExecuteNonQuery();
        }
        transaksi.Commit();
    }

    // ================= Pembantu =================
    // FOR UPDATE: bila dua pengurus memproses pinjaman yang sama, yang kedua menunggu
    // lalu melihat status terbaru, sehingga tidak mungkin disetujui dua kali
    private static void Kunci(NpgsqlConnection koneksi, NpgsqlTransaction transaksi, int id)
    {
        using var cmd = new NpgsqlCommand(
            "SELECT 1 FROM pinjaman WHERE id_pinjaman = @id FOR UPDATE", koneksi, transaksi);
        cmd.Parameters.AddWithValue("id", id);
        if (cmd.ExecuteScalar() is null)
            throw new AturanBisnisException("Data pinjaman tidak ditemukan.");
    }

    private static void PastikanBukanMasaDepan(DateOnly tanggal, string nama)
    {
        if (tanggal > DateOnly.FromDateTime(DateTime.Today))
            throw new AturanBisnisException(
                $"Tanggal {nama} tidak boleh lebih dari hari ini.");
    }
}