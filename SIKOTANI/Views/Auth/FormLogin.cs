namespace SIKOTANI.Views;

public partial class FormLogin : Form
{
    private readonly AuthController _auth = new();

    private readonly TextBox txtUsername = new() { Width = 340, Font = new Font("Segoe UI", 11f) };
    private readonly TextBox txtPassword = new() { Width = 340, Font = new Font("Segoe UI", 11f), UseSystemPasswordChar = true };
    private readonly Button btnMasuk = new() { Text = "Masuk", Width = 340, Height = 44, Margin = new Padding(3, 22, 3, 3) };
    private readonly Label lblPesan = new() { ForeColor = Tema.Bahaya, AutoSize = false, Width = 340, Height = 48 };
    private int _percobaanGagal;

    public FormLogin()
    {
        InitializeComponent();

        Text = "SIKOTANI - Masuk";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(420, 500);

        var kepala = new Panel { Dock = DockStyle.Top, Height = 150, BackColor = Tema.Sidebar };
        kepala.Controls.Add(new Label
        {
            Text = "SIKOTANI",
            ForeColor = Color.White,
            Font = new Font("Segoe UI Semibold", 26f),
            AutoSize = false,
            Left = 0,
            Top = 36,
            Width = 420,
            Height = 52,
            TextAlign = ContentAlignment.MiddleCenter
        });
        kepala.Controls.Add(new Label
        {
            Text = "Sistem Informasi Koperasi Tani",
            ForeColor = Color.FromArgb(165, 214, 167),
            Font = new Font("Segoe UI", 10f),
            AutoSize = false,
            Left = 0,
            Top = 92,
            Width = 420,
            Height = 24,
            TextAlign = ContentAlignment.MiddleCenter
        });

        var badan = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(38, 26, 38, 10),
            BackColor = Color.White
        };

        Label Etiket(string teks) => new()
        {
            Text = teks,
            AutoSize = true,
            ForeColor = Tema.TeksPudar,
            Margin = new Padding(3, 12, 3, 4)
        };

        badan.Controls.Add(Etiket("Username"));
        badan.Controls.Add(txtUsername);
        badan.Controls.Add(Etiket("Kata sandi"));
        badan.Controls.Add(txtPassword);
        badan.Controls.Add(btnMasuk);
        badan.Controls.Add(lblPesan);

        Controls.Add(badan);    // Fill lebih dulu, lalu panel tepi
        Controls.Add(kepala);

        AcceptButton = btnMasuk;
        btnMasuk.Click += btnMasuk_Click;
        Tema.Terapkan(this);
    }

    private void btnMasuk_Click(object? sender, EventArgs e)
    {
        lblPesan.Text = string.Empty;
        string username = txtUsername.Text.Trim();
        string kataSandi = txtPassword.Text;

        if (username.Length == 0 || kataSandi.Length == 0)
        {
            lblPesan.Text = "Username dan kata sandi wajib diisi.";
            return;
        }

        try
        {
            if (_auth.Masuk(username, kataSandi))
            {
                DialogResult = DialogResult.OK;   // menutup form login
                return;
            }

            _percobaanGagal++;
            txtPassword.Clear();

            if (_percobaanGagal >= 5)
            {
                MessageBox.Show("Terlalu banyak percobaan gagal. Aplikasi ditutup.",
                    "SIKOTANI", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.Cancel;
                return;
            }

            // Pesan sengaja sama untuk username salah, sandi salah, dan akun nonaktif
            lblPesan.Text = $"Username atau kata sandi salah. ({_percobaanGagal}/5)";
        }
        catch (Exception ex)
        {
            lblPesan.Text = "Tidak dapat terhubung ke database: " + ex.Message;
        }
    }
}