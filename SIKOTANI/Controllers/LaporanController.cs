using SIKOTANI.Models;
using SIKOTANI.Models.Repositories;

namespace SIKOTANI.Controllers;

public class LaporanController : ControllerDasar
{
    private const int HariKeDepan = 30;

    // Laporan Anggota
    public TabelLaporan LaporanAnggota()
    {
        PastikanBolehLihat(MenuAplikasi.LaporanAnggota);
        var data = LaporanRepository.AmbilAnggota();

        var tabel = new TabelLaporan("Laporan Anggota",
            ("ID", false), ("Nama", false), ("Telepon", false), ("Alamat", false),
            ("Tgl Gabung", false), ("Luas Lahan (ha)", false), ("Status", false),
            ("Kelompok Tani", false), ("Komoditas", false));

        foreach (var a in data)
            tabel.TambahBaris(a.IdAnggota, a.Nama, a.NoTelepon, a.Alamat, a.TanggalGabung,
                              a.LuasLahanHa, a.Status, a.Kelompok, a.Komoditas);

        int aktif = data.Count(a => a.Status == "Aktif");
        decimal luas = data.Where(a => a.Status == "Aktif").Sum(a => a.LuasLahanHa);
        tabel.AturRingkasan($"Anggota aktif {aktif}  |  Keluar {data.Count - aktif}  |  " +
                            $"Total luas lahan anggota aktif {FormatTeks.Angka(luas)} ha");
        return tabel;
    }

    // Laporan Keuangan
    public TabelLaporan LaporanDana()
    {
        PastikanBolehLihat(MenuAplikasi.LaporanKeuangan);
        var data = LaporanRepository.AmbilDana();

        var tabel = new TabelLaporan("Laporan Dana Anggota",
            ("Anggota", false), ("Status", false), ("Simpanan Pokok", true),
            ("Simpanan Wajib", true), ("Tabungan", true), ("Total", true));

        foreach (var d in data)
            tabel.TambahBaris(d.Nama, d.Status, d.Pokok, d.Wajib, d.Tabungan, d.Total);

        tabel.AturRingkasan(
            $"Pokok {FormatTeks.Rp(data.Sum(d => d.Pokok))}  |  Wajib {FormatTeks.Rp(data.Sum(d => d.Wajib))}  |  " +
            $"Tabungan {FormatTeks.Rp(data.Sum(d => d.Tabungan))}  |  Total {FormatTeks.Rp(data.Sum(d => d.Total))}");
        return tabel;
    }

    public TabelLaporan LaporanPinjaman()
    {
        PastikanBolehLihat(MenuAplikasi.LaporanKeuangan);
        var hariIni = DateOnly.FromDateTime(DateTime.Today);
        var daftar = PinjamanRepository.Daftar(StatusPinjaman.Aktif);

        var tabel = new TabelLaporan("Laporan Pinjaman Aktif",
            ("ID", false), ("Anggota", false), ("Jenis", false), ("Pokok", true),
            ("Tenor", false), ("Bunga", false), ("Sisa Pokok", true),
            ("Jatuh Tempo Berikutnya", false), ("Tagihan Berikutnya", true), ("Keterangan", false));

        int terlambat = 0;
        foreach (var pj in daftar)
        {
            var berikut = pj.JadwalAngsuran.FirstOrDefault(x => x.Status != StatusAngsuran.Lunas);
            bool lewat = berikut is not null && berikut.Terlambat(hariIni);
            if (lewat) terlambat++;

            tabel.TambahBaris(pj.IdPinjaman, pj.Anggota.Nama, pj.NamaJenis, pj.HitungPokokPinjaman(),
                pj.TenorBulan + " bulan",
                pj.SukuBungaTahunan.ToString("0.##", FormatTeks.Budaya) + "%",
                pj.GetSisaPokok(), berikut?.JatuhTempo, berikut?.GetSisaTagihan(),
                lewat ? "Terlambat" : string.Empty);
        }

        tabel.AturRingkasan(
            $"{daftar.Count} pinjaman aktif  |  Pokok disalurkan {FormatTeks.Rp(daftar.Sum(x => x.HitungPokokPinjaman()))}  |  " +
            $"Sisa pokok {FormatTeks.Rp(daftar.Sum(x => x.GetSisaPokok()))}  |  Terlambat {terlambat}");
        return tabel;
    }

