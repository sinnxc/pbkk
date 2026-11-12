namespace StudentRegistrationApp;

public sealed class Mahasiswa
{
    public required string Nim { get; init; }
    public required string Nama { get; init; }
    public required string Prodi { get; init; }
    public required string JenisKelamin { get; init; }
    public string Ringkasan => $"{Nim} | {Nama}";
    public string Detail => $"{Prodi} | {JenisKelamin}";
}
