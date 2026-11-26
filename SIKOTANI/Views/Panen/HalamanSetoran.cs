namespace SIKOTANI.Views;

public class HalamanSetoran : UserControl
{
    private sealed record PilihanPinjaman(Pinjaman Data, string Teks)
    {
        public override string ToString() => Teks;
    }

    private sealed record PilihanAngsuran(int No, decimal Sisa, string Teks)
    {
        public override string ToString() => Teks;
    }

    private readonly SetoranController _kontrolSetoran = new();
    private readonly AnggotaController _kontrolAnggota = new();
    private readonly KomoditasController _kontrolKomoditas = new();
    private readonly PinjamanController _kontrolPinjaman = new();

    private readonly DataGridView gridSetoran = UiBantu.BuatGrid();
    private readonly DataGridView gridRiwayat = UiBantu.BuatGrid();

    // ----- Tab 1: setoran baru -----
    private readonly ComboBox cmbAnggota = new() { Width = 340, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox cmbKomoditas = new() { Width = 340, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DateTimePicker dtpSetoran = new() { Width = 340, Format = DateTimePickerFormat.Short };
    private readonly NumericUpDown numBerat = new() { Width = 340, DecimalPlaces = 2, Maximum = 10_000_000m, ThousandsSeparator = true, Increment = 10m };
    private readonly NumericUpDown numHarga = new() { Width = 340, Maximum = 9_999_999_999m, ThousandsSeparator = true, Increment = 100m };
    private readonly Label lblNilai = new() { AutoSize = true, Font = Tema.HurufTebal, Margin = new Padding(3, 12, 3, 0) };
    private readonly Button btnSimpan = new() { Text = "Simpan Setoran", Dock = DockStyle.Fill };

    // ----- Tab 2: potong angsuran -----
    private readonly Label lblInfoSetoran = new() { AutoSize = true, MaximumSize = new Size(340, 0), Font = Tema.Huruf, Margin = new Padding(3, 4, 3, 4) };
    private readonly ComboBox cmbPinjaman = new() { Width = 340, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox cmbAngsuran = new() { Width = 340, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Label lblSisa = new() { AutoSize = true, MaximumSize = new Size(340, 0), Font = Tema.HurufTebal, Margin = new Padding(3, 8, 3, 0) };
    private readonly NumericUpDown numPotong = new() { Width = 340, Maximum = 9_999_999_999m, ThousandsSeparator = true, Increment = 10000m };
    private readonly Button btnMaks = new() { Text = "Isi Jumlah Maksimum", Width = 340, Height = 30, Margin = new Padding(3, 8, 3, 3) };
    private readonly DateTimePicker dtpPotong = new() { Width = 340, Format = DateTimePickerFormat.Short };
    private readonly CheckBox chkSimulasi = new()
    {
        Text = "Simulasikan kegagalan di tengah transaksi (khusus uji T10)",
        AutoSize = false,
        Width = 340,
        Height = 44,
        Margin = new Padding(3, 14, 3, 0)
    };
    private readonly Button btnPotong = new() { Text = "Potong Angsuran", Dock = DockStyle.Fill };

    // Pesan di panel tetap di dasar (tidak ikut tergulung)
    private readonly Label lblPesan = new() { AutoSize = false, Dock = DockStyle.Fill, Margin = Padding.Empty };

    private readonly bool _bolehPotong;
    private List<SetoranPanen> _setoran = new();
    private List<Anggota> _anggotaAktif = new();
    private List<Komoditas> _komoditas = new();
    private List<Pinjaman> _pinjamanAktif = new();
    private SetoranPanen? _setoranTerpilih;
    private bool _mengisi;

    public HalamanSetoran()
    {
        Dock = DockStyle.Fill;

        var pengguna = Sesi.PenggunaAktif
            ?? throw new InvalidOperationException("Belum ada sesi masuk.");
        bool bolehSetoran = pengguna.BolehKelola(MenuAplikasi.SetoranPanen);
        _bolehPotong = bolehSetoran && pengguna.BolehKelola(MenuAplikasi.PembayaranAngsuran);

        dtpSetoran.MaxDate = DateTime.Today;
        dtpSetoran.Value = DateTime.Today;
        dtpPotong.MaxDate = DateTime.Today;
        dtpPotong.Value = DateTime.Today;
        #if !DEBUG
        chkSimulasi.Visible = false;   // alat uji T10: hanya tampil saat dijalankan dari Visual Studio (Debug)
        #endif
        PerbaruiNilai();

        var (tabSetoran, isiSetoran, bawahSetoran) = BuatTab("Setoran Baru");
        var (tabPotong, isiPotong, bawahPotong) = BuatTab("Potong Angsuran");

        void Tambah(FlowLayoutPanel tujuan, string teks, Control kontrol)
        {
            tujuan.Controls.Add(new Label { Text = teks, AutoSize = true, Margin = new Padding(3, 10, 3, 2) });
            tujuan.Controls.Add(kontrol);
        }

        // ---------- tab setoran baru ----------
        Tambah(isiSetoran, "Anggota (hanya anggota Aktif)", cmbAnggota);
        Tambah(isiSetoran, "Komoditas", cmbKomoditas);
        Tambah(isiSetoran, "Tanggal setoran", dtpSetoran);
        Tambah(isiSetoran, "Berat (kg)", numBerat);
        Tambah(isiSetoran, "Harga per kg (Rp)", numHarga);
        isiSetoran.Controls.Add(lblNilai);
        bawahSetoran.Controls.Add(btnSimpan);

        // ---------- tab potong angsuran ----------
        isiPotong.Controls.Add(lblInfoSetoran);
        Tambah(isiPotong, "Pinjaman aktif anggota", cmbPinjaman);
        Tambah(isiPotong, "Angsuran yang dibayar", cmbAngsuran);
        isiPotong.Controls.Add(lblSisa);
        Tambah(isiPotong, "Jumlah dipotong (Rp)", numPotong);
        isiPotong.Controls.Add(btnMaks);
        Tambah(isiPotong, "Tanggal pemotongan", dtpPotong);
        isiPotong.Controls.Add(chkSimulasi);
        bawahPotong.Controls.Add(btnPotong);

        tabSetoran.Enabled = bolehSetoran;   // Ketua: hanya melihat
        tabPotong.Enabled = _bolehPotong;

        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(tabSetoran);
        tabs.TabPages.Add(tabPotong);

        lblPesan.Padding = new Padding(0, 4, 0, 0);
        var panelPesan = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 80,
            BackColor = Tema.Kartu,
            Padding = new Padding(14, 8, 14, 8)
        };
        panelPesan.Controls.Add(lblPesan);

        var kananLuar = new Panel { Dock = DockStyle.Right, Width = 400, BackColor = Tema.Kartu };
        kananLuar.Controls.Add(tabs);         // Fill lebih dulu
        kananLuar.Controls.Add(panelPesan);   // lalu panel tepi bawah

        // ---------- panel kiri ----------
        var atas = new Panel { Dock = DockStyle.Top, Height = 300 };
        atas.Controls.Add(gridSetoran);
        atas.Controls.Add(new Label
        {
            Dock = DockStyle.Top,
            Height = 30,
            Text = "Setoran panen (Unit Hasil Panen)",
            Font = Tema.HurufSubjudul,
            TextAlign = ContentAlignment.MiddleLeft
        });

        var bawah = new Panel { Dock = DockStyle.Fill };
        bawah.Controls.Add(gridRiwayat);
        bawah.Controls.Add(new Label
        {
            Dock = DockStyle.Top,
            Height = 34,
            Text = "Riwayat pemotongan untuk setoran terpilih",
            Font = Tema.HurufSubjudul,
            TextAlign = ContentAlignment.BottomLeft
        });

        var kiri = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 0, 12, 0) };
        kiri.Controls.Add(bawah);    // Fill lebih dulu
        kiri.Controls.Add(atas);

        Controls.Add(kiri);
        Controls.Add(kananLuar);

        // ---------- peristiwa ----------
        gridSetoran.SelectionChanged += (_, _) =>
        {
            if (_mengisi) return;
            lblPesan.Text = string.Empty;
            PilihDariBaris();
        };
        numBerat.ValueChanged += (_, _) => PerbaruiNilai();
        numHarga.ValueChanged += (_, _) => PerbaruiNilai();
        cmbPinjaman.SelectedIndexChanged += (_, _) => IsiPilihanAngsuran();
        cmbAngsuran.SelectedIndexChanged += (_, _) => PerbaruiKontrolPotong();
        btnSimpan.Click += (_, _) => Simpan();
        btnMaks.Click += (_, _) => IsiMaksimum();
        btnPotong.Click += (_, _) => Potong();
        Load += (_, _) => MuatSemua(null);
    }

    private static (TabPage Halaman, FlowLayoutPanel Isi, Panel Bawah) BuatTab(string judul)
    {
        var halaman = new TabPage(judul) { Tag = "kartu" };
        var isi = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(12)
        };
        var bawah = new Panel { Dock = DockStyle.Bottom, Height = 64, Padding = new Padding(12, 10, 12, 8) };
        halaman.Controls.Add(isi);      // Fill lebih dulu
        halaman.Controls.Add(bawah);
        return (halaman, isi, bawah);
    }

