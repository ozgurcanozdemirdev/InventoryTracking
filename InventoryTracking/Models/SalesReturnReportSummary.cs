using System;

namespace InventoryTracking.Models
{
  public class SalesReturnReportSummary
  {
    public string Donem { get; set; }
    public long SatisAdedi { get; set; }
    public long IadeAdedi { get; set; }
    public long NetAdet => SatisAdedi - IadeAdedi;
    public decimal SatisTutari { get; set; }
    public decimal IadeTutari { get; set; }
    public decimal NetSatisTutari => SatisTutari - IadeTutari;
    public long IslemSayisi { get; set; }
  }
}
