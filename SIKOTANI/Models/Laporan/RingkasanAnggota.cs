namespace SIKOTANI.Models;

public record RingkasanAnggota(int IdAnggota, string Nama, string NoTelepon, string Alamat, DateOnly TanggalGabung, decimal LuasLahanHa, string Status, string Kelompok, string Komoditas);