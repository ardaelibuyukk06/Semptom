using Microsoft.AspNetCore.Identity;

namespace SemptomAnalizApp.Core.Entities;

public class Kullanici : IdentityUser
{
    public string Ad { get; set; } = string.Empty;
    public string Soyad { get; set; } = string.Empty;
    public DateTime KayitTarihi { get; set; } = DateTime.UtcNow;
    public bool Aktif { get; set; } = true;

    public SaglikProfili? SaglikProfili { get; set; }
    public ICollection<AnalizOturumu> AnalizOturumlari { get; set; } = [];
}
