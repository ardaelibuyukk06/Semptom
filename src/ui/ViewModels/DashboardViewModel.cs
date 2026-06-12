namespace SemptomAnalizApp.Web.ViewModels;

public class DashboardViewModel
{
    public string KullaniciAd { get; set; } = string.Empty;
    public int ToplamAnalizSayisi { get; set; }
    public DateTime? SonAnalizTarihi { get; set; }
    public int TekrarlayaniSemptomSayisi { get; set; }
    public string RiskOzeti { get; set; } = "Normal";
    public string RiskRengi { get; set; } = "success";

    public bool ProfilTamamlandi { get; set; }
    public List<SonAnalizSatiri> SonAnalizler { get; set; } = [];

    // Aciliyet trendi — kronolojik sıra, son 10 analiz
    public List<int> TrendSkorlar { get; set; } = [];
    public List<string> TrendEtiketler { get; set; } = [];
}

public class SonAnalizSatiri
{
    public int Id { get; set; }
    public DateTime Tarih { get; set; }
    public string AnaSemptomlar { get; set; } = string.Empty;
    public string AciliyetEtiketi { get; set; } = string.Empty;
    public string AciliyetRengi { get; set; } = string.Empty;
    public int AciliyetSkoru { get; set; }
}
