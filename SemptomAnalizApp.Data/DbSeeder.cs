using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SemptomAnalizApp.Core.Entities;
using SemptomAnalizApp.Core.Enums;

namespace SemptomAnalizApp.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context, UserManager<Kullanici> userManager, RoleManager<IdentityRole> roleManager, string? adminPassword = null)
    {
        await context.Database.MigrateAsync();

        // ── Roller ────────────────────────────────────────────────────
        foreach (var rol in new[] { "Admin", "Kullanici" })
        {
            if (!await roleManager.RoleExistsAsync(rol))
                await roleManager.CreateAsync(new IdentityRole(rol));
        }

        // ── Admin ─────────────────────────────────────────────────────
        if (await userManager.FindByEmailAsync("admin@semptomanaliz.com") == null)
        {
            var admin = new Kullanici
            {
                UserName = "admin@semptomanaliz.com",
                Email = "admin@semptomanaliz.com",
                Ad = "Sistem", Soyad = "Yöneticisi",
                EmailConfirmed = true
            };
            var r = await userManager.CreateAsync(admin, adminPassword ?? "Admin123!");
            if (r.Succeeded) await userManager.AddToRoleAsync(admin, "Admin");
        }

        // ── Semptom Kataloğu ──────────────────────────────────────────
        if (!await context.SemptomKatalog.AnyAsync())
        {
            var semptomlar = new List<SemptomKatalog>
            {
                new() { Ad = "Baş ağrısı",       Kategori = SemptomKategorisi.BasBoyun, IkonKodu = "bi-emoji-dizzy" },
                new() { Ad = "Boyun tutulması",   Kategori = SemptomKategorisi.BasBoyun, IkonKodu = "bi-person-arms-up" },
                new() { Ad = "Baş dönmesi",       Kategori = SemptomKategorisi.BasBoyun, IkonKodu = "bi-arrow-repeat" },
                new() { Ad = "Kulak ağrısı",      Kategori = SemptomKategorisi.BasBoyun, IkonKodu = "bi-ear" },
                new() { Ad = "Boğaz ağrısı",      Kategori = SemptomKategorisi.BasBoyun, IkonKodu = "bi-activity" },
                new() { Ad = "Öksürük",           Kategori = SemptomKategorisi.Solunum,  IkonKodu = "bi-wind" },
                new() { Ad = "Nefes darlığı",     Kategori = SemptomKategorisi.Solunum,  IkonKodu = "bi-lungs" },
                new() { Ad = "Burun akıntısı",    Kategori = SemptomKategorisi.Solunum,  IkonKodu = "bi-droplet" },
                new() { Ad = "Burun tıkanıklığı", Kategori = SemptomKategorisi.Solunum,  IkonKodu = "bi-x-circle" },
                new() { Ad = "Hırıltılı nefes",   Kategori = SemptomKategorisi.Solunum,  IkonKodu = "bi-soundwave" },
                new() { Ad = "Karın ağrısı",      Kategori = SemptomKategorisi.Sindirim, IkonKodu = "bi-tsunami" },
                new() { Ad = "Bulantı",           Kategori = SemptomKategorisi.Sindirim, IkonKodu = "bi-emoji-frown" },
                new() { Ad = "Kusma",             Kategori = SemptomKategorisi.Sindirim, IkonKodu = "bi-exclamation-triangle" },
                new() { Ad = "İshal",             Kategori = SemptomKategorisi.Sindirim, IkonKodu = "bi-water" },
                new() { Ad = "Kabızlık",          Kategori = SemptomKategorisi.Sindirim, IkonKodu = "bi-lock" },
                new() { Ad = "İştahsızlık",       Kategori = SemptomKategorisi.Sindirim, IkonKodu = "bi-dash-circle" },
                new() { Ad = "Eklem ağrısı",      Kategori = SemptomKategorisi.MasEklem, IkonKodu = "bi-gear" },
                new() { Ad = "Kas ağrısı",        Kategori = SemptomKategorisi.MasEklem, IkonKodu = "bi-lightning" },
                new() { Ad = "Sırt ağrısı",       Kategori = SemptomKategorisi.MasEklem, IkonKodu = "bi-person-standing" },
                new() { Ad = "Şişlik",            Kategori = SemptomKategorisi.MasEklem, IkonKodu = "bi-arrow-up-circle" },
                new() { Ad = "Ateş",              Kategori = SemptomKategorisi.Genel,    IkonKodu = "bi-thermometer-high" },
                new() { Ad = "Halsizlik",         Kategori = SemptomKategorisi.Genel,    IkonKodu = "bi-battery-half" },
                new() { Ad = "Titreme",           Kategori = SemptomKategorisi.Genel,    IkonKodu = "bi-snow" },
                new() { Ad = "Terleme",           Kategori = SemptomKategorisi.Genel,    IkonKodu = "bi-droplet-fill" },
                new() { Ad = "Yorgunluk",         Kategori = SemptomKategorisi.Genel,    IkonKodu = "bi-moon" },
                new() { Ad = "Kilo kaybı",        Kategori = SemptomKategorisi.Genel,    IkonKodu = "bi-graph-down" },
                new() { Ad = "Döküntü",           Kategori = SemptomKategorisi.Deri,     IkonKodu = "bi-circle-square" },
                new() { Ad = "Kaşıntı",           Kategori = SemptomKategorisi.Deri,     IkonKodu = "bi-hand-index" },
                new() { Ad = "Kızarıklık",        Kategori = SemptomKategorisi.Deri,     IkonKodu = "bi-circle-fill" },
                new() { Ad = "Göğüs ağrısı",      Kategori = SemptomKategorisi.Kalp,     IkonKodu = "bi-heart-pulse" },
                new() { Ad = "Çarpıntı",          Kategori = SemptomKategorisi.Kalp,     IkonKodu = "bi-activity" },
                new() { Ad = "Baygınlık hissi",   Kategori = SemptomKategorisi.Kalp,     IkonKodu = "bi-person-down" },
            };
            await context.SemptomKatalog.AddRangeAsync(semptomlar);
            await context.SaveChangesAsync();
        }

        // ── Teşhis Motoru: Semptomlar ────────────────────────────────
        // Semptom.Id 1-32, SemptomKatalog.Id 1-32 ile birebir eşleşir (aynı sırada eklendi)
        if (!await context.Semptomlar.AnyAsync())
        {
            var semptomlar = new List<Semptom>
            {
                // Baş-Boyun (1-5)
                new() { Ad = "Baş ağrısı",       KritikMi = false, Kategori = "Baş-Boyun" },
                new() { Ad = "Boyun tutulması",   KritikMi = false, Kategori = "Baş-Boyun" },
                new() { Ad = "Baş dönmesi",       KritikMi = false, Kategori = "Baş-Boyun" },
                new() { Ad = "Kulak ağrısı",      KritikMi = false, Kategori = "Baş-Boyun" },
                new() { Ad = "Boğaz ağrısı",      KritikMi = false, Kategori = "Baş-Boyun" },
                // Solunum (6-10)
                new() { Ad = "Öksürük",           KritikMi = false, Kategori = "Solunum" },
                new() { Ad = "Nefes darlığı",     KritikMi = true,  Kategori = "Solunum" },
                new() { Ad = "Burun akıntısı",    KritikMi = false, Kategori = "Solunum" },
                new() { Ad = "Burun tıkanıklığı", KritikMi = false, Kategori = "Solunum" },
                new() { Ad = "Hırıltılı nefes",   KritikMi = false, Kategori = "Solunum" },
                // Sindirim (11-16)
                new() { Ad = "Karın ağrısı",      KritikMi = false, Kategori = "Sindirim" },
                new() { Ad = "Bulantı",           KritikMi = false, Kategori = "Sindirim" },
                new() { Ad = "Kusma",             KritikMi = false, Kategori = "Sindirim" },
                new() { Ad = "İshal",             KritikMi = false, Kategori = "Sindirim" },
                new() { Ad = "Kabızlık",          KritikMi = false, Kategori = "Sindirim" },
                new() { Ad = "İştahsızlık",       KritikMi = false, Kategori = "Sindirim" },
                // Kas-Eklem (17-20)
                new() { Ad = "Eklem ağrısı",      KritikMi = false, Kategori = "Kas-Eklem" },
                new() { Ad = "Kas ağrısı",        KritikMi = false, Kategori = "Kas-Eklem" },
                new() { Ad = "Sırt ağrısı",       KritikMi = false, Kategori = "Kas-Eklem" },
                new() { Ad = "Şişlik",            KritikMi = false, Kategori = "Kas-Eklem" },
                // Genel (21-26)
                new() { Ad = "Ateş",              KritikMi = false, Kategori = "Genel" },
                new() { Ad = "Halsizlik",         KritikMi = false, Kategori = "Genel" },
                new() { Ad = "Titreme",           KritikMi = false, Kategori = "Genel" },
                new() { Ad = "Terleme",           KritikMi = false, Kategori = "Genel" },
                new() { Ad = "Yorgunluk",         KritikMi = false, Kategori = "Genel" },
                new() { Ad = "Kilo kaybı",        KritikMi = false, Kategori = "Genel" },
                // Deri (27-29)
                new() { Ad = "Döküntü",           KritikMi = false, Kategori = "Deri" },
                new() { Ad = "Kaşıntı",           KritikMi = false, Kategori = "Deri" },
                new() { Ad = "Kızarıklık",        KritikMi = false, Kategori = "Deri" },
                // Kalp (30-32)
                new() { Ad = "Göğüs ağrısı",     KritikMi = true,  Kategori = "Kalp" },
                new() { Ad = "Çarpıntı",          KritikMi = false, Kategori = "Kalp" },
                new() { Ad = "Baygınlık hissi",   KritikMi = true,  Kategori = "Kalp" },
            };
            await context.Semptomlar.AddRangeAsync(semptomlar);
            await context.SaveChangesAsync();
        }

        // ── SemptomKatalog ↔ Semptom FK Senkronizasyonu ──────────────
        // Migration sonrası SemptomId null olan katalog kayıtlarını Ada göre eşleştir.
        // Her startup çalışır; sadece null FK'lı kayıtları günceller.
        var katalogsWithoutFk = await context.SemptomKatalog
            .Where(sk => sk.SemptomId == null)
            .ToListAsync();

        if (katalogsWithoutFk.Any())
        {
            var semptomAdMap = await context.Semptomlar
                .Select(s => new { s.Id, s.Ad })
                .ToDictionaryAsync(s => s.Ad, s => s.Id);

            foreach (var katalog in katalogsWithoutFk)
            {
                if (semptomAdMap.TryGetValue(katalog.Ad, out var semptomId))
                    katalog.SemptomId = semptomId;
            }
            await context.SaveChangesAsync();
        }

        // ── Teşhis Motoru: Hastalıklar + Ağırlıklar (42 hastalık) ──────
        // Semptom ID referansı (Semptom.Id = SemptomKatalog.Id, aynı sırada seeded):
        // 1=BaşAğrısı 2=BoyunTutulması 3=BaşDönmesi 4=KulakAğrısı 5=BoğazAğrısı
        // 6=Öksürük 7=NefesDarlığı 8=BurunAkıntısı 9=BurunTıkanıklığı 10=HırıltılıNefes
        // 11=KarınAğrısı 12=Bulantı 13=Kusma 14=İshal 15=Kabızlık 16=İştahsızlık
        // 17=EklemAğrısı 18=KasAğrısı 19=SırtAğrısı 20=Şişlik
        // 21=Ateş 22=Halsizlik 23=Titreme 24=Terleme 25=Yorgunluk 26=KiloKaybı
        // 27=Döküntü 28=Kaşıntı 29=Kızarıklık
        // 30=GöğüsAğrısı 31=Çarpıntı 32=BaygınlıkHissi
        if (!await context.Hastaliklar.AnyAsync())
        {
            // ── SOLUNUM SİSTEMİ ────────────────────────────────────────
            var grip    = new Hastalik { Ad = "Grip / İnfluenza",                 OnerilenBolum = "Dahiliye / Aile Hekimliği", Prevalans = 80, AkutMu = true,  Aciklama = "Virüs kaynaklı solunum yolu enfeksiyonu; yüksek ateş, kas ağrısı ve ani başlayan halsizlik ile karakterizedir." };
            var usye    = new Hastalik { Ad = "Üst Solunum Yolu Enfeksiyonu",      OnerilenBolum = "Kulak Burun Boğaz",         Prevalans = 90, AkutMu = true,  Aciklama = "Öksürük, boğaz ağrısı ve burun belirtileriyle seyreden yaygın solunum yolu enfeksiyonu." };
            var bronsit = new Hastalik { Ad = "Bronşit",                           OnerilenBolum = "Göğüs Hastalıkları",        Prevalans = 55, AkutMu = true,  Aciklama = "Bronş mukozasının iltihabı; üretken öksürük ve nefes darlığı belirgindir." };
            var pnomoni = new Hastalik { Ad = "Pnömoni (Zatürre)",                 OnerilenBolum = "Göğüs Hastalıkları / Acil", Prevalans = 30, AkutMu = true,  MinYas = 5,  ErkekKatsayi = 1.1m, Aciklama = "Akciğer parankiminin enfeksiyonu; yüksek ateş, üretken öksürük ve nefes darlığı eşliğinde ciddi tabloya yol açabilir." };
            var astim   = new Hastalik { Ad = "Astım",                             OnerilenBolum = "Göğüs Hastalıkları / Alerji", Prevalans = 45, AkutMu = false, Aciklama = "Bronş hiperreaktivitesi sonucu epizodik nefes darlığı, hışıltı ve göğüste sıkışma hissiyle karakterize kronik solunum hastalığı." };
            var sinuzit = new Hastalik { Ad = "Sinüzit",                           OnerilenBolum = "Kulak Burun Boğaz",         Prevalans = 60, AkutMu = true,  Aciklama = "Paranazal sinüslerin iltihabı; yüz ağrısı, burun tıkanıklığı ve başağrısı belirgindir." };
            var tbc     = new Hastalik { Ad = "Tüberküloz (TB)",                   OnerilenBolum = "Göğüs Hastalıkları / Enfeksiyon", Prevalans = 8, AkutMu = false, Aciklama = "Mycobacterium tuberculosis'e bağlı kronik enfeksiyon; persistan öksürük, kilo kaybı ve gece terlemeleri tipiktir." };
            var farenjit= new Hastalik { Ad = "Viral Farenjit (Boğaz Enfeksiyonu)",OnerilenBolum = "Kulak Burun Boğaz",         Prevalans = 65, AkutMu = true,  Aciklama = "Orofarinksin viral enfeksiyonu; yutma güçlüğü ve boğaz ağrısı ön plandadır." };

            // ── KARDİYOVASKÜLER ────────────────────────────────────────
            var kardiyak= new Hastalik { Ad = "Kardiyak Değerlendirme Gerekli",    OnerilenBolum = "Kardiyoloji / Acil",        Prevalans = 25, AkutMu = true,  MinYas = 35, ErkekKatsayi = 1.6m, KadinKatsayi = 0.9m, Aciklama = "Göğüs ağrısı ile birlikte kardiyak kökenli bir durumu dışlamak için acil değerlendirme zorunludur." };
            var anjin   = new Hastalik { Ad = "Anjin Pektoris",                    OnerilenBolum = "Kardiyoloji",               Prevalans = 20, AkutMu = true,  MinYas = 40, ErkekKatsayi = 1.8m, Aciklama = "Koroner arterlerin yetersiz perfüzyonuna bağlı göğüs ağrısı; eforla tipik olarak tetiklenir." };
            var aritmi  = new Hastalik { Ad = "Aritmisi / Çarpıntı Bozukluğu",    OnerilenBolum = "Kardiyoloji",               Prevalans = 40, AkutMu = false, MinYas = 30, Aciklama = "Kalp ritim bozukluğu; ani başlayan çarpıntı, baygınlık hissi ve nefes darlığı ile kendini gösterebilir." };
            var htkriz  = new Hastalik { Ad = "Hipertansif Kriz",                  OnerilenBolum = "Kardiyoloji / Acil",        Prevalans = 15, AkutMu = true,  MinYas = 40, ErkekKatsayi = 1.3m, Aciklama = "Kan basıncının tehlikeli düzeyde yükselmesi; şiddetli başağrısı ve baş dönmesiyle kendini gösterebilir." };

            // ── SİNDİRİM SİSTEMİ ───────────────────────────────────────
            var gastro  = new Hastalik { Ad = "Akut Gastroenterit",                OnerilenBolum = "Gastroenteroloji / Acil",   Prevalans = 80, AkutMu = true,  Aciklama = "Viral ya da bakteriyel kaynaklı akut bağırsak enfeksiyonu; bulantı, kusma ve ishal triadı belirgindir." };
            var apandis = new Hastalik { Ad = "Apandisit",                         OnerilenBolum = "Genel Cerrahi / Acil",      Prevalans = 12, AkutMu = true,  MinYas = 5, MaxYas = 45, Aciklama = "Appendiksin akut iltihabı; karın sağ alt kadranında başlayıp yoğunlaşan ağrı ve ateş ile karakterizedir." };
            var ulser   = new Hastalik { Ad = "Peptik Ülser",                      OnerilenBolum = "Gastroenteroloji",          Prevalans = 30, AkutMu = false, MinYas = 25, ErkekKatsayi = 1.4m, Aciklama = "Mide veya duodenum mukozasında ülser oluşumu; epigastrik ağrı, bulantı ve iştahsızlık tipiktir." };
            var gerd    = new Hastalik { Ad = "GERD (Gastroözofajeal Reflü)",      OnerilenBolum = "Gastroenteroloji",          Prevalans = 55, AkutMu = false, Aciklama = "Mide içeriğinin özofagusa kaçması; yakıcı göğüs ağrısı, yutma güçlüğü ve öksürük görülebilir." };
            var kolesist= new Hastalik { Ad = "Kolesistit (Safra Kesesi İltihabı)",OnerilenBolum = "Genel Cerrahi / Gastroenteroloji", Prevalans = 20, AkutMu = true, MinYas = 25, KadinKatsayi = 1.5m, Aciklama = "Safra kesesinin iltihabı; sağ üst karın ağrısı, ateş ve bulantı ile seyreder." };
            var ibs     = new Hastalik { Ad = "İrritabl Bağırsak Sendromu (IBS)", OnerilenBolum = "Gastroenteroloji",          Prevalans = 45, AkutMu = false, KadinKatsayi = 1.5m, Aciklama = "Fonksiyonel bağırsak bozukluğu; karın ağrısı, şişkinlik ve ishal–kabızlık değişimi ile karakterizedir." };

            // ── NÖROLOJİK ──────────────────────────────────────────────
            var migren  = new Hastalik { Ad = "Migren",                            OnerilenBolum = "Nöroloji",                  Prevalans = 60, AkutMu = true,  KadinKatsayi = 1.6m, Aciklama = "Vasküler kökenli epizodik başağrısı; ışığa hassasiyet, bulantı ve kusma eşlik edebilir." };
            var gerilim = new Hastalik { Ad = "Gerilim Tipi Başağrısı",            OnerilenBolum = "Nöroloji / Aile Hekimliği", Prevalans = 75, AkutMu = true,  Aciklama = "Bilateral sıkıştırıcı başağrısı; stres ve kas gerginliğiyle ilişkilidir." };
            var menenjit= new Hastalik { Ad = "Menenjit (Beyin Zarı İltihabı)",    OnerilenBolum = "Nöroloji / Acil",           Prevalans = 4,  AkutMu = true,  Aciklama = "Beyin zarlarının enfeksiyonu; ense sertliği, yüksek ateş ve şiddetli başağrısı üçlüsü tanıyı düşündürür — acil değerlendirme şarttır." };
            var vertigo = new Hastalik { Ad = "Vertigo (Denge Bozukluğu)",         OnerilenBolum = "Nöroloji / KBB",            Prevalans = 50, AkutMu = false, Aciklama = "İç kulak veya merkezi sinir sistemi kökenli baş dönmesi; çevre döner hissiyle birlikte bulantı eşlik eder." };
            var inme    = new Hastalik { Ad = "İnme (Serebrovasküler Olay)",       OnerilenBolum = "Nöroloji / Acil",           Prevalans = 10, AkutMu = true,  MinYas = 45, ErkekKatsayi = 1.4m, Aciklama = "Beyin damar olayı; ani gelişen nörolojik defisit, baş dönmesi ve bilinç değişikliği acil müdahale gerektirir." };

            // ── KAS-İSKELET ─────────────────────────────────────────────
            var kasEkl  = new Hastalik { Ad = "Kas-İskelet Zorlanması",            OnerilenBolum = "Ortopedi / Fizik Tedavi",   Prevalans = 70, AkutMu = true,  Aciklama = "Travma veya aşırı kullanıma bağlı kas-eklem ağrısı; lokalize ağrı ve hareket kısıtlılığı belirgindir." };
            var romato  = new Hastalik { Ad = "Romatoid Artrit",                   OnerilenBolum = "Romatoloji",                Prevalans = 18, AkutMu = false, MinYas = 20, KadinKatsayi = 1.8m, Aciklama = "Otoimmün eklem hastalığı; simetrik eklem iltihabı, sabah tutukluğu ve sistemik belirtilerle seyreder." };
            var gut     = new Hastalik { Ad = "Gut Artriti",                       OnerilenBolum = "Romatoloji / Dahiliye",     Prevalans = 20, AkutMu = true,  MinYas = 35, ErkekKatsayi = 2.0m, Aciklama = "Ürik asit kristallerinin eklemlerde birikmesi; ani başlayan şiddetli eklem ağrısı ve kızarıklık tipiktir." };
            var diskopat= new Hastalik { Ad = "Diskopati / Bel Fıtığı",            OnerilenBolum = "Ortopedi / Nöroşirurji",   Prevalans = 45, AkutMu = false, MinYas = 25, ErkekKatsayi = 1.3m, Aciklama = "İntervertebral diskin dejenerasyonu veya protrüzyonu; bel ağrısı ve bacağa vuran ağrı (siyatik) ile seyreder." };
            var fibromi = new Hastalik { Ad = "Fibromiyalji",                      OnerilenBolum = "Romatoloji / Fizik Tedavi", Prevalans = 22, AkutMu = false, KadinKatsayi = 2.0m, Aciklama = "Yaygın kas-iskelet ağrısı sendromu; yorgunluk, uyku bozukluğu ve hassas nokta ağrısı karakteristiktir." };

            // ── ENDOKRİN / METABOLİK ───────────────────────────────────
            var anemi   = new Hastalik { Ad = "Anemi (Kansızlık)",                 OnerilenBolum = "Dahiliye / Hematoloji",     Prevalans = 50, AkutMu = false, KadinKatsayi = 1.6m, Aciklama = "Hemoglobin düşüklüğü; halsizlik, yorgunluk, çarpıntı ve baş dönmesiyle kendini gösterir." };
            var hipotir = new Hastalik { Ad = "Hipotiroidizm",                     OnerilenBolum = "Endokrinoloji",             Prevalans = 28, AkutMu = false, MinYas = 20, KadinKatsayi = 2.2m, Aciklama = "Yetersiz tiroid hormonu üretimi; yorgunluk, kilo artışı ve soğuğa hassasiyet ile karakterizedir." };
            var hipertir= new Hastalik { Ad = "Hipertiroidizm",                    OnerilenBolum = "Endokrinoloji",             Prevalans = 16, AkutMu = false, MinYas = 15, KadinKatsayi = 2.0m, Aciklama = "Aşırı tiroid hormonu üretimi; çarpıntı, terleme, kilo kaybı ve sinirlilik belirgindir." };
            var diabet  = new Hastalik { Ad = "Diyabet Tip 2 (Belirtileri)",       OnerilenBolum = "Endokrinoloji / Dahiliye",  Prevalans = 35, AkutMu = false, MinYas = 30, ErkekKatsayi = 1.2m, Aciklama = "İnsülin direncine bağlı metabolizma bozukluğu; aşırı yorgunluk ve kilo değişimi erken belirtiler arasındadır." };

            // ── ENFEKSİYON / DİĞER ─────────────────────────────────────
            var mono    = new Hastalik { Ad = "Mononükleoz (EBV Enfeksiyonu)",     OnerilenBolum = "Enfeksiyon Hastalıkları",   Prevalans = 12, AkutMu = true,  MaxYas = 35, Aciklama = "Epstein-Barr virüsü kaynaklı enfeksiyon; şiddetli boğaz ağrısı, ateş, yorgunluk ve boyun bezi şişliği tipiktir." };
            var uti     = new Hastalik { Ad = "Üriner Sistem Enfeksiyonu",         OnerilenBolum = "Üroloji / Aile Hekimliği", Prevalans = 50, AkutMu = true,  KadinKatsayi = 2.2m, Aciklama = "İdrar yolu enfeksiyonu; alt karın ağrısı, ateş ve sık idrara çıkma ile seyreder." };
            var bobrektasi=new Hastalik{ Ad = "Böbrek Taşı",                       OnerilenBolum = "Üroloji",                   Prevalans = 22, AkutMu = true,  MinYas = 20, ErkekKatsayi = 1.5m, Aciklama = "Üriner sistemde taş oluşumu; dalga tarzında çok şiddetli bel ve karın ağrısı (renal kolik) ile kendini gösterir." };
            var alerji  = new Hastalik { Ad = "Alerjik Reaksiyon / Alerjik Rinit", OnerilenBolum = "Alerji ve İmmünoloji",      Prevalans = 65, AkutMu = true,  Aciklama = "Alerjene maruziyete bağlı deri ve solunum belirtileri; kaşıntı, döküntü ve burun tıkanıklığı ön plandadır." };

            // ── PSİKİYATRİK / FONKSİYONEL ──────────────────────────────
            var anksiye = new Hastalik { Ad = "Anksiyete Bozukluğu",               OnerilenBolum = "Psikiyatri / Aile Hekimliği", Prevalans = 55, AkutMu = false, KadinKatsayi = 1.5m, Aciklama = "Kronik anksiyete; çarpıntı, nefes darlığı, terleme ve baş dönmesi gibi somatik belirtiler eşlik edebilir." };
            var panik   = new Hastalik { Ad = "Panik Atak",                        OnerilenBolum = "Psikiyatri / Acil",         Prevalans = 40, AkutMu = true,  KadinKatsayi = 1.6m, Aciklama = "Ani yoğun korku atağı; çarpıntı, nefes darlığı ve baygınlık hissi kardiyak acil ile karışabilir." };
            var depresyon=new Hastalik { Ad = "Depresyon",                         OnerilenBolum = "Psikiyatri",                Prevalans = 55, AkutMu = false, KadinKatsayi = 1.5m, Aciklama = "Major depresif bozukluk; enerji kaybı, iştahsızlık, uyku bozukluğu ve bedensel ağrılarla kendini gösterebilir." };
            var kronikYor=new Hastalik { Ad = "Kronik Yorgunluk Sendromu",         OnerilenBolum = "Dahiliye / Psikiyatri",     Prevalans = 18, AkutMu = false, Aciklama = "6 aydan uzun süren egzersizle artmayan yorgunluk; kognitif bozukluk ve uyku bozukluğu eşlik eder." };

            // ── DERMATOLOJİK ───────────────────────────────────────────
            var urtiker = new Hastalik { Ad = "Ürtiker (Kurdeşen)",                OnerilenBolum = "Dermatoloji / Alerji",      Prevalans = 40, AkutMu = true,  Aciklama = "Alerjik veya idiyopatik kökenli kabarık, kaşıntılı deri lezyonları; yüz ve boğaz tutulumu acil olabilir." };
            var kontakt = new Hastalik { Ad = "Kontakt Dermatit (Egzama)",         OnerilenBolum = "Dermatoloji",               Prevalans = 45, AkutMu = false, Aciklama = "Temas eden maddeye bağlı deri iltihabı; kaşıntı, kızarıklık ve vezikül oluşumu belirgindir." };
            var zona    = new Hastalik { Ad = "Zona (Herpes Zoster)",              OnerilenBolum = "Dermatoloji / Enfeksiyon",  Prevalans = 18, AkutMu = true,  MinYas = 45, Aciklama = "Varisella-zoster virüsünün reaktivasyonu; unilateral band tarzında ağrılı döküntü tipiktir." };

            await context.Hastaliklar.AddRangeAsync(
                grip, usye, bronsit, pnomoni, astim, sinuzit, tbc, farenjit,
                kardiyak, anjin, aritmi, htkriz,
                gastro, apandis, ulser, gerd, kolesist, ibs,
                migren, gerilim, menenjit, vertigo, inme,
                kasEkl, romato, gut, diskopat, fibromi,
                anemi, hipotir, hipertir, diabet,
                mono, uti, bobrektasi, alerji,
                anksiye, panik, depresyon, kronikYor,
                urtiker, kontakt, zona);
            await context.SaveChangesAsync();

            // ── HastalikSemptom — Agirlik 1..10 ──────────────────────────
            var iliskiler = new List<HastalikSemptom>
            {
                // GRIP: Ateş+Kas+Halsizlik üçlüsü çok güçlü
                new() { HastalikId = grip.Id,     SemptomId = 21, Agirlik = 9  }, // Ateş
                new() { HastalikId = grip.Id,     SemptomId = 18, Agirlik = 8  }, // Kas ağrısı
                new() { HastalikId = grip.Id,     SemptomId = 22, Agirlik = 8  }, // Halsizlik
                new() { HastalikId = grip.Id,     SemptomId = 23, Agirlik = 7  }, // Titreme
                new() { HastalikId = grip.Id,     SemptomId = 1,  Agirlik = 6  }, // Baş ağrısı
                new() { HastalikId = grip.Id,     SemptomId = 25, Agirlik = 7  }, // Yorgunluk
                new() { HastalikId = grip.Id,     SemptomId = 24, Agirlik = 5  }, // Terleme
                new() { HastalikId = grip.Id,     SemptomId = 6,  Agirlik = 5  }, // Öksürük
                new() { HastalikId = grip.Id,     SemptomId = 16, Agirlik = 4  }, // İştahsızlık

                // ÜSYE: burun + boğaz belirtileri
                new() { HastalikId = usye.Id,     SemptomId = 8,  Agirlik = 9  }, // Burun akıntısı
                new() { HastalikId = usye.Id,     SemptomId = 9,  Agirlik = 8  }, // Burun tıkanıklığı
                new() { HastalikId = usye.Id,     SemptomId = 5,  Agirlik = 8  }, // Boğaz ağrısı
                new() { HastalikId = usye.Id,     SemptomId = 6,  Agirlik = 7  }, // Öksürük
                new() { HastalikId = usye.Id,     SemptomId = 1,  Agirlik = 4  }, // Baş ağrısı
                new() { HastalikId = usye.Id,     SemptomId = 22, Agirlik = 4  }, // Halsizlik
                new() { HastalikId = usye.Id,     SemptomId = 21, Agirlik = 3  }, // Düşük ateş

                // BRONŞİT
                new() { HastalikId = bronsit.Id,  SemptomId = 6,  Agirlik = 10 }, // Öksürük (baskın)
                new() { HastalikId = bronsit.Id,  SemptomId = 7,  Agirlik = 7  }, // Nefes darlığı
                new() { HastalikId = bronsit.Id,  SemptomId = 10, Agirlik = 7  }, // Hırıltılı nefes
                new() { HastalikId = bronsit.Id,  SemptomId = 30, Agirlik = 5  }, // Göğüs ağrısı
                new() { HastalikId = bronsit.Id,  SemptomId = 21, Agirlik = 4  }, // Ateş
                new() { HastalikId = bronsit.Id,  SemptomId = 22, Agirlik = 5  }, // Halsizlik

                // PNÖMONİ: ateş + solunum güçlüğü kombine
                new() { HastalikId = pnomoni.Id,  SemptomId = 21, Agirlik = 9  }, // Ateş (yüksek)
                new() { HastalikId = pnomoni.Id,  SemptomId = 7,  Agirlik = 8  }, // Nefes darlığı
                new() { HastalikId = pnomoni.Id,  SemptomId = 6,  Agirlik = 8  }, // Öksürük
                new() { HastalikId = pnomoni.Id,  SemptomId = 30, Agirlik = 7  }, // Göğüs ağrısı
                new() { HastalikId = pnomoni.Id,  SemptomId = 23, Agirlik = 6  }, // Titreme
                new() { HastalikId = pnomoni.Id,  SemptomId = 22, Agirlik = 7  }, // Halsizlik
                new() { HastalikId = pnomoni.Id,  SemptomId = 24, Agirlik = 5  }, // Terleme
                new() { HastalikId = pnomoni.Id,  SemptomId = 16, Agirlik = 5  }, // İştahsızlık

                // ASTIM: episodik hırıltı + nefes
                new() { HastalikId = astim.Id,    SemptomId = 7,  Agirlik = 10 }, // Nefes darlığı
                new() { HastalikId = astim.Id,    SemptomId = 10, Agirlik = 9  }, // Hırıltılı nefes
                new() { HastalikId = astim.Id,    SemptomId = 6,  Agirlik = 8  }, // Öksürük
                new() { HastalikId = astim.Id,    SemptomId = 30, Agirlik = 5  }, // Göğüs sıkışması

                // SİNÜZİT
                new() { HastalikId = sinuzit.Id,  SemptomId = 9,  Agirlik = 9  }, // Burun tıkanıklığı
                new() { HastalikId = sinuzit.Id,  SemptomId = 1,  Agirlik = 8  }, // Baş ağrısı
                new() { HastalikId = sinuzit.Id,  SemptomId = 8,  Agirlik = 7  }, // Burun akıntısı
                new() { HastalikId = sinuzit.Id,  SemptomId = 4,  Agirlik = 5  }, // Kulak ağrısı
                new() { HastalikId = sinuzit.Id,  SemptomId = 5,  Agirlik = 4  }, // Boğaz ağrısı
                new() { HastalikId = sinuzit.Id,  SemptomId = 21, Agirlik = 3  }, // Subfebril ateş

                // TÜBERKÜLOZ: kronik + gece terlemesi
                new() { HastalikId = tbc.Id,      SemptomId = 6,  Agirlik = 9  }, // Öksürük (kronik)
                new() { HastalikId = tbc.Id,      SemptomId = 26, Agirlik = 9  }, // Kilo kaybı
                new() { HastalikId = tbc.Id,      SemptomId = 24, Agirlik = 8  }, // Terleme (gece)
                new() { HastalikId = tbc.Id,      SemptomId = 21, Agirlik = 7  }, // Ateş (hafif)
                new() { HastalikId = tbc.Id,      SemptomId = 22, Agirlik = 7  }, // Halsizlik
                new() { HastalikId = tbc.Id,      SemptomId = 7,  Agirlik = 5  }, // Nefes darlığı
                new() { HastalikId = tbc.Id,      SemptomId = 16, Agirlik = 5  }, // İştahsızlık

                // FARANJİT
                new() { HastalikId = farenjit.Id, SemptomId = 5,  Agirlik = 10 }, // Boğaz ağrısı
                new() { HastalikId = farenjit.Id, SemptomId = 21, Agirlik = 6  }, // Ateş
                new() { HastalikId = farenjit.Id, SemptomId = 22, Agirlik = 5  }, // Halsizlik
                new() { HastalikId = farenjit.Id, SemptomId = 4,  Agirlik = 5  }, // Kulak ağrısı
                new() { HastalikId = farenjit.Id, SemptomId = 1,  Agirlik = 4  }, // Baş ağrısı
                new() { HastalikId = farenjit.Id, SemptomId = 16, Agirlik = 4  }, // İştahsızlık

                // KARDİYAK DEĞERLENDİRME: göğüs + diğer kritikler
                new() { HastalikId = kardiyak.Id, SemptomId = 30, Agirlik = 10 }, // Göğüs ağrısı
                new() { HastalikId = kardiyak.Id, SemptomId = 32, Agirlik = 8  }, // Baygınlık
                new() { HastalikId = kardiyak.Id, SemptomId = 7,  Agirlik = 8  }, // Nefes darlığı
                new() { HastalikId = kardiyak.Id, SemptomId = 31, Agirlik = 7  }, // Çarpıntı
                new() { HastalikId = kardiyak.Id, SemptomId = 24, Agirlik = 6  }, // Terleme
                new() { HastalikId = kardiyak.Id, SemptomId = 22, Agirlik = 5  }, // Halsizlik
                new() { HastalikId = kardiyak.Id, SemptomId = 18, Agirlik = 4  }, // Kol/çene ağrısı (kas)

                // ANJİN
                new() { HastalikId = anjin.Id,    SemptomId = 30, Agirlik = 10 }, // Göğüs ağrısı
                new() { HastalikId = anjin.Id,    SemptomId = 7,  Agirlik = 7  }, // Nefes darlığı
                new() { HastalikId = anjin.Id,    SemptomId = 32, Agirlik = 6  }, // Baygınlık
                new() { HastalikId = anjin.Id,    SemptomId = 24, Agirlik = 5  }, // Terleme
                new() { HastalikId = anjin.Id,    SemptomId = 31, Agirlik = 4  }, // Çarpıntı

                // ARİTMİ
                new() { HastalikId = aritmi.Id,   SemptomId = 31, Agirlik = 10 }, // Çarpıntı
                new() { HastalikId = aritmi.Id,   SemptomId = 32, Agirlik = 7  }, // Baygınlık
                new() { HastalikId = aritmi.Id,   SemptomId = 7,  Agirlik = 6  }, // Nefes darlığı
                new() { HastalikId = aritmi.Id,   SemptomId = 30, Agirlik = 5  }, // Göğüs ağrısı
                new() { HastalikId = aritmi.Id,   SemptomId = 25, Agirlik = 5  }, // Yorgunluk

                // HİPERTANSİF KRİZ
                new() { HastalikId = htkriz.Id,   SemptomId = 1,  Agirlik = 9  }, // Şiddetli baş ağrısı
                new() { HastalikId = htkriz.Id,   SemptomId = 3,  Agirlik = 8  }, // Baş dönmesi
                new() { HastalikId = htkriz.Id,   SemptomId = 12, Agirlik = 5  }, // Bulantı
                new() { HastalikId = htkriz.Id,   SemptomId = 30, Agirlik = 6  }, // Göğüs ağrısı
                new() { HastalikId = htkriz.Id,   SemptomId = 31, Agirlik = 4  }, // Çarpıntı

                // GASTROENTERİT
                new() { HastalikId = gastro.Id,   SemptomId = 13, Agirlik = 9  }, // Kusma
                new() { HastalikId = gastro.Id,   SemptomId = 12, Agirlik = 8  }, // Bulantı
                new() { HastalikId = gastro.Id,   SemptomId = 14, Agirlik = 9  }, // İshal
                new() { HastalikId = gastro.Id,   SemptomId = 11, Agirlik = 7  }, // Karın ağrısı
                new() { HastalikId = gastro.Id,   SemptomId = 21, Agirlik = 5  }, // Ateş
                new() { HastalikId = gastro.Id,   SemptomId = 16, Agirlik = 5  }, // İştahsızlık
                new() { HastalikId = gastro.Id,   SemptomId = 22, Agirlik = 4  }, // Halsizlik

                // APANDİSİT
                new() { HastalikId = apandis.Id,  SemptomId = 11, Agirlik = 10 }, // Karın ağrısı (sağ alt)
                new() { HastalikId = apandis.Id,  SemptomId = 21, Agirlik = 7  }, // Ateş
                new() { HastalikId = apandis.Id,  SemptomId = 12, Agirlik = 7  }, // Bulantı
                new() { HastalikId = apandis.Id,  SemptomId = 13, Agirlik = 5  }, // Kusma
                new() { HastalikId = apandis.Id,  SemptomId = 16, Agirlik = 7  }, // İştahsızlık
                new() { HastalikId = apandis.Id,  SemptomId = 22, Agirlik = 4  }, // Halsizlik

                // PEPTİK ÜLSER
                new() { HastalikId = ulser.Id,    SemptomId = 11, Agirlik = 9  }, // Karın ağrısı
                new() { HastalikId = ulser.Id,    SemptomId = 12, Agirlik = 7  }, // Bulantı
                new() { HastalikId = ulser.Id,    SemptomId = 13, Agirlik = 4  }, // Kusma
                new() { HastalikId = ulser.Id,    SemptomId = 16, Agirlik = 6  }, // İştahsızlık
                new() { HastalikId = ulser.Id,    SemptomId = 26, Agirlik = 4  }, // Kilo kaybı

                // GERD
                new() { HastalikId = gerd.Id,     SemptomId = 30, Agirlik = 8  }, // Göğüs yanması
                new() { HastalikId = gerd.Id,     SemptomId = 11, Agirlik = 7  }, // Karın ağrısı
                new() { HastalikId = gerd.Id,     SemptomId = 12, Agirlik = 6  }, // Bulantı
                new() { HastalikId = gerd.Id,     SemptomId = 5,  Agirlik = 5  }, // Boğaz ağrısı
                new() { HastalikId = gerd.Id,     SemptomId = 6,  Agirlik = 4  }, // Öksürük

                // KOLESİSTİT
                new() { HastalikId = kolesist.Id, SemptomId = 11, Agirlik = 10 }, // Karın ağrısı (sağ üst)
                new() { HastalikId = kolesist.Id, SemptomId = 12, Agirlik = 8  }, // Bulantı
                new() { HastalikId = kolesist.Id, SemptomId = 13, Agirlik = 7  }, // Kusma
                new() { HastalikId = kolesist.Id, SemptomId = 21, Agirlik = 6  }, // Ateş
                new() { HastalikId = kolesist.Id, SemptomId = 16, Agirlik = 5  }, // İştahsızlık

                // İBS
                new() { HastalikId = ibs.Id,      SemptomId = 11, Agirlik = 8  }, // Karın ağrısı
                new() { HastalikId = ibs.Id,      SemptomId = 14, Agirlik = 7  }, // İshal
                new() { HastalikId = ibs.Id,      SemptomId = 15, Agirlik = 7  }, // Kabızlık
                new() { HastalikId = ibs.Id,      SemptomId = 20, Agirlik = 6  }, // Şişlik
                new() { HastalikId = ibs.Id,      SemptomId = 12, Agirlik = 5  }, // Bulantı
                new() { HastalikId = ibs.Id,      SemptomId = 25, Agirlik = 4  }, // Yorgunluk

                // MİGREN
                new() { HastalikId = migren.Id,   SemptomId = 1,  Agirlik = 10 }, // Baş ağrısı (şiddetli)
                new() { HastalikId = migren.Id,   SemptomId = 12, Agirlik = 7  }, // Bulantı
                new() { HastalikId = migren.Id,   SemptomId = 13, Agirlik = 5  }, // Kusma
                new() { HastalikId = migren.Id,   SemptomId = 3,  Agirlik = 5  }, // Baş dönmesi
                new() { HastalikId = migren.Id,   SemptomId = 22, Agirlik = 4  }, // Halsizlik
                new() { HastalikId = migren.Id,   SemptomId = 2,  Agirlik = 3  }, // Boyun tutulması

                // GERİLİM TIPI BAŞ AĞRISI
                new() { HastalikId = gerilim.Id,  SemptomId = 1,  Agirlik = 9  }, // Baş ağrısı
                new() { HastalikId = gerilim.Id,  SemptomId = 2,  Agirlik = 7  }, // Boyun tutulması
                new() { HastalikId = gerilim.Id,  SemptomId = 18, Agirlik = 5  }, // Kas ağrısı
                new() { HastalikId = gerilim.Id,  SemptomId = 22, Agirlik = 4  }, // Halsizlik
                new() { HastalikId = gerilim.Id,  SemptomId = 25, Agirlik = 4  }, // Yorgunluk

                // MENENJİT: boyun sertliği kritik
                new() { HastalikId = menenjit.Id, SemptomId = 2,  Agirlik = 10 }, // Boyun tutulması
                new() { HastalikId = menenjit.Id, SemptomId = 1,  Agirlik = 9  }, // Baş ağrısı
                new() { HastalikId = menenjit.Id, SemptomId = 21, Agirlik = 9  }, // Yüksek ateş
                new() { HastalikId = menenjit.Id, SemptomId = 12, Agirlik = 6  }, // Bulantı
                new() { HastalikId = menenjit.Id, SemptomId = 13, Agirlik = 6  }, // Kusma
                new() { HastalikId = menenjit.Id, SemptomId = 22, Agirlik = 6  }, // Halsizlik
                new() { HastalikId = menenjit.Id, SemptomId = 3,  Agirlik = 4  }, // Baş dönmesi

                // VERTİGO
                new() { HastalikId = vertigo.Id,  SemptomId = 3,  Agirlik = 10 }, // Baş dönmesi
                new() { HastalikId = vertigo.Id,  SemptomId = 12, Agirlik = 7  }, // Bulantı
                new() { HastalikId = vertigo.Id,  SemptomId = 13, Agirlik = 5  }, // Kusma
                new() { HastalikId = vertigo.Id,  SemptomId = 22, Agirlik = 4  }, // Halsizlik
                new() { HastalikId = vertigo.Id,  SemptomId = 4,  Agirlik = 4  }, // Kulak ağrısı

                // İNME: ani nörolojik defisit
                new() { HastalikId = inme.Id,     SemptomId = 1,  Agirlik = 8  }, // Baş ağrısı (ani)
                new() { HastalikId = inme.Id,     SemptomId = 3,  Agirlik = 8  }, // Baş dönmesi
                new() { HastalikId = inme.Id,     SemptomId = 32, Agirlik = 8  }, // Baygınlık
                new() { HastalikId = inme.Id,     SemptomId = 12, Agirlik = 5  }, // Bulantı
                new() { HastalikId = inme.Id,     SemptomId = 13, Agirlik = 4  }, // Kusma

                // KAS-İSKELET ZORLANMASI
                new() { HastalikId = kasEkl.Id,   SemptomId = 18, Agirlik = 9  }, // Kas ağrısı
                new() { HastalikId = kasEkl.Id,   SemptomId = 17, Agirlik = 8  }, // Eklem ağrısı
                new() { HastalikId = kasEkl.Id,   SemptomId = 19, Agirlik = 7  }, // Sırt ağrısı
                new() { HastalikId = kasEkl.Id,   SemptomId = 20, Agirlik = 5  }, // Şişlik

                // ROMATOİD ARTRİT
                new() { HastalikId = romato.Id,   SemptomId = 17, Agirlik = 10 }, // Eklem ağrısı
                new() { HastalikId = romato.Id,   SemptomId = 20, Agirlik = 8  }, // Şişlik
                new() { HastalikId = romato.Id,   SemptomId = 25, Agirlik = 7  }, // Yorgunluk
                new() { HastalikId = romato.Id,   SemptomId = 22, Agirlik = 7  }, // Halsizlik
                new() { HastalikId = romato.Id,   SemptomId = 18, Agirlik = 6  }, // Kas ağrısı
                new() { HastalikId = romato.Id,   SemptomId = 21, Agirlik = 4  }, // Subfebril ateş

                // GUT ARTRİTİ
                new() { HastalikId = gut.Id,      SemptomId = 17, Agirlik = 10 }, // Eklem ağrısı (şiddetli)
                new() { HastalikId = gut.Id,      SemptomId = 20, Agirlik = 9  }, // Şişlik
                new() { HastalikId = gut.Id,      SemptomId = 29, Agirlik = 7  }, // Kızarıklık
                new() { HastalikId = gut.Id,      SemptomId = 22, Agirlik = 4  }, // Halsizlik

                // DİSKOPATİ
                new() { HastalikId = diskopat.Id, SemptomId = 19, Agirlik = 10 }, // Sırt ağrısı
                new() { HastalikId = diskopat.Id, SemptomId = 18, Agirlik = 7  }, // Kas ağrısı
                new() { HastalikId = diskopat.Id, SemptomId = 17, Agirlik = 6  }, // Eklem ağrısı

                // FİBROMİYALJİ
                new() { HastalikId = fibromi.Id,  SemptomId = 18, Agirlik = 9  }, // Kas ağrısı (yaygın)
                new() { HastalikId = fibromi.Id,  SemptomId = 25, Agirlik = 9  }, // Yorgunluk
                new() { HastalikId = fibromi.Id,  SemptomId = 22, Agirlik = 8  }, // Halsizlik
                new() { HastalikId = fibromi.Id,  SemptomId = 1,  Agirlik = 6  }, // Baş ağrısı
                new() { HastalikId = fibromi.Id,  SemptomId = 17, Agirlik = 7  }, // Eklem ağrısı
                new() { HastalikId = fibromi.Id,  SemptomId = 16, Agirlik = 5  }, // İştahsızlık

                // ANEMİ
                new() { HastalikId = anemi.Id,    SemptomId = 22, Agirlik = 9  }, // Halsizlik
                new() { HastalikId = anemi.Id,    SemptomId = 25, Agirlik = 8  }, // Yorgunluk
                new() { HastalikId = anemi.Id,    SemptomId = 3,  Agirlik = 7  }, // Baş dönmesi
                new() { HastalikId = anemi.Id,    SemptomId = 31, Agirlik = 6  }, // Çarpıntı
                new() { HastalikId = anemi.Id,    SemptomId = 7,  Agirlik = 5  }, // Nefes darlığı
                new() { HastalikId = anemi.Id,    SemptomId = 1,  Agirlik = 4  }, // Baş ağrısı
                new() { HastalikId = anemi.Id,    SemptomId = 26, Agirlik = 3  }, // Kilo kaybı

                // HİPOTİROİDİZM
                new() { HastalikId = hipotir.Id,  SemptomId = 25, Agirlik = 9  }, // Yorgunluk
                new() { HastalikId = hipotir.Id,  SemptomId = 22, Agirlik = 8  }, // Halsizlik
                new() { HastalikId = hipotir.Id,  SemptomId = 18, Agirlik = 6  }, // Kas ağrısı
                new() { HastalikId = hipotir.Id,  SemptomId = 15, Agirlik = 6  }, // Kabızlık
                new() { HastalikId = hipotir.Id,  SemptomId = 17, Agirlik = 5  }, // Eklem ağrısı
                new() { HastalikId = hipotir.Id,  SemptomId = 20, Agirlik = 4  }, // Şişlik (yüz)
                new() { HastalikId = hipotir.Id,  SemptomId = 1,  Agirlik = 4  }, // Baş ağrısı

                // HİPERTİROİDİZM
                new() { HastalikId = hipertir.Id, SemptomId = 31, Agirlik = 9  }, // Çarpıntı
                new() { HastalikId = hipertir.Id, SemptomId = 24, Agirlik = 8  }, // Terleme
                new() { HastalikId = hipertir.Id, SemptomId = 26, Agirlik = 7  }, // Kilo kaybı
                new() { HastalikId = hipertir.Id, SemptomId = 23, Agirlik = 6  }, // Titreme
                new() { HastalikId = hipertir.Id, SemptomId = 25, Agirlik = 6  }, // Yorgunluk
                new() { HastalikId = hipertir.Id, SemptomId = 7,  Agirlik = 5  }, // Nefes darlığı

                // DİYABET TİP 2
                new() { HastalikId = diabet.Id,   SemptomId = 25, Agirlik = 8  }, // Yorgunluk
                new() { HastalikId = diabet.Id,   SemptomId = 22, Agirlik = 7  }, // Halsizlik
                new() { HastalikId = diabet.Id,   SemptomId = 26, Agirlik = 6  }, // Kilo kaybı
                new() { HastalikId = diabet.Id,   SemptomId = 3,  Agirlik = 4  }, // Baş dönmesi
                new() { HastalikId = diabet.Id,   SemptomId = 28, Agirlik = 4  }, // Kaşıntı
                new() { HastalikId = diabet.Id,   SemptomId = 24, Agirlik = 4  }, // Terleme

                // MONONÜKLEOz
                new() { HastalikId = mono.Id,     SemptomId = 5,  Agirlik = 9  }, // Boğaz ağrısı
                new() { HastalikId = mono.Id,     SemptomId = 21, Agirlik = 8  }, // Ateş
                new() { HastalikId = mono.Id,     SemptomId = 22, Agirlik = 8  }, // Halsizlik
                new() { HastalikId = mono.Id,     SemptomId = 25, Agirlik = 8  }, // Yorgunluk (belirgin)
                new() { HastalikId = mono.Id,     SemptomId = 2,  Agirlik = 6  }, // Boyun tutulması (bezler)
                new() { HastalikId = mono.Id,     SemptomId = 16, Agirlik = 5  }, // İştahsızlık
                new() { HastalikId = mono.Id,     SemptomId = 20, Agirlik = 4  }, // Şişlik (dalak)

                // ÜRİNER SİSTEM ENFEKSİYONU
                new() { HastalikId = uti.Id,      SemptomId = 11, Agirlik = 8  }, // Karın ağrısı (alt)
                new() { HastalikId = uti.Id,      SemptomId = 21, Agirlik = 7  }, // Ateş
                new() { HastalikId = uti.Id,      SemptomId = 19, Agirlik = 6  }, // Sırt ağrısı (bel)
                new() { HastalikId = uti.Id,      SemptomId = 12, Agirlik = 4  }, // Bulantı
                new() { HastalikId = uti.Id,      SemptomId = 22, Agirlik = 4  }, // Halsizlik

                // BÖBREK TAŞI
                new() { HastalikId = bobrektasi.Id, SemptomId = 11, Agirlik = 10 }, // Karın ağrısı (kolik)
                new() { HastalikId = bobrektasi.Id, SemptomId = 19, Agirlik = 9  }, // Sırt ağrısı
                new() { HastalikId = bobrektasi.Id, SemptomId = 12, Agirlik = 7  }, // Bulantı
                new() { HastalikId = bobrektasi.Id, SemptomId = 13, Agirlik = 6  }, // Kusma
                new() { HastalikId = bobrektasi.Id, SemptomId = 21, Agirlik = 4  }, // Ateş

                // ALERJİ / ALERJİK RİNİT
                new() { HastalikId = alerji.Id,   SemptomId = 28, Agirlik = 9  }, // Kaşıntı
                new() { HastalikId = alerji.Id,   SemptomId = 27, Agirlik = 8  }, // Döküntü
                new() { HastalikId = alerji.Id,   SemptomId = 29, Agirlik = 7  }, // Kızarıklık
                new() { HastalikId = alerji.Id,   SemptomId = 8,  Agirlik = 7  }, // Burun akıntısı
                new() { HastalikId = alerji.Id,   SemptomId = 9,  Agirlik = 6  }, // Burun tıkanıklığı
                new() { HastalikId = alerji.Id,   SemptomId = 7,  Agirlik = 4  }, // Nefes darlığı
                new() { HastalikId = alerji.Id,   SemptomId = 20, Agirlik = 5  }, // Şişlik (ödem)

                // ANKSİYETE
                new() { HastalikId = anksiye.Id,  SemptomId = 31, Agirlik = 8  }, // Çarpıntı
                new() { HastalikId = anksiye.Id,  SemptomId = 3,  Agirlik = 7  }, // Baş dönmesi
                new() { HastalikId = anksiye.Id,  SemptomId = 7,  Agirlik = 6  }, // Nefes darlığı
                new() { HastalikId = anksiye.Id,  SemptomId = 24, Agirlik = 6  }, // Terleme
                new() { HastalikId = anksiye.Id,  SemptomId = 22, Agirlik = 6  }, // Halsizlik
                new() { HastalikId = anksiye.Id,  SemptomId = 1,  Agirlik = 5  }, // Baş ağrısı
                new() { HastalikId = anksiye.Id,  SemptomId = 25, Agirlik = 6  }, // Yorgunluk
                new() { HastalikId = anksiye.Id,  SemptomId = 18, Agirlik = 4  }, // Kas gerginliği

                // PANİK ATAK
                new() { HastalikId = panik.Id,    SemptomId = 31, Agirlik = 10 }, // Çarpıntı
                new() { HastalikId = panik.Id,    SemptomId = 7,  Agirlik = 9  }, // Nefes darlığı
                new() { HastalikId = panik.Id,    SemptomId = 32, Agirlik = 9  }, // Baygınlık
                new() { HastalikId = panik.Id,    SemptomId = 24, Agirlik = 8  }, // Terleme
                new() { HastalikId = panik.Id,    SemptomId = 23, Agirlik = 7  }, // Titreme
                new() { HastalikId = panik.Id,    SemptomId = 30, Agirlik = 7  }, // Göğüs sıkışması
                new() { HastalikId = panik.Id,    SemptomId = 3,  Agirlik = 6  }, // Baş dönmesi

                // DEPRESYON
                new() { HastalikId = depresyon.Id,SemptomId = 25, Agirlik = 9  }, // Yorgunluk
                new() { HastalikId = depresyon.Id,SemptomId = 22, Agirlik = 8  }, // Halsizlik
                new() { HastalikId = depresyon.Id,SemptomId = 16, Agirlik = 7  }, // İştahsızlık
                new() { HastalikId = depresyon.Id,SemptomId = 26, Agirlik = 5  }, // Kilo kaybı
                new() { HastalikId = depresyon.Id,SemptomId = 1,  Agirlik = 5  }, // Baş ağrısı
                new() { HastalikId = depresyon.Id,SemptomId = 18, Agirlik = 4  }, // Bedensel ağrı

                // KRONİK YORGUNLUK
                new() { HastalikId = kronikYor.Id,SemptomId = 25, Agirlik = 10 }, // Yorgunluk
                new() { HastalikId = kronikYor.Id,SemptomId = 22, Agirlik = 9  }, // Halsizlik
                new() { HastalikId = kronikYor.Id,SemptomId = 18, Agirlik = 6  }, // Kas ağrısı
                new() { HastalikId = kronikYor.Id,SemptomId = 1,  Agirlik = 6  }, // Baş ağrısı
                new() { HastalikId = kronikYor.Id,SemptomId = 17, Agirlik = 5  }, // Eklem ağrısı
                new() { HastalikId = kronikYor.Id,SemptomId = 3,  Agirlik = 4  }, // Baş dönmesi

                // ÜRTİKER
                new() { HastalikId = urtiker.Id,  SemptomId = 28, Agirlik = 10 }, // Kaşıntı
                new() { HastalikId = urtiker.Id,  SemptomId = 27, Agirlik = 9  }, // Döküntü
                new() { HastalikId = urtiker.Id,  SemptomId = 29, Agirlik = 8  }, // Kızarıklık
                new() { HastalikId = urtiker.Id,  SemptomId = 20, Agirlik = 6  }, // Şişlik (anjioödem)
                new() { HastalikId = urtiker.Id,  SemptomId = 7,  Agirlik = 4  }, // Nefes darlığı (anafilaksi riski)

                // KONTAKT DERMATİT
                new() { HastalikId = kontakt.Id,  SemptomId = 28, Agirlik = 10 }, // Kaşıntı
                new() { HastalikId = kontakt.Id,  SemptomId = 27, Agirlik = 9  }, // Döküntü
                new() { HastalikId = kontakt.Id,  SemptomId = 29, Agirlik = 8  }, // Kızarıklık

                // ZONA
                new() { HastalikId = zona.Id,     SemptomId = 27, Agirlik = 9  }, // Döküntü (band tarzı)
                new() { HastalikId = zona.Id,     SemptomId = 28, Agirlik = 8  }, // Kaşıntı
                new() { HastalikId = zona.Id,     SemptomId = 29, Agirlik = 7  }, // Kızarıklık
                new() { HastalikId = zona.Id,     SemptomId = 19, Agirlik = 6  }, // Sırt/yanı ağrısı
                new() { HastalikId = zona.Id,     SemptomId = 21, Agirlik = 4  }, // Ateş
                new() { HastalikId = zona.Id,     SemptomId = 22, Agirlik = 4  }, // Halsizlik
            };

            await context.HastalikSemptomlar.AddRangeAsync(iliskiler);
            await context.SaveChangesAsync();
        }

        // ── Test Kullanıcıları ────────────────────────────────────────
        if (await userManager.FindByEmailAsync("demo@semptomanaliz.com") == null)
        {
            await SeedTestKullanici(context, userManager,
                email: "demo@semptomanaliz.com",
                ad: "Furkan", soyad: "Aydın",
                yas: 22, cinsiyet: "Erkek", boy: 180, kilo: 78);
        }

        if (await userManager.FindByEmailAsync("ayse@semptomanaliz.com") == null)
        {
            await SeedTestKullanici(context, userManager,
                email: "ayse@semptomanaliz.com",
                ad: "Ayşe", soyad: "Kaya",
                yas: 35, cinsiyet: "Kadın", boy: 165, kilo: 62,
                kronik: "Hipertansiyon");
        }
    }

    private static async Task SeedTestKullanici(
        AppDbContext context, UserManager<Kullanici> userManager,
        string email, string ad, string soyad,
        int yas, string cinsiyet, decimal boy, decimal kilo,
        string? kronik = null)
    {
        var kullanici = new Kullanici
        {
            UserName = email, Email = email,
            Ad = ad, Soyad = soyad, EmailConfirmed = true
        };
        var r = await userManager.CreateAsync(kullanici, "Demo123!");
        if (!r.Succeeded) return;

        await userManager.AddToRoleAsync(kullanici, "Kullanici");

        // Sağlık profili
        var profil = new SaglikProfili
        {
            KullaniciId = kullanici.Id,
            Yas = yas, Cinsiyet = cinsiyet,
            Boy = boy, Kilo = kilo,
            KronikHastaliklar = kronik
        };
        context.SaglikProfilleri.Add(profil);
        await context.SaveChangesAsync();

        // Demo analizler
        var analizGruplari = new List<(int[] semptomIdler, int gunOncesi, string? not)>
        {
            (new[]{21,1,22,17}, 0,  "Sabahtan beri ateşim var, başım çok ağrıyor."),
            (new[]{21,1,22,17}, 14, "Geçen haftaki gibi hissediyorum."),
            (new[]{6,5,8},      30, "Soğuk aldım sanırım."),
            (new[]{16,18,19},   45, "Spor sonrası kas ağrıları."),
            (new[]{12,11,14},   60, "Mide bulanıyor, ishal var."),
        };

        foreach (var (semptomlar, gunOncesi, not) in analizGruplari)
        {
            var imza = OlusturImza(semptomlar);
            var oturum = new AnalizOturumu
            {
                KullaniciId = kullanici.Id,
                EkNotlar = not,
                SemptomImzasi = imza,
                OlusturulmaTarihi = DateTime.UtcNow.AddDays(-gunOncesi)
            };
            context.AnalizOturumlari.Add(oturum);
            await context.SaveChangesAsync();

            // Semptomlar
            foreach (var sid in semptomlar)
            {
                context.AnalizSemptomlari.Add(new AnalizSemptomu
                {
                    AnalizOturumuId = oturum.Id,
                    SemptomKatalogId = sid,
                    Siddet = semptomlar.Length > 2 ? SemptomSiddeti.Orta : SemptomSiddeti.Hafif,
                    SureGun = gunOncesi == 0 ? 2 : 1
                });
            }

            // BMI hesapla
            var boyM = boy / 100m;
            var bmi = Math.Round(kilo / (boyM * boyM), 1);
            var bmiKat = bmi < 18.5m ? BmiKategori.ZayifAltinda
                       : bmi < 25m   ? BmiKategori.Normal
                       : bmi < 30m   ? BmiKategori.Fazlakilolu
                       : BmiKategori.ObezeI;

            // Tekrar skoru
            var esikTarih = DateTime.UtcNow.AddDays(-gunOncesi - 30);
            var tekrar = await context.AnalizOturumlari
                .CountAsync(o => o.KullaniciId == kullanici.Id
                              && o.SemptomImzasi == imza
                              && o.OlusturulmaTarihi >= esikTarih
                              && o.Id != oturum.Id);

            var (aciliyetSkor, seviye, bolum, aciklama, genelYorum) =
                HesaplaTestSonuc(semptomlar, bmi, kronik, tekrar);

            var sonuc = new AnalizSonucu
            {
                AnalizOturumuId = oturum.Id,
                AciliyetSeviyesi = seviye,
                AciliyetSkoru = aciliyetSkor,
                OnerilenBolum = bolum,
                NedenAciklamasi = aciklama,
                GenelYorum = genelYorum,
                TekrarSkoru = tekrar,
                EnYakinTekrarGunOncesi = tekrar > 0 ? 14 : null,
                HesaplananBmi = bmi,
                BmiKategori = bmiKat,
                GunlukOnerilerJson = JsonSerializer.Serialize(new[]
                {
                    new { Ikon = "💧", Baslik = "Sıvı Tüketimi", Metin = "Günde en az 2-3 litre su tüketin." },
                    new { Ikon = "🛌", Baslik = "Dinlenme",      Metin = "Bol istirahat edin." },
                    new { Ikon = "🌡️", Baslik = "Takip",         Metin = "Belirtileri sabah/akşam not alın." }
                }),
                UyariGostergeleriJson = JsonSerializer.Serialize(new[]
                {
                    "Belirtileriniz 5 günden uzun sürer ve iyileşme görülmezse doktora başvurun.",
                    "Ateş 39°C üzerine çıkarsa vakit kaybetmeden sağlık kuruluşuna gidin."
                }),
                OlusturulmaTarihi = oturum.OlusturulmaTarihi
            };
            context.AnalizSonuclari.Add(sonuc);
            await context.SaveChangesAsync();

            // Olası durumlar
            var (d1, d2) = OlasiDurumlarHesapla(semptomlar);
            context.OlasiDurumlar.Add(new OlasiDurum
            {
                AnalizSonucuId = sonuc.Id, Ad = d1.Ad,
                SkorYuzdesi = d1.Skor, Aciklama = d1.Aciklama
            });
            if (d2 != null)
                context.OlasiDurumlar.Add(new OlasiDurum
                {
                    AnalizSonucuId = sonuc.Id, Ad = d2.Value.Ad,
                    SkorYuzdesi = d2.Value.Skor, Aciklama = d2.Value.Aciklama
                });
            await context.SaveChangesAsync();
        }
    }

    // ── Yardımcı ──────────────────────────────────────────────────────

    private static string OlusturImza(int[] ids)
    {
        var s = string.Join("|", ids.OrderBy(x => x));
        var h = SHA256.HashData(Encoding.UTF8.GetBytes(s));
        return Convert.ToHexString(h)[..12].ToLower();
    }

    private static (int skor, AciliyetSeviyesi seviye, string bolum, string aciklama, string genelYorum)
        HesaplaTestSonuc(int[] semptomlar, decimal bmi, string? kronik, int tekrar)
    {
        // Grip grubu (Ateş=21, Baş ağrısı=1, Halsizlik=22, Kas ağrısı=17)
        if (semptomlar.Contains(21) && semptomlar.Contains(1))
            return (63, AciliyetSeviyesi.Dikkat, "Dahiliye",
                "Yüksek ateş ve baş ağrısı kombinasyonu grip enfeksiyonunu işaret etmektedir. Semptomların 2 gündür sürmesi ve orta şiddette seyretmesi analiz skorunu etkilemiştir.",
                tekrar > 0
                    ? $"Belirtileriniz dikkat gerektiren bir tablo oluşturmaktadır. Bu semptom kombinasyonunu son 30 gün içinde {tekrar} kez yaşadığınız tespit edildi; tekrar eden belirtiler altta yatan bir durumu işaret edebilir."
                    : "Belirtileriniz dikkat gerektiren bir tablo oluşturmaktadır; yakın zamanda bir sağlık profesyoneliyle görüşmeniz tavsiye edilir. En yüksek olasılıklı değerlendirme 'Grip / İnfluenza' yönündedir.");

        // Solunum grubu (Öksürük=6, Boğaz ağrısı=5, Burun akıntısı=8)
        if (semptomlar.Contains(6) && semptomlar.Contains(5))
            return (38, AciliyetSeviyesi.Izle, "Kulak Burun Boğaz",
                "Öksürük ve boğaz ağrısı birlikteliği üst solunum yolu enfeksiyonuna işaret etmektedir. Belirtilerin hafif seyretmesi aciliyet skorunu düşük tutmaktadır.",
                "Belirtileriniz hafif-orta düzeyde seyrediyor; durumu takip etmeniz önerilir. En yüksek olasılıklı değerlendirme 'Üst Solunum Yolu Enfeksiyonu' yönündedir.");

        // Kas/eklem grubu (Eklem=16, Kas=18, Sırt=19)
        if (semptomlar.Contains(16) || semptomlar.Contains(18))
            return (28, AciliyetSeviyesi.Izle, "Ortopedi / Fizik Tedavi",
                "Eklem ve kas ağrıları birlikteliği kas-iskelet sistemi sorununa işaret etmektedir. Belirtilerin hafif seyretmesi aciliyet skorunu düşük tutmaktadır.",
                "Belirtileriniz hafif düzeyde seyrediyor. Fiziksel aktivite sonrası gelişen kas-iskelet belirtileri genellikle geçici niteliktedir; dinlenme önerilir.");

        // Sindirim grubu (Bulantı=12, Karın ağrısı=11, İshal=14)
        if (semptomlar.Contains(12) || semptomlar.Contains(11))
            return (45, AciliyetSeviyesi.Izle, "Gastroenteroloji",
                "Bulantı ve karın ağrısı birlikteliği sindirim sistemi rahatsızlığına işaret etmektedir. Belirtilerin kısa süreli olması aciliyet skorunu orta düzeyde tutmaktadır.",
                "Belirtileriniz izleme gerektiren bir tablo oluşturmaktadır. Sindirim sistemi belirtileri çoğunlukla 24-48 saat içinde geriler; hafif beslenme önerilir.");

        // Default
        return (18, AciliyetSeviyesi.Normal, "Aile Hekimliği",
            "Belirtileriniz normal sınırlar içinde değerlendirilmektedir.",
            "Genel tablonuz normal sınırlar içinde görünmektedir.");
    }

    private static ((string Ad, int Skor, string Aciklama) d1, (string Ad, int Skor, string Aciklama)? d2)
        OlasiDurumlarHesapla(int[] semptomlar)
    {
        if (semptomlar.Contains(21) && semptomlar.Contains(1))
            return (("Grip / İnfluenza", 82, "Yüksek ateş, kas ağrısı ve halsizlik kombinasyonu grip enfeksiyonunun tipik bulgularıdır."),
                    ("Üst Solunum Yolu Enfeksiyonu", 58, "Baş ağrısı eşliğinde solunum belirtileri de göz önüne alındı."));

        if (semptomlar.Contains(6) && semptomlar.Contains(5))
            return (("Üst Solunum Yolu Enfeksiyonu", 74, "Öksürük ve boğaz ağrısı birlikteliği bu tanıyı desteklemektedir."),
                    ("Sinüzit", 42, "Burun akıntısı eşliğinde sinüzit olasılığı da değerlendirildi."));

        if (semptomlar.Contains(16) || semptomlar.Contains(18))
            return (("Kas-İskelet Sorunu", 65, "Eklem ve kas ağrıları kas-iskelet sistemi patolojisine işaret etmektedir."), null);

        if (semptomlar.Contains(12) || semptomlar.Contains(11))
            return (("Akut Gastroenterit", 60, "Bulantı ve karın ağrısı sindirim sistemi enfeksiyonuna işaret etmektedir."),
                    ("Genel Halsizlik / Yorgunluk Sendromu", 35, "İştahsızlık eşliğinde halsizlik de değerlendirildi."));

        return (("Genel Halsizlik / Yorgunluk Sendromu", 40, "Belirtiler genel halsizlik tablosuna uymaktadır."), null);
    }
}
