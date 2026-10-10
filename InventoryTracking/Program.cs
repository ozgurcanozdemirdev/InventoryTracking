using System;
using System.Windows.Forms;
using InventoryTracking.Connection;
using InventoryTracking.UI.Localization;

namespace InventoryTracking
{
  internal static class Program
  {
    [STAThread]
    static void Main()
    {
      try
      {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // Formlar oluşturulmadan önce kayıtlı arayüz dili yüklenir.
        LanguageManager.Initialize();

        string connectionError;
        try
        {
          connectionError = ConnectionSettingsStore.Test(
            SqlDatabaseConnection.ResolveConnectionString());
        }
        catch (Exception ex)
        {
          connectionError = ex.Message;
        }

        if (connectionError != null)
        {
          if (!string.IsNullOrWhiteSpace(
            Environment.GetEnvironmentVariable(
              "INVENTORYTRACKING_CONNECTION_STRING")))
          {
            UiMessage.Show(
              connectionError +
              UiText.Get("EnvironmentConnectionHint"),
              UiText.Get("DatabaseConnection"),
              MessageBoxButtons.OK,
              MessageBoxIcon.Error);
            return;
          }

          using (var setup = new InventoryTracking.UI.Forms.ConnectionSetupForm())
          {
            if (setup.ShowDialog() != DialogResult.OK)
              return;
          }
        }

        Application.Run(new InventoryTracking.UI.Forms.MainForm());
      }
      catch (Exception ex)
      {
        UiMessage.Show(
          UiText.Format("StartupError", ex.Message, ex.StackTrace),
          UiText.Get("CriticalError"),
          MessageBoxButtons.OK,
          MessageBoxIcon.Error);
      }
    }
  }
}
