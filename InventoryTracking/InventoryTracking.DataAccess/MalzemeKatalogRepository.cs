using InventoryTracking.Connection;
using System;
using System.Data.SqlClient;
using static InventoryTracking.Models.Entities;

namespace InventoryTracking.DataAccess
{
  public class MalzemeKatalogRepository
  {
    public MalzemeKatalog GetByBarkod(string barkod)
    {
      using (SqlConnection conn = SqlDBConnection.GetConnection())
      using (SqlCommand cmd = new SqlCommand(@"
          SELECT m.MalzemeId, m.MalzemeNo, m.Barkod, m.Aciklama, m.BirimId, b.BirimAdi
          FROM dbo.MalzemeKatalog AS m
          INNER JOIN dbo.Birim AS b ON b.BirimId = m.BirimId
          WHERE m.Barkod = @Barkod", conn))
      {
        cmd.Parameters.AddWithValue("@Barkod", barkod.Trim());
        using (SqlDataReader reader = cmd.ExecuteReader())
        {
          if (!reader.Read()) return null;
          return new MalzemeKatalog
          {
            MalzemeId = reader.GetInt32(0),
            MalzemeNo = reader.GetString(1),
            Barkod = reader.GetString(2),
            Aciklama = reader.GetString(3),
            BirimId = reader.GetInt32(4),
            BirimAdi = reader.GetString(5)
          };
        }
      }
    }

    public OperationResult Add(MalzemeKatalog malzeme)
    {
      if (malzeme == null || string.IsNullOrWhiteSpace(malzeme.MalzemeNo) ||
          string.IsNullOrWhiteSpace(malzeme.Barkod) || string.IsNullOrWhiteSpace(malzeme.Aciklama) ||
          malzeme.BirimId <= 0)
        return OperationResult.Fail("Malzeme numarası, barkod, açıklama ve birim zorunludur.");

      try
      {
        // Numara ve barkod ayrı UNIQUE kısıtlarıyla korunur; eşzamanlı kayıtta da tekrar oluşmaz.
        using (SqlConnection conn = SqlDBConnection.GetConnection())
        using (SqlCommand cmd = new SqlCommand(@"
            INSERT INTO dbo.MalzemeKatalog (MalzemeNo, Barkod, Aciklama, BirimId)
            VALUES (@MalzemeNo, @Barkod, @Aciklama, @BirimId);
            SELECT CAST(SCOPE_IDENTITY() AS INT);", conn))
        {
          cmd.Parameters.AddWithValue("@MalzemeNo", malzeme.MalzemeNo.Trim());
          cmd.Parameters.AddWithValue("@Barkod", malzeme.Barkod.Trim());
          cmd.Parameters.AddWithValue("@Aciklama", malzeme.Aciklama.Trim());
          cmd.Parameters.AddWithValue("@BirimId", malzeme.BirimId);
          malzeme.MalzemeId = (int)cmd.ExecuteScalar();
          return OperationResult.Ok("Malzeme kataloğa kaydedildi.", malzeme.MalzemeId);
        }
      }
      catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
      {
        return OperationResult.Fail("Bu malzeme numarası veya barkod zaten kayıtlıdır.");
      }
      catch (Exception ex)
      {
        return OperationResult.Fail("Malzeme kaydı yapılamadı: " + ex.Message);
      }
    }

    public OperationResult Update(MalzemeKatalog malzeme)
    {
      if (malzeme == null || malzeme.MalzemeId <= 0 ||
          string.IsNullOrWhiteSpace(malzeme.MalzemeNo) || string.IsNullOrWhiteSpace(malzeme.Aciklama) ||
          malzeme.BirimId <= 0)
        return OperationResult.Fail("Malzeme numarası, açıklama ve birim zorunludur.");

      try
      {
        using (SqlConnection conn = SqlDBConnection.GetConnection())
        using (SqlTransaction transaction = conn.BeginTransaction(System.Data.IsolationLevel.Serializable))
        {
          // Birim değişimi dış anahtarın ON UPDATE CASCADE kuralıyla ürüne yansır.
          // Açıklamayı da aynı transaction'da eşitliyoruz; hata olursa ikisi geri alınır.
          using (SqlCommand cmd = new SqlCommand(@"
              UPDATE MalzemeKatalog
              SET MalzemeNo=@MalzemeNo, Aciklama=@Aciklama, BirimId=@BirimId
              WHERE MalzemeId=@MalzemeId AND Barkod=@Barkod", conn, transaction))
          {
            cmd.Parameters.AddWithValue("@MalzemeId", malzeme.MalzemeId);
            cmd.Parameters.AddWithValue("@Barkod", malzeme.Barkod);
            cmd.Parameters.AddWithValue("@MalzemeNo", malzeme.MalzemeNo.Trim());
            cmd.Parameters.AddWithValue("@Aciklama", malzeme.Aciklama.Trim());
            cmd.Parameters.AddWithValue("@BirimId", malzeme.BirimId);
            if (cmd.ExecuteNonQuery() == 0)
              return OperationResult.Fail("Malzeme kaydı bulunamadı.");
          }
          using (SqlCommand cmd = new SqlCommand(@"
              UPDATE Urun SET UrunAdi=@Aciklama, GuncellemeTarihi=GETDATE()
              WHERE MalzemeId=@MalzemeId", conn, transaction))
          {
            cmd.Parameters.AddWithValue("@MalzemeId", malzeme.MalzemeId);
            cmd.Parameters.AddWithValue("@Aciklama", malzeme.Aciklama.Trim());
            cmd.ExecuteNonQuery();
          }
          transaction.Commit();
          return OperationResult.Ok("Malzeme ve bağlı ürün bilgileri güncellendi.");
        }
      }
      catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
      {
        return OperationResult.Fail("Bu malzeme numarası veya ürün açıklaması zaten kullanılıyor.");
      }
      catch (Exception ex)
      {
        return OperationResult.Fail("Malzeme güncellenemedi: " + ex.Message);
      }
    }
  }
}
