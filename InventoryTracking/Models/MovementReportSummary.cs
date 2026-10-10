using InventoryTracking.UI.Localization;
using System;

namespace InventoryTracking.Models
{
  public class MovementReportSummary
  {
    public string Donem { get; set; }
    public long NormalGiris { get; set; }
    public long NormalCikis { get; set; }
    public long TransferGiris { get; set; }
    public long TransferCikis { get; set; }
    public long DuzeltmeGiris { get; set; }
    public long DuzeltmeCikis { get; set; }
    public long NetDegisim { get; set; }
    public long HareketSayisi { get; set; }
  }
}
