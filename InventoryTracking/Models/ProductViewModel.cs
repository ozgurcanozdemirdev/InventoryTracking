using InventoryTracking.UI.Localization;
using System;

namespace InventoryTracking.Models
{
  /// <summary>
  /// Ürün ekleme/düzenleme için ViewModel
  /// </summary>
  public class ProductViewModel
  {
    public int UrunId { get; set; }
    public string Barkod { get; set; }
    public string UrunAdi { get; set; }
    public int KategoriId { get; set; }
    public int BirimId { get; set; }
    public decimal Fiyat { get; set; }
    public int MinimumStok { get; set; }
    public int MaksimumStok { get; set; }
    public int DepoId { get; set; }
    public int Miktar { get; set; }
    public bool AktifMi { get; set; }
  }
}
