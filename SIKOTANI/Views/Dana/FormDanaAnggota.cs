using SIKOTANI.Models;

namespace SIKOTANI.Views;

public partial class FormDanaAnggota : UserControl
{
    private readonly AnggotaController _kontrolAnggota = new();
    private readonly DanaController _kontrolDana = new();
    private static readonly (string Kode, string Nama)[] JenisDana =
    {
        ("Pokok", "Simpanan Pokok"),
        ("Wajib", "Simpanan Wajib"),
        ("Tabungan", "Tabungan")
    };

    private readonly bool _bolehKelola;

    private readonly DataGridView gridAnggota = UiBantu.BuatGrid();
    private readonly DataGridView gridDana = UiBantu.BuatGrid();
    private readonly DataGridView gridRiwayat = UiBantu.BuatGrid();

    private readonly TextBox txtCari = new() { Dock = DockStyle.Top, PlaceholderText = "Cari nama atau telepon" };
    private readonly CheckBox chkKeluar = new() { Text = "Tampilkan yang sudah keluar", Dock = DockStyle.Fill };
    private readonly Label lblAnggota = new()
    {
        Dock = DockStyle.Top,
        Height = 34,
        Font = new Font("Segoe UI", 12, FontStyle.Bold),
        TextAlign = ContentAlignment.MiddleLeft,
        Padding = new Padding(8, 0, 0, 0)
    };
    private readonly Label lblRiwayat = new()
    {
        Dock = DockStyle.Top,
        Height = 26,
        Text = "Riwayat transaksi (terbaru di atas)",
        TextAlign = ContentAlignment.BottomLeft,
        Padding = new Padding(8, 0, 0, 2)
    };

    private readonly FlowLayoutPanel panelBuka = new() { Dock = DockStyle.Bottom, Height = 46, Padding = new Padding(8, 6, 0, 0) };
    private readonly ComboBox cmbJenis = new() { Width = 170, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Button btnBuka = new() { Text = "Buka Rekening", Width = 130, Height = 30 };

    private readonly FlowLayoutPanel panelAksi = new() { Dock = DockStyle.Bottom, Height = 56, Padding = new Padding(8, 8, 8, 4) };
    private readonly NumericUpDown numJumlah = new() { Width = 140, Maximum = 9_999_999_999m, ThousandsSeparator = true, Increment = 10000m };
    private readonly DateTimePicker dtpTanggal = new() { Width = 120, Format = DateTimePickerFormat.Short };
    private readonly Button btnSetor = new() { Text = "Setor", Width = 85, Height = 32 };
    private readonly Button btnTarik = new() { Text = "Tarik", Width = 85, Height = 32 };

    private readonly FlowLayoutPanel panelInfo = new()
    {
        Dock = DockStyle.Bottom,
        Height = 100,
        FlowDirection = FlowDirection.TopDown,
        WrapContents = false,
        Padding = new Padding(10, 6, 10, 0)
    };
    private readonly Label lblSaldo = new() { AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Bold) };
    private readonly Label lblImbalan = new() { AutoSize = true, Margin = new Padding(3, 4, 3, 0) };
    private readonly Label lblPesan = new() { AutoSize = true, MaximumSize = new Size(640, 0), Margin = new Padding(3, 6, 3, 0) };

    private List<Anggota> _anggota = new();
    private List<DanaAnggota> _dana = new();
    private Anggota? _terpilih;
    private DanaAnggota? _danaTerpilih;
    private ParameterKoperasi? _parameter;
    private bool _mengisi;

    private static string Rp(decimal nilai) => "Rp" + nilai.ToString("N0");

