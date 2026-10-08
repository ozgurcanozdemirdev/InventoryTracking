using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static InventoryTracking.Business.Services;
using static InventoryTracking.Models.Entities;

namespace InventoryTracking.UI
{
  public partial class ZReportForm : Form
  {
    private int _yazdirilacakSatirIndex = 0;
    private readonly ZReportService _reportService = new ZReportService();
    private readonly ComboBox _cmbDepo = new ComboBox();
    private readonly TextBox _txtUrun = new TextBox();
    private readonly ComboBox _cmbHareket = new ComboBox();
    private readonly ComboBox _cmbYon = new ComboBox();
    private readonly TabControl _sekmeler = new TabControl();
    private readonly TabPage _ozetSekmesi = new TabPage("Özet");
    private readonly TabPage _detaySekmesi = new TabPage("Hareket Detayı");
    private readonly DataGridView _dgvDetay = new DataGridView();
    private readonly Label _lblToplam = new Label();
    private string _uygulananFiltre = "";
    private bool _raporGetirildi;

    private sealed class DepoSecenegi
    {
      public int Id { get; set; }
      public string Ad { get; set; }
      public override string ToString() => Ad;
    }

    private static string CsvValue(object value)
    {
      // Ayraç ve tırnakları güvenli biçimde dışa aktar.
      string text = value?.ToString() ?? "";
      // Excel'de kullanıcı metninin formül olarak çalışmasını engelle.
      if (value is string && text.Length > 0 && "=+-@\t\r\n".IndexOf(text[0]) >= 0)
        text = "'" + text;
      return "\"" + text.Replace("\"", "\"\"") + "\"";
    }
    public ZReportForm()
    {
      InitializeComponent();
    }

    private void lblTitle_Click(object sender, EventArgs e)
    {

    }

    private void ZReportForm_Load(object sender, EventArgs e)
    {
      // Form açıldığında varsayılan tarihleri ayarlayalım (Bugün ve 1 ay öncesi)
      dtpBaslangic.Value = DateTime.Now.AddMonths(-1);
      dtpBitis.Value = DateTime.Now;

      InitializeReportLayout();
      LoadDepoOptions();
      SetupDataGridViewStyle();
    }

