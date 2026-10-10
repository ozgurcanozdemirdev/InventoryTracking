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
  public class CategoryRepository
  {
    private SqlConnection GetConnection() => SqlDatabaseConnection.GetConnection();
    public List<Category> GetAll()
    {
      var kategoriler = new List<Category>();
      using (SqlConnection conn = SqlDatabaseConnection.GetConnection())
      {
        string query = "SELECT KategoriId, KategoriAdi, Aciklama, OlusturulmaTarihi, GuncellemeTarihi FROM Kategori ORDER BY KategoriAdi";
        using (SqlCommand cmd = new SqlCommand(query, conn))
        using (SqlDataReader reader = cmd.ExecuteReader())
        {
          while (reader.Read())
          {
            kategoriler.Add(new Category { KategoriId = SafeGetInt(reader, "KategoriId"), KategoriAdi = SafeGetString(reader, "KategoriAdi"), Aciklama = SafeGetString(reader, "Aciklama"), OlusturulmaTarihi = SafeGetDateTime(reader, "OlusturulmaTarihi"), GuncellemeTarihi = SafeGetDateTime(reader, "GuncellemeTarihi") });
          }
        }
      }

      return kategoriler;
    }

    public Category GetById(int kategoriId)
    {
      using (SqlConnection conn = GetConnection())
      using (SqlCommand cmd = new SqlCommand("SELECT KategoriId, KategoriAdi, Aciklama, OlusturulmaTarihi, GuncellemeTarihi FROM Kategori WHERE KategoriId = @KategoriId", conn))
      {
        cmd.Parameters.AddWithValue("@KategoriId", kategoriId);
        using (SqlDataReader reader = cmd.ExecuteReader())
        {
          if (reader.Read())
          {
            return new Category
            {
              KategoriId = Convert.ToInt32(reader["KategoriId"]),
              KategoriAdi = reader["KategoriAdi"].ToString(),
              Aciklama = reader["Aciklama"] == DBNull.Value ? "" : reader["Aciklama"].ToString(),
              OlusturulmaTarihi = Convert.ToDateTime(reader["OlusturulmaTarihi"]),
              GuncellemeTarihi = Convert.ToDateTime(reader["GuncellemeTarihi"])
            };
          }
        }
      }

      return null;
    }

    public OperationResult Add(Category kategori)
    {
      try
      {
        using (SqlConnection conn = SqlDatabaseConnection.GetConnection())
        {
          string query = "INSERT INTO Kategori (KategoriAdi, Aciklama) VALUES (@KategoriAdi, @Aciklama)";
          using (SqlCommand cmd = new SqlCommand(query, conn))
          {
            cmd.Parameters.AddWithValue("@KategoriAdi", kategori.KategoriAdi);
            cmd.Parameters.AddWithValue("@Aciklama", kategori.Aciklama ?? (object)DBNull.Value);
            cmd.ExecuteNonQuery();
          }
        }

        return OperationResult.Ok(UiText.Get("CategoryAdded"));
      }
      catch (SqlException ex)when (ex.Number == 2601 || ex.Number == 2627)
      {
        return OperationResult.Fail(UiText.Get("CategoryNameExists"));
      }
      catch (Exception ex)
      {
        return OperationResult.Fail(UiText.Format("CategoryAddException", ex.Message));
      }
    }

    public OperationResult Update(Category kategori)
    {
      try
      {
        using (SqlConnection conn = SqlDatabaseConnection.GetConnection())
        {
          string query = "UPDATE Kategori SET KategoriAdi = @KategoriAdi, Aciklama = @Aciklama, GuncellemeTarihi = GETDATE() WHERE KategoriId = @KategoriId";
          using (SqlCommand cmd = new SqlCommand(query, conn))
          {
            cmd.Parameters.AddWithValue("@KategoriId", kategori.KategoriId);
            cmd.Parameters.AddWithValue("@KategoriAdi", kategori.KategoriAdi);
            cmd.Parameters.AddWithValue("@Aciklama", kategori.Aciklama ?? (object)DBNull.Value);
            if (cmd.ExecuteNonQuery() == 0)
              return OperationResult.Fail(UiText.Get("CategoryNotFound"));
          }
        }

        return OperationResult.Ok(UiText.Get("CategoryUpdated"));
      }
      catch (SqlException ex)when (ex.Number == 2601 || ex.Number == 2627)
      {
        return OperationResult.Fail(UiText.Get("CategoryNameExists"));
      }
      catch (Exception ex)
      {
        return OperationResult.Fail(UiText.Format("CategoryUpdateException", ex.Message));
      }
    }

    public OperationResult Delete(int kategoriId)
    {
      try
      {
        using (SqlConnection conn = GetConnection())
        using (SqlCommand cmd = new SqlCommand(@"
            DELETE FROM Kategori
            WHERE KategoriId = @KategoriId
              AND NOT EXISTS (SELECT 1 FROM Urun WHERE KategoriId = @KategoriId)", conn))
        {
          cmd.Parameters.AddWithValue("@KategoriId", kategoriId);
          if (cmd.ExecuteNonQuery() > 0)
            return OperationResult.Ok(UiText.Get("CategoryDeleted"));
        }

        return OperationResult.Fail(UiText.Get("CategoryDeleteRejected"));
      }
      catch (SqlException ex)when (ex.Number == 547)
      {
        return OperationResult.Fail(UiText.Get("CategoryInUse"));
      }
      catch (Exception ex)
      {
        return OperationResult.Fail(UiText.Get("CategoryDeleteError") + ex.Message);
      }
    }

#region Güvenli Okuma Metotları
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

