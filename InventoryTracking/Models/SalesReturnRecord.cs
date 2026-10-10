using System;

namespace InventoryTracking.Models
{
  public class SalesReturnRecord
  {
    public long IslemId { get; set; }
    public DateTime IslemTarihi { get; set; }
    public string IslemTuru { get; set; }
    public string UrunAdi { get; set; }
    public string Barkod { get; set; }
    public string DepoAdi { get; set; }
    public int Miktar { get; set; }
    public decimal BirimFiyat { get; set; }
    public decimal ToplamTutar { get; set; }
    public string KullaniciAdi { get; set; }
  }
}
