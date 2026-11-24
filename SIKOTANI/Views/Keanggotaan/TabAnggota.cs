using SIKOTANI.Models;

namespace SIKOTANI.Views;

public class TabAnggota : UserControl
{
    private readonly AnggotaController _kontrolAnggota = new();
    private readonly KomoditasController _kontrolKomoditas = new();
    private readonly DataGridView grid = UiBantu.BuatGrid();
    private readonly TextBox txtCari = new() { Width = 230, PlaceholderText = "Cari nama atau telepon" };
    private readonly CheckBox chkKeluar = new() { Text = "Tampilkan yang sudah keluar", AutoSize = true, Margin = new Padding(15, 5, 3, 3) };

    private readonly FlowLayoutPanel kanan = new()
    {
        Dock = DockStyle.Right,
        Width = 340,
        FlowDirection = FlowDirection.TopDown,
        WrapContents = false,
        AutoScroll = true,
        Padding = new Padding(10)
    };
    private readonly TextBox txtNama = new() { Width = 290 };
    private readonly TextBox txtAlamat = new() { Width = 290 };
    private readonly TextBox txtTelepon = new() { Width = 290 };
    private readonly DateTimePicker dtpGabung = new() { Width = 290, Format = DateTimePickerFormat.Short };
    private readonly NumericUpDown numLuas = new() { Width = 290, DecimalPlaces = 2, Maximum = 100000m, Increment = 0.1m };
    private readonly CheckedListBox clbKomoditas = new() { Width = 290, Height = 95, CheckOnClick = true };
    private readonly Button btnBaru = new() { Text = "Baru", Width = 90, Height = 32 };
    private readonly Button btnSimpan = new() { Text = "Simpan Anggota Baru", Width = 190, Height = 32 };
    private readonly Button btnKeluarkan = new() { Text = "Keluarkan dari Koperasi", Width = 290, Height = 32, Enabled = false, Margin = new Padding(3, 12, 3, 3) };
    private readonly Label lblPesan = new() { AutoSize = false, Width = 290, Height = 50 };

    private List<Anggota> _daftar = new();
    private List<Komoditas> _komoditas = new();
    private int? _idTerpilih;
    private bool _mengisi;

