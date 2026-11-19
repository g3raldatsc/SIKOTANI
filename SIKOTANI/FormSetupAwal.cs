using SIKOTANI.Data;
using SIKOTANI.Models;

namespace SIKOTANI;

public partial class FormSetupAwal : Form
{
    private readonly TextBox txtNama = new() { Width = 280 };
    private readonly TextBox txtUsername = new() { Width = 280 };
    private readonly TextBox txtPassword = new() { Width = 280, UseSystemPasswordChar = true };
    private readonly TextBox txtKonfirmasi = new() { Width = 280, UseSystemPasswordChar = true };
    private readonly Button btnBuat = new() { Text = "Buat Akun", Width = 280, Height = 34, Margin = new Padding(3, 14, 3, 3) };
    private readonly Label lblPesan = new() { ForeColor = Color.Red, AutoSize = false, Width = 280, Height = 40 };

    public FormSetupAwal()
    {
        InitializeComponent();

        Text = "SIKOTANI - Buat Akun Ketua Pertama";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(340, 390);

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(25, 10, 25, 10)
        };

        void Tambah(string teks, Control kontrol)
        {
            panel.Controls.Add(new Label { Text = teks, AutoSize = true, Margin = new Padding(3, 10, 3, 0) });
            panel.Controls.Add(kontrol);
        }

        Tambah("Nama lengkap Ketua", txtNama);
        Tambah("Username (min. 3 karakter)", txtUsername);
        Tambah("Kata sandi (min. 8 karakter)", txtPassword);
        Tambah("Ulangi kata sandi", txtKonfirmasi);
        panel.Controls.Add(btnBuat);
        panel.Controls.Add(lblPesan);
        Controls.Add(panel);

        AcceptButton = btnBuat;
        btnBuat.Click += btnBuat_Click;
    }

    private void btnBuat_Click(object? sender, EventArgs e)
    {
        lblPesan.Text = string.Empty;

        if (txtPassword.Text != txtKonfirmasi.Text)
        {
            lblPesan.Text = "Konfirmasi kata sandi tidak sama.";
            return;
        }

        try
        {
            PenggunaRepository.BuatKetuaAwal(txtNama.Text, txtUsername.Text, txtPassword.Text);
            MessageBox.Show("Akun Ketua berhasil dibuat. Silakan masuk.",
                "SIKOTANI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
        }
        catch (AturanBisnisException ex)
        {
            lblPesan.Text = ex.Message;
        }
        catch (Exception ex)
        {
            lblPesan.Text = "Gagal menyimpan: " + ex.Message;
        }
    }
}