using StudentManagementApp.Models;

namespace StudentManagementApp.Repositories;

public sealed class DemoStudentRepository : IStudentRepository
{
    private readonly List<Student> students = [];
    private int nextId = 1;

    public Task<IReadOnlyList<Student>> GetAll() =>
        Task.FromResult<IReadOnlyList<Student>>(students.Select(Clone).ToList());

    public Task Insert(Student student)
    {
        if (students.Any(x => string.Equals(x.NIM, student.NIM, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("NIM sudah terdaftar.");
        var copy = Clone(student);
        copy.Id = nextId++;
        students.Add(copy);
        return Task.CompletedTask;
    }

    public Task Delete(int id)
    {
        if (students.RemoveAll(x => x.Id == id) == 0)
            throw new InvalidOperationException("Data mahasiswa tidak ditemukan. Muat ulang daftar.");
        return Task.CompletedTask;
    }

    private static Student Clone(Student s) => new()
    {
        Id = s.Id, NIM = s.NIM, Nama = s.Nama,
        Jurusan = s.Jurusan, Gender = s.Gender, Email = s.Email
    };
}
