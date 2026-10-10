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
  /// Birim Business Logic Service
  /// </summary>
  public class UnitService
  {
    private readonly UnitRepository _birimRepository;
    public UnitService()
    {
      _birimRepository = new UnitRepository();
    }

    public List<Unit> GetAllBirimler()
    {
      return _birimRepository.GetAll();
    }

    public Unit GetBirimById(int birimId)
    {
      if (birimId <= 0)
        throw new ArgumentException(UiText.Get("InvalidUnitId"));
      return _birimRepository.GetById(birimId);
    }
  }
}
