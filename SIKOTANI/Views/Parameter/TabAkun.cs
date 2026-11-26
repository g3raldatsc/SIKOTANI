namespace SIKOTANI.Views;

public class TabAkun : UserControl
{
    private readonly AkunController _kontrol = new();

    private readonly DataGridView grid = UiBantu.BuatGrid();

    private readonly FlowLayoutPanel kanan = new()
    {
        Dock = DockStyle.Fill,
        BackColor = Tema.Kartu,
        FlowDirection = FlowDirection.TopDown,
        WrapContents = false,
        AutoScroll = true,
        Padding = new Padding(14)
    };

    // BUAT AKUN BARU
    private readonly ComboBox cmbAnggota = new() { Width = 360, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox cmbPeran = new() { Width = 360, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox txtUsername = new() { Width = 360, MaxLength = 50 };
    private readonly TextBox txtSandi = new() { Width = 360, UseSystemPasswordChar = true };
    private readonly TextBox txtKonfirmasi = new() { Width = 360, UseSystemPasswordChar = true };
    private readonly Button btnBuat = new() { Text = "Buat Akun", Width = 360, Height = 38, Margin = new Padding(3, 12, 3, 3) };

    // AKUN TERPILIH HEHEE
    private readonly Label lblTerpilih = new() { AutoSize = true, MaximumSize = new Size(360, 0), Font = Tema.Huruf, Margin = new Padding(3, 4, 3, 4) };
    private readonly Button btnNonaktif = new() { Text = "Nonaktifkan Akun", Width = 360, Height = 34, Enabled = false };
    private readonly Button btnAktif = new() { Text = "Aktifkan Akun", Width = 360, Height = 34, Enabled = false, Margin = new Padding(3, 6, 3, 3) };
    private readonly TextBox txtSandiBaru = new() { Width = 360, UseSystemPasswordChar = true };
    private readonly TextBox txtKonfirmasiBaru = new() { Width = 360, UseSystemPasswordChar = true };
    private readonly Button btnReset = new() { Text = "Atur Ulang Kata Sandi", Width = 360, Height = 34, Enabled = false, Margin = new Padding(3, 8, 3, 3) };

    // Pesan di panel tetap di dasar
    private readonly Label lblPesan = new() { AutoSize = false, Dock = DockStyle.Fill, Margin = Padding.Empty };

    private readonly bool _bolehKelola;
    private List<Pengguna> _akun = new();
    private Pengguna? _terpilih;
    private bool _mengisi;

    public TabAkun(bool bolehKelola)
    {
        Dock = DockStyle.Fill;
        _bolehKelola = bolehKelola;

        cmbPeran.Items.AddRange(new object[] { "Ketua", "Sekretaris", "Bendahara" });
        cmbPeran.SelectedIndex = 1;

        void Tambah(string teks, Control kontrol)
        {
            kanan.Controls.Add(new Label { Text = teks, AutoSize = true, Margin = new Padding(3, 10, 3, 2) });
            kanan.Controls.Add(kontrol);
        }

        // panel kanan
        kanan.Controls.Add(new Label { Text = "Buat akun pengurus baru", Font = Tema.HurufSubjudul, AutoSize = true });
        Tambah("Anggota (aktif, belum memiliki akun)", cmbAnggota);
        Tambah("Peran", cmbPeran);
        Tambah("Username (min. 3 karakter, tanpa spasi)", txtUsername);
        Tambah("Kata sandi (min. 8 karakter)", txtSandi);
        Tambah("Ulangi kata sandi", txtKonfirmasi);
        kanan.Controls.Add(btnBuat);

        kanan.Controls.Add(new Label
        {
            Text = "Akun terpilih",
            Font = Tema.HurufSubjudul,
            AutoSize = true,
            Margin = new Padding(3, 28, 3, 0)
        });
        kanan.Controls.Add(lblTerpilih);
        kanan.Controls.Add(btnNonaktif);
        kanan.Controls.Add(btnAktif);
        Tambah("Kata sandi baru", txtSandiBaru);
        Tambah("Ulangi kata sandi baru", txtKonfirmasiBaru);
        kanan.Controls.Add(btnReset);
        kanan.Enabled = _bolehKelola;

        var panelPesan = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 80,
            BackColor = Tema.Kartu,
            Padding = new Padding(14, 8, 14, 8)
        };
        panelPesan.Controls.Add(lblPesan);

        var kananLuar = new Panel { Dock = DockStyle.Right, Width = 400, BackColor = Tema.Kartu };
        kananLuar.Controls.Add(kanan);  // Fill lebih dulu
        kananLuar.Controls.Add(panelPesan);   // lalu panel tepi bawah

        var kiri = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 0, 12, 0) };
        kiri.Controls.Add(grid);

        Controls.Add(kiri);
        Controls.Add(kananLuar);

