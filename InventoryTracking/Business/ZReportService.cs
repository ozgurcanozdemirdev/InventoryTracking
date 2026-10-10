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
  public class ZReportService
  {
    private const string RaporFiltreSql = @"
            FROM StokHareketi h
            INNER JOIN Urun u ON u.UrunId = h.UrunId
            INNER JOIN Depo d ON d.DepoId = h.DepoId
            LEFT JOIN StokTransferi t ON t.TransferId = h.TransferId
            WHERE h.HareketTarihi >= @Baslangic AND h.HareketTarihi < @Bitis
              AND (@DepoId IS NULL OR h.DepoId = @DepoId)
              AND (@UrunArama = N'' OR CHARINDEX(@UrunArama, u.UrunAdi) > 0
                   OR CHARINDEX(@UrunArama, u.Barkod) > 0)
              AND (@CikisMi IS NULL OR h.CikisMi = @CikisMi)
              AND (@HareketGrubu = 0
                   OR (@HareketGrubu = 1 AND h.TransferId IS NULL AND h.HareketTipi <> N'Sayım Düzeltmesi')
                   OR (@HareketGrubu = 2 AND h.TransferId IS NOT NULL)
                   OR (@HareketGrubu = 3 AND h.TransferId IS NULL AND h.HareketTipi = N'Sayım Düzeltmesi'))";
    private static void AddRaporParameters(SqlCommand command, MovementReportFilter filtre)
    {
      if (filtre == null || filtre.Baslangic.Date > filtre.Bitis.Date || filtre.Bitis.Date == DateTime.MaxValue.Date || filtre.HareketGrubu < 0 || filtre.HareketGrubu > 3)
        throw new ArgumentException(UiText.Get("InvalidReportFilter"));
      // Bitiş gününün tamamını kapsamak için ertesi günün başlangıcını hariç tutuyoruz.
      command.Parameters.AddWithValue("@Baslangic", filtre.Baslangic.Date);
      command.Parameters.AddWithValue("@Bitis", filtre.Bitis.Date.AddDays(1));
      command.Parameters.Add("@DepoId", System.Data.SqlDbType.Int).Value = (object)filtre.DepoId ?? DBNull.Value;
      command.Parameters.Add("@UrunArama", System.Data.SqlDbType.NVarChar, 200).Value = (filtre.UrunArama ?? "").Trim();
      command.Parameters.Add("@CikisMi", System.Data.SqlDbType.Bit).Value = (object)filtre.CikisMi ?? DBNull.Value;
      command.Parameters.AddWithValue("@HareketGrubu", filtre.HareketGrubu);
    }

    public List<MovementReportSummary> GetHareketOzeti(MovementReportFilter filtre, string periyot)
    {
      string donemSorgusu;
      switch (periyot)
      {
        case "Günlük":
          donemSorgusu = "CONVERT(varchar(10), h.HareketTarihi, 104)";
          break;
        case "Aylık":
          donemSorgusu = "RIGHT(CONVERT(varchar(10), h.HareketTarihi, 104), 7)";
          break;
        case "Yıllık":
          donemSorgusu = "CAST(YEAR(h.HareketTarihi) AS varchar(4))";
          break;
        default:
          throw new ArgumentException(UiText.Get("InvalidPeriod"));
      }

      // İç transferler iki hareket üretir; normal giriş/çıkış toplamına katılmaz.
      string query = $@"
            SELECT {donemSorgusu} AS Donem,
              SUM(CASE WHEN h.TransferId IS NULL AND h.HareketTipi <> N'Sayım Düzeltmesi' AND h.CikisMi=0 THEN CAST(h.DegisenMiktar AS BIGINT) ELSE 0 END) AS NormalGiris,
              SUM(CASE WHEN h.TransferId IS NULL AND h.HareketTipi <> N'Sayım Düzeltmesi' AND h.CikisMi=1 THEN -CAST(h.DegisenMiktar AS BIGINT) ELSE 0 END) AS NormalCikis,
              SUM(CASE WHEN h.TransferId IS NOT NULL AND h.CikisMi=0 THEN CAST(h.DegisenMiktar AS BIGINT) ELSE 0 END) AS TransferGiris,
              SUM(CASE WHEN h.TransferId IS NOT NULL AND h.CikisMi=1 THEN -CAST(h.DegisenMiktar AS BIGINT) ELSE 0 END) AS TransferCikis,
              SUM(CASE WHEN h.TransferId IS NULL AND h.HareketTipi = N'Sayım Düzeltmesi' AND h.CikisMi=0 THEN CAST(h.DegisenMiktar AS BIGINT) ELSE 0 END) AS DuzeltmeGiris,
              SUM(CASE WHEN h.TransferId IS NULL AND h.HareketTipi = N'Sayım Düzeltmesi' AND h.CikisMi=1 THEN -CAST(h.DegisenMiktar AS BIGINT) ELSE 0 END) AS DuzeltmeCikis,
              SUM(CAST(h.DegisenMiktar AS BIGINT)) AS NetDegisim,
              COUNT_BIG(*) AS HareketSayisi
            {RaporFiltreSql}
            GROUP BY {donemSorgusu}
            ORDER BY MIN(h.HareketTarihi) DESC";
      var sonuc = new List<MovementReportSummary>();
      using (var connection = SqlDatabaseConnection.GetConnection())
      using (var command = new SqlCommand(query, connection))
      {
        AddRaporParameters(command, filtre);
        using (var reader = command.ExecuteReader())
        {
          while (reader.Read())
          {
            sonuc.Add(new MovementReportSummary { Donem = reader.GetString(0), NormalGiris = reader.GetInt64(1), NormalCikis = reader.GetInt64(2), TransferGiris = reader.GetInt64(3), TransferCikis = reader.GetInt64(4), DuzeltmeGiris = reader.GetInt64(5), DuzeltmeCikis = reader.GetInt64(6), NetDegisim = reader.GetInt64(7), HareketSayisi = reader.GetInt64(8) });
          }
        }
      }

      return sonuc;
    }

    public List<MovementReportDetail> GetHareketDetayi(MovementReportFilter filtre)
    {
      string query = @"
            SELECT h.HareketId, h.HareketTarihi, u.UrunAdi, u.Barkod, d.DepoAdi,
                   h.HareketTipi, h.CikisMi, h.DegisenMiktar, h.OncekiMiktar,
                   h.YeniMiktar, COALESCE(h.KullaniciAdi, N''),
                   COALESCE(t.Aciklama, h.Aciklama, N''), h.TransferId
            " + RaporFiltreSql + @"
            ORDER BY h.HareketTarihi DESC, h.HareketId DESC";
      var sonuc = new List<MovementReportDetail>();
      using (var connection = SqlDatabaseConnection.GetConnection())
      using (var command = new SqlCommand(query, connection))
      {
        AddRaporParameters(command, filtre);
        using (var reader = command.ExecuteReader())
        {
          while (reader.Read())
          {
            sonuc.Add(new MovementReportDetail { HareketId = reader.GetInt32(0), HareketTarihi = reader.GetDateTime(1), UrunAdi = reader.GetString(2), Barkod = reader.GetString(3), DepoAdi = reader.GetString(4), HareketTipi = reader.GetString(5), Yon = reader.GetBoolean(6) ? "Çıkış" : "Giriş", DegisenMiktar = reader.GetInt32(7), OncekiMiktar = reader.GetInt32(8), YeniMiktar = reader.GetInt32(9), KullaniciAdi = reader.GetString(10), Aciklama = reader.GetString(11), TransferId = reader.IsDBNull(12) ? (int? )null : reader.GetInt32(12) });
          }
        }
      }

      return sonuc;
    }
  }
}
