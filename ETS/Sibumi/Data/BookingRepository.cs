using System.Data;
using Microsoft.Data.SqlClient;
using Sibumi.Core;
using Sibumi.Models;

namespace Sibumi.Data;

public class BookingRepository
{
    const string SelectSql = @"
        SELECT b.BookingId, b.KodeBooking, b.JalurId, g.NamaGunung + N' - ' + j.NamaJalur,
               b.NamaKetua, b.NIK, b.NoHp, b.Email, b.JumlahAnggota,
               b.TanggalNaik, b.TanggalTurun, b.TotalBiaya, b.Status, b.CreatedAt,
               b.CreatedBy, g.GunungId, g.NamaGunung
        FROM Booking b
        JOIN Jalur j ON j.JalurId = b.JalurId
        JOIN Gunung g ON g.GunungId = j.GunungId";

    static Booking Map(SqlDataReader r) => new()
    {
        BookingId = r.GetInt32(0),
        KodeBooking = r.GetString(1),
        JalurId = r.GetInt32(2),
        NamaJalur = r.GetString(3),
        NamaKetua = r.GetString(4),
        NIK = r.GetString(5).Trim(),
        NoHp = r.GetString(6),
        Email = r.IsDBNull(7) ? null : r.GetString(7),
        JumlahAnggota = r.GetInt32(8),
        TanggalNaik = r.GetDateTime(9),
        TanggalTurun = r.GetDateTime(10),
        TotalBiaya = r.GetDecimal(11),
        Status = r.GetString(12),
        CreatedAt = r.GetDateTime(13),
        CreatedBy = r.IsDBNull(14) ? null : r.GetInt32(14),
        GunungId = r.GetInt32(15),
        NamaGunung = r.GetString(16)
    };

    public async Task<(List<Booking> Items, int Total)> SearchAsync(string keyword, string status, int page, int pageSize)
    {
        Session.RequireLogin();
        const string filter = @" WHERE (@k = N'' OR b.NamaKetua LIKE N'%' + @k + N'%'
                                         OR b.KodeBooking LIKE N'%' + @k + N'%'
                                         OR b.NIK LIKE N'%' + @k + N'%')
                                    AND (@s = N'' OR b.Status = @s)";
        var sql = $@"SELECT COUNT(*) FROM Booking b{filter};
                     {SelectSql}{filter}
                     ORDER BY b.CreatedAt DESC, b.BookingId DESC
                     OFFSET @o ROWS FETCH NEXT @n ROWS ONLY;";

