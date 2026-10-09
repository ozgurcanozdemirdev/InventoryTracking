using InventoryTracking.Localization;
using InventoryTracking.Connection;
using InventoryTracking.DataAccess;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using static InventoryTracking.Models.Entities;

namespace InventoryTracking.Business
{
  public class Services
  {
    // Mevcut formların kullandığı iç içe servis tipleri için korunur.

    /// <summary>
    /// Ürün Business Logic Service - Ürünlerle ilgili iş kurallarının işletildiği sınıf
    /// </summary>
    public class UrunService
    {
      // Bağımlılıklar (Dependencies) tanımlanıyor. 
      // Veritabanı işlemleri için repository nesneleri sınıf seviyesinde tutuluyor.
      private readonly UrunRepository _urunRepository;
      private readonly StokRepository _stokRepository;
      private readonly MalzemeKatalogRepository _malzemeRepository;

      public UrunService()
      {
        // Sınıf örneklendiğinde (new UrunService()) repository'ler de ayağa kaldırılır.
        _urunRepository = new UrunRepository();
        _stokRepository = new StokRepository();
        _malzemeRepository = new MalzemeKatalogRepository();
      }

      /// <summary>
      /// Barkod ile ürün arama
      /// </summary>
      public Urun SearchByBarkod(string barkod)
      {
        // 1. İş Kuralı: Barkod boş gönderilemez. Boşsa sistemin patlamaması için hata fırlatılır.
        if (string.IsNullOrWhiteSpace(barkod))
        {
          throw new ArgumentException("Barkod boş olamaz");
        }

        // 2. İş Kuralı: Gelen barkodun başındaki ve sonundaki boşluklar Trim() ile temizlenip veritabanında aranır.
        return _urunRepository.GetByBarkod(barkod.Trim());
      }

      public Urun SearchInactiveByBarkod(string barkod)
      {
        if (string.IsNullOrWhiteSpace(barkod)) return null;
        return _urunRepository.GetInactiveByBarkod(barkod.Trim());
      }

      public OperationResult ReactivateProduct(int urunId)
      {
        return urunId > 0 ? _urunRepository.Reactivate(urunId)
          : OperationResult.Fail(UiText.Get("InvalidProduct"));
      }

      /// <summary>
      /// Tüm ürünleri getir
      /// </summary>
      public List<Urun> GetAllProducts()
      {
        // Ekstra bir iş kuralı yok, doğrudan repository'den veriler listelenir.
        return _urunRepository.GetAll();
      }

      /// <summary>
      /// ID ile ürün getir
      /// </summary>
      public Urun GetProductById(int urunId)
      {
        // İş Kuralı: ID değeri 0 veya negatif olamaz.
        if (urunId <= 0)
        {
          throw new ArgumentException(UiText.Get("InvalidProductId"));
        }

        return _urunRepository.GetById(urunId);
      }

      public OperationResult AddProduct(UrunViewModel model)
      {
        // 1. AŞAMA: Gelen verilerin doğruluğunu kontrol et (Validasyon).
        // Kullanıcı arayüzden boş bir isim, negatif bir fiyat veya sistemde zaten var olan 
        // bir barkod girmiş olabilir. ValidateUrunViewModel metodu bu iş kurallarını test eder.
        if (!ValidateUrunViewModel(model, out string validationError))
        {
          // Eğer veriler kurallara uymazsa işlemi hemen iptal ediyoruz.
          // Sistemi kitlememek (Exception fırlatmamak) için Success = false diyerek
          // hatayı güvenli bir OperationResult nesnesi içinde UI (Form) tarafına gönderiyoruz.
          return new OperationResult { Success = false, Message = validationError };
        }

        // 2. AŞAMA: Veri Dönüşümü (Mapping).
        // UI'dan gelen 'UrunViewModel' (Sadece ekranda gösterilen veriler) nesnesini, 
        // veritabanına kaydedilebilecek formattaki gerçek 'Urun' (Entity) nesnesine dönüştürüyoruz.
        var katalogKaydi = _malzemeRepository.GetByBarkod(model.Barkod);
        if (katalogKaydi == null)
          return OperationResult.Fail(UiText.Format("MaterialNumberMissing", model.Barkod.Trim()));

        var urun = new Urun
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
        using (var connection = SqlDBConnection.GetConnection())
        using (var transaction = connection.BeginTransaction())
        {
          var result = _urunRepository.Add(urun, transaction);
          if (!result.Success) { transaction.Rollback(); return result; }
          var stokResult = _stokRepository.UpdateStok(urun.UrunId, model.DepoId, model.Miktar,
            "Giriş", UiText.Get("InitialStockDescription"), transaction);
          if (!stokResult.Success) { transaction.Rollback(); return stokResult; }
          transaction.Commit();
          return result;
        }
      }

