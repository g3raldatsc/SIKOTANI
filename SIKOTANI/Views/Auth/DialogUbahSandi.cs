namespace SIKOTANI.Views;

/// <summary>Dialog agar setiap pengguna dapat mengubah kata sandinya sendiri.</summary>
public class DialogUbahSandi : Form
{
    private readonly AuthController _auth = new();

    private readonly TextBox txtLama = new() { Width = 320, UseSystemPasswordChar = true };
    private readonly TextBox txtBaru = new() { Width = 320, UseSystemPasswordChar = true };
    private readonly TextBox txtKonfirmasi = new() { Width = 320, UseSystemPasswordChar = true };
    private readonly Button btnSimpan = new() { Text = "Simpan Kata Sandi", Width = 320, Height = 40, Margin = new Padding(3, 18, 3, 3) };
    private readonly Label lblPesan = new() { AutoSize = false, Width = 320, Height = 48 };

    public DialogUbahSandi()
    {
        Text = "Ubah Kata Sandi";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(380, 410);

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(30, 20, 30, 10),
            BackColor = Color.White
        };

        void Tambah(string teks, Control kontrol)
        {
            panel.Controls.Add(new Label { Text = teks, AutoSize = true, Margin = new Padding(3, 10, 3, 2) });
            panel.Controls.Add(kontrol);
        }

        panel.Controls.Add(new Label { Text = "Ubah kata sandi Anda", Font = Tema.HurufSubjudul, AutoSize = true });
        Tambah("Kata sandi lama", txtLama);
        Tambah("Kata sandi baru (min. 8 karakter)", txtBaru);
        Tambah("Ulangi kata sandi baru", txtKonfirmasi);
        panel.Controls.Add(btnSimpan);
        panel.Controls.Add(lblPesan);
        Controls.Add(panel);

        AcceptButton = btnSimpan;
        btnSimpan.Click += (_, _) => Simpan();
        Tema.Terapkan(this);
    }

    private void Simpan()
    {
        lblPesan.Text = string.Empty;
        if (txtBaru.Text != txtKonfirmasi.Text)
        {
            UiBantu.Pesan(lblPesan, "Konfirmasi kata sandi baru tidak sama.", true);
            return;
        }

        try
        {
            _auth.UbahKataSandiSaya(txtLama.Text, txtBaru.Text);
            MessageBox.Show("Kata sandi berhasil diubah.", "SIKOTANI",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
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