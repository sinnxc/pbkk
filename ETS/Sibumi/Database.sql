IF DB_ID('SIBUMI_DB') IS NULL 
    CREATE DATABASE SIBUMI_DB;
GO

USE SIBUMI_DB;
GO

-- PERHATIAN: bagian ini menghapus semua tabel beserta datanya, lalu membuatnya ulang.
-- Jalankan hanya untuk setup awal atau reset database.
IF OBJECT_ID('Aktivitas', 'U') IS NOT NULL DROP TABLE Aktivitas;
IF OBJECT_ID('Booking', 'U') IS NOT NULL DROP TABLE Booking;
IF OBJECT_ID('Jalur', 'U') IS NOT NULL DROP TABLE Jalur;
IF OBJECT_ID('Gunung', 'U') IS NOT NULL DROP TABLE Gunung;
IF OBJECT_ID('Users', 'U') IS NOT NULL DROP TABLE Users;
GO

-- 1. Tabel Users
CREATE TABLE Users (
    UserId         INT IDENTITY PRIMARY KEY,
    Username       NVARCHAR(50)  NOT NULL UNIQUE,
    PasswordHash   NVARCHAR(200) NOT NULL,
    Nama           NVARCHAR(100) NOT NULL,
    Role           NVARCHAR(20)  NOT NULL CHECK (Role IN (N'Admin', N'Petugas', N'User')),
    NoHp           NVARCHAR(20)  NULL,
    NIK            CHAR(16)      NULL,
    IsActive       BIT NOT NULL DEFAULT 1
);
GO

-- 2. Tabel Gunung
CREATE TABLE Gunung (
    GunungId         INT IDENTITY PRIMARY KEY,
    NamaGunung       NVARCHAR(100) NOT NULL UNIQUE,
    Lokasi           NVARCHAR(150) NOT NULL,
    Ketinggian       INT NOT NULL CHECK (Ketinggian > 0),
    BisaDidaki       BIT NOT NULL DEFAULT 1,
    KeteranganStatus NVARCHAR(250) NULL
);
GO

-- 3. Tabel Jalur
CREATE TABLE Jalur (
    JalurId       INT IDENTITY PRIMARY KEY,
    GunungId      INT NOT NULL REFERENCES Gunung(GunungId),
    NamaJalur     NVARCHAR(100) NOT NULL,
    KuotaHarian   INT NOT NULL CHECK (KuotaHarian > 0),
    TarifPerOrang DECIMAL(12,2) NOT NULL CHECK (TarifPerOrang >= 0),
    IsActive      BIT NOT NULL DEFAULT 1,
    CONSTRAINT UQ_Jalur UNIQUE (GunungId, NamaJalur)
);
GO

-- 4. Tabel Booking
CREATE TABLE Booking (
    BookingId      INT IDENTITY PRIMARY KEY,
    KodeBooking    NVARCHAR(20) NOT NULL UNIQUE,
    JalurId        INT NOT NULL REFERENCES Jalur(JalurId),
    NamaKetua      NVARCHAR(100) NOT NULL,
    NIK            CHAR(16) NOT NULL,
    NoHp           NVARCHAR(20) NOT NULL,
    Email          NVARCHAR(100) NULL,
    JumlahAnggota  INT NOT NULL CHECK (JumlahAnggota BETWEEN 1 AND 15),
    TanggalNaik    DATE NOT NULL,
    TanggalTurun   DATE NOT NULL,
    TotalBiaya     DECIMAL(14,2) NOT NULL,
    Status         NVARCHAR(20) NOT NULL DEFAULT N'Menunggu'
                   CHECK (Status IN (N'Menunggu', N'Dikonfirmasi', N'Mendaki', N'Selesai', N'Dibatalkan')),
    CreatedAt      DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CreatedBy      INT NULL REFERENCES Users(UserId),
    CONSTRAINT CK_Booking_Tanggal CHECK (TanggalTurun >= TanggalNaik)
);
GO

CREATE INDEX IX_Booking_Kuota ON Booking (JalurId, TanggalNaik) INCLUDE (JumlahAnggota, Status);
GO

-- 5. Tabel Aktivitas
CREATE TABLE Aktivitas (
    AktivitasId INT IDENTITY PRIMARY KEY,
    UserId      INT NULL REFERENCES Users(UserId),
    JenisAksi   NVARCHAR(30)  NOT NULL,
    Deskripsi   NVARCHAR(250) NOT NULL,
    Waktu       DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);
