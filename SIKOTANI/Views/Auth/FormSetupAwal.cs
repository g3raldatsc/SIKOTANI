namespace SIKOTANI.Views;

public partial class FormSetupAwal : Form
{
    private readonly AuthController _auth = new();

    private readonly TextBox txtNama = new() { Width = 340, Font = new Font("Segoe UI", 11f) };
    private readonly TextBox txtUsername = new() { Width = 340, Font = new Font("Segoe UI", 11f) };
    private readonly TextBox txtPassword = new() { Width = 340, Font = new Font("Segoe UI", 11f), UseSystemPasswordChar = true };
    private readonly TextBox txtKonfirmasi = new() { Width = 340, Font = new Font("Segoe UI", 11f), UseSystemPasswordChar = true };
    private readonly Button btnBuat = new() { Text = "Buat Akun", Width = 340, Height = 44, Margin = new Padding(3, 20, 3, 3) };
    private readonly Label lblPesan = new() { ForeColor = Tema.Bahaya, AutoSize = false, Width = 340, Height = 44 };

    public FormSetupAwal()
    {
        InitializeComponent();

        Text = "SIKOTANI - Buat Akun Ketua Pertama";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(420, 640);

        var kepala = new Panel { Dock = DockStyle.Top, Height = 120, BackColor = Tema.Sidebar };
        kepala.Controls.Add(new Label
        {
            Text = "Pengaturan Awal",
            ForeColor = Color.White,
            Font = new Font("Segoe UI Semibold", 20f),
            AutoSize = false,
            Left = 0,
            Top = 28,
            Width = 420,
            Height = 44,
            TextAlign = ContentAlignment.MiddleCenter
        });
        kepala.Controls.Add(new Label
        {
            Text = "Buat akun Ketua untuk pertama kali",
            ForeColor = Color.FromArgb(165, 214, 167),
            Font = new Font("Segoe UI", 10f),
            AutoSize = false,
            Left = 0,
            Top = 76,
            Width = 420,
            Height = 24,
            TextAlign = ContentAlignment.MiddleCenter
        });

        var badan = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(38, 14, 38, 10),
            BackColor = Color.White
        };

        void Tambah(string teks, Control kontrol)
        {
            badan.Controls.Add(new Label
            {
                Text = teks,
                AutoSize = true,
                ForeColor = Tema.TeksPudar,
                Margin = new Padding(3, 10, 3, 4)
            });
            badan.Controls.Add(kontrol);
        }

        Tambah("Nama lengkap Ketua", txtNama);
        Tambah("Username (min. 3 karakter)", txtUsername);
        Tambah("Kata sandi (min. 8 karakter)", txtPassword);
        Tambah("Ulangi kata sandi", txtKonfirmasi);
        badan.Controls.Add(btnBuat);
        badan.Controls.Add(lblPesan);

        Controls.Add(badan);
        Controls.Add(kepala);

        AcceptButton = btnBuat;
        btnBuat.Click += btnBuat_Click;
        Tema.Terapkan(this);
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
            _auth.BuatKetuaAwal(txtNama.Text, txtUsername.Text, txtPassword.Text);
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