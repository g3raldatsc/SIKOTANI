namespace SIKOTANI.Views;

public partial class FormUtama : Form
{
    private readonly AuthController _auth = new();
    private readonly DashboardController _dashboard = new();
    private readonly Pengguna _pengguna;

    private readonly Panel panelIsi = new() { Dock = DockStyle.Fill, BackColor = Tema.Latar, Padding = new Padding(20) };
    private readonly Label lblJudul = new() { AutoSize = false, Left = 24, Top = 10, Width = 640, Height = 34, Font = Tema.HurufJudul, ForeColor = Tema.Teks };
    private readonly Label lblSub = new() { AutoSize = false, Left = 26, Top = 46, Width = 640, Height = 20, Font = Tema.Huruf, ForeColor = Tema.TeksPudar };
    private readonly Label lblTanggal = new() { AutoSize = false, Dock = DockStyle.Right, Width = 330, Font = Tema.Huruf, ForeColor = Tema.TeksPudar, TextAlign = ContentAlignment.MiddleRight, Padding = new Padding(0, 0, 24, 0) };

    // Modul yang sudah jadi didaftarkan di sini pada tahap-tahap berikutnya
    private readonly Dictionary<MenuAplikasi, Func<Control>> _modul = new();
    private ItemMenu? _itemAktif;

    // Ikon: kode karakter Segoe MDL2 Assets / Segoe Fluent Icons
    private static readonly (MenuAplikasi Menu, string Teks, int Tahap, string Glyph)[] DaftarMenu =
    {
        (MenuAplikasi.Keanggotaan,         "Keanggotaan",          6,  "\uE716"),
        (MenuAplikasi.DanaAnggota,         "Dana Anggota",         7,  "\uE825"),
        (MenuAplikasi.PengajuanPinjaman,   "Pengajuan Pinjaman",   8,  "\uE8EF"),
        (MenuAplikasi.PersetujuanPinjaman, "Persetujuan Pinjaman", 8,  "\uE73E"),
        (MenuAplikasi.PembayaranAngsuran,  "Pembayaran Angsuran",  8,  "\uE8EF"),
        (MenuAplikasi.SetoranPanen,        "Setoran Panen",        9,  "\uE7BF"),
        (MenuAplikasi.ParameterDanAkun,    "Parameter dan Akun",   10, "\uE713"),
        (MenuAplikasi.LaporanAnggota,      "Laporan Anggota",      10, "\uE8A5"),
        (MenuAplikasi.LaporanKeuangan,     "Laporan Keuangan",     10, "\uE8A5")
    };

    public FormUtama()
    {
        InitializeComponent();

        // Abstraksi: form hanya tahu tipe Pengguna, bukan Ketua/Sekretaris/Bendahara
        _pengguna = Sesi.PenggunaAktif
            ?? throw new InvalidOperationException("Belum ada sesi masuk.");

        Text = "SIKOTANI - Sistem Informasi Koperasi Tani";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1240, 760);
        MinimumSize = new Size(1040, 640);
        BackColor = Tema.Latar;

        _modul[MenuAplikasi.Keanggotaan] = () => new FormKeanggotaan();
        _modul[MenuAplikasi.DanaAnggota] = () => new FormDanaAnggota();