GO

-- Data contoh

-- Gunung yang BISA DIDAKI dan TIDAK BISA DIDAKI (Ditutup)
INSERT INTO Gunung (NamaGunung, Lokasi, Ketinggian, BisaDidaki, KeteranganStatus) VALUES
(N'Gunung Penanggungan', N'Mojokerto, Jawa Timur', 1653, 1, N'Buka — Jalur Normal & Kondusif'),
(N'Gunung Welirang',     N'Pasuruan, Jawa Timur',  3156, 1, N'Buka — Jalur Normal'),
(N'Gunung Lawu',         N'Magetan, Jawa Timur',   3265, 1, N'Buka — Jalur Normal'),
(N'Gunung Bromo',        N'Probolinggo, Jawa Timur', 2329, 1, N'Buka — Jalur Normal'),
(N'Gunung Semeru',       N'Lumajang, Jawa Timur',  3676, 0, N'Ditutup Sementara — Aktivitas Vulkanik & Status Siaga Level III'),
(N'Gunung Merapi',       N'Sleman / Magelang / Klaten', 2968, 0, N'Ditutup Total — Zona Bahaya Erupsi & Guguran Awan Panas'),
(N'Gunung Raung',        N'Banyuwangi, Jawa Timur', 3344, 0, N'Ditutup — Cuaca Ekstrem dan Risiko Longsor Jalur Puncak');
GO

INSERT INTO Jalur (GunungId, NamaJalur, KuotaHarian, TarifPerOrang, IsActive)
SELECT g.GunungId, v.Nama, v.Kuota, v.Tarif, v.Aktif
FROM (VALUES
  (N'Gunung Penanggungan', N'Tamiajeng',     150, 25000, 1),
  (N'Gunung Penanggungan', N'Jalatunda',     100, 25000, 1),
  (N'Gunung Welirang',     N'Tretes',        200, 30000, 1),
  (N'Gunung Lawu',         N'Cemoro Sewu',   250, 30000, 1),
  (N'Gunung Bromo',        N'Cemoro Lawang', 250, 35000, 1),
  (N'Gunung Semeru',       N'Ranu Pani',     300, 40000, 0)
) v(Gunung, Nama, Kuota, Tarif, Aktif)
JOIN Gunung g ON g.NamaGunung = v.Gunung;
GO

INSERT INTO Booking (KodeBooking, JalurId, NamaKetua, NIK, NoHp, Email, JumlahAnggota,
                     TanggalNaik, TanggalTurun, TotalBiaya, Status)
SELECT v.Kode, j.JalurId, v.Nama, v.NIK, v.Hp, v.Email, v.Jml,
       DATEADD(DAY, v.HariOffset, CAST(GETDATE() AS DATE)),
       DATEADD(DAY, v.HariOffset + v.Lama, CAST(GETDATE() AS DATE)),
       j.TarifPerOrang * v.Jml, v.Status
FROM (VALUES
  (N'SBM-SEED-0001', N'Tamiajeng',   N'Rizky Pratama',  '3578010101990001', N'081234567801', N'rizky@mail.com', 4,  0, 1, N'Mendaki'),
  (N'SBM-SEED-0002', N'Tamiajeng',   N'Dewi Lestari',   '3578010101990002', N'081234567802', NULL,             6,  2, 1, N'Dikonfirmasi'),
  (N'SBM-SEED-0003', N'Jalatunda',   N'Ahmad Fauzi',    '3578010101990003', N'081234567803', N'fauzi@mail.com', 3, -3, 1, N'Selesai'),
  (N'SBM-SEED-0004', N'Tretes',      N'Salsa Nabila',   '3578010101990005', N'081234567805', N'salsa@mail.com', 5,  1, 2, N'Dikonfirmasi'),
  (N'SBM-SEED-0005', N'Cemoro Sewu', N'Hendra Wijaya',  '3578010101990006', N'081234567806', NULL,             2,  3, 1, N'Dibatalkan'),
  (N'SBM-SEED-0006', N'Cemoro Lawang', N'Nadia Putri',  '3578010101990007', N'081234567807', NULL,             4,  0, 3, N'Mendaki')
) v(Kode, Jalur, Nama, NIK, Hp, Email, Jml, HariOffset, Lama, Status)
JOIN Jalur j ON j.NamaJalur = v.Jalur;
GO