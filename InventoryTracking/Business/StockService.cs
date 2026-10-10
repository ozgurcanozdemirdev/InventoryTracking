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
  /// Stok Business Logic Service - Stok hareketlerinin iş kuralları
  /// </summary>
  public class StockService
  {
    private readonly StockRepository _stokRepository;
    private readonly ProductRepository _urunRepository;
    private readonly WarehouseRepository _depoRepository;
    public StockService()
    {
      _stokRepository = new StockRepository();
      _urunRepository = new ProductRepository();
      _depoRepository = new WarehouseRepository();
    }

    /// <summary>
    /// Bir ürüne ait spesifik stok kaydını getirir (Performans iyileştirmesi için eklendi)
    /// </summary>
    public Stock GetStok(int urunId, int depoId)
    {
      return _stokRepository.GetByUrunAndDepo(urunId, depoId);
    }

    /// <summary>
    /// Seçilen depodaki tüm stokları getirir
    /// </summary>
    public List<Stock> GetStokByDepo(int depoId)
    {
      if (depoId <= 0)
      {
        throw new ArgumentException(UiText.Get("InvalidWarehouseId"));
      }

      return _stokRepository.GetByDepo(depoId) ?? new List<Stock>();
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
    public StockStatusType GetStockStatus(int miktarUrunId, int depoId)
    {
      // Ürünün mevcut stoğunu bul
      var stok = _stokRepository.GetByUrunAndDepo(miktarUrunId, depoId);
      if (stok == null)
      {
        return StockStatusType.Critical; // Kayıt yoksa stok kritiktir (0 kabul edilir)
      }

      // Ürünün limit bilgilerini bul
      var urun = _urunRepository.GetById(miktarUrunId);
      if (urun == null)
      {
        return StockStatusType.Normal;
      }

      // 1. İş Kuralı: Mevcut miktar, belirlenen minimum stok seviyesinin altındaysa "Kritik"
      if (stok.Miktar <= urun.MinimumStok)
      {
        return StockStatusType.Critical;
      }

      // 2. İş Kuralı: Mevcut miktar, maksimum stok kapasitesinin %25'inin altındaysa "Düşük"
      if (stok.Miktar <= urun.MaksimumStok * 0.25)
      {
        return StockStatusType.Low;
      }

      // Şartlara takılmadıysa stok seviyesi "Normal"dir.
      return StockStatusType.Normal;
    }

    /// <summary>
    /// Depodaki durumu sadece "Kritik" olan ürünleri filtreleyip getirir
    /// </summary>
    public List<Stock> GetCriticalStocks(int depoId)
    {
      var stoklar = GetStokByDepo(depoId);
      // LINQ kullanarak bellek üzerinde (RAM) verileri filtreler
      return stoklar.Where(s => s.StokDurumu == "Kritik").ToList();
    }
  }
}
