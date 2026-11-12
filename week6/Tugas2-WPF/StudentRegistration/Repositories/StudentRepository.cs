using System.Data;
using Microsoft.Data.SqlClient;
using StudentRegistration.Models;

namespace StudentRegistration.Repositories;

public sealed class StudentRepository
{
    public const string DefaultConnectionString =
        @"Server=.\SQLEXPRESS;Database=StudentRegistrationDB;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=5;";

    private readonly string _connectionString;

    public StudentRepository(string? connectionString = null)
    {
        _connectionString = connectionString
            ?? Environment.GetEnvironmentVariable("WEEK6_SQL_CONNECTION")
            ?? DefaultConnectionString;
    }

    public List<Student> GetAll()
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
        var result = ReadStudents(connection, transaction);
        transaction.Commit();
        return result;
    }

    public (List<Student> Students, int Total) GetSnapshot()
    {
        // Kedua SELECT melihat kumpulan data yang sama meskipun ada penulis lain.
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
        var students = ReadStudents(connection, transaction);
        using var command = new SqlCommand("SELECT COUNT(*) FROM dbo.Students;", connection, transaction);
        var total = Convert.ToInt32(command.ExecuteScalar());
        transaction.Commit();
        return (students, total);
    }

    private static List<Student> ReadStudents(SqlConnection connection, SqlTransaction transaction)
    {
        const string sql = """
            SELECT s.StudentId, s.NIM, s.Name, s.ProgramId, p.ProgramName,
                   s.BirthDate, s.Address, s.PhoneNumber, s.CreatedAt, s.UpdatedAt
            FROM dbo.Students s
            INNER JOIN dbo.Programs p ON p.ProgramId = s.ProgramId
            ORDER BY s.StudentId;
            """;
        using var command = new SqlCommand(sql, connection, transaction);
        using SqlDataReader reader = command.ExecuteReader();
        var students = new List<Student>();
        while (reader.Read())
        {
            students.Add(new Student
            {
                StudentId = reader.GetInt32(0),
                NIM = reader.GetString(1),
                Name = reader.GetString(2),
                ProgramId = reader.GetInt32(3),
                ProgramName = reader.GetString(4),
                BirthDate = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                Address = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                PhoneNumber = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                CreatedAt = reader.GetDateTime(8),
                UpdatedAt = reader.IsDBNull(9) ? null : reader.GetDateTime(9)
            });
        }
        return students;
    }
}
