using InventoryTracking.UI.Localization;
using System;

namespace InventoryTracking.Models
{
  public class MovementReportFilter
  {
    public DateTime Baslangic { get; set; }
    public DateTime Bitis { get; set; }
    public int? DepoId { get; set; }
    public string UrunArama { get; set; }
    // 0: tümü, 1: normal giriş/çıkış, 2: transfer, 3: sayım düzeltmesi.
    public int HareketGrubu { get; set; }
    public bool? CikisMi { get; set; }
  }
}