        // KANAN: header dan isi
        var header = new Panel { Dock = DockStyle.Top, Height = 74, BackColor = Color.White };
        lblTanggal.Text = DateTime.Today.ToString("dddd, d MMMM yyyy");
        header.Controls.Add(lblTanggal);
        header.Controls.Add(lblJudul);
        header.Controls.Add(lblSub);
        header.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Tema.Garis });

        var kanan = new Panel { Dock = DockStyle.Fill };
        kanan.Controls.Add(panelIsi);
        kanan.Controls.Add(header);

        // KIRI: sidebar
        var sidebar = new Panel { Dock = DockStyle.Left, Width = 252, BackColor = Tema.Sidebar };

        var brand = new Panel { Dock = DockStyle.Top, Height = 84, BackColor = Tema.Sidebar };
        brand.Controls.Add(new Label
        {
            Text = "SIKOTANI",
            Font = new Font("Segoe UI Semibold", 18f),
            ForeColor = Color.White,
            AutoSize = false,
            Left = 20,
            Top = 14,
            Width = 220,
            Height = 34
        });
        brand.Controls.Add(new Label
        {
            Text = "Sistem Informasi Koperasi Tani",
            Font = new Font("Segoe UI", 8.5f),
            ForeColor = Color.FromArgb(165, 214, 167),
            AutoSize = false,
            Left = 22,
            Top = 50,
            Width = 220,
            Height = 20
        });

        var user = new Panel { Dock = DockStyle.Top, Height = 74, BackColor = Tema.SidebarSorot };
        user.Controls.Add(new Label
        {
            Text = _pengguna.Anggota.Nama,
            Font = Tema.HurufSubjudul,
            ForeColor = Color.White,
            AutoSize = false,
            Left = 20,
            Top = 14,
            Width = 220,
            Height = 24
        });
        user.Controls.Add(new Label
        {
            Text = $"{_pengguna.GetPeran()}  |  {_pengguna.Username}",
            Font = Tema.Huruf,
            ForeColor = Color.FromArgb(165, 214, 167),
            AutoSize = false,
            Left = 20,
            Top = 42,
            Width = 220,
            Height = 20
        });

        var panelMenu = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(10, 12, 10, 8),
            BackColor = Tema.Sidebar
        };

        ItemMenu BuatItem(string glyph, string teks) => new() { Glyph = glyph, Text = teks };

        var itemDashboard = BuatItem("\uE80F", "Dashboard");
        itemDashboard.Click += (_, _) => { Aktifkan(itemDashboard); TampilkanDashboard(); };
        panelMenu.Controls.Add(itemDashboard);

        // Polymorphism: satu loop untuk semua peran, hasilnya berbeda per peran
        foreach (var (menu, teks, tahap, glyph) in DaftarMenu)
        {
            var hak = _pengguna.GetHakAkses(menu);
            if (hak == HakAkses.Tidak) continue;   // menu disembunyikan

            var item = BuatItem(glyph, hak == HakAkses.Lihat ? $"{teks} (lihat)" : teks);
            item.Click += (_, _) => BukaMenu(item, menu, teks, tahap);
            panelMenu.Controls.Add(item);
        }

        // Tombol sementara: hanya untuk Ketua, dihapus di Tahap 10
        if (_pengguna.BolehKelola(MenuAplikasi.ParameterDanAkun))
        {
            var itemContoh = BuatItem("\uE713", "[Sementara] Akun Contoh");
            itemContoh.Margin = new Padding(0, 18, 0, 4);
            itemContoh.Click += (_, _) => BuatAkunContoh();
            panelMenu.Controls.Add(itemContoh);
        }

        var bawah = new Panel { Dock = DockStyle.Bottom, Height = 62, BackColor = Tema.Sidebar, Padding = new Padding(10, 8, 10, 8) };
        var itemKeluar = BuatItem("\uE7E8", "Keluar (ganti pengguna)");
        itemKeluar.Dock = DockStyle.Fill;
        itemKeluar.Click += (_, _) =>
        {
            _auth.Keluar();   // Program.cs menampilkan login lagi
            Close();
        };
        bawah.Controls.Add(itemKeluar);

        // Urutan Add: Fill dahulu, lalu panel tepi (yang terakhir ditempel paling luar)
        sidebar.Controls.Add(panelMenu);
        sidebar.Controls.Add(bawah);
        sidebar.Controls.Add(user);
        sidebar.Controls.Add(brand);

        Controls.Add(kanan);
        Controls.Add(sidebar);

        Aktifkan(itemDashboard);
        TampilkanDashboard();
    }

    // Pembantu tampilan
    private void Aktifkan(ItemMenu item)
    {
        if (_itemAktif is not null) _itemAktif.Aktif = false;
        _itemAktif = item;
        item.Aktif = true;
    }

    private void SetHeader(string judul, string sub)
    {
        lblJudul.Text = judul;
        lblSub.Text = sub;
    }

    private static void LepasData(Control induk)
    {
        foreach (Control anak in induk.Controls)
        {
            if (anak is DataGridView grid) grid.DataSource = null;
            LepasData(anak);
        }
    }

    private void BersihkanIsi()
    {
        foreach (var kontrol in panelIsi.Controls.Cast<Control>().ToList())
        {
            LepasData(kontrol);
            panelIsi.Controls.Remove(kontrol);
            kontrol.Dispose();
        }
    }

    // Dashboard
    private void TampilkanDashboard()
    {
        BersihkanIsi();
        SetHeader("Dashboard", $"Selamat datang, {_pengguna.Anggota.Nama}");

        var wadah = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            AutoScroll = true,
            BackColor = Tema.Latar
        };

        try
        {
            var r = _dashboard.Muat();
            wadah.Controls.Add(Tema.BuatKartu("\uE716", "Anggota aktif", r.AnggotaAktif.ToString("N0")));
            wadah.Controls.Add(Tema.BuatKartu("\uE77B", "Kelompok tani", r.KelompokTani.ToString("N0")));

            // Kartu yang tidak berhak dilihat peran ini tidak ditampilkan (nilainya null)
            if (r.TotalSaldoDana is decimal saldo)
                wadah.Controls.Add(Tema.BuatKartu("\uE825", "Total saldo dana anggota", Tema.Rp(saldo)));
            if (r.PinjamanAktif is int aktif)
                wadah.Controls.Add(Tema.BuatKartu("\uE8EF", "Pinjaman aktif", aktif.ToString("N0")));
            if (r.NilaiSetoran is decimal setoran)
                wadah.Controls.Add(Tema.BuatKartu("\uE7BF", "Nilai setoran panen", Tema.Rp(setoran)));
        }
        catch (Exception ex)
        {
            wadah.Controls.Add(new Label
            {
                Text = "Gagal memuat dashboard: " + ex.Message,
                ForeColor = Tema.Bahaya,
                AutoSize = true
            });
        }

        panelIsi.Controls.Add(wadah);
    }

    // Modul
    private void BukaMenu(ItemMenu item, MenuAplikasi menu, string teks, int tahap)
    {
        // Lapis kedua: meski tombol hanya muncul untuk yang berhak, hak diperiksa ulang
        try
        {
            _pengguna.PastikanBolehAkses(menu);
        }
        catch (AturanBisnisException ex)
        {
            MessageBox.Show(ex.Message, "SIKOTANI", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Aktifkan(item);
        var hak = _pengguna.GetHakAkses(menu);
        string sub = hak == HakAkses.Kelola
            ? "Hak akses Anda: kelola (lihat dan ubah)"
            : "Hak akses Anda: lihat saja";

        BersihkanIsi();
        SetHeader(teks.Replace(" (lihat)", string.Empty), sub);

        if (_modul.TryGetValue(menu, out var buatHalaman))
        {
            // Halaman modul (UserControl) disematkan di area isi
            var halaman = buatHalaman();
            halaman.Dock = DockStyle.Fill;
            panelIsi.Controls.Add(halaman);
            Tema.Terapkan(halaman);
            return;
        }

        panelIsi.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = Tema.HurufSubjudul,
            ForeColor = Tema.TeksPudar,
            Text = $"Modul ini dibuat pada Tahap {tahap}."
        });
    }

    private void BuatAkunContoh()
    {
        try
        {
            string hasil = _auth.BuatAkunContoh();
            MessageBox.Show(hasil + "\nKata sandi akun contoh: contoh1234\n(data uji)",
                "SIKOTANI", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (AturanBisnisException ex)
        {
            MessageBox.Show(ex.Message, "SIKOTANI", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}