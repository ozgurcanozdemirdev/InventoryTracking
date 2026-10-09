using InventoryTracking.Localization;
using System;
using System.Data;
using System.Data.SqlClient;

namespace InventoryTracking.Connection
{
  public static class SqlDBConnection
  {
    private static SqlConnection _connection = null;

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
        _connection.Open();

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
    }
  }
}
