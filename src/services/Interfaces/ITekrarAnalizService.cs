namespace SemptomAnalizApp.Service.Interfaces;

public interface ITekrarAnalizService
{
    Task<(int tekrar, int? enYakinGun)> TespitEtAsync(string kullaniciId, string imza);
}
