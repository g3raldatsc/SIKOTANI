namespace SIKOTANI.Views;

public class HalamanPembayaran : UserControl
{
    private sealed record PilihanAngsuran(int No, decimal Sisa, string Teks)
    {
        public override string ToString() => Teks;
    }

    private readonly PinjamanController _kontrol = new();

    private readonly DataGridView gridPinjaman = UiBantu.BuatGrid();
    private readonly DataGridView gridJadwal = UiBantu.BuatGrid();
    private readonly DataGridView gridRiwayat = UiBantu.BuatGrid();
    private readonly CheckBox chkLunas = new()
    {
        Text = "Tampilkan yang sudah lunas",
        AutoSize = true,
        Margin = new Padding(14, 7, 3, 3)
    };
    private readonly Label lblRingkas = new()
    {
        Dock = DockStyle.Top,
        Height = 34,
        Font = Tema.HurufTebal,
        ForeColor = Tema.Teks,
        TextAlign = ContentAlignment.MiddleLeft
    };

    private readonly FlowLayoutPanel kanan = new()
    {
        Dock = DockStyle.Fill,
        BackColor = Tema.Kartu,
        FlowDirection = FlowDirection.TopDown,
        WrapContents = false,
        AutoScroll = true,
        Padding = new Padding(14)
    };
    private readonly Label lblInfo = new()
    {
        AutoSize = true,
        MaximumSize = new Size(360, 0),
        Font = Tema.Huruf,
        ForeColor = Tema.Teks,
        Margin = new Padding(3, 8, 3, 8)
    };
    private readonly ComboBox cmbAngsuran = new() { Width = 360, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Label lblSisa = new()
    {
        AutoSize = true,
        MaximumSize = new Size(360, 0),
        Font = Tema.HurufTebal,
        Margin = new Padding(3, 8, 3, 0)
    };
    private readonly NumericUpDown numJumlah = new()
    {
        Width = 360,
        Maximum = 9_999_999_999m,
        ThousandsSeparator = true,
        Increment = 10000m
    };
    private readonly DateTimePicker dtpTanggal = new() { Width = 360, Format = DateTimePickerFormat.Short };
    private readonly Button btnIsi = new() { Text = "Isi Sesuai Sisa Tagihan", Width = 360, Height = 32, Margin = new Padding(3, 10, 3, 3) };
    private readonly Button btnBayar = new() { Text = "Catat Pembayaran", Width = 360, Height = 42, Margin = new Padding(3, 10, 3, 3) };
    private readonly Label lblPesan = new() { MaximumSize = new Size(360, 0) };

    private readonly bool _bolehKelola;
    private List<Pinjaman> _semua = new();
    private List<Pinjaman> _tampil = new();
    private Pinjaman? _terpilih;
    private bool _mengisi;

    public HalamanPembayaran()
    {
        Dock = DockStyle.Fill;

        var pengguna = Sesi.PenggunaAktif
            ?? throw new InvalidOperationException("Belum ada sesi masuk.");
        _bolehKelola = pengguna.BolehKelola(MenuAplikasi.PembayaranAngsuran);

        dtpTanggal.MaxDate = DateTime.Today;   // tanggal pembayaran tidak boleh di masa depan
        dtpTanggal.Value = DateTime.Today;

        void Tambah(string teks, Control kontrol)
        {
            kanan.Controls.Add(new Label { Text = teks, AutoSize = true, Margin = new Padding(3, 10, 3, 2) });
            kanan.Controls.Add(kontrol);
        }

        // ---------- panel kanan: formulir pembayaran ----------
        kanan.Controls.Add(new Label { Text = "Catat pembayaran angsuran", Font = Tema.HurufSubjudul, AutoSize = true });
        kanan.Controls.Add(lblInfo);
        Tambah("Angsuran yang dibayar", cmbAngsuran);
        kanan.Controls.Add(lblSisa);
        Tambah("Jumlah pembayaran (Rp)", numJumlah);
        kanan.Controls.Add(btnIsi);
        Tambah("Tanggal pembayaran", dtpTanggal);
        kanan.Controls.Add(btnBayar);
        kanan.Enabled = _bolehKelola;   // Ketua: hanya melihat

        // Pesan di panel tetap di dasar agar selalu terlihat
        lblPesan.AutoSize = false;
        lblPesan.Dock = DockStyle.Fill;
        lblPesan.Margin = Padding.Empty;
        var panelPesan = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 70,
            BackColor = Tema.Kartu,
            Padding = new Padding(14, 8, 14, 8)
        };
        panelPesan.Controls.Add(lblPesan);
        var kananLuar = new Panel { Dock = DockStyle.Right, Width = 400, BackColor = Tema.Kartu };
        kananLuar.Controls.Add(kanan);
        kananLuar.Controls.Add(panelPesan);

        // ---------- panel kiri: pinjaman, jadwal, riwayat ----------
        var barisJudul = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 36 };
        barisJudul.Controls.Add(new Label
        {
            Text = "Pinjaman berjalan",
            Font = Tema.HurufSubjudul,
            AutoSize = true,
            Margin = new Padding(0, 7, 0, 0)
        });
        barisJudul.Controls.Add(chkLunas);
        var atas = new Panel { Dock = DockStyle.Top, Height = 220 };
        atas.Controls.Add(gridPinjaman);
        atas.Controls.Add(barisJudul);

