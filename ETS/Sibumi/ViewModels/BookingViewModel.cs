using System.Collections.ObjectModel;
using System.Windows.Input;
using Sibumi.Core;
using Sibumi.Data;
using Sibumi.Models;

namespace Sibumi.ViewModels;

public class BookingViewModel : ViewModelBase, IPage
{
    const int PageSize = 10;
    readonly BookingRepository _repo = new();
    CancellationTokenSource _cts;
    int _version, _page = 1, _total;
    string _search = "", _status = "Semua";
    bool _loading, _empty;
    Booking _selected;

    public ObservableCollection<Booking> Items { get; } = new();
    public string[] StatusOptions { get; } = { "Semua", "Menunggu", "Dikonfirmasi", "Mendaki", "Selesai", "Dibatalkan" };

    public bool IsAdmin => Session.IsAdmin;
    public string PageHeaderTitle => "Booking";
    public string PageHeaderSubtitle => "Cari, verifikasi tiket, dan pantau status pendaki.";
    public string AddButtonText => "Booking Baru";

    public string Search { get => _search; set { if (Set(ref _search, value)) _ = ReloadAsync(); } }
    public string Status { get => _status; set { if (Set(ref _status, value)) _ = ReloadAsync(); } }
    public Booking Selected { get => _selected; set => Set(ref _selected, value); }
    public bool IsLoading { get => _loading; set => Set(ref _loading, value); }
    public bool IsEmpty { get => _empty; set => Set(ref _empty, value); }

    public int TotalPages => Math.Max(1, (int)Math.Ceiling(_total / (double)PageSize));
    public string PageInfo => _total == 0
        ? "Tidak ada data"
        : $"{(_page - 1) * PageSize + 1}-{Math.Min(_page * PageSize, _total)} dari {_total} booking";
    public string PageLabel => $"Halaman {_page} / {TotalPages}";

    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand StatusCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand NextCommand { get; }
    public ICommand PrevCommand { get; }
    public ICommand RefreshCommand { get; }

    public BookingViewModel()
    {
        AddCommand = new AsyncCommand(async _ => { if (DialogService.ShowBookingForm(null)) await LoadAsync(); });

        EditCommand = new AsyncCommand(
            async _ => { if (DialogService.ShowBookingForm(Selected.Clone())) await LoadAsync(); },
            _ => Selected != null && BookingRules.IsEditable(Selected.Status));

        StatusCommand = new AsyncCommand(async p =>
        {
            var to = (string)p; var b = Selected;
            if (to == "Dibatalkan" && !DialogService.Confirm("Batalkan booking",
                    $"Batalkan booking {b.KodeBooking} a.n. {b.NamaKetua}?\nKuota tanggal tersebut akan dilepas."))
                return;
            await _repo.UpdateStatusAsync(b.BookingId, to);
            Toast.Success($"Booking {b.KodeBooking} sekarang berstatus {to}.");
            await LoadAsync();
        }, p => Selected != null && BookingRules.CanTransition(Selected.Status, (string)p));

        DeleteCommand = new AsyncCommand(async _ =>
        {
            var b = Selected;
            if (!DialogService.Confirm("Hapus booking", $"Hapus permanen booking {b.KodeBooking} a.n. {b.NamaKetua}?")) return;
            await _repo.DeleteAsync(b.BookingId);
            Toast.Success($"Booking {b.KodeBooking} dihapus.");
            await LoadAsync();
        }, _ => Selected != null && Session.IsAdmin && BookingRules.IsDeletable(Selected.Status));

        NextCommand = new AsyncCommand(async _ => { _page++; await LoadAsync(); }, _ => _page < TotalPages);
        PrevCommand = new AsyncCommand(async _ => { _page--; await LoadAsync(); }, _ => _page > 1);
        RefreshCommand = new AsyncCommand(_ => LoadAsync());
    }

    async Task ReloadAsync()
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;
        try
        {
            await Task.Delay(350, token);
            _page = 1;
            await LoadAsync();
        }
        catch (TaskCanceledException) { }
        catch (Exception ex) { ErrorHandler.Handle(ex); }
    }

    public async Task LoadAsync()
    {
        OnPropertyChanged(nameof(IsAdmin));
        var ver = ++_version;
        IsLoading = true;
        try
        {
            var (items, total) = await _repo.SearchAsync(Search.Trim(), Status == "Semua" ? "" : Status, _page, PageSize);
            if (ver != _version) return;

            Items.Clear();
            foreach (var b in items) Items.Add(b);
            _total = total;
            IsEmpty = items.Count == 0;
            Selected = Items.FirstOrDefault();
            OnPropertyChanged(nameof(PageInfo));
            OnPropertyChanged(nameof(PageLabel));
            OnPropertyChanged(nameof(TotalPages));
        }
        catch (Exception ex)
        {
            if (ver == _version) ErrorHandler.Handle(ex);
        }
        finally
        {
            if (ver == _version) IsLoading = false;
        }
    }
}