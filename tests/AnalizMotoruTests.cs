using SemptomAnalizApp.Core.Entities;
using SemptomAnalizApp.Core.Enums;
using SemptomAnalizApp.Service.Interfaces;
using SemptomAnalizApp.Service.Services;

namespace SemptomAnalizApp.Tests;

/// <summary>
/// AnalizMotoru saf (DB bağımsız) statik metodlarının unit testleri.
/// </summary>
public class AnalizMotoruTests
{
    // ═══════════════════════════════════════════════════════════
    //  HesaplaBmi
    // ═══════════════════════════════════════════════════════════

    [Fact]
    public void HesaplaBmi_NullProfil_ReturnsZeroNormal()
    {
        var (bmi, kat) = AnalizMotoru.HesaplaBmi(null);

        Assert.Equal(0, bmi);
        Assert.Equal(BmiKategori.Normal, kat);
    }

    [Fact]
    public void HesaplaBmi_ZeroHeight_ReturnsZeroNormal()
    {
        var profil = new SaglikProfili { Boy = 0, Kilo = 70 };
        var (bmi, kat) = AnalizMotoru.HesaplaBmi(profil);

        Assert.Equal(0, bmi);
        Assert.Equal(BmiKategori.Normal, kat);
    }

    [Theory]
    [InlineData(170, 55, BmiKategori.Normal)]       // BMI ≈ 19.0
    [InlineData(170, 72, BmiKategori.Normal)]       // BMI ≈ 24.9
    [InlineData(170, 75, BmiKategori.Fazlakilolu)]  // BMI ≈ 26.0
    [InlineData(170, 90, BmiKategori.ObezeI)]       // BMI ≈ 31.1
    [InlineData(170, 115, BmiKategori.ObezeII)]     // BMI ≈ 39.8
    [InlineData(170, 50, BmiKategori.ZayifAltinda)] // BMI ≈ 17.3
    public void HesaplaBmi_ValidProfil_ReturnsCorrectKategori(int boy, int kilo, BmiKategori beklenenKat)
    {
        var profil = new SaglikProfili { Boy = boy, Kilo = kilo };
        var (_, kat) = AnalizMotoru.HesaplaBmi(profil);

        Assert.Equal(beklenenKat, kat);
    }

    [Fact]
    public void HesaplaBmi_StandardInput_BmiRoundedTo1Decimal()
    {
        // 175cm, 70kg → BMI = 70 / 1.75² = 70 / 3.0625 = 22.857... → round(1) = 22.9
        var profil = new SaglikProfili { Boy = 175, Kilo = 70 };
        var (bmi, _) = AnalizMotoru.HesaplaBmi(profil);

        Assert.Equal(22.9m, bmi);
    }

    // ═══════════════════════════════════════════════════════════
    //  OlusturImza
    // ═══════════════════════════════════════════════════════════

    [Fact]
    public void OlusturImza_SameIds_SameSignature()
    {
        var imza1 = AnalizMotoru.OlusturImza([1, 2, 3]);
        var imza2 = AnalizMotoru.OlusturImza([1, 2, 3]);

        Assert.Equal(imza1, imza2);
    }

    [Fact]
    public void OlusturImza_OrderIndependent()
    {
        // Sıralama farklı olsa da aynı imza çıkmalı
        var imza1 = AnalizMotoru.OlusturImza([3, 1, 2]);
        var imza2 = AnalizMotoru.OlusturImza([1, 2, 3]);

        Assert.Equal(imza1, imza2);
    }

    [Fact]
    public void OlusturImza_DifferentIds_DifferentSignature()
    {
        var imza1 = AnalizMotoru.OlusturImza([1, 2, 3]);
        var imza2 = AnalizMotoru.OlusturImza([1, 2, 4]);

        Assert.NotEqual(imza1, imza2);
    }

    [Fact]
    public void OlusturImza_ReturnsExactly12Chars()
    {
        var imza = AnalizMotoru.OlusturImza([5, 10, 15]);

        Assert.Equal(12, imza.Length);
    }

    [Fact]
    public void OlusturImza_IsLowerHex()
    {
        var imza = AnalizMotoru.OlusturImza([1]);

        Assert.Matches("^[0-9a-f]{12}$", imza);
    }

