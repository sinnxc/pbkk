using Microsoft.Data.SqlClient;
using Sibumi.Core;
using Sibumi.Models;

namespace Sibumi.Data;

public class DashboardRepository
{
    public async Task<DashboardData> GetAsync()
    {
        Session.RequireLogin();
        const string sql = @"
            DECLARE @awal  DATE = DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1);
            DECLARE @akhir DATE = DATEADD(MONTH, 1, @awal);

            SELECT COUNT(*) FROM Booking WHERE TanggalNaik >= @awal AND TanggalNaik < @akhir AND Status <> N'Dibatalkan';
            SELECT ISNULL(SUM(JumlahAnggota), 0) FROM Booking WHERE Status = N'Mendaki';
            SELECT ISNULL(SUM(JumlahAnggota), 0) FROM Booking WHERE TanggalNaik = CAST(GETDATE() AS DATE) AND Status <> N'Dibatalkan';
            SELECT ISNULL(SUM(KuotaHarian), 0) FROM Jalur WHERE IsActive = 1;
            SELECT ISNULL(SUM(TotalBiaya), 0) FROM Booking
              WHERE TanggalNaik >= @awal AND TanggalNaik < @akhir AND Status IN (N'Dikonfirmasi', N'Mendaki', N'Selesai');

            SELECT g.NamaGunung + N' - ' + j.NamaJalur, ISNULL(SUM(b.JumlahAnggota), 0) AS Total
            FROM Jalur j
            JOIN Gunung g ON g.GunungId = j.GunungId
            LEFT JOIN Booking b ON b.JalurId = j.JalurId AND b.TanggalNaik >= @awal AND b.TanggalNaik < @akhir
                               AND b.Status <> N'Dibatalkan'
            WHERE j.IsActive = 1
            GROUP BY g.NamaGunung, j.NamaJalur
            ORDER BY Total DESC, g.NamaGunung;

            SELECT TOP 8 a.Waktu, ISNULL(u.Nama, N'Sistem'), a.JenisAksi, a.Deskripsi
            FROM Aktivitas a LEFT JOIN Users u ON u.UserId = a.UserId
            ORDER BY a.Waktu DESC, a.AktivitasId DESC;";

        var d = new DashboardData();
        await using var c = Db.Create();
        await c.OpenAsync();
        await using var cmd = new SqlCommand(sql, c);
        await using var r = await cmd.ExecuteReaderAsync();

        await r.ReadAsync(); d.TotalBooking = r.GetInt32(0); await r.NextResultAsync();
        await r.ReadAsync(); d.PendakiAktif = r.GetInt32(0); await r.NextResultAsync();
        await r.ReadAsync(); d.KuotaTerpakai = r.GetInt32(0); await r.NextResultAsync();
        await r.ReadAsync(); d.KuotaTotal = r.GetInt32(0); await r.NextResultAsync();
        await r.ReadAsync(); d.Pendapatan = r.GetDecimal(0); await r.NextResultAsync();

        while (await r.ReadAsync())
            d.Chart.Add(new ChartItem { Label = r.GetString(0), Value = r.GetInt32(1) });
        await r.NextResultAsync();

        while (await r.ReadAsync())
            d.Aktivitas.Add(new ActivityItem
            {
                WaktuText = r.GetDateTime(0).ToString("dd MMM yyyy HH:mm", Fmt.Id),
                Pengguna = r.GetString(1),
                JenisAksi = r.GetString(2),
                Deskripsi = r.GetString(3)
            });

        var max = Math.Max(1, d.Chart.Count == 0 ? 1 : d.Chart.Max(x => x.Value));
        foreach (var item in d.Chart) item.Max = max;
        return d;
    }
}