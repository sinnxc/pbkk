using System.Drawing;
using System.Windows.Forms;

namespace SistemDataMahasiswa;

public class FormUtama : Form
{
    private TextBox txtNim = default!;
    private TextBox txtNama = default!;
    private TextBox txtProdi = default!;
    private TextBox txtIpk = default!;

    private TextBox txtCari = default!;

    private DataGridView grid = default!;

    public FormUtama()
    {
        InitializeUi();
        DatabaseHelper.Inisialisasi();
        MuatData();
    }

    private void InitializeUi()
    {
        Text = "Sistem Data Mahasiswa";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(920, 560);
        Font = new Font("Segoe UI", 9F);
        Padding = new Padding(12);

        var lblJudul = new Label
        {
            Text = "Data Mahasiswa",
            Font = new Font("Segoe UI", 13F, FontStyle.Bold),
            Dock = DockStyle.Top,
            Height = 36,
            TextAlign = ContentAlignment.MiddleLeft
        };

        var panelInput = new GroupBox
        {
            Text = "Input Data Mahasiswa",
            Dock = DockStyle.Top,
            Height = 88,
            Padding = new Padding(10)
        };

        txtNim = BuatField(panelInput, "NIM:", 15, 22, 110);
        txtNama = BuatField(panelInput, "Nama:", 140, 22, 230);
        txtProdi = BuatField(panelInput, "Program Studi:", 385, 22, 170);
        txtIpk = BuatField(panelInput, "IPK:", 570, 22, 70);

        var btnTambah = new Button
        {
            Text = "Tambah",
            Location = new Point(655, 41),
            Size = new Size(80, 26),
            UseVisualStyleBackColor = true
        };
        btnTambah.Click += (_, _) => TambahMahasiswa();
        panelInput.Controls.Add(btnTambah);

        var btnResetInput = new Button
        {
            Text = "Batal",
            Location = new Point(745, 41),
            Size = new Size(75, 26),
            UseVisualStyleBackColor = true
        };
        btnResetInput.Click += (_, _) =>
        {
            txtNim.Clear();
            txtNama.Clear();
            txtProdi.Clear();
            txtIpk.Clear();
            txtNim.Focus();
        };
        panelInput.Controls.Add(btnResetInput);

        var panelCari = new GroupBox
        {
            Text = "Pencarian & Aksi",
            Dock = DockStyle.Top,
            Height = 72,
            Padding = new Padding(10)
        };

        panelCari.Controls.Add(new Label
        {
            Text = "Cari NIM:",
            Location = new Point(15, 30),
            AutoSize = true
        });

        txtCari = new TextBox
        {
            Location = new Point(80, 27),
            Width = 190
        };
        panelCari.Controls.Add(txtCari);

        var btnCari = new Button
        {
            Text = "Cari",
            Location = new Point(280, 25),
            Size = new Size(75, 26),
            UseVisualStyleBackColor = true
        };
        btnCari.Click += (_, _) => CariMahasiswa();
        panelCari.Controls.Add(btnCari);

        var btnRefresh = new Button
        {
            Text = "Refresh",
            Location = new Point(362, 25),
            Size = new Size(75, 26),
            UseVisualStyleBackColor = true
        };
        btnRefresh.Click += (_, _) =>
        {
            txtCari.Clear();
            MuatData();
        };
        panelCari.Controls.Add(btnRefresh);

        var btnHapus = new Button
        {
            Text = "Hapus",
            Location = new Point(444, 25),
            Size = new Size(75, 26),
            UseVisualStyleBackColor = true
        };
        btnHapus.Click += (_, _) => HapusMahasiswa();
        panelCari.Controls.Add(btnHapus);

        grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = SystemColors.Window,
            BorderStyle = BorderStyle.Fixed3D,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            RowTemplate = { Height = 26 },
            ColumnHeadersHeight = 28,
            EnableHeadersVisualStyles = true
        };

        grid.CellClick += (_, e) =>
        {
            if (e.RowIndex >= 0 && grid.Rows[e.RowIndex].DataBoundItem is Mahasiswa m)
            {
                txtCari.Text = m.NIM;
            }
        };

