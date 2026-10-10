using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using InventoryTracking.Connection;
using InventoryTracking.UI.Localization;
using InventoryTracking.Models;

namespace InventoryTracking.DataAccess
{
  public class SalesReturnRepository
  {
    public OperationResult Kaydet(Guid islemNo, int urunId, int depoId, int miktar, bool iadeMi)
    {
      try
      {
        // İşlem numarası ağ hatasından sonra tekrar denemede aynı tutulur.
        // Stok, hareket ve satış kaydı SQL'de tek transaction ile kaydedilir.
        using (var connection = SqlDatabaseConnection.OpenNewConnection())
        using (var command = new SqlCommand("dbo.sp_SatisIadeKaydet", connection))
        {
          command.CommandType = CommandType.StoredProcedure;
          command.Parameters.Add("@IslemNo", SqlDbType.UniqueIdentifier).Value = islemNo;
          command.Parameters.Add("@UrunId", SqlDbType.Int).Value = urunId;
          command.Parameters.Add("@DepoId", SqlDbType.Int).Value = depoId;
          command.Parameters.Add("@Miktar", SqlDbType.Int).Value = miktar;
          command.Parameters.Add("@IadeMi", SqlDbType.Bit).Value = iadeMi;
          using (var reader = command.ExecuteReader())
          {
            if (!reader.Read()) return OperationResult.Fail(UiText.Get("SaleSaveFailed"));
            return OperationResult.Ok(UiText.Format(iadeMi ? "ReturnCompleted" : "SaleCompleted", miktar), reader.GetInt64(0));
          }
        }
      }
      catch (SqlException ex)
      {
        switch (ex.Number)
        {
          case 50203: return OperationResult.Fail(UiText.Get("SaleInsufficientStock"));
          case 50202: return OperationResult.Fail(UiText.Get("StockActiveRequired"));
          case 50200: return OperationResult.Fail(UiText.Get("SaleQuantityRequired"));
          case 50201: return OperationResult.Fail(UiText.Get("SaleRequestConflict"));
          case 50204: return OperationResult.Fail(UiText.Get("SaleStockLimit"));
          case 2812: return OperationResult.Fail(UiText.Get("SaleSchemaMissing"));
          default: return OperationResult.Fail(UiText.Format("SaleDatabaseError", ex.Message));
        }
      }
    }

    public List<SalesReturnRecord> GetHistory(int depoId, int? urunId = null)
    {
      var rows = new List<SalesReturnRecord>();
      using (var connection = SqlDatabaseConnection.OpenNewConnection())
      using (var command = new SqlCommand(@"
        SELECT s.IslemId,s.IslemTarihi,CASE WHEN s.IadeMi=1 THEN N'İade' ELSE N'Satış' END,
          s.UrunAdi,s.Barkod,d.DepoAdi,s.Miktar,s.BirimFiyat,s.ToplamTutar,s.KullaniciAdi
        FROM dbo.SatisIade s JOIN dbo.Depo d ON d.DepoId=s.DepoId
        WHERE s.DepoId=@Depo AND (@Urun IS NULL OR s.UrunId=@Urun)
        ORDER BY s.IslemTarihi DESC,s.IslemId DESC", connection))
      {
        command.Parameters.Add("@Depo", SqlDbType.Int).Value = depoId;
        command.Parameters.Add("@Urun", SqlDbType.Int).Value = (object)urunId ?? DBNull.Value;
        using (var reader = command.ExecuteReader())
          while (reader.Read())
            rows.Add(new SalesReturnRecord { IslemId=reader.GetInt64(0),IslemTarihi=reader.GetDateTime(1),
              IslemTuru=reader.GetString(2),UrunAdi=reader.GetString(3),Barkod=reader.GetString(4),
              DepoAdi=reader.GetString(5),Miktar=reader.GetInt32(6),BirimFiyat=reader.GetDecimal(7),
              ToplamTutar=reader.GetDecimal(8),KullaniciAdi=reader.GetString(9) });
      }
      return rows;
    }

    public static void LogUiAction(string form, string action)
    {
      // Parola, bağlantı dizesi veya kullanıcı girdisi günlüğe aktarılmaz.
      // Bağlantı kurulmadan önceki ekranlar SQL günlüğünü kullanamaz.
      if (!SqlDatabaseConnection.AuditAvailable) return;
      try
      {
        using (var connection = SqlDatabaseConnection.OpenNewConnection())
        using (var command = new SqlCommand("dbo.sp_UygulamaOlayiKaydet", connection))
        {
          command.CommandType = CommandType.StoredProcedure;
          command.Parameters.Add("@Eylem", SqlDbType.NVarChar, 50).Value = "Düğme";
          command.Parameters.Add("@Aciklama", SqlDbType.NVarChar, 1000).Value = form + ": " + action;
          command.ExecuteNonQuery();
        }
      }
      catch (SqlException ex) { System.Diagnostics.Trace.TraceWarning("İşlem günlüğü: " + ex.Message); }
    }
  }
}