    // ═══════════════════════════════════════════════════════════
    //  SkoraSeviyeAta
    // ═══════════════════════════════════════════════════════════

    [Theory]
    [InlineData(0,   AciliyetSeviyesi.Normal)]
    [InlineData(25,  AciliyetSeviyesi.Normal)]
    [InlineData(26,  AciliyetSeviyesi.Izle)]
    [InlineData(50,  AciliyetSeviyesi.Izle)]
    [InlineData(51,  AciliyetSeviyesi.Dikkat)]
    [InlineData(75,  AciliyetSeviyesi.Dikkat)]
    [InlineData(76,  AciliyetSeviyesi.Acil)]
    [InlineData(100, AciliyetSeviyesi.Acil)]
    public void SkoraSeviyeAta_Thresholds_CorrectLevel(int skor, AciliyetSeviyesi beklenen)
    {
        var seviye = AnalizMotoru.SkoraSeviyeAta(skor);

        Assert.Equal(beklenen, seviye);
    }

    // ═══════════════════════════════════════════════════════════
    //  HesaplaAciliyetSkoru
    // ═══════════════════════════════════════════════════════════

    [Fact]
    public void HesaplaAciliyetSkoru_EmptyGirdiler_Returns10()
    {
        var skor = AnalizMotoru.HesaplaAciliyetSkoru([], [], null, false);

        Assert.Equal(10, skor);
    }

    [Fact]
    public void HesaplaAciliyetSkoru_KritikSemptom_BoostScore()
    {
        var girdiler = new List<SemptomGirdisi>
        {
            new(SemptomId: 1, Siddet: 1, SureGun: 1)
        };
        int skorKritiksiz = AnalizMotoru.HesaplaAciliyetSkoru(girdiler, [], null, false);
        int skorKritikli  = AnalizMotoru.HesaplaAciliyetSkoru(girdiler, [], null, true);

        Assert.True(skorKritikli > skorKritiksiz, "Kritik semptom puanı artırmalı");
    }

    [Fact]
    public void HesaplaAciliyetSkoru_YasliHasta_BoostScore()
    {
        var girdiler = new List<SemptomGirdisi>
        {
            new(SemptomId: 1, Siddet: 2, SureGun: 3)
        };
        var profilGenc  = new SaglikProfili { Yas = 30, Boy = 170, Kilo = 70 };
        var profilYasli = new SaglikProfili { Yas = 70, Boy = 170, Kilo = 70 };

        int skorGenc  = AnalizMotoru.HesaplaAciliyetSkoru(girdiler, [], profilGenc,  false);
        int skorYasli = AnalizMotoru.HesaplaAciliyetSkoru(girdiler, [], profilYasli, false);

        Assert.True(skorYasli > skorGenc, "65+ yaş skoru artırmalı");
    }

    [Fact]
    public void HesaplaAciliyetSkoru_KronikHastalik_BoostScore()
    {
        var girdiler = new List<SemptomGirdisi>
        {
            new(SemptomId: 1, Siddet: 2, SureGun: 2)
        };
        var profilTemiz  = new SaglikProfili { Yas = 40, Boy = 170, Kilo = 70 };
        var profilKronik = new SaglikProfili { Yas = 40, Boy = 170, Kilo = 70, KronikHastaliklar = "Diyabet" };

        int skorTemiz  = AnalizMotoru.HesaplaAciliyetSkoru(girdiler, [], profilTemiz,  false);
        int skorKronik = AnalizMotoru.HesaplaAciliyetSkoru(girdiler, [], profilKronik, false);

        Assert.True(skorKronik > skorTemiz, "Kronik hastalık skoru artırmalı");
    }

    [Fact]
    public void HesaplaAciliyetSkoru_NeverExceeds100()
    {
        var girdiler = Enumerable.Range(1, 10)
            .Select(i => new SemptomGirdisi(SemptomId: i, Siddet: 3, SureGun: 30))
            .ToList();
        var profilYasliKronik = new SaglikProfili { Yas = 80, KronikHastaliklar = "Kalp yetmezliği" };

        var skor = AnalizMotoru.HesaplaAciliyetSkoru(girdiler, [], profilYasliKronik, true);

        Assert.InRange(skor, 0, 100);
    }
}
