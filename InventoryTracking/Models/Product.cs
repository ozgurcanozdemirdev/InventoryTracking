using InventoryTracking.UI.Localization;
using System;

namespace InventoryTracking.Models
{
  /// <summary>
  /// Ürün entity modeli
  /// </summary>
  public class Product
  {
    public int UrunId { get; set; }
    public int MalzemeId { get; set; }
    public string Barkod { get; set; }
    public string UrunAdi { get; set; }
    public int KategoriId { get; set; }
    public string KategoriAdi { get; set; }
    public int BirimId { get; set; }
    public string BirimAdi { get; set; }
    public decimal Fiyat { get; set; }
    public int MinimumStok { get; set; }
    public int MaksimumStok { get; set; }
    public bool AktifMi { get; set; }
    public DateTime OlusturulmaTarihi { get; set; }
    public DateTime GuncellemeTarihi { get; set; }
  }
}
