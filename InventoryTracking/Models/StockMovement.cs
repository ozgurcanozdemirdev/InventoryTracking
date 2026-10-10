using InventoryTracking.UI.Localization;
using System;

namespace InventoryTracking.Models
{
  /// <summary>
  /// Stok Hareketi entity modeli (Audit)
  /// </summary>
  public class StockMovement
  {
    public int HareketId { get; set; }
    public int UrunId { get; set; }
    public int DepoId { get; set; }
    public int OncekiMiktar { get; set; }
    public int YeniMiktar { get; set; }
    public int DegisenMiktar { get; set; }
    public string HareketTipi { get; set; }
    public bool CikisMi { get; set; }
    public string Aciklama { get; set; }
    public string KullaniciAdi { get; set; }
    public DateTime HareketTarihi { get; set; }
  }
}
