using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using InventoryTracking.Connection;

namespace InventoryTracking
{
  internal static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
      try
      {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        string connectionError;
        try { connectionError = ConnectionSettingsStore.Test(SqlDBConnection.ResolveConnectionString()); }
        catch (Exception ex) { connectionError = ex.Message; }
        if (connectionError != null)
        {
          if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("INVENTORYTRACKING_CONNECTION_STRING")))
          {
            MessageBox.Show(connectionError + "\n\nOrtam değişkenindeki bağlantı bilgisini düzeltin.",
              "Veritabanı Bağlantısı", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
          }
          using (var setup = new InventoryTracking.UI.ConnectionSetupForm())
          {
            if (setup.ShowDialog() != DialogResult.OK) return;
          }
        }
        Application.Run(new InventoryTracking.UI.MainForm());
      }
      catch (Exception ex)
      {
        MessageBox.Show($"Uygulama başlatılırken hata oluştu:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
            "Kritik Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }
  }
}