        await using var c = Db.Create();
        await c.OpenAsync();
        await using var cmd = new SqlCommand(sql, c)
            .With("@k", SqlDbType.NVarChar, keyword ?? "")
            .With("@s", SqlDbType.NVarChar, status ?? "")
            .With("@o", SqlDbType.Int, (page - 1) * pageSize)
            .With("@n", SqlDbType.Int, pageSize);
        await using var r = await cmd.ExecuteReaderAsync();
        await r.ReadAsync();
        var total = r.GetInt32(0);
        await r.NextResultAsync();
        var items = new List<Booking>();
        while (await r.ReadAsync()) items.Add(Map(r));
        return (items, total);
    }

    // BookingId = 0 berarti booking baru. Baris jalur dikunci supaya dua orang tidak bisa merebut sisa kuota yang sama.
    public async Task SaveAsync(Booking b)
    {
        Session.RequireLogin();
        var baru = b.BookingId == 0;

        await using var c = Db.Create();
        await c.OpenAsync();
        await using var tx = (SqlTransaction)await c.BeginTransactionAsync();

        int kuota; decimal tarif;
        await using (var cmd = new SqlCommand(
            "SELECT KuotaHarian, TarifPerOrang FROM Jalur WITH (UPDLOCK, ROWLOCK) WHERE JalurId=@j AND IsActive=1", c, tx)
            .With("@j", SqlDbType.Int, b.JalurId))
        await using (var r = await cmd.ExecuteReaderAsync())
        {
            if (!await r.ReadAsync()) throw new BusinessException("Jalur tidak tersedia atau sudah dinonaktifkan.");
            kuota = r.GetInt32(0);
            tarif = r.GetDecimal(1);
        }

        if (!baru)
        {
            await using var cmd = new SqlCommand("SELECT Status FROM Booking WITH (UPDLOCK) WHERE BookingId=@id", c, tx)
                .With("@id", SqlDbType.Int, b.BookingId);
            var st = (string)await cmd.ExecuteScalarAsync();
            if (st == null) throw new BusinessException("Data booking tidak ditemukan.");
            if (!BookingRules.IsEditable(st))
                throw new BusinessException($"Booking berstatus {st} tidak dapat diubah.");
        }

        int terpakai;
        await using (var cmd = new SqlCommand(
            @"SELECT ISNULL(SUM(JumlahAnggota), 0) FROM Booking
              WHERE JalurId=@j AND TanggalNaik=@t AND Status <> N'Dibatalkan' AND BookingId <> @id", c, tx)
            .With("@j", SqlDbType.Int, b.JalurId)
            .With("@t", SqlDbType.Date, b.TanggalNaik.Date)
            .With("@id", SqlDbType.Int, b.BookingId))
        {
            terpakai = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        if (terpakai + b.JumlahAnggota > kuota)
            throw new BusinessException(
                $"Kuota tanggal {b.TanggalNaik:dd MMM yyyy} tidak cukup. Sisa kuota: {Math.Max(0, kuota - terpakai)} orang.");

        b.TotalBiaya = tarif * b.JumlahAnggota;

        if (baru)
        {
            // kode dibuat acak, jadi kalau kebetulan kembar coba lagi
            for (var i = 0; ; i++)
            {
                b.KodeBooking = $"SBM-{DateTime.Now:yyMMdd}-{Random.Shared.Next(0, 0x10000):X4}";
                try
                {
                    await using var cmd = new SqlCommand(
                        @"INSERT INTO Booking (KodeBooking, JalurId, NamaKetua, NIK, NoHp, Email, JumlahAnggota,
                                               TanggalNaik, TanggalTurun, TotalBiaya, Status, CreatedBy)
                          VALUES (@kode, @j, @nama, @nik, @hp, @email, @jml, @naik, @turun, @total, N'Menunggu', @by)", c, tx)
                        .With("@kode", SqlDbType.NVarChar, b.KodeBooking)
                        .With("@by", SqlDbType.Int, Session.CurrentUser.UserId);
                    Bind(cmd, b);
                    await cmd.ExecuteNonQueryAsync();
                    break;
                }
                catch (SqlException ex) when ((ex.Number == 2627 || ex.Number == 2601) && i < 4) { }
            }
        }
        else
        {
            await using var cmd = new SqlCommand(
                @"UPDATE Booking SET JalurId=@j, NamaKetua=@nama, NIK=@nik, NoHp=@hp, Email=@email,
                         JumlahAnggota=@jml, TanggalNaik=@naik, TanggalTurun=@turun, TotalBiaya=@total
                  WHERE BookingId=@id", c, tx)
                .With("@id", SqlDbType.Int, b.BookingId);
            Bind(cmd, b);
            await cmd.ExecuteNonQueryAsync();
        }

        await ActivityLog.WriteAsync(c, tx, "Booking",
            $"Booking {b.KodeBooking} a.n. {b.NamaKetua} {(baru ? "dibuat" : "diperbarui")}");
        await tx.CommitAsync();
    }

    static void Bind(SqlCommand cmd, Booking b) => cmd
        .With("@j", SqlDbType.Int, b.JalurId)
        .With("@nama", SqlDbType.NVarChar, b.NamaKetua)
        .With("@nik", SqlDbType.Char, b.NIK)
        .With("@hp", SqlDbType.NVarChar, b.NoHp)
        .With("@email", SqlDbType.NVarChar, b.Email)
        .With("@jml", SqlDbType.Int, b.JumlahAnggota)
        .With("@naik", SqlDbType.Date, b.TanggalNaik.Date)
        .With("@turun", SqlDbType.Date, b.TanggalTurun.Date)
        .With("@total", SqlDbType.Decimal, b.TotalBiaya);

    public async Task UpdateStatusAsync(int bookingId, string newStatus)
    {
        Session.RequireLogin();
        await using var c = Db.Create();
        await c.OpenAsync();
        await using var tx = (SqlTransaction)await c.BeginTransactionAsync();

        string kode, current; DateTime naik;
        await using (var cmd = new SqlCommand(
            "SELECT KodeBooking, Status, TanggalNaik FROM Booking WITH (UPDLOCK) WHERE BookingId=@id", c, tx)
            .With("@id", SqlDbType.Int, bookingId))
        await using (var r = await cmd.ExecuteReaderAsync())
        {
            if (!await r.ReadAsync()) throw new BusinessException("Data booking tidak ditemukan.");
            kode = r.GetString(0); current = r.GetString(1); naik = r.GetDateTime(2);
        }

        if (!BookingRules.CanTransition(current, newStatus))
            throw new BusinessException($"Status {current} tidak dapat diubah menjadi {newStatus}.");
        if (newStatus == "Mendaki" && naik.Date > DateTime.Today)
            throw new BusinessException("Check-in baru dapat dilakukan pada tanggal naik.");

        await using (var cmd = new SqlCommand("UPDATE Booking SET Status=@s WHERE BookingId=@id", c, tx)
            .With("@s", SqlDbType.NVarChar, newStatus)
            .With("@id", SqlDbType.Int, bookingId))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        await ActivityLog.WriteAsync(c, tx, "Status", $"Booking {kode}: {current} menjadi {newStatus}");
        await tx.CommitAsync();
    }

    public async Task DeleteAsync(int bookingId)
    {
        Session.RequireAdmin();
        await using var c = Db.Create();
        await c.OpenAsync();
        await using var tx = (SqlTransaction)await c.BeginTransactionAsync();

        string kode, status;
        await using (var cmd = new SqlCommand("SELECT KodeBooking, Status FROM Booking WITH (UPDLOCK) WHERE BookingId=@id", c, tx)
            .With("@id", SqlDbType.Int, bookingId))
        await using (var r = await cmd.ExecuteReaderAsync())
        {
            if (!await r.ReadAsync()) throw new BusinessException("Data booking tidak ditemukan.");
            kode = r.GetString(0); status = r.GetString(1);
        }
        if (!BookingRules.IsDeletable(status))
            throw new BusinessException($"Booking berstatus {status} tidak boleh dihapus.");

        await using (var cmd = new SqlCommand("DELETE FROM Booking WHERE BookingId=@id", c, tx)
            .With("@id", SqlDbType.Int, bookingId))
        {
            await cmd.ExecuteNonQueryAsync();
        }
        await ActivityLog.WriteAsync(c, tx, "Booking", $"Booking {kode} dihapus");
        await tx.CommitAsync();
    }
}