namespace SemptomAnalizApp.Service.Interfaces;

public record ClaudeAnalizGirdisi(
    List<string> SecilmisSemptomlar,
    List<string> DbOlasiDurumlar,   // Naive Bayes top 3
    string AciliyetEtiketi,
    string OnerilenBolum,
    int Yas,
    string Cinsiyet,
    decimal Bmi);

public record ClaudeOnerdigiDurum(
    string Ad,
    string Aciklama,
    string UyumDuzeyi);   // "Yüksek" | "Orta" | "Düşük"

public record ClaudeAnalizSonucu(
    string Yorum,
    List<ClaudeOnerdigiDurum> OnerdigiDurumlar);

public interface IClaudeYorumService
{
    Task<ClaudeAnalizSonucu?> AnalizEtAsync(ClaudeAnalizGirdisi girdi);
}
