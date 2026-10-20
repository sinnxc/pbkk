using System.Text.RegularExpressions;
using System.Windows.Input;
using Sibumi.Core;
using Sibumi.Data;
using Sibumi.Models;

namespace Sibumi.ViewModels;

public class BookingFormViewModel : ViewModelBase
{
    readonly BookingRepository _bookings = new();
    readonly JalurRepository _jalurRepo = new();
    readonly Booking _existing;

    List<Gunung> _gunungs = new();
    Gunung _selectedGunung;
    List<Jalur> _allJalurs = new();
    List<Jalur> _filteredJalurs = new();
    Jalur _jalur;
    string _nama = "", _nik = "", _hp = "", _email = "", _jumlah = "1", _error;
    DateTime? _naik = DateTime.Today.AddDays(1), _turun = DateTime.Today.AddDays(2);
    bool _busy;

    public BookingFormViewModel(Booking existing)
    {
        _existing = existing;
        if (existing != null)
        {
            _nama = existing.NamaKetua; _nik = existing.NIK; _hp = existing.NoHp; _email = existing.Email ?? "";
            _jumlah = existing.JumlahAnggota.ToString();
            _naik = existing.TanggalNaik; _turun = existing.TanggalTurun;
        }
        SaveCommand = new AsyncCommand(async _ => await SaveAsync());
    }

    public string Title => _existing == null ? "Booking Pendakian Baru" : $"Ubah Booking {_existing.KodeBooking}";
    public List<Gunung> Gunungs { get => _gunungs; private set => Set(ref _gunungs, value); }
    public Gunung SelectedGunung
    {
        get => _selectedGunung;
        set
        {
            if (Set(ref _selectedGunung, value))
            {
                OnPropertyChanged(nameof(IsGunungDitutup));
                OnPropertyChanged(nameof(GunungStatusWarning));
                UpdateJalursForSelectedGunung();
            }
        }
    }

    public bool IsGunungDitutup => SelectedGunung != null && !SelectedGunung.BisaDidaki;
    public string GunungStatusWarning => SelectedGunung != null && !SelectedGunung.BisaDidaki
        ? $"{SelectedGunung.KeteranganStatus}. Pemesanan tiket untuk gunung ini dinonaktifkan."
        : "";

    public List<Jalur> Jalurs { get => _filteredJalurs; private set => Set(ref _filteredJalurs, value); }
    public Jalur SelectedJalur
    {
        get => _jalur;
        set { if (Set(ref _jalur, value)) { OnPropertyChanged(nameof(JalurInfo)); OnPropertyChanged(nameof(Estimasi)); } }
    }
    public string NamaKetua { get => _nama; set => Set(ref _nama, value); }
    public string NIK { get => _nik; set => Set(ref _nik, value); }
    public string NoHp { get => _hp; set => Set(ref _hp, value); }
    public string Email { get => _email; set => Set(ref _email, value); }
    public string JumlahText { get => _jumlah; set { if (Set(ref _jumlah, value)) OnPropertyChanged(nameof(Estimasi)); } }
    public DateTime? TanggalNaik { get => _naik; set { if (Set(ref _naik, value) && value.HasValue && (!_turun.HasValue || _turun < value)) TanggalTurun = value; } }
    public DateTime? TanggalTurun { get => _turun; set => Set(ref _turun, value); }
    public string ErrorMessage { get => _error; set { Set(ref _error, value); OnPropertyChanged(nameof(HasError)); } }
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool IsBusy { get => _busy; set => Set(ref _busy, value); }

    public string JalurInfo => SelectedJalur == null ? "" :
        $"Kuota harian {SelectedJalur.KuotaHarian} orang, tarif {Fmt.Rp(SelectedJalur.TarifPerOrang)}/orang";
    public string Estimasi => SelectedJalur != null && int.TryParse(JumlahText, out var n) && n > 0
        ? Fmt.Rp(SelectedJalur.TarifPerOrang * n) : "-";

    public ICommand SaveCommand { get; }
    public event Action<bool> RequestClose;

