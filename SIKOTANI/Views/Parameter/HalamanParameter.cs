namespace SIKOTANI.Views;

public class HalamanParameter : UserControl
{
    public HalamanParameter()
    {
        Dock = DockStyle.Fill;

        var pengguna = Sesi.PenggunaAktif
            ?? throw new InvalidOperationException("Belum ada sesi masuk.");
        bool bolehKelola = pengguna.BolehKelola(MenuAplikasi.ParameterDanAkun);

        var tabs = new TabControl { Dock = DockStyle.Fill };

        var halamanParameter = new TabPage("Parameter Koperasi") { Padding = new Padding(10) };
        halamanParameter.Controls.Add(new TabParameter(bolehKelola));

        var halamanAkun = new TabPage("Akun Pengurus") { Padding = new Padding(10) };
        halamanAkun.Controls.Add(new TabAkun(bolehKelola));

        tabs.TabPages.Add(halamanParameter);
        tabs.TabPages.Add(halamanAkun);
        Controls.Add(tabs);
    }
}