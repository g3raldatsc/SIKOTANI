namespace SIKOTANI.Models;

public enum StatusAnggota { Aktif, Keluar }
public enum JenisTransaksi { Setor, Tarik }
public enum StatusPinjaman { Diajukan, Aktif, Ditolak, Lunas }
public enum StatusAngsuran { Belum, Sebagian, Lunas }
public enum SumberPembayaran { Tunai, PotongPanen }
public enum HakAkses { Tidak, Lihat, Kelola }

public enum MenuAplikasi
{
    Keanggotaan,
    DanaAnggota,
    PengajuanPinjaman,
    PersetujuanPinjaman,
    PembayaranAngsuran,
    SetoranPanen,
    ParameterDanAkun,
    LaporanAnggota,
    LaporanKeuangan
}