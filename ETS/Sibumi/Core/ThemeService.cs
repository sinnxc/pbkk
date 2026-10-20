using System.IO;
using System.Text.Json;
using System.Windows;

namespace Sibumi.Core;

public static class ThemeService
{
    class Prefs { public bool Dark { get; set; } }

    static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sibumi", "settings.json");

    public static bool IsDark { get; private set; }

    public static void LoadSaved()
    {
        try
        {
            if (File.Exists(FilePath))
                IsDark = JsonSerializer.Deserialize<Prefs>(File.ReadAllText(FilePath))?.Dark ?? false;
        }
        catch { }
        Apply(IsDark, save: false);
    }

    public static void Apply(bool dark, bool save = true)
    {
        IsDark = dark;
        var merged = Application.Current.Resources.MergedDictionaries;

        var old = merged.FirstOrDefault(d => d.Source != null &&
            (d.Source.OriginalString.StartsWith("Themes/Dark") || d.Source.OriginalString.StartsWith("Themes/Light")));
        if (old != null) merged.Remove(old);

        var file = dark ? "Dark" : "Light";
        merged.Insert(0, new ResourceDictionary { Source = new Uri($"Themes/{file}.xaml", UriKind.Relative) });

        if (!save) return;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            File.WriteAllText(FilePath, JsonSerializer.Serialize(new Prefs { Dark = dark }));
        }
        catch { }
    }
}
