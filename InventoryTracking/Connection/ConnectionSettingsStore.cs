using InventoryTracking.UI.Localization;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace InventoryTracking.Connection
{
  public static class ConnectionSettingsStore
  {
    public static string DefaultPath => Path.Combine(
      Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
      "InventoryTracking", "connection.dat");

    public static string Load(string path = null)
    {
      string file = path ?? DefaultPath;
      if (!File.Exists(file)) return null;
      try
      {
        byte[] encrypted = File.ReadAllBytes(file);
        return Encoding.UTF8.GetString(ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser));
      }
      catch (Exception ex) when (ex is CryptographicException || ex is IOException)
      {
        throw new InvalidOperationException(UiText.Get("SavedConnectionUnreadable"), ex);
      }
    }

    public static void Save(string connectionString, string path = null)
    {
      if (string.IsNullOrWhiteSpace(connectionString))
        throw new ArgumentException(UiText.Get("EmptyConnection"), nameof(connectionString));
      string file = path ?? DefaultPath;
      string folder = Path.GetDirectoryName(file);
      Directory.CreateDirectory(folder);
      byte[] encrypted = ProtectedData.Protect(Encoding.UTF8.GetBytes(connectionString),
        null, DataProtectionScope.CurrentUser);
      string temporary = file + ".new";
      try
      {
        File.WriteAllBytes(temporary, encrypted);
        if (File.Exists(file)) File.Replace(temporary, file, null);
        else File.Move(temporary, file);
      }
      finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }

    public static string Test(string connectionString)
    {
      try
      {
        var builder = new SqlConnectionStringBuilder(connectionString);
        if (string.IsNullOrWhiteSpace(builder.DataSource) || string.IsNullOrWhiteSpace(builder.InitialCatalog))
          return UiText.Get("SqlServerRequired");
        using (var connection = new SqlConnection(builder.ConnectionString))
        {
          connection.Open();
          using (var command = new SqlCommand(@"
              SELECT TOP (0) UrunId FROM dbo.Urun;
              SELECT TOP (0) Miktar FROM dbo.Stok;
              SELECT TOP (0) CikisMi, TransferId FROM dbo.StokHareketi;
              SELECT TOP (0) TransferId FROM dbo.StokTransferi;
              SELECT TOP (0) MalzemeId FROM dbo.MalzemeKatalog;", connection))
            command.ExecuteNonQuery();
        }
        return null;
      }
      catch (SqlException ex)
      {
        return UiText.Get("SchemaConnectionError") + ex.Message;
      }
      catch (ArgumentException ex) { return UiText.Get("InvalidConnection") + ex.Message; }
    }
  }
}
