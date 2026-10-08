using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

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