        var tengah = new Panel { Dock = DockStyle.Top, Height = 230 };
        tengah.Controls.Add(gridJadwal);
        tengah.Controls.Add(lblRingkas);
        tengah.Controls.Add(new Label
        {
            Dock = DockStyle.Top,
            Height = 34,
            Text = "Jadwal angsuran",
            Font = Tema.HurufSubjudul,
            TextAlign = ContentAlignment.BottomLeft
        });

        var bawah = new Panel { Dock = DockStyle.Fill };
        bawah.Controls.Add(gridRiwayat);
        bawah.Controls.Add(new Label
        {
            Dock = DockStyle.Top,
            Height = 34,
            Text = "Riwayat pembayaran",
            Font = Tema.HurufSubjudul,
            TextAlign = ContentAlignment.BottomLeft
        });

        var kiri = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 0, 12, 0) };
        kiri.Controls.Add(bawah);    // Fill lebih dulu
        kiri.Controls.Add(tengah);
        kiri.Controls.Add(atas);

        Controls.Add(kiri);
        Controls.Add(kananLuar);

        // ---------- peristiwa ----------
        chkLunas.CheckedChanged += (_, _) => TerapkanFilter(_terpilih?.IdPinjaman);
        gridPinjaman.SelectionChanged += (_, _) =>
        {
            if (_mengisi) return;
            lblPesan.Text = string.Empty;
            PilihDariBaris();
        };
        cmbAngsuran.SelectedIndexChanged += (_, _) => PerbaruiSisa();
        btnIsi.Click += (_, _) => IsiSesuaiSisa();
        btnBayar.Click += (_, _) => Bayar();
        Load += (_, _) => Muat(null);
    }

    private void Muat(int? pilihId)
    {
        try
        {
            _semua = _kontrol.Daftar();
        }
        catch (Exception ex)
        {
            UiBantu.Pesan(lblPesan, "Gagal memuat data: " + ex.Message, true);
            return;
        }
        TerapkanFilter(pilihId);
    }

    private void TerapkanFilter(int? pilihId)
    {
        _tampil = _semua.Where(p => p.Status == StatusPinjaman.Aktif ||
                                    (chkLunas.Checked && p.Status == StatusPinjaman.Lunas)).ToList();

        _mengisi = true;
        gridPinjaman.DataSource = _tampil.Select(x => new
        {
            x.IdPinjaman,
            Anggota = x.Anggota.Nama,
            Jenis = x.NamaJenis,
            Pokok = Tema.Rp(x.HitungPokokPinjaman()),
            SisaPokok = Tema.Rp(x.GetSisaPokok()),
            Status = x.Status.ToString(),
            TglCair = x.TanggalCair?.ToString("dd-MM-yyyy") ?? "-"
        }).ToList();
        UiBantu.Judul(gridPinjaman, ("IdPinjaman", "ID"), ("SisaPokok", "Sisa Pokok"), ("TglCair", "Tgl Cair"));
        if (gridPinjaman.Columns["IdPinjaman"] is { } kolomId) kolomId.FillWeight = 20;

        gridPinjaman.CurrentCell = null;
        gridPinjaman.ClearSelection();
        if (pilihId is int id)
        {
            foreach (DataGridViewRow baris in gridPinjaman.Rows)
            {
                if ((int)baris.Cells["IdPinjaman"].Value == id)
                {
                    baris.Selected = true;
                    break;
                }
            }
        }
        _mengisi = false;

        PilihDariBaris();
    }

    private void PilihDariBaris()
    {
        if (gridPinjaman.SelectedRows.Count == 0)
        {
            _terpilih = null;
        }
        else
        {
            int id = (int)gridPinjaman.SelectedRows[0].Cells["IdPinjaman"].Value;
            _terpilih = _tampil.FirstOrDefault(x => x.IdPinjaman == id);
        }
        TampilkanRincian();
    }

    private void TampilkanRincian()
    {
        cmbAngsuran.Items.Clear();

        if (_terpilih is null)
        {
            lblInfo.Text = _tampil.Count == 0 ? "Tidak ada pinjaman berjalan." : "Pilih pinjaman di tabel.";
            lblRingkas.Text = string.Empty;
            gridJadwal.DataSource = null;
            gridRiwayat.DataSource = null;
            PerbaruiSisa();
            return;
        }

        var p = _terpilih;
        var hariIni = DateOnly.FromDateTime(DateTime.Today);

        lblInfo.Text =
            $"Pinjaman #{p.IdPinjaman} | {p.Anggota.Nama}\n" +
            $"{p.NamaJenis}, tenor {p.TenorBulan} bulan, bunga {p.SukuBungaTahunan:0.##}% per tahun\n" +
            $"Sisa pokok: {Tema.Rp(p.GetSisaPokok())}";

        decimal totalTagihan = p.JadwalAngsuran.Sum(a => a.HitungTagihan());
        decimal totalBayar = p.JadwalAngsuran.Sum(a => a.GetTerbayar());
        lblRingkas.Text = $"Total tagihan {Tema.Rp(totalTagihan)}  |  Terbayar {Tema.Rp(totalBayar)}  |  " +
                          $"Sisa {Tema.Rp(totalTagihan - totalBayar)}";

        gridJadwal.DataSource = p.JadwalAngsuran.Select(a => new
        {
            No = a.NoAngsuran,
            JatuhTempo = a.JatuhTempo.ToString("dd-MM-yyyy"),
            Tagihan = Tema.Rp(a.HitungTagihan()),
            Terbayar = Tema.Rp(a.GetTerbayar()),
            Sisa = Tema.Rp(a.GetSisaTagihan()),
            Status = a.Status.ToString(),
            Keterangan = a.Terlambat(hariIni) ? "Terlambat" : string.Empty
        }).ToList();
        UiBantu.Judul(gridJadwal, ("JatuhTempo", "Jatuh Tempo"));
        if (gridJadwal.Columns["No"] is { } kolomNo) kolomNo.FillWeight = 18;

        var riwayat = p.JadwalAngsuran
            .SelectMany(a => a.Pembayaran.Select(b => (a.NoAngsuran, Pembayaran: b)))
            .OrderByDescending(x => x.Pembayaran.Tanggal)
            .ThenByDescending(x => x.Pembayaran.IdPembayaran)
            .ToList();
        gridRiwayat.DataSource = riwayat.Select(x => new
        {
            Tanggal = x.Pembayaran.Tanggal.ToString("dd-MM-yyyy"),
            Angsuran = "ke-" + x.NoAngsuran,
            Jumlah = Tema.Rp(x.Pembayaran.Jumlah),
            Sumber = x.Pembayaran.Sumber == SumberPembayaran.PotongPanen
                ? $"Potong panen (setoran #{x.Pembayaran.IdSetoran})" : "Tunai"
        }).ToList();

        // Hanya angsuran yang belum lunas yang dapat dipilih
        foreach (var a in p.JadwalAngsuran.Where(a => a.Status != StatusAngsuran.Lunas))
        {
            cmbAngsuran.Items.Add(new PilihanAngsuran(a.NoAngsuran, a.GetSisaTagihan(),
                $"Angsuran ke-{a.NoAngsuran} | jatuh tempo {a.JatuhTempo:dd-MM-yyyy} | sisa {Tema.Rp(a.GetSisaTagihan())}"));
        }
        if (cmbAngsuran.Items.Count > 0) cmbAngsuran.SelectedIndex = 0;
        PerbaruiSisa();
    }

    private void PerbaruiSisa()
    {
        if (cmbAngsuran.SelectedItem is PilihanAngsuran pilihan)
            lblSisa.Text = $"Sisa tagihan angsuran ke-{pilihan.No}: {Tema.Rp(pilihan.Sisa)}";
        else
            lblSisa.Text = _terpilih is null ? string.Empty : "Semua angsuran pinjaman ini sudah lunas.";

        bool ada = cmbAngsuran.SelectedItem is PilihanAngsuran;
        btnBayar.Enabled = _bolehKelola && ada;
        btnIsi.Enabled = _bolehKelola && ada;
    }

    private void IsiSesuaiSisa()
    {
        if (cmbAngsuran.SelectedItem is PilihanAngsuran pilihan)
            numJumlah.Value = Math.Min(pilihan.Sisa, numJumlah.Maximum);
    }

    private void Bayar()
    {
        if (_terpilih is null || cmbAngsuran.SelectedItem is not PilihanAngsuran pilihan)
        {
            UiBantu.Pesan(lblPesan, "Pilih pinjaman dan angsuran terlebih dahulu.", true);
            return;
        }

        int id = _terpilih.IdPinjaman;
        try
        {
            _kontrol.BayarAngsuran(id, pilihan.No, numJumlah.Value,
                                   DateOnly.FromDateTime(dtpTanggal.Value));
            numJumlah.Value = 0m;
            Muat(id);

            string pesan = $"Pembayaran angsuran ke-{pilihan.No} tercatat.";
            if (_semua.FirstOrDefault(x => x.IdPinjaman == id)?.Status == StatusPinjaman.Lunas)
                pesan += $" Pinjaman #{id} sudah lunas.";
            UiBantu.Pesan(lblPesan, pesan, false);
        }
        catch (AturanBisnisException ex)
        {
            UiBantu.Pesan(lblPesan, ex.Message, true);
        }
        catch (Exception ex)
        {
            UiBantu.Pesan(lblPesan, "Gagal menyimpan: " + ex.Message, true);
        }
    }
}