    public TabelLaporan LaporanJatuhTempo()
    {
        PastikanBolehLihat(MenuAplikasi.LaporanKeuangan);
        var hariIni = DateOnly.FromDateTime(DateTime.Today);
        var batas = hariIni.AddDays(HariKeDepan);
        var daftar = PinjamanRepository.Daftar(StatusPinjaman.Aktif);

        var baris = new List<(Pinjaman Induk, Angsuran Tagihan)>();
        foreach (var pj in daftar)
        {
            foreach (var tagihan in pj.JadwalAngsuran)
            {
                if (tagihan.Status != StatusAngsuran.Lunas && tagihan.JatuhTempo <= batas)
                    baris.Add((pj, tagihan));
            }
        }

        var tabel = new TabelLaporan($"Angsuran Jatuh Tempo ({HariKeDepan} hari ke depan dan yang terlambat)",
            ("Jatuh Tempo", false), ("Anggota", false), ("Pinjaman", false),
            ("Angsuran", false), ("Sisa Tagihan", true), ("Keterangan", false));

        int terlambat = 0;
        foreach (var (induk, tagihan) in baris.OrderBy(x => x.Tagihan.JatuhTempo))
        {
            int selisih = tagihan.JatuhTempo.DayNumber - hariIni.DayNumber;
            if (selisih < 0) terlambat++;
            string keterangan = selisih < 0 ? $"Terlambat {-selisih} hari"
                              : selisih == 0 ? "Jatuh tempo hari ini"
                              : $"{selisih} hari lagi";

            tabel.TambahBaris(tagihan.JatuhTempo, induk.Anggota.Nama, "#" + induk.IdPinjaman,
                              "ke-" + tagihan.NoAngsuran, tagihan.GetSisaTagihan(), keterangan);
        }

        tabel.AturRingkasan(
            $"{baris.Count} angsuran  |  Total sisa tagihan {FormatTeks.Rp(baris.Sum(x => x.Tagihan.GetSisaTagihan()))}  |  " +
            $"Terlambat {terlambat}");
        return tabel;
    }

    public TabelLaporan LaporanPanen()
    {
        PastikanBolehLihat(MenuAplikasi.LaporanKeuangan);
        var setoran = SetoranRepository.Daftar();

        var tabel = new TabelLaporan("Laporan Hasil Panen per Komoditas",
            ("Komoditas", false), ("Jumlah Setoran", false), ("Total Berat (kg)", false),
            ("Total Nilai", true), ("Dipotong untuk Angsuran", true), ("Tersedia", true));

        foreach (var grup in setoran.GroupBy(s => s.Komoditas.Nama).OrderBy(g => g.Key))
        {
            tabel.TambahBaris(grup.Key, grup.Count(), grup.Sum(s => s.BeratKg),
                              grup.Sum(s => s.HitungNilai()), grup.Sum(s => s.Terpakai),
                              grup.Sum(s => s.HitungNilaiTersedia()));
        }

        tabel.AturRingkasan(
            $"{setoran.Count} setoran  |  Nilai {FormatTeks.Rp(setoran.Sum(s => s.HitungNilai()))}  |  " +
            $"Dipotong {FormatTeks.Rp(setoran.Sum(s => s.Terpakai))}  |  " +
            $"Tersedia {FormatTeks.Rp(setoran.Sum(s => s.HitungNilaiTersedia()))}");
        return tabel;
    }
}