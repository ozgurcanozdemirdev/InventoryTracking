using InventoryTracking.UI.Localization;
using System;

namespace InventoryTracking.Models
{
  /// <summary>
  /// Birim entity modeli
  /// </summary>
  public class Unit
  {
    public int BirimId { get; set; }
    public string BirimAdi { get; set; }
    public string Kisaltma { get; set; }
    public DateTime OlusturulmaTarihi { get; set; }
  }
}
