using InventoryTracking.UI.Localization;
using System;

namespace InventoryTracking.Models
{
  /// <summary>
  /// Stok entity modeli
  /// </summary>
  public class Stock
  {
    public int StokId { get; set; }
    public int UrunId { get; set; }
    public int DepoId { get; set; }
    public string Barkod { get; set; }
    public string MalzemeNo { get; set; }
    public string UrunAdi { get; set; }
    public string KategoriAdi { get; set; }
    public string BirimAdi { get; set; }
    public decimal Fiyat { get; set; }
    public int Miktar { get; set; }
    public string StokDurumu { get; set; }
    // Hareket yoksa tarih boş kalır; stok kaydının güncelleme tarihiyle karıştırılmaz.
    public DateTime? SonHareketTarihi { get; set; }
    public DateTime OlusturulmaTarihi { get; set; }
    public DateTime GuncellemeTarihi { get; set; }
  }
}
