namespace SIKOTANI.Views;

public class TabParameter : UserControl
{
    private readonly ParameterController _kontrol = new();

    private readonly NumericUpDown numPokok = new() { Width = 400, Maximum = 9_999_999_999m, ThousandsSeparator = true, Increment = 10000m };
    private readonly NumericUpDown numBungaTabungan = new() { Width = 400, DecimalPlaces = 2, Maximum = 100m, Increment = 0.5m };
    private readonly NumericUpDown numBungaPinjaman = new() { Width = 400, DecimalPlaces = 2, Maximum = 100m, Increment = 0.5m };
    private readonly Label lblBatasTabungan = new() { AutoSize = true, MaximumSize = new Size(420, 0), ForeColor = Tema.TeksPudar };
    private readonly Label lblBatasPinjaman = new() { AutoSize = true, MaximumSize = new Size(420, 0), ForeColor = Tema.TeksPudar };
    private readonly Button btnSimpan = new() { Text = "Simpan Parameter", Width = 200, Height = 40 };
    private readonly Button btnBatal = new() { Text = "Batalkan Perubahan", Width = 200, Height = 40 };
    private readonly Label lblPesan = new() { AutoSize = true, MaximumSize = new Size(420, 0), Margin = new Padding(3, 14, 3, 0) };

    public TabParameter(bool bolehKelola)
    {
        Dock = DockStyle.Fill;

        var kartu = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Tema.Kartu,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(24)
        };

        void Tambah(string teks, Control kontrol)
        {
            kartu.Controls.Add(new Label { Text = teks, AutoSize = true, Margin = new Padding(3, 14, 3, 2) });
            kartu.Controls.Add(kontrol);
        }

        kartu.Controls.Add(new Label { Text = "Parameter koperasi", Font = Tema.HurufSubjudul, AutoSize = true });
        kartu.Controls.Add(new Label
        {
            Text = "Nilai di bawah dipakai pada transaksi dan pengajuan berikutnya.",
            AutoSize = true,
            MaximumSize = new Size(420, 0),
            ForeColor = Tema.TeksPudar,
            Margin = new Padding(3, 4, 3, 0)
        });

        Tambah("Nilai simpanan pokok (Rp)", numPokok);
        Tambah("Bunga tabungan berlaku (% per tahun)", numBungaTabungan);
        kartu.Controls.Add(lblBatasTabungan);
        Tambah("Bunga pinjaman berlaku (% per tahun)", numBungaPinjaman);
        kartu.Controls.Add(lblBatasPinjaman);

        var baris = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0, 20, 0, 0) };
        baris.Controls.Add(btnSimpan);
        baris.Controls.Add(btnBatal);
        kartu.Controls.Add(baris);
        kartu.Controls.Add(lblPesan);

        kartu.Controls.Add(new Label
        {
            Text = "Catatan: bunga pinjaman yang sudah diajukan tetap memakai bunga saat pengajuan, " +
                   "dan simpanan pokok yang sudah dibayar tidak berubah. Batas maksimum bunga mengikuti " +
                   "Permenkop UKM No. 8 Tahun 2023 dan tidak dapat diubah dari aplikasi.",
            AutoSize = true,
            MaximumSize = new Size(420, 0),
            ForeColor = Tema.TeksPudar,
            Margin = new Padding(3, 24, 3, 0)
        });

        kartu.Enabled = bolehKelola;
        Controls.Add(kartu);

        btnSimpan.Click += (_, _) => Simpan();
        btnBatal.Click += (_, _) => Muat();
        Load += (_, _) => Muat();
    }

    private void Muat()
    {
        try
        {
            var p = _kontrol.Muat();
            numPokok.Value = p.NilaiSimpananPokok;
            numBungaTabungan.Value = p.BungaTabunganBerlaku;
            numBungaPinjaman.Value = p.BungaPinjamanBerlaku;
            lblBatasTabungan.Text = $"Batas maksimum: {p.BungaTabunganMaks:0.##}% per tahun (Permenkop UKM No. 8 Tahun 2023 Pasal 26 ayat 4)";
            lblBatasPinjaman.Text = $"Batas maksimum: {p.BungaPinjamanMaks:0.##}% per tahun (Permenkop UKM No. 8 Tahun 2023 Pasal 27 ayat 3)";
            lblPesan.Text = string.Empty;
        }
        catch (Exception ex)
        {
            UiBantu.Pesan(lblPesan, "Gagal memuat parameter: " + ex.Message, true);
        }
    }

    private void Simpan()
    {
        try
        {
            _kontrol.Simpan(numPokok.Value, numBungaTabungan.Value, numBungaPinjaman.Value);
            UiBantu.Pesan(lblPesan, "Parameter tersimpan. Berlaku untuk transaksi dan pengajuan berikutnya.", false);
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