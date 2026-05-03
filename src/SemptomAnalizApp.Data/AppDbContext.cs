using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SemptomAnalizApp.Core.Entities;
using SemptomAnalizApp.Core.Interfaces;

namespace SemptomAnalizApp.Data;

public class AppDbContext : IdentityDbContext<Kullanici>, IAnalizDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<SaglikProfili> SaglikProfilleri => Set<SaglikProfili>();
    public DbSet<SemptomKatalog> SemptomKatalog => Set<SemptomKatalog>();
    public DbSet<AnalizOturumu> AnalizOturumlari => Set<AnalizOturumu>();
    public DbSet<AnalizSemptomu> AnalizSemptomlari => Set<AnalizSemptomu>();
    public DbSet<AnalizSonucu> AnalizSonuclari => Set<AnalizSonucu>();
    public DbSet<OlasiDurum> OlasiDurumlar => Set<OlasiDurum>();

    // Teşhis motoru entity'leri
    public DbSet<Hastalik>       Hastaliklar       => Set<Hastalik>();
    public DbSet<Semptom>        Semptomlar        => Set<Semptom>();
    public DbSet<HastalikSemptom> HastalikSemptomlar => Set<HastalikSemptom>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Kullanici>(e =>
        {
            e.Property(u => u.Ad).HasMaxLength(80);
            e.Property(u => u.Soyad).HasMaxLength(80);

            e.HasOne(u => u.SaglikProfili)
             .WithOne(p => p.Kullanici)
             .HasForeignKey<SaglikProfili>(p => p.KullaniciId);

            e.HasMany(u => u.AnalizOturumlari)
             .WithOne(a => a.Kullanici)
             .HasForeignKey(a => a.KullaniciId);
        });

        builder.Entity<AnalizOturumu>(e =>
        {
            e.Property(a => a.EkNotlar).HasMaxLength(1000);
            e.Property(a => a.SemptomImzasi).HasMaxLength(64);

            e.HasOne(a => a.AnalizSonucu)
             .WithOne(s => s.AnalizOturumu)
             .HasForeignKey<AnalizSonucu>(s => s.AnalizOturumuId);

            e.HasMany(a => a.AnalizSemptomlari)
             .WithOne(s => s.AnalizOturumu)
             .HasForeignKey(s => s.AnalizOturumuId);

            // Tekrar tespiti sorgularını hızlandır
            e.HasIndex(a => a.SemptomImzasi);
        });

        builder.Entity<AnalizSonucu>(e =>
        {
            e.Property(s => s.OnerilenBolum).HasMaxLength(160);
            e.Property(s => s.NedenAciklamasi).HasMaxLength(2000);
            e.Property(s => s.GenelYorum).HasMaxLength(3000);

            e.HasMany(s => s.OlasiDurumlar)
             .WithOne(o => o.AnalizSonucu)
             .HasForeignKey(o => o.AnalizSonucuId);

            e.Property(s => s.HesaplananBmi).HasPrecision(5, 2);
        });

        builder.Entity<SaglikProfili>(e =>
        {
            e.Property(p => p.Cinsiyet).HasMaxLength(32);
            e.Property(p => p.KronikHastaliklar).HasMaxLength(500);
            e.Property(p => p.Notlar).HasMaxLength(1000);
            e.Property(p => p.Boy).HasPrecision(5, 1);
            e.Property(p => p.Kilo).HasPrecision(5, 1);
        });

        // SemptomKatalog → Semptom (nullable FK)
        // SetNull: bir Semptom silinirse katalog kaydı silinmez, FK null'a döner
        builder.Entity<SemptomKatalog>(e =>
        {
            e.Property(sk => sk.Ad).HasMaxLength(120);
            e.Property(sk => sk.IkonKodu).HasMaxLength(80);
            e.Property(sk => sk.Aciklama).HasMaxLength(500);

            e.HasOne(sk => sk.Semptom)
             .WithMany()
             .HasForeignKey(sk => sk.SemptomId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // OlasiDurum → Hastalik (nullable FK)
        // SetNull: bir Hastalik silinirse geçmiş sonuç kaydı silinmez, FK null'a döner
        builder.Entity<OlasiDurum>(e =>
        {
            e.Property(od => od.Ad).HasMaxLength(160);
            e.Property(od => od.Aciklama).HasMaxLength(1500);

            e.HasOne(od => od.Hastalik)
             .WithMany()
             .HasForeignKey(od => od.HastalikId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // Tablo adlarını Türkçe yap
        builder.Entity<SaglikProfili>().ToTable("SaglikProfilleri");
        builder.Entity<SemptomKatalog>().ToTable("SemptomKatalog");
        builder.Entity<AnalizOturumu>().ToTable("AnalizOturumlari");
        builder.Entity<AnalizSemptomu>().ToTable("AnalizSemptomlari");
        builder.Entity<AnalizSonucu>().ToTable("AnalizSonuclari");
        builder.Entity<OlasiDurum>().ToTable("OlasiDurumlar");

        // ── Teşhis motoru ────────────────────────────────────────────────
        builder.Entity<Hastalik>().ToTable("Hastaliklar");
        builder.Entity<Semptom>().ToTable("Semptomlar");

        // HastalikSemptom → composite primary key (HastalikId + SemptomId)
        // Kendi Id kolonu yok; iki foreign key birlikte PK oluşturur
        builder.Entity<HastalikSemptom>(e =>
        {
            e.ToTable("HastalikSemptomlar");

            // Composite PK
            e.HasKey(hs => new { hs.HastalikId, hs.SemptomId });

            // Agirlik: 1–10 arasında zorunlu
            e.Property(hs => hs.Agirlik)
             .IsRequired()
             .HasDefaultValue(5);

            // Hastalık → HastalikSemptom ilişkisi
            e.HasOne(hs => hs.Hastalik)
             .WithMany(h => h.HastalikSemptomlari)
             .HasForeignKey(hs => hs.HastalikId)
             .OnDelete(DeleteBehavior.Cascade);

            // Semptom → HastalikSemptom ilişkisi
            e.HasOne(hs => hs.Semptom)
             .WithMany(s => s.HastalikSemptomlari)
             .HasForeignKey(hs => hs.SemptomId)
             .OnDelete(DeleteBehavior.Cascade);

            // Bayesian sorgusu için kritik index: seçilen semptomlarla eşleşen hastalıkları bul
            e.HasIndex(hs => hs.SemptomId);
        });

        // Dashboard ve analiz sorgularını hızlandır
        builder.Entity<AnalizOturumu>(e =>
        {
            e.HasIndex(a => a.KullaniciId);
        });

        // Her analizde profil çekilir — unique index doğruluğu da garantiler
        builder.Entity<SaglikProfili>(e =>
        {
            e.HasIndex(p => p.KullaniciId).IsUnique();
        });

        // Hastalik yeni alanlarının precision ayarları
        builder.Entity<Hastalik>(e =>
        {
            e.Property(h => h.Ad).HasMaxLength(160);
            e.Property(h => h.Aciklama).HasMaxLength(1500);
            e.Property(h => h.OnerilenBolum).HasMaxLength(160);
            e.Property(h => h.ErkekKatsayi).HasPrecision(4, 2);
            e.Property(h => h.KadinKatsayi).HasPrecision(4, 2);
        });

        builder.Entity<Semptom>(e =>
        {
            e.Property(s => s.Ad).HasMaxLength(120);
            e.Property(s => s.Kategori).HasMaxLength(80);
        });
    }
}
