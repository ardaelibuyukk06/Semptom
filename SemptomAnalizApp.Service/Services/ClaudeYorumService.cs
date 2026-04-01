using System.Text.Json;
using Anthropic;
using Anthropic.Models.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SemptomAnalizApp.Service.Interfaces;

namespace SemptomAnalizApp.Service.Services;

public class ClaudeYorumService(IConfiguration config, ILogger<ClaudeYorumService> logger)
    : IClaudeYorumService
{
    private static readonly JsonSerializerOptions _jsonOpt = new() { PropertyNameCaseInsensitive = true };

    public async Task<ClaudeAnalizSonucu?> AnalizEtAsync(ClaudeAnalizGirdisi girdi)
    {
        var apiKey = config["Claude:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogDebug("Claude API anahtarı yapılandırılmamış, AI analizi atlandı.");
            return null;
        }

        try
        {
            var client = new AnthropicClient { ApiKey = apiKey };

            var semptomListesi = string.Join(", ", girdi.SecilmisSemptomlar);
            var yasInfo        = girdi.Yas > 0 ? $"{girdi.Yas} yaşında" : "";
            var cinsiyetInfo   = !string.IsNullOrEmpty(girdi.Cinsiyet) ? girdi.Cinsiyet : "";
            var bmiInfo        = girdi.Bmi > 0 ? $"BMI {girdi.Bmi:F1}" : "";
            var profilBilgisi  = string.Join(", ", new[] { yasInfo, cinsiyetInfo, bmiInfo }
                                     .Where(s => !string.IsNullOrEmpty(s)));

            var profilSatiri = string.IsNullOrEmpty(profilBilgisi) ? "" : $"\nHasta profili: {profilBilgisi}";
            var jsonOrnek =
                "{\n" +
                "  \"yorum\": \"150-200 kelimelik Türkçe genel değerlendirme\",\n" +
                "  \"onerilen_durumlar\": [\n" +
                "    {\"ad\": \"Durum Adı\", \"aciklama\": \"Kısa açıklama (1-2 cümle)\", \"uyum\": \"Yüksek|Orta|Düşük\"}\n" +
                "  ]\n" +
                "}";

            var prompt =
                $"Semptomlar: {semptomListesi}{profilSatiri}\n\n" +
                "Görevin:\n" +
                "1. Bu semptomları tıbbi bilginle bağımsız olarak değerlendir.\n" +
                "2. Olası durumları/hastalıkları listele — veritabanıyla sınırlı değilsin, tüm tıp bilginle analiz yap.\n" +
                "3. Her durum için kısa, anlaşılır bir açıklama ekle.\n" +
                "4. Uyum düzeyini \"Yüksek\", \"Orta\" veya \"Düşük\" olarak belirt.\n" +
                "5. Son olarak genel bir değerlendirme yorumu yaz.\n\n" +
                "SADECE aşağıdaki JSON formatında cevap ver, başka hiçbir şey yazma:\n" +
                jsonOrnek + "\n\n" +
                "En az 3, en fazla 6 durum öner. Türkçe yaz. Tıbbi teşhis olmadığını belirt.";

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(45));

            var response = await client.Messages.Create(new MessageCreateParams
            {
                Model     = Model.ClaudeOpus4_6,
                MaxTokens = 1024,
                System    = "Sen bir klinik tıp asistanısın. Semptom analizinde yalnızca JSON formatında yanıt üretiyorsun. Tıbbi teşhis yapmıyorsun; istatistiksel benzerlik ve genel bilgi sunuyorsun. Yanıtın daima geçerli JSON olmalı.",
                Messages  =
                [
                    new() { Role = Role.User, Content = prompt }
                ]
            }, cancellationToken: cts.Token);

            var metin = response.Content
                .Select(b => b.Value)
                .OfType<TextBlock>()
                .FirstOrDefault()?.Text?.Trim();

            if (string.IsNullOrEmpty(metin)) return null;

            // JSON bloğunu temizle (```json ... ``` sarmalı olabilir)
            if (metin.StartsWith("```"))
            {
                var start = metin.IndexOf('{');
                var end   = metin.LastIndexOf('}');
                if (start >= 0 && end > start)
                    metin = metin[start..(end + 1)];
            }

            var parsed = JsonSerializer.Deserialize<ClaudeJsonResponse>(metin, _jsonOpt);
            if (parsed == null) return null;

            var durumlar = parsed.Onerilen_Durumlar?
                .Select(d => new ClaudeOnerdigiDurum(
                    Ad:          d.Ad ?? "",
                    Aciklama:    d.Aciklama ?? "",
                    UyumDuzeyi:  d.Uyum ?? "Orta"))
                .ToList() ?? [];

            return new ClaudeAnalizSonucu(
                Yorum:            parsed.Yorum ?? "",
                OnerdigiDurumlar: durumlar);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Claude API isteği zaman aşımına uğradı.");
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Claude API analizi sırasında hata oluştu.");
            return null;
        }
    }

    // DTO sınıfları — sadece JSON parse için
    private sealed class ClaudeJsonResponse
    {
        public string? Yorum { get; set; }
        public List<ClaudeJsonDurum>? Onerilen_Durumlar { get; set; }
    }

    private sealed class ClaudeJsonDurum
    {
        public string? Ad { get; set; }
        public string? Aciklama { get; set; }
        public string? Uyum { get; set; }
    }
}
