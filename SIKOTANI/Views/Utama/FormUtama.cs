using SIKOTANI.Models;

namespace SIKOTANI.Views;

public partial class FormUtama : Form
{
    private readonly AuthController _auth = new();
    private readonly Pengguna _pengguna;
    private readonly Label lblJudul = new()
    {
        Dock = DockStyle.Top,
        Height = 70,
        Font = new Font("Segoe UI", 16, FontStyle.Bold),
        Padding = new Padding(20, 20, 0, 0),
        Text = "Selamat datang di SIKOTANI"
    };
    private readonly Label lblIsi = new()
    {
        Dock = DockStyle.Fill,
        Font = new Font("Segoe UI", 11),
        Padding = new Padding(20, 10, 20, 0),
        Text = "Pilih menu di sebelah kiri."
    };

    // Modul yang sudah jadi didaftarkan di sini pada tahap-tahap berikutnya
    private readonly Dictionary<MenuAplikasi, Func<Form>> _modul = new();

    private static readonly (MenuAplikasi Menu, string Teks, int Tahap)[] DaftarMenu =
    {
        (MenuAplikasi.Keanggotaan, "Anggota, Kelompok Tani, Komoditas", 6),
        (MenuAplikasi.DanaAnggota, "Dana Anggota", 7),
        (MenuAplikasi.PengajuanPinjaman, "Pengajuan Pinjaman", 8),
        (MenuAplikasi.PersetujuanPinjaman, "Persetujuan Pinjaman", 8),
        (MenuAplikasi.PembayaranAngsuran, "Pembayaran Angsuran", 8),
        (MenuAplikasi.SetoranPanen, "Setoran Panen", 9),
        (MenuAplikasi.ParameterDanAkun, "Parameter dan Akun", 10),
        (MenuAplikasi.LaporanAnggota, "Laporan Anggota", 10),
        (MenuAplikasi.LaporanKeuangan, "Laporan Keuangan", 10)
    };

    public FormUtama()
    {
        InitializeComponent();

        // Abstraksi: form hanya tahu tipe Pengguna, bukan Ketua/Sekretaris/Bendahara
        _pengguna = Sesi.PenggunaAktif
            ?? throw new InvalidOperationException("Belum ada sesi masuk.");
        _modul[MenuAplikasi.Keanggotaan] = () => new FormKeanggotaan();
        _modul[MenuAplikasi.DanaAnggota] = () => new FormDanaAnggota();

        Text = "SIKOTANI - Sistem Informasi Koperasi Tani";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(960, 600);
        MinimumSize = new Size(800, 520);

        // Area isi (Fill) ditambahkan lebih dulu, lalu panel tepi
        var panelIsi = new Panel { Dock = DockStyle.Fill };
        panelIsi.Controls.Add(lblIsi);
        panelIsi.Controls.Add(lblJudul);

        var panelMenu = new FlowLayoutPanel
        {
            Dock = DockStyle.Left,
            Width = 260,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(10),
            BackColor = Color.WhiteSmoke
        };

        panelMenu.Controls.Add(new Label
        {
            Text = $"{_pengguna.Anggota.Nama}\n{_pengguna.GetPeran()}",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            AutoSize = true,
            Margin = new Padding(3, 3, 3, 15)
        });

        // Polymorphism: satu loop untuk semua peran, hasilnya berbeda per peran
        foreach (var (menu, teks, tahap) in DaftarMenu)
        {
            var hak = _pengguna.GetHakAkses(menu);
            if (hak == HakAkses.Tidak) continue; // menu disembunyikan

            var tombol = new Button
            {
                Text = hak == HakAkses.Lihat ? $"{teks} (lihat)" : teks,
                Width = 225,
                Height = 40,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(3, 3, 3, 6)
            };
            tombol.Click += (_, _) => BukaMenu(menu, teks, tahap);
            panelMenu.Controls.Add(tombol);
        }

        // Tombol sementara: hanya untuk Ketua, dihapus di Tahap 10
        if (_pengguna.BolehKelola(MenuAplikasi.ParameterDanAkun))
        {
            var btnContoh = new Button
            {
                Text = "[Sementara] Buat Akun Contoh",
                Width = 225,
                Height = 40,
                Margin = new Padding(3, 20, 3, 6)
            };
            btnContoh.Click += (_, _) => BuatAkunContoh();
            panelMenu.Controls.Add(btnContoh);
        }

        var btnKeluar = new Button
        {
            Text = "Keluar (ganti pengguna)",
            Width = 225,
            Height = 40,
            Margin = new Padding(3, 20, 3, 6)
        };
        btnKeluar.Click += (_, _) =>
        {
            _auth.Keluar(); // Program.cs menampilkan login lagi
            Close();
        };
        panelMenu.Controls.Add(btnKeluar);

        var status = new StatusStrip();
        status.Items.Add(new ToolStripStatusLabel(
            $"Pengguna: {_pengguna.Anggota.Nama} ({_pengguna.Username})  |  Peran: {_pengguna.GetPeran()}"));

        Controls.Add(panelIsi);
        Controls.Add(panelMenu);
        Controls.Add(status);
    }

    private void BukaMenu(MenuAplikasi menu, string teks, int tahap)
    {
        // Lapis kedua: meski tombol disembunyikan, hak tetap diperiksa ulang
        try
        {
            _pengguna.PastikanBolehAkses(menu);
        }
        catch (AturanBisnisException ex)
        {
            MessageBox.Show(ex.Message, "SIKOTANI",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_modul.TryGetValue(menu, out var buatForm))
        {
            using var form = buatForm();
            form.ShowDialog(this);
            return;
        }

        var hak = _pengguna.GetHakAkses(menu);
        lblJudul.Text = teks;
        lblIsi.Text =
            $"Hak akses {_pengguna.GetPeran()}: " +
            (hak == HakAkses.Kelola ? "Kelola (lihat dan ubah)" : "Lihat saja") +
            $"\n\nModul ini dibuat pada Tahap {tahap}.";
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
            MessageBox.Show(ex.Message, "SIKOTANI",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}