    public FormDanaAnggota()
    {
        InitializeComponent();

        var pengguna = Sesi.PenggunaAktif
            ?? throw new InvalidOperationException("Belum ada sesi masuk.");
        _bolehKelola = pengguna.BolehKelola(MenuAplikasi.DanaAnggota);

        Text = "Dana Anggota";
        ClientSize = new Size(1100, 660);

        dtpTanggal.Value = DateTime.Today;
        dtpTanggal.MaxDate = DateTime.Today; // tanggal transaksi tidak boleh di masa depan

        cmbJenis.Items.AddRange(JenisDana.Select(j => j.Nama).ToArray());
        cmbJenis.SelectedIndex = 0;

        // kiri: daftar anggota
        var panelCari = new Panel { Dock = DockStyle.Top, Height = 66, Padding = new Padding(5) };
        panelCari.Controls.Add(chkKeluar);
        panelCari.Controls.Add(txtCari);
        var kiri = new Panel { Dock = DockStyle.Left, Width = 330 };
        kiri.Controls.Add(gridAnggota);
        kiri.Controls.Add(panelCari);

        //  tengah atas: rekening dana 
        panelBuka.Controls.Add(new Label { Text = "Buka rekening baru:", AutoSize = true, Margin = new Padding(3, 8, 3, 0) });
        panelBuka.Controls.Add(cmbJenis);
        panelBuka.Controls.Add(btnBuka);
        var panelDana = new Panel { Dock = DockStyle.Top, Height = 190 };
        panelDana.Controls.Add(gridDana);
        panelDana.Controls.Add(panelBuka);

        //  tengah: riwayat 
        var panelRiwayat = new Panel { Dock = DockStyle.Fill };
        panelRiwayat.Controls.Add(gridRiwayat);
        panelRiwayat.Controls.Add(lblRiwayat);

        //  tengah bawah: aksi dan info 
        panelAksi.Controls.Add(new Label { Text = "Jumlah (Rp):", AutoSize = true, Margin = new Padding(3, 8, 3, 0) });
        panelAksi.Controls.Add(numJumlah);
        panelAksi.Controls.Add(new Label { Text = "Tanggal:", AutoSize = true, Margin = new Padding(15, 8, 3, 0) });
        panelAksi.Controls.Add(dtpTanggal);
        panelAksi.Controls.Add(btnSetor);
        panelAksi.Controls.Add(btnTarik);

        panelInfo.Controls.Add(lblSaldo);
        panelInfo.Controls.Add(lblImbalan);
        panelInfo.Controls.Add(lblPesan);

        // Urutan Add penting: Fill dahulu, lalu tepi (yang terakhir ditempel paling luar)
        var tengah = new Panel { Dock = DockStyle.Fill };
        tengah.Controls.Add(panelRiwayat);
        tengah.Controls.Add(panelInfo);
        tengah.Controls.Add(panelAksi);
        tengah.Controls.Add(panelDana);
        tengah.Controls.Add(lblAnggota);

        Controls.Add(tengah);
        Controls.Add(kiri);
        if (!_bolehKelola)
        {
            Controls.Add(new Label
            {
                Dock = DockStyle.Top,
                Height = 32,
                Text = $"Mode lihat saja: peran {pengguna.GetPeran()} tidak dapat mencatat transaksi dana.",
                BackColor = Color.LightYellow,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            });
        }

        //  peristiwa 
        gridAnggota.SelectionChanged += (_, _) =>
        {
            if (_mengisi) return;
            lblPesan.Text = string.Empty;
            PilihAnggotaDariBaris();
        };
        gridDana.SelectionChanged += (_, _) =>
        {
            if (_mengisi) return;
            lblPesan.Text = string.Empty;
            PilihDanaDariBaris();
        };
        txtCari.TextChanged += (_, _) => MuatAnggota(_terpilih?.IdAnggota);
        chkKeluar.CheckedChanged += (_, _) => MuatAnggota(_terpilih?.IdAnggota);
        btnBuka.Click += (_, _) => BukaRekening();
        btnSetor.Click += (_, _) => Catat(JenisTransaksi.Setor);
        btnTarik.Click += (_, _) => Catat(JenisTransaksi.Tarik);
        Load += (_, _) => MuatAnggota(null);
    }

    private void MuatAnggota(int? pilihId)
    {
        try
        {
            _anggota = _kontrolAnggota.Cari(txtCari.Text, chkKeluar.Checked);
        }
        catch (Exception ex)
        {
            UiBantu.Pesan(lblPesan, "Gagal memuat anggota: " + ex.Message, true);
            return;
        }

        _mengisi = true;
        gridAnggota.DataSource = _anggota.Select(a => new
        {
            a.IdAnggota,
            a.Nama,
            Status = a.Status.ToString()
        }).ToList();
        UiBantu.Judul(gridAnggota, ("IdAnggota", "ID"));
        if (gridAnggota.Columns["IdAnggota"] is { } kolomId) kolomId.FillWeight = 25;

        gridAnggota.CurrentCell = null;
        gridAnggota.ClearSelection();
        if (pilihId is int id)
        {
            foreach (DataGridViewRow baris in gridAnggota.Rows)
            {
                if (Convert.ToInt32(baris.Cells["IdAnggota"].Value) == id)
                {
                    baris.Selected = true;
                    break;
                }
            }
        }
        _mengisi = false;

        PilihAnggotaDariBaris();
    }

    private void PilihAnggotaDariBaris()
    {
        if (gridAnggota.SelectedRows.Count == 0)
        {
            _terpilih = null;
        }
        else
        {
            int id = Convert.ToInt32(gridAnggota.SelectedRows[0].Cells["IdAnggota"].Value);
            _terpilih = _anggota.FirstOrDefault(a => a.IdAnggota == id);
        }
        MuatDana(null);
    }

