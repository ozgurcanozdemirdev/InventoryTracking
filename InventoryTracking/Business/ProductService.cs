using InventoryTracking.UI.Localization;
using InventoryTracking.Connection;
using InventoryTracking.DataAccess;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using InventoryTracking.Models;

namespace InventoryTracking.Business
{
  /// <summary>
  /// Ürün Business Logic Service - Ürünlerle ilgili iş kurallarının işletildiği sınıf
  /// </summary>
  public class ProductService
  {
    // Bağımlılıklar (Dependencies) tanımlanıyor. 
    // Veritabanı işlemleri için repository nesneleri sınıf seviyesinde tutuluyor.
    private readonly ProductRepository _urunRepository;
    private readonly StockRepository _stokRepository;
    private readonly MaterialCatalogRepository _malzemeRepository;
    public ProductService()
    {
      // Sınıf örneklendiğinde (new ProductService()) repository'ler de ayağa kaldırılır.
      _urunRepository = new ProductRepository();
      _stokRepository = new StockRepository();
      _malzemeRepository = new MaterialCatalogRepository();
    }

    /// <summary>
    /// Barkod ile ürün arama
    /// </summary>
    public Product SearchByBarkod(string barkod)
    {
      // 1. İş Kuralı: Barkod boş gönderilemez. Boşsa sistemin patlamaması için hata fırlatılır.
      if (string.IsNullOrWhiteSpace(barkod))
      {
        throw new ArgumentException("Barkod boş olamaz");
      }

      // 2. İş Kuralı: Gelen barkodun başındaki ve sonundaki boşluklar Trim() ile temizlenip veritabanında aranır.
      return _urunRepository.GetByBarkod(barkod.Trim());
    }

    public Product SearchInactiveByBarkod(string barkod)
    {
      if (string.IsNullOrWhiteSpace(barkod))
        return null;
      return _urunRepository.GetInactiveByBarkod(barkod.Trim());
    }

    public OperationResult ReactivateProduct(int urunId)
    {
      return urunId > 0 ? _urunRepository.Reactivate(urunId) : OperationResult.Fail(UiText.Get("InvalidProduct"));
    }

    /// <summary>
    /// Tüm ürünleri getir
    /// </summary>
    public List<Product> GetAllProducts()
    {
      // Ekstra bir iş kuralı yok, doğrudan repository'den veriler listelenir.
      return _urunRepository.GetAll();
    }

    /// <summary>
    /// ID ile ürün getir
    /// </summary>
    public Product GetProductById(int urunId)
    {
      // İş Kuralı: ID değeri 0 veya negatif olamaz.
      if (urunId <= 0)
      {
        throw new ArgumentException(UiText.Get("InvalidProductId"));
      }

      return _urunRepository.GetById(urunId);
    }

    public OperationResult AddProduct(ProductViewModel model)
    {
      // 1. AŞAMA: Gelen verilerin doğruluğunu kontrol et (Validasyon).
      // Kullanıcı arayüzden boş bir isim, negatif bir fiyat veya sistemde zaten var olan 
      // bir barkod girmiş olabilir. ValidateUrunViewModel metodu bu iş kurallarını test eder.
      if (!ValidateUrunViewModel(model, out string validationError))
      {
        // Eğer veriler kurallara uymazsa işlemi hemen iptal ediyoruz.
        // Sistemi kitlememek (Exception fırlatmamak) için Success = false diyerek
        // hatayı güvenli bir OperationResult nesnesi içinde UI (Form) tarafına gönderiyoruz.
        return new OperationResult
        {
          Success = false,
          Message = validationError
        };
      }

      // 2. AŞAMA: Veri Dönüşümü (Mapping).
      // UI'dan gelen 'UrunViewModel' (Sadece ekranda gösterilen veriler) nesnesini, 
      // veritabanına kaydedilebilecek formattaki gerçek 'Urun' (Entity) nesnesine dönüştürüyoruz.
      var katalogKaydi = _malzemeRepository.GetByBarkod(model.Barkod);
      if (katalogKaydi == null)
        return OperationResult.Fail(UiText.Format("MaterialNumberMissing", model.Barkod.Trim()));
      var urun = new Product
      {
        MalzemeId = katalogKaydi.MalzemeId,
        // Trim() kullanarak kullanıcının metin kutusunda yanlışlıkla bıraktığı boşlukları temizliyoruz.
        // Soru işareti (?) null-conditional operatörüdür; eğer değer null gelirse kodun çökmesini engeller.
        Barkod = model.Barkod?.Trim(),
        UrunAdi = model.UrunAdi?.Trim(),
        KategoriId = model.KategoriId,
        BirimId = model.BirimId,
        Fiyat = model.Fiyat,
        MinimumStok = model.MinimumStok,
        MaksimumStok = model.MaksimumStok,
        // Sisteme yeni eklenen bir ürün mantıken satışa/kullanıma hazır durumdadır.
        AktifMi = true
      };
      if (model.DepoId <= 0 || model.Miktar < 0)
        return OperationResult.Fail(UiText.Get("SelectWarehouseQuantity"));
      // Ürün ve ilk stok aynı transaction içinde kaydedilir; biri başarısızsa ikisi de geri alınır.
      using (var connection = SqlDatabaseConnection.GetConnection())
      using (var transaction = connection.BeginTransaction())
      {
        var result = _urunRepository.Add(urun, transaction);
        if (!result.Success)
        {
          transaction.Rollback();
          return result;
        }

        var stokResult = _stokRepository.UpdateStok(urun.UrunId, model.DepoId, model.Miktar, "Giriş", UiText.Get("InitialStockDescription"), transaction);
        if (!stokResult.Success)
        {
          transaction.Rollback();
          return stokResult;
        }

        transaction.Commit();
        return result;
      }
    }

