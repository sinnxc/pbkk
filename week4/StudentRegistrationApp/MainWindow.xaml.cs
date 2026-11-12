using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace StudentRegistrationApp;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<Mahasiswa> daftar = new();
    private Mahasiswa? sedangDiedit;

    public MainWindow()
    {
        InitializeComponent();
        TampilkanData();
    }

    private bool Tolak(string pesan, Control kontrol)
    {
        MessageBox.Show(this, pesan, "Validasi", MessageBoxButton.OK, MessageBoxImage.Warning);
        kontrol.Focus();
        return false;
    }

    private bool Validasi()
    {
        if (string.IsNullOrWhiteSpace(txtNim.Text)) return Tolak("NIM harus diisi!", txtNim);
        if (string.IsNullOrWhiteSpace(txtNama.Text)) return Tolak("Nama harus diisi!", txtNama);
        if (cmbProdi.SelectedItem is not ComboBoxItem) return Tolak("Pilih program studi!", cmbProdi);
        if (rbLaki.IsChecked != true && rbPerempuan.IsChecked != true) return Tolak("Pilih jenis kelamin!", rbLaki);
        if (daftar.Any(p => p != sedangDiedit && string.Equals(p.Nim, txtNim.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
            return Tolak("NIM sudah terdaftar!", txtNim);
        return true;
    }

    private void BtnSimpan_Click(object sender, RoutedEventArgs e)
    {
        if (!Validasi()) return;
        var mahasiswa = new Mahasiswa
        {
            Nim = txtNim.Text.Trim(), Nama = txtNama.Text.Trim(),
            Prodi = ((ComboBoxItem)cmbProdi.SelectedItem).Content.ToString()!,
            JenisKelamin = rbLaki.IsChecked == true ? "Laki-laki" : "Perempuan"
        };
        bool edit = sedangDiedit != null;
        if (sedangDiedit != null) daftar[daftar.IndexOf(sedangDiedit)] = mahasiswa;
        else daftar.Add(mahasiswa);
        ResetForm();
        txtCari.Clear();
        TampilkanData();
        MessageBox.Show(this, edit ? "Data mahasiswa berhasil diperbarui!" : "Data mahasiswa berhasil disimpan!",
            "Informasi", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ResetForm(bool fokus = true)
    {
        sedangDiedit = null;
        lstMahasiswa.SelectedItem = null;
        txtNim.Clear(); txtNama.Clear();
        cmbProdi.SelectedIndex = -1;
        rbLaki.IsChecked = rbPerempuan.IsChecked = false;
        if (fokus) txtNim.Focus();
    }

    private void BtnReset_Click(object sender, RoutedEventArgs e) => ResetForm();

    private void BtnHapus_Click(object sender, RoutedEventArgs e)
    {
        if (lstMahasiswa.SelectedItem is not Mahasiswa mahasiswa)
        {
            Tolak("Pilih data yang ingin dihapus!", lstMahasiswa);
            return;
        }
        if (MessageBox.Show(this, $"Hapus mahasiswa {mahasiswa.Nim} - {mahasiswa.Nama}?", "Konfirmasi Hapus",
            MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes) return;
        daftar.Remove(mahasiswa);
        ResetForm();
        TampilkanData();
    }

    private void LstMahasiswa_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (lstMahasiswa.SelectedItem is not Mahasiswa mahasiswa) return;
        sedangDiedit = mahasiswa;
        txtNim.Text = mahasiswa.Nim; txtNama.Text = mahasiswa.Nama;
        cmbProdi.SelectedItem = cmbProdi.Items.Cast<ComboBoxItem>()
            .First(item => item.Content.ToString() == mahasiswa.Prodi);
        rbLaki.IsChecked = mahasiswa.JenisKelamin == "Laki-laki";
        rbPerempuan.IsChecked = mahasiswa.JenisKelamin == "Perempuan";
    }

    private void TxtCari_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (lstMahasiswa == null) return;
        ResetForm(false);
        TampilkanData();
    }

    private void TampilkanData()
    {
        string cari = txtCari.Text.Trim();
        var hasil = daftar.Where(p => p.Nim.Contains(cari, StringComparison.OrdinalIgnoreCase)
            || p.Nama.Contains(cari, StringComparison.OrdinalIgnoreCase)).ToList();
        lstMahasiswa.ItemsSource = hasil;
        txtJumlah.Text = $"Jumlah Mahasiswa: {daftar.Count} (tampil: {hasil.Count})";
        txtKosong.Text = daftar.Count == 0 ? "Belum ada data mahasiswa." : "Mahasiswa tidak ditemukan.";
        txtKosong.Visibility = hasil.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }
}