    private void MuatDana(int? pilihIdDana)
    {
        _mengisi = true;

        if (_terpilih is null)
        {
            _dana = new List<DanaAnggota>();
            gridDana.DataSource = null;
            _mengisi = false;
            PilihDanaDariBaris();
            return;
        }

        try
        {
            _parameter = _kontrolDana.MuatParameter();
            _dana = _kontrolDana.DaftarDana(_terpilih);
        }
        catch (Exception ex)
        {
            _mengisi = false;
            UiBantu.Pesan(lblPesan, "Gagal memuat dana: " + ex.Message, true);
            return;
        }

        // Polymorphism: daftar bertipe DanaAnggota, isinya campuran pokok, wajib, tabungan
        gridDana.DataSource = _dana.Select(d => new
        {
            d.IdDana,
            Jenis = d.NamaJenis,
            Saldo = Rp(d.Saldo)
        }).ToList();
        UiBantu.Judul(gridDana, ("IdDana", "ID"));
        if (gridDana.Columns["IdDana"] is { } kolomId) kolomId.FillWeight = 25;

        gridDana.CurrentCell = null;
        gridDana.ClearSelection();

        DataGridViewRow? pilih = null;
        foreach (DataGridViewRow baris in gridDana.Rows)
        {
            if (pilihIdDana is int id && Convert.ToInt32(baris.Cells["IdDana"].Value) == id)
            {
                pilih = baris;
                break;
            }
        }
        if (pilih is null && gridDana.Rows.Count > 0) pilih = gridDana.Rows[0];
        if (pilih is not null) pilih.Selected = true;

        _mengisi = false;
        PilihDanaDariBaris();
    }

    private void PilihDanaDariBaris()
    {
        if (gridDana.SelectedRows.Count == 0 || _terpilih is null)
        {
            _danaTerpilih = null;
            gridRiwayat.DataSource = null;
            lblSaldo.Text = _terpilih is null
                ? string.Empty
                : "Anggota ini belum memiliki rekening dana. Buka rekening di bawah tabel.";
            lblImbalan.Text = string.Empty;
            PerbaruiKontrol();
            return;
        }

        int id = Convert.ToInt32(gridDana.SelectedRows[0].Cells["IdDana"].Value);
        _danaTerpilih = _dana.First(d => d.IdDana == id);

        try
        {
            var riwayat = _kontrolDana.Riwayat(id);
            gridRiwayat.DataSource = riwayat.Select(t => new
            {
                Tanggal = t.Tanggal.ToString("dd-MM-yyyy"),
                Jenis = t.Jenis.ToString(),
                Jumlah = Rp(t.Jumlah),
                SaldoSetelah = Rp(t.SaldoSetelah)
            }).ToList();
            UiBantu.Judul(gridRiwayat, ("SaldoSetelah", "Saldo Setelah"));
        }
        catch (Exception ex)
        {
            UiBantu.Pesan(lblPesan, "Gagal memuat riwayat: " + ex.Message, true);
        }

        lblSaldo.Text = $"Saldo {_danaTerpilih.NamaJenis}: {Rp(_danaTerpilih.Saldo)}";

        // Polymorphism: HitungImbalan() dipanggil lewat tipe DanaAnggota, hasilnya beda per jenis
        if (_danaTerpilih is Tabungan && _parameter is not null)
        {
            lblImbalan.Text =
                $"Perkiraan imbalan sebulan: {Rp(_danaTerpilih.HitungImbalan(_parameter))} " +
                $"(bunga tabungan {_parameter.BungaTabunganBerlaku:0.##}% per tahun)";
        }
        else
        {
            lblImbalan.Text = "Simpanan ini tidak berbunga (hanya simpanan berbentuk tabungan yang berimbalan).";
        }

        PerbaruiKontrol();
    }

    private void PerbaruiKontrol()
    {
        lblAnggota.Text = _terpilih is null
            ? "Pilih anggota di sebelah kiri."
            : $"{_terpilih.Nama}  |  {_terpilih.Status}";

        panelBuka.Enabled = _bolehKelola && _terpilih is { Aktif: true };
        panelAksi.Enabled = _bolehKelola && _danaTerpilih is not null;
    }

    private void BukaRekening()
    {
        if (_terpilih is null) return;
        var jenis = JenisDana[cmbJenis.SelectedIndex];

        try
        {
            int idBaru = _kontrolDana.BukaDana(_terpilih.IdAnggota, jenis.Kode);
            MuatDana(idBaru);
            UiBantu.Pesan(lblPesan, $"Rekening {jenis.Nama} dibuka.", false);
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

    private void Catat(JenisTransaksi jenis)
    {
        if (_danaTerpilih is null) return;
        int idDana = _danaTerpilih.IdDana;

        try
        {
            _kontrolDana.Catat(idDana, jenis, numJumlah.Value,
                                 DateOnly.FromDateTime(dtpTanggal.Value));
            MuatDana(idDana);
            numJumlah.Value = 0;
            UiBantu.Pesan(lblPesan,
                jenis == JenisTransaksi.Setor ? "Setoran tercatat." : "Penarikan tercatat.", false);
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