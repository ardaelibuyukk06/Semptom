using Anthropic;
using Anthropic.Models.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SemptomAnalizApp.Service.Interfaces;

namespace SemptomAnalizApp.Service.Services;

public class ClaudeYorumService(IConfiguration config, ILogger<ClaudeYorumService> logger)
    : IClaudeYorumService
{
    public async Task<string?> YorumOlusturAsync(ClaudeAnalizGirdisi girdi)
    {
        var apiKey = config["Claude:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogDebug("Claude API anahtarı yapılandırılmamış, AI yorumu atlandı.");
            return null;
        }

        try
        {
            var client = new AnthropicClient { ApiKey = apiKey };

            var semptomListesi = string.Join(", ", girdi.SecilmisSemptomlar);
            var durumListesi   = string.Join(", ", girdi.OlasiDurumlar.Take(3));
            var yasInfo        = girdi.Yas > 0 ? $"{girdi.Yas} yaşında" : "yaş bilinmiyor";
            var cinsiyetInfo   = !string.IsNullOrEmpty(girdi.Cinsiyet) ? girdi.Cinsiyet : "cinsiyet bilinmiyor";
            var bmiInfo        = girdi.Bmi > 0 ? $"BMI: {girdi.Bmi:F1}" : "BMI bilinmiyor";

            var prompt = $"""
                Kullanıcının semptom analiz sonuçları:
                - Semptomlar: {semptomListesi}
                - Olası durumlar (istatistiksel benzerlik): {durumListesi}
                - Aciliyet düzeyi: {girdi.AciliyetEtiketi}
                - Önerilen sağlık birimi: {girdi.OnerilenBolum}
                - Profil: {yasInfo}, {cinsiyetInfo}, {bmiInfo}

                Bu sonuçlara dayanarak 150-200 kelimelik kısa, empatik ve anlaşılır Türkçe bir yorum yaz.
                Kullanıcıyı rahatlatırken sağlık konusunda bilinçli olmalarını teşvik et.
                Gerekli durumlarda doktora başvurmalarını nazikçe hatırlat.
                Başlık veya "Yapay Zeka" gibi bir ön ek ekleme — doğrudan yoruma başla.
                """;

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            var response = await client.Messages.Create(new MessageCreateParams
            {
                Model     = Model.ClaudeOpus4_6,
                MaxTokens = 512,
                System    = "Sen empatik bir sağlık bilgi asistanısın. Tıbbi teşhis yapmıyorsun; yalnızca kullanıcının istatistiksel semptom analizi sonuçlarını sade ve anlaşılır Türkçe ile yorumluyorsun.",
                Messages  =
                [
                    new() { Role = Role.User, Content = prompt }
                ]
            }, cancellationToken: cts.Token);

            var textBlock = response.Content
                .Select(b => b.Value)
                .OfType<TextBlock>()
                .FirstOrDefault();

            return textBlock?.Text?.Trim();
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Claude API isteği zaman aşımına uğradı.");
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Claude API yorumu oluşturulurken hata oluştu.");
            return null;
        }
    }
}
