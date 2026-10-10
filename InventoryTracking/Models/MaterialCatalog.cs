using InventoryTracking.UI.Localization;
using System;

namespace InventoryTracking.Models
{
  /// <summary>
  /// Barkod, açıklama ve birimin yetkili ana kaydı. MalzemeNo kullanıcı tarafından verilir.
  /// </summary>
  public class MaterialCatalog
  {
    public int MalzemeId { get; set; }
    public string MalzemeNo { get; set; }
    public string Barkod { get; set; }
    public string Aciklama { get; set; }
    public int BirimId { get; set; }
    public string BirimAdi { get; set; }
  }
}
