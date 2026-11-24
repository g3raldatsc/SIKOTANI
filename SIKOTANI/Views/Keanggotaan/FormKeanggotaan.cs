using SIKOTANI.Models;

namespace SIKOTANI.Views;

public partial class FormKeanggotaan : Form
{
    public FormKeanggotaan()
    {
        InitializeComponent();

        var pengguna = Sesi.PenggunaAktif
            ?? throw new InvalidOperationException("Belum ada sesi masuk.");
        bool bolehKelola = pengguna.BolehKelola(MenuAplikasi.Keanggotaan);

        Text = "Anggota, Kelompok Tani, dan Komoditas";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(1040, 620);
        MinimumSize = new Size(900, 560);

        var tabAnggota = new TabAnggota(bolehKelola);
        var tabKelompok = new TabKelompok(bolehKelola);
        var tabKomoditas = new TabKomoditas(bolehKelola);

        var tabs = new TabControl { Dock = DockStyle.Fill };
        void TambahHalaman(string judul, Control isi)
        {
            var halaman = new TabPage(judul);
            halaman.Controls.Add(isi);
            tabs.TabPages.Add(halaman);
        }
        TambahHalaman("Anggota", tabAnggota);
        TambahHalaman("Kelompok Tani", tabKelompok);
        TambahHalaman("Komoditas", tabKomoditas);

        Controls.Add(tabs); // Fill lebih dulu

        if (!bolehKelola)
        {
            Controls.Add(new Label
            {
                Dock = DockStyle.Top,
                Height = 32,
                Text = $"Mode lihat saja: peran {pengguna.GetPeran()} tidak dapat mengubah data keanggotaan.",
                BackColor = Color.LightYellow,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            });
        }

        // Data tab lain bisa berubah (misalnya komoditas baru), jadi tab aktif dimuat ulang
        tabs.SelectedIndexChanged += (_, _) =>
        {
            switch (tabs.SelectedIndex)
            {
                case 0: tabAnggota.Segarkan(); break;
                case 1: tabKelompok.Segarkan(); break;
                case 2: tabKomoditas.Segarkan(); break;
            }
        };

        Load += (_, _) =>
        {
            tabAnggota.Segarkan();
            tabKelompok.Segarkan();
            tabKomoditas.Segarkan();
        };
    }
}