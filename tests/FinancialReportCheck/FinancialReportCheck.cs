using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;
using InventoryTracking.Business;
using InventoryTracking.Connection;
using InventoryTracking.UI.Localization;
using InventoryTracking.Models;
using InventoryTracking.UI.Forms;

class FinancialReportCheck
{
  static int checks;
  static SqlConnection admin;
  static string database;
  static readonly XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
  static readonly List<string> messages = new List<string>();
  static void Check(bool condition, string label)
  { if (!condition) throw new Exception("FAILED: " + label); checks++; Console.WriteLine("PASS: " + label); }
  static object Sql(string query)
  { using (var command = new SqlCommand(query, admin)) { command.CommandTimeout = 60; return command.ExecuteScalar(); } }
  static object Field(object target, string name) => target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(target);
  static object Invoke(object target, string name, params object[] args) => target.GetType().GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(target, args);
  static Control Find(Control form, string name) => form.Controls.Find(name, true).Single();
  static void Invalid(Action call, string label)
  { bool rejected = false; try { call(); } catch (ArgumentException) { rejected = true; } Check(rejected, label); }
  static void Record(int product, int depot, int quantity, bool returned, DateTime date, decimal price)
  {
    Sql("UPDATE dbo.Urun SET Fiyat=" + price.ToString(System.Globalization.CultureInfo.InvariantCulture) + " WHERE UrunId=" + product);
    var result = new SalesReturnService().Kaydet(Guid.NewGuid(), product, depot, quantity, returned);
    Check(result.Success, "fixture sale/return commits");
    using (var command = new SqlCommand(@"UPDATE dbo.StokHareketi SET HareketTarihi=@Tarih WHERE HareketId=(SELECT HareketId FROM dbo.SatisIade WHERE IslemId=@Id);
      UPDATE dbo.SatisIade SET IslemTarihi=@Tarih WHERE IslemId=@Id;", admin))
    {
      command.Parameters.Add("@Tarih", SqlDbType.DateTime2).Value = date;
      command.Parameters.Add("@Id", SqlDbType.BigInt).Value = Convert.ToInt64(result.Data);
      command.ExecuteNonQuery();
    }
  }
  static void Snapshot(Form form, string output, string name)
  {
    form.PerformLayout(); Application.DoEvents();
    using (var bitmap = new Bitmap(form.Width, form.Height))
    { form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size)); bitmap.Save(Path.Combine(output, name)); }
  }
  static XDocument Sheet(string file)
  {
    using (var archive = ZipFile.OpenRead(file))
    {
      Check(archive.GetEntry("[Content_Types].xml") != null && archive.GetEntry("xl/styles.xml") != null, "xlsx is an Open XML package");
      foreach (var entry in archive.Entries) using (var stream = entry.Open()) XDocument.Load(stream);
      using (var stream = archive.GetEntry("xl/worksheets/sheet1.xml").Open()) return XDocument.Load(stream);
    }
  }
  static string Text(XDocument sheet, string cell) => sheet.Descendants(ns + "c").Single(c => (string)c.Attribute("r") == cell).Value;
  static void CheckSheet(string path, int dataRows, string title)
  {
    var sheet = Sheet(path);
    Check(sheet.Root.Name == ns + "worksheet" && sheet.Descendants(ns + "row").Count() == dataRows + 4, "xlsx includes title, filters, totals, headers and all data rows");
    Check(Text(sheet, "A1") == title, "xlsx report title follows selected tab and language");
    Check(sheet.Descendants(ns + "pane").Single().Attribute("ySplit").Value == "4", "xlsx freezes report header rows");
    Check(sheet.Descendants(ns + "autoFilter").Any(), "xlsx includes column filters");
    Check(!sheet.Descendants(ns + "f").Any(), "exported user text is not an Excel formula");
  }
  [STAThread]
  static int Main(string[] args)
  {
    string previous = Environment.GetEnvironmentVariable("INVENTORYTRACKING_CONNECTION_STRING");
    string qa = "InventoryTracking_FinanceTest_" + Guid.NewGuid().ToString("N");
    try
    {
      Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
      Directory.CreateDirectory(args[2]);
      var builder = new SqlConnectionStringBuilder { DataSource = args[0], InitialCatalog = "master", IntegratedSecurity = true, TrustServerCertificate = true };
      admin = new SqlConnection(builder.ConnectionString); admin.Open();
      Sql("CREATE DATABASE [" + qa + "]"); database = qa; admin.ChangeDatabase(qa);
      string schema = File.ReadAllText(args[1]).Replace("InventoryTracking", qa);
      foreach (string batch in Regex.Split(schema, @"(?im)^\s*GO\s*(?:--[^\r\n]*)?$")) if (!string.IsNullOrWhiteSpace(batch)) Sql(batch);
      builder.InitialCatalog = qa; Environment.SetEnvironmentVariable("INVENTORYTRACKING_CONNECTION_STRING", builder.ConnectionString);
      SqlDatabaseConnection.ResetConnection();
      string preference = Path.Combine(args[2], "language.txt"); File.WriteAllText(preference, "tr");
      typeof(LanguageManager).GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { preference });
      var handler = new Func<string, string, MessageBoxButtons, MessageBoxIcon, DialogResult>((text, title, buttons, icon) =>
      { messages.Add(text); return DialogResult.OK; });
      typeof(LanguageManager).Assembly.GetType("InventoryTracking.UI.Localization.UiMessage").GetProperty("Handler", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, handler);

      DateTime today = DateTime.Today, start = SalesReturnReportService.SonBirYilBaslangici(today);
      DateTime middle = today.AddMonths(-3), end = today.AddDays(1).AddMilliseconds(-1);
      Sql("UPDATE dbo.Stok SET Miktar=1000; UPDATE dbo.Urun SET UrunAdi=N'Finance Test Product' WHERE UrunId=1;");
      string barcode = (string)Sql("SELECT Barkod FROM dbo.Urun WHERE UrunId=1");
      Record(1, 1, 10, false, start, 100m);
      Record(1, 1, 2, true, start.AddDays(1), 100m);
      Record(1, 1, 3, false, middle, 120m);
      Record(1, 1, 1, true, end, 120m);
      Record(1, 2, 4, false, today, 120m);
      Record(2, 1, 5, false, today, 20m);
      Record(1, 1, 7, false, start.AddSeconds(-1), 900m);
      Record(1, 1, 9, false, today.AddDays(1), 900m);
      Sql("UPDATE dbo.Urun SET Fiyat=999, UrunAdi=N'Current Different Name' WHERE UrunId=1");
      var stockService = new InventoryTracking.Business.StockService();
      Check(stockService.UpdateStok(1,1,5,"Sayım Düzeltmesi","Test count adjustment").Success, "fixture count adjustment commits");
      Check(stockService.TransferStock(1,1,2,6,"Test warehouse transfer").Success, "fixture warehouse transfer commits");
      Check(stockService.UpdateStok(1,1,7,"Giriş","Test stock receipt").Success, "fixture non-sale stock receipt commits");

      var filter = new MovementReportFilter { Baslangic=start, Bitis=today, DepoId=1, UrunArama=barcode };
      var service = new SalesReturnReportService();
      var days = service.GetOzet(filter, "Günlük");
      Check(days.Count == 4 && days.Sum(x => x.SatisAdedi) == 13 && days.Sum(x => x.IadeAdedi) == 3, "daily report separates sales and returns and includes full boundary days");
      Check(days.Sum(x => x.SatisTutari) == 1360m && days.Sum(x => x.IadeTutari) == 320m && days.Sum(x => x.NetSatisTutari) == 1040m, "financial totals use stored prices instead of current product prices");
      Check(days.Sum(x => x.NetAdet) == 10 && days.Sum(x => x.IslemSayisi) == 4, "net quantity and transaction count reconcile");
      Check(days.First().NetSatisTutari == -120m, "return-only periods keep negative net sales");
      var months = service.GetOzet(filter, "Aylık");
      Check(months.Count == new[] { start, start.AddDays(1), middle, end }.Select(d => d.ToString("MM.yyyy")).Distinct().Count() && months.Sum(x => x.NetSatisTutari) == 1040m, "monthly grouping reconciles to daily totals");
      var years = service.GetOzet(filter, "Yıllık");
      Check(years.Count == new[] { start, start.AddDays(1), middle, end }.Select(d => d.Year).Distinct().Count() && years.Sum(x => x.NetSatisTutari) == 1040m, "yearly grouping reconciles to daily totals");
      filter.HareketGrubu=2; filter.CikisMi=true;
      Check(service.GetOzet(filter, "Günlük").Sum(x => x.NetSatisTutari) == 1040m, "stock movement type and direction do not alter financial totals");
      Check(Convert.ToInt32(Sql("SELECT COUNT(*) FROM dbo.StokHareketi")) > Convert.ToInt32(Sql("SELECT COUNT(*) FROM dbo.SatisIade")), "non-financial stock movements exist but are excluded");
      filter.DepoId=null;
      Check(service.GetOzet(filter, "Yıllık").Sum(x => x.SatisTutari) == 1840m, "all-warehouse filter includes the second warehouse");
      filter.UrunArama="";
      Check(service.GetOzet(filter, "Yıllık").Sum(x => x.SatisTutari) == 1940m, "all-product filter includes other products");
      filter.DepoId=1; filter.UrunArama="Finance Test Product";
      Check(service.GetOzet(filter, "Yıllık").Sum(x => x.NetSatisTutari) == 1040m, "product search uses transaction name snapshots after product rename");
      filter.UrunArama="' OR 1=1 --";
      Check(service.GetOzet(filter, "Günlük").Count == 0, "SQL-like product search remains a literal parameter");
      Invalid(() => service.GetOzet(null, "Günlük"), "null report filter rejected");
      Invalid(() => service.GetOzet(new MovementReportFilter { Baslangic=today, Bitis=today.AddDays(-1) }, "Günlük"), "reversed date range rejected");
      Invalid(() => service.GetOzet(new MovementReportFilter { Baslangic=today, Bitis=DateTime.MaxValue }, "Günlük"), "unrepresentable end date rejected");
      Invalid(() => service.GetOzet(filter, "unexpected"), "unsupported period rejected");
      Check(SalesReturnReportService.SonBirYilBaslangici(new DateTime(2024,2,29)) == new DateTime(2023,3,1), "last-year shortcut includes leap day without an extra boundary day");

      using (var report = new ZReportForm())
      {
        report.StartPosition=FormStartPosition.Manual; report.Location=new Point(-10000,-10000); report.ShowInTaskbar=false;
        report.Show(); Application.DoEvents();
        var tabs=(TabControl)Find(report,"tabHareketRaporu"); tabs.SelectedIndex=2;
        var input=(TextBox)Find(report,"txtRaporUrun"); input.Text=barcode;
        ((ComboBox)Find(report,"cmbRaporDepo")).SelectedIndex=1;
        var movement=(ComboBox)Find(report,"cmbRaporHareket"); var direction=(ComboBox)Find(report,"cmbRaporYon");
        Check(tabs.TabCount == 3 && !movement.Enabled && !direction.Enabled, "financial tab exists and irrelevant stock filters are disabled");
        ((Button)Find(report,"btnSonBirYil")).PerformClick(); Application.DoEvents();
        Check(((DateTimePicker)Find(report,"dtpBaslangic")).Value.Date == start && ((DateTimePicker)Find(report,"dtpBitis")).Value.Date == today, "last-year button selects full calendar-year range");
        var grid=(DataGridView)Find(report,"dgvSatisIadeRapor");
        Check(grid.Rows.Count==4 && grid.Columns.Count==8, "last-year button loads financial rows immediately");
        Check(((Label)Find(report,"lblSatisIadeToplam")).Text.Contains("1.040") || ((Label)Find(report,"lblSatisIadeToplam")).Text.Contains("1,040"), "financial footer includes net sales total");
        int rowCount=grid.Rows.Count;
        object loadedRows=grid.DataSource;
        foreach (string language in new[] { "tr", "en" })
        {
          LanguageManager.SetLanguage(language); Application.DoEvents();
          Check(grid.Rows.Count==rowCount && ReferenceEquals(loadedRows,grid.DataSource), "language switch preserves loaded financial rows without re-query: "+language);
          Check(grid.Columns["SatisTutari"].HeaderText==(language=="tr"?"Satış Tutarı (TRY)":"Sales Amount (TRY)"), "financial columns translated: "+language);
          var buttons=Find(report,"pnlButtons").Controls.OfType<Button>().OrderBy(b=>b.Left).ToArray();
          Check(buttons.Zip(buttons.Skip(1),(a,b)=>a.Right<=b.Left).All(x=>x), "export buttons do not overlap: "+language);
          string excel=Path.Combine(args[2],"financial-"+language+".xlsx");
          Invoke(report,"WriteExcel",excel); CheckSheet(excel,grid.Rows.Count,language=="tr"?"SATIŞ / İADE RAPORU":"SALES / RETURNS REPORT");
          var sheet=Sheet(excel);
          Check(sheet.Descendants(ns+"c").Where(c=>((string)c.Attribute("r")).StartsWith("E") && (string)c.Attribute("r")!="E4").All(c=>c.Attribute("t")==null), "money cells remain numeric: "+language);
          string csv=Path.Combine(args[2],"financial-"+language+".csv");
          report.GetType().GetMethod("WriteCsv",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{csv,grid});
          Check(File.ReadAllLines(csv).Length==5 && File.ReadAllText(csv).Contains(grid.Columns["SatisTutari"].HeaderText),"CSV export uses selected financial rows and localized headings: "+language);
          string description=(string)Invoke(report,"FinancialFilterDescription");
          input.Text="Unapplied filter";
          Check((string)Invoke(report,"FinancialFilterDescription")==description,"financial export keeps applied filters, not unsubmitted edits");
          input.Text=barcode;
          Snapshot(report,args[2],"financial-"+language+".png");
        }
        LanguageManager.SetLanguage("tr"); Application.DoEvents();
        foreach(string period in new[]{"rbAylik","rbYillik"})
        {
          ((RadioButton)Find(report,period)).Checked=true;
          ((Button)Find(report,"btnGetir")).PerformClick(); Application.DoEvents();
          Check(((List<SalesReturnReportSummary>)grid.DataSource).Sum(x=>x.NetSatisTutari)==1040m,"UI period selection preserves financial totals: "+period);
        }
        ((RadioButton)Find(report,"rbGunluk")).Checked=true;
        ((Button)Find(report,"btnGetir")).PerformClick(); Application.DoEvents();
        using (var bitmap=new Bitmap(1200,900))
        using (var graphics=Graphics.FromImage(bitmap))
        {
          int pages=0; bool more;
          do
          {
            var bounds=new Rectangle(50,50,1100,230);
            var page=new PrintPageEventArgs(graphics,bounds,bounds,new PageSettings());
            Invoke(report,"Pd_PrintPage",null,page); more=page.HasMorePages; pages++;
            if(pages>10) throw new Exception("Financial PDF pagination does not finish.");
          } while(more);
          Check(pages>1,"financial print layout paginates and finishes including totals");
        }
        using (var document=new PrintDocument())
        {
          document.PrinterSettings.PrinterName="Microsoft Print to PDF";
          if(document.PrinterSettings.IsValid)
          {
            string pdf=Path.Combine(args[2],"financial-report.pdf");
            document.PrinterSettings.PrintToFile=true; document.PrinterSettings.PrintFileName=pdf;
            document.DefaultPageSettings.Landscape=true; document.PrintController=new StandardPrintController();
            document.PrintPage+=(sender,e)=>Invoke(report,"Pd_PrintPage",sender,e); document.Print();
            for(int attempt=0;attempt<30 && (!File.Exists(pdf)||new FileInfo(pdf).Length<100);attempt++) Thread.Sleep(100);
            Check(File.Exists(pdf)&&new FileInfo(pdf).Length>100,"actual Microsoft Print to PDF creates financial report");
          }
          else Console.WriteLine("SKIP: Microsoft Print to PDF not installed; graphics pagination verified.");
        }
        tabs.SelectedIndex=0; Application.DoEvents();
        Check(movement.Enabled && direction.Enabled,"stock filters restored when leaving financial tab");
        Invoke(report,"WriteExcel",Path.Combine(args[2],"stock-summary.xlsx"));
        CheckSheet(Path.Combine(args[2],"stock-summary.xlsx"),((DataGridView)Find(report,"dgvRapor")).Rows.Count,"STOK HAREKET RAPORU - ÖZET");
        tabs.SelectedIndex=1; Application.DoEvents();
        Invoke(report,"WriteExcel",Path.Combine(args[2],"stock-details.xlsx"));
        CheckSheet(Path.Combine(args[2],"stock-details.xlsx"),((DataGridView)Find(report,"dgvHareketDetay")).Rows.Count,"STOK HAREKET RAPORU - HAREKET DETAYI");
        // Aynı adla yeniden kaydetmek geçici ZIP'i hedefe alır; eski rapor bozulmaz.
        Invoke(report,"WriteExcel",Path.Combine(args[2],"stock-details.xlsx"));
        Check(Directory.GetFiles(args[2],".report-*.tmp").Length==0,"successful overwrite leaves no temporary workbook");
        tabs.SelectedIndex=2;
        ((DateTimePicker)Find(report,"dtpBaslangic")).Value=today;
        ((DateTimePicker)Find(report,"dtpBitis")).Value=today.AddDays(-1);
        ((Button)Find(report,"btnGetir")).PerformClick();
        Check(grid.DataSource==null && !(bool)Field(report,"_raporGetirildi"),"invalid UI dates clear old report and disable export state");
      }
      Console.WriteLine("TOTAL: "+checks+" checks"); return 0;
    }
    catch(Exception ex) { Console.WriteLine(ex); return 1; }
    finally
    {
      SqlDatabaseConnection.CloseConnection(); SqlConnection.ClearAllPools();
      Environment.SetEnvironmentVariable("INVENTORYTRACKING_CONNECTION_STRING",previous);
      if(admin!=null)
      {
        if(admin.State!=ConnectionState.Open) admin.Open(); admin.ChangeDatabase("master");
        if(database!=null) Sql("ALTER DATABASE ["+database+"] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE ["+database+"]");
        admin.Dispose();
      }
      if(args.Length>2 && File.Exists(Path.Combine(args[2],"language.txt"))) File.Delete(Path.Combine(args[2],"language.txt"));
    }
  }
}
