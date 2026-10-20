using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using Sibumi.Core;
using Sibumi.Data;
using Sibumi.Models;

namespace Sibumi.ViewModels;

public class JalurViewModel : ViewModelBase, IPage
{
    readonly JalurRepository _repo = new();
    Jalur _selected;
    int _id;
    Gunung _gunung;
    string _nama = "", _kuota = "", _tarif = "", _error;
    bool _aktif = true;

    public ObservableCollection<Jalur> Items { get; } = new();
    public List<Gunung> Gunungs { get; private set; } = new();

    public Jalur Selected { get => _selected; set { if (Set(ref _selected, value)) Fill(value); } }
    public Gunung FGunung { get => _gunung; set => Set(ref _gunung, value); }
    public string FNama { get => _nama; set => Set(ref _nama, value); }
    public string FKuota { get => _kuota; set => Set(ref _kuota, value); }
    public string FTarif { get => _tarif; set => Set(ref _tarif, value); }
    public bool FAktif { get => _aktif; set => Set(ref _aktif, value); }
    public string Error { get => _error; set { Set(ref _error, value); OnPropertyChanged(nameof(HasError)); } }
    public bool HasError => !string.IsNullOrEmpty(Error);
    public bool IsEditing => _id != 0;
    public string FormTitle => IsEditing ? "Ubah Jalur" : "Jalur Baru";

    public ICommand NewCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand DeleteCommand { get; }

    public JalurViewModel()
    {
        NewCommand = new RelayCommand(_ => { Selected = null; Reset(); });

        SaveCommand = new AsyncCommand(async _ =>
        {
            if (!Validate(out var kuota, out var tarif)) return;
            await _repo.SaveAsync(new Jalur
            {
                JalurId = _id,
                GunungId = FGunung.GunungId,
                NamaJalur = FNama.Trim(),
                KuotaHarian = kuota,
                TarifPerOrang = tarif,
                IsActive = FAktif
            });
            Toast.Success("Data jalur disimpan.");
            await LoadAsync();
        });

        DeleteCommand = new AsyncCommand(async _ =>
        {
            var j = Selected;
            if (!DialogService.Confirm("Hapus jalur", $"Hapus jalur {j.NamaJalur}?")) return;
            await _repo.DeleteAsync(j);
            Toast.Success("Jalur dihapus.");
            await LoadAsync();
        }, _ => Selected != null);
    }

    public async Task LoadAsync()
    {
        Gunungs = await _repo.GetGunungAsync();
        OnPropertyChanged(nameof(Gunungs));
        var list = await _repo.GetAllAsync();
        Items.Clear();
        foreach (var j in list) Items.Add(j);
        Selected = null;
        Reset();
    }

    void Reset()
    {
        _id = 0; FGunung = Gunungs.FirstOrDefault(); FNama = ""; FKuota = ""; FTarif = ""; FAktif = true; Error = null;
        OnPropertyChanged(nameof(IsEditing)); OnPropertyChanged(nameof(FormTitle));
    }

    void Fill(Jalur j)
    {
        if (j == null) return;
        _id = j.JalurId;
        FGunung = Gunungs.FirstOrDefault(g => g.GunungId == j.GunungId);
        FNama = j.NamaJalur; FKuota = j.KuotaHarian.ToString(); FTarif = j.TarifPerOrang.ToString("0.##", Fmt.Id);
        FAktif = j.IsActive; Error = null;
        OnPropertyChanged(nameof(IsEditing)); OnPropertyChanged(nameof(FormTitle));
    }

    bool Validate(out int kuota, out decimal tarif)
    {
        var errs = new List<string>();
        tarif = 0;
        if (FGunung == null) errs.Add("Pilih gunung.");
        if (FNama.Trim().Length < 3) errs.Add("Nama jalur minimal 3 karakter.");
        if (!int.TryParse(FKuota, out kuota) || kuota < 1) errs.Add("Kuota harian harus angka positif.");
        if (!decimal.TryParse(FTarif, NumberStyles.Number, Fmt.Id, out tarif) || tarif < 0) errs.Add("Tarif harus berupa angka (contoh: 25000).");
        Error = errs.Count == 0 ? null : string.Join(Environment.NewLine, errs.Select(e => "- " + e));
        return errs.Count == 0;
    }
}