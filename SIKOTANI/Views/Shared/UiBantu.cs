namespace SIKOTANI.Views;

internal static class UiBantu
{
    public static DataGridView BuatGrid() => new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        AllowUserToResizeRows = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        RowHeadersVisible = false,
        BackgroundColor = SystemColors.Window
    };

    public static void Judul(DataGridView grid, params (string Kolom, string Teks)[] judul)
    {
        foreach (var (kolom, teks) in judul)
            if (grid.Columns[kolom] is { } c) c.HeaderText = teks;
    }

    public static void Pesan(Label label, string teks, bool galat)
    {
        label.ForeColor = galat ? Tema.Bahaya : Tema.PrimerGelap;
        label.Text = teks;
    }
}