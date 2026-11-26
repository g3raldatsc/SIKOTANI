namespace SIKOTANI.Views;

public class HalamanPengajuan : UserControl
{
    private readonly PinjamanController _kontrolPinjaman = new();
    private readonly AnggotaController _kontrolAnggota = new();

    private readonly DataGridView gridPinjaman = UiBantu.BuatGrid();
    private readonly DataGridView gridJadwal = UiBantu.BuatGrid();
    private readonly Label lblDetail = new()
    {
        Dock = DockStyle.Top,
        Height = 110,
        Font = Tema.Huruf,
        ForeColor = Tema.Teks,
        Padding = new Padding(2, 6, 0, 0)
    };

    private readonly FlowLayoutPanel kanan = new()
    {
        Dock = DockStyle.Fill,
        BackColor = Tema.Kartu,
        FlowDirection = FlowDirection.TopDown,
        WrapContents = false,
        AutoScroll = true,
        Padding = new Padding(14)
    };
    private readonly ComboBox cmbAnggota = new() { Width = 360, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox cmbJenis = new() { Width = 360, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NumericUpDown numTenor = new() { Width = 360, Minimum = 1, Maximum = 60, Value = 3 };
    private readonly DateTimePicker dtpTanggal = new() { Width = 360, Format = DateTimePickerFormat.Short };
    private readonly Label lblBunga = new()
    {
        AutoSize = true,
        MaximumSize = new Size(360, 0),
        ForeColor = Tema.TeksPudar,
        Margin = new Padding(3, 8, 3, 0)
    };

    // Pinjaman tunai
    private readonly FlowLayoutPanel panelTunai = new() { FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true };
    private readonly NumericUpDown numJumlah = new() { Width = 360, Maximum = 9_999_999_999m, ThousandsSeparator = true, Increment = 100000m };

    // Pinjaman saprodi
    private readonly FlowLayoutPanel panelSaprodi = new() { FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, Visible = false };
    private readonly TextBox txtNamaItem = new() { Width = 360, MaxLength = 100 };
    private readonly TextBox txtJenisItem = new() { Width = 360, MaxLength = 30, PlaceholderText = "Pupuk, Benih, Pestisida" };
    private readonly NumericUpDown numJumlahItem = new() { Width = 360, DecimalPlaces = 2, Maximum = 1_000_000m, Value = 1m };
    private readonly TextBox txtSatuan = new() { Width = 360, MaxLength = 20, PlaceholderText = "karung, kg, liter" };
    private readonly NumericUpDown numHarga = new() { Width = 360, Maximum = 9_999_999_999m, ThousandsSeparator = true, Increment = 1000m };
    private readonly Button btnTambahItem = new() { Text = "Tambah Item", Width = 360, Height = 32, Margin = new Padding(3, 12, 3, 3) };
    private readonly ListBox lstItem = new() { Width = 360, Height = 100 };
    private readonly Button btnHapusItem = new() { Text = "Hapus Item Terpilih", Width = 360, Height = 30 };
    private readonly Label lblTotal = new() { AutoSize = true, Font = Tema.HurufTebal, Margin = new Padding(3, 8, 3, 0) };

    // Tombol Ajukan dan pesan berada di panel tetap di dasar (tidak ikut tergulung)
    private readonly Button btnAjukan = new() { Text = "Ajukan Pinjaman", Dock = DockStyle.Top, Height = 44 };
    private readonly Label lblPesan = new() { AutoSize = false, Dock = DockStyle.Fill, Margin = Padding.Empty };

    private List<Anggota> _anggota = new();
    private List<Pinjaman> _pinjaman = new();
    private readonly List<ItemSaprodi> _itemBaru = new();
    private bool _mengisi;

    public HalamanPengajuan()
    {
        Dock = DockStyle.Fill;

        var pengguna = Sesi.PenggunaAktif
            ?? throw new InvalidOperationException("Belum ada sesi masuk.");
        bool bolehKelola = pengguna.BolehKelola(MenuAplikasi.PengajuanPinjaman);

        dtpTanggal.MaxDate = DateTime.Today;   // tanggal pengajuan tidak boleh di masa depan
        dtpTanggal.Value = DateTime.Today;
        cmbJenis.Items.AddRange(new object[] { "Pinjaman Tunai", "Pinjaman Saprodi" });
        cmbJenis.SelectedIndex = 0;

        void Tambah(string teks, Control kontrol, FlowLayoutPanel? ke = null)
        {
            var tujuan = ke ?? kanan;
            tujuan.Controls.Add(new Label { Text = teks, AutoSize = true, Margin = new Padding(3, 10, 3, 2) });
            tujuan.Controls.Add(kontrol);
        }

        // ---------- panel kanan: formulir pengajuan ----------
        kanan.Controls.Add(new Label { Text = "Ajukan pinjaman baru", Font = Tema.HurufSubjudul, AutoSize = true });
        Tambah("Anggota (hanya anggota Aktif)", cmbAnggota);
        Tambah("Jenis pinjaman", cmbJenis);
        Tambah("Tenor (bulan)", numTenor);
        Tambah("Tanggal pengajuan", dtpTanggal);
        kanan.Controls.Add(lblBunga);

        Tambah("Jumlah pinjaman (Rp)", numJumlah, panelTunai);
        kanan.Controls.Add(panelTunai);

        Tambah("Nama item", txtNamaItem, panelSaprodi);
        Tambah("Jenis item", txtJenisItem, panelSaprodi);
        Tambah("Jumlah", numJumlahItem, panelSaprodi);
        Tambah("Satuan", txtSatuan, panelSaprodi);
        Tambah("Harga satuan (Rp)", numHarga, panelSaprodi);
        panelSaprodi.Controls.Add(btnTambahItem);
        panelSaprodi.Controls.Add(new Label { Text = "Daftar item", AutoSize = true, Margin = new Padding(3, 10, 3, 2) });
        panelSaprodi.Controls.Add(lstItem);
        panelSaprodi.Controls.Add(btnHapusItem);
        panelSaprodi.Controls.Add(lblTotal);
        kanan.Controls.Add(panelSaprodi);

        kanan.Enabled = bolehKelola;   // Ketua: hanya melihat

        // Tombol Ajukan dan pesan ditaruh di panel tetap di dasar: selalu terlihat
        // dan tidak ikut tergulung bersama formulir yang panjang
        btnAjukan.Enabled = bolehKelola;
        lblPesan.Padding = new Padding(0, 8, 0, 0);

        var panelAksi = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 130,
            BackColor = Tema.Kartu,
            Padding = new Padding(14, 10, 14, 8)
        };
        panelAksi.Controls.Add(lblPesan);      // Fill lebih dulu
        panelAksi.Controls.Add(btnAjukan);     // lalu tombol di atas pesan

        var kananLuar = new Panel { Dock = DockStyle.Right, Width = 400, BackColor = Tema.Kartu };
        kananLuar.Controls.Add(kanan);         // Fill lebih dulu
        kananLuar.Controls.Add(panelAksi);     // lalu panel tepi bawah

        // ---------- panel kiri: daftar dan jadwal ----------
        var panelAtas = new Panel { Dock = DockStyle.Top, Height = 270 };
        panelAtas.Controls.Add(gridPinjaman);
        panelAtas.Controls.Add(new Label
        {
            Dock = DockStyle.Top,
            Height = 30,
            Text = "Daftar pinjaman",
            Font = Tema.HurufSubjudul,
            TextAlign = ContentAlignment.MiddleLeft
        });

        var panelBawah = new Panel { Dock = DockStyle.Fill };
        panelBawah.Controls.Add(gridJadwal);
        panelBawah.Controls.Add(lblDetail);
        panelBawah.Controls.Add(new Label
        {
            Dock = DockStyle.Top,
            Height = 34,
            Text = "Rincian dan jadwal angsuran",
            Font = Tema.HurufSubjudul,
            TextAlign = ContentAlignment.BottomLeft
        });

        var kiri = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 0, 12, 0) };
        kiri.Controls.Add(panelBawah);
        kiri.Controls.Add(panelAtas);

