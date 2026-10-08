using System;

namespace InventoryTracking.Models
{
  public class Entities
  {
    /// <summary>
    /// Kategori entity modeli
    /// </summary>
    public class Kategori
    {
      public int KategoriId { get; set; }
      public string KategoriAdi { get; set; }
      public string Aciklama { get; set; }
      public DateTime OlusturulmaTarihi { get; set; }
      public DateTime GuncellemeTarihi { get; set; }
    }

    /// <summary>
    /// Birim entity modeli
    /// </summary>
    public class Birim
    {
      public int BirimId { get; set; }
      public string BirimAdi { get; set; }
      public string Kisaltma { get; set; }
      public DateTime OlusturulmaTarihi { get; set; }
    }

    /// <summary>
    /// Depo entity modeli
    /// </summary>
    public class Depo
    {
      public int DepoId { get; set; }
      public string DepoAdi { get; set; }
      public string Lokasyon { get; set; }
      public string Aciklama { get; set; }
      public bool AktifMi { get; set; }
      public DateTime OlusturulmaTarihi { get; set; }
      public DateTime GuncellemeTarihi { get; set; }
    }

    /// <summary>
    /// Barkod, açıklama ve birimin yetkili ana kaydı. MalzemeNo kullanıcı tarafından verilir.
    /// </summary>
    public class MalzemeKatalog
    {
      public int MalzemeId { get; set; }
      public string MalzemeNo { get; set; }
      public string Barkod { get; set; }
      public string Aciklama { get; set; }
      public int BirimId { get; set; }
      public string BirimAdi { get; set; }
    }

    /// <summary>
    /// Ürün entity modeli
    /// </summary>
    public class Urun
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

    /// <summary>
    /// Stok entity modeli
    /// </summary>
    public class Stok
    {
      public int StokId { get; set; }
      public int UrunId { get; set; }
      public int DepoId { get; set; }
      public string Barkod { get; set; }
      public string UrunAdi { get; set; }
      public string KategoriAdi { get; set; }
      public string BirimAdi { get; set; }
      public decimal Fiyat { get; set; }
      public int Miktar { get; set; }
      public string StokDurumu { get; set; }
      public DateTime OlusturulmaTarihi { get; set; }
      public DateTime GuncellemeTarihi { get; set; }
    }

    /// <summary>
    /// Stok Hareketi entity modeli (Audit)
    /// </summary>
    public class StokHareketi
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

    /// <summary>
    /// Ürün ekleme/düzenleme için ViewModel
    /// </summary>
    public class UrunViewModel
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

    /// <summary>
    /// API/Service yanıtları için result modeli
    /// </summary>
    public class OperationResult
    {
      public bool Success { get; set; }
      public string Message { get; set; }
      public object Data { get; set; }

      public static OperationResult Ok(string message = "İşlem başarılı", object data = null)
      {
        return new OperationResult { Success = true, Message = message, Data = data };
      }

      public static OperationResult Fail(string message)
      {
        return new OperationResult { Success = false, Message = message, Data = null };
      }
    }

    /// <summary>
    /// Stok durumu enum
    /// </summary>
    public enum StokDurumuTipi
    {
      Kritik = 1,    // Minimum stoğun altında
      Düşük = 2,     // Çeyreğinin altında
      Normal = 3     // Normal seviye
    }

    public class ZRaporuDto
    {
      public string Donem { get; set; }
      public int ToplamGirisMiktari { get; set; }
      public int ToplamCikisMiktari { get; set; }
    }
  }
}
