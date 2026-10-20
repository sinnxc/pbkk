using Sibumi.Core;

namespace Sibumi.ViewModels;

public class SettingsViewModel : ViewModelBase, IPage
{
    public bool IsDark
    {
        get => ThemeService.IsDark;
        set
        {
            if (value == ThemeService.IsDark) return;
            ThemeService.Apply(value);
            OnPropertyChanged(nameof(IsDark));
            OnPropertyChanged(nameof(IsLight));
        }
    }
    public bool IsLight { get => !IsDark; set => IsDark = !value; }

    public string Nama => Session.CurrentUser?.Nama;
    public string Username => Session.CurrentUser?.Username;
    public string Initial => string.IsNullOrEmpty(Nama) ? "?" : Nama.Substring(0, 1).ToUpperInvariant();
    public string Role => Session.CurrentUser?.Role;

    public Task LoadAsync()
    {
        OnPropertyChanged(nameof(Nama)); OnPropertyChanged(nameof(Username)); OnPropertyChanged(nameof(Role)); OnPropertyChanged(nameof(Initial));
        return Task.CompletedTask;
    }
}