    public async Task InitAsync()
    {
        Gunungs = await _jalurRepo.GetGunungAsync();
        _allJalurs = await _jalurRepo.GetAllAsync(activeOnly: false);

        if (_existing != null)
        {
            var j = _allJalurs.FirstOrDefault(x => x.JalurId == _existing.JalurId);
            SelectedGunung = Gunungs.FirstOrDefault(g => g.GunungId == j?.GunungId) ?? Gunungs.FirstOrDefault();
            SelectedJalur = Jalurs.FirstOrDefault(x => x.JalurId == _existing.JalurId);
        }
        else
        {
            // Pilih gunung pertama yang BISA DIDAKI secara default
            SelectedGunung = Gunungs.FirstOrDefault(g => g.BisaDidaki) ?? Gunungs.FirstOrDefault();
        }
    }

    void UpdateJalursForSelectedGunung()
    {
        if (SelectedGunung == null || !SelectedGunung.BisaDidaki)
        {
            Jalurs = new List<Jalur>();
            SelectedJalur = null;
        }
        else
        {
            Jalurs = _allJalurs.Where(j => j.GunungId == SelectedGunung.GunungId && j.IsActive).ToList();
            SelectedJalur = Jalurs.FirstOrDefault();
        }
    }

    bool Validate(out int jumlah)
    {
        var errs = new List<string>();
        jumlah = 0;
        if (SelectedGunung == null) errs.Add("Pilih gunung tujuan pendakian.");
        else if (!SelectedGunung.BisaDidaki) errs.Add($"Gunung {SelectedGunung.NamaGunung} saat ini sedang DITUTUP dan tidak dapat didaki.");
        if (SelectedJalur == null) errs.Add("Pilih jalur pendakian yang tersedia.");
        if (NamaKetua.Trim().Length < 3) errs.Add("Nama ketua rombongan minimal 3 karakter.");
        if (!Regex.IsMatch(NIK.Trim(), @"^\d{16}$")) errs.Add("NIK harus 16 digit angka.");
        if (!Regex.IsMatch(NoHp.Trim().Replace(" ", ""), @"^(\+62|62|0)8\d{7,12}$")) errs.Add("Nomor HP tidak valid (contoh: 081234567890).");
        if (!string.IsNullOrWhiteSpace(Email) && !Regex.IsMatch(Email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) errs.Add("Format email tidak valid.");
        if (!int.TryParse(JumlahText, out jumlah) || jumlah < 1 || jumlah > 15) errs.Add("Jumlah anggota harus 1 sampai 15 orang.");
        if (!TanggalNaik.HasValue || !TanggalTurun.HasValue) errs.Add("Tanggal naik dan turun wajib diisi.");
        else
        {
            if (_existing == null && TanggalNaik.Value.Date < DateTime.Today) errs.Add("Tanggal naik tidak boleh di masa lalu.");
            if (TanggalTurun.Value.Date < TanggalNaik.Value.Date) errs.Add("Tanggal turun tidak boleh sebelum tanggal naik.");
            else if ((TanggalTurun.Value.Date - TanggalNaik.Value.Date).Days > 7) errs.Add("Durasi pendakian maksimal 7 hari.");
        }
        ErrorMessage = errs.Count == 0 ? null : string.Join(Environment.NewLine, errs.Select(e => "- " + e));
        return errs.Count == 0;
    }

    async Task SaveAsync()
    {
        if (!Validate(out var jumlah)) return;
        IsBusy = true;
        try
        {
            var b = new Booking
            {
                BookingId = _existing?.BookingId ?? 0,
                KodeBooking = _existing?.KodeBooking,
                JalurId = SelectedJalur.JalurId,
                NamaKetua = NamaKetua.Trim(),
                NIK = NIK.Trim(),
                NoHp = NoHp.Trim().Replace(" ", ""),
                Email = string.IsNullOrWhiteSpace(Email) ? null : Email.Trim(),
                JumlahAnggota = jumlah,
                TanggalNaik = TanggalNaik.Value.Date,
                TanggalTurun = TanggalTurun.Value.Date
            };
            await _bookings.SaveAsync(b);
            Toast.Success(_existing == null ? $"Booking {b.KodeBooking} berhasil dibuat." : "Perubahan booking disimpan.");
            RequestClose?.Invoke(true);
        }
        catch (BusinessException ex) { ErrorMessage = ex.Message; }   // misalnya kuota penuh, pesannya tampil di form
        finally { IsBusy = false; }
    }
}