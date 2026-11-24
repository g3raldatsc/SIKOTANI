using SIKOTANI.Models;

namespace SIKOTANI.Views;

public class TabKelompok : UserControl
{
    private readonly KelompokTaniController _kontrolKelompok = new();
    private readonly AnggotaController _kontrolAnggota = new();
    private readonly DataGridView grid = UiBantu.BuatGrid();
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
    private readonly TextBox txtDesa = new() { Width = 290 };
    private readonly Button btnBaru = new() { Text = "Baru", Width = 90, Height = 32 };
    private readonly Button btnSimpan = new() { Text = "Simpan Kelompok Baru", Width = 190, Height = 32 };
    private readonly Button btnHapus = new() { Text = "Hapus Kelompok", Width = 290, Height = 32, Enabled = false, Margin = new Padding(3, 10, 3, 3) };
    private readonly ListBox lstAnggota = new() { Width = 290, Height = 110 };
    private readonly Button btnKeluarkanAnggota = new() { Text = "Keluarkan dari Kelompok", Width = 290, Height = 32, Enabled = false };
    private readonly ComboBox cmbAnggota = new() { Width = 290, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Button btnTambahAnggota = new() { Text = "Tambah ke Kelompok", Width = 290, Height = 32, Enabled = false };
    private readonly Label lblPesan = new() { AutoSize = false, Width = 290, Height = 50 };

    private List<KelompokTani> _daftar = new();
    private int? _idTerpilih;
    private bool _mengisi;

    public TabKelompok(bool bolehKelola)
    {
        Dock = DockStyle.Fill;

        void Tambah(string teks, Control kontrol)
        {
            kanan.Controls.Add(new Label { Text = teks, AutoSize = true, Margin = new Padding(3, 8, 3, 0) });
            kanan.Controls.Add(kontrol);
        }

        Tambah("Nama kelompok tani", txtNama);
        Tambah("Desa", txtDesa);
        var baris = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0, 12, 0, 0) };
        baris.Controls.Add(btnBaru);
        baris.Controls.Add(btnSimpan);
        kanan.Controls.Add(baris);
        kanan.Controls.Add(btnHapus);
        Tambah("Anggota kelompok (satu anggota boleh di banyak kelompok)", lstAnggota);
        kanan.Controls.Add(btnKeluarkanAnggota);
        Tambah("Tambah anggota ke kelompok", cmbAnggota);
        kanan.Controls.Add(btnTambahAnggota);
        kanan.Controls.Add(lblPesan);
        kanan.Enabled = bolehKelola;

        Controls.Add(grid);
        Controls.Add(kanan);

