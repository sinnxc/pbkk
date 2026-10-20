using System.Data;
using Microsoft.Data.SqlClient;
using Sibumi.Core;
using Sibumi.Models;

namespace Sibumi.Data;

public class UserRepository
{
    public async Task<User> GetByUsernameAsync(string username)
    {
        await using var c = Db.Create();
        await c.OpenAsync();
        await using var cmd = new SqlCommand(
            "SELECT UserId, Username, PasswordHash, Nama, Role, IsActive, NoHp, NIK FROM Users WHERE Username = @u", c)
            .With("@u", SqlDbType.NVarChar, username);
        await using var r = await cmd.ExecuteReaderAsync();
        if (!await r.ReadAsync()) return null;
        return new User
        {
            UserId = r.GetInt32(0),
            Username = r.GetString(1),
            PasswordHash = r.GetString(2),
            Nama = r.GetString(3),
            Role = r.GetString(4),
            IsActive = r.GetBoolean(5),
            NoHp = r.IsDBNull(6) ? null : r.GetString(6),
            NIK = r.IsDBNull(7) ? null : r.GetString(7)?.Trim()
        };
    }

    public async Task<int> CountAsync()
    {
        await using var c = Db.Create();
        await c.OpenAsync();
        await using var cmd = new SqlCommand("SELECT COUNT(*) FROM Users", c);
        return (int)await cmd.ExecuteScalarAsync();
    }

    public async Task<bool> ExistsUsernameAsync(string username)
    {
        await using var c = Db.Create();
        await c.OpenAsync();
        await using var cmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Username = @u", c)
            .With("@u", SqlDbType.NVarChar, username);
        return (int)await cmd.ExecuteScalarAsync() > 0;
    }

    public async Task InsertAsync(string username, string password, string nama, string role, string noHp = null, string nik = null)
    {
        await using var c = Db.Create();
        await c.OpenAsync();
        await using var cmd = new SqlCommand(
            @"INSERT INTO Users (Username, PasswordHash, Nama, Role, NoHp, NIK) 
              VALUES (@u, @p, @n, @r, @hp, @nik)", c)
            .With("@u", SqlDbType.NVarChar, username)
            .With("@p", SqlDbType.NVarChar, PasswordHasher.Hash(password))
            .With("@n", SqlDbType.NVarChar, nama)
            .With("@r", SqlDbType.NVarChar, role)
            .With("@hp", SqlDbType.NVarChar, noHp)
            .With("@nik", SqlDbType.NVarChar, nik);
        await cmd.ExecuteNonQueryAsync();
    }
}