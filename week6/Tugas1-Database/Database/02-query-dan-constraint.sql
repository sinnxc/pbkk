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
