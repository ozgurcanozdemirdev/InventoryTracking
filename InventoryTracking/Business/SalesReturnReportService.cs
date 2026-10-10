using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using InventoryTracking.Connection;
using InventoryTracking.UI.Localization;
using InventoryTracking.Models;

namespace InventoryTracking.Business
{
  public class SalesReturnReportService
  {
    public List<SalesReturnReportSummary> GetOzet(MovementReportFilter filtre, string periyot)
    {
      if (filtre == null || filtre.Baslangic.Date > filtre.Bitis.Date ||
          filtre.Bitis.Date == DateTime.MaxValue.Date)
        throw new ArgumentException(UiText.Get("InvalidReportFilter"));

      // Dönem SQL'i yalnızca bu sabit seçeneklerden seçilir. Kullanıcının ürün
      // araması ve tarihleri SQL metnine eklenmez; ayrı parametrelerle gönderilir.
      string donem;
      switch (periyot)
      {
        case "Günlük": donem = "CONVERT(char(10), s.IslemTarihi, 104)"; break;
        case "Aylık": donem = "RIGHT(CONVERT(char(10), s.IslemTarihi, 104), 7)"; break;
        case "Yıllık": donem = "CONVERT(varchar(4), YEAR(s.IslemTarihi))"; break;
        default: throw new ArgumentException(UiText.Get("InvalidPeriod"));
      }

      // Yalnızca satış/iade kayıtları parasal toplamı etkiler. Transfer, ürün
      // girişi ve sayım düzeltmeleri burada gelir veya iade olarak sayılmaz.
      // Kaydın kendi fiyatı kullanılır; sonradan değişen ürün fiyatı geçmişi bozmaz.
      string sql = $@"
        SELECT {donem} AS Donem,
          SUM(CASE WHEN s.IadeMi=0 THEN CAST(s.Miktar AS BIGINT) ELSE 0 END),
          SUM(CASE WHEN s.IadeMi=1 THEN CAST(s.Miktar AS BIGINT) ELSE 0 END),
          SUM(CASE WHEN s.IadeMi=0 THEN s.ToplamTutar ELSE 0 END),
          SUM(CASE WHEN s.IadeMi=1 THEN s.ToplamTutar ELSE 0 END),
          COUNT_BIG(*)
        FROM dbo.SatisIade s
        WHERE s.IslemTarihi >= @Baslangic AND s.IslemTarihi < @Bitis
          AND (@DepoId IS NULL OR s.DepoId=@DepoId)
          AND (@UrunArama=N'' OR CHARINDEX(@UrunArama,s.UrunAdi)>0
               OR CHARINDEX(@UrunArama,s.Barkod)>0)
        GROUP BY {donem}
        ORDER BY MIN(s.IslemTarihi) DESC";

      var rows = new List<SalesReturnReportSummary>();
      using (var connection = SqlDatabaseConnection.OpenNewConnection())
      using (var command = new SqlCommand(sql, connection))
      {
        command.Parameters.Add("@Baslangic", SqlDbType.DateTime2).Value = filtre.Baslangic.Date;
        // Bitiş günündeki 23:59 kayıtları da dahil; ertesi gün rapora girmez.
        command.Parameters.Add("@Bitis", SqlDbType.DateTime2).Value = filtre.Bitis.Date.AddDays(1);
        command.Parameters.Add("@DepoId", SqlDbType.Int).Value = (object)filtre.DepoId ?? DBNull.Value;
        command.Parameters.Add("@UrunArama", SqlDbType.NVarChar, 200).Value = (filtre.UrunArama ?? "").Trim();
        using (var reader = command.ExecuteReader())
          while (reader.Read())
            rows.Add(new SalesReturnReportSummary
            {
              Donem = reader.GetString(0), SatisAdedi = reader.GetInt64(1),
              IadeAdedi = reader.GetInt64(2), SatisTutari = reader.GetDecimal(3),
              IadeTutari = reader.GetDecimal(4), IslemSayisi = reader.GetInt64(5)
            });
      }
      return rows;
    }

    public static DateTime SonBirYilBaslangici(DateTime bugun)
    {
      // Bugünün tamamını içeren bir takvim yılı: bitişin ertesi günü hariçtir.
      // AddYears artık yılları da kapsar; sabit 365 gün çıkarmıyoruz.
      return bugun.Date.AddDays(1).AddYears(-1);
    }
  }
}
