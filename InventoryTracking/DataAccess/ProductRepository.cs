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
  /// Ürün Repository - Ürün işlemleri
  /// </summary>
  public class ProductRepository
  {
    // 02 - Bağlantı adresini merkezi sınıftan güvenli havuz mantığıyla alır.
    private SqlConnection GetConnection() => SqlDatabaseConnection.GetConnection();
    public bool IsNameTaken(string urunAdi, int excludedUrunId = 0)
    {
      // Eklemede 0, düzenlemede mevcut ürünün ID'si gönderilir. SQL Server'ın
      // Türkçe ve büyük/küçük harfe duyarsız karşılaştırması aynı adı bulur.
      using (SqlConnection conn = GetConnection())
      using (SqlCommand cmd = new SqlCommand("SELECT CASE WHEN EXISTS (SELECT 1 FROM Urun WHERE UrunAdi = @UrunAdi AND UrunId <> @ExcludedUrunId) THEN 1 ELSE 0 END", conn))
      {
        cmd.Parameters.AddWithValue("@UrunAdi", urunAdi.Trim());
        cmd.Parameters.AddWithValue("@ExcludedUrunId", excludedUrunId);
        return (int)cmd.ExecuteScalar() == 1;
      }
    }

    /// <summary>
    /// 03 - Barkoda göre ürün arama işlemi.
    /// </summary>
    public Product GetByBarkod(string barkod)
    {
      // 04 - 'using' bloğu sayesinde sorgu bitince bağlantı otomatik olarak kapatılır (Dispose).
      using (SqlConnection conn = GetConnection())
      {
        using (SqlCommand cmd = new SqlCommand("sp_GetUrunByBarkod", conn))
        {
          cmd.CommandType = CommandType.StoredProcedure;
          cmd.Parameters.AddWithValue("@Barkod", barkod);
          using (SqlDataReader reader = cmd.ExecuteReader())
          {
            if (reader.Read())
            {
              return new Product
              {
                UrunId = SafeGetInt(reader, "UrunId"),
                Barkod = SafeGetString(reader, "Barkod"),
                UrunAdi = SafeGetString(reader, "UrunAdi"),
                KategoriId = SafeGetInt(reader, "KategoriId"),
                KategoriAdi = SafeGetString(reader, "KategoriAdi"),
                BirimId = SafeGetInt(reader, "BirimId"),
                BirimAdi = SafeGetString(reader, "BirimAdi"),
                Fiyat = SafeGetDecimal(reader, "Fiyat"),
                MinimumStok = SafeGetInt(reader, "MinimumStok"),
                MaksimumStok = SafeGetInt(reader, "MaksimumStok"),
                AktifMi = SafeGetBoolean(reader, "AktifMi")
              };
            }
          }
        }
      }

      return null;
    }

    public Product GetInactiveByBarkod(string barkod)
    {
      int urunId;
      using (SqlConnection conn = GetConnection())
      using (SqlCommand cmd = new SqlCommand("SELECT UrunId FROM Urun WHERE Barkod = @Barkod AND AktifMi = 0", conn))
      {
        cmd.Parameters.AddWithValue("@Barkod", barkod.Trim());
        var id = cmd.ExecuteScalar();
        if (id == null)
          return null;
        urunId = (int)id;
      }

      return GetById(urunId);
    }

    public OperationResult Reactivate(int urunId)
    {
      try
      {
        using (SqlConnection conn = GetConnection())
        using (SqlCommand cmd = new SqlCommand("UPDATE Urun SET AktifMi = 1, GuncellemeTarihi = GETDATE() WHERE UrunId = @UrunId AND AktifMi = 0", conn))
        {
          cmd.Parameters.AddWithValue("@UrunId", urunId);
          return cmd.ExecuteNonQuery() == 1 ? OperationResult.Ok(UiText.Get("ProductActivated")) : OperationResult.Fail(UiText.Get("InactiveProductMissing"));
        }
      }
      catch (Exception ex)
      {
        return OperationResult.Fail(UiText.Get("ProductActivateError") + ex.Message);
      }
    }

    /// <summary>
    /// 05 - Aktif tüm ürünleri listeleme işlemi.
    /// </summary>
    public List<Product> GetAll()
    {
      var urunler = new List<Product>();
      try
      {
        using (SqlConnection conn = GetConnection())
        {
          using (SqlCommand cmd = new SqlCommand("sp_GetAllUrunler", conn))
          {
            cmd.CommandType = CommandType.StoredProcedure;
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
              while (reader.Read())
              {
                urunler.Add(new Product { UrunId = SafeGetInt(reader, "UrunId"), Barkod = SafeGetString(reader, "Barkod"), UrunAdi = SafeGetString(reader, "UrunAdi"), KategoriAdi = SafeGetString(reader, "KategoriAdi"), BirimAdi = SafeGetString(reader, "BirimAdi"), Fiyat = SafeGetDecimal(reader, "Fiyat"), MinimumStok = SafeGetInt(reader, "MinimumStok"), MaksimumStok = SafeGetInt(reader, "MaksimumStok"), // GÜNCELLENDİ: SafeGetBoolean burada çağrılıyor
                AktifMi = SafeGetBoolean(reader, "AktifMi"), OlusturulmaTarihi = SafeGetDateTime(reader, "OlusturulmaTarihi") });
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        throw new Exception(UiText.Format("ProductListException", ex.Message), ex);
      }

      return urunler;
    }

    /// <summary>
    /// 06 - ID değerine göre ürün getirme.
    /// </summary>
    public Product GetById(int urunId)
    {
      using (SqlConnection conn = GetConnection())
      {
        string query = "SELECT UrunId, Barkod, UrunAdi, KategoriId, BirimId, Fiyat, MinimumStok, MaksimumStok, AktifMi FROM Urun WHERE UrunId = @UrunId";
        using (SqlCommand cmd = new SqlCommand(query, conn))
        {
          cmd.Parameters.AddWithValue("@UrunId", urunId);
          using (SqlDataReader reader = cmd.ExecuteReader())
          {
            if (reader.Read())
            {
              return new Product
              {
                UrunId = SafeGetInt(reader, "UrunId"),
                Barkod = SafeGetString(reader, "Barkod"),
                UrunAdi = SafeGetString(reader, "UrunAdi"),
                KategoriId = SafeGetInt(reader, "KategoriId"),
                BirimId = SafeGetInt(reader, "BirimId"),
                Fiyat = SafeGetDecimal(reader, "Fiyat"),
                MinimumStok = SafeGetInt(reader, "MinimumStok"),
                MaksimumStok = SafeGetInt(reader, "MaksimumStok"),
                AktifMi = SafeGetBoolean(reader, "AktifMi")
              };
            }
          }
        }
      }

      return null;
    }

    /// <summary>
    /// 07 - Yeni ürün ekleme ve üretilen yeni ID'yi yakalama (Scope Identity).
    /// </summary>
    public OperationResult Add(Product urun, SqlTransaction transaction = null)
    {
      try
      {
        SqlConnection conn = transaction?.Connection ?? GetConnection();
        try
        {
          string query = @"INSERT INTO Urun (MalzemeId, Barkod, UrunAdi, KategoriId, BirimId, Fiyat, MinimumStok, MaksimumStok, AktifMi) 
                                     VALUES (@MalzemeId, @Barkod, @UrunAdi, @KategoriId, @BirimId, @Fiyat, @MinimumStok, @MaksimumStok, @AktifMi);
                                     SELECT CAST(SCOPE_IDENTITY() as int);";
          using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
          {
            cmd.Parameters.AddWithValue("@MalzemeId", urun.MalzemeId);
            cmd.Parameters.AddWithValue("@Barkod", urun.Barkod);
            cmd.Parameters.AddWithValue("@UrunAdi", urun.UrunAdi);
            cmd.Parameters.AddWithValue("@KategoriId", urun.KategoriId);
            cmd.Parameters.AddWithValue("@BirimId", urun.BirimId);
            cmd.Parameters.AddWithValue("@Fiyat", urun.Fiyat);
            cmd.Parameters.AddWithValue("@MinimumStok", urun.MinimumStok);
            cmd.Parameters.AddWithValue("@MaksimumStok", urun.MaksimumStok);
            cmd.Parameters.AddWithValue("@AktifMi", urun.AktifMi);
            // ExecuteScalar ile yeni oluşan auto-increment ID alınır.
            urun.UrunId = (int)cmd.ExecuteScalar();
          }
        }
        finally
        {
          if (transaction == null)
            conn.Dispose();
        }

        return OperationResult.Ok(UiText.Get("ProductAdded"));
      }
      catch (SqlException ex)when (ex.Number == 2601 || ex.Number == 2627)
      {
        // Ön kontrolü iki kullanıcı aynı anda geçebilir; benzersiz indeks son
        // güvenceyi sağlar ve burada kullanıcıya anlaşılır bir sonuç döneriz.
        return OperationResult.Fail(ex.Message.Contains("UX_Urun_UrunAdi") ? UiText.Get("ProductNameExists") : UiText.Get("BarcodeExists"));
      }
      catch (Exception ex)
      {
        return OperationResult.Fail(UiText.Format("ProductAddException", ex.Message));
      }
    }

    /// <summary>
    /// 08 - Ürün bilgilerini güncelleme.
    /// </summary>
    public OperationResult Update(Product urun, SqlTransaction transaction = null)
    {
      try
      {
        SqlConnection conn = transaction?.Connection ?? GetConnection();
        try
        {
          string query = "UPDATE Urun SET Barkod = @Barkod, UrunAdi = @UrunAdi, KategoriId = @KategoriId, BirimId = @BirimId, Fiyat = @Fiyat, MinimumStok = @MinimumStok, MaksimumStok = @MaksimumStok, AktifMi = @AktifMi, GuncellemeTarihi = GETDATE() WHERE UrunId = @UrunId";
          using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
          {
            cmd.Parameters.AddWithValue("@UrunId", urun.UrunId);
            cmd.Parameters.AddWithValue("@Barkod", urun.Barkod);
            cmd.Parameters.AddWithValue("@UrunAdi", urun.UrunAdi);
            cmd.Parameters.AddWithValue("@KategoriId", urun.KategoriId);
            cmd.Parameters.AddWithValue("@BirimId", urun.BirimId);
            cmd.Parameters.AddWithValue("@Fiyat", urun.Fiyat);
            cmd.Parameters.AddWithValue("@MinimumStok", urun.MinimumStok);
            cmd.Parameters.AddWithValue("@MaksimumStok", urun.MaksimumStok);
            cmd.Parameters.AddWithValue("@AktifMi", urun.AktifMi);
            if (cmd.ExecuteNonQuery() == 0)
              return OperationResult.Fail(UiText.Get("ProductMissingDot"));
          }
        }
        finally
        {
          if (transaction == null)
            conn.Dispose();
        }

        return OperationResult.Ok(UiText.Get("ProductUpdated"));
      }
      catch (SqlException ex)when (ex.Number == 2601 || ex.Number == 2627)
      {
        return OperationResult.Fail(ex.Message.Contains("UX_Urun_UrunAdi") ? UiText.Get("ProductNameExists") : UiText.Get("BarcodeExists"));
      }
      catch (Exception ex)
      {
        return OperationResult.Fail(UiText.Format("ProductUpdateException", ex.Message));
      }
    }

    /// <summary>
    /// 09 - Ürün silme (Soft Delete - AktifMi false yapılır).
    /// </summary>
    public OperationResult Delete(int urunId)
    {
      try
      {
        using (SqlConnection conn = GetConnection())
        {
          string query = "UPDATE Urun SET AktifMi = 0, GuncellemeTarihi = GETDATE() WHERE UrunId = @UrunId";
          using (SqlCommand cmd = new SqlCommand(query, conn))
          {
            cmd.Parameters.AddWithValue("@UrunId", urunId);
            int affectedRows = cmd.ExecuteNonQuery();
            return affectedRows > 0 ? OperationResult.Ok(UiText.Get("ProductDeleted")) : OperationResult.Fail(UiText.Get("ProductMissing"));
          }
        }
      }
      catch (Exception ex)
      {
        return OperationResult.Fail(UiText.Format("ProductDeleteException", ex.Message));
      }
    }

#region Güvenli Okuma Metotları (Null-Safe)
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

