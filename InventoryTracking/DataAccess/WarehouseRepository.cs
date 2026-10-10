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
  /// Depo Repository - Depo işlemleri
  /// </summary>
  public class WarehouseRepository
  {
    private SqlConnection GetConnection() => SqlDatabaseConnection.GetConnection();
    public List<Warehouse> GetAll()
    {
      var depolar = new List<Warehouse>();
      using (SqlConnection conn = GetConnection())
      using (SqlCommand cmd = new SqlCommand("SELECT DepoId, DepoAdi, Lokasyon, Aciklama, AktifMi FROM Depo ORDER BY AktifMi DESC, DepoAdi", conn))
      using (SqlDataReader reader = cmd.ExecuteReader())
      {
        while (reader.Read())
        {
          depolar.Add(new Warehouse { DepoId = reader.GetInt32(0), DepoAdi = reader.GetString(1), Lokasyon = reader.IsDBNull(2) ? "" : reader.GetString(2), Aciklama = reader.IsDBNull(3) ? "" : reader.GetString(3), AktifMi = reader.GetBoolean(4) });
        }
      }

      return depolar;
    }

    public List<Warehouse> GetAllActive()
    {
      var depolar = new List<Warehouse>();
      using (SqlConnection conn = SqlDatabaseConnection.GetConnection())
      {
        string query = "SELECT DepoId, DepoAdi, Lokasyon, Aciklama, AktifMi, OlusturulmaTarihi, GuncellemeTarihi FROM Depo WHERE AktifMi = 1 ORDER BY DepoAdi";
        using (SqlCommand cmd = new SqlCommand(query, conn))
        using (SqlDataReader reader = cmd.ExecuteReader())
        {
          while (reader.Read())
          {
            depolar.Add(new Warehouse { DepoId = SafeGetInt(reader, "DepoId"), DepoAdi = SafeGetString(reader, "DepoAdi"), Lokasyon = SafeGetString(reader, "Lokasyon"), Aciklama = SafeGetString(reader, "Aciklama"), AktifMi = SafeGetBoolean(reader, "AktifMi"), OlusturulmaTarihi = SafeGetDateTime(reader, "OlusturulmaTarihi"), GuncellemeTarihi = SafeGetDateTime(reader, "GuncellemeTarihi") });
          }
        }
      }

      return depolar;
    }

    public Warehouse GetById(int depoId)
    {
      using (SqlConnection conn = SqlDatabaseConnection.GetConnection())
      {
        string query = "SELECT DepoId, DepoAdi, Lokasyon, Aciklama, AktifMi, OlusturulmaTarihi, GuncellemeTarihi FROM Depo WHERE DepoId = @DepoId";
        using (SqlCommand cmd = new SqlCommand(query, conn))
        {
          cmd.Parameters.AddWithValue("@DepoId", depoId);
          using (SqlDataReader reader = cmd.ExecuteReader())
          {
            if (reader.Read())
            {
              return new Warehouse
              {
                DepoId = SafeGetInt(reader, "DepoId"),
                DepoAdi = SafeGetString(reader, "DepoAdi"),
                Lokasyon = SafeGetString(reader, "Lokasyon"),
                Aciklama = SafeGetString(reader, "Aciklama"),
                AktifMi = SafeGetBoolean(reader, "AktifMi"),
                OlusturulmaTarihi = SafeGetDateTime(reader, "OlusturulmaTarihi"),
                GuncellemeTarihi = SafeGetDateTime(reader, "GuncellemeTarihi")
              };
            }
          }
        }
      }

      return null;
    }

    public OperationResult Add(Warehouse depo)
    {
      try
      {
        using (SqlConnection conn = GetConnection())
        using (SqlCommand cmd = new SqlCommand(@"
            INSERT INTO Depo (DepoAdi, Lokasyon, Aciklama, AktifMi)
            VALUES (@DepoAdi, @Lokasyon, @Aciklama, 1)", conn))
        {
          cmd.Parameters.AddWithValue("@DepoAdi", depo.DepoAdi);
          cmd.Parameters.AddWithValue("@Lokasyon", depo.Lokasyon ?? "");
          cmd.Parameters.AddWithValue("@Aciklama", depo.Aciklama ?? "");
          cmd.ExecuteNonQuery();
          return OperationResult.Ok(UiText.Get("WarehouseAdded"));
        }
      }
      catch (SqlException ex)when (ex.Number == 2601 || ex.Number == 2627)
      {
        return OperationResult.Fail(UiText.Get("WarehouseNameExists"));
      }
      catch (Exception ex)
      {
        return OperationResult.Fail(UiText.Get("WarehouseAddError") + ex.Message);
      }
    }

    public OperationResult Update(Warehouse depo)
    {
      try
      {
        using (SqlConnection conn = GetConnection())
        using (SqlCommand cmd = new SqlCommand(@"
            UPDATE Depo SET DepoAdi=@DepoAdi, Lokasyon=@Lokasyon, Aciklama=@Aciklama,
                GuncellemeTarihi=GETDATE() WHERE DepoId=@DepoId", conn))
        {
          cmd.Parameters.AddWithValue("@DepoId", depo.DepoId);
          cmd.Parameters.AddWithValue("@DepoAdi", depo.DepoAdi);
          cmd.Parameters.AddWithValue("@Lokasyon", depo.Lokasyon ?? "");
          cmd.Parameters.AddWithValue("@Aciklama", depo.Aciklama ?? "");
          return cmd.ExecuteNonQuery() > 0 ? OperationResult.Ok(UiText.Get("WarehouseUpdated")) : OperationResult.Fail(UiText.Get("WarehouseNotFound"));
        }
      }
      catch (SqlException ex)when (ex.Number == 2601 || ex.Number == 2627)
      {
        return OperationResult.Fail(UiText.Get("WarehouseNameExists"));
      }
      catch (Exception ex)
      {
        return OperationResult.Fail(UiText.Get("WarehouseUpdateError") + ex.Message);
      }
    }

    public OperationResult SetActive(int depoId, bool aktifMi)
    {
      try
      {
        using (SqlConnection conn = GetConnection())
        using (SqlTransaction transaction = conn.BeginTransaction(IsolationLevel.Serializable))
        using (SqlCommand cmd = new SqlCommand(aktifMi ? "UPDATE Depo SET AktifMi=1, GuncellemeTarihi=GETDATE() WHERE DepoId=@DepoId AND AktifMi=0" : @"UPDATE Depo SET AktifMi=0, GuncellemeTarihi=GETDATE()
              WHERE DepoId=@DepoId AND AktifMi=1
                AND (SELECT COUNT(*) FROM Depo WHERE AktifMi=1)>1
                AND NOT EXISTS (SELECT 1 FROM Stok WHERE DepoId=@DepoId AND Miktar>0)", conn, transaction))
        {
          cmd.Parameters.AddWithValue("@DepoId", depoId);
          int affected = cmd.ExecuteNonQuery();
          if (affected == 0)
            return OperationResult.Fail(aktifMi ? UiText.Get("WarehouseAlreadyActive") : UiText.Get("WarehouseDeactivateRejected"));
          transaction.Commit();
          return OperationResult.Ok(aktifMi ? UiText.Get("WarehouseActivated") : UiText.Get("WarehouseDeactivated"));
        }
      }
      catch (Exception ex)
      {
        return OperationResult.Fail(UiText.Get("WarehouseStatusError") + ex.Message);
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