        Controls.Add(kiri);    // Fill lebih dulu, lalu panel tepi
        Controls.Add(kananLuar);

        // ---------- peristiwa ----------
        cmbJenis.SelectedIndexChanged += (_, _) =>
        {
            bool saprodi = cmbJenis.SelectedIndex == 1;
            panelTunai.Visible = !saprodi;
            panelSaprodi.Visible = saprodi;
        };
        gridPinjaman.SelectionChanged += (_, _) => TampilkanDetail();
        btnTambahItem.Click += (_, _) => TambahItem();
        btnHapusItem.Click += (_, _) => HapusItem();
        btnAjukan.Click += (_, _) => Ajukan();
        Load += (_, _) => MuatSemua(null);
    }

    private void MuatSemua(int? pilihId)
    {
        try
        {
            var parameter = _kontrolPinjaman.MuatParameter();
            lblBunga.Text = $"Suku bunga berlaku: {parameter.BungaPinjamanBerlaku:0.##}% per tahun " +
                            "(dari parameter koperasi), dihitung dengan metode efektif.";
            _anggota = _kontrolAnggota.Cari(string.Empty, false);
            _pinjaman = _kontrolPinjaman.Daftar();
        }
        catch (Exception ex)
        {
            UiBantu.Pesan(lblPesan, "Gagal memuat data: " + ex.Message, true);
            return;
        }

        _mengisi = true;
        cmbAnggota.DataSource = _anggota;
        cmbAnggota.DisplayMember = nameof(Anggota.Nama);

        gridPinjaman.DataSource = _pinjaman.Select(x => new
        {
            x.IdPinjaman,
            Anggota = x.Anggota.Nama,
            Jenis = x.NamaJenis,
            Pokok = Tema.Rp(x.HitungPokokPinjaman()),
            Tenor = x.TenorBulan + " bln",
            Bunga = x.SukuBungaTahunan.ToString("0.##") + "%",
            Status = x.Status.ToString(),
            Pengajuan = x.TanggalPengajuan.ToString("dd-MM-yyyy")
        }).ToList();
        UiBantu.Judul(gridPinjaman, ("IdPinjaman", "ID"), ("Pengajuan", "Tgl Pengajuan"));
        if (gridPinjaman.Columns["IdPinjaman"] is { } kolomId) kolomId.FillWeight = 20;

        gridPinjaman.CurrentCell = null;
        gridPinjaman.ClearSelection();
        if (pilihId is int id)
        {
            foreach (DataGridViewRow baris in gridPinjaman.Rows)
            {
                if (Convert.ToInt32(baris.Cells["IdPinjaman"].Value) == id)
                {
                    baris.Selected = true;
                    break;
                }
            }
        }
        _mengisi = false;
        TampilkanDetail();
    }

    private void TampilkanDetail()
    {
        if (_mengisi) return;

        if (gridPinjaman.SelectedRows.Count == 0)
        {
            lblDetail.Text = "Pilih pinjaman di tabel untuk melihat rincian dan jadwalnya.";
            gridJadwal.DataSource = null;
            return;
        }

        int id = Convert.ToInt32(gridPinjaman.SelectedRows[0].Cells["IdPinjaman"].Value);
        var pinjaman = _pinjaman.First(x => x.IdPinjaman == id);

        // Polymorphism: Rincian() berbeda per jenis pinjaman
        string tanggalCair = pinjaman.TanggalCair?.ToString("dd-MM-yyyy") ?? "-";
        lblDetail.Text =
            $"Pinjaman #{pinjaman.IdPinjaman} | {pinjaman.Anggota.Nama} | Status: {pinjaman.Status} | Tanggal cair: {tanggalCair}\n" +
            pinjaman.Rincian();

        if (pinjaman.JadwalAngsuran.Count == 0)
        {
            gridJadwal.DataSource = null;
            lblDetail.Text += pinjaman.Status == StatusPinjaman.Diajukan
                ? "\nBelum ada jadwal angsuran: menunggu persetujuan Ketua."
                : "\nTidak ada jadwal angsuran.";
            return;
        }

        gridJadwal.DataSource = pinjaman.JadwalAngsuran.Select(a => new
        {
            No = a.NoAngsuran,
            JatuhTempo = a.JatuhTempo.ToString("dd-MM-yyyy"),
            Pokok = Tema.Rp(a.Pokok),
            Bunga = Tema.Rp(a.Bunga),
            Tagihan = Tema.Rp(a.HitungTagihan()),
            Status = a.Status.ToString()
        }).ToList();
        UiBantu.Judul(gridJadwal, ("JatuhTempo", "Jatuh Tempo"));
        if (gridJadwal.Columns["No"] is { } kolomNo) kolomNo.FillWeight = 20;
    }

    // ---------- item saprodi ----------
    private void TambahItem()
    {
        try
        {
            // Validasi dilakukan class domain ItemSaprodi
            var item = new ItemSaprodi(txtNamaItem.Text, txtJenisItem.Text,
                                       numJumlahItem.Value, txtSatuan.Text, numHarga.Value);
            _itemBaru.Add(item);
            PerbaruiDaftarItem();

            txtNamaItem.Clear();
            txtJenisItem.Clear();
            txtSatuan.Clear();
            numJumlahItem.Value = 1m;
            numHarga.Value = 0m;
            txtNamaItem.Focus();
            lblPesan.Text = string.Empty;
        }
        catch (AturanBisnisException ex)
        {
            UiBantu.Pesan(lblPesan, ex.Message, true);
        }
    }

    private void HapusItem()
    {
        int indeks = lstItem.SelectedIndex;
        if (indeks < 0)
        {
            UiBantu.Pesan(lblPesan, "Pilih dulu item di daftar.", true);
            return;
        }
        _itemBaru.RemoveAt(indeks);
        PerbaruiDaftarItem();
    }

    private void PerbaruiDaftarItem()
    {
        lstItem.Items.Clear();
        foreach (var i in _itemBaru)
            lstItem.Items.Add($"{i.NamaItem}: {i.Jumlah:0.##} {i.Satuan} x {Tema.Rp(i.HargaSatuan)} = {Tema.Rp(i.HitungSubtotal())}");
        lblTotal.Text = $"Total pokok: {Tema.Rp(_itemBaru.Sum(i => i.HitungSubtotal()))}";
    }

    // ---------- pengajuan ----------
    private void Ajukan()
    {
        if (cmbAnggota.SelectedItem is not Anggota anggota)
        {
            UiBantu.Pesan(lblPesan, "Pilih anggota terlebih dahulu.", true);
            return;
        }

        var tanggal = DateOnly.FromDateTime(dtpTanggal.Value);
        int tenor = (int)numTenor.Value;

        try
        {
            int id = cmbJenis.SelectedIndex == 0
                ? _kontrolPinjaman.AjukanTunai(anggota.IdAnggota, numJumlah.Value, tenor, tanggal)
                : _kontrolPinjaman.AjukanSaprodi(anggota.IdAnggota, _itemBaru.ToList(), tenor, tanggal);

            numJumlah.Value = 0m;
            _itemBaru.Clear();
            PerbaruiDaftarItem();

            MuatSemua(id);
            UiBantu.Pesan(lblPesan, $"Pinjaman #{id} diajukan. Menunggu persetujuan Ketua.", false);
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
}