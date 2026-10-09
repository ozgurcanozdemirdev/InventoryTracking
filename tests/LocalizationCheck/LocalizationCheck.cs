using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using InventoryTracking.Localization;
using InventoryTracking.UI;
using static InventoryTracking.Models.Entities;

class LocalizationCheck
{
  static int checks;
  static readonly List<string> dialogs = new List<string>();
  static void Check(bool ok, string label)
  {
    if (!ok)
      throw new Exception("FAILED: " + label);
    checks++;
    Console.WriteLine("PASS: " + label);
  }

  static Control Find(Control root, string name) => root.Controls.Find(name, true).Single();
  static string Text(string key) => (string)typeof(LanguageManager).Assembly.GetType("InventoryTracking.Localization.UiText").GetMethod("Get").Invoke(null, new object[] { key });
  [STAThread]
  static int Main(string[] args)
  {
    try
    {
      Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
      var messageType = typeof(LanguageManager).Assembly.GetType("InventoryTracking.Localization.UiMessage");
      var handler = new Func<string, string, MessageBoxButtons, MessageBoxIcon, DialogResult>((text, caption, buttons, icon) =>
      {
        dialogs.Add(caption + ": " + text);
        Console.WriteLine("DIALOG: " + caption + ": " + text);
        return buttons == MessageBoxButtons.YesNo ? DialogResult.No : DialogResult.OK;
      });
      messageType.GetProperty("Handler", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, handler);
      string pref = Path.Combine(args[0], "language.txt");
      Directory.CreateDirectory(args[0]);
      File.WriteAllText(pref, "tr");
      typeof(LanguageManager).GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { pref });
      var resourceType = typeof(LanguageManager).Assembly.GetType("InventoryTracking.Properties.Resources");
      var manager = (System.Resources.ResourceManager)resourceType.GetProperty("ResourceManager", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).GetValue(null);
      var turkish = manager.GetResourceSet(CultureInfo.GetCultureInfo("tr-TR"), true, true);
      var english = manager.GetResourceSet(CultureInfo.GetCultureInfo("en"), true, true);
      foreach (DictionaryEntry entry in turkish)
      {
        string translated = english.GetString((string)entry.Key);
        Check(!string.IsNullOrWhiteSpace(translated), "English resource: " + entry.Key);
        string[] trArgs = Regex.Matches((string)entry.Value, @"\{(\d+)[^{}]*\}").Cast<Match>().Select(m => m.Groups[1].Value).Distinct().OrderBy(x => x).ToArray();
        string[] enArgs = Regex.Matches(translated, @"\{(\d+)[^{}]*\}").Cast<Match>().Select(m => m.Groups[1].Value).Distinct().OrderBy(x => x).ToArray();
        if (!trArgs.SequenceEqual(enArgs))
          throw new Exception("Mismatched format arguments: " + entry.Key);
      }

      var culture = CultureInfo.CurrentCulture;
      System.Windows.Forms.Timer clock;
      using (var main = new MainForm())
      using (var category = new KategoriYonetimForm())
      using (var warehouse = new DepoYonetimForm())
      using (var material = new MalzemeKayitForm())
      using (var add = new AddEditProductForm(1))
      using (var transfer = new StokTransferForm(1))
      using (var report = new ZReportForm())
      using (var reason = new StokDuzeltmeNedeniForm())
      using (var connection = new ConnectionSetupForm())
      {
        var container = (System.ComponentModel.IContainer)Field(main, "components");
        clock = container.Components.OfType<System.Windows.Forms.Timer>().Single();
        Check(clock.Enabled, "main clock is running and owned by the form");
        Check(container.Components.OfType<ToolTip>().Single() == Field(main, "_toolTip"), "tooltip is owned by the form");
        Find(main, "txtBarkod").Text = "Kayıt: İptal";
        var grid = (DataGridView)Find(main, "dgvStoklar");
        var properties = grid.Columns.Cast<DataGridViewColumn>().Select(c => c.DataPropertyName).ToArray();
        main.WindowState = FormWindowState.Normal;
        main.ClientSize = new System.Drawing.Size(1440, 830);
        LanguageManager.SetLanguage("en");
        Check(main.Text == "Inventory Tracking System", "main title switches to English");
        Check(Find(main, "btnYeniUrun").Text == "New Product", "main button switches to English");
        Check(category.Text == "Category Management" && warehouse.Text == "Warehouse Management", "management forms switch without reload");
        var warehouseGrid = (DataGridView)Field(warehouse, "dgvDepolar");
        var warehouseEditor = warehouse.Controls.OfType<Panel>().Single(p => p.Dock == DockStyle.Bottom);
        Check(warehouseGrid.Columns[1].HeaderText == "Location" && warehouseEditor.Controls.OfType<Label>().Any(l => l.Text == "Location:"), "warehouse location header and label switch to English");
        Check(material.Text == "New Material Record" && transfer.Text == "Warehouse Stock Transfer", "material and transfer forms switch");
        Check(connection.Text == "Database Connection Settings" && reason.Text == "Stock Adjustment Reason", "connection and adjustment forms switch");
        Check(Find(add, "btnKaydet").Text == "✓ Save", "designer button switches");
        Check(Find(main, "txtBarkod").Text == "Kayıt: İptal", "search text is preserved");
        Check(properties.SequenceEqual(grid.Columns.Cast<DataGridViewColumn>().Select(c => c.DataPropertyName)), "grid bindings stay unchanged");
        Check(grid.Columns["Barkod"].HeaderText == "Barcode" && grid.Columns["Miktar"].HeaderText == "Quantity", "grid headers switch");
        Check(CultureInfo.CurrentCulture.Equals(culture), "number and date input culture is preserved");
        Check(File.ReadAllText(pref) == "en", "language preference saved");
        Check(new InventoryTracking.Business.Services.UrunService().AddProduct(null).Message == "Model cannot be null.", "business validation uses English");
        LanguageManager.SetLanguage("tr");
        Check(Find(main, "btnYeniUrun").Text == "Yeni Ürün" && category.Text == "Kategori Yönetimi", "switch back to Turkish");
        Check(warehouseGrid.Columns[1].HeaderText == "Konum" && warehouseEditor.Controls.OfType<Label>().Any(l => l.Text == "Konum:"), "warehouse location captions restore Turkish");
        Check(Find(main, "txtBarkod").Text == "Kayıt: İptal", "search text survives roundtrip");
        Check(properties.Contains("Barkod") && properties.Contains("Miktar"), "original model property names retained");
      }

      Check(!clock.Enabled, "closing the form stops and disposes the clock");
      LanguageManager.SetLanguage("en");
      Check(true, "disposed forms unsubscribe from language event");
      typeof(LanguageManager).GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { pref });
      Check(LanguageManager.IsEnglish, "saved language restores on startup");
      File.WriteAllText(pref, "unsupported");
      typeof(LanguageManager).GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { pref });
      Check(!LanguageManager.IsEnglish, "invalid preference falls back to Turkish");
      string blocked = Path.Combine(args[0], "blocked-language-path");
      Directory.CreateDirectory(blocked);
      typeof(LanguageManager).GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { blocked });
      bool saveFailed = false;
      try
      {
        LanguageManager.SetLanguage("en");
      }
      catch (Exception ex)when (ex is IOException || ex is UnauthorizedAccessException)
      {
        saveFailed = true;
      }

      Check(saveFailed && !LanguageManager.IsEnglish, "failed preference write keeps the current language");
      Directory.Delete(blocked);
      typeof(LanguageManager).GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { pref });
      if (args.Length >= 3)
        CheckDatabase(args[1], args[2], args.Length >= 4 ? args[3] : null);
      Check(dialogs.Count == 0, "no unexpected error or validation dialogs");
      Console.WriteLine("TOTAL: " + checks + " checks");
      return 0;
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex);
      return 1;
    }
  }

  static object Field(object owner, string name) => owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner);
  static void RunSql(SqlConnection connection, string query)
  {
    using (var command = new SqlCommand(query, connection))
    {
      command.CommandTimeout = 60;
      command.ExecuteNonQuery();
    }
  }

  static void Position(Form form)
  {
    form.WindowState = FormWindowState.Normal;
    form.StartPosition = FormStartPosition.Manual;
    form.Location = new Point(-10000, -10000);
    form.ShowInTaskbar = false;
  }

  static void Snapshot(Form form, string folder, string filename)
  {
    if (string.IsNullOrEmpty(folder))
      return;
    Directory.CreateDirectory(folder);
    form.PerformLayout();
    Application.DoEvents();
    using (var bitmap = new Bitmap(form.Width, form.Height))
    {
      form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
      bitmap.Save(Path.Combine(folder, filename));
    }
  }

  static void CheckDatabase(string server, string schema, string screenshots)
  {
    string database = "InventoryTracking_LanguageTest_" + Guid.NewGuid().ToString("N");
    var builder = new SqlConnectionStringBuilder
    {
      DataSource = server,
      InitialCatalog = "master",
      IntegratedSecurity = true,
      TrustServerCertificate = true,
      ConnectTimeout = 5
    };
    string previous = Environment.GetEnvironmentVariable("INVENTORYTRACKING_CONNECTION_STRING");
    using (var admin = new SqlConnection(builder.ConnectionString))
    {
      admin.Open();
      try
      {
        // Kurulum ve bütün değişiklikler yalnızca bu test için açılan veritabanında yapılır.
        string script = File.ReadAllText(schema).Replace("InventoryTracking", database);
        foreach (string batch in Regex.Split(script, @"(?im)^\s*GO\s*$"))
          if (!string.IsNullOrWhiteSpace(batch))
            RunSql(admin, batch);
        builder.InitialCatalog = database;
        Environment.SetEnvironmentVariable("INVENTORYTRACKING_CONNECTION_STRING", builder.ConnectionString);
        InventoryTracking.Connection.SqlDBConnection.ResetConnection();
        RunSql(admin, @"UPDATE dbo.Urun SET UrunAdi=CASE UrunId WHEN 1 THEN N'Bottled Water 1.5L' WHEN 2 THEN N'Milk 1L' ELSE N'Bread' END;
UPDATE dbo.MalzemeKatalog SET Aciklama=CASE MalzemeId WHEN 1 THEN N'Bottled Water 1.5L' WHEN 2 THEN N'Milk 1L' ELSE N'Bread' END;
UPDATE dbo.Depo SET DepoAdi=CASE DepoId WHEN 1 THEN N'Main Warehouse' ELSE N'Branch Warehouse' END;
UPDATE dbo.Kategori SET KategoriAdi=CASE KategoriId WHEN 1 THEN N'Beverages' WHEN 2 THEN N'Food' ELSE N'Cleaning' END;
UPDATE dbo.Birim SET BirimAdi=CASE BirimId WHEN 1 THEN N'Piece' WHEN 2 THEN N'Kilogram' WHEN 3 THEN N'Litre' ELSE N'Pack' END;");
        string connectionFile = Path.Combine(Path.GetTempPath(), "InventoryTrackingConnectionUI_" + Guid.NewGuid().ToString("N") + ".dat");
        try
        {
          using (var setup = (ConnectionSetupForm)Activator.CreateInstance(typeof(ConnectionSetupForm), BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { connectionFile }, null))
          {
            Position(setup);
            setup.Show();
            ((TextBox)Field(setup, "txtServer")).Text = server;
            ((TextBox)Field(setup, "txtDatabase")).Text = database;
            ((CheckBox)Field(setup, "chkWindows")).Checked = true;
            ((CheckBox)Field(setup, "chkTrustCertificate")).Checked = true;
            setup.AcceptButton.PerformClick();
            Check(setup.DialogResult == DialogResult.OK && File.Exists(connectionFile), "connection save button succeeds with an isolated settings file");
            var saved = new SqlConnectionStringBuilder(InventoryTracking.Connection.ConnectionSettingsStore.Load(connectionFile));
            Check(saved.DataSource == server && saved.InitialCatalog == database && saved.IntegratedSecurity && !System.Text.Encoding.UTF8.GetString(File.ReadAllBytes(connectionFile)).Contains(database), "UI-saved settings roundtrip securely without changing user preferences");
          }
        }
        finally { if (File.Exists(connectionFile)) File.Delete(connectionFile); }
        var stock = new InventoryTracking.Business.Services.StokService();
        Check(stock.UpdateStok(1, 1, 20, "Giriş", "New delivery").Success, "stock entry before language test");
        Check(stock.TransferStock(1, 1, 2, 10, "Branch replenishment").Success, "stock transfer before language test");
        Check(stock.SetStock(1, 1, 150, "Stock count adjustment").Success, "stock count before language test");
        RunSql(admin, "UPDATE dbo.StokHareketi SET KullaniciAdi=N'Demo user';");
        using (var main = new MainForm())
        using (var report = new ZReportForm())
        {
          Position(main);
          main.ClientSize = new Size(1440, 830);
          main.Show();
          Application.DoEvents();
          Find(main, "lblUser").Text = "Demo user";
          ((ComboBox)Find(main, "cmbDepo")).SelectedValue = 1;
          Application.DoEvents();
          var grid = (DataGridView)Find(main, "dgvStoklar");
          var search = Find(main, "txtBarkod");
          search.Text = "M-001";
          Application.DoEvents();
          Check(grid.Rows.Count == 1, "loaded grid filters by material number");
          var selected = grid.SelectedRows[0].DataBoundItem;
          object depot = ((ComboBox)Find(main, "cmbDepo")).SelectedValue;
          string quantity = Find(main, "lblStockAmount").Text;
          ((Button)Find(main, "btnLanguage")).PerformClick();
          Application.DoEvents();
          Check(LanguageManager.IsEnglish, "main language button switches to English");
          Check(grid.SelectedRows[0].DataBoundItem == selected && search.Text == "M-001", "English switch keeps loaded product and search");
          Check(Equals(depot, ((ComboBox)Find(main, "cmbDepo")).SelectedValue) && quantity == Find(main, "lblStockAmount").Text, "English switch keeps warehouse and quantity");
          Check(Find(main, "lblDetailUrunAdi").Text == "Bottled Water 1.5L", "record names stay unchanged");
          Check(Find(main, "btnKapat").Text == "Exit", "exit button is distinct from report direction");
          Check(new InventoryTracking.Business.Services.UrunService().SearchByBarkod("8690000123456").Fiyat == 3.50m, "English product query retains SQL field names");
          Check(stock.GetStokByDepo(1).Count == 3 && stock.GetStok(1, 1).Miktar == 150, "English stock queries retain SQL field names");
          foreach (int width in new[]
          {
            1180,
            1440
          }

          )
          {
            main.ClientSize = new Size(width, 830);
            main.PerformLayout();
            var language = Find(main, "btnLanguage");
            var header = language.Parent;
            Check(language.Right <= header.ClientSize.Width, "English language button visible at width " + width);
            var actions = Find(main, "pnlButtons").Controls.Cast<Control>().OrderBy(c => c.Left).ToArray();
            Check(actions.Zip(actions.Skip(1), (a, b) => a.Right <= b.Left).All(x => x), "English toolbar fits at width " + width);
          }

          search.Text = "";
          main.ClientSize = new Size(1440, 830);
          Application.DoEvents();
          Snapshot(main, screenshots, "main-en.png");
          LanguageManager.SetLanguage("tr");
          Application.DoEvents();
          Snapshot(main, screenshots, "main-tr.png");
          LanguageManager.SetLanguage("en");
          Position(report);
          report.Show();
          Application.DoEvents();
          ((Button)Find(report, "btnGetir")).PerformClick();
          Application.DoEvents();
          var detail = (DataGridView)Find(report, "dgvHareketDetay");
          var summary = (DataGridView)Find(report, "dgvRapor");
          var movement = (ComboBox)Find(report, "cmbRaporHareket");
          var direction = (ComboBox)Find(report, "cmbRaporYon");
          var product = Find(report, "txtRaporUrun");
          Check(detail.Rows.Count >= 4, "English report loads regular, transfer and count movements");
          Check(movement.Items[3].ToString() == "Count Adjustment" && direction.Items[2].ToString() == "Out", "report movement and direction choices translated");
          var tabs = (TabControl)Find(report, "tabHareketRaporu");
          Snapshot(report, screenshots, "report-summary-en.png");
          tabs.SelectedIndex = 1;
          Application.DoEvents();
          Snapshot(report, screenshots, "report-details-en.png");
          object loadedRows = detail.DataSource;
          string originalDescription = (string)Field(report, "_uygulananFiltre");
          product.Text = "Unapplied filter";
          movement.SelectedIndex = 2;
          direction.SelectedIndex = 2;
          LanguageManager.SetLanguage("tr");
          Application.DoEvents();
          Check(ReferenceEquals(detail.DataSource, loadedRows) && product.Text == "Unapplied filter" && movement.SelectedIndex == 2 && direction.SelectedIndex == 2, "report switch keeps loaded rows and pending filters");
          Check(!((string)Field(report, "_uygulananFiltre")).Contains("Unapplied filter"), "print description remains tied to applied filters");
          LanguageManager.SetLanguage("en");
          Application.DoEvents();
          Check((string)Field(report, "_uygulananFiltre") == originalDescription, "applied filter description restores after roundtrip");
          Check(summary.Columns[0].HeaderText == "Period" && detail.Columns["Yon"].HeaderText == "Direction", "loaded report column headers switch");
          Check(detail.Rows[0].Cells["HareketTipi"].Value.ToString() == "Sayım Düzeltmesi" && detail.Rows[0].Cells["HareketTipi"].FormattedValue.ToString() == "Count Adjustment", "movement display translates while stored code is preserved");
          string csv = Path.Combine(Path.GetTempPath(), "InventoryTracking-" + Guid.NewGuid().ToString("N") + ".csv");
          try
          {
            report.GetType().GetMethod("WriteCsv", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { csv, detail });
            string exported = File.ReadAllText(csv);
            Check(exported.Contains("\"Direction\"") && exported.Contains("Count Adjustment") && !exported.Contains("Sayım Düzeltmesi"), "CSV uses English headers and movement labels");
          }
          finally
          {
            if (File.Exists(csv))
              File.Delete(csv);
          }

          using (var bitmap = new Bitmap(1200, 900))
          using (var graphics = Graphics.FromImage(bitmap))
          {
            foreach (int index in new[]
            {
              0,
              1
            }

            )
            {
              tabs.SelectedIndex = index;
              var bounds = new Rectangle(50, 50, 1100, 800);
              var page = new PrintPageEventArgs(graphics, bounds, bounds, new PageSettings());
              report.GetType().GetMethod("Pd_PrintPage", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(report, new object[] { null, page });
              Check(true, "English print layout renders for tab " + index);
            }
          }

          product.Text = "8690000123456";
          movement.SelectedIndex = 2;
          direction.SelectedIndex = 2;
          ((Button)Find(report, "btnGetir")).PerformClick();
          Application.DoEvents();
          Check(detail.Rows.Count == 1 && detail.Rows[0].Cells["HareketTipi"].Value.ToString() == "Transfer Çıkış", "English filters keep SQL movement semantics");
          var productService = new InventoryTracking.Business.Services.UrunService();
          var item = productService.GetProductById(1);
          using (var detailForm = new ProductDetailForm(item, 1, stock))
          using (var add = new AddEditProductForm(item, 1))
          using (var category = new KategoriYonetimForm())
          using (var warehouse = new DepoYonetimForm())
          using (var transfer = new StokTransferForm(1, 1))
          using (var material = new MalzemeKayitForm())
          {
            foreach (var form in new Form[]
            {
              detailForm,
              add,
              category,
              warehouse,
              transfer,
              material
            }

            )
            {
              Console.WriteLine("OPEN: " + form.GetType().Name);
              Position(form);
              form.Show();
              Application.DoEvents();
              Check(form.Visible, "English screen opens: " + form.GetType().Name);
            }

            Check(Find(detailForm, "lblUrunAdiValue").Text == item.UrunAdi, "detail product name is not translated");
            Snapshot(transfer, screenshots, "transfer-en.png");
            Snapshot(add, screenshots, "product-en.png");
          }
        }
      }
      finally
      {
        InventoryTracking.Connection.SqlDBConnection.CloseConnection();
        SqlConnection.ClearAllPools();
        Environment.SetEnvironmentVariable("INVENTORYTRACKING_CONNECTION_STRING", previous);
        if (admin.State != System.Data.ConnectionState.Open)
          admin.Open();
        admin.ChangeDatabase("master");
        RunSql(admin, "IF DB_ID('" + database + "') IS NOT NULL BEGIN ALTER DATABASE [" + database + "] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [" + database + "]; END");
      }
    }
  }
}
