using InventoryTracking.UI.Localization;
using InventoryTracking.Connection;
using InventoryTracking.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using InventoryTracking.Models;

namespace InventoryTracking.DataAccess
{
  /// <summary>
  /// Stok Repository - Stok işlemleri
  /// </summary>
  public class StockRepository
  {
    private SqlConnection GetConnection() => SqlDatabaseConnection.GetConnection();
    /// <summary>
    /// 12 - Belirli bir depodaki tüm stokları listeler.
    /// </summary>
    public List<Stock> GetByDepo(int depoId)
    {
      var stoklar = new List<Stock>();
      using (SqlConnection conn = GetConnection())
      {
        // Katalog numarası ve son hareket aynı sorguda alınır. Böylece satır seçerken
        // tekrar SQL sorgusu yapılmaz; son hareket yalnızca seçili depoya ait olur.
        const string query = @"
        SELECT s.StokId, s.UrunId, u.Barkod, m.MalzemeNo, u.UrunAdi,
               k.KategoriAdi, b.BirimAdi, u.Fiyat, s.Miktar,
               CASE WHEN s.Miktar <= u.MinimumStok THEN N'Kritik'
                    WHEN s.Miktar <= (u.MaksimumStok / 4) THEN N'Düşük'
                    ELSE N'Normal' END AS StokDurumu,
               s.OlusturulmaTarihi, s.GuncellemeTarihi, h.SonHareketTarihi
        FROM dbo.Stok s
        INNER JOIN dbo.Urun u ON s.UrunId = u.UrunId
        INNER JOIN dbo.MalzemeKatalog m ON u.MalzemeId = m.MalzemeId
        INNER JOIN dbo.Kategori k ON u.KategoriId = k.KategoriId
        INNER JOIN dbo.Birim b ON u.BirimId = b.BirimId
        OUTER APPLY (SELECT MAX(HareketTarihi) AS SonHareketTarihi
                     FROM dbo.StokHareketi
                     WHERE UrunId = s.UrunId AND DepoId = s.DepoId) h
        WHERE s.DepoId = @DepoId AND u.AktifMi = 1
        ORDER BY u.UrunAdi;";
        using (SqlCommand cmd = new SqlCommand(query, conn))
        {
          cmd.Parameters.Add("@DepoId", SqlDbType.Int).Value = depoId;
          using (SqlDataReader r = cmd.ExecuteReader())
          {
            while (r.Read())
            {
              stoklar.Add(new Stock { StokId = SafeGetInt(r, "StokId"), UrunId = SafeGetInt(r, "UrunId"), DepoId = depoId, Barkod = SafeGetString(r, "Barkod"), MalzemeNo = SafeGetString(r, "MalzemeNo"), UrunAdi = SafeGetString(r, "UrunAdi"), KategoriAdi = SafeGetString(r, "KategoriAdi"), BirimAdi = SafeGetString(r, "BirimAdi"), Fiyat = SafeGetDecimal(r, "Fiyat"), Miktar = SafeGetInt(r, "Miktar"), StokDurumu = SafeGetString(r, "StokDurumu"), SonHareketTarihi = r.IsDBNull(r.GetOrdinal("SonHareketTarihi")) ? (DateTime? )null : r.GetDateTime(r.GetOrdinal("SonHareketTarihi")), OlusturulmaTarihi = SafeGetDateTime(r, "OlusturulmaTarihi"), GuncellemeTarihi = SafeGetDateTime(r, "GuncellemeTarihi") });
            }
          }
        }
      }

      return stoklar;
    }

    // ── Yardımcı NULL-safe okuma metodları ──────────────────────────────
#region 16 - Güvenli Okuma Metotları (Null-Safe)
    private static int SafeGetInt(SqlDataReader reader, string column)
    {
      int ordinal = reader.GetOrdinal(column);
      return reader.IsDBNull(ordinal) ? 0 : reader.GetInt32(ordinal);
    }

    private static string SafeGetString(SqlDataReader reader, string column)
    {
      int ordinal = reader.GetOrdinal(column);
      return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
    }

