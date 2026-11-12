using System.Windows;
using StudentRegistration.Repositories;

namespace StudentRegistration;

public partial class MainWindow : Window
{
    private readonly StudentRepository _studentRepository = new();
    private bool _isLoading;

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e) => await LoadStudents();

    private async void RefreshButton_Click(object sender, RoutedEventArgs e) => await LoadStudents();

    private async Task LoadStudents()
    {
        if (_isLoading) return;
        _isLoading = true;
        RefreshButton.IsEnabled = false;
        StatusText.Text = "Memuat data dari SQL Server...";
        try
        {
            var snapshot = await Task.Run(() => _studentRepository.GetSnapshot());
            StudentDataGrid.ItemsSource = snapshot.Students;
            TotalText.Text = $"Total Mahasiswa: {snapshot.Total}";
            EmptyState.Visibility = snapshot.Total == 0 ? Visibility.Visible : Visibility.Collapsed;
            StatusText.Text = $"Data SQL Server dimuat. {snapshot.Students.Count} mahasiswa. Diperbarui {DateTime.Now:HH:mm:ss}.";
        }
        catch (Exception ex)
        {
            StatusText.Text = "Gagal memuat data. Periksa instance SQL Server, database, dan koneksi, lalu klik Refresh. Data terakhir tetap ditampilkan.";
            MessageBox.Show(this, ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            RefreshButton.IsEnabled = true;
            _isLoading = false;
        }
    }
}
