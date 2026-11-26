namespace SIKOTANI.Views;

public class HalamanLaporanKeuangan : UserControl
{
    private sealed record ItemLaporan(string Judul, PanelLaporan Panel, Func<TabelLaporan> Ambil);

    private readonly LaporanController _kontrol = new();
    private readonly List<ItemLaporan> _item = new();
    private readonly TabControl tabs = new() { Dock = DockStyle.Fill };
    private readonly Label lblPesan = new() { Dock = DockStyle.Top, Height = 28, ForeColor = Tema.Bahaya };

    public HalamanLaporanKeuangan()
    {
        Dock = DockStyle.Fill;

        _item.Add(new ItemLaporan("Dana Anggota", new PanelLaporan(), _kontrol.LaporanDana));
        _item.Add(new ItemLaporan("Pinjaman Aktif", new PanelLaporan(), _kontrol.LaporanPinjaman));
        _item.Add(new ItemLaporan("Angsuran Jatuh Tempo", new PanelLaporan(), _kontrol.LaporanJatuhTempo));
        _item.Add(new ItemLaporan("Hasil Panen", new PanelLaporan(), _kontrol.LaporanPanen));

        foreach (var item in _item)
        {
            var halaman = new TabPage(item.Judul) { Padding = new Padding(10) };
            item.Panel.Dock = DockStyle.Fill;
            halaman.Controls.Add(item.Panel);
            tabs.TabPages.Add(halaman);
        }

        Controls.Add(tabs); // Fill lebih dulu
        Controls.Add(lblPesan);

        // Tab dimuat ulang tiap dibuka, agar angkanya selalu terbaru
        tabs.SelectedIndexChanged += (_, _) => Muat(tabs.SelectedIndex);
        Load += (_, _) => Muat(0);
    }

    private void Muat(int indeks)
    {
        if (indeks < 0 || indeks >= _item.Count) return;
        try
        {
            lblPesan.Text = string.Empty;
            _item[indeks].Panel.Tampilkan(_item[indeks].Ambil());
        }
        catch (Exception ex)
        {
            lblPesan.Text = "Gagal memuat laporan: " + ex.Message;
        }
    }
}