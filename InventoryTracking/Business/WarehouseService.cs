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
  /// Depo Business Logic Service
  /// </summary>
  public class WarehouseService
  {
    private readonly WarehouseRepository _depoRepository;
    public WarehouseService()
    {
      _depoRepository = new WarehouseRepository();
    }

    public List<Warehouse> GetAllActiveDepolar()
    {
      return _depoRepository.GetAllActive();
    }

    public Warehouse GetDepoById(int depoId)
    {
      if (depoId <= 0)
        throw new ArgumentException(UiText.Get("InvalidWarehouseId"));
      return _depoRepository.GetById(depoId);
    }

    public List<Warehouse> GetAllDepolar()
    {
      return _depoRepository.GetAll();
    }

    public OperationResult AddDepo(string depoAdi, string lokasyon, string aciklama)
    {
      if (string.IsNullOrWhiteSpace(depoAdi))
        return OperationResult.Fail(UiText.Get("WarehouseNameRequired"));
      return _depoRepository.Add(new Warehouse { DepoAdi = depoAdi.Trim(), Lokasyon = lokasyon?.Trim(), Aciklama = aciklama?.Trim() });
    }

    public OperationResult UpdateDepo(int depoId, string depoAdi, string lokasyon, string aciklama)
    {
      if (depoId <= 0 || string.IsNullOrWhiteSpace(depoAdi))
        return OperationResult.Fail(UiText.Get("SelectWarehouseName"));
      return _depoRepository.Update(new Warehouse { DepoId = depoId, DepoAdi = depoAdi.Trim(), Lokasyon = lokasyon?.Trim(), Aciklama = aciklama?.Trim() });
    }

    public OperationResult SetDepoActive(int depoId, bool aktifMi)
    {
      return depoId > 0 ? _depoRepository.SetActive(depoId, aktifMi) : OperationResult.Fail(UiText.Get("InvalidWarehouse"));
    }
  }
}
