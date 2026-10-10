using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using InventoryTracking.Business;
using InventoryTracking.Connection;
using InventoryTracking.DataAccess;
using InventoryTracking.UI.Localization;
using InventoryTracking.UI.Forms;

class SalesReturnCheck
{
  static int checks, questions;
  static SqlConnection admin;
  static string db;
  static object Sql(string query)
  {
    using (var command=new SqlCommand(query,admin)) { command.CommandTimeout=60; return command.ExecuteScalar(); }
  }
  static int Count(string table) => Convert.ToInt32(Sql("SELECT COUNT(*) FROM dbo."+table));
  static void Check(bool ok,string label) { if(!ok) throw new Exception("FAILED: "+label); checks++; Console.WriteLine("PASS: "+label); }
  static Control Find(Control form,string name) => form.Controls.Find(name,true).Single();
  static void Scan(SalesReturnForm form,string barcode)
  {
    foreach(char key in barcode) { var message=Message.Create(form.Handle,0x0102,(IntPtr)key,IntPtr.Zero); form.PreFilterMessage(ref message); }
    var enter=Message.Create(form.Handle,0x0100,(IntPtr)13,IntPtr.Zero); form.PreFilterMessage(ref enter);
  }
  static void CheckCreationFlow(MainForm main,string barcode,Type expected)
  {
    bool opened=false; int ticks=0;
    var input=(TextBox)Find(main,"txtBarkod"); input.Text=barcode;
    using(var timer=new Timer { Interval=60 })
    {
      timer.Tick+=(s,e)=>
      {
        ticks++;
        var child=Application.OpenForms.Cast<Form>().FirstOrDefault(x=>x!=main);
        if(child!=null) { opened=child.GetType()==expected;timer.Stop();child.DialogResult=DialogResult.Cancel;child.Close(); }
        if(ticks>30) { timer.Stop();throw new Exception("Missing barcode dialog: "+expected.Name); }
      };
      timer.Start();
      typeof(MainForm).GetMethod("txtBarkod_KeyPress",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(main,new object[]{input,new KeyPressEventArgs('\r')});
    }
    Check(opened,"unregistered barcode opens "+expected.Name);
  }
  [STAThread]
  static int Main(string[] args)
  {
    string previous=Environment.GetEnvironmentVariable("INVENTORYTRACKING_CONNECTION_STRING");
    string qa="InventoryTracking_SalesTest_"+Guid.NewGuid().ToString("N");
    try
    {
      Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
      var builder=new SqlConnectionStringBuilder { DataSource=args[0],InitialCatalog="master",IntegratedSecurity=true,TrustServerCertificate=true };
      admin=new SqlConnection(builder.ConnectionString); admin.Open();
      Sql("CREATE DATABASE ["+qa+"]"); db=qa; admin.ChangeDatabase(qa);
      string schema=File.ReadAllText(args[1]).Replace("InventoryTracking",qa);
      foreach(var batch in Regex.Split(schema,@"(?im)^\s*GO\s*(?:--[^\r\n]*)?$")) if(!string.IsNullOrWhiteSpace(batch)) Sql(batch);
      builder.InitialCatalog=qa; Environment.SetEnvironmentVariable("INVENTORYTRACKING_CONNECTION_STRING",builder.ConnectionString);
      SqlDatabaseConnection.ResetConnection();
      string preference=Path.Combine(args[3],"language.txt"); Directory.CreateDirectory(args[3]);
      File.WriteAllText(preference,"tr");
      typeof(LanguageManager).GetMethod("Initialize",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{preference});
      var handler=new Func<string,string,MessageBoxButtons,MessageBoxIcon,DialogResult>((text,title,buttons,icon)=>
      {
        if(buttons==MessageBoxButtons.YesNo) questions++;
        Console.WriteLine("DIALOG: "+title+": "+text);
        return buttons==MessageBoxButtons.YesNo?DialogResult.No:DialogResult.OK;
      });
      typeof(LanguageManager).Assembly.GetType("InventoryTracking.UI.Localization.UiMessage").GetProperty("Handler",BindingFlags.NonPublic|BindingFlags.Static).SetValue(null,handler);
      int stocks=Count("Stok"),movements=Count("StokHareketi");
      string migration=File.ReadAllText(args[2]); Sql(migration); Sql(migration);
      Check(Count("Stok")==stocks && Count("StokHareketi")==movements,"migration repeat preserves old stock and movements");
      var service=new SalesReturnService();
      int product=Convert.ToInt32(Sql("SELECT TOP 1 UrunId FROM dbo.Stok WHERE DepoId=1 ORDER BY UrunId"));
      int initial=Convert.ToInt32(Sql("SELECT Miktar FROM dbo.Stok WHERE DepoId=1 AND UrunId="+product));
      var request=Guid.NewGuid();
      Check(service.Kaydet(request,product,1,1,false).Success,"one barcode sells one item");
      Check(Convert.ToInt32(Sql("SELECT Miktar FROM dbo.Stok WHERE DepoId=1 AND UrunId="+product))==initial-1,"sale reduces selected warehouse stock");
      Check(Count("SatisIade")==1,"sale creates one transaction");
      Check(service.Kaydet(request,product,1,1,false).Success && Count("SatisIade")==1,"same request retry does not duplicate sale");
      Check(!service.Kaydet(request,product,1,2,false).Success && Count("SatisIade")==1,"same request with changed quantity is rejected");
      Check(service.Kaydet(Guid.NewGuid(),product,1,3,true).Success,"return is recorded");
      Check(Convert.ToInt32(Sql("SELECT Miktar FROM dbo.Stok WHERE DepoId=1 AND UrunId="+product))==initial+2,"return increases stock by selected quantity");
      int before=Count("SatisIade"),beforeMovement=Count("StokHareketi");
      Check(!service.Kaydet(Guid.NewGuid(),product,1,1000000,false).Success,"overselling rejected");
      Check(Count("SatisIade")==before && Count("StokHareketi")==beforeMovement,"failed sale leaves no transaction or movement");
      Check(Convert.ToInt32(Sql("SELECT COUNT(*) FROM dbo.IslemGunlugu WHERE TabloAdi=N'SatisIade' AND Basarili=0"))>=2,"rejected attempts are logged separately");
      var first=service.GetHistory(1,product).Last();
      Check(first.Miktar==1 && first.KullaniciAdi==Environment.UserName && first.IslemTarihi>DateTime.Now.AddMinutes(-10),"history contains date, quantity and acting user");
      Check(first.IslemTuru=="Satış" && service.GetHistory(1,product).First().IslemTuru=="İade","history distinguishes sales and returns");
      Sql("UPDATE dbo.Urun SET UrunAdi=N'QA Changed Product',Fiyat=999 WHERE UrunId="+product);
      var saved=service.GetHistory(1,product).Last();
      Check(saved.UrunAdi==first.UrunAdi && saved.BirimFiyat==first.BirimFiyat,"history preserves original product name and price");
      Check(saved.ToplamTutar==saved.Miktar*saved.BirimFiyat,"transaction amount uses its saved price");
      Sql("UPDATE dbo.Urun SET AktifMi=0 WHERE UrunId="+product);
      Check(!service.Kaydet(Guid.NewGuid(),product,1,1,false).Success,"inactive product cannot be sold");
      Sql("UPDATE dbo.Urun SET AktifMi=1 WHERE UrunId="+product);
      Sql("UPDATE dbo.Depo SET AktifMi=0 WHERE DepoId=1");
      Check(!service.Kaydet(Guid.NewGuid(),product,1,1,true).Success,"inactive warehouse cannot receive returns");
      Sql("UPDATE dbo.Depo SET AktifMi=1 WHERE DepoId=1");
      int newDepot=Convert.ToInt32(Sql("INSERT dbo.Depo(DepoAdi,AktifMi) VALUES(N'QA New Warehouse',1); SELECT SCOPE_IDENTITY();"));
      Check(service.Kaydet(Guid.NewGuid(),product,newDepot,2,true).Success,"return can create missing warehouse stock");
      Check(Convert.ToInt32(Sql("SELECT Miktar FROM dbo.Stok WHERE UrunId="+product+" AND DepoId="+newDepot))==2,"new warehouse return starts from zero");
      before=Count("SatisIade");beforeMovement=Count("StokHareketi");
      int amountBefore=Convert.ToInt32(Sql("SELECT Miktar FROM dbo.Stok WHERE DepoId=1 AND UrunId="+product));
      Sql("ALTER TABLE dbo.SatisIade WITH NOCHECK ADD CONSTRAINT CK_QA_Fail CHECK(IslemId<0)");
      Check(!service.Kaydet(Guid.NewGuid(),product,1,1,false).Success,"transaction insertion failure is reported");
      Sql("ALTER TABLE dbo.SatisIade DROP CONSTRAINT CK_QA_Fail");
      Check(Count("SatisIade")==before && Count("StokHareketi")==beforeMovement && Convert.ToInt32(Sql("SELECT Miktar FROM dbo.Stok WHERE DepoId=1 AND UrunId="+product))==amountBefore,"database failure rolls back stock, movement and sale together");

      Sql("UPDATE dbo.Stok SET Miktar=1 WHERE DepoId=1 AND UrunId="+product);
      before=Count("SatisIade");
      var results=Enumerable.Range(0,6).Select(x=>Task.Run(()=>new SalesReturnService().Kaydet(Guid.NewGuid(),product,1,1,false))).ToArray();
      Task.WaitAll(results);
      Check(results.Count(x=>x.Result.Success)==1 && Count("SatisIade")==before+1,"concurrent sales cannot sell the last item twice");
      Check(Convert.ToInt32(Sql("SELECT Miktar FROM dbo.Stok WHERE DepoId=1 AND UrunId="+product))==0,"concurrent sales leave zero, never negative stock");
      Sql("UPDATE dbo.Stok SET Miktar=10 WHERE DepoId=1 AND UrunId="+product);
      before=Count("SatisIade");request=Guid.NewGuid();
      results=Enumerable.Range(0,4).Select(x=>Task.Run(()=>new SalesReturnService().Kaydet(request,product,1,1,false))).ToArray();
      Task.WaitAll(results);
      Check(results.All(x=>x.Result.Success) && Count("SatisIade")==before+1,"concurrent retries are idempotent");
      using(var connection=SqlDatabaseConnection.OpenNewConnection())
      using(var command=new SqlCommand("UPDATE dbo.Kategori SET Aciklama=N'QA Bulk Log' WHERE KategoriId IN(1,2)",connection)) command.ExecuteNonQuery();
      Check(Convert.ToInt32(Sql("SELECT COUNT(*) FROM dbo.IslemGunlugu WHERE TabloAdi=N'Kategori' AND YeniVeri LIKE N'%QA Bulk Log%' AND KullaniciAdi=N'"+Environment.UserName.Replace("'","''")+"'"))==2,"multi-row changes log each row and the user");
      Check(Convert.ToInt32(Sql("SELECT COUNT(*) FROM dbo.IslemGunlugu WHERE TabloAdi=N'Urun' AND OncekiVeri IS NOT NULL AND YeniVeri IS NOT NULL"))>=3,"product edits capture before and after values");
      Sql("CREATE USER SalesQAUser WITHOUT LOGIN; ALTER ROLE InventoryTrackingApp ADD MEMBER SalesQAUser;");
      Sql("EXECUTE AS USER=N'SalesQAUser'; EXEC sys.sp_set_session_context @key=N'KullaniciAdi',@value=N'QA Limited User';");
      try
      {
        Check(Convert.ToInt32(Sql("SELECT HAS_PERMS_BY_NAME(N'dbo.SatisIade',N'OBJECT',N'UPDATE')"))==0,"application role cannot rewrite sale history");
        Check(Convert.ToInt32(Sql("SELECT HAS_PERMS_BY_NAME(N'dbo.IslemGunlugu',N'OBJECT',N'DELETE')"))==0,"application role cannot delete audit log");
        Sql("EXEC dbo.sp_SatisIadeKaydet @IslemNo='"+Guid.NewGuid()+"',@UrunId="+product+",@DepoId=1,@Miktar=1,@IadeMi=0");
        Check(true,"limited application role can save sale with trigger logging");
      }
      finally { Sql("REVERT; EXEC sys.sp_set_session_context @key=N'KullaniciAdi',@value=NULL;"); }
      var urun=new ProductService().GetProductById(product);
      foreach(string language in new[]{"tr","en"})
      {
        LanguageManager.SetLanguage(language);
        using(var dialog=new SalesReturnForm(urun,1))
        {
          dialog.Show(); Application.DoEvents();
          var quantity=(NumericUpDown)Find(dialog,"numAdet");
          Check(quantity.Value==1,"first scan starts at one: "+language);
          Check(dialog.RegisterBarcode(urun.Barkod) && quantity.Value==2,"repeat barcode increments quantity: "+language);
          Check(!dialog.RegisterBarcode("different") && quantity.Value==2,"different barcode leaves quantity unchanged: "+language);
          Find(dialog,"btnIade").Focus();
          Scan(dialog,urun.Barkod);
          Check(quantity.Value==3,"scanner works while return button focused: "+language);
          quantity.Focus(); Scan(dialog,urun.Barkod);
          Check(quantity.Value==4,"scanner works while quantity focused: "+language);
          Check(Find(dialog,"btnSatis").Text==(language=="tr"?"Satış":"Sale"),"action captions follow language: "+language);
          if(args.Length>4) { using(var bitmap=new Bitmap(dialog.Width,dialog.Height)) { dialog.DrawToBitmap(bitmap,new Rectangle(Point.Empty,dialog.Size)); bitmap.Save(Path.Combine(args[4],"sale-return-"+language+".png")); } }
          dialog.Close();
        }
      }
      LanguageManager.SetLanguage("tr");
      before=Count("SatisIade"); int previousQuestions=questions;
      using(var main=new MainForm())
      {
        main.WindowState=FormWindowState.Normal; main.Size=new Size(1280,800); main.Show(); Application.DoEvents();
        var input=(TextBox)Find(main,"txtBarkod"); input.Text=urun.Barkod;
        Check(Count("SatisIade")==before,"scan without Enter does not change stock");
        using(var timer=new Timer { Interval=80 })
        {
          timer.Tick+=(s,e)=>
          {
            var dialog=Application.OpenForms.OfType<SalesReturnForm>().FirstOrDefault();
            if(dialog==null) return; timer.Stop();
            Scan(dialog,urun.Barkod);
            ((Button)Find(dialog,"btnSatis")).PerformClick();
          };
          timer.Start();
          typeof(MainForm).GetMethod("txtBarkod_KeyPress",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(main,new object[]{input,new KeyPressEventArgs('\r')});
        }
        Check(questions==previousQuestions,"active barcode opens sale/return directly without old question");
        Check(Count("SatisIade")==before+1 && service.GetHistory(1).First().Miktar==2,"modal scan and sale save selected quantity");
        var grid=(DataGridView)Find(main,"dgvSatisIade");
        Check(grid.Rows.Count==service.GetHistory(1).Count,"main history refreshes after transaction");
        Check(input.Text=="","barcode input resets after completed transaction");
        ((TabControl)Find(main,"tabWorkspace")).SelectedIndex=1; Application.DoEvents();
        if(args.Length>4) { using(var bitmap=new Bitmap(main.Width,main.Height)) { main.DrawToBitmap(bitmap,new Rectangle(Point.Empty,main.Size)); bitmap.Save(Path.Combine(args[4],"sale-history-tr.png")); } }
        CheckCreationFlow(main,"QA-NOT-IN-CATALOG",typeof(AddEditProductForm));
        Sql("INSERT dbo.MalzemeKatalog(MalzemeNo,Barkod,Aciklama,BirimId) VALUES(N'QA-NEW',N'QA-NEW-CATALOG',N'QA New Product',1)");
        CheckCreationFlow(main,"QA-NEW-CATALOG",typeof(AddEditProductForm));
        Check(Convert.ToInt32(Sql("SELECT COUNT(*) FROM dbo.Urun WHERE Barkod=N'QA-NEW-CATALOG'"))==0,"cancelled product creation creates no product");
        main.Close();
      }
      Check(Convert.ToInt32(Sql("SELECT COUNT(*) FROM dbo.IslemGunlugu WHERE TabloAdi=N'Uygulama'"))>0,"application button actions are logged");
      Console.WriteLine("TOTAL: "+checks+" checks"); return 0;
    }
    catch(Exception ex) { Console.WriteLine(ex); return 1; }
    finally
    {
      SqlDatabaseConnection.ResetConnection(); SqlConnection.ClearAllPools();
      Environment.SetEnvironmentVariable("INVENTORYTRACKING_CONNECTION_STRING",previous);
      if(admin!=null)
      {
        if(admin.State!=ConnectionState.Open) admin.Open();
        admin.ChangeDatabase("master");
        if(db!=null) Sql("ALTER DATABASE ["+db+"] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE ["+db+"]");
        admin.Dispose();
      }
    }
  }
}
