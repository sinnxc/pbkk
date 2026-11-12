using Microsoft.Data.SqlClient;
using StudentManagementApp.Models;

namespace StudentManagementApp.Repositories;

public sealed class StudentRepository(string connectionString) : IStudentRepository
{
    public async Task<IReadOnlyList<Student>> GetAll()
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(
            "SELECT Id, Nim, Nama, Jurusan, Gender, Email FROM dbo.Mahasiswa ORDER BY Id", connection);
        await using var reader = await command.ExecuteReaderAsync();
        var students = new List<Student>();
        while (await reader.ReadAsync())
            students.Add(new Student
            {
                Id = reader.GetInt32(0), NIM = reader.GetString(1), Nama = reader.GetString(2),
                Jurusan = reader.GetString(3), Gender = reader.GetString(4),
                Email = reader.IsDBNull(5) ? "" : reader.GetString(5)
            });
        return students;
    }

    public async Task Insert(Student student)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(
            "INSERT INTO dbo.Mahasiswa (Nim,Nama,Jurusan,Gender,Email) VALUES (@NIM,@Nama,@Jurusan,@Gender,@Email)", connection);
        command.Parameters.Add("@NIM", System.Data.SqlDbType.VarChar, 20).Value = student.NIM;
        command.Parameters.Add("@Nama", System.Data.SqlDbType.VarChar, 100).Value = student.Nama;
        command.Parameters.Add("@Jurusan", System.Data.SqlDbType.VarChar, 100).Value = student.Jurusan;
        command.Parameters.Add("@Gender", System.Data.SqlDbType.VarChar, 20).Value = student.Gender;
        command.Parameters.Add("@Email", System.Data.SqlDbType.VarChar, 100).Value =
            string.IsNullOrWhiteSpace(student.Email) ? DBNull.Value : student.Email;
        try { await command.ExecuteNonQueryAsync(); }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        { throw new InvalidOperationException("NIM sudah terdaftar.", ex); }
    }

    public async Task Delete(int id)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand("DELETE FROM dbo.Mahasiswa WHERE Id=@Id", connection);
        command.Parameters.Add("@Id", System.Data.SqlDbType.Int).Value = id;
        if (await command.ExecuteNonQueryAsync() == 0)
            throw new InvalidOperationException("Data mahasiswa tidak ditemukan. Muat ulang daftar.");
    }
}
