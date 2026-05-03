namespace SemptomAnalizApp.Web.ViewModels;

public class GecmisViewModel
{
    public List<GecmisYilGrubu> YilGruplari { get; set; } = [];
}

public class GecmisYilGrubu
{
    public int Yil { get; set; }
    public List<GecmisAyGrubu> AyGruplari { get; set; } = [];
}

public class GecmisAyGrubu
{
    public string AyAdi { get; set; } = string.Empty;
    public List<GecmisKayit> Kayitlar { get; set; } = [];
}

public class GecmisKayit
{
    public int OturumId { get; set; }
    public int SonucId { get; set; }
    public DateTime Tarih { get; set; }
    public List<string> Semptomlar { get; set; } = [];
    public string AciliyetEtiketi { get; set; } = string.Empty;
    public string AciliyetRengi { get; set; } = string.Empty;
    public int AciliyetSkoru { get; set; }
    public string OnerilenBolum { get; set; } = string.Empty;
    public string SemptomImzasi { get; set; } = string.Empty;
}
