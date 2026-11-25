using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using StudentRegistrationApp.Models;
using StudentRegistrationApp.Services;

namespace StudentRegistrationApp
{
    public partial class MainWindow : Window
    {
        private readonly StudentRepository _repository = new();
        private Student? _selectedStudent;

        public MainWindow()
        {
            InitializeComponent();

            cbProdi.ItemsSource = new[]
            {
                "Teknik Informatika", "Sistem Informasi", "Teknik Elektro",
                "Teknik Industri", "Manajemen", "Akuntansi"
            };

            LoadData();
        }

        // ===================== LOAD / SEARCH =====================

        private void LoadData(string? keyword = null)
        {
            var data = _repository.Search(keyword ?? string.Empty);
            dgStudents.ItemsSource = data;

            txtStatus.Text = string.IsNullOrWhiteSpace(keyword)
                ? $"Total mahasiswa: {data.Count}"
                : $"Ditemukan {data.Count} data untuk \"{keyword}\"";
        }

        private void BtnCari_Click(object sender, RoutedEventArgs e) => LoadData(txtCari.Text);

        private void TxtCari_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) LoadData(txtCari.Text);
        }

        // ===================== CREATE =====================

        private void BtnSimpan_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm(null)) return;

            var student = ReadForm(new Student());
            _repository.Add(student);

            LoadData(txtCari.Text);
            ClearForm();
            txtStatus.Text = $"Data {student.Nama} berhasil disimpan.";
        }

        // ===================== UPDATE =====================

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedStudent == null)
            {
                MessageBox.Show("Pilih data pada tabel terlebih dahulu.", "Informasi",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!ValidateForm(_selectedStudent.Id)) return;

            var updated = ReadForm(new Student { Id = _selectedStudent.Id });
            _repository.Update(updated);

            LoadData(txtCari.Text);
            ClearForm();
            txtStatus.Text = $"Data {updated.Nama} berhasil diperbarui.";
        }

        // ===================== DELETE =====================

        private void BtnHapus_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedStudent == null) return;

            var confirm = MessageBox.Show(
                $"Hapus data \"{_selectedStudent.Nama}\" ({_selectedStudent.Nim})?",
                "Konfirmasi Hapus", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            string nama = _selectedStudent.Nama;
            _repository.Delete(_selectedStudent.Id);

            LoadData(txtCari.Text);
            ClearForm();
            txtStatus.Text = $"Data {nama} berhasil dihapus.";
        }

        // ===================== RESET =====================

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            txtCari.Clear();
            LoadData();
            txtStatus.Text = "Form dan pencarian direset.";
        }

        // ===================== SELECTION =====================

        private void DgStudents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgStudents.SelectedItem is not Student s) return;

            _selectedStudent = s;

            txtNim.Text = s.Nim;
            txtNama.Text = s.Nama;
            rbLaki.IsChecked = s.JenisKelamin == "Laki-laki";
            rbPerempuan.IsChecked = s.JenisKelamin == "Perempuan";
            dpTanggalLahir.SelectedDate = s.TanggalLahir;
            cbProdi.SelectedItem = s.ProgramStudi;
            txtEmail.Text = s.Email;
            txtNoHp.Text = s.NoHp;
            txtAlamat.Text = s.Alamat;

            SetEditMode(true);
        }

        // ===================== HELPERS =====================

        private void SetEditMode(bool editing)
        {
            btnSimpan.IsEnabled = !editing;
            btnEdit.IsEnabled = editing;
            btnHapus.IsEnabled = editing;
            txtFormTitle.Text = editing ? "Edit Data Mahasiswa" : "Form Mahasiswa Baru";
        }

        private void ClearForm()
        {
            _selectedStudent = null;
            dgStudents.SelectedItem = null;

            txtNim.Clear();
            txtNama.Clear();
            rbLaki.IsChecked = false;
            rbPerempuan.IsChecked = false;
            dpTanggalLahir.SelectedDate = null;
            cbProdi.SelectedIndex = -1;
            txtEmail.Clear();
            txtNoHp.Clear();
            txtAlamat.Clear();

            SetEditMode(false);
            txtNim.Focus();
        }

        private Student ReadForm(Student target)
        {
            target.Nim = txtNim.Text.Trim();
            target.Nama = txtNama.Text.Trim();
            target.JenisKelamin = rbLaki.IsChecked == true ? "Laki-laki" : "Perempuan";
            target.TanggalLahir = dpTanggalLahir.SelectedDate!.Value;
            target.ProgramStudi = cbProdi.SelectedItem?.ToString() ?? string.Empty;
            target.Email = txtEmail.Text.Trim();
            target.NoHp = txtNoHp.Text.Trim();
            target.Alamat = txtAlamat.Text.Trim();
            return target;
        }

        private bool ValidateForm(Guid? currentId)
        {
            string? error = null;
            string nim = txtNim.Text.Trim();

            if (string.IsNullOrWhiteSpace(nim))
                error = "NIM wajib diisi.";
            else if (!Regex.IsMatch(nim, @"^\d+$"))
                error = "NIM hanya boleh berisi angka.";
            else if (_repository.NimExists(nim, currentId))
                error = $"NIM {nim} sudah terdaftar.";
            else if (string.IsNullOrWhiteSpace(txtNama.Text))
                error = "Nama wajib diisi.";
            else if (rbLaki.IsChecked != true && rbPerempuan.IsChecked != true)
                error = "Jenis kelamin wajib dipilih.";
            else if (dpTanggalLahir.SelectedDate == null)
                error = "Tanggal lahir wajib diisi.";
            else if (dpTanggalLahir.SelectedDate > DateTime.Today)
                error = "Tanggal lahir tidak boleh di masa depan.";
            else if (cbProdi.SelectedItem == null)
                error = "Program studi wajib dipilih.";
            else if (string.IsNullOrWhiteSpace(txtEmail.Text))
                error = "Email wajib diisi.";
            else if (!Regex.IsMatch(txtEmail.Text.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                error = "Format email tidak valid.";
            else if (!string.IsNullOrWhiteSpace(txtNoHp.Text)
                     && !Regex.IsMatch(txtNoHp.Text.Trim(), @"^\+?\d{8,15}$"))
                error = "No. HP harus berupa angka (8-15 digit).";

            if (error != null)
            {
                MessageBox.Show(error, "Validasi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }
    }
}
