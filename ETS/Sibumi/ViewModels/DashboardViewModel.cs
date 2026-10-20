using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
using Sibumi.Core;
using Sibumi.Data;
using Sibumi.Models;

namespace Sibumi.ViewModels;

public class StatCard
{
    public string Title { get; init; }
    public string Value { get; init; }
    public string Caption { get; init; }
    public string Icon { get; init; }
    public Brush Accent { get; init; }
}

public class DashboardViewModel : ViewModelBase, IPage
{
    readonly DashboardRepository _repo = new();
    bool _loading;
    int _kuotaPersen;
    string _kuotaText = "-";

    public ObservableCollection<StatCard> Cards { get; } = new();
    public ObservableCollection<ChartItem> Chart { get; } = new();
    public ObservableCollection<ActivityItem> Activities { get; } = new();

    public bool IsLoading { get => _loading; set => Set(ref _loading, value); }
    public bool ChartEmpty => Chart.Count == 0;
    public bool ActivityEmpty => Activities.Count == 0;
    public int KuotaPersen { get => _kuotaPersen; private set => Set(ref _kuotaPersen, value); }
    public string KuotaText { get => _kuotaText; private set => Set(ref _kuotaText, value); }
    public string Greeting => $"{Salam()}, {Session.CurrentUser?.Nama}";
    public string Today => DateTime.Now.ToString("dddd, dd MMMM yyyy", Fmt.Id);
    public ICommand RefreshCommand { get; }

    public DashboardViewModel() => RefreshCommand = new AsyncCommand(_ => LoadAsync());

    static string Salam()
    {
        var h = DateTime.Now.Hour;
        if (h < 11) return "Selamat pagi";
        if (h < 15) return "Selamat siang";
        if (h < 18) return "Selamat sore";
        return "Selamat malam";
    }

    static Brush Hex(string hex)
    {
        var b = (Brush)new BrushConverter().ConvertFromString(hex);
        b.Freeze();
        return b;
    }

    public async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var d = await _repo.GetAsync();
            var persen = d.KuotaTotal == 0 ? 0 : (int)Math.Round(d.KuotaTerpakai * 100.0 / d.KuotaTotal);

            Cards.Clear();
            Cards.Add(new StatCard { Title = "Booking Bulan Ini", Value = d.TotalBooking.ToString("N0", Fmt.Id), Caption = "Tidak termasuk yang dibatalkan", Accent = Hex("#4C9BFF"), Icon = "\uE787" });
            Cards.Add(new StatCard { Title = "Pendaki di Gunung", Value = d.PendakiAktif.ToString("N0", Fmt.Id), Caption = "Berstatus Mendaki saat ini", Accent = Hex("#2FD38F"), Icon = "\uE716" });
            Cards.Add(new StatCard { Title = "Kuota Hari Ini", Value = $"{d.KuotaTerpakai} / {d.KuotaTotal}", Caption = $"{persen}% terpakai", Accent = Hex("#F5B547"), Icon = "\uE7C1" });
            Cards.Add(new StatCard { Title = "Pendapatan Bulan Ini", Value = Fmt.Rp(d.Pendapatan), Caption = "Booking terkonfirmasi ke atas", Accent = Hex("#A78BFA"), Icon = "\uE9D9" });

            KuotaPersen = Math.Min(100, persen);
            KuotaText = $"{d.KuotaTerpakai} dari {d.KuotaTotal} orang";
            OnPropertyChanged(nameof(Greeting));

            Chart.Clear();
            foreach (var x in d.Chart) Chart.Add(x);
            Activities.Clear();
            foreach (var a in d.Aktivitas) Activities.Add(a);
            OnPropertyChanged(nameof(ChartEmpty));
            OnPropertyChanged(nameof(ActivityEmpty));
        }
        finally { IsLoading = false; }
    }
}