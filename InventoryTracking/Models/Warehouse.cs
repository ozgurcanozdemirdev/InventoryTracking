using InventoryTracking.UI.Localization;
using System;

namespace InventoryTracking.Models
{
  /// <summary>
  /// Depo entity modeli
  /// </summary>
  public class Warehouse
  {
    public int DepoId { get; set; }
    public string DepoAdi { get; set; }
    public string Lokasyon { get; set; }
    public string Aciklama { get; set; }
    public bool AktifMi { get; set; }
    public string Durum => AktifMi ? "Aktif" : "Pasif";
    public DateTime OlusturulmaTarihi { get; set; }
    public DateTime GuncellemeTarihi { get; set; }
  }
}
