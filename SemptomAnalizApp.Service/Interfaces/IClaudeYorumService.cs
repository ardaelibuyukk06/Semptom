namespace SemptomAnalizApp.Service.Interfaces;

public record ClaudeAnalizGirdisi(
    List<string> SecilmisSemptomlar,
    List<string> OlasiDurumlar,   // top 3 hastalık adı
    string AciliyetEtiketi,
    string OnerilenBolum,
    int Yas,
    string Cinsiyet,
    decimal Bmi);

public interface IClaudeYorumService
{
    Task<string?> YorumOlusturAsync(ClaudeAnalizGirdisi girdi);
}
