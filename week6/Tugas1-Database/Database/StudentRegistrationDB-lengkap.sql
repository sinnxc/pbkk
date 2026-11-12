-- Week 6: seluruh skrip praktikum. Jalankan melalui SSMS (mendukung GO).
-- Dua uji constraint di bagian akhir memang menghasilkan error 547 dan 2627.

-- Sumber: 01-setup.sql
USE master;
GO
IF DB_ID(N'StudentRegistrationDB') IS NULL
    CREATE DATABASE StudentRegistrationDB;
GO
USE StudentRegistrationDB;
GO
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.Programs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Programs
    (
        ProgramId INT IDENTITY(1,1) CONSTRAINT PK_Programs PRIMARY KEY,
        ProgramCode VARCHAR(20) NOT NULL CONSTRAINT UQ_Programs_ProgramCode UNIQUE,
        ProgramName VARCHAR(100) NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Programs_IsActive DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Programs_CreatedAt DEFAULT SYSDATETIME()
    );
END;

IF OBJECT_ID(N'dbo.Students', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Students
    (
        StudentId INT IDENTITY(1,1) CONSTRAINT PK_Students PRIMARY KEY,
        NIM VARCHAR(20) NOT NULL CONSTRAINT UQ_Students_NIM UNIQUE,
        Name VARCHAR(100) NOT NULL,
        ProgramId INT NOT NULL,
        BirthDate DATE NULL,
        Address VARCHAR(250) NULL,
        PhoneNumber VARCHAR(20) NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Students_CreatedAt DEFAULT SYSDATETIME(),
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT FK_Students_Programs FOREIGN KEY (ProgramId) REFERENCES dbo.Programs(ProgramId)
    );
END;

INSERT INTO dbo.Programs (ProgramCode, ProgramName)
SELECT v.ProgramCode, v.ProgramName
FROM (VALUES
    ('TI', 'Teknik Informatika'),
    ('SI', 'Sistem Informasi'),
    ('MNJ', 'Manajemen'),
    ('TE', 'Teknik Elektro'),
    ('DKV', 'Desain Komunikasi Visual')
) v(ProgramCode, ProgramName)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Programs p WHERE p.ProgramCode = v.ProgramCode);

INSERT INTO dbo.Students (NIM, Name, ProgramId, BirthDate, Address, PhoneNumber)
SELECT v.NIM, v.Name, p.ProgramId, CONVERT(DATE, v.BirthDate, 23), v.Address, v.PhoneNumber
FROM (VALUES
    ('20231001', 'Ahmad Rizki', 'TI', '2003-03-12', 'Jl. Merdeka No. 123, Surabaya', '081234567890'),
    ('20231002', 'Siti Aisyah', 'SI', '2004-07-21', 'Jl. Diponegoro No. 45, Surabaya', '082233445566'),
    ('20231003', 'Budi Santoso', 'TI', '2003-11-05', 'Jl. Ahmad Yani No. 10, Surabaya', '081355667788'),
    ('20231004', 'Citra Lestari', 'MNJ', '2004-01-18', 'Jl. Darmo No. 20, Surabaya', '089900112233'),
    ('20231005', 'Dimas Prayoga', 'TE', '2003-09-27', 'Jl. Raya ITS, Surabaya', '081288776655')
) v(NIM, Name, ProgramCode, BirthDate, Address, PhoneNumber)
INNER JOIN dbo.Programs p ON p.ProgramCode = v.ProgramCode
WHERE NOT EXISTS (SELECT 1 FROM dbo.Students s WHERE s.NIM = v.NIM);

COMMIT TRANSACTION;
GO
SELECT DB_NAME() AS CurrentDatabase;
SELECT * FROM dbo.Programs ORDER BY ProgramId;
SELECT * FROM dbo.Students ORDER BY StudentId;

GO

-- Sumber: 03-mini-assignment.sql
USE StudentRegistrationDB;
GO
SET XACT_ABORT ON;
BEGIN TRANSACTION;
INSERT INTO StudentRegistrationDB.dbo.Students (NIM, Name, ProgramId, BirthDate, Address, PhoneNumber)
SELECT v.NIM, v.Name, p.ProgramId, CONVERT(DATE, v.BirthDate, 23), v.Address, v.PhoneNumber
FROM (VALUES
    ('20231006', 'Andi Saputra', 'TI', '2004-02-14', 'Jl. Kenanga No. 6, Surabaya', '081234560006'),
    ('20231007', 'Nadia Putri', 'SI', '2004-06-20', 'Jl. Melati No. 7, Surabaya', '081234560007'),
    ('20231008', 'Rafi Pratama', 'TE', '2003-12-10', 'Jl. Mawar No. 8, Surabaya', '081234560008')
) v(NIM, Name, ProgramCode, BirthDate, Address, PhoneNumber)
INNER JOIN StudentRegistrationDB.dbo.Programs p ON p.ProgramCode = v.ProgramCode
WHERE NOT EXISTS (SELECT 1 FROM StudentRegistrationDB.dbo.Students s WHERE s.NIM = v.NIM);
COMMIT TRANSACTION;
GO
SELECT s.NIM, s.Name, p.ProgramName, s.BirthDate, s.PhoneNumber
FROM StudentRegistrationDB.dbo.Students s
INNER JOIN StudentRegistrationDB.dbo.Programs p ON s.ProgramId = p.ProgramId
ORDER BY s.Name;

