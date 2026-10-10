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
  /// Birim Repository - Birim işlemleri
  /// </summary>
  public class UnitRepository
  {
    private SqlConnection GetConnection() => SqlDatabaseConnection.GetConnection();
    public List<Unit> GetAll()
    {
      var birimler = new List<Unit>();
      using (SqlConnection conn = SqlDatabaseConnection.GetConnection())
      {
        string query = "SELECT BirimId, BirimAdi, Kisaltma, OlusturulmaTarihi FROM Birim ORDER BY BirimAdi";
        using (SqlCommand cmd = new SqlCommand(query, conn))
        using (SqlDataReader reader = cmd.ExecuteReader())
        {
          while (reader.Read())
          {
            birimler.Add(new Unit { BirimId = SafeGetInt(reader, "BirimId"), BirimAdi = SafeGetString(reader, "BirimAdi"), Kisaltma = SafeGetString(reader, "Kisaltma"), OlusturulmaTarihi = SafeGetDateTime(reader, "OlusturulmaTarihi") });
          }
        }
      }

      return birimler;
    }

    public Unit GetById(int birimId)
    {
      using (SqlConnection conn = GetConnection())
      using (SqlCommand cmd = new SqlCommand("SELECT BirimId, BirimAdi, Kisaltma, OlusturulmaTarihi FROM Birim WHERE BirimId = @BirimId", conn))
      {
        cmd.Parameters.AddWithValue("@BirimId", birimId);
        using (SqlDataReader reader = cmd.ExecuteReader())
        {
          if (reader.Read())
          {
            return new Unit
            {
              BirimId = Convert.ToInt32(reader["BirimId"]),
              BirimAdi = reader["BirimAdi"].ToString(),
              Kisaltma = reader["Kisaltma"].ToString(),
              OlusturulmaTarihi = Convert.ToDateTime(reader["OlusturulmaTarihi"])
            };
          }
        }
      }

      return null;
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

