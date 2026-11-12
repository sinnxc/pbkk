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
