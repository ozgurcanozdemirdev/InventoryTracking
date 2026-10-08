using InventoryTracking.Connection;
using InventoryTracking.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using static InventoryTracking.Models.Entities;

namespace InventoryTracking.DataAccess
{
  public class KategoriRepository
  {
    private SqlConnection GetConnection() => SqlDBConnection.GetConnection();

    public List<Kategori> GetAll()
    {
      var kategoriler = new List<Kategori>();

      using (SqlConnection conn = SqlDBConnection.GetConnection())
      {
        string query = "SELECT KategoriId, KategoriAdi, Aciklama, OlusturulmaTarihi, GuncellemeTarihi FROM Kategori ORDER BY KategoriAdi";
        using (SqlCommand cmd = new SqlCommand(query, conn))
        using (SqlDataReader reader = cmd.ExecuteReader())
        {
          while (reader.Read())
          {
            kategoriler.Add(new Kategori
            {
              KategoriId = SafeGetInt(reader, "KategoriId"),
              KategoriAdi = SafeGetString(reader, "KategoriAdi"),
              Aciklama = SafeGetString(reader, "Aciklama"),
              OlusturulmaTarihi = SafeGetDateTime(reader, "OlusturulmaTarihi"),
              GuncellemeTarihi = SafeGetDateTime(reader, "GuncellemeTarihi")
            });
          }
        }
      }
      return kategoriler;
    }

