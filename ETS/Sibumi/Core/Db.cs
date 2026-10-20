using System.Data;
using System.IO;
using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace Sibumi.Core;

public static class Db
{
    static readonly Lazy<string> _cs = new(() =>
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        return doc.RootElement.GetProperty("ConnectionStrings").GetProperty("Sibumi").GetString();
    });

    public static SqlConnection Create() => new(_cs.Value);

    public static SqlCommand With(this SqlCommand cmd, string name, SqlDbType type, object value)
    {
        var p = cmd.Parameters.Add(name, type);
        if (type == SqlDbType.Decimal)
        {
            // tanpa ini SqlClient membulatkan desimal jadi bilangan bulat
            p.Precision = 14;
            p.Scale = 2;
        }
        p.Value = value ?? DBNull.Value;
        return cmd;
    }
}
