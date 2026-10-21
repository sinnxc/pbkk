# Evaluasi Tengah Semester Pemrograman Berbasis Kerangka Kerja

| Nama | NRP | Kelas |
| ---- | ---- | ---- |
| Ahsin Khuluqil Karim | 5025241063 | Pemrograman Berbasis Kerangka Kerja D |

Topik: Evaluasi Tengah Semester

## Tautan Proyek

- **Link PRD (Product Requirements Document):**
  [`PRD_SIBUMI.md`](PRD_SIBUMI.md)
- **Link Repository GitHub:**
  [https://github.com/sinnxc/pbkk/tree/main/ETS](https://github.com/sinnxc/pbkk/tree/main/ETS)
- **Link Video Demo:**
  [Link Video Demo](https://youtu.be/bXBVLyUpcVo)

---

# SIBUMI - Sistem Informasi Booking dan Manajemen Pendakian

Aplikasi Desktop Rich UI berbasis **WPF, C#, dan Microsoft SQL Server** untuk petugas basecamp dan pengelola kawasan konservasi. SIBUMI mencatat pemesanan izin pendakian (simaksi), mengontrol kuota harian tiap jalur, memantau status pendaki yang sedang di gunung, dan menyajikan rekap pendapatan tiket secara langsung dari database.

---

## 1. Deskripsi dan Ruang Lingkup Fitur

### 1.1 Deskripsi Aplikasi

**SIBUMI** adalah aplikasi desktop Windows yang dibangun dengan **Windows Presentation Foundation (WPF)** dan arsitektur **MVVM (Model–View–ViewModel)**. Aplikasi ini dipakai di loket basecamp untuk menggantikan pencatatan izin pendakian berbasis kertas: petugas memasukkan data rombongan, sistem menghitung biaya dan mengecek sisa kuota secara otomatis, lalu data tersimpan ke **Microsoft SQL Server** (`SIBUMI_DB`).

| Atribut | Keterangan |
| :--- | :--- |
| **Nama Produk** | SIBUMI - Sistem Informasi Booking dan Manajemen Pendakian |
| **Jenis Aplikasi** | Desktop Rich UI |
| **Platform** | Windows 10 / 11 (WPF) |
| **Bahasa & Framework** | C#, XAML, .NET 10.0 (`net10.0-windows`) |
| **Basis Data** | Microsoft SQL Server LocalDB, `Microsoft.Data.SqlClient` 5.2.2 (ADO.NET) |
| **Pola Arsitektur** | MVVM + Repository + Service |
| **Basis Desain** | Modern Nature-Tech Rich UI (Dark Forest & Alpine Light) |

### 1.2 Fitur dalam Ruang Lingkup (In Scope)

| No | Modul / Fitur | Cakupan yang Diimplementasikan |
| :---: | :--- | :--- |
| 1 | **Autentikasi & Sesi** | Login terenkripsi (PBKDF2-SHA256), penyimpanan sesi aktif, peran akun, dan logout aman yang mengembalikan pengguna ke layar Login. |
| 2 | **Dashboard Interaktif** | Empat kartu metrik (Booking Bulan Ini, Pendaki di Gunung, Kuota Hari Ini, Pendapatan Bulan Ini), bar kuota per jalur, dan feed aktivitas terbaru. Semua angka dihitung dari query agregasi SQL Server, bukan data statis. |
| 3 | **Manajemen Booking (SIMAKSI)** | DataGrid dengan kolom kode booking, jalur, nama ketua, tanggal naik/turun, jumlah anggota, total biaya, dan badge status berwarna. Dilengkapi pagination di sisi server (10 baris per halaman). |
| 4 | **Pencarian & Filter** | Pencarian terpadu *real-time* (debounce 350 ms) berdasarkan Kode Booking, Nama Ketua, atau NIK; filter status (Semua, Menunggu, Dikonfirmasi, Mendaki, Selesai, Dibatalkan). |
| 5 | **Formulir Booking** | Pemilihan Gunung → Jalur bertingkat, auto-kalkulasi total biaya (tarif × jumlah anggota), pengecekan kuota, dan generate kode booking otomatis format `SBM-yyMMdd-XXXX`. |
| 6 | **Validasi Data** | NIK wajib 16 digit, nomor HP dicek regex, jumlah anggota 1–15 orang, tanggal naik tidak boleh di masa lalu, durasi maksimal 7 hari, dan tanggal turun ≥ tanggal naik. |
| 7 | **Alur Status Pendakian** | Menunggu → Dikonfirmasi → Mendaki → Selesai, serta opsi Dibatalkan. Check-in ke status *Mendaki* hanya dapat dilakukan pada tanggal naik. |
| 8 | **Master Jalur & Gunung** | Admin dapat menambah, mengubah, dan menonaktifkan jalur (nama, gunung, kuota harian, tarif). Gunung dapat ditandai buka/tutup beserta alasan penutupannya. |
| 9 | **Dukungan Tema** | Peralihan **Dark Mode** (Night Mountain) dan **Light Mode** (Alpine Daylight) secara instan tanpa me-restart aplikasi; preferensi tersimpan di `%LOCALAPPDATA%\Sibumi\settings.json`. |
| 10 | **Notifikasi & Error Handling** | Notifikasi *toast* untuk operasi berhasil/gagal, dialog konfirmasi untuk aksi destruktif, dan penanganan error yang membedakan kesalahan bisnis, gangguan koneksi database, dan error tak terduga (dicatat ke file log). |

### 1.3 Peran Pengguna dan Hak Akses

| Peran | Kebutuhan Utama | Hak Akses |
| :--- | :--- | :--- |
| **Administrator** | Manajemen sistem menyeluruh. | Akses penuh: CRUD Booking, CRUD Master Gunung & Jalur, hapus booking, audit aktivitas, pengaturan. |
| **Petugas Basecamp** | Pelayanan loket harian. | CRUD Booking, pencarian & filter, Check-In/Check-Out, akses baca pada Master Jalur. **Tidak** dapat membuka halaman Jalur Pendakian. |
| **Pengguna (Pendaki)** | Mengakses aplikasi dan login. | Terbatas pada autentikasi dan tampilan sesi. |

Pengecekan hak akses dilakukan berlapis: tombol disembunyikan di UI **dan** diverifikasi ulang di layer repository (`Session.RequireAdmin()` / `Session.RequireLogin()`), sehingga tidak cukup hanya menyembunyikan elemen tampilan.

### 1.4 Struktur Project (MVVM)

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

### 1.5 Skema Database Ringkas

Database `SIBUMI_DB` terdiri atas lima tabel yang saling berelasi:

- **`Users`** - akun pengelola (username unik, password hash, nama, role, `IsActive`).
- **`Gunung`** - data gunung (nama, lokasi, ketinggian, `BisaDidaki`, keterangan status).
- **`Jalur`** - jalur milik satu gunung (kuota harian, tarif per orang, status aktif).
- **`Booking`** - transaksi pemesanan (kode booking, data ketua, jumlah anggota, tanggal naik/turun, total biaya, status, pembuat).
- **`Aktivitas`** - log audit (siapa melakukan apa dan kapan).

Relasi: `Gunung` → `Jalur` (one-to-many), `Jalur` → `Booking` (one-to-many), `Users` → `Booking`/`Aktivitas` (one-to-many). Integritas data dijaga lewat `UNIQUE`, `CHECK` (misal `JumlahAnggota BETWEEN 1 AND 15`, `TanggalTurun >= TanggalNaik`), foreign key, dan index `IX_Booking_Kuota` pada `(JalurId, TanggalNaik)`.

---

## 2. Latar Belakang dan Manfaat

### 2.1 Latar Belakang

Pengelolaan izin pendakian di banyak basecamp masih dilakukan secara manual dan terpisah-pisah. Kondisi ini memunculkan beberapa masalah nyata:

- **Risiko *overcapacity* jalur.** Tanpa kontrol kuota harian yang ketat, jumlah pendaki di satu jalur dapat melampaui daya dukung (*carrying capacity*) kawasan. Akibatnya terjadi penumpukan di area perkemahan, kerusakan ekosistem, dan meningkatnya risiko kecelakaan.
- **Pencatatan konvensional yang rentan hilang.** Registrasi berbasis kertas menyulitkan pencarian data pendaki saat cuaca ekstrem, pendaki tersesat, atau proses evakuasi darurat, padahal data ketua rombongan (NIK, nomor HP, email) sangat dibutuhkan tim SAR.
- **Perhitungan tarif manual.** Kesalahan hitung tarif tiket dan retribusi sering terjadi saat antrean loket padat, misalnya pada *weekend rush*.
- **Kebutuhan UI yang nyaman untuk jam kerja panjang.** Petugas bekerja di berbagai kondisi pencahayaan - siang terik hingga malam di pos jaga - sehingga dibutuhkan antarmuka dengan kontras tinggi dan dukungan tema Gelap/Terang.

### 2.2 Tujuan Aplikasi Dibuat

1. **Digitalisasi izin pendakian** - menggantikan pencatatan kertas dengan sistem digital terpusat yang cepat dan akurat.
2. **Pengendalian daya dukung lingkungan** - mencegah *overcapacity* melalui validasi kuota harian secara otomatis per tanggal naik dan per jalur.
3. **Peningkatan keselamatan pendaki** - memudahkan pemantauan status pendaki dan penyediaan data kontak darurat untuk kebutuhan SAR.
4. **Efisiensi finansial dan administrasi** - mengotomatiskan perhitungan total tarif retribusi berdasarkan jumlah anggota dan jalur yang dipilih.
5. **Pengalaman UI modern** - menghadirkan aplikasi desktop kelas industri yang responsif, tidak membeku (pemrosesan asinkron), dan mudah dioperasikan oleh petugas.

### 2.3 Manfaat Aplikasi

| Pihak | Manfaat |
| :--- | :--- |
| **Pengelola Taman Nasional** | Memiliki satu sumber data terpusat untuk memantau pendaki, kuota, dan pendapatan; dasar pengambilan keputusan berbasis angka nyata. |
| **Petugas Basecamp** | Proses registrasi lebih cepat, biaya dihitung otomatis, pencarian data pendaki instan, dan antarmuka yang nyaman dipakai dalam shift panjang. |
| **Ketua Rombongan / Pendaki** | Proses perizinan lebih tertib dan transparan, dengan bukti pemesanan berkode unik sehingga verifikasi di pos lebih cepat. |
| **Tim SAR / Keamanan** | Data kontak ketua rombongan (NIK, nomor HP, email) dan status pendaki tersedia saat dibutuhkan untuk pencarian dan evakuasi. |
| **Konservasi Lingkungan** | Kontrol kuota harian membantu menjaga daya dukung jalur dan mengurangi risiko kerusakan ekosistem. |
| **Akuntabilitas Operasional** | Setiap aksi tercatat pada tabel `Aktivitas` (audit log), sehingga perubahan data dapat ditelusuri. |

---

## 3. Poster Infografis (1 Halaman)