    private void InitializeReportLayout()
    {
      ClientSize = new Size(1180, 680);
      pnlFilter.Height = 120;

      // Önceki tarih ve dönem denetimlerini koruyup ikinci satıra ayrıntı filtreleri ekliyoruz.
      AddFilterLabel("Depo:", 15, 78, 50);
      _cmbDepo.SetBounds(65, 74, 225, 28);
      _cmbDepo.Name = "cmbRaporDepo";
      _cmbDepo.DropDownStyle = ComboBoxStyle.DropDownList;
      pnlFilter.Controls.Add(_cmbDepo);

      AddFilterLabel("Ürün / Barkod:", 305, 78, 120);
      _txtUrun.SetBounds(425, 74, 215, 28);
      _txtUrun.Name = "txtRaporUrun";
      _txtUrun.MaxLength = 200;
      pnlFilter.Controls.Add(_txtUrun);

      AddFilterLabel("Hareket:", 655, 78, 80);
      _cmbHareket.SetBounds(735, 74, 195, 28);
      _cmbHareket.Name = "cmbRaporHareket";
      _cmbHareket.DropDownStyle = ComboBoxStyle.DropDownList;
      _cmbHareket.Items.AddRange(new object[] { "Tümü", "Normal Giriş/Çıkış", "Transfer", "Sayım Düzeltmesi" });
      _cmbHareket.SelectedIndex = 0;
      pnlFilter.Controls.Add(_cmbHareket);

      AddFilterLabel("Yön:", 945, 78, 45);
      _cmbYon.SetBounds(990, 74, 165, 28);
      _cmbYon.Name = "cmbRaporYon";
      _cmbYon.DropDownStyle = ComboBoxStyle.DropDownList;
      _cmbYon.Items.AddRange(new object[] { "Tümü", "Giriş", "Çıkış" });
      _cmbYon.SelectedIndex = 0;
      pnlFilter.Controls.Add(_cmbYon);

      pnlGrid.Controls.Remove(dgvRapor);
      _sekmeler.Dock = DockStyle.Fill;
      _sekmeler.Name = "tabHareketRaporu";
      _sekmeler.Font = new Font("Segoe UI", 10F);
      _ozetSekmesi.BackColor = Color.White;
      _detaySekmesi.BackColor = Color.White;
      dgvRapor.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
      dgvRapor.Dock = DockStyle.Fill;
      _lblToplam.Dock = DockStyle.Bottom;
      _lblToplam.Height = 45;
      _lblToplam.Padding = new Padding(8, 5, 8, 0);
      _lblToplam.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
      _lblToplam.Text = "Filtreleri seçip Raporu Getir'e basın.";
      _ozetSekmesi.Controls.Add(dgvRapor);
      _ozetSekmesi.Controls.Add(_lblToplam);

      _dgvDetay.Dock = DockStyle.Fill;
      _dgvDetay.Name = "dgvHareketDetay";
      _dgvDetay.ReadOnly = true;
      _dgvDetay.AllowUserToAddRows = false;
      _dgvDetay.AllowUserToDeleteRows = false;
      _dgvDetay.RowHeadersVisible = false;
      _dgvDetay.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
      _dgvDetay.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
      _dgvDetay.BackgroundColor = Color.White;
      _dgvDetay.BorderStyle = BorderStyle.None;
      _detaySekmesi.Controls.Add(_dgvDetay);
      _sekmeler.TabPages.Add(_ozetSekmesi);
      _sekmeler.TabPages.Add(_detaySekmesi);
      pnlGrid.Controls.Add(_sekmeler);
    }

    private void AddFilterLabel(string text, int left, int top, int width)
    {
      pnlFilter.Controls.Add(new Label
      {
        Text = text, Left = left, Top = top, Width = width, Height = 24,
        Font = new Font("Segoe UI", 10F, FontStyle.Bold),
        ForeColor = Color.FromArgb(52, 73, 94)
      });
    }

    private void LoadDepoOptions()
    {
      _cmbDepo.Items.Clear();
      _cmbDepo.Items.Add(new DepoSecenegi { Id = 0, Ad = "Tüm depolar" });
      try
      {
        foreach (var depo in new DepoService().GetAllDepolar())
          _cmbDepo.Items.Add(new DepoSecenegi { Id = depo.DepoId, Ad = depo.DepoAdi });
      }
      catch (Exception ex)
      {
        MessageBox.Show("Depo filtresi yüklenemedi: " + ex.Message, "Hata",
          MessageBoxButtons.OK, MessageBoxIcon.Warning);
      }
      _cmbDepo.SelectedIndex = 0;
    }

    private HareketRaporFiltresi GetFilter()
    {
      if (dtpBaslangic.Value.Date > dtpBitis.Value.Date)
        throw new ArgumentException("Başlangıç tarihi, bitiş tarihinden ileri olamaz.");
      var selectedDepo = _cmbDepo.SelectedItem as DepoSecenegi;
      return new HareketRaporFiltresi
      {
        Baslangic = dtpBaslangic.Value.Date,
        Bitis = dtpBitis.Value.Date,
        DepoId = selectedDepo != null && selectedDepo.Id > 0 ? (int?)selectedDepo.Id : null,
        UrunArama = _txtUrun.Text.Trim(),
        HareketGrubu = _cmbHareket.SelectedIndex,
        CikisMi = _cmbYon.SelectedIndex == 0 ? (bool?)null : _cmbYon.SelectedIndex == 2
      };
    }

