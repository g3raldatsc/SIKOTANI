using SIKOTANI.Models;

namespace SIKOTANI.Views;

public class TabKomoditas : UserControl
{
    private readonly KomoditasController _kontrolKomoditas = new();
    private readonly DataGridView grid = UiBantu.BuatGrid();
    private readonly FlowLayoutPanel kanan = new()
    {
        Dock = DockStyle.Right,
        Width = 340,
        FlowDirection = FlowDirection.TopDown,
        WrapContents = false,
        Padding = new Padding(10)
    };
    private readonly TextBox txtNama = new() { Width = 290 };
    private readonly TextBox txtSatuan = new() { Width = 290, Text = "kg" };
    private readonly Button btnTambah = new() { Text = "Tambah Komoditas", Width = 290, Height = 32, Margin = new Padding(3, 12, 3, 3) };
    private readonly Button btnHapus = new() { Text = "Hapus Komoditas Terpilih", Width = 290, Height = 32, Enabled = false };
    private readonly Label lblPesan = new() { AutoSize = false, Width = 290, Height = 50 };

    private List<Komoditas> _daftar = new();
    private bool _mengisi;

    public TabKomoditas(bool bolehKelola)
    {
        Dock = DockStyle.Fill;

        kanan.Controls.Add(new Label { Text = "Nama komoditas", AutoSize = true, Margin = new Padding(3, 8, 3, 0) });
        kanan.Controls.Add(txtNama);
        kanan.Controls.Add(new Label { Text = "Satuan", AutoSize = true, Margin = new Padding(3, 8, 3, 0) });
        kanan.Controls.Add(txtSatuan);
        kanan.Controls.Add(btnTambah);
        kanan.Controls.Add(btnHapus);
        kanan.Controls.Add(lblPesan);
        kanan.Enabled = bolehKelola;

        Controls.Add(grid);
        Controls.Add(kanan);

        grid.SelectionChanged += (_, _) =>
        {
            if (!_mengisi) btnHapus.Enabled = grid.SelectedRows.Count > 0;
        };
        btnTambah.Click += (_, _) => Tambah();
        btnHapus.Click += (_, _) => Hapus();
    }

    public void Segarkan() => Muat();

    private void Muat()
    {
        try
        {
            _daftar = _kontrolKomoditas.DaftarSemua();
        }
        catch (Exception ex)
        {
            UiBantu.Pesan(lblPesan, "Gagal memuat data: " + ex.Message, true);
            return;
        }

        _mengisi = true;
        grid.DataSource = _daftar.Select(k => new { k.IdKomoditas, k.Nama, k.Satuan }).ToList();
        UiBantu.Judul(grid, ("IdKomoditas", "ID"));
        if (grid.Columns["IdKomoditas"] is { } kolomId) kolomId.FillWeight = 25;
        grid.CurrentCell = null;
        grid.ClearSelection();
        btnHapus.Enabled = false;
        _mengisi = false;
    }

    private void Tambah()
    {
        try
        {
            _kontrolKomoditas.Tambah(txtNama.Text, txtSatuan.Text);
            txtNama.Clear();
            txtSatuan.Text = "kg";
            Muat();
            UiBantu.Pesan(lblPesan, "Komoditas ditambahkan.", false);
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

    private void Hapus()
    {
        if (grid.SelectedRows.Count == 0) return;
        int id = Convert.ToInt32(grid.SelectedRows[0].Cells["IdKomoditas"].Value);
        string nama = (Convert.ToString(grid.SelectedRows[0].Cells["Nama"].Value) ?? string.Empty);

        var jawab = MessageBox.Show(
            $"Hapus komoditas {nama}?\nTanda komoditas itu pada data anggota ikut dilepas.",
            "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (jawab != DialogResult.Yes) return;

        try
        {
            _kontrolKomoditas.Hapus(id);
            Muat();
            UiBantu.Pesan(lblPesan, $"Komoditas {nama} dihapus.", false);
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