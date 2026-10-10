using System;
using System.Collections.Generic;
using InventoryTracking.DataAccess;
using InventoryTracking.UI.Localization;
using InventoryTracking.Models;

namespace InventoryTracking.Business
{
  public class SalesReturnService
  {
    private readonly SalesReturnRepository _repository = new SalesReturnRepository();
    public OperationResult Kaydet(Guid islemNo, int urunId, int depoId, int miktar, bool iadeMi)
    {
      if (islemNo == Guid.Empty || urunId <= 0 || depoId <= 0 || miktar < 1 || miktar > 1000000)
        return OperationResult.Fail(UiText.Get("SaleQuantityRequired"));
      return _repository.Kaydet(islemNo, urunId, depoId, miktar, iadeMi);
    }
    public List<SalesReturnRecord> GetHistory(int depoId, int? urunId = null) => _repository.GetHistory(depoId, urunId);
  }
}
