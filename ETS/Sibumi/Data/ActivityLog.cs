using System.Data;
using Microsoft.Data.SqlClient;
using Sibumi.Core;

namespace Sibumi.Data;

public static class ActivityLog
{
    // Dipanggil di dalam transaksi yang sama dengan operasi utamanya
    public static async Task WriteAsync(SqlConnection c, SqlTransaction tx, string aksi, string deskripsi)
    {
        await using var cmd = new SqlCommand(
            "INSERT INTO Aktivitas (UserId, JenisAksi, Deskripsi) VALUES (@u, @a, @d)", c, tx)
            .With("@u", SqlDbType.Int, Session.CurrentUser?.UserId)
            .With("@a", SqlDbType.NVarChar, aksi)
            .With("@d", SqlDbType.NVarChar, deskripsi);
        await cmd.ExecuteNonQueryAsync();
    }
}
