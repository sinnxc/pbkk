using System.Windows;
using StudentManagementApp.Repositories;
using StudentManagementApp.ViewModels;

namespace StudentManagementApp;

public partial class MainWindow : Window
{
    private readonly StudentViewModel viewModel;

    public MainWindow()
    {
        InitializeComponent();
        var connection = Environment.GetEnvironmentVariable("WEEK5_SQL_CONNECTION");
        IStudentRepository repository = string.IsNullOrWhiteSpace(connection)
            ? new DemoStudentRepository() : new StudentRepository(connection);
        var description = string.IsNullOrWhiteSpace(connection)
            ? "Mode latihan (memori). Data hilang saat aplikasi ditutup."
            : "Mode SQL Server. Data tersimpan di tabel dbo.Mahasiswa.";
        viewModel = new StudentViewModel(repository, description,
            student => MessageBox.Show(this, $"Hapus {student.NIM} - {student.Nama}?", "Konfirmasi Hapus",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes);
        DataContext = viewModel;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e) => await viewModel.LoadData();
}
