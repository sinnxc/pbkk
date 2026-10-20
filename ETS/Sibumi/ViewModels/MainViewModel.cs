using System.Windows.Input;
using System.Windows.Threading;
using Sibumi.Core;

namespace Sibumi.ViewModels;

public class MainViewModel : ViewModelBase, IDisposable
{
    readonly DispatcherTimer _toastTimer = new() { Interval = TimeSpan.FromSeconds(3.5) };
    ViewModelBase _current;
    string _active = "Dashboard", _toastText;
    ToastKind _toastKind;
    bool _toastVisible;

    public DashboardViewModel Dashboard { get; } = new();
    public BookingViewModel Booking { get; } = new();
    public JalurViewModel Jalur { get; } = new();
    public SettingsViewModel Settings { get; } = new();

    public ViewModelBase Current { get => _current; private set => Set(ref _current, value); }
    public string Active { get => _active; private set => Set(ref _active, value); }

    public string UserName => Session.CurrentUser.Nama;
    public string UserRole => Session.CurrentUser.Role;
    public string Initial => Session.CurrentUser.Nama.Substring(0, 1).ToUpperInvariant();
    public bool IsAdmin => Session.IsAdmin;

    public string ToastText { get => _toastText; private set => Set(ref _toastText, value); }
    public ToastKind ToastKind { get => _toastKind; private set => Set(ref _toastKind, value); }
    public bool ToastVisible { get => _toastVisible; private set => Set(ref _toastVisible, value); }

    public ICommand NavigateCommand { get; }
    public ICommand LogoutCommand { get; }
    public event Action LogoutRequested;

    public MainViewModel()
    {
        Current = Dashboard;
        NavigateCommand = new AsyncCommand(async p => await NavigateAsync((string)p));
        LogoutCommand = new RelayCommand(_ => LogoutRequested?.Invoke());
        Toast.Raised += OnToast;
        _toastTimer.Tick += (_, _) => { _toastTimer.Stop(); ToastVisible = false; };
    }

    async Task NavigateAsync(string key)
    {
        ViewModelBase vm = key switch
        {
            "Dashboard" => Dashboard,
            "Booking" => Booking,
            "Jalur" => Jalur,
            "Pengaturan" => Settings,
            _ => Dashboard
        };
        if (key == "Jalur" && !Session.IsAdmin) throw new UnauthorizedAccessException();

        Active = key;
        Current = vm;
        await ((IPage)vm).LoadAsync();
    }

    void OnToast(string text, ToastKind kind)
    {
        ToastText = text; ToastKind = kind; ToastVisible = true;
        _toastTimer.Stop(); _toastTimer.Start();
    }

    public void Dispose() => Toast.Raised -= OnToast;
}