      /// <summary>
      /// Mevcut bir ürünün bilgilerini günceller.
      /// </summary>
      public OperationResult UpdateProduct(UrunViewModel model, string duzeltmeNedeni = null)
      {
        // 1. AŞAMA: Güvenlik ve Mantık Kontrolü.
        // Güncellenecek ürünün veritabanında mutlaka bir karşılığı (UrunId) olmak zorundadır.
        // 0 veya negatif bir ID geldiyse (yanlış parametre yollandıysa) işlemi anında reddediyoruz.
        if (model == null || model.UrunId <= 0)
        {
          return new OperationResult { Success = false, Message = UiText.Get("InvalidProductId") };
        }

        // 2. AŞAMA: Validasyon (Doğrulama).
        // Güncellenen verilerin (örneğin yeni girilen fiyatın veya ismin) 
        // sistemsel kurallara uygun olup olmadığını tekrar test ediyoruz.
        if (!ValidateUrunViewModel(model, out string validationError))
        {
          return new OperationResult { Success = false, Message = validationError };
        }

        // 3. AŞAMA: Veri Dönüşümü (Mapping).
        // Arayüzden (Form'dan) gelen güncel bilgileri alıp yeni bir 'Urun' nesnesine paketliyoruz.
        var urun = new Urun
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
        using (var connection = SqlDBConnection.GetConnection())
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
          if (!result.Success) { transaction.Rollback(); return result; }
          if (fark != 0 || mevcut == null)
          {
            var stokResult = _stokRepository.UpdateStok(model.UrunId, model.DepoId, fark,
              "Sayım Düzeltmesi", fark == 0 ? "" : duzeltmeNedeni.Trim(), transaction);
            if (!stokResult.Success) { transaction.Rollback(); return stokResult; }
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
          return new OperationResult { Success = false, Message = UiText.Get("InvalidProductId") };
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
      private bool ValidateUrunViewModel(UrunViewModel model, out string errorMessage)
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
        if (malzeme.BirimId != model.BirimId ||
            !string.Equals(malzeme.Aciklama, model.UrunAdi.Trim(), StringComparison.Ordinal))
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

    /// <summary>
    /// Stok Business Logic Service - Stok hareketlerinin iş kuralları
    /// </summary>
    public class StokService
    {
      private readonly StokRepository _stokRepository;
      private readonly UrunRepository _urunRepository;
      private readonly DepoRepository _depoRepository;

      public StokService()
      {
        _stokRepository = new StokRepository();
        _urunRepository = new UrunRepository();
        _depoRepository = new DepoRepository();
      }

      /// <summary>
      /// Bir ürüne ait spesifik stok kaydını getirir (Performans iyileştirmesi için eklendi)
      /// </summary>
      public Stok GetStok(int urunId, int depoId)
      {
        return _stokRepository.GetByUrunAndDepo(urunId, depoId);
      }

      /// <summary>
      /// Seçilen depodaki tüm stokları getirir
      /// </summary>
      public List<Stok> GetStokByDepo(int depoId)
      {
        if (depoId <= 0)
        {
          throw new ArgumentException(UiText.Get("InvalidWarehouseId"));
        }

        return _stokRepository.GetByDepo(depoId) ?? new List<Stok>();
      }

      /// <summary>
      /// Stok güncelle (ekleme/çıkarma işlemleri için güvenli metot)
      /// </summary>
      public OperationResult UpdateStok(int urunId, int depoId, int degisim, string hareketTipi, string aciklama)
      {
        return _stokRepository.UpdateStok(urunId, depoId, degisim, hareketTipi, aciklama);
      }
      /// <summary>
      /// Stok miktarını direkt olarak belirli bir sayıya set (eşitleme) eder
      /// </summary>
      public OperationResult SetStock(int urunId, int depoId, int miktar, string neden)
      {
        if (urunId <= 0 || depoId <= 0 || miktar < 0)
        {
          return OperationResult.Fail(UiText.Get("InvalidParameters"));
        }

        return _stokRepository.UpsertStok(urunId, depoId, miktar, neden);
      }

      public OperationResult TransferStock(int urunId, int kaynakDepoId, int hedefDepoId, int miktar, string aciklama)
      {
        return _stokRepository.TransferStock(urunId, kaynakDepoId, hedefDepoId, miktar, aciklama);
      }

      /// <summary>
      /// İş kurallarına göre Stok Durumunu (Kritik, Düşük, Normal) hesaplar
      /// </summary>
      public StokDurumuTipi GetStockStatus(int miktarUrunId, int depoId)
      {
        // Ürünün mevcut stoğunu bul
        var stok = _stokRepository.GetByUrunAndDepo(miktarUrunId, depoId);
        if (stok == null)
        {
          return StokDurumuTipi.Kritik; // Kayıt yoksa stok kritiktir (0 kabul edilir)
        }

        // Ürünün limit bilgilerini bul
        var urun = _urunRepository.GetById(miktarUrunId);
        if (urun == null)
        {
          return StokDurumuTipi.Normal;
        }

        // 1. İş Kuralı: Mevcut miktar, belirlenen minimum stok seviyesinin altındaysa "Kritik"
        if (stok.Miktar <= urun.MinimumStok)
        {
          return StokDurumuTipi.Kritik;
        }

        // 2. İş Kuralı: Mevcut miktar, maksimum stok kapasitesinin %25'inin altındaysa "Düşük"
        if (stok.Miktar <= urun.MaksimumStok * 0.25)
        {
          return StokDurumuTipi.Düşük;
        }


        // Şartlara takılmadıysa stok seviyesi "Normal"dir.
        return StokDurumuTipi.Normal;
      }

      /// <summary>
      /// Depodaki durumu sadece "Kritik" olan ürünleri filtreleyip getirir
      /// </summary>
      public List<Stok> GetCriticalStocks(int depoId)
      {
        var stoklar = GetStokByDepo(depoId);
        // LINQ kullanarak bellek üzerinde (RAM) verileri filtreler
        return stoklar.Where(s => s.StokDurumu == "Kritik").ToList();
      }



    }

    /// <summary>
    /// Malzeme kataloğu işlemleri
    /// </summary>
    public class MalzemeService
    {
      private readonly MalzemeKatalogRepository _repository = new MalzemeKatalogRepository();

      public MalzemeKatalog GetByBarkod(string barkod)
      {
        if (string.IsNullOrWhiteSpace(barkod)) return null;
        return _repository.GetByBarkod(barkod.Trim());
      }

      public OperationResult Add(MalzemeKatalog malzeme)
      {
        return _repository.Add(malzeme);
      }

      public OperationResult Update(MalzemeKatalog malzeme)
      {
        return _repository.Update(malzeme);
      }
    }

    /// <summary>
    /// Kategori Business Logic Service
    /// </summary>
    public class KategoriService
    {
      private readonly KategoriRepository _kategoriRepository;

      public KategoriService()
      {
        _kategoriRepository = new KategoriRepository();
      }

      public List<Kategori> GetAllKategoriler()
      {
        return _kategoriRepository.GetAll();
      }

      public Kategori GetKategoriById(int kategoriId)
      {
        if (kategoriId <= 0)
        {
          throw new ArgumentException(UiText.Get("InvalidCategoryId"));
        }
        return _kategoriRepository.GetById(kategoriId);
      }

      public OperationResult AddKategori(string kategoriAdi, string aciklama = "")
      {
        if (string.IsNullOrWhiteSpace(kategoriAdi))
        {
          return OperationResult.Fail(UiText.Get("CategoryNameRequired"));
        }

        var kategori = new Kategori
        {
          KategoriAdi = kategoriAdi.Trim(),
          Aciklama = aciklama.Trim()
        };

        return _kategoriRepository.Add(kategori);
      }

      public OperationResult UpdateKategori(int kategoriId, string kategoriAdi, string aciklama = "")
      {
        if (kategoriId <= 0) return OperationResult.Fail(UiText.Get("InvalidCategoryId"));
        if (string.IsNullOrWhiteSpace(kategoriAdi)) return OperationResult.Fail(UiText.Get("CategoryNameRequired"));

        var kategori = new Kategori
        {
          KategoriId = kategoriId,
          KategoriAdi = kategoriAdi.Trim(),
          Aciklama = aciklama.Trim()
        };

        return _kategoriRepository.Update(kategori);
      }

      public OperationResult DeleteKategori(int kategoriId)
      {
        return kategoriId > 0 ? _kategoriRepository.Delete(kategoriId)
          : OperationResult.Fail(UiText.Get("InvalidCategory"));
      }
    }

    /// <summary>
    /// Birim Business Logic Service
    /// </summary>
    public class BirimService
    {
      private readonly BirimRepository _birimRepository;

      public BirimService()
      {
        _birimRepository = new BirimRepository();
      }

      public List<Birim> GetAllBirimler()
      {
        return _birimRepository.GetAll();
      }

      public Birim GetBirimById(int birimId)
      {
        if (birimId <= 0) throw new ArgumentException(UiText.Get("InvalidUnitId"));
        return _birimRepository.GetById(birimId);
      }
    }

    /// <summary>
    /// Depo Business Logic Service
    /// </summary>
    public class DepoService
    {
      private readonly DepoRepository _depoRepository;

      public DepoService()
      {
        _depoRepository = new DepoRepository();
      }

      public List<Depo> GetAllActiveDepolar()
      {
        return _depoRepository.GetAllActive();
      }

      public Depo GetDepoById(int depoId)
      {
        if (depoId <= 0) throw new ArgumentException(UiText.Get("InvalidWarehouseId"));
        return _depoRepository.GetById(depoId);
      }

      public List<Depo> GetAllDepolar()
      {
        return _depoRepository.GetAll();
      }

      public OperationResult AddDepo(string depoAdi, string lokasyon, string aciklama)
      {
        if (string.IsNullOrWhiteSpace(depoAdi)) return OperationResult.Fail(UiText.Get("WarehouseNameRequired"));
        return _depoRepository.Add(new Depo
        {
          DepoAdi = depoAdi.Trim(), Lokasyon = lokasyon?.Trim(), Aciklama = aciklama?.Trim()
        });
      }

      public OperationResult UpdateDepo(int depoId, string depoAdi, string lokasyon, string aciklama)
      {
        if (depoId <= 0 || string.IsNullOrWhiteSpace(depoAdi))
          return OperationResult.Fail(UiText.Get("SelectWarehouseName"));
        return _depoRepository.Update(new Depo
        {
          DepoId = depoId, DepoAdi = depoAdi.Trim(), Lokasyon = lokasyon?.Trim(), Aciklama = aciklama?.Trim()
        });
      }

      public OperationResult SetDepoActive(int depoId, bool aktifMi)
      {
        return depoId > 0 ? _depoRepository.SetActive(depoId, aktifMi)
          : OperationResult.Fail(UiText.Get("InvalidWarehouse"));
      }
    }

    public class ZReportService
    {
      private const string RaporFiltreSql = @"
            FROM StokHareketi h
            INNER JOIN Urun u ON u.UrunId = h.UrunId
            INNER JOIN Depo d ON d.DepoId = h.DepoId
            LEFT JOIN StokTransferi t ON t.TransferId = h.TransferId
            WHERE h.HareketTarihi >= @Baslangic AND h.HareketTarihi < @Bitis
              AND (@DepoId IS NULL OR h.DepoId = @DepoId)
              AND (@UrunArama = N'' OR CHARINDEX(@UrunArama, u.UrunAdi) > 0
                   OR CHARINDEX(@UrunArama, u.Barkod) > 0)
              AND (@CikisMi IS NULL OR h.CikisMi = @CikisMi)
              AND (@HareketGrubu = 0
                   OR (@HareketGrubu = 1 AND h.TransferId IS NULL AND h.HareketTipi <> N'Sayım Düzeltmesi')
                   OR (@HareketGrubu = 2 AND h.TransferId IS NOT NULL)
                   OR (@HareketGrubu = 3 AND h.TransferId IS NULL AND h.HareketTipi = N'Sayım Düzeltmesi'))";

      private static void AddRaporParameters(SqlCommand command, HareketRaporFiltresi filtre)
      {
        if (filtre == null || filtre.Baslangic.Date > filtre.Bitis.Date ||
            filtre.Bitis.Date == DateTime.MaxValue.Date || filtre.HareketGrubu < 0 || filtre.HareketGrubu > 3)
          throw new ArgumentException(UiText.Get("InvalidReportFilter"));

        // Bitiş gününün tamamını kapsamak için ertesi günün başlangıcını hariç tutuyoruz.
        command.Parameters.AddWithValue("@Baslangic", filtre.Baslangic.Date);
        command.Parameters.AddWithValue("@Bitis", filtre.Bitis.Date.AddDays(1));
        command.Parameters.Add("@DepoId", System.Data.SqlDbType.Int).Value =
          (object)filtre.DepoId ?? DBNull.Value;
        command.Parameters.Add("@UrunArama", System.Data.SqlDbType.NVarChar, 200).Value =
          (filtre.UrunArama ?? "").Trim();
        command.Parameters.Add("@CikisMi", System.Data.SqlDbType.Bit).Value =
          (object)filtre.CikisMi ?? DBNull.Value;
        command.Parameters.AddWithValue("@HareketGrubu", filtre.HareketGrubu);
      }

      public List<HareketRaporOzeti> GetHareketOzeti(HareketRaporFiltresi filtre, string periyot)
      {
        string donemSorgusu;
        switch (periyot)
        {
          case "Günlük": donemSorgusu = "CONVERT(varchar(10), h.HareketTarihi, 104)"; break;
          case "Aylık": donemSorgusu = "RIGHT(CONVERT(varchar(10), h.HareketTarihi, 104), 7)"; break;
          case "Yıllık": donemSorgusu = "CAST(YEAR(h.HareketTarihi) AS varchar(4))"; break;
          default: throw new ArgumentException(UiText.Get("InvalidPeriod"));
        }

        // İç transferler iki hareket üretir; normal giriş/çıkış toplamına katılmaz.
        string query = $@"
            SELECT {donemSorgusu} AS Donem,
              SUM(CASE WHEN h.TransferId IS NULL AND h.HareketTipi <> N'Sayım Düzeltmesi' AND h.CikisMi=0 THEN CAST(h.DegisenMiktar AS BIGINT) ELSE 0 END) AS NormalGiris,
              SUM(CASE WHEN h.TransferId IS NULL AND h.HareketTipi <> N'Sayım Düzeltmesi' AND h.CikisMi=1 THEN -CAST(h.DegisenMiktar AS BIGINT) ELSE 0 END) AS NormalCikis,
              SUM(CASE WHEN h.TransferId IS NOT NULL AND h.CikisMi=0 THEN CAST(h.DegisenMiktar AS BIGINT) ELSE 0 END) AS TransferGiris,
              SUM(CASE WHEN h.TransferId IS NOT NULL AND h.CikisMi=1 THEN -CAST(h.DegisenMiktar AS BIGINT) ELSE 0 END) AS TransferCikis,
              SUM(CASE WHEN h.TransferId IS NULL AND h.HareketTipi = N'Sayım Düzeltmesi' AND h.CikisMi=0 THEN CAST(h.DegisenMiktar AS BIGINT) ELSE 0 END) AS DuzeltmeGiris,
              SUM(CASE WHEN h.TransferId IS NULL AND h.HareketTipi = N'Sayım Düzeltmesi' AND h.CikisMi=1 THEN -CAST(h.DegisenMiktar AS BIGINT) ELSE 0 END) AS DuzeltmeCikis,
              SUM(CAST(h.DegisenMiktar AS BIGINT)) AS NetDegisim,
              COUNT_BIG(*) AS HareketSayisi
            {RaporFiltreSql}
            GROUP BY {donemSorgusu}
            ORDER BY MIN(h.HareketTarihi) DESC";

        var sonuc = new List<HareketRaporOzeti>();
        using (var connection = SqlDBConnection.GetConnection())
        using (var command = new SqlCommand(query, connection))
        {
          AddRaporParameters(command, filtre);
          using (var reader = command.ExecuteReader())
          {
            while (reader.Read())
            {
              sonuc.Add(new HareketRaporOzeti
              {
                Donem = reader.GetString(0),
                NormalGiris = reader.GetInt64(1), NormalCikis = reader.GetInt64(2),
                TransferGiris = reader.GetInt64(3), TransferCikis = reader.GetInt64(4),
                DuzeltmeGiris = reader.GetInt64(5), DuzeltmeCikis = reader.GetInt64(6),
                NetDegisim = reader.GetInt64(7), HareketSayisi = reader.GetInt64(8)
              });
            }
          }
        }
        return sonuc;
      }

      public List<HareketRaporDetayi> GetHareketDetayi(HareketRaporFiltresi filtre)
      {
        string query = @"
            SELECT h.HareketId, h.HareketTarihi, u.UrunAdi, u.Barkod, d.DepoAdi,
                   h.HareketTipi, h.CikisMi, h.DegisenMiktar, h.OncekiMiktar,
                   h.YeniMiktar, COALESCE(h.KullaniciAdi, N''),
                   COALESCE(t.Aciklama, h.Aciklama, N''), h.TransferId
            " + RaporFiltreSql + @"
            ORDER BY h.HareketTarihi DESC, h.HareketId DESC";

        var sonuc = new List<HareketRaporDetayi>();
        using (var connection = SqlDBConnection.GetConnection())
        using (var command = new SqlCommand(query, connection))
        {
          AddRaporParameters(command, filtre);
          using (var reader = command.ExecuteReader())
          {
            while (reader.Read())
            {
              sonuc.Add(new HareketRaporDetayi
              {
                HareketId = reader.GetInt32(0), HareketTarihi = reader.GetDateTime(1),
                UrunAdi = reader.GetString(2), Barkod = reader.GetString(3),
                DepoAdi = reader.GetString(4), HareketTipi = reader.GetString(5),
                Yon = reader.GetBoolean(6) ? "Çıkış" : "Giriş",
                DegisenMiktar = reader.GetInt32(7), OncekiMiktar = reader.GetInt32(8),
                YeniMiktar = reader.GetInt32(9), KullaniciAdi = reader.GetString(10),
                Aciklama = reader.GetString(11),
                TransferId = reader.IsDBNull(12) ? (int?)null : reader.GetInt32(12)
              });
            }
          }
        }
        return sonuc;
      }

    }
  }
}
