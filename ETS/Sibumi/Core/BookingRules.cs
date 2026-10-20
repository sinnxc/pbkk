namespace Sibumi.Core;

public static class BookingRules
{
    static readonly Dictionary<string, string[]> Next = new()
    {
        ["Menunggu"] = new[] { "Dikonfirmasi", "Dibatalkan" },
        ["Dikonfirmasi"] = new[] { "Mendaki", "Dibatalkan" },
        ["Mendaki"] = new[] { "Selesai" },
        ["Selesai"] = Array.Empty<string>(),
        ["Dibatalkan"] = Array.Empty<string>(),
    };

    public static bool CanTransition(string from, string to) =>
        from != null && Next.TryGetValue(from, out var list) && list.Contains(to);

    public static bool IsEditable(string status) => status is "Menunggu" or "Dikonfirmasi";
    public static bool IsDeletable(string status) => status is "Menunggu" or "Dibatalkan";
}