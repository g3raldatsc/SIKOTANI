namespace SIKOTANI.Views;

public class HalamanPersetujuan : UserControl
{
    private readonly PinjamanController _kontrol = new();

    private readonly DataGridView gridMenunggu = UiBantu.BuatGrid();
    private readonly DataGridView gridPratinjau = UiBantu.BuatGrid();
    private readonly Label lblRingkas = new()
    {
        Dock = DockStyle.Bottom,
        Height = 32,
        Font = Tema.HurufTebal,
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
    private readonly DateTimePicker dtpTanggal = new() { Width = 360, Format = DateTimePickerFormat.Short };
    private readonly Button btnSetujui = new() { Text = "Setujui Pinjaman", Width = 360, Height = 42, Margin = new Padding(3, 16, 3, 3), Enabled = false };
    private readonly Button btnTolak = new() { Text = "Tolak Pinjaman", Width = 360, Height = 42, Enabled = false };

    // Pesan ditaruh di panel tetap di dasar (tidak ikut tergulung), agar selalu terlihat
    private readonly Label lblPesan = new() { AutoSize = false, Dock = DockStyle.Fill, Margin = Padding.Empty };

    private List<Pinjaman> _menunggu = new();
    private Pinjaman? _terpilih;
    private bool _mengisi;

    public HalamanPersetujuan()
    {
        Dock = DockStyle.Fill;

        var pengguna = Sesi.PenggunaAktif
            ?? throw new InvalidOperationException("Belum ada sesi masuk.");
        bool bolehKelola = pengguna.BolehKelola(MenuAplikasi.PersetujuanPinjaman);

        dtpTanggal.MaxDate = DateTime.Today;
        dtpTanggal.Value = DateTime.Today;

        // ---------- panel kanan: keputusan ----------
        kanan.Controls.Add(new Label { Text = "Keputusan", Font = Tema.HurufSubjudul, AutoSize = true });
        kanan.Controls.Add(lblInfo);
        kanan.Controls.Add(new Label { Text = "Tanggal persetujuan (tanggal cair)", AutoSize = true, Margin = new Padding(3, 8, 3, 2) });
        kanan.Controls.Add(dtpTanggal);
        kanan.Controls.Add(btnSetujui);
        kanan.Controls.Add(btnTolak);
        kanan.Enabled = bolehKelola;

        var panelPesan = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 70,
            BackColor = Tema.Kartu,
            Padding = new Padding(14, 8, 14, 8)
        };
        panelPesan.Controls.Add(lblPesan);

        var kananLuar = new Panel { Dock = DockStyle.Right, Width = 400, BackColor = Tema.Kartu };
        kananLuar.Controls.Add(kanan);        // Fill lebih dulu
        kananLuar.Controls.Add(panelPesan);   // lalu panel tepi bawah

        // ---------- panel kiri ----------
        var panelAtas = new Panel { Dock = DockStyle.Top, Height = 250 };
        panelAtas.Controls.Add(gridMenunggu);
        panelAtas.Controls.Add(new Label
        {
            Dock = DockStyle.Top,
            Height = 30,
            Text = "Menunggu persetujuan",
            Font = Tema.HurufSubjudul,
            TextAlign = ContentAlignment.MiddleLeft
        });

        var panelBawah = new Panel { Dock = DockStyle.Fill };
        panelBawah.Controls.Add(gridPratinjau);
        panelBawah.Controls.Add(lblRingkas);
        panelBawah.Controls.Add(new Label
        {
            Dock = DockStyle.Top,
            Height = 34,
            Text = "Pratinjau jadwal angsuran (bila disetujui)",
            Font = Tema.HurufSubjudul,
            TextAlign = ContentAlignment.BottomLeft
        });

        var kiri = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 0, 12, 0) };
        kiri.Controls.Add(panelBawah);
        kiri.Controls.Add(panelAtas);

        Controls.Add(kiri);
        Controls.Add(kananLuar);

        gridMenunggu.SelectionChanged += (_, _) => PilihDariBaris();
        dtpTanggal.ValueChanged += (_, _) => PerbaruiPratinjau();
        btnSetujui.Click += (_, _) => Setujui();
        btnTolak.Click += (_, _) => Tolak();
        Load += (_, _) => Muat();
    }

    private void Muat()
    {
        try
        {
            _menunggu = _kontrol.Daftar(StatusPinjaman.Diajukan);
        }
        catch (Exception ex)
        {
            UiBantu.Pesan(lblPesan, "Gagal memuat data: " + ex.Message, true);
            return;
        }

        _mengisi = true;
        gridMenunggu.DataSource = _menunggu.Select(x => new
        {
            x.IdPinjaman,
            Anggota = x.Anggota.Nama,
            Jenis = x.NamaJenis,
            Pokok = Tema.Rp(x.HitungPokokPinjaman()),
            Tenor = x.TenorBulan + " bln",
            Bunga = x.SukuBungaTahunan.ToString("0.##") + "%",
            Pengajuan = x.TanggalPengajuan.ToString("dd-MM-yyyy")
        }).ToList();
        UiBantu.Judul(gridMenunggu, ("IdPinjaman", "ID"), ("Pengajuan", "Tgl Pengajuan"));
        if (gridMenunggu.Columns["IdPinjaman"] is { } kolomId) kolomId.FillWeight = 20;
        gridMenunggu.CurrentCell = null;
        gridMenunggu.ClearSelection();
        _mengisi = false;

        PilihDariBaris();
    }

    private void PilihDariBaris()
    {
        if (_mengisi) return;

        if (gridMenunggu.SelectedRows.Count == 0)
        {
            _terpilih = null;
            lblInfo.Text = _menunggu.Count == 0
                ? "Tidak ada pinjaman yang menunggu persetujuan."
                : "Pilih pinjaman di tabel.";
            PerbaruiPratinjau();
            return;
        }

        int id = (int)gridMenunggu.SelectedRows[0].Cells["IdPinjaman"].Value;
        _terpilih = _menunggu.First(x => x.IdPinjaman == id);
        lblInfo.Text =
            $"Pinjaman #{_terpilih.IdPinjaman}\nAnggota: {_terpilih.Anggota.Nama}\n" +
            $"Pokok: {Tema.Rp(_terpilih.HitungPokokPinjaman())} | Tenor {_terpilih.TenorBulan} bulan | " +
            $"Bunga {_terpilih.SukuBungaTahunan:0.##}% per tahun\n\n{_terpilih.Rincian()}";
        lblPesan.Text = string.Empty;
        PerbaruiPratinjau();
    }

    private void PerbaruiPratinjau()
    {
        btnSetujui.Enabled = _terpilih is not null;
        btnTolak.Enabled = _terpilih is not null;

        if (_terpilih is null)
        {
            gridPratinjau.DataSource = null;
            lblRingkas.Text = string.Empty;
            return;
        }

        var tanggal = DateOnly.FromDateTime(dtpTanggal.Value);
        var jadwal = _kontrol.PratinjauJadwal(_terpilih, tanggal);

        gridPratinjau.DataSource = jadwal.Select(a => new
        {
            No = a.NoAngsuran,
            JatuhTempo = a.JatuhTempo.ToString("dd-MM-yyyy"),
            Pokok = Tema.Rp(a.Pokok),
            Bunga = Tema.Rp(a.Bunga),
            Tagihan = Tema.Rp(a.HitungTagihan())
        }).ToList();
        UiBantu.Judul(gridPratinjau, ("JatuhTempo", "Jatuh Tempo"));
        if (gridPratinjau.Columns["No"] is { } kolomNo) kolomNo.FillWeight = 20;

        lblRingkas.Text = $"Total bunga {Tema.Rp(jadwal.Sum(a => a.Bunga))}  |  " +
                          $"Total yang dibayar {Tema.Rp(jadwal.Sum(a => a.HitungTagihan()))}";
    }

    private void Setujui()
    {
        if (_terpilih is null) return;

        var jawab = MessageBox.Show(
            $"Setujui pinjaman #{_terpilih.IdPinjaman} atas nama {_terpilih.Anggota.Nama} " +
            $"sebesar {Tema.Rp(_terpilih.HitungPokokPinjaman())}?\nJadwal angsuran akan dibuat.",
            "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (jawab != DialogResult.Yes) return;

        try
        {
            int id = _terpilih.IdPinjaman;
            _kontrol.Setujui(id, DateOnly.FromDateTime(dtpTanggal.Value));
            _terpilih = null;
            Muat();
            UiBantu.Pesan(lblPesan, $"Pinjaman #{id} disetujui. Jadwal angsuran dibuat.", false);
        }
        catch (AturanBisnisException ex)
        {
            UiBantu.Pesan(lblPesan, ex.Message, true);
        }
        catch (Exception ex)
        {
            UiBantu.Pesan(lblPesan, "Gagal: " + ex.Message, true);
        }
    }

    private void Tolak()
    {
        if (_terpilih is null) return;

        var jawab = MessageBox.Show(
            $"Tolak pinjaman #{_terpilih.IdPinjaman} atas nama {_terpilih.Anggota.Nama}?",
            "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (jawab != DialogResult.Yes) return;

        try
        {
            int id = _terpilih.IdPinjaman;
            _kontrol.Tolak(id);
            _terpilih = null;
            Muat();
            UiBantu.Pesan(lblPesan, $"Pinjaman #{id} ditolak.", false);
        }
        catch (AturanBisnisException ex)
        {
            UiBantu.Pesan(lblPesan, ex.Message, true);
        }
        catch (Exception ex)
        {
            UiBantu.Pesan(lblPesan, "Gagal: " + ex.Message, true);
        }
    }
}