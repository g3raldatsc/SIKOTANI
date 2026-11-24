using System.ComponentModel;

namespace SIKOTANI.Views;

/// <summary>Kontrol menu samping buatan sendiri: ikon, teks, efek sorot, dan penanda aktif.</summary>
public class ItemMenu : Control
{
    private bool _sorot;
    private bool _aktif;

    // properti ini diisi lewat kode, bukan disimpan oleh Designer
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Glyph { get; set; } = string.Empty;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool Aktif
    {
        get => _aktif;
        set { _aktif = value; Invalidate(); }
    }

    public ItemMenu()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Cursor = Cursors.Hand;
        Size = new Size(228, 44);
        Margin = new Padding(0, 0, 0, 4);
    }

    protected override void OnMouseEnter(EventArgs e) { _sorot = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _sorot = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnTextChanged(EventArgs e) { Invalidate(); base.OnTextChanged(e); }

    // Polymorphism: menimpa cara Control menggambar dirinya
    protected override void OnPaint(PaintEventArgs e)
    {
        Color latar = _aktif ? Tema.Primer : _sorot ? Tema.SidebarSorot : Tema.Sidebar;
        using (var kuas = new SolidBrush(latar))
            e.Graphics.FillRectangle(kuas, ClientRectangle);

        if (Tema.PunyaIkon && Glyph.Length > 0)
        {
            using var fontIkon = Tema.HurufIkon(13f);
            TextRenderer.DrawText(e.Graphics, Glyph, fontIkon,
                new Rectangle(10, 0, 34, Height), Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPadding);
        }

        TextRenderer.DrawText(e.Graphics, Text, Tema.Huruf,
            new Rectangle(52, 0, Width - 60, Height), Color.White,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }
}