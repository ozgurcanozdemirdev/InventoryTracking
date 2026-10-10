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
  /// Malzeme kataloğu işlemleri
  /// </summary>
  public class MaterialService
  {
    private readonly MaterialCatalogRepository _repository = new MaterialCatalogRepository();
    public MaterialCatalog GetByBarkod(string barkod)
    {
      if (string.IsNullOrWhiteSpace(barkod))
        return null;
      return _repository.GetByBarkod(barkod.Trim());
    }

    public OperationResult Add(MaterialCatalog malzeme)
    {
      return _repository.Add(malzeme);
    }

    public OperationResult Update(MaterialCatalog malzeme)
    {
      return _repository.Update(malzeme);
    }
  }
}