    private void PerbaruiNilai()
    {
        decimal nilai = Math.Round(numBerat.Value * numHarga.Value, 0, MidpointRounding.AwayFromZero);
        lblNilai.Text = "Nilai setoran: " + Tema.Rp(nilai);
    }

    // ================= Memuat data =================
    private void MuatSemua(int? pilihId)
    {
        int? idAnggota = (cmbAnggota.SelectedItem as Anggota)?.IdAnggota;
        int? idKomoditas = (cmbKomoditas.SelectedItem as Komoditas)?.IdKomoditas;

        try
        {
            _setoran = _kontrolSetoran.Daftar();
            _anggotaAktif = _kontrolAnggota.Cari(string.Empty, false);
            _komoditas = _kontrolKomoditas.DaftarSemua();
            _pinjamanAktif = _kontrolPinjaman.Daftar(StatusPinjaman.Aktif);
        }
        catch (Exception ex)
        {
            UiBantu.Pesan(lblPesan, "Gagal memuat data: " + ex.Message, true);
            return;
        }

        _mengisi = true;

        cmbAnggota.DataSource = _anggotaAktif;
        cmbAnggota.DisplayMember = nameof(Anggota.Nama);
        if (idAnggota is int ia)
        {
            int i = _anggotaAktif.FindIndex(a => a.IdAnggota == ia);
            if (i >= 0) cmbAnggota.SelectedIndex = i;
        }

        cmbKomoditas.DataSource = _komoditas;
        cmbKomoditas.DisplayMember = nameof(Komoditas.Nama);
        if (idKomoditas is int ik)
        {
            int i = _komoditas.FindIndex(k => k.IdKomoditas == ik);
            if (i >= 0) cmbKomoditas.SelectedIndex = i;
        }

        gridSetoran.DataSource = _setoran.Select(s => new
        {
            s.IdSetoran,
            Tanggal = s.Tanggal.ToString("dd-MM-yyyy"),
            Anggota = s.Anggota.Nama,
            Komoditas = s.Komoditas.Nama,
            Berat = s.BeratKg.ToString("N2"),
            Harga = Tema.Rp(s.HargaPerKg),
            Nilai = Tema.Rp(s.HitungNilai()),
            Terpakai = Tema.Rp(s.Terpakai),
            Tersedia = Tema.Rp(s.HitungNilaiTersedia())
        }).ToList();
        UiBantu.Judul(gridSetoran, ("IdSetoran", "ID"), ("Berat", "Berat (kg)"), ("Harga", "Harga/kg"));
        if (gridSetoran.Columns["IdSetoran"] is { } kolomId) kolomId.FillWeight = 20;

        gridSetoran.CurrentCell = null;
        gridSetoran.ClearSelection();
        if (pilihId is int id)
        {
            foreach (DataGridViewRow baris in gridSetoran.Rows)
            {
                if (Convert.ToInt32(baris.Cells["IdSetoran"].Value) == id)
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
        if (gridSetoran.SelectedRows.Count == 0)
        {
            _setoranTerpilih = null;
        }
        else
        {
            int id = Convert.ToInt32(gridSetoran.SelectedRows[0].Cells["IdSetoran"].Value);
            _setoranTerpilih = _setoran.FirstOrDefault(x => x.IdSetoran == id);
        }
        TampilkanRiwayat();
        IsiPilihanPinjaman();
    }

    private void TampilkanRiwayat()
    {
        if (_setoranTerpilih is null)
        {
            gridRiwayat.DataSource = null;
            return;
        }

        try
        {
            var riwayat = _kontrolSetoran.Riwayat(_setoranTerpilih.IdSetoran);
            gridRiwayat.DataSource = riwayat.Select(x => new
            {
                Tanggal = x.Tanggal.ToString("dd-MM-yyyy"),
                Jumlah = Tema.Rp(x.Jumlah),
                Pinjaman = "#" + x.IdPinjaman,
                Angsuran = "ke-" + x.NoAngsuran
            }).ToList();
        }
        catch (Exception ex)
        {
            UiBantu.Pesan(lblPesan, "Gagal memuat riwayat: " + ex.Message, true);
        }
    }

    // ================= Tab potong angsuran =================
    private void IsiPilihanPinjaman()
    {
        cmbPinjaman.Items.Clear();
        cmbAngsuran.Items.Clear();

        if (_setoranTerpilih is null)
        {
            lblInfoSetoran.Text = "Pilih satu setoran di tabel untuk memotong nilainya.";
            PerbaruiKontrolPotong();
            return;
        }

        var s = _setoranTerpilih;
        lblInfoSetoran.Text =
            $"Setoran #{s.IdSetoran} | {s.Anggota.Nama}\n" +
            $"{s.Komoditas.Nama}, {s.BeratKg:N2} kg | nilai {Tema.Rp(s.HitungNilai())}\n" +
            $"Tersedia untuk dipotong: {Tema.Rp(s.HitungNilaiTersedia())}";

        foreach (var p in _pinjamanAktif.Where(p => p.Anggota.IdAnggota == s.Anggota.IdAnggota))
        {
            cmbPinjaman.Items.Add(new PilihanPinjaman(p,
                $"Pinjaman #{p.IdPinjaman} | {p.NamaJenis} | sisa pokok {Tema.Rp(p.GetSisaPokok())}"));
        }

        if (cmbPinjaman.Items.Count > 0) cmbPinjaman.SelectedIndex = 0;   // memicu pengisian angsuran
        else PerbaruiKontrolPotong();
    }

    private void IsiPilihanAngsuran()
    {
        cmbAngsuran.Items.Clear();

        if (cmbPinjaman.SelectedItem is PilihanPinjaman pil)
        {
            foreach (var a in pil.Data.JadwalAngsuran.Where(a => a.Status != StatusAngsuran.Lunas))
            {
                cmbAngsuran.Items.Add(new PilihanAngsuran(a.NoAngsuran, a.GetSisaTagihan(),
                    $"Angsuran ke-{a.NoAngsuran} | jatuh tempo {a.JatuhTempo:dd-MM-yyyy} | sisa {Tema.Rp(a.GetSisaTagihan())}"));
            }
            if (cmbAngsuran.Items.Count > 0) cmbAngsuran.SelectedIndex = 0;
        }
        PerbaruiKontrolPotong();
    }

    private void PerbaruiKontrolPotong()
    {
        bool adaPinjaman = cmbPinjaman.SelectedItem is PilihanPinjaman;
        bool adaAngsuran = cmbAngsuran.SelectedItem is PilihanAngsuran;
        bool adaNilai = _setoranTerpilih is not null && _setoranTerpilih.HitungNilaiTersedia() > 0;

        if (adaAngsuran)
        {
            var ang = (PilihanAngsuran)cmbAngsuran.SelectedItem!;
            lblSisa.Text = $"Sisa tagihan angsuran ke-{ang.No}: {Tema.Rp(ang.Sisa)}";
        }
        else if (_setoranTerpilih is null) lblSisa.Text = string.Empty;
        else if (!adaNilai) lblSisa.Text = "Nilai setoran ini sudah habis dipotong.";
        else if (!adaPinjaman) lblSisa.Text = "Anggota ini tidak memiliki pinjaman aktif.";
        else lblSisa.Text = "Semua angsuran pinjaman ini sudah lunas.";

        bool siap = _bolehPotong && adaNilai && adaAngsuran;
        btnPotong.Enabled = siap;
        btnMaks.Enabled = siap;
    }

    private void IsiMaksimum()
    {
        if (_setoranTerpilih is null || cmbAngsuran.SelectedItem is not PilihanAngsuran ang) return;
        decimal maks = Math.Min(_setoranTerpilih.HitungNilaiTersedia(), ang.Sisa);
        numPotong.Value = Math.Min(maks, numPotong.Maximum);
    }

    // ================= Aksi =================
    private void Simpan()
    {
        if (cmbAnggota.SelectedItem is not Anggota anggota ||
            cmbKomoditas.SelectedItem is not Komoditas komoditas)
        {
            UiBantu.Pesan(lblPesan, "Pilih anggota dan komoditas terlebih dahulu. " +
                "Komoditas ditambahkan di menu Keanggotaan, tab Komoditas.", true);
            return;
        }

        decimal nilai = Math.Round(numBerat.Value * numHarga.Value, 0, MidpointRounding.AwayFromZero);
        try
        {
            int id = _kontrolSetoran.Catat(anggota.IdAnggota, komoditas.IdKomoditas,
                DateOnly.FromDateTime(dtpSetoran.Value), numBerat.Value, numHarga.Value);

            numBerat.Value = 0m;
            MuatSemua(id);
            UiBantu.Pesan(lblPesan,
                $"Setoran #{id} tercatat di Unit Hasil Panen, nilai {Tema.Rp(nilai)}.", false);
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

    private void Potong()
    {
        if (_setoranTerpilih is null ||
            cmbPinjaman.SelectedItem is not PilihanPinjaman pil ||
            cmbAngsuran.SelectedItem is not PilihanAngsuran ang)
        {
            UiBantu.Pesan(lblPesan, "Pilih setoran, pinjaman, dan angsuran terlebih dahulu.", true);
            return;
        }

        int idSetoran = _setoranTerpilih.IdSetoran;
        int idPinjaman = pil.Data.IdPinjaman;
        decimal jumlah = numPotong.Value;
        bool simulasi = chkSimulasi.Checked;

        try
        {
            _kontrolSetoran.PotongAngsuran(idSetoran, idPinjaman, ang.No, jumlah,
                DateOnly.FromDateTime(dtpPotong.Value), simulasi);

            numPotong.Value = 0m;
            MuatSemua(idSetoran);

            bool lunas = _pinjamanAktif.All(p => p.IdPinjaman != idPinjaman);
            UiBantu.Pesan(lblPesan,
                $"Potongan {Tema.Rp(jumlah)} dicatat sebagai pembayaran angsuran ke-{ang.No} " +
                $"pinjaman #{idPinjaman}." + (lunas ? " Pinjaman sudah lunas." : string.Empty), false);
        }
        catch (AturanBisnisException ex)
        {
            UiBantu.Pesan(lblPesan, ex.Message, true);
        }
        catch (Exception ex)
        {
            // Muat ulang: bila transaksi dibatalkan, angka di layar harus sama seperti sebelumnya
            MuatSemua(idSetoran);
            UiBantu.Pesan(lblPesan, "Gagal: " + ex.Message +
                " Semua perubahan dibatalkan (rollback).", true);
        }
    }
}