    public Kategori GetById(int kategoriId)
    {
      using (SqlConnection conn = GetConnection())
      using (SqlCommand cmd = new SqlCommand("SELECT KategoriId, KategoriAdi, Aciklama, OlusturulmaTarihi, GuncellemeTarihi FROM Kategori WHERE KategoriId = @KategoriId", conn))
      {
        cmd.Parameters.AddWithValue("@KategoriId", kategoriId);

        using (SqlDataReader reader = cmd.ExecuteReader())
        {
          if (reader.Read())
          {
            return new Kategori
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

    public OperationResult Add(Kategori kategori)
    {
      try
      {
        using (SqlConnection conn = SqlDBConnection.GetConnection())
        {
          string query = "INSERT INTO Kategori (KategoriAdi, Aciklama) VALUES (@KategoriAdi, @Aciklama)";
          using (SqlCommand cmd = new SqlCommand(query, conn))
          {
            cmd.Parameters.AddWithValue("@KategoriAdi", kategori.KategoriAdi);
            cmd.Parameters.AddWithValue("@Aciklama", kategori.Aciklama ?? (object)DBNull.Value);
            cmd.ExecuteNonQuery();
          }
        }
        return OperationResult.Ok("Kategori başarıyla eklendi");
      }
      catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
      {
        return OperationResult.Fail("Bu isimde bir kategori zaten kayıtlıdır.");
      }
      catch (Exception ex)
      {
        return OperationResult.Fail($"Kategori ekleme hatası: {ex.Message}");
      }
    }

    public OperationResult Update(Kategori kategori)
    {
      try
      {
        using (SqlConnection conn = SqlDBConnection.GetConnection())
        {
          string query = "UPDATE Kategori SET KategoriAdi = @KategoriAdi, Aciklama = @Aciklama, GuncellemeTarihi = GETDATE() WHERE KategoriId = @KategoriId";
          using (SqlCommand cmd = new SqlCommand(query, conn))
          {
            cmd.Parameters.AddWithValue("@KategoriId", kategori.KategoriId);
            cmd.Parameters.AddWithValue("@KategoriAdi", kategori.KategoriAdi);
            cmd.Parameters.AddWithValue("@Aciklama", kategori.Aciklama ?? (object)DBNull.Value);
            if (cmd.ExecuteNonQuery() == 0)
              return OperationResult.Fail("Kategori bulunamadı.");
          }
        }
        return OperationResult.Ok("Kategori güncellendi");
      }
      catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
      {
        return OperationResult.Fail("Bu isimde bir kategori zaten kayıtlıdır.");
      }
      catch (Exception ex)
      {
        return OperationResult.Fail($"Kategori güncelleme hatası: {ex.Message}");
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
            return OperationResult.Ok("Kategori silindi.");
        }
        return OperationResult.Fail("Kategori bulunamadı veya bir üründe kullanılıyor. Kullanılan kategori silinemez.");
      }
      catch (SqlException ex) when (ex.Number == 547)
      {
        return OperationResult.Fail("Kategori başka kayıtlarda kullanılıyor; silinemez.");
      }
      catch (Exception ex)
      {
        return OperationResult.Fail("Kategori silme hatası: " + ex.Message);
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

  /// <summary>
  /// Birim Repository - Birim işlemleri
  /// </summary>
  public class BirimRepository
  {
    private SqlConnection GetConnection() => SqlDBConnection.GetConnection();

    public List<Birim> GetAll()
    {
      var birimler = new List<Birim>();
      using (SqlConnection conn = SqlDBConnection.GetConnection())
      {
        string query = "SELECT BirimId, BirimAdi, Kisaltma, OlusturulmaTarihi FROM Birim ORDER BY BirimAdi";
        using (SqlCommand cmd = new SqlCommand(query, conn))
        using (SqlDataReader reader = cmd.ExecuteReader())
        {
          while (reader.Read())
          {
            birimler.Add(new Birim
            {
              BirimId = SafeGetInt(reader, "BirimId"),
              BirimAdi = SafeGetString(reader, "BirimAdi"),
              Kisaltma = SafeGetString(reader, "Kisaltma"),
              OlusturulmaTarihi = SafeGetDateTime(reader, "OlusturulmaTarihi")
            });
          }
        }
      }
      return birimler;
    }


    public Birim GetById(int birimId)
    {
      using (SqlConnection conn = GetConnection())
      using (SqlCommand cmd = new SqlCommand("SELECT BirimId, BirimAdi, Kisaltma, OlusturulmaTarihi FROM Birim WHERE BirimId = @BirimId", conn))
      {
        cmd.Parameters.AddWithValue("@BirimId", birimId);

        using (SqlDataReader reader = cmd.ExecuteReader())
        {
          if (reader.Read())
          {
            return new Birim
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

  /// <summary>
  /// Depo Repository - Depo işlemleri
  /// </summary>
  public class DepoRepository
  {
    private SqlConnection GetConnection() => SqlDBConnection.GetConnection();

    public List<Depo> GetAll()
    {
      var depolar = new List<Depo>();
      using (SqlConnection conn = GetConnection())
      using (SqlCommand cmd = new SqlCommand("SELECT DepoId, DepoAdi, Lokasyon, Aciklama, AktifMi FROM Depo ORDER BY AktifMi DESC, DepoAdi", conn))
      using (SqlDataReader reader = cmd.ExecuteReader())
      {
        while (reader.Read())
        {
          depolar.Add(new Depo
          {
            DepoId = reader.GetInt32(0),
            DepoAdi = reader.GetString(1),
            Lokasyon = reader.IsDBNull(2) ? "" : reader.GetString(2),
            Aciklama = reader.IsDBNull(3) ? "" : reader.GetString(3),
            AktifMi = reader.GetBoolean(4)
          });
        }
      }
      return depolar;
    }

    public List<Depo> GetAllActive()
    {
      var depolar = new List<Depo>();
      using (SqlConnection conn = SqlDBConnection.GetConnection())
      {
        string query = "SELECT DepoId, DepoAdi, Lokasyon, Aciklama, AktifMi, OlusturulmaTarihi, GuncellemeTarihi FROM Depo WHERE AktifMi = 1 ORDER BY DepoAdi";
        using (SqlCommand cmd = new SqlCommand(query, conn))
        using (SqlDataReader reader = cmd.ExecuteReader())
        {
          while (reader.Read())
          {
            depolar.Add(new Depo
            {
              DepoId = SafeGetInt(reader, "DepoId"),
              DepoAdi = SafeGetString(reader, "DepoAdi"),
              Lokasyon = SafeGetString(reader, "Lokasyon"),
              Aciklama = SafeGetString(reader, "Aciklama"),
              AktifMi = SafeGetBoolean(reader, "AktifMi"),
              OlusturulmaTarihi = SafeGetDateTime(reader, "OlusturulmaTarihi"),
              GuncellemeTarihi = SafeGetDateTime(reader, "GuncellemeTarihi")
            });
          }
        }
      }
      return depolar;
    }

    public Depo GetById(int depoId)
    {
      using (SqlConnection conn = SqlDBConnection.GetConnection())
      {
        string query = "SELECT DepoId, DepoAdi, Lokasyon, Aciklama, AktifMi, OlusturulmaTarihi, GuncellemeTarihi FROM Depo WHERE DepoId = @DepoId";
        using (SqlCommand cmd = new SqlCommand(query, conn))
        {
          cmd.Parameters.AddWithValue("@DepoId", depoId);
          using (SqlDataReader reader = cmd.ExecuteReader())
          {
            if (reader.Read())
            {
              return new Depo
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

    public OperationResult Add(Depo depo)
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
          return OperationResult.Ok("Depo eklendi.");
        }
      }
      catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
      {
        return OperationResult.Fail("Bu isimde bir depo zaten kayıtlıdır.");
      }
      catch (Exception ex)
      {
        return OperationResult.Fail("Depo ekleme hatası: " + ex.Message);
      }
    }

    public OperationResult Update(Depo depo)
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
          return cmd.ExecuteNonQuery() > 0
            ? OperationResult.Ok("Depo güncellendi.") : OperationResult.Fail("Depo bulunamadı.");
        }
      }
      catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
      {
        return OperationResult.Fail("Bu isimde bir depo zaten kayıtlıdır.");
      }
      catch (Exception ex)
      {
        return OperationResult.Fail("Depo güncelleme hatası: " + ex.Message);
      }
    }

    public OperationResult SetActive(int depoId, bool aktifMi)
    {
      try
      {
        using (SqlConnection conn = GetConnection())
        using (SqlTransaction transaction = conn.BeginTransaction(IsolationLevel.Serializable))
        using (SqlCommand cmd = new SqlCommand(aktifMi
          ? "UPDATE Depo SET AktifMi=1, GuncellemeTarihi=GETDATE() WHERE DepoId=@DepoId AND AktifMi=0"
          : @"UPDATE Depo SET AktifMi=0, GuncellemeTarihi=GETDATE()
              WHERE DepoId=@DepoId AND AktifMi=1
                AND (SELECT COUNT(*) FROM Depo WHERE AktifMi=1)>1
                AND NOT EXISTS (SELECT 1 FROM Stok WHERE DepoId=@DepoId AND Miktar>0)", conn, transaction))
        {
          cmd.Parameters.AddWithValue("@DepoId", depoId);
          int affected = cmd.ExecuteNonQuery();
          if (affected == 0)
            return OperationResult.Fail(aktifMi
              ? "Depo bulunamadı veya zaten aktif."
              : "Depo pasife alınamadı. Son aktif depo veya içinde stok bulunan depo kapatılamaz.");
          transaction.Commit();
          return OperationResult.Ok(aktifMi ? "Depo etkinleştirildi." : "Depo pasife alındı.");
        }
      }
      catch (Exception ex)
      {
        return OperationResult.Fail("Depo durumu değiştirilemedi: " + ex.Message);
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

  /// <summary>
  /// Ürün Repository - Ürün işlemleri
  /// </summary>
  public class UrunRepository
  {
    // 02 - Bağlantı adresini merkezi sınıftan güvenli havuz mantığıyla alır.
    private SqlConnection GetConnection() => SqlDBConnection.GetConnection();

    public bool IsNameTaken(string urunAdi, int excludedUrunId = 0)
    {
      // Eklemede 0, düzenlemede mevcut ürünün ID'si gönderilir. SQL Server'ın
      // Türkçe ve büyük/küçük harfe duyarsız karşılaştırması aynı adı bulur.
      using (SqlConnection conn = GetConnection())
      using (SqlCommand cmd = new SqlCommand(
        "SELECT CASE WHEN EXISTS (SELECT 1 FROM Urun WHERE UrunAdi = @UrunAdi AND UrunId <> @ExcludedUrunId) THEN 1 ELSE 0 END", conn))
      {
        cmd.Parameters.AddWithValue("@UrunAdi", urunAdi.Trim());
        cmd.Parameters.AddWithValue("@ExcludedUrunId", excludedUrunId);
        return (int)cmd.ExecuteScalar() == 1;
      }
    }

    /// <summary>
    /// 03 - Barkoda göre ürün arama işlemi.
    /// </summary>
    public Urun GetByBarkod(string barkod)
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
              return new Urun
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
    public Urun GetInactiveByBarkod(string barkod)
    {
      int urunId;
      using (SqlConnection conn = GetConnection())
      using (SqlCommand cmd = new SqlCommand(
        "SELECT UrunId FROM Urun WHERE Barkod = @Barkod AND AktifMi = 0", conn))
      {
        cmd.Parameters.AddWithValue("@Barkod", barkod.Trim());
        var id = cmd.ExecuteScalar();
        if (id == null) return null;
        urunId = (int)id;
      }
      return GetById(urunId);
    }

    public OperationResult Reactivate(int urunId)
    {
      try
      {
        using (SqlConnection conn = GetConnection())
        using (SqlCommand cmd = new SqlCommand(
          "UPDATE Urun SET AktifMi = 1, GuncellemeTarihi = GETDATE() WHERE UrunId = @UrunId AND AktifMi = 0", conn))
        {
          cmd.Parameters.AddWithValue("@UrunId", urunId);
          return cmd.ExecuteNonQuery() == 1
            ? OperationResult.Ok("Ürün yeniden etkinleştirildi. Mevcut stok kayıtları korundu.")
            : OperationResult.Fail("Pasif ürün bulunamadı.");
        }
      }
      catch (Exception ex)
      {
        return OperationResult.Fail("Ürün etkinleştirilemedi: " + ex.Message);
      }
    }

    /// <summary>
    /// 05 - Aktif tüm ürünleri listeleme işlemi.
    /// </summary>
    public List<Urun> GetAll()
    {
      var urunler = new List<Urun>();

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
                urunler.Add(new Urun
                {
                  UrunId = SafeGetInt(reader, "UrunId"),
                  Barkod = SafeGetString(reader, "Barkod"),
                  UrunAdi = SafeGetString(reader, "UrunAdi"),
                  KategoriAdi = SafeGetString(reader, "KategoriAdi"),
                  BirimAdi = SafeGetString(reader, "BirimAdi"),
                  Fiyat = SafeGetDecimal(reader, "Fiyat"),
                  MinimumStok = SafeGetInt(reader, "MinimumStok"),
                  MaksimumStok = SafeGetInt(reader, "MaksimumStok"),
                  // GÜNCELLENDİ: SafeGetBoolean burada çağrılıyor
                  AktifMi = SafeGetBoolean(reader, "AktifMi"),
                  OlusturulmaTarihi = SafeGetDateTime(reader, "OlusturulmaTarihi")
                });
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        throw new Exception($"Ürünler listelenirken hata oluştu: {ex.Message}", ex);
      }

      return urunler;
    }

    /// <summary>
    /// 06 - ID değerine göre ürün getirme.
    /// </summary>
    public Urun GetById(int urunId)
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
              return new Urun
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
    public OperationResult Add(Urun urun, SqlTransaction transaction = null)
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
        finally { if (transaction == null) conn.Dispose(); }
        return OperationResult.Ok("Ürün başarıyla eklendi");
      }
      catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
      {
        // Ön kontrolü iki kullanıcı aynı anda geçebilir; benzersiz indeks son
        // güvenceyi sağlar ve burada kullanıcıya anlaşılır bir sonuç döneriz.
        return OperationResult.Fail(ex.Message.Contains("UX_Urun_UrunAdi")
          ? "Bu isimde bir ürün zaten kayıtlıdır."
          : "Bu barkod zaten sistemde kayıtlıdır.");
      }
      catch (Exception ex)
      {
        return OperationResult.Fail($"Ürün ekleme hatası: {ex.Message}");
      }
    }

    /// <summary>
    /// 08 - Ürün bilgilerini güncelleme.
    /// </summary>
    public OperationResult Update(Urun urun, SqlTransaction transaction = null)
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
              return OperationResult.Fail("Ürün bulunamadı.");
          }
        }
        finally { if (transaction == null) conn.Dispose(); }
        return OperationResult.Ok("Ürün başarıyla güncellendi");
      }
      catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
      {
        return OperationResult.Fail(ex.Message.Contains("UX_Urun_UrunAdi")
          ? "Bu isimde bir ürün zaten kayıtlıdır."
          : "Bu barkod zaten sistemde kayıtlıdır.");
      }
      catch (Exception ex)
      {
        return OperationResult.Fail($"Ürün güncelleme hatası: {ex.Message}");
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

            return affectedRows > 0 ? OperationResult.Ok("Ürün başarıyla silindi") : OperationResult.Fail("Ürün bulunamadı");
          }
        }
      }
      catch (Exception ex)
      {
        return OperationResult.Fail($"Ürün silme hatası: {ex.Message}");
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
}
/// <summary>
/// Stok Repository - Stok işlemleri
/// </summary>
public class StokRepository
{
  private SqlConnection GetConnection() => SqlDBConnection.GetConnection();

  /// <summary>
  /// 12 - Belirli bir depodaki tüm stokları listeler.
  /// </summary>
  public List<Stok> GetByDepo(int depoId)
  {
    var stoklar = new List<Stok>();
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
            stoklar.Add(new Stok
            {
              StokId = SafeGetInt(r, "StokId"),
              UrunId = SafeGetInt(r, "UrunId"),
              DepoId = depoId,
              Barkod = SafeGetString(r, "Barkod"),
              MalzemeNo = SafeGetString(r, "MalzemeNo"),
              UrunAdi = SafeGetString(r, "UrunAdi"),
              KategoriAdi = SafeGetString(r, "KategoriAdi"),
              BirimAdi = SafeGetString(r, "BirimAdi"),
              Fiyat = SafeGetDecimal(r, "Fiyat"),
              Miktar = SafeGetInt(r, "Miktar"),
              StokDurumu = SafeGetString(r, "StokDurumu"),
              SonHareketTarihi = r.IsDBNull(r.GetOrdinal("SonHareketTarihi"))
                ? (DateTime?)null : r.GetDateTime(r.GetOrdinal("SonHareketTarihi")),
              OlusturulmaTarihi = SafeGetDateTime(r, "OlusturulmaTarihi"),
              GuncellemeTarihi = SafeGetDateTime(r, "GuncellemeTarihi")
            });
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
  public Stok GetByUrunAndDepo(int urunId, int depoId, SqlTransaction transaction = null)
  {
    // Eğer aktif bir Transaction varsa aynı bağlantı kullanılır, yoksa yeni bağlantı açılır.
    SqlConnection conn = transaction?.Connection ?? GetConnection();
    try
    {
      using (SqlCommand cmd = new SqlCommand("SELECT StokId, Miktar FROM Stok" +
        (transaction == null ? "" : " WITH (UPDLOCK, HOLDLOCK)") +
        " WHERE UrunId = @UrunId AND DepoId = @DepoId", conn))
      {
        if (transaction != null)
          cmd.Transaction = transaction;

        cmd.Parameters.AddWithValue("@UrunId", urunId);
        cmd.Parameters.AddWithValue("@DepoId", depoId);

        using (SqlDataReader r = cmd.ExecuteReader())
        {
          if (r.Read())
          {
            return new Stok
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
  public OperationResult UpdateStok(int urunId, int depoId, int degisim, string tip, string aciklama,
    SqlTransaction transaction = null, int? transferId = null)
  {
    if (urunId <= 0 || depoId <= 0 || string.IsNullOrWhiteSpace(tip))
      return OperationResult.Fail("Geçersiz stok işlemi.");
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
            return OperationResult.Fail("Stok işlemi için ürün ve depo aktif olmalıdır.");
        }

        // Mevcut miktar kontrolü
        var mevcut = GetByUrunAndDepo(urunId, depoId, trans);
        int eskiMiktar = mevcut?.Miktar ?? 0;
        int yeniMiktar = checked(eskiMiktar + degisim);

        if (yeniMiktar < 0)
        {
          return OperationResult.Fail("Yetersiz stok! İşlem gerçekleştirilemedi.");
        }

        // Stok miktarını güncelle
        using (SqlCommand cmd = new SqlCommand(mevcut == null
          ? "INSERT INTO Stok (UrunId, DepoId, Miktar) VALUES (@UrunId, @DepoId, @Miktar)"
          : "UPDATE Stok SET Miktar = @Miktar, GuncellemeTarihi = GETDATE() WHERE UrunId = @UrunId AND DepoId = @DepoId", conn, trans))
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
        if (transaction == null) trans.Commit();
        return OperationResult.Ok("Stok başarıyla güncellendi");
    }
    catch (Exception ex)
    {
      if (transaction == null) trans.Rollback();
      return OperationResult.Fail($"Stok güncelleme hatası: {ex.Message}");
    }
    finally
    {
      if (transaction == null) { trans.Dispose(); conn.Dispose(); }
    }
  }

  public OperationResult TransferStock(int urunId, int kaynakDepoId, int hedefDepoId, int miktar, string aciklama)
  {
    if (urunId <= 0 || kaynakDepoId <= 0 || hedefDepoId <= 0 ||
        kaynakDepoId == hedefDepoId || miktar <= 0 || string.IsNullOrWhiteSpace(aciklama) || aciklama.Trim().Length > 500)
      return OperationResult.Fail("Ürün, farklı iki depo, pozitif miktar ve en fazla 500 karakterlik açıklama zorunludur.");

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
            return OperationResult.Fail("Ürün veya seçilen depolardan biri aktif değil.");
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
        var cikis = UpdateStok(urunId, kaynakDepoId, -miktar, "Transfer Çıkış", not, transaction, transferId);
        if (!cikis.Success) { transaction.Rollback(); return cikis; }
        var giris = UpdateStok(urunId, hedefDepoId, miktar, "Transfer Giriş", not, transaction, transferId);
        if (!giris.Success) { transaction.Rollback(); return giris; }
        transaction.Commit();
        return OperationResult.Ok($"Transfer tamamlandı. İşlem numarası: {transferId}", transferId);
      }
    }
    catch (Exception ex)
    {
      return OperationResult.Fail("Stok transferi yapılamadı: " + ex.Message);
    }
  }

  /// <summary>
  /// 15 - Stoğu doğrudan belirli bir değere set etme (Upsert).
  /// </summary>
  public OperationResult UpsertStok(int urunId, int depoId, int miktar, string neden)
  {
    if (urunId <= 0 || depoId <= 0 || miktar < 0 || string.IsNullOrWhiteSpace(neden) || neden.Trim().Length > 500)
      return OperationResult.Fail("Geçerli stok miktarı ve en fazla 500 karakterlik düzeltme nedeni zorunludur.");
    try
    {
      using (SqlConnection conn = GetConnection())
      using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.Serializable))
      {
        int mevcut = GetByUrunAndDepo(urunId, depoId, trans)?.Miktar ?? 0;
        var result = UpdateStok(urunId, depoId, checked(miktar - mevcut), "Sayım Düzeltmesi", neden.Trim(), trans);
        if (!result.Success) { trans.Rollback(); return result; }
        trans.Commit();
        return result;
      }
    }
    catch (Exception ex)
    {
      return OperationResult.Fail($"Stok set etme hatası: {ex.Message}");
    }
  }

  public void UrunleriListele(DataGridView dgvStoklar)
  {
    try
    {
      UrunRepository urunRepo = new UrunRepository();
      var urunListesi = urunRepo.GetAll();

      dgvStoklar.DataSource = null; // Önce temizlemek, eski kalıntıların veya hatalı bind işlemlerinin önüne geçer
      dgvStoklar.DataSource = urunListesi; // Veriyi atıyoruz
    }
    catch (Exception ex)
    {
      MessageBox.Show(ex.Message);
    }
  }
}



/*
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
        MessageBox.Show("Veriler yüklenirken hata oluştu: " + ex.Message);
      }
    }
 */
