using StudentManagementApp.Models;

namespace StudentManagementApp.Repositories;

public interface IStudentRepository
{
    Task<IReadOnlyList<Student>> GetAll();
    Task Insert(Student student);
    Task Delete(int id);
}
