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
