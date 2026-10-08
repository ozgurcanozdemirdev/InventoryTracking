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
      public string Durum => AktifMi ? "Aktif" : "Pasif";
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

    public class HareketRaporFiltresi
    {
      public DateTime Baslangic { get; set; }
      public DateTime Bitis { get; set; }
      public int? DepoId { get; set; }
      public string UrunArama { get; set; }
      // 0: tümü, 1: normal giriş/çıkış, 2: transfer, 3: sayım düzeltmesi.
      public int HareketGrubu { get; set; }
      public bool? CikisMi { get; set; }
    }

    public class HareketRaporOzeti
    {
      public string Donem { get; set; }
      public long NormalGiris { get; set; }
      public long NormalCikis { get; set; }
      public long TransferGiris { get; set; }
      public long TransferCikis { get; set; }
      public long DuzeltmeGiris { get; set; }
      public long DuzeltmeCikis { get; set; }
      public long NetDegisim { get; set; }
      public long HareketSayisi { get; set; }
    }

    public class HareketRaporDetayi
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
}
