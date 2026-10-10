using InventoryTracking.UI.Localization;
using System;

namespace InventoryTracking.Models
{
  public class MovementReportDetail
  {
    public int HareketId { get; set; }
    public DateTime HareketTarihi { get; set; }
    public string UrunAdi { get; set; }
    public string Barkod { get; set; }
    public string DepoAdi { get; set; }
    public string HareketTipi { get; set; }
    public string Yon { get; set; }
    public int DegisenMiktar { get; set; }
    public int OncekiMiktar { get; set; }
    public int YeniMiktar { get; set; }
    public string KullaniciAdi { get; set; }
    public string Aciklama { get; set; }
    public int? TransferId { get; set; }
  }
}