        Controls.Add(grid);
        Controls.Add(panelCari);
        Controls.Add(panelInput);
        Controls.Add(lblJudul);
    }

    private TextBox BuatField(GroupBox parent, string label, int x, int y, int textBoxWidth)
    {
        var lbl = new Label
        {
            Text = label,
            Location = new Point(x, y),
            AutoSize = true
        };

        var textBox = new TextBox
        {
            Location = new Point(x, y + 20),
            Width = textBoxWidth
        };

        parent.Controls.Add(lbl);
        parent.Controls.Add(textBox);
        return textBox;
    }

    private void MuatData()
    {
        grid.DataSource = null;
        var data = DatabaseHelper.AmbilSemua();
        grid.DataSource = data;
        AturKolomGrid();
    }

    private void AturKolomGrid()
    {
        if (grid.Columns.Count == 0) return;

        if (grid.Columns["NIM"] != null)
        {
            grid.Columns["NIM"].HeaderText = "NIM";
            grid.Columns["NIM"].FillWeight = 20;
        }
        if (grid.Columns["Nama"] != null)
        {
            grid.Columns["Nama"].HeaderText = "Nama Mahasiswa";
            grid.Columns["Nama"].FillWeight = 40;
        }
        if (grid.Columns["Prodi"] != null)
        {
            grid.Columns["Prodi"].HeaderText = "Program Studi";
            grid.Columns["Prodi"].FillWeight = 25;
        }
        if (grid.Columns["IPK"] != null)
        {
            grid.Columns["IPK"].HeaderText = "IPK";
            grid.Columns["IPK"].FillWeight = 15;
            grid.Columns["IPK"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }
    }

    private void TambahMahasiswa()
    {
        string nim = txtNim.Text.Trim();
        string nama = txtNama.Text.Trim();
        string prodi = txtProdi.Text.Trim();
        string ipkText = txtIpk.Text.Trim();

        if (nim.Length == 0 || nama.Length == 0 || prodi.Length == 0 || ipkText.Length == 0)
        {
            MessageBox.Show("Semua kolom harus diisi.", "Peringatan",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!double.TryParse(ipkText, out double ipk) || ipk < 0 || ipk > 4)
        {
            MessageBox.Show("IPK harus berupa angka 0 - 4.", "Peringatan",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            DatabaseHelper.Tambah(new Mahasiswa
            {
                NIM = nim,
                Nama = nama,
                Prodi = prodi,
                IPK = ipk
            });

            MessageBox.Show("Data mahasiswa berhasil ditambahkan.", "Sukses",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            txtNim.Clear();
            txtNama.Clear();
            txtProdi.Clear();
            txtIpk.Clear();

            MuatData();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Gagal menambahkan: " + ex.Message, "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void CariMahasiswa()
    {
        string nim = txtCari.Text.Trim();
        if (nim.Length == 0)
        {
            MuatData();
            return;
        }

        var hasil = DatabaseHelper.Cari(nim);
        if (hasil is null)
        {
            MessageBox.Show("Mahasiswa dengan NIM tersebut tidak ditemukan.", "Hasil",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            MuatData();
            return;
        }

        grid.DataSource = null;
        grid.DataSource = new List<Mahasiswa> { hasil };
        AturKolomGrid();
    }

    private void HapusMahasiswa()
    {
        string nim = txtCari.Text.Trim();
        if (nim.Length == 0)
        {
            MessageBox.Show("Masukkan NIM yang ingin dihapus terlebih dahulu.", "Peringatan",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var konfirmasi = MessageBox.Show(
            $"Yakin ingin menghapus mahasiswa dengan NIM {nim}?",
            "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        if (konfirmasi != DialogResult.Yes)
        {
            return;
        }

        if (DatabaseHelper.Hapus(nim))
        {
            MessageBox.Show("Data mahasiswa berhasil dihapus.", "Sukses",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            txtCari.Clear();
            MuatData();
        }
        else
        {
            MessageBox.Show("Data mahasiswa tidak ditemukan.", "Hasil",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}