    /// <summary>
    /// Mevcut bir ürünün bilgilerini günceller.
    /// </summary>
    public OperationResult UpdateProduct(ProductViewModel model, string duzeltmeNedeni = null)
    {
      // 1. AŞAMA: Güvenlik ve Mantık Kontrolü.
      // Güncellenecek ürünün veritabanında mutlaka bir karşılığı (UrunId) olmak zorundadır.
      // 0 veya negatif bir ID geldiyse (yanlış parametre yollandıysa) işlemi anında reddediyoruz.
      if (model == null || model.UrunId <= 0)
      {
        return new OperationResult
        {
          Success = false,
          Message = UiText.Get("InvalidProductId")
        };
      }

      // 2. AŞAMA: Validasyon (Doğrulama).
      // Güncellenen verilerin (örneğin yeni girilen fiyatın veya ismin) 
      // sistemsel kurallara uygun olup olmadığını tekrar test ediyoruz.
      if (!ValidateUrunViewModel(model, out string validationError))
      {
        return new OperationResult
        {
          Success = false,
          Message = validationError
        };
      }

      // 3. AŞAMA: Veri Dönüşümü (Mapping).
      // Arayüzden (Form'dan) gelen güncel bilgileri alıp yeni bir 'Urun' nesnesine paketliyoruz.
      var urun = new Product
      {
        // model.UrunId'yi atamak çok kritik; aksi takdirde EF veya SQL hangi satırı güncelleyeceğini bilemez.
        UrunId = model.UrunId,
        Barkod = model.Barkod?.Trim(),
        UrunAdi = model.UrunAdi?.Trim(),
        KategoriId = model.KategoriId,
        BirimId = model.BirimId,
        Fiyat = model.Fiyat,
        MinimumStok = model.MinimumStok,
        MaksimumStok = model.MaksimumStok,
        // Güncelleme ekranında kullanıcı ürünü pasife çekmiş (AktifMi tick'ini kaldırmış) olabilir.
        // Bu yüzden varsayılan değer değil, arayüzdeki güncel kararı uyguluyoruz.
        AktifMi = model.AktifMi
      };
      if (model.DepoId <= 0 || model.Miktar < 0)
        return OperationResult.Fail(UiText.Get("SelectWarehouseQuantity"));
      // Ekrandaki miktar da düzenlemenin parçasıdır; açıklama ve stok tek transaction'dadır.
      using (var connection = SqlDatabaseConnection.GetConnection())
      using (var transaction = connection.BeginTransaction(System.Data.IsolationLevel.Serializable))
      {
        var mevcut = _stokRepository.GetByUrunAndDepo(model.UrunId, model.DepoId, transaction);
        int fark = checked(model.Miktar - (mevcut?.Miktar ?? 0));
        if (fark != 0 && (string.IsNullOrWhiteSpace(duzeltmeNedeni) || duzeltmeNedeni.Trim().Length > 500))
        {
          transaction.Rollback();
          return OperationResult.Fail(UiText.Get("StockAdjustmentReason"));
        }

        var result = _urunRepository.Update(urun, transaction);
        if (!result.Success)
        {
          transaction.Rollback();
          return result;
        }

        if (fark != 0 || mevcut == null)
        {
          var stokResult = _stokRepository.UpdateStok(model.UrunId, model.DepoId, fark, "Sayım Düzeltmesi", fark == 0 ? "" : duzeltmeNedeni.Trim(), transaction);
          if (!stokResult.Success)
          {
            transaction.Rollback();
            return stokResult;
          }
        }

        transaction.Commit();
        return result;
      }
    }