        grid.SelectionChanged += (_, _) => IsiFormDariBaris();
        btnBaru.Click += (_, _) => BuatBaru();
        btnSimpan.Click += (_, _) => Simpan();
        btnHapus.Click += (_, _) => Hapus();
        btnTambahAnggota.Click += (_, _) => TambahAnggota();
        btnKeluarkanAnggota.Click += (_, _) => KeluarkanAnggota();
    }

    public void Segarkan() => Muat(_idTerpilih);

    private void Muat(int? pilihId)
    {
        try
        {
            _daftar = _kontrolKelompok.DaftarSemua();
        }
        catch (Exception ex)
        {
            UiBantu.Pesan(lblPesan, "Gagal memuat data: " + ex.Message, true);
            return;
        }

        _mengisi = true;
        grid.DataSource = _daftar.Select(k => new
        {
            k.IdKelompok,
            k.Nama,
            k.Desa,
            Jumlah = k.HitungJumlahAnggota()
        }).ToList();
        UiBantu.Judul(grid, ("IdKelompok", "ID"), ("Jumlah", "Jumlah Anggota"));
        if (grid.Columns["IdKelompok"] is { } kolomId) kolomId.FillWeight = 25;

        grid.CurrentCell = null;
        grid.ClearSelection();

        bool ketemu = false;
        if (pilihId is int id)
        {
            foreach (DataGridViewRow baris in grid.Rows)
            {
                if ((int)baris.Cells["IdKelompok"].Value == id)
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

        int id = (int)grid.SelectedRows[0].Cells["IdKelompok"].Value;
        var kelompok = _daftar.First(k => k.IdKelompok == id);
        _idTerpilih = id;

        txtNama.Text = kelompok.Nama;
        txtDesa.Text = kelompok.Desa;
        btnSimpan.Text = "Simpan Perubahan";
        btnHapus.Enabled = true;
        btnTambahAnggota.Enabled = true;
        btnKeluarkanAnggota.Enabled = true;

        lstAnggota.DataSource = kelompok.DaftarAnggota.ToList();
        lstAnggota.DisplayMember = nameof(Anggota.Nama);

        // Hanya anggota Aktif yang belum ada di kelompok ini
        var belum = _kontrolAnggota.Cari(string.Empty, false)
            .Where(a => kelompok.DaftarAnggota.All(x => x.IdAnggota != a.IdAnggota))
            .ToList();
        cmbAnggota.DataSource = belum;
        cmbAnggota.DisplayMember = nameof(Anggota.Nama);

        lblPesan.Text = string.Empty;
    }

    private void Kosongkan()
    {
        _idTerpilih = null;
        txtNama.Clear();
        txtDesa.Clear();
        btnSimpan.Text = "Simpan Kelompok Baru";
        btnHapus.Enabled = false;
        btnTambahAnggota.Enabled = false;
        btnKeluarkanAnggota.Enabled = false;
        lstAnggota.DataSource = null;
        cmbAnggota.DataSource = null;
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
                id = _kontrolKelompok.Tambah(txtNama.Text, txtDesa.Text);
            else
            {
                id = _idTerpilih.Value;
                _kontrolKelompok.Ubah(id, txtNama.Text, txtDesa.Text);
            }
            Muat(id);
            UiBantu.Pesan(lblPesan, "Kelompok tani tersimpan.", false);
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

    private void Hapus()
    {
        if (_idTerpilih is null) return;

        var jawab = MessageBox.Show(
            $"Hapus kelompok {txtNama.Text}?\nAnggotanya tetap tercatat sebagai anggota koperasi.",
            "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (jawab != DialogResult.Yes) return;

        try
        {
            string nama = txtNama.Text;
            _kontrolKelompok.Hapus(_idTerpilih.Value);
            Muat(null);
            UiBantu.Pesan(lblPesan, $"Kelompok {nama} dihapus. Anggotanya tetap ada.", false);
        }
        catch (Exception ex)
        {
            UiBantu.Pesan(lblPesan, "Gagal menghapus: " + ex.Message, true);
        }
    }

    private void TambahAnggota()
    {
        if (_idTerpilih is null) return;
        if (cmbAnggota.SelectedItem is not Anggota anggota)
        {
            UiBantu.Pesan(lblPesan, "Tidak ada anggota yang dapat ditambahkan.", true);
            return;
        }

        try
        {
            _kontrolKelompok.TambahAnggota(_idTerpilih.Value, anggota.IdAnggota);
            Muat(_idTerpilih);
            UiBantu.Pesan(lblPesan, $"{anggota.Nama} ditambahkan ke kelompok.", false);
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

    private void KeluarkanAnggota()
    {
        if (_idTerpilih is null) return;
        if (lstAnggota.SelectedItem is not Anggota anggota)
        {
            UiBantu.Pesan(lblPesan, "Pilih dulu anggota di daftar.", true);
            return;
        }

        try
        {
            _kontrolKelompok.KeluarkanAnggota(_idTerpilih.Value, anggota.IdAnggota);
            Muat(_idTerpilih);
            UiBantu.Pesan(lblPesan, $"{anggota.Nama} dikeluarkan dari kelompok (tetap anggota koperasi).", false);
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