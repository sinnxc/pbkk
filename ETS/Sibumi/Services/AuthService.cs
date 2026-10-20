using System.Data;
using Microsoft.Data.SqlClient;
using Sibumi.Core;
using Sibumi.Data;
using Sibumi.Models;

namespace Sibumi.Services;

public class AuthService
{
    readonly UserRepository _users = new();

    // Buat akun awal admin, petugas, dan user jika belum ada
    public async Task EnsureSeedAsync()
    {
        await EnsureSchemaMigrationAsync();

        if (await _users.GetByUsernameAsync("admin") == null)
            await _users.InsertAsync("admin", "Admin#123", "Administrator", "Admin");

        if (await _users.GetByUsernameAsync("petugas") == null)
            await _users.InsertAsync("petugas", "Petugas#123", "Petugas Basecamp", "Petugas");

        if (await _users.GetByUsernameAsync("user") == null)
            await _users.InsertAsync("user", "User#123", "Aziz Ramadhani (Pendaki)", "User", "081234567890", "3578010101990001");
    }

    async Task EnsureSchemaMigrationAsync()
    {
        try
        {
            await using var c = Db.Create();
            await c.OpenAsync();
            const string sql = @"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Gunung') AND name = 'BisaDidaki')
                    ALTER TABLE Gunung ADD BisaDidaki BIT NOT NULL CONSTRAINT DF_Gunung_BisaDidaki DEFAULT 1;

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Gunung') AND name = 'KeteranganStatus')
                    ALTER TABLE Gunung ADD KeteranganStatus NVARCHAR(250) NULL;

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'NoHp')
                    ALTER TABLE Users ADD NoHp NVARCHAR(20) NULL;

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'NIK')
                    ALTER TABLE Users ADD NIK CHAR(16) NULL;
            ";
            await using var cmd = new SqlCommand(sql, c);
            await cmd.ExecuteNonQueryAsync();
        }
        catch { /* abaikan jika sudah ada atau permissions terbatas */ }
    }

    public async Task<User> LoginAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            throw new BusinessException("Username dan password wajib diisi.");

        var user = await _users.GetByUsernameAsync(username.Trim());
        if (user == null || !PasswordHasher.Verify(password, user.PasswordHash))
            throw new BusinessException("Username atau password salah.");
        if (!user.IsActive)
            throw new BusinessException("Akun Anda nonaktif. Hubungi administrator.");

        Session.Start(user);
        return user;
    }

    public async Task<User> RegisterAsync(string username, string password, string nama, string noHp, string nik)
    {
        if (string.IsNullOrWhiteSpace(username) || username.Trim().Length < 3)
            throw new BusinessException("Username minimal 3 karakter.");
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            throw new BusinessException("Password minimal 6 karakter.");
        if (string.IsNullOrWhiteSpace(nama) || nama.Trim().Length < 3)
            throw new BusinessException("Nama lengkap minimal 3 karakter.");
        if (string.IsNullOrWhiteSpace(nik) || nik.Trim().Length != 16)
            throw new BusinessException("NIK harus 16 digit angka.");
        if (string.IsNullOrWhiteSpace(noHp) || noHp.Trim().Length < 10)
            throw new BusinessException("Nomor HP minimal 10 digit.");

        if (await _users.ExistsUsernameAsync(username.Trim()))
            throw new BusinessException("Username sudah digunakan. Silakan pilih username lain.");

        await _users.InsertAsync(username.Trim(), password, nama.Trim(), "User", noHp.Trim(), nik.Trim());
        var user = await _users.GetByUsernameAsync(username.Trim());
        Session.Start(user);
        return user;
    }
}