    private string FilterDescription()
    {
      string donem = rbYillik.Checked ? "Yıllık" : rbAylik.Checked ? "Aylık" : "Günlük";
      string urun = string.IsNullOrWhiteSpace(_txtUrun.Text) ? "Tüm ürünler" : _txtUrun.Text.Trim();
      return $"{dtpBaslangic.Value:dd.MM.yyyy} - {dtpBitis.Value:dd.MM.yyyy} | {donem} | " +
        $"{_cmbDepo.Text} | {urun} | {_cmbHareket.Text} | {_cmbYon.Text}";
    }

    private DataGridView SelectedGrid() => _sekmeler.SelectedTab == _detaySekmesi ? _dgvDetay : dgvRapor;

    private void btnExcel_Click(object sender, EventArgs e)
    {
      var grid = SelectedGrid();
      if (!_raporGetirildi || grid.Rows.Count == 0)
      {
        MessageBox.Show("Aktarılacak veri bulunamadı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
      }

      using (var sfd = new SaveFileDialog())
      {
        // Uzantı uyarısını kaldırmak için sadece CSV bıraktık
        sfd.Filter = "CSV Dosyası (*.csv)|*.csv";
        sfd.FileName = $"StokHareket_{(_sekmeler.SelectedTab == _detaySekmesi ? "Detay" : "Ozet")}_{DateTime.Now:yyyyMMdd_HHmmss}";

        if (sfd.ShowDialog() != DialogResult.OK) return;

        try
        {
          WriteCsv(sfd.FileName, grid);

          MessageBox.Show($"Rapor kaydedildi:\n{sfd.FileName}", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
          MessageBox.Show("Aktarım sırasında hata:\n" + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
      }
    }

    private void btnPdf_Click(object sender, EventArgs e)
    {
      if (!_raporGetirildi || SelectedGrid().Rows.Count == 0)
      {
        MessageBox.Show("Aktarılacak veri bulunamadı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
      }

      using (var sfd = new SaveFileDialog())
      {
        sfd.Filter = "PDF Dosyası (*.pdf)|*.pdf";
        sfd.FileName = $"StokHareket_{(_sekmeler.SelectedTab == _detaySekmesi ? "Detay" : "Ozet")}_{DateTime.Now:yyyyMMdd_HHmmss}";

        if (sfd.ShowDialog() != DialogResult.OK) return;

        try
        {
          _yazdirilacakSatirIndex = 0; // Yazdırma işlemine en baştan (0. satır) başla

          using (var pd = new System.Drawing.Printing.PrintDocument())
          {
            pd.PrinterSettings.PrinterName = "Microsoft Print to PDF";
            if (!pd.PrinterSettings.IsValid)
              throw new InvalidOperationException("Microsoft Print to PDF yazıcısı bulunamadı.");
            pd.PrinterSettings.PrintToFile = true;
            pd.PrinterSettings.PrintFileName = sfd.FileName;
            pd.DefaultPageSettings.Landscape = true;
            pd.PrintPage += Pd_PrintPage;
            pd.Print();
          }

          MessageBox.Show($"PDF başarıyla kaydedildi:\n{sfd.FileName}", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
          MessageBox.Show("PDF oluşturulurken hata meydana geldi. Bilgisayarınızda 'Microsoft Print to PDF' özelliğinin açık olduğundan emin olun.\nDetay: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
      }
    }
    private void btnKapat_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    private void btnGetir_Click(object sender, EventArgs e)
    {
      try
      {
        _raporGetirildi = false;
        dgvRapor.DataSource = null;
        _dgvDetay.DataSource = null;
        _lblToplam.Text = "Rapor yükleniyor...";
        var filtre = GetFilter();
        string periyot = rbYillik.Checked ? "Yıllık" : rbAylik.Checked ? "Aylık" : "Günlük";
        var ozet = _reportService.GetHareketOzeti(filtre, periyot);
        var detay = _reportService.GetHareketDetayi(filtre);
        dgvRapor.DataSource = ozet;
        _dgvDetay.DataSource = detay;
        ConfigureColumns();
        _uygulananFiltre = FilterDescription();
        _raporGetirildi = true;
        _lblToplam.Text = $"Normal: +{ozet.Sum(x => x.NormalGiris):N0} / -{ozet.Sum(x => x.NormalCikis):N0}   " +
          $"Transfer: +{ozet.Sum(x => x.TransferGiris):N0} / -{ozet.Sum(x => x.TransferCikis):N0}   " +
          $"Sayım: +{ozet.Sum(x => x.DuzeltmeGiris):N0} / -{ozet.Sum(x => x.DuzeltmeCikis):N0}\n" +
          $"Net değişim: {ozet.Sum(x => x.NetDegisim):N0}   Hareket sayısı: {detay.Count:N0}";

        if (detay.Count == 0)
        {
          MessageBox.Show("Seçilen filtrelere uygun stok hareketi bulunamadı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
      }
      catch (Exception ex)
      {
        _lblToplam.Text = "Rapor getirilemedi.";
        MessageBox.Show("Rapor verileri getirilemedi:\n\n" + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private static void WriteCsv(string fileName, DataGridView grid)
    {
      using (var writer = new System.IO.StreamWriter(fileName, false, new System.Text.UTF8Encoding(true)))
      {
        var basliklar = new List<string>();
        foreach (DataGridViewColumn col in grid.Columns)
          if (col.Visible) basliklar.Add(CsvValue(col.HeaderText));
        writer.WriteLine(string.Join(";", basliklar));

        foreach (DataGridViewRow row in grid.Rows)
        {
          var satir = new List<string>();
          foreach (DataGridViewCell cell in row.Cells)
            if (grid.Columns[cell.ColumnIndex].Visible) satir.Add(CsvValue(cell.Value));
          writer.WriteLine(string.Join(";", satir));
        }
      }
    }

    private void ConfigureColumns()
    {
      string[] ozetAdlari = { "Dönem", "Normal Giriş", "Normal Çıkış", "Transfer Giriş",
        "Transfer Çıkış", "Sayım Giriş", "Sayım Çıkış", "Net Değişim", "Hareket Sayısı" };
      for (int i = 0; i < ozetAdlari.Length && i < dgvRapor.Columns.Count; i++)
        dgvRapor.Columns[i].HeaderText = ozetAdlari[i];

      string[] detayAdlari = { "Hareket No", "Tarih / Saat", "Ürün", "Barkod", "Depo",
        "Hareket Türü", "Yön", "Değişim", "Önceki", "Yeni", "Kullanıcı", "Açıklama", "Transfer No" };
      for (int i = 0; i < detayAdlari.Length && i < _dgvDetay.Columns.Count; i++)
        _dgvDetay.Columns[i].HeaderText = detayAdlari[i];
      if (_dgvDetay.Columns.Contains("HareketTarihi"))
        _dgvDetay.Columns["HareketTarihi"].DefaultCellStyle.Format = "dd.MM.yyyy HH:mm:ss";
      if (_dgvDetay.Columns.Contains("Aciklama")) _dgvDetay.Columns["Aciklama"].Width = 260;
    }



    private void Pd_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
    {
      if (_sekmeler.SelectedTab == _detaySekmesi)
      {
        PrintDetailPage(e);
        return;
      }
      // Çizimde kullanılacak yazı tipleri ve fırçalar
      Font baslikFont = new Font("Segoe UI", 16, FontStyle.Bold);
      Font kolonFont = new Font("Segoe UI", 10, FontStyle.Bold);
      Font icerikFont = new Font("Segoe UI", 9, FontStyle.Regular);
      SolidBrush firca = new SolidBrush(Color.Black);
      Pen cizgi = new Pen(Color.Black, 1);
      try
      {

      int y = e.MarginBounds.Top; // Sayfanın üstten boşluğu
      int solMarj = e.MarginBounds.Left; // Sayfanın soldan boşluğu

      // Filtre ve başlık her sayfada görünür; sayfa değişince bağlam kaybolmaz.
      e.Graphics.DrawString("STOK HAREKET RAPORU - ÖZET", baslikFont, firca, solMarj, y);
      y += 35;
      int filterHeight = (int)Math.Ceiling(e.Graphics.MeasureString(_uygulananFiltre, icerikFont, e.MarginBounds.Width).Height);
      e.Graphics.DrawString(_uygulananFiltre, icerikFont, firca,
        new RectangleF(solMarj, y, e.MarginBounds.Width, filterHeight));
      y += filterHeight + 12;

      // Tablonun PDF sayfasına tam sığması için oran hesaplıyoruz
      int toplamGenislik = 0;
      foreach (DataGridViewColumn col in dgvRapor.Columns)
        if (col.Visible) toplamGenislik += col.Width;

      float oran = (float)e.MarginBounds.Width / Math.Max(1, toplamGenislik);

      // 2. Tablo Sütun Başlıklarını Çizdir (Açık Gri Arka Planlı)
      int x = solMarj;
      foreach (DataGridViewColumn col in dgvRapor.Columns)
      {
        if (!col.Visible) continue;
        int cWidth = (int)(col.Width * oran);
        Rectangle rect = new Rectangle(x, y, cWidth, 30);

        e.Graphics.FillRectangle(Brushes.LightGray, rect); // Başlık arka planı
        e.Graphics.DrawRectangle(cizgi, rect); // Kenarlık

        // Metni karenin tam ortasına hizala
        StringFormat sf = new StringFormat();
        sf.Alignment = StringAlignment.Center;
        sf.LineAlignment = StringAlignment.Center;

        e.Graphics.DrawString(col.HeaderText, kolonFont, firca, rect, sf);
        x += cWidth;
      }
      y += 30;

      // 3. Tablo Satırlarını Çizdir (Eğer sayfaya sığmazsa diğer sayfaya geçer)
      while (_yazdirilacakSatirIndex < dgvRapor.Rows.Count)
      {
        DataGridViewRow row = dgvRapor.Rows[_yazdirilacakSatirIndex];
        x = solMarj;
        int rowHeight = 25; // Her satırın yüksekliği

        // Sayfanın en altına geldik mi kontrolü
        if (y + rowHeight > e.MarginBounds.Bottom)
        {
          e.HasMorePages = true; // Yeni sayfa aç
          return; // Çizimi burada durdur, yeni sayfada metot baştan çalışıp kaldığı index'ten devam edecek
        }

        foreach (DataGridViewCell cell in row.Cells)
        {
          if (!dgvRapor.Columns[cell.ColumnIndex].Visible) continue;

          int cWidth = (int)(dgvRapor.Columns[cell.ColumnIndex].Width * oran);
          Rectangle rect = new Rectangle(x, y, cWidth, rowHeight);

          e.Graphics.DrawRectangle(cizgi, rect); // Hücre kenarlığı

          StringFormat sf = new StringFormat();
          sf.Alignment = StringAlignment.Near; // Sola yasla
          sf.LineAlignment = StringAlignment.Center; // Dikeyde ortala

          string metin = cell.Value != null ? cell.Value.ToString() : "";

          // Eğer rakamsa sağa yaslayalım ki daha profesyonel dursun
          if (decimal.TryParse(metin, out _))
            sf.Alignment = StringAlignment.Far;

          e.Graphics.DrawString(metin, icerikFont, firca, rect, sf);

          x += cWidth;
        }
        y += rowHeight;
        _yazdirilacakSatirIndex++;
      }

      // Bütün satırlar bittiyse sayfalamayı kapat
      e.HasMorePages = false;
      _yazdirilacakSatirIndex = 0; // Bir sonraki yeni yazdırma işlemi için sıfırla
      }
      finally
      {
      baslikFont.Dispose();
      kolonFont.Dispose();
      icerikFont.Dispose();
      firca.Dispose();
      cizgi.Dispose();
      }
    }

    private void PrintDetailPage(System.Drawing.Printing.PrintPageEventArgs e)
    {
      using (var titleFont = new Font("Segoe UI", 14F, FontStyle.Bold))
      using (var boldFont = new Font("Segoe UI", 8F, FontStyle.Bold))
      using (var textFont = new Font("Segoe UI", 8F))
      using (var line = new Pen(Color.LightGray))
      {
        int y = e.MarginBounds.Top;
        int left = e.MarginBounds.Left;
        int width = e.MarginBounds.Width;
        e.Graphics.DrawString("STOK HAREKET RAPORU - HAREKET DETAYI", titleFont, Brushes.Black, left, y);
        y += 35;
        int filterHeight = (int)Math.Ceiling(e.Graphics.MeasureString(_uygulananFiltre, textFont, width).Height);
        e.Graphics.DrawString(_uygulananFiltre, textFont, Brushes.Black,
          new RectangleF(left, y, width, filterHeight));
        y += filterHeight + 12;

        // Çok sütunlu hareketler PDF'de tüm alanlarıyla okunur kartlar halinde yer alır.
        while (_yazdirilacakSatirIndex < _dgvDetay.Rows.Count)
        {
          var hareket = _dgvDetay.Rows[_yazdirilacakSatirIndex].DataBoundItem as HareketRaporDetayi;
          if (hareket == null) { _yazdirilacakSatirIndex++; continue; }
          string satir1 = $"#{hareket.HareketId}  {hareket.HareketTarihi:dd.MM.yyyy HH:mm:ss}  " +
            $"{hareket.UrunAdi} [{hareket.Barkod}]  |  {hareket.DepoAdi}";
          string satir2 = $"{hareket.HareketTipi} / {hareket.Yon}  " +
            $"Değişim: {hareket.DegisenMiktar:+#;-#;0}  Stok: {hareket.OncekiMiktar} → {hareket.YeniMiktar}  " +
            $"Kullanıcı: {hareket.KullaniciAdi}  Transfer: {(hareket.TransferId?.ToString() ?? "-")}";
          string satir3 = "Açıklama: " + hareket.Aciklama;
          int h1 = (int)Math.Ceiling(e.Graphics.MeasureString(satir1, boldFont, width).Height);
          int h2 = (int)Math.Ceiling(e.Graphics.MeasureString(satir2, textFont, width).Height);
          int h3 = (int)Math.Ceiling(e.Graphics.MeasureString(satir3, textFont, width).Height);
          int rowHeight = h1 + h2 + h3 + 15;
          if (y + rowHeight > e.MarginBounds.Bottom && y > e.MarginBounds.Top + 90)
          {
            e.HasMorePages = true;
            return;
          }
          e.Graphics.DrawString(satir1, boldFont, Brushes.Black, new RectangleF(left, y, width, h1));
          y += h1;
          e.Graphics.DrawString(satir2, textFont, Brushes.Black, new RectangleF(left, y, width, h2));
          y += h2;
          e.Graphics.DrawString(satir3, textFont, Brushes.Black, new RectangleF(left, y, width, h3));
          y += h3 + 5;
          e.Graphics.DrawLine(line, left, y, left + width, y);
          y += 10;
          _yazdirilacakSatirIndex++;
        }
        e.HasMorePages = false;
        _yazdirilacakSatirIndex = 0;
      }
    }

    private void SetupDataGridViewStyle()
    {
      foreach (var grid in new[] { dgvRapor, _dgvDetay })
      {
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        grid.ColumnHeadersHeight = 40;
        grid.DefaultCellStyle.Font = new Font("Segoe UI", 10);
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(189, 195, 199);
        grid.DefaultCellStyle.SelectionForeColor = Color.Black;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(236, 240, 241);
      }
    }
  }
}