    private static decimal SafeGetDecimal(SqlDataReader reader, string column)
    {
      int ordinal = reader.GetOrdinal(column);
      return reader.IsDBNull(ordinal) ? 0m : reader.GetDecimal(ordinal);
    }

    private static bool SafeGetBoolean(SqlDataReader reader, string column)
    {
      int ordinal = reader.GetOrdinal(column);
      return !reader.IsDBNull(ordinal) && reader.GetBoolean(ordinal);
    }

    private static DateTime SafeGetDateTime(SqlDataReader reader, string column)
    {
      int ordinal = reader.GetOrdinal(column);
      return reader.IsDBNull(ordinal) ? DateTime.MinValue : reader.GetDateTime(ordinal);
    }

#endregion
    /// <summary>
    /// 13 - Ürün ve Depo bazlı tekil stok sorgusu (Transaction destekli).
    /// </summary>
    public Stock GetByUrunAndDepo(int urunId, int depoId, SqlTransaction transaction = null)
    {
      // Eğer aktif bir Transaction varsa aynı bağlantı kullanılır, yoksa yeni bağlantı açılır.
      SqlConnection conn = transaction?.Connection ?? GetConnection();
      try
      {
        using (SqlCommand cmd = new SqlCommand("SELECT StokId, Miktar FROM Stok" + (transaction == null ? "" : " WITH (UPDLOCK, HOLDLOCK)") + " WHERE UrunId = @UrunId AND DepoId = @DepoId", conn))
        {
          if (transaction != null)
            cmd.Transaction = transaction;
          cmd.Parameters.AddWithValue("@UrunId", urunId);
          cmd.Parameters.AddWithValue("@DepoId", depoId);
          using (SqlDataReader r = cmd.ExecuteReader())
          {
            if (r.Read())
            {
              return new Stock
              {
                StokId = SafeGetInt(r, "StokId"),
                UrunId = urunId,
                DepoId = depoId,
                Miktar = SafeGetInt(r, "Miktar")
              };
            }
          }
        }

        return null;
      }
      finally
      {
        // Yalnızca bu metodun açtığı bağlantıyı kapatıyoruz. Dışarıdan gelen
        // transaction bağlantısının ömrünü çağıran yönetir.
        if (transaction == null)
          conn.Dispose();
      }
    }

