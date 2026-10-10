using InventoryTracking.UI.Localization;
using System;
using System.Data;
using System.Data.SqlClient;

namespace InventoryTracking.Connection
{
  public static class SqlDatabaseConnection
  {
    private static SqlConnection _connection = null;
    public static bool AuditAvailable { get; private set; }

    // Satış penceresi ve günlük kendi bağlantısını kullanır; açık transaction
    // başka bir ekranın sorgusuyla paylaşılmaz.
    public static SqlConnection OpenNewConnection()
    {
      var connection = new SqlConnection(ResolveConnectionString());
      try { connection.Open(); InitializeSession(connection); return connection; }
      catch { connection.Dispose(); throw; }
    }

    private static void InitializeSession(SqlConnection connection)
    {
      using (var command = new SqlCommand(@"
        EXEC sys.sp_set_session_context @key=N'KullaniciAdi', @value=@Kullanici;
        SELECT CASE WHEN OBJECT_ID(N'dbo.IslemGunlugu',N'U') IS NOT NULL THEN 1 ELSE 0 END;", connection))
      {
        command.Parameters.Add("@Kullanici", SqlDbType.NVarChar, 100).Value = Environment.UserName;
        AuditAvailable = Convert.ToInt32(command.ExecuteScalar()) == 1;
      }
    }

    public static string ResolveConnectionString()
    {
      string connectionString = Environment.GetEnvironmentVariable("INVENTORYTRACKING_CONNECTION_STRING");
      if (string.IsNullOrWhiteSpace(connectionString)) connectionString = ConnectionSettingsStore.Load();
      if (string.IsNullOrWhiteSpace(connectionString))
        connectionString = InventoryTracking.Properties.Settings.Default.DBConStr;
      return connectionString;
    }

    public static SqlConnection GetConnection()
    {
      // using bloğu Dispose çağırdığında nesne null olmaz, bağlantı dizesi temizlenir.
      // Sonraki sorguda aynı hatayı almamak için bu durumda bağlantıyı yeniden oluşturuyoruz.
      if (_connection == null || string.IsNullOrWhiteSpace(_connection.ConnectionString))
      {
        // Dağıtımda bağlantı bilgisini uygulama ayarı yerine ortam değişkeninden alabiliriz.
        string connectionString = ResolveConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
          throw new InvalidOperationException(UiText.Get("EmptyDbConfig"));

        _connection = new SqlConnection(connectionString);
      }

      if (_connection.State == ConnectionState.Closed)
      {
        _connection.Open();
        InitializeSession(_connection);
      }

      return _connection;
    }

    public static void CloseConnection()
    {
      // Close bağlantı dizesini korur; sonraki çağrı aynı nesneyi tekrar açabilir.
      if (_connection != null && _connection.State == ConnectionState.Open)
        _connection.Close();
    }

    public static void ResetConnection()
    {
      _connection?.Dispose();
      _connection = null;
      AuditAvailable = false;
    }
  }
}
