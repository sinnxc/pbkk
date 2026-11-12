using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using StudentManagementApp.Models;
using StudentManagementApp.Repositories;

namespace StudentManagementApp.ViewModels;

public sealed class StudentViewModel : INotifyPropertyChanged
{
    private readonly IStudentRepository repository;
    private readonly Func<Student, bool> confirmDelete;
    private string nim = "", nama = "", email = "", status = "Belum memuat data.";
    private string? jurusan, gender;
    private Student? selectedStudent;
    private bool isBusy;

    public StudentViewModel(IStudentRepository repository, string storageDescription, Func<Student, bool> confirmDelete)
    {
        this.repository = repository;
        this.confirmDelete = confirmDelete;
        StorageDescription = storageDescription;
        LoadCommand = new AsyncCommand(LoadData, () => !IsBusy);
        SaveCommand = new AsyncCommand(Save, () => !IsBusy);
        DeleteCommand = new AsyncCommand(Delete, () => !IsBusy && SelectedStudent != null);
        ResetCommand = new AsyncCommand(() => { ResetForm(); Status = "Form dikosongkan."; return Task.CompletedTask; }, () => !IsBusy);
        Students.CollectionChanged += (_, _) => RefreshStatistics();
    }

    public ObservableCollection<Student> Students { get; } = [];
    public string[] JurusanOptions { get; } = ["Informatika", "Sistem Informasi"];
    public string[] GenderOptions { get; } = ["Laki-laki", "Perempuan"];
    public string StorageDescription { get; }
    public int TotalStudents => Students.Count;
    public int TotalInformatika => Students.Count(x => x.Jurusan == "Informatika");
    public int TotalSistemInformasi => Students.Count(x => x.Jurusan == "Sistem Informasi");
    public int TotalLakiLaki => Students.Count(x => x.Gender == "Laki-laki");
    public int TotalPerempuan => Students.Count(x => x.Gender == "Perempuan");
    public bool IsEmpty => Students.Count == 0;
    public string NIM { get => nim; set { nim = value; OnPropertyChanged(); } }
    public string Nama { get => nama; set { nama = value; OnPropertyChanged(); } }
    public string Email { get => email; set { email = value; OnPropertyChanged(); } }
    public string? Jurusan { get => jurusan; set { jurusan = value; OnPropertyChanged(); } }
    public string? Gender { get => gender; set { gender = value; OnPropertyChanged(); } }
    public string Status { get => status; private set { status = value; OnPropertyChanged(); } }
    public Student? SelectedStudent
    {
        get => selectedStudent;
        set { selectedStudent = value; OnPropertyChanged(); DeleteCommand.NotifyCanExecuteChanged(); }
    }
    public bool IsBusy
    {
        get => isBusy;
        private set
        {
            isBusy = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsReady));
            foreach (var command in new[] { LoadCommand, SaveCommand, DeleteCommand, ResetCommand })
                command.NotifyCanExecuteChanged();
        }
    }
    public bool IsReady => !IsBusy;
    public AsyncCommand LoadCommand { get; }
    public AsyncCommand SaveCommand { get; }
    public AsyncCommand DeleteCommand { get; }
    public AsyncCommand ResetCommand { get; }

    public async Task LoadData() => await Run(async () =>
    {
        await Reload();
        Status = $"Data dimuat. Total {TotalStudents} mahasiswa.";
    });

    private async Task Reload()
    {
        // Fetch first so a failed connection does not erase the last successful view.
        var loaded = await repository.GetAll();
        SelectedStudent = null;
        Students.Clear();
        foreach (var student in loaded) Students.Add(student);
        RefreshStatistics();
    }

    private async Task Save()
    {
        if (string.IsNullOrWhiteSpace(NIM)) { Status = "NIM harus diisi."; return; }
        if (string.IsNullOrWhiteSpace(Nama)) { Status = "Nama harus diisi."; return; }
        if (Jurusan == null || !JurusanOptions.Contains(Jurusan)) { Status = "Pilih jurusan."; return; }
        if (Gender == null || !GenderOptions.Contains(Gender)) { Status = "Pilih gender."; return; }
        if (NIM.Trim().Length > 20 || Nama.Trim().Length > 100 || Email.Trim().Length > 100)
        { Status = "Batas panjang: NIM 20, nama 100, dan email 100 karakter."; return; }
        if (!string.IsNullOrWhiteSpace(Email) &&
            (!MailAddress.TryCreate(Email.Trim(), out var address) || address.Address != Email.Trim()))
        { Status = "Isi alamat email yang valid."; return; }
        if (Students.Any(x => string.Equals(x.NIM, NIM.Trim(), StringComparison.OrdinalIgnoreCase)))
        { Status = "NIM sudah terdaftar."; return; }
        await Run(async () =>
        {
            await repository.Insert(new Student
            {
                NIM = NIM.Trim(), Nama = Nama.Trim(), Jurusan = Jurusan!, Gender = Gender!, Email = Email.Trim()
            });
            ResetForm();
            Status = "Data tersimpan. Memuat ulang statistik...";
            try { await Reload(); Status = "Data mahasiswa berhasil disimpan. Statistik diperbarui."; }
            catch { Status = "Data tersimpan, tetapi pemuatan ulang gagal. Klik Muat Ulang untuk menyinkronkan statistik."; }
        });
    }

    private async Task Delete()
    {
        var student = SelectedStudent;
        if (student == null) return;
        if (!confirmDelete(student)) { Status = "Penghapusan dibatalkan."; return; }
        await Run(async () =>
        {
            await repository.Delete(student.Id);
            Status = "Data dihapus. Memuat ulang statistik...";
            try { await Reload(); Status = "Data mahasiswa berhasil dihapus. Statistik diperbarui."; }
            catch { Status = "Data dihapus, tetapi pemuatan ulang gagal. Klik Muat Ulang untuk menyinkronkan statistik."; }
        });
    }

    private async Task Run(Func<Task> action)
    {
        if (IsBusy) return;
        IsBusy = true;
        Status = "Memproses data...";
        try { await action(); }
        catch (InvalidOperationException ex) { Status = ex.Message; }
        catch { Status = "Operasi gagal. Periksa SQL Server, database, dan connection string, lalu coba lagi."; }
        finally { IsBusy = false; }
    }

    private void ResetForm()
    {
        NIM = Nama = Email = "";
        Jurusan = Gender = null;
        SelectedStudent = null;
    }

    private void RefreshStatistics()
    {
        OnPropertyChanged(nameof(TotalStudents));
        OnPropertyChanged(nameof(TotalInformatika));
        OnPropertyChanged(nameof(TotalSistemInformasi));
        OnPropertyChanged(nameof(TotalLakiLaki));
        OnPropertyChanged(nameof(TotalPerempuan));
        OnPropertyChanged(nameof(IsEmpty));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