    public TabAnggota(bool bolehKelola)
    {
        Dock = DockStyle.Fill;

        void Tambah(string teks, Control kontrol)
        {
            kanan.Controls.Add(new Label { Text = teks, AutoSize = true, Margin = new Padding(3, 8, 3, 0) });
            kanan.Controls.Add(kontrol);
        }

        Tambah("Nama lengkap", txtNama);
        Tambah("Alamat", txtAlamat);
        Tambah("No. telepon", txtTelepon);
        Tambah("Tanggal gabung", dtpGabung);
        Tambah("Luas lahan (ha)", numLuas);
        Tambah("Komoditas yang ditanam", clbKomoditas);

        var baris = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0, 12, 0, 0) };
        baris.Controls.Add(btnBaru);
        baris.Controls.Add(btnSimpan);
        kanan.Controls.Add(baris);
        kanan.Controls.Add(btnKeluarkan);
        kanan.Controls.Add(lblPesan);
        kanan.Enabled = bolehKelola; // Ketua dan Bendahara: hanya melihat

        var atas = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 38, Padding = new Padding(5, 6, 0, 0) };
        atas.Controls.Add(new Label { Text = "Cari:", AutoSize = true, Margin = new Padding(3, 6, 3, 0) });
        atas.Controls.Add(txtCari);
        atas.Controls.Add(chkKeluar);

        // Urutan penting: Fill lebih dulu, lalu Right, lalu Top
        Controls.Add(grid);
        Controls.Add(kanan);
        Controls.Add(atas);

        grid.SelectionChanged += (_, _) => IsiFormDariBaris();
        txtCari.TextChanged += (_, _) => Muat(_idTerpilih);
        chkKeluar.CheckedChanged += (_, _) => Muat(_idTerpilih);
        btnBaru.Click += (_, _) => BuatBaru();
        btnSimpan.Click += (_, _) => Simpan();
        btnKeluarkan.Click += (_, _) => Keluarkan();
    }

    public void Segarkan() => Muat(_idTerpilih);

    private void Muat(int? pilihId)
    {
        try
        {
            _daftar = _kontrolAnggota.Cari(txtCari.Text, chkKeluar.Checked);
            _komoditas = _kontrolKomoditas.DaftarSemua();
        }
        catch (Exception ex)
        {
            UiBantu.Pesan(lblPesan, "Gagal memuat data: " + ex.Message, true);
            return;
        }

        _mengisi = true;
        clbKomoditas.Items.Clear();
        foreach (var k in _komoditas) clbKomoditas.Items.Add(k.Nama);

        grid.DataSource = _daftar.Select(a => new
        {
            a.IdAnggota,
            a.Nama,
            a.NoTelepon,
            a.Alamat,
            Gabung = a.TanggalGabung.ToString("dd-MM-yyyy"),
            LuasHa = a.LuasLahanHa.ToString("0.##"),
            Status = a.Status.ToString()
        }).ToList();

        UiBantu.Judul(grid, ("IdAnggota", "ID"), ("NoTelepon", "Telepon"),
                      ("Gabung", "Tgl Gabung"), ("LuasHa", "Luas (ha)"));
        if (grid.Columns["IdAnggota"] is { } kolomId) kolomId.FillWeight = 25;

        grid.CurrentCell = null;
        grid.ClearSelection();

        bool ketemu = false;
        if (pilihId is int id)
        {
            foreach (DataGridViewRow baris in grid.Rows)
            {
                if ((int)baris.Cells["IdAnggota"].Value == id)
                {
                    baris.Selected = true;
                    ketemu = true;
                    break;
                }
            }
        }
        _mengisi = false;

        if (ketemu) IsiFormDariBaris(); else Kosongkan();
    }

    private void IsiFormDariBaris()
    {
        if (_mengisi || grid.SelectedRows.Count == 0) return;

        int id = (int)grid.SelectedRows[0].Cells["IdAnggota"].Value;
        var a = _daftar.First(x => x.IdAnggota == id);
        _idTerpilih = id;

        txtNama.Text = a.Nama;
        txtAlamat.Text = a.Alamat;
        txtTelepon.Text = a.NoTelepon;
        dtpGabung.Value = a.TanggalGabung.ToDateTime(TimeOnly.MinValue);
        dtpGabung.Enabled = false;   // tanggal gabung tidak diubah
        numLuas.Value = a.LuasLahanHa;

        var terpilih = _kontrolKomoditas.IdKomoditasAnggota(id);
        for (int i = 0; i < _komoditas.Count; i++)
            clbKomoditas.SetItemChecked(i, terpilih.Contains(_komoditas[i].IdKomoditas));

        btnSimpan.Text = "Simpan Perubahan";
        btnKeluarkan.Enabled = a.Aktif;
        lblPesan.Text = string.Empty;
    }

    private void Kosongkan()
    {
        _idTerpilih = null;
        txtNama.Clear();
        txtAlamat.Clear();
        txtTelepon.Clear();
        dtpGabung.Value = DateTime.Today;
        dtpGabung.Enabled = true;
        numLuas.Value = 0;
        for (int i = 0; i < clbKomoditas.Items.Count; i++) clbKomoditas.SetItemChecked(i, false);
        btnSimpan.Text = "Simpan Anggota Baru";
        btnKeluarkan.Enabled = false;
        lblPesan.Text = string.Empty;
    }

    private void BuatBaru()
    {
        _mengisi = true;
        grid.CurrentCell = null;
        grid.ClearSelection();
        _mengisi = false;
        Kosongkan();
        txtNama.Focus();
    }

    private void Simpan()
    {
        try
        {
            int id;
            if (_idTerpilih is null)
            {
                id = _kontrolAnggota.Tambah(txtNama.Text, txtAlamat.Text, txtTelepon.Text,
                    DateOnly.FromDateTime(dtpGabung.Value), numLuas.Value);
            }
            else
            {
                id = _idTerpilih.Value;
                _kontrolAnggota.Ubah(id, txtNama.Text, txtAlamat.Text, txtTelepon.Text, numLuas.Value);
            }

            var dipilih = new List<int>();
            for (int i = 0; i < clbKomoditas.Items.Count; i++)
                if (clbKomoditas.GetItemChecked(i)) dipilih.Add(_komoditas[i].IdKomoditas);
            _kontrolKomoditas.SetKomoditasAnggota(id, dipilih);

            Muat(id);
            UiBantu.Pesan(lblPesan, "Data anggota tersimpan.", false);
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

    private void Keluarkan()
    {
        if (_idTerpilih is null) return;

        var jawab = MessageBox.Show(
            $"Keluarkan {txtNama.Text} dari koperasi?\nData simpanan dan pinjamannya tetap tersimpan sebagai riwayat.",
            "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (jawab != DialogResult.Yes) return;

        try
        {
            string nama = txtNama.Text;
            _kontrolAnggota.Keluarkan(_idTerpilih.Value);
            Muat(null);
            UiBantu.Pesan(lblPesan, $"{nama} telah dikeluarkan dari koperasi.", false);
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