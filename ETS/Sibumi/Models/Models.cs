namespace Sibumi.Models;

public class User
{
    public int UserId { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string Nama { get; set; }
    public string Role { get; set; } // "Admin", "Petugas", "User"
    public string NoHp { get; set; }
    public string NIK { get; set; }
    public bool IsActive { get; set; }
}

public class Gunung
{
    public int GunungId { get; set; }
    public string NamaGunung { get; set; }
    public string Lokasi { get; set; }
    public int Ketinggian { get; set; }
    public bool BisaDidaki { get; set; } = true;
    public string KeteranganStatus { get; set; }

    public string StatusBadge => BisaDidaki ? "Bisa Didaki" : "Ditutup";
    public string Display => BisaDidaki 
        ? $"{NamaGunung} ({Ketinggian} mdpl)" 
        : $"{NamaGunung} ({Ketinggian} mdpl) • Ditutup";

    public override string ToString() => Display;
}

public class Jalur
{
    public int JalurId { get; set; }
    public int GunungId { get; set; }
    public string NamaGunung { get; set; }
    public string NamaJalur { get; set; }
    public int KuotaHarian { get; set; }
    public decimal TarifPerOrang { get; set; }
    public bool IsActive { get; set; }
    public bool GunungBisaDidaki { get; set; } = true;
    public string Display => $"{NamaGunung} - {NamaJalur}";

    public override string ToString() => NamaJalur;
}

public class Booking
{
    public int BookingId { get; set; }
    public string KodeBooking { get; set; }
    public int JalurId { get; set; }
    public int GunungId { get; set; }
    public string NamaGunung { get; set; }
    public string NamaJalur { get; set; }
    public string NamaKetua { get; set; }
    public string NIK { get; set; }
    public string NoHp { get; set; }
    public string Email { get; set; }
    public int JumlahAnggota { get; set; }
    public DateTime TanggalNaik { get; set; }
    public DateTime TanggalTurun { get; set; }
    public decimal TotalBiaya { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? CreatedBy { get; set; }

    public Booking Clone() => (Booking)MemberwiseClone();
}

public class ChartItem
{
    public string Label { get; set; }
    public int Value { get; set; }
    public int Max { get; set; }
}

public class ActivityItem
{
    public string Pengguna { get; set; }
    public string JenisAksi { get; set; }
    public string Deskripsi { get; set; }
    public string WaktuText { get; set; }
}

public class DashboardData
{
    public int TotalBooking { get; set; }
    public int PendakiAktif { get; set; }
    public int KuotaTerpakai { get; set; }
    public int KuotaTotal { get; set; }
    public decimal Pendapatan { get; set; }
    public List<ChartItem> Chart { get; set; } = new();
    public List<ActivityItem> Aktivitas { get; set; } = new();
}