        // EVENT
        grid.SelectionChanged += (_, _) =>
        {
            if (_mengisi) return;
            lblPesan.Text = string.Empty;
            PilihDariBaris();
        };
        btnBuat.Click += (_, _) => Buat();
        btnNonaktif.Click += (_, _) => UbahStatus(false);
        btnAktif.Click += (_, _) => UbahStatus(true);
        btnReset.Click += (_, _) => AturUlang();
        Load += (_, _) => Muat(null);
    }

    private void Muat(int? pilihId)
    {
        List<Anggota> tanpaAkun;
        try
        {
            _akun = _kontrol.DaftarAkun();
            tanpaAkun = _kontrol.AnggotaTanpaAkun();
        }
        catch (Exception ex)
        {
            UiBantu.Pesan(lblPesan, "Gagal memuat data: " + ex.Message, true);
            return;
        }

        _mengisi = true;
        cmbAnggota.DataSource = tanpaAkun;
        cmbAnggota.DisplayMember = nameof(Anggota.Nama);

        grid.DataSource = _akun.Select(p => new
        {
            p.IdPengguna,
            p.Username,
            Nama = p.Anggota.Nama,
            Peran = p.GetPeran(),
            Status = p.Aktif ? "Aktif" : "Nonaktif",
            Keterangan = p.Anggota.Aktif ? string.Empty : "Anggota sudah keluar"
        }).ToList();
        UiBantu.Judul(grid, ("IdPengguna", "ID"));
        if (grid.Columns["IdPengguna"] is { } kolomId) kolomId.FillWeight = 20;

        grid.CurrentCell = null;
        grid.ClearSelection();
        if (pilihId is int id)
        {
            foreach (DataGridViewRow baris in grid.Rows)
            {
                if (Convert.ToInt32(baris.Cells["IdPengguna"].Value) == id)
                {
                    baris.Selected = true;
                    break;
                }
            }
        }
        _mengisi = false;
        PilihDariBaris();
    }

    private void PilihDariBaris()
    {
        if (grid.SelectedRows.Count == 0)
        {
            _terpilih = null;
            lblTerpilih.Text = "Pilih akun di tabel.";
        }
        else
        {
            int id = Convert.ToInt32(grid.SelectedRows[0].Cells["IdPengguna"].Value);
            _terpilih = _akun.FirstOrDefault(p => p.IdPengguna == id);
            if (_terpilih is not null)
            {
                lblTerpilih.Text = $"{_terpilih.Username} | {_terpilih.Anggota.Nama} | {_terpilih.GetPeran()} | " +
                                   (_terpilih.Aktif ? "Aktif" : "Nonaktif");
            }
        }

        btnNonaktif.Enabled = _bolehKelola && _terpilih is { Aktif: true };
        btnAktif.Enabled = _bolehKelola && _terpilih is { Aktif: false };
        btnReset.Enabled = _bolehKelola && _terpilih is not null;
    }

    private void Buat()
    {
        if (cmbAnggota.SelectedItem is not Anggota anggota)
        {
            UiBantu.Pesan(lblPesan, "Tidak ada anggota aktif tanpa akun. Daftarkan anggota di menu Keanggotaan.", true);
            return;
        }
        if (cmbPeran.SelectedItem is not string peran)
        {
            UiBantu.Pesan(lblPesan, "Pilih peran terlebih dahulu.", true);
            return;
        }
        if (txtSandi.Text != txtKonfirmasi.Text)
        {
            UiBantu.Pesan(lblPesan, "Konfirmasi kata sandi tidak sama.", true);
            return;
        }

        try
        {
            _kontrol.BuatAkun(anggota.IdAnggota, txtUsername.Text, txtSandi.Text, peran);
            string username = txtUsername.Text.Trim().ToLowerInvariant();
            txtUsername.Clear();
            txtSandi.Clear();
            txtKonfirmasi.Clear();
            Muat(null);
            UiBantu.Pesan(lblPesan, $"Akun {username} ({peran}) untuk {anggota.Nama} dibuat.", false);
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

    private void UbahStatus(bool aktif)
    {
        if (_terpilih is null) return;

        if (!aktif)
        {
            var jawab = MessageBox.Show(
                $"Nonaktifkan akun {_terpilih.Username}?\nPengguna tidak dapat masuk sampai akun diaktifkan lagi.",
                "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (jawab != DialogResult.Yes) return;
        }

        int id = _terpilih.IdPengguna;
        string username = _terpilih.Username;
        try
        {
            _kontrol.UbahStatus(id, aktif);
            Muat(id);
            UiBantu.Pesan(lblPesan, $"Akun {username} {(aktif ? "diaktifkan" : "dinonaktifkan")}.", false);
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

    private void AturUlang()
    {
        if (_terpilih is null) return;
        if (txtSandiBaru.Text != txtKonfirmasiBaru.Text)
        {
            UiBantu.Pesan(lblPesan, "Konfirmasi kata sandi baru tidak sama.", true);
            return;
        }

        try
        {
            _kontrol.AturUlangKataSandi(_terpilih.IdPengguna, txtSandiBaru.Text);
            txtSandiBaru.Clear();
            txtKonfirmasiBaru.Clear();
            UiBantu.Pesan(lblPesan, $"Kata sandi akun {_terpilih.Username} diatur ulang.", false);
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