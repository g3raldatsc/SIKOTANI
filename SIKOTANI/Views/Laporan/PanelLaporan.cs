using System.Data;

namespace SIKOTANI.Views;

/// <summary>Panel tabel laporan yang dipakai ulang oleh semua laporan.</summary>
public class PanelLaporan : UserControl
{
    private readonly DataGridView grid = UiBantu.BuatGrid();
    private readonly Label lblJudul = new()
    {
        AutoSize = false,
        Dock = DockStyle.Fill,
        Font = Tema.HurufSubjudul,
        TextAlign = ContentAlignment.MiddleLeft
    };
    private readonly Button btnEkspor = new() { Text = "Ekspor ke CSV", Dock = DockStyle.Right, Width = 160, Enabled = false };
    private readonly Label lblRingkas = new()
    {
        Dock = DockStyle.Bottom,
        Height = 40,
        Font = Tema.HurufTebal,
        ForeColor = Tema.Teks,
        TextAlign = ContentAlignment.MiddleLeft
    };

    private TabelLaporan? _tabel;

    public PanelLaporan()
    {
        Dock = DockStyle.Fill;

        var atas = new Panel { Dock = DockStyle.Top, Height = 46, Padding = new Padding(0, 4, 0, 6) };
        atas.Controls.Add(lblJudul);    // Fill lebih dulu
        atas.Controls.Add(btnEkspor);

        Controls.Add(grid);   // Fill lebih dulu, lalu panel tepi
        Controls.Add(lblRingkas);
        Controls.Add(atas);

        btnEkspor.Click += (_, _) => Ekspor();
    }

    public void Tampilkan(TabelLaporan tabel)
    {
        _tabel = tabel;
        lblJudul.Text = tabel.Judul;
        lblRingkas.Text = tabel.Ringkasan;

        var data = new DataTable();
        foreach (var kolom in tabel.Kolom) data.Columns.Add(kolom, typeof(string));
        foreach (var baris in tabel.Baris)
            data.Rows.Add(baris.Select((nilai, i) => (object)tabel.Tampil(nilai, i)).ToArray());

        grid.DataSource = data;
        for (int i = 0; i < grid.Columns.Count; i++)
        {
            grid.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;   // nilai berupa teks
            if (tabel.KolomUang[i])
                grid.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }
        btnEkspor.Enabled = tabel.Baris.Count > 0;
    }

    private void Ekspor()
    {
        if (_tabel is null) return;

        using var dialog = new SaveFileDialog
        {
            Filter = "CSV (*.csv)|*.csv",
            FileName = $"{_tabel.Judul.Replace(' ', '-')}-{DateTime.Today:yyyyMMdd}.csv"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            File.WriteAllText(dialog.FileName, _tabel.KeCsv(), new System.Text.UTF8Encoding(true));
            MessageBox.Show("Laporan diekspor ke:\n" + dialog.FileName, "SIKOTANI",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Gagal mengekspor: " + ex.Message, "SIKOTANI",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}