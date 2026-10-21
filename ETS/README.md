# Evaluasi Tengah Semester Pemrograman Berbasis Kerangka Kerja

| Nama | NRP | Kelas |
| ---- | ---- | ---- |
| Ahsin Khuluqil Karim | 5025241063 | Pemrograman Berbasis Kerangka Kerja D |

Topik: Evaluasi Tengah Semester

## Tautan Proyek

- **Link PRD (Product Requirements Document):**
  [`Sibumi/PRD_SIBUMI.md`](Sibumi/PRD_SIBUMI.md)
- **Link Repository GitHub:**
  [https://github.com/sinnxc/pbkk/tree/main/ETS](https://github.com/sinnxc/pbkk/tree/main/ETS)
- **Link Video Demo:**
  [Link Video Demo](https://youtu.be/bXBVLyUpcVo)

---

Pada ETS kali ini kami membuat aplikasi desktop bernama **SIBUMI (Sistem Informasi Booking dan Manajemen Pendakian)**. Aplikasi ini dipakai petugas basecamp untuk mencatat pemesanan izin pendakian (simaksi), mengontrol kuota harian tiap jalur, memantau status pendaki yang sedang di gunung, dan melihat rekap pendapatan tiket. Alurnya sederhana: rancang dulu database di SQL Server, lalu buat aplikasi WPF yang membaca dan menulis ke database tersebut.

## Bagian 1: Merancang Database di SQL Server

### 1. Alat yang Digunakan

- **SQL Server LocalDB** dengan instance `(localdb)\MSSQLLocalDB`, versi ringan SQL Server yang ikut terpasang bersama Visual Studio.
- **SQL Server Management Studio (SSMS)** untuk menjalankan skrip `Database.sql` dan memeriksa isi tabel.
- **Visual Studio 2022** untuk membuat dan menjalankan aplikasi WPF.
- **.NET 10.0** (`net10.0-windows`) dan paket NuGet **`Microsoft.Data.SqlClient` 5.2.2** sebagai driver ADO.NET.

### 2. Membuat Database dan Tabel

Seluruh struktur database ditulis dalam satu file, `Sibumi/Database.sql`. Skrip ini membuat database `SIBUMI_DB` lalu lima tabel:

- **`Users`**: akun pengelola (`Username` unik, `PasswordHash`, `Nama`, `Role`, `NoHp`, `NIK`, `IsActive`). Role dibatasi lewat `CHECK` ke `Admin`, `Petugas`, atau `User`.
- **`Gunung`**: data gunung (nama, lokasi, ketinggian). Ada dua kolom tambahan, `BisaDidaki` dan `KeteranganStatus`, supaya gunung yang sedang ditutup tetap tersimpan sebagai data dan hanya ditandai `BisaDidaki = 0` beserta alasan penutupannya.
- **`Jalur`**: jalur milik satu gunung, lengkap dengan kuota harian, tarif per orang, dan status aktif.
- **`Booking`**: transaksi pemesanan (kode booking, data ketua rombongan, jumlah anggota, tanggal naik dan turun, total biaya, status, siapa yang membuat).
- **`Aktivitas`**: log audit yang mencatat siapa melakukan apa dan kapan.

Relasi antar tabel:

- `Gunung` ke `Jalur`: **one-to-many**. Satu gunung punya banyak jalur.
- `Jalur` ke `Booking`: **one-to-many**. Satu jalur dipesan oleh banyak booking.
- `Users` ke `Booking` dan `Aktivitas`: **one-to-many**, untuk mencatat siapa yang membuat booking dan siapa yang melakukan aksi.

### 3. Constraint untuk Menjaga Kualitas Data

Database dibuat agar menolak data yang salah, bukan hanya mengandalkan validasi di aplikasi:

- `UNIQUE` pada `Users.Username`, `Gunung.NamaGunung`, `Booking.KodeBooking`, serta pasangan `(GunungId, NamaJalur)` lewat `UQ_Jalur`, supaya tidak ada nama jalur ganda dalam satu gunung.
- `CHECK (JumlahAnggota BETWEEN 1 AND 15)` untuk membatasi ukuran satu rombongan.
- `CHECK (TanggalTurun >= TanggalNaik)` supaya tanggal turun tidak mendahului tanggal naik.
- `CHECK (Status IN ('Menunggu', 'Dikonfirmasi', 'Mendaki', 'Selesai', 'Dibatalkan'))` untuk status booking.
- `CHECK` pada `Ketinggian`, `KuotaHarian`, dan `TarifPerOrang` supaya tidak bernilai nol atau negatif.
- Satu index `IX_Booking_Kuota` pada `(JalurId, TanggalNaik)` dengan `INCLUDE (JumlahAnggota, Status)`, karena query penghitungan kuota paling sering memakai kombinasi kolom tersebut.

### 4. Data Contoh

Skrip yang sama mengisi data awal: tujuh gunung (empat bisa didaki, tiga ditutup yaitu Semeru, Merapi, dan Raung), enam jalur, dan enam booking contoh dengan status yang berbeda-beda. Tanggal booking dibuat relatif terhadap hari ini memakai `DATEADD`, jadi data selalu terlihat masuk akal kapan pun skrip dijalankan.

Tabel `Users` sengaja tidak diisi lewat SQL. Akun awal dibuat oleh `AuthService.EnsureSeedAsync()` saat aplikasi pertama kali dijalankan, sehingga password langsung tersimpan dalam bentuk hash:

| Username | Password | Role |
| --- | --- | --- |
| `admin` | `Admin#123` | Admin |
| `petugas` | `Petugas#123` | Petugas |
| `user` | `User#123` | User |

## Bagian 2: Membuat Aplikasi WPF

### 1. Membuat Project

1. Buka Visual Studio, klik **Create a new project**.
2. Pilih template **WPF Application** (C#), bukan yang berlabel *.NET Framework*.
3. Beri nama project `Sibumi`, pilih framework **.NET 10.0**.
4. Tambahkan paket NuGet `Microsoft.Data.SqlClient` versi 5.2.2.

Connection string disimpan di `appsettings.json` dan disalin ke folder output saat build (`CopyToOutputDirectory`), jadi tidak ada kredensial yang ditulis langsung di kode:

```json
{
  "ConnectionStrings": {
    "Sibumi": "Server=(localdb)\\MSSQLLocalDB;Database=SIBUMI_DB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### 2. Struktur Project (MVVM)

Aplikasi memakai pola **MVVM** supaya tampilan, logika, dan akses data terpisah:

```
Sibumi/
├── App.xaml / App.xaml.cs      # Titik masuk, handler exception global, pemuatan tema
├── appsettings.json            # Connection string SQL Server
├── Database.sql                # DDL tabel, constraint, index, dan data contoh
├── Core/                       # Db, Session, PasswordHasher, ThemeService, Toast, ErrorHandler,
│                               # Logger, DialogService, BookingRules, Commands, ViewModelBase
├── Models/                     # User, Gunung, Jalur, Booking, DashboardData, ChartItem, ActivityItem
├── Data/                       # BookingRepository, JalurRepository, UserRepository,
│                               # DashboardRepository, ActivityLog
├── Services/                   # AuthService (login, registrasi, seed akun)
├── ViewModels/                 # Login, Main, Dashboard, Booking, BookingForm, Jalur, Settings
├── Views/                      # LoginWindow, MainWindow, DashboardView, BookingView,
│                               # BookingFormWindow, JalurView, SettingsView
└── Themes/                     # Dark.xaml, Light.xaml, Styles.xaml
```

### 3. Cara Aplikasi Mengakses Database

Repository memakai `SqlConnection`, `SqlCommand`, dan `SqlDataReader`, tetapi semuanya **asynchronous** (`OpenAsync`, `ExecuteReaderAsync`, `ExecuteNonQueryAsync`) supaya jendela tidak membeku saat data diambil. Beberapa hal yang diterapkan:

- **Query berparameter.** Semua query memakai parameter (`@k`, `@s`, `@j`, dan seterusnya) lewat method bantu `With(...)`, tidak ada penyambungan string, jadi aman dari SQL Injection.
- **Tipe decimal yang benar.** Method `With(...)` mengeset `Precision = 14` dan `Scale = 2` untuk parameter `Decimal`. Tanpa ini SqlClient membulatkan nilai desimal menjadi bilangan bulat, dan tarif tiket jadi salah.
- **Password di-hash.** Password disimpan dengan PBKDF2-SHA256 (100.000 iterasi, salt acak 16 byte) dalam format `iterasi.salt.hash`, dan pengecekannya memakai `CryptographicOperations.FixedTimeEquals`.
- **Transaksi dan penguncian.** Saat menyimpan booking, baris jalur dikunci dengan `UPDLOCK, ROWLOCK` di dalam transaksi supaya dua petugas tidak bisa merebut sisa kuota yang sama. Log aktivitas ditulis di transaksi yang sama lewat `ActivityLog.WriteAsync`, jadi booking dan lognya selalu konsisten.
- **Pagination di sisi server.** Daftar booking diambil per 10 baris memakai `OFFSET ... FETCH NEXT`, bukan mengambil semua data lalu memotong di memori.

### 4. Fitur Aplikasi

- **Login dan hak akses.** Ada dua role yang dipakai sehari-hari: `Admin` dan `Petugas`. Halaman Jalur Pendakian hanya bisa dibuka Admin; percobaan membukanya oleh Petugas ditolak dan ditampilkan sebagai toast. Pengecekan juga diulang di repository (`Session.RequireAdmin`), jadi tidak cukup hanya menyembunyikan tombol.
- **Dashboard.** Empat kartu statistik (booking bulan ini, pendaki di gunung, kuota hari ini, pendapatan bulan ini), bar kuota per jalur, dan delapan aktivitas terbaru. Semuanya dihitung langsung dari SQL Server lewat query agregasi, bukan angka statis.
- **Booking.** DataGrid dengan pencarian (nama ketua, kode booking, atau NIK), filter status, dan pagination. Pencarian diberi jeda 350 ms (*debounce*) supaya query tidak dijalankan di setiap ketukan huruf.
- **Form booking.** Gunung dan jalur dipilih bertingkat; memilih gunung yang ditutup mengosongkan daftar jalur dan menampilkan alasannya. Total biaya dihitung otomatis (tarif × jumlah anggota), dan kode booking dibuat otomatis dengan format `SBM-yyMMdd-XXXX`.
- **Validasi.** NIK harus 16 digit, nomor HP dicek dengan regex, jumlah anggota 1 sampai 15, tanggal naik tidak boleh di masa lalu, durasi maksimal 7 hari, dan kuota dicek terhadap booking lain di tanggal serta jalur yang sama.
- **Alur status.** Perubahan status diatur oleh `BookingRules`: Menunggu → Dikonfirmasi → Mendaki → Selesai, dan booking bisa dibatalkan selama belum mendaki. Check-in ke status *Mendaki* hanya bisa dilakukan pada tanggal naik.
- **Master jalur.** Admin dapat menambah, mengubah, dan menghapus jalur. Jalur yang sudah dipakai booking tidak bisa dihapus (ditolak foreign key) dan hanya bisa dinonaktifkan. Menutup sebuah gunung otomatis menonaktifkan semua jalurnya.
- **Tema.** Mode Light dan Dark bisa diganti tanpa restart. Pilihannya disimpan di `%LOCALAPPDATA%\Sibumi\settings.json` dan dimuat lagi saat aplikasi dibuka.
- **Penanganan error.** `ErrorHandler` membedakan kesalahan bisnis (tampil sebagai toast), kegagalan koneksi database (pesan ramah), dan error tak terduga (dicatat ke `%LOCALAPPDATA%\Sibumi\logs\error.log`), jadi aplikasi tidak langsung tertutup.

### 5. Cara Menjalankan

1. Jalankan `Sibumi/Database.sql` di SSMS yang terhubung ke `(localdb)\MSSQLLocalDB`.

   Perlu diperhatikan: skrip ini menghapus lalu membuat ulang tabel, jadi jangan dijalankan di database yang datanya masih dipakai.

2. Buka `Sibumi/Sibumi.sln` di Visual Studio dan tekan **F5**, atau lewat terminal:

   ```
   dotnet run --project Sibumi
   ```

3. Saat pertama dibuka, aplikasi membuat akun awal secara otomatis: `admin` / `Admin#123` dan `petugas` / `Petugas#123`.

### 6. Uji Coba Aplikasi

| Kode | Skenario | Hasil |
| :---: | :--- | :--- |
| TC-01 | Login `admin` / `Admin#123` | Jendela utama terbuka dengan profil Administrator, menu Jalur Pendakian aktif. |
| TC-02 | Login dengan password salah | Muncul pesan "Username atau password salah", form login tetap terbuka. |
| TC-03 | Database baru tanpa akun | Akun `admin`, `petugas`, dan `user` dibuat otomatis dalam bentuk hash. |
| TC-04 | NIK kurang dari 16 digit | Form menolak menyimpan dan menampilkan pesan "NIK harus 16 digit angka". |
| TC-05 | Tanggal turun sebelum tanggal naik | Form menolak dengan pesan bahwa tanggal turun tidak boleh mendahului tanggal naik. |
| TC-06 | Memesan melebihi sisa kuota | Penyimpanan ditolak, pesan menyebutkan sisa kuota pada tanggal tersebut. |
| TC-07 | Pencarian nama ketua | DataGrid langsung menyusut sesuai kata kunci, tanpa menekan tombol. |
| TC-08 | Ganti tema di Pengaturan | Seluruh tampilan berganti seketika, tanpa restart. |
| TC-09 | SQL Server dimatikan saat aplikasi berjalan | Muncul pesan koneksi yang ramah, aplikasi tidak crash, detailnya masuk ke file log. |

Catatan tambahan dari pengujian manual:

- Memilih gunung yang ditutup (misalnya Semeru) membuat daftar jalur kosong dan menampilkan keterangan penutupannya.
- Booking berstatus *Selesai* atau *Dibatalkan* tidak bisa diubah lagi; tombol ubah dan hapus otomatis nonaktif.
- Akun `petugas` tidak bisa membuka halaman Jalur Pendakian, sedangkan `admin` bisa.
- Setelah check-in, angka "Pendaki di Gunung" pada dashboard ikut bertambah, dan berkurang saat status diubah menjadi *Selesai*.
