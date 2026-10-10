using InventoryTracking.UI.Localization;
using System;

namespace InventoryTracking.Models
{
  /// <summary>
  /// Kategori entity modeli
  /// </summary>
  public class Category
  {
    public int KategoriId { get; set; }
    public string KategoriAdi { get; set; }
    public string Aciklama { get; set; }
    public DateTime OlusturulmaTarihi { get; set; }
    public DateTime GuncellemeTarihi { get; set; }
  }
}