    /// <summary>
    /// 14 - Stok artırma/azaltma işlemi ve Stok Hareketi (Log) kaydı (Transaction Güvencesiyle).
    /// </summary>
    public OperationResult UpdateStok(int urunId, int depoId, int degisim, string tip, string aciklama, SqlTransaction transaction = null, int? transferId = null)
    {
      if (urunId <= 0 || depoId <= 0 || string.IsNullOrWhiteSpace(tip))
        return OperationResult.Fail(UiText.Get("InvalidStockOperation"));
      SqlConnection conn = transaction?.Connection ?? GetConnection();
      SqlTransaction trans = transaction ?? conn.BeginTransaction(IsolationLevel.Serializable);
      try
      {
        // Pasif ürün veya depoya yeni hareket yazılmasını işlem boyunca engelle.
        using (SqlCommand check = new SqlCommand(@"
            SELECT CASE WHEN EXISTS (SELECT 1 FROM Urun WITH (UPDLOCK, HOLDLOCK)
                                     WHERE UrunId=@UrunId AND AktifMi=1)
                AND EXISTS (SELECT 1 FROM Depo WITH (UPDLOCK, HOLDLOCK)
                            WHERE DepoId=@DepoId AND AktifMi=1)
              THEN 1 ELSE 0 END", conn, trans))
        {
          check.Parameters.AddWithValue("@UrunId", urunId);
          check.Parameters.AddWithValue("@DepoId", depoId);
          if ((int)check.ExecuteScalar() != 1)
            return OperationResult.Fail(UiText.Get("StockActiveRequired"));
        }

        // Mevcut miktar kontrolü
        var mevcut = GetByUrunAndDepo(urunId, depoId, trans);
        int eskiMiktar = mevcut?.Miktar ?? 0;
        int yeniMiktar = checked(eskiMiktar + degisim);
        if (yeniMiktar < 0)
        {
          return OperationResult.Fail(UiText.Get("InsufficientStock"));
        }

        // Stok miktarını güncelle
        using (SqlCommand cmd = new SqlCommand(mevcut == null ? "INSERT INTO Stok (UrunId, DepoId, Miktar) VALUES (@UrunId, @DepoId, @Miktar)" : "UPDATE Stok SET Miktar = @Miktar, GuncellemeTarihi = GETDATE() WHERE UrunId = @UrunId AND DepoId = @DepoId", conn, trans))
        {
          cmd.Parameters.AddWithValue("@UrunId", urunId);
          cmd.Parameters.AddWithValue("@DepoId", depoId);
          cmd.Parameters.AddWithValue("@Miktar", yeniMiktar);
          cmd.ExecuteNonQuery();
        }

        // Sıfır miktarlı ilk kaydı görünür tut; miktar değişmediyse hareket yazma.
        if (degisim != 0)
        {
          // Stok hareket logunu (Audit) kaydet
          string logQuery = @"INSERT INTO StokHareketi (UrunId, DepoId, OncekiMiktar, YeniMiktar, DegisenMiktar, CikisMi, HareketTipi, Aciklama, KullaniciAdi, TransferId)
                                        VALUES (@U, @D, @O, @Y, @Deg, @CikisMi, @T, @A, @K, @TransferId)";
          using (SqlCommand cmd = new SqlCommand(logQuery, conn, trans))
          {
            cmd.Parameters.AddWithValue("@U", urunId);
            cmd.Parameters.AddWithValue("@D", depoId);
            cmd.Parameters.AddWithValue("@O", eskiMiktar);
            cmd.Parameters.AddWithValue("@Y", yeniMiktar);
            cmd.Parameters.AddWithValue("@Deg", degisim);
            // SQL BIT: 0 giriş, 1 çıkış. Sayım düzeltmesinde de yön miktarın işaretinden gelir.
            cmd.Parameters.AddWithValue("@CikisMi", degisim < 0);
            cmd.Parameters.AddWithValue("@T", tip);
            cmd.Parameters.AddWithValue("@A", aciklama ?? "");
            cmd.Parameters.AddWithValue("@K", Environment.UserName);
            cmd.Parameters.AddWithValue("@TransferId", (object)transferId ?? DBNull.Value);
            cmd.ExecuteNonQuery();
          }
        }

        // Her iki işlem de başarılı olursa veritabanına kalıcı olarak işle
        if (transaction == null)
          trans.Commit();
        return OperationResult.Ok(UiText.Get("StockUpdated"));
      }
      catch (Exception ex)
      {
        if (transaction == null)
          trans.Rollback();
        return OperationResult.Fail(UiText.Format("StockUpdateException", ex.Message));
      }
      finally
      {
        if (transaction == null)
        {
          trans.Dispose();
          conn.Dispose();
        }
      }
    }

    public OperationResult TransferStock(int urunId, int kaynakDepoId, int hedefDepoId, int miktar, string aciklama)
    {
      if (urunId <= 0 || kaynakDepoId <= 0 || hedefDepoId <= 0 || kaynakDepoId == hedefDepoId || miktar <= 0 || string.IsNullOrWhiteSpace(aciklama) || aciklama.Trim().Length > 500)
        return OperationResult.Fail(UiText.Get("TransferValidation"));
      try
      {
        using (SqlConnection conn = GetConnection())
        using (SqlTransaction transaction = conn.BeginTransaction(IsolationLevel.Serializable))
        {
          using (SqlCommand check = new SqlCommand(@"
            SELECT CASE WHEN EXISTS (SELECT 1 FROM Urun WITH (UPDLOCK, HOLDLOCK)
                                     WHERE UrunId=@UrunId AND AktifMi=1)
                AND (SELECT COUNT(*) FROM Depo WITH (UPDLOCK, HOLDLOCK)
                     WHERE DepoId IN (@Kaynak, @Hedef) AND AktifMi=1)=2
              THEN 1 ELSE 0 END", conn, transaction))
          {
            check.Parameters.AddWithValue("@UrunId", urunId);
            check.Parameters.AddWithValue("@Kaynak", kaynakDepoId);
            check.Parameters.AddWithValue("@Hedef", hedefDepoId);
            if ((int)check.ExecuteScalar() != 1)
            {
              transaction.Rollback();
              return OperationResult.Fail(UiText.Get("TransferInactive"));
            }
          }

          int transferId;
          using (SqlCommand insert = new SqlCommand(@"
            INSERT INTO StokTransferi (UrunId, KaynakDepoId, HedefDepoId, Miktar, Aciklama, KullaniciAdi)
            VALUES (@UrunId, @Kaynak, @Hedef, @Miktar, @Aciklama, @Kullanici);
            SELECT CAST(SCOPE_IDENTITY() AS INT);", conn, transaction))
          {
            insert.Parameters.AddWithValue("@UrunId", urunId);
            insert.Parameters.AddWithValue("@Kaynak", kaynakDepoId);
            insert.Parameters.AddWithValue("@Hedef", hedefDepoId);
            insert.Parameters.AddWithValue("@Miktar", miktar);
            insert.Parameters.AddWithValue("@Aciklama", aciklama.Trim());
            insert.Parameters.AddWithValue("@Kullanici", Environment.UserName);
            transferId = (int)insert.ExecuteScalar();
          }

          string not = $"Transfer #{transferId}";
          var cikis = UpdateStok(urunId, kaynakDepoId, -miktar, UiText.Get("TransferOut"), not, transaction, transferId);
          if (!cikis.Success)
          {
            transaction.Rollback();
            return cikis;
          }

          var giris = UpdateStok(urunId, hedefDepoId, miktar, UiText.Get("TransferIn"), not, transaction, transferId);
          if (!giris.Success)
          {
            transaction.Rollback();
            return giris;
          }

          transaction.Commit();
          return OperationResult.Ok(UiText.Format("TransferCompleted", transferId), transferId);
        }
      }
      catch (Exception ex)
      {
        return OperationResult.Fail(UiText.Get("TransferError") + ex.Message);
      }
    }

    /// <summary>
    /// 15 - Stoğu doğrudan belirli bir değere set etme (Upsert).
    /// </summary>
    public OperationResult UpsertStok(int urunId, int depoId, int miktar, string neden)
    {
      if (urunId <= 0 || depoId <= 0 || miktar < 0 || string.IsNullOrWhiteSpace(neden) || neden.Trim().Length > 500)
        return OperationResult.Fail(UiText.Get("AdjustmentValidation"));
      try
      {
        using (SqlConnection conn = GetConnection())
        using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.Serializable))
        {
          int mevcut = GetByUrunAndDepo(urunId, depoId, trans)?.Miktar ?? 0;
          var result = UpdateStok(urunId, depoId, checked(miktar - mevcut), "Sayım Düzeltmesi", neden.Trim(), trans);
          if (!result.Success)
          {
            trans.Rollback();
            return result;
          }

          trans.Commit();
          return result;
        }
      }
      catch (Exception ex)
      {
        return OperationResult.Fail(UiText.Format("StockSetException", ex.Message));
      }
    }

    public void UrunleriListele(DataGridView dgvStoklar)
    {
      try
      {
        ProductRepository urunRepo = new ProductRepository();
        var urunListesi = urunRepo.GetAll();
        dgvStoklar.DataSource = null; // Önce temizlemek, eski kalıntıların veya hatalı bind işlemlerinin önüne geçer
        dgvStoklar.DataSource = urunListesi; // Veriyi atıyoruz
      }
      catch (Exception ex)
      {
        UiMessage.Show(ex.Message);
      }
    }
  }
}/*
 private void UrunleriListele()
    {
      try
      {
        UrunRepository urunRepo = new UrunRepository();
        var urunListesi = urunRepo.GetAll();

        // Önce temizlemek, eski kalıntıların veya hatalı bind işlemlerinin önüne geçer
        dgvStoklar.DataSource = null;

        // Veriyi atıyoruz
        dgvStoklar.DataSource = urunListesi;
      }
      catch (Exception ex)
      {
        UiMessage.Show("Veriler yüklenirken hata oluştu: " + ex.Message);
      }
    }
 */

