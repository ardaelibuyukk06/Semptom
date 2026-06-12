namespace SemptomAnalizApp.Core.Entities;

public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }
}
