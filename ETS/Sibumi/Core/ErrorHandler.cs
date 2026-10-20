using Microsoft.Data.SqlClient;

namespace Sibumi.Core;

public static class ErrorHandler
{
    public static void Handle(Exception ex)
    {
        switch (ex)
        {
            case BusinessException:
                Toast.Error(ex.Message);
                break;
            case UnauthorizedAccessException:
                Toast.Error("Akses ditolak. Peran Anda tidak memiliki izin untuk aksi ini.");
                break;
            case SqlException sql when sql.Number is -1 or -2 or 2 or 53 or 4060 or 18456:
                Logger.Write(ex);
                Toast.Error("Tidak dapat terhubung ke database. Periksa SQL Server dan connection string.");
                break;
            default:
                Logger.Write(ex);
                Toast.Error("Terjadi kesalahan tak terduga. Detailnya sudah dicatat ke log.");
                break;
        }
    }
}