SELECT COUNT(*) AS TotalMahasiswa FROM StudentRegistrationDB.dbo.Students;
SELECT p.ProgramCode, p.ProgramName, COUNT(s.StudentId) AS JumlahMahasiswa
FROM StudentRegistrationDB.dbo.Programs p
LEFT JOIN StudentRegistrationDB.dbo.Students s ON p.ProgramId = s.ProgramId
GROUP BY p.ProgramId, p.ProgramCode, p.ProgramName
ORDER BY p.ProgramId;

GO

-- Sumber: 04-query-screenshot.sql
-- Query baca untuk screenshot. Nama database ditulis lengkap agar tetap
-- berjalan ketika dropdown SSMS masih master atau hanya satu SELECT disorot.
SELECT DB_NAME() AS DatabaseAktif,
       N'StudentRegistrationDB' AS DatabaseYangDibaca;

SELECT *
FROM StudentRegistrationDB.dbo.Programs
ORDER BY ProgramId;

SELECT *
FROM StudentRegistrationDB.dbo.Students
ORDER BY StudentId;

SELECT s.NIM, s.Name, p.ProgramName, s.BirthDate, s.PhoneNumber
FROM StudentRegistrationDB.dbo.Students s
INNER JOIN StudentRegistrationDB.dbo.Programs p ON p.ProgramId = s.ProgramId
ORDER BY s.Name;

SELECT s.NIM, s.Name, p.ProgramName
FROM StudentRegistrationDB.dbo.Students s
INNER JOIN StudentRegistrationDB.dbo.Programs p ON p.ProgramId = s.ProgramId
WHERE p.ProgramCode = 'TI'
ORDER BY s.NIM;

SELECT COUNT(*) AS TotalMahasiswa
FROM StudentRegistrationDB.dbo.Students;

SELECT p.ProgramCode, p.ProgramName, COUNT(s.StudentId) AS JumlahMahasiswa
FROM StudentRegistrationDB.dbo.Programs p
LEFT JOIN StudentRegistrationDB.dbo.Students s ON s.ProgramId = p.ProgramId
GROUP BY p.ProgramId, p.ProgramCode, p.ProgramName
ORDER BY p.ProgramId;

GO

-- Sumber: 02-query-dan-constraint.sql
USE StudentRegistrationDB;
GO
-- Challenge 1: seluruh mahasiswa.
SELECT * FROM StudentRegistrationDB.dbo.Students ORDER BY StudentId;

-- JOIN menampilkan program studi setiap mahasiswa.
SELECT s.StudentId, s.NIM, s.Name, p.ProgramCode, p.ProgramName,
       s.BirthDate, s.PhoneNumber
FROM StudentRegistrationDB.dbo.Students s
INNER JOIN StudentRegistrationDB.dbo.Programs p ON p.ProgramId = s.ProgramId
ORDER BY s.StudentId;

-- Challenge 2: hanya Teknik Informatika.
SELECT s.NIM, s.Name, p.ProgramName
FROM StudentRegistrationDB.dbo.Students s
INNER JOIN StudentRegistrationDB.dbo.Programs p ON p.ProgramId = s.ProgramId
WHERE p.ProgramCode = 'TI'
ORDER BY s.NIM;

-- Challenge 3: jumlah seluruh mahasiswa.
SELECT COUNT(*) AS TotalMahasiswa FROM StudentRegistrationDB.dbo.Students;

-- Challenge 4: LEFT JOIN menyertakan program tanpa mahasiswa.
SELECT p.ProgramCode, p.ProgramName, COUNT(s.StudentId) AS JumlahMahasiswa
FROM StudentRegistrationDB.dbo.Programs p
LEFT JOIN StudentRegistrationDB.dbo.Students s ON s.ProgramId = p.ProgramId
GROUP BY p.ProgramId, p.ProgramCode, p.ProgramName
ORDER BY p.ProgramId;
GO

-- Jalankan tiap blok terpisah di SSMS untuk screenshot pesan error asli.
-- ROLLBACK juga menjaga data jika database pernah dimodifikasi sebelumnya.
BEGIN TRANSACTION;
BEGIN TRY
    INSERT INTO StudentRegistrationDB.dbo.Students (NIM, Name, ProgramId)
    VALUES ('20231099', 'Test Student', 99);
    ROLLBACK TRANSACTION;
    THROW 51000, 'Uji FK gagal: ProgramId 99 diterima. Periksa data Programs.', 1;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

BEGIN TRANSACTION;
BEGIN TRY
    INSERT INTO StudentRegistrationDB.dbo.Students (NIM, Name, ProgramId)
    SELECT '20231001', 'Mahasiswa Duplikat', ProgramId
    FROM StudentRegistrationDB.dbo.Programs WHERE ProgramCode = 'TI';
    ROLLBACK TRANSACTION;
    THROW 51001, 'Uji UNIQUE gagal: NIM duplikat diterima.', 1;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

GO