    /// <summary>
    /// Ürünü sistemden kaldırır (Soft Delete / Mantıksal Silme mantığı ile).
    /// </summary>
    public OperationResult DeleteProduct(int urunId)
    {
      // 1. AŞAMA: Güvenlik Kontrolü.
      // Silinmek istenen ID'nin geçerli bir sayı olup olmadığını kontrol ediyoruz.
      if (urunId <= 0)
      {
        return new OperationResult
        {
          Success = false,
          Message = UiText.Get("InvalidProductId")
        };
      }

      // 2. AŞAMA: Silme İşlemi (Soft Delete).
      // Kurumsal sistemlerde veriler "DELETE FROM Urun..." komutuyla veritabanından kalıcı olarak SİLİNMEZ.
      // Çünkü bu ürün geçmişte faturalarda veya stok hareketlerinde (Loglarda) kullanılmıştır.
      // Kalıcı silinirse ilişkisel veritabanı (Foreign Key) çöker.
      // Bunun yerine Repository içindeki Delete metodu, "AktifMi = 0" (false) yaparak ürünü sistemde sadece gizler.
      return _urunRepository.Delete(urunId);
    }

    /// <summary>
    /// Model validasyonu - Merkezi doğrulama kuralları
    /// </summary>
    private bool ValidateUrunViewModel(ProductViewModel model, out string errorMessage)
    {
      errorMessage = string.Empty;
      // Arka arkaya iş kuralları denetleniyor. Herhangi bir kural ihlal edilirse "false" dönüp işlem kesilir.
      if (model == null)
      {
        errorMessage = UiText.Get("NullModel");
        return false;
      }

      if (string.IsNullOrWhiteSpace(model.Barkod))
      {
        errorMessage = "Barkod boş olamaz";
        return false;
      }

      if (string.IsNullOrWhiteSpace(model.UrunAdi))
      {
        errorMessage = UiText.Get("ProductNameRequired");
        return false;
      }

      if (model.KategoriId <= 0)
      {
        errorMessage = UiText.Get("SelectCategory");
        return false;
      }

      if (model.BirimId <= 0)
      {
        errorMessage = UiText.Get("SelectUnit");
        return false;
      }

      if (model.Fiyat < 0)
      {
        errorMessage = UiText.Get("PriceNegative");
        return false;
      }

      if (model.MinimumStok < 0 || model.MaksimumStok < 0)
      {
        errorMessage = UiText.Get("StockLimitsNegative");
        return false;
      }

      if (model.MinimumStok > model.MaksimumStok)
      {
        errorMessage = UiText.Get("StockLimitsOrder");
        return false;
      }

      // Ürün kaydı ancak ana katalogdaki barkodla açılır; ad ve birim katalogla aynı kalır.
      var malzeme = _malzemeRepository.GetByBarkod(model.Barkod);
      if (malzeme == null)
      {
        errorMessage = UiText.Format("MaterialNumberMissing", model.Barkod.Trim());
        return false;
      }

      if (malzeme.BirimId != model.BirimId || !string.Equals(malzeme.Aciklama, model.UrunAdi.Trim(), StringComparison.Ordinal))
      {
        errorMessage = UiText.Get("ProductCatalogMatch");
        return false;
      }

      // Düzenleme sırasında ürünün kendisini dışarıda bırakıyoruz. Aynı adın
      // farklı büyük/küçük harfle yeniden eklenmesini kullanıcıya erkenden bildiriyoruz.
      if (_urunRepository.IsNameTaken(model.UrunAdi, model.UrunId))
      {
        errorMessage = UiText.Get("ProductNameExists");
        return false;
      }

      return true; // Tüm testleri geçerse onay verilir.
    }
  }
}
