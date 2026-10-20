using System.Globalization;

namespace Sibumi.Core;

public static class Fmt
{
    public static readonly CultureInfo Id = new("id-ID");
    public static string Rp(decimal v) => "Rp " + v.ToString("N0", Id);
}