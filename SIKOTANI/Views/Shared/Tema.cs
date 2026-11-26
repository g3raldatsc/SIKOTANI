namespace SIKOTANI.Views;

/// <summary>Satu-satunya tempat warna, huruf, dan gaya aplikasi. Ubah di sini, semua tampilan ikut berubah :3</summary>
public static class Tema
{
    // iki warna e buat tema (kalau ga sesuai nanti diganti)
    public static readonly Color Primer = Color.FromArgb(46, 125, 50);
    public static readonly Color PrimerGelap = Color.FromArgb(27, 94, 32);
    public static readonly Color Sidebar = Color.FromArgb(24, 58, 45);
    public static readonly Color SidebarSorot = Color.FromArgb(38, 84, 66);
    public static readonly Color Latar = Color.FromArgb(244, 246, 248);
    public static readonly Color Kartu = Color.White;
    public static readonly Color Teks = Color.FromArgb(33, 37, 41);
    public static readonly Color TeksPudar = Color.FromArgb(108, 117, 125);
    public static readonly Color Garis = Color.FromArgb(222, 226, 230);
    public static readonly Color Bahaya = Color.FromArgb(198, 40, 40);
    public static readonly Color Peringatan = Color.FromArgb(239, 108, 0);
    public static readonly Color Sekunder = Color.FromArgb(96, 125, 139);
    public static readonly Color Nonaktif = Color.FromArgb(205, 210, 216);

    // Huruf
    public static readonly Font Huruf = new("Segoe UI", 9.5f);
    public static readonly Font HurufTebal = new("Segoe UI Semibold", 9.5f);
    public static readonly Font HurufSubjudul = new("Segoe UI Semibold", 11f);
    public static readonly Font HurufJudul = new("Segoe UI Semibold", 16f);
    public static readonly Font HurufAngka = new("Segoe UI Semibold", 20f);

    // Ikon (Segoe Fluent Icons di Windows 11, Segoe MDL2 Assets di Windows 10)
    private static readonly string FontIkon = CariFontIkon();
    public static bool PunyaIkon => FontIkon.Length > 0;
    public static Font HurufIkon(float ukuran) => new Font(FontIkon, ukuran);

    private static string CariFontIkon()
    {
        using var koleksi = new System.Drawing.Text.InstalledFontCollection();
        var nama = koleksi.Families.Select(f => f.Name).ToHashSet();
        if (nama.Contains("Segoe Fluent Icons")) return "Segoe Fluent Icons";
        if (nama.Contains("Segoe MDL2 Assets")) return "Segoe MDL2 Assets";
        return string.Empty;
    }

    public static string Rp(decimal nilai) => FormatTeks.Rp(nilai);

    // Penerapan otomatis pada form beserta seluruh isinya
    public static void Terapkan(Control akar)
    {
        if (akar is Form form) akar.BackColor = Latar;
        Gaya(akar);
    }

    private static void Gaya(Control induk)
    {
        foreach (Control anak in induk.Controls)
        {
            switch (anak)
            {
                case Button tombol:
                    GayaTombol(tombol);
                    break;
                case DataGridView grid:
                    GayaGrid(grid);
                    break;
                case TabPage halaman:
                    halaman.UseVisualStyleBackColor = false;
                    halaman.BackColor = halaman.Tag is "kartu" ? Kartu : Latar;
                    break;
                case FlowLayoutPanel panel when panel.Dock == DockStyle.Right:
                    panel.BackColor = Kartu;    // panel input di sisi kanan menjadi kartu putih
                    break;
            }
            Gaya(anak);
        }
    }

    private static void GayaTombol(Button tombol)
    {
        string teks = tombol.Text ?? string.Empty;
        Color warna = Primer;
        if (teks.StartsWith("Hapus") || teks.StartsWith("Keluarkan") ||
            teks.StartsWith("Tolak") || teks.StartsWith("Nonaktifkan")) warna = Bahaya;
        else if (teks == "Tarik") warna = Peringatan;
        else if (teks == "Baru" || teks.StartsWith("Batalkan")) warna = Sekunder;

        tombol.FlatStyle = FlatStyle.Flat;
        tombol.FlatAppearance.BorderSize = 0;
        tombol.FlatAppearance.MouseOverBackColor = Gelapkan(warna, 0.88);
        tombol.UseVisualStyleBackColor = false;
        tombol.Cursor = Cursors.Hand;
        tombol.Font = HurufTebal;

        void Perbarui()
        {
            tombol.BackColor = tombol.Enabled ? warna : Nonaktif;
            tombol.ForeColor = tombol.Enabled ? Color.White : Color.FromArgb(120, 125, 135);
        }
        // Enabled juga berubah bila panel induknya dinonaktifkan (mode lihat saja)
        tombol.EnabledChanged += (_, _) => Perbarui();
        Perbarui();
    }

    private static void GayaGrid(DataGridView g)
    {
        g.BorderStyle = BorderStyle.None;
        g.BackgroundColor = Kartu;
        g.GridColor = Garis;
        g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        g.EnableHeadersVisualStyles = false;
        g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        g.ColumnHeadersDefaultCellStyle.BackColor = Primer;
        g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Primer;
        g.ColumnHeadersDefaultCellStyle.Font = HurufTebal;
        g.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
        g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        g.ColumnHeadersHeight = 34;
        g.RowTemplate.Height = 30;
        g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(200, 230, 201);
        g.DefaultCellStyle.SelectionForeColor = Teks;
        g.DefaultCellStyle.Padding = new Padding(4, 0, 0, 0);
        g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 249);
    }

    private static Color Gelapkan(Color c, double faktor)
        => Color.FromArgb(c.A, (int)(c.R * faktor), (int)(c.G * faktor), (int)(c.B * faktor));

    // Kartu angka untuk dashboard
    public static Panel BuatKartu(string glyph, string judul, string nilai)
    {
        var kartu = new Panel
        {
            Width = 236,
            Height = 104,
            BackColor = Kartu,
            Margin = new Padding(0, 0, 16, 16)
        };
        kartu.Paint += (_, e) =>
        {
            using var pena = new Pen(Garis);
            e.Graphics.DrawRectangle(pena, 0, 0, kartu.Width - 1, kartu.Height - 1);
        };
        kartu.Controls.Add(new Label
        {
            Text = judul,
            Font = Huruf,
            ForeColor = TeksPudar,
            AutoSize = false,
            Left = 18,
            Top = 16,
            Width = 160,
            Height = 22
        });
        kartu.Controls.Add(new Label
        {
            Text = nilai,
            Font = HurufAngka,
            ForeColor = Teks,
            AutoSize = false,
            Left = 18,
            Top = 44,
            Width = 205,
            Height = 44
        });
        if (PunyaIkon && glyph.Length > 0)
        {
            kartu.Controls.Add(new Label
            {
                Text = glyph,
                Font = HurufIkon(16f),
                ForeColor = TeksPudar,
                AutoSize = false,
                Left = 192,
                Top = 14,
                Width = 32,
                Height = 28,
                TextAlign = ContentAlignment.MiddleCenter
            });
        }
        return kartu;
    }
}