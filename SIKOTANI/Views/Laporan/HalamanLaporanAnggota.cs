namespace SIKOTANI.Views;

public class HalamanLaporanAnggota : UserControl
{
    private readonly LaporanController _kontrol = new();
    private readonly PanelLaporan _panel = new();
    private readonly Label lblPesan = new() { Dock = DockStyle.Top, Height = 28, ForeColor = Tema.Bahaya };

    public HalamanLaporanAnggota()
    {
        Dock = DockStyle.Fill;
        Controls.Add(_panel);       // Fill lebih dulu
        Controls.Add(lblPesan);
        Load += (_, _) => Muat();
    }

    private void Muat()
    {
        try
        {
            _panel.Tampilkan(_kontrol.LaporanAnggota());
        }
        catch (Exception ex)
        {
            lblPesan.Text = "Gagal memuat laporan: " + ex.Message;
        }
    }
}