using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SemptomAnalizApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialSqlite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Ad = table.Column<string>(type: "TEXT", nullable: false),
                    Soyad = table.Column<string>(type: "TEXT", nullable: false),
                    KayitTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: true),
                    SecurityStamp = table.Column<string>(type: "TEXT", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Hastaliklar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ad = table.Column<string>(type: "TEXT", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: false),
                    OnerilenBolum = table.Column<string>(type: "TEXT", nullable: false),
                    Prevalans = table.Column<int>(type: "INTEGER", nullable: false),
                    MinYas = table.Column<int>(type: "INTEGER", nullable: true),
                    MaxYas = table.Column<int>(type: "INTEGER", nullable: true),
                    ErkekKatsayi = table.Column<decimal>(type: "TEXT", precision: 4, scale: 2, nullable: false),
                    KadinKatsayi = table.Column<decimal>(type: "TEXT", precision: 4, scale: 2, nullable: false),
                    AkutMu = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hastaliklar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Semptomlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ad = table.Column<string>(type: "TEXT", nullable: false),
                    KritikMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    Kategori = table.Column<string>(type: "TEXT", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Semptomlar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnalizOturumlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    KullaniciId = table.Column<string>(type: "TEXT", nullable: false),
                    EkNotlar = table.Column<string>(type: "TEXT", nullable: true),
                    SemptomImzasi = table.Column<string>(type: "TEXT", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalizOturumlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnalizOturumlari_AspNetUsers_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderKey = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SaglikProfilleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    KullaniciId = table.Column<string>(type: "TEXT", nullable: false),
                    Yas = table.Column<int>(type: "INTEGER", nullable: false),
                    Cinsiyet = table.Column<string>(type: "TEXT", nullable: false),
                    Boy = table.Column<decimal>(type: "TEXT", precision: 5, scale: 1, nullable: false),
                    Kilo = table.Column<decimal>(type: "TEXT", precision: 5, scale: 1, nullable: false),
                    KronikHastaliklar = table.Column<string>(type: "TEXT", nullable: true),
                    Notlar = table.Column<string>(type: "TEXT", nullable: true),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaglikProfilleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaglikProfilleri_AspNetUsers_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HastalikSemptomlar",
                columns: table => new
                {
                    HastalikId = table.Column<int>(type: "INTEGER", nullable: false),
                    SemptomId = table.Column<int>(type: "INTEGER", nullable: false),
                    Agirlik = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 5)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HastalikSemptomlar", x => new { x.HastalikId, x.SemptomId });
                    table.ForeignKey(
                        name: "FK_HastalikSemptomlar_Hastaliklar_HastalikId",
                        column: x => x.HastalikId,
                        principalTable: "Hastaliklar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HastalikSemptomlar_Semptomlar_SemptomId",
                        column: x => x.SemptomId,
                        principalTable: "Semptomlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SemptomKatalog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ad = table.Column<string>(type: "TEXT", nullable: false),
                    Kategori = table.Column<int>(type: "INTEGER", nullable: false),
                    IkonKodu = table.Column<string>(type: "TEXT", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false),
                    SemptomId = table.Column<int>(type: "INTEGER", nullable: true),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SemptomKatalog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SemptomKatalog_Semptomlar_SemptomId",
                        column: x => x.SemptomId,
                        principalTable: "Semptomlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AnalizSonuclari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AnalizOturumuId = table.Column<int>(type: "INTEGER", nullable: false),
                    AciliyetSeviyesi = table.Column<int>(type: "INTEGER", nullable: false),
                    AciliyetSkoru = table.Column<int>(type: "INTEGER", nullable: false),
                    OnerilenBolum = table.Column<string>(type: "TEXT", nullable: false),
                    NedenAciklamasi = table.Column<string>(type: "TEXT", nullable: false),
                    GenelYorum = table.Column<string>(type: "TEXT", nullable: false),
                    TekrarSkoru = table.Column<int>(type: "INTEGER", nullable: false),
                    EnYakinTekrarGunOncesi = table.Column<int>(type: "INTEGER", nullable: true),
                    HesaplananBmi = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    BmiKategori = table.Column<int>(type: "INTEGER", nullable: false),
                    GunlukOnerilerJson = table.Column<string>(type: "TEXT", nullable: false),
                    UyariGostergeleriJson = table.Column<string>(type: "TEXT", nullable: false),
                    ClaudeAnalizJson = table.Column<string>(type: "TEXT", nullable: true),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalizSonuclari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnalizSonuclari_AnalizOturumlari_AnalizOturumuId",
                        column: x => x.AnalizOturumuId,
                        principalTable: "AnalizOturumlari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnalizSemptomlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AnalizOturumuId = table.Column<int>(type: "INTEGER", nullable: false),
                    SemptomKatalogId = table.Column<int>(type: "INTEGER", nullable: false),
                    Siddet = table.Column<int>(type: "INTEGER", nullable: false),
                    SureGun = table.Column<int>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalizSemptomlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnalizSemptomlari_AnalizOturumlari_AnalizOturumuId",
                        column: x => x.AnalizOturumuId,
                        principalTable: "AnalizOturumlari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnalizSemptomlari_SemptomKatalog_SemptomKatalogId",
                        column: x => x.SemptomKatalogId,
                        principalTable: "SemptomKatalog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OlasiDurumlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AnalizSonucuId = table.Column<int>(type: "INTEGER", nullable: false),
                    Ad = table.Column<string>(type: "TEXT", nullable: false),
                    SkorYuzdesi = table.Column<int>(type: "INTEGER", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: false),
                    HastalikId = table.Column<int>(type: "INTEGER", nullable: true),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OlasiDurumlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OlasiDurumlar_AnalizSonuclari_AnalizSonucuId",
                        column: x => x.AnalizSonucuId,
                        principalTable: "AnalizSonuclari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OlasiDurumlar_Hastaliklar_HastalikId",
                        column: x => x.HastalikId,
                        principalTable: "Hastaliklar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnalizOturumlari_KullaniciId",
                table: "AnalizOturumlari",
                column: "KullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalizOturumlari_SemptomImzasi",
                table: "AnalizOturumlari",
                column: "SemptomImzasi");

            migrationBuilder.CreateIndex(
                name: "IX_AnalizSemptomlari_AnalizOturumuId",
                table: "AnalizSemptomlari",
                column: "AnalizOturumuId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalizSemptomlari_SemptomKatalogId",
                table: "AnalizSemptomlari",
                column: "SemptomKatalogId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalizSonuclari_AnalizOturumuId",
                table: "AnalizSonuclari",
                column: "AnalizOturumuId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HastalikSemptomlar_SemptomId",
                table: "HastalikSemptomlar",
                column: "SemptomId");

            migrationBuilder.CreateIndex(
                name: "IX_OlasiDurumlar_AnalizSonucuId",
                table: "OlasiDurumlar",
                column: "AnalizSonucuId");

            migrationBuilder.CreateIndex(
                name: "IX_OlasiDurumlar_HastalikId",
                table: "OlasiDurumlar",
                column: "HastalikId");

            migrationBuilder.CreateIndex(
                name: "IX_SaglikProfilleri_KullaniciId",
                table: "SaglikProfilleri",
                column: "KullaniciId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SemptomKatalog_SemptomId",
                table: "SemptomKatalog",
                column: "SemptomId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalizSemptomlari");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "HastalikSemptomlar");

            migrationBuilder.DropTable(
                name: "OlasiDurumlar");

            migrationBuilder.DropTable(
                name: "SaglikProfilleri");

            migrationBuilder.DropTable(
                name: "SemptomKatalog");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AnalizSonuclari");

            migrationBuilder.DropTable(
                name: "Hastaliklar");

            migrationBuilder.DropTable(
                name: "Semptomlar");

            migrationBuilder.DropTable(
                name: "AnalizOturumlari");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
