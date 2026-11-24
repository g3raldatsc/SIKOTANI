namespace SIKOTANI.Views;

public partial class FormLogin : Form
{
    private readonly AuthController _auth = new();
    private readonly TextBox txtUsername = new() { Width = 250 };
    private readonly TextBox txtPassword = new() { Width = 250, UseSystemPasswordChar = true };
    private readonly Button btnMasuk = new() { Text = "Masuk", Width = 250, Height = 34, Margin = new Padding(3, 14, 3, 3) };
    private readonly Label lblPesan = new() { ForeColor = Color.Red, AutoSize = false, Width = 250, Height = 40 };
    private int _percobaanGagal;

    public FormLogin()
    {
        InitializeComponent();

        Text = "SIKOTANI - Masuk";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(310, 250);

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(25, 20, 25, 10)
        };

        panel.Controls.Add(new Label { Text = "Username", AutoSize = true });
        panel.Controls.Add(txtUsername);
        panel.Controls.Add(new Label { Text = "Kata sandi", AutoSize = true, Margin = new Padding(3, 12, 3, 0) });
        panel.Controls.Add(txtPassword);
        panel.Controls.Add(btnMasuk);
        panel.Controls.Add(lblPesan);
        Controls.Add(panel);

        AcceptButton = btnMasuk;
        btnMasuk.Click += btnMasuk_Click;
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