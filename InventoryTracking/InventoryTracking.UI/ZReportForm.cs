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

namespace InventoryTracking.UI
{
  public partial class ZReportForm : Form
  {
    private int _yazdirilacakSatirIndex = 0;
    private static string CsvValue(string value)
    {
      // Ayraç ve tırnakları güvenli biçimde dışa aktar.
      return "\"" + value.Replace("\"", "\"\"") + "\"";
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

      // DataGridView için şık görünüm ayarları
      SetupDataGridViewStyle();
    }

    private void btnExcel_Click(object sender, EventArgs e)
    {
      // Raporu CSV biçiminde dışa aktar.
      if (dgvRapor.DataSource == null || dgvRapor.Rows.Count == 0)
      {
        MessageBox.Show("Aktarılacak veri bulunamadı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
      }

      using (var sfd = new SaveFileDialog())
      {
        // Uzantı uyarısını kaldırmak için sadece CSV bıraktık
        sfd.Filter = "CSV Dosyası (*.csv)|*.csv";
        sfd.FileName = $"StokHareketRaporu_{DateTime.Now:yyyyMMdd_HHmmss}";

        if (sfd.ShowDialog() != DialogResult.OK) return;

        try
        {
          using (var writer = new System.IO.StreamWriter(sfd.FileName, false, System.Text.Encoding.UTF8))
          {
            var basliklar = new List<string>();
            foreach (DataGridViewColumn col in dgvRapor.Columns)
            {
              if (col.Visible)
                basliklar.Add(CsvValue(col.HeaderText));
            }
            writer.WriteLine(string.Join(";", basliklar));

            foreach (DataGridViewRow row in dgvRapor.Rows)
            {
              var satir = new List<string>();
              foreach (DataGridViewCell cell in row.Cells)
              {
                if (dgvRapor.Columns[cell.ColumnIndex].Visible)
                  satir.Add(CsvValue(cell.Value?.ToString() ?? ""));
              }
              writer.WriteLine(string.Join(";", satir));
            }
          }

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
      if (dgvRapor.DataSource == null || dgvRapor.Rows.Count == 0)
      {
        MessageBox.Show("Aktarılacak veri bulunamadı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
      }

      using (var sfd = new SaveFileDialog())
      {
        sfd.Filter = "PDF Dosyası (*.pdf)|*.pdf";
        sfd.FileName = $"StokHareketRaporu_{DateTime.Now:yyyyMMdd_HHmmss}";

        if (sfd.ShowDialog() != DialogResult.OK) return;

        try
        {
          _yazdirilacakSatirIndex = 0; // Yazdırma işlemine en baştan (0. satır) başla

          System.Drawing.Printing.PrintDocument pd = new System.Drawing.Printing.PrintDocument();

          // Windows'un yerleşik PDF oluşturucusuna bağlanıyoruz
          pd.PrinterSettings.PrinterName = "Microsoft Print to PDF";
          pd.PrinterSettings.PrintToFile = true;
          pd.PrinterSettings.PrintFileName = sfd.FileName;

          // Tablomuz geniş olduğu için PDF'i yatay (Landscape) ayarlıyoruz
          pd.DefaultPageSettings.Landscape = true;

          // PDF'in içine çizim yapacak olan metodu bağlıyoruz
          pd.PrintPage += Pd_PrintPage;

          // İşlemi tetikle (Sessizce PDF olarak kaydeder)
          pd.Print();

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
        // 1. Hangi periyodun seçildiğini radyo butonlardan buluyoruz
        string periyot = "Günlük";
        if (rbAylik.Checked) periyot = "Aylık";
        if (rbYillik.Checked) periyot = "Yıllık";

        // 2. Başlangıç ve Bitiş tarihlerini alıyoruz
        DateTime baslangic = dtpBaslangic.Value;
        DateTime bitis = dtpBitis.Value;

        if (baslangic.Date > bitis.Date)
        {
          MessageBox.Show("Başlangıç tarihi, Bitiş tarihinden ileri bir tarih olamaz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
          return;
        }

        // 3. Yazdığımız servisi çağırıp veriyi çekiyoruz
        var reportService = new ZReportService();
        var raporListesi = reportService.GetZRaporu(baslangic, bitis, periyot);

        // 4. Gelen veriyi (DTO Listesini) DataGridView'a bağlıyoruz
        dgvRapor.DataSource = raporListesi;

        // 5. DataGridView Sütunlarını Türkçeleştirip Görselleştiriyoruz
        if (dgvRapor.Columns.Count > 0)
        {
          dgvRapor.Columns["Donem"].HeaderText = "Dönem";
          dgvRapor.Columns["ToplamGirisMiktari"].HeaderText = "Toplam Giriş (Adet)";
          dgvRapor.Columns["ToplamCikisMiktari"].HeaderText = "Toplam Çıkış (Adet)";
        }

        if (raporListesi.Count == 0)
        {
          MessageBox.Show("Seçilen tarih aralığında stok hareketi bulunamadı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show("Rapor verileri getirilirken veritabanında bir hata oluştu:\n\n" + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }



    private void Pd_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
    {
      // Çizimde kullanılacak yazı tipleri ve fırçalar
      Font baslikFont = new Font("Segoe UI", 16, FontStyle.Bold);
      Font kolonFont = new Font("Segoe UI", 10, FontStyle.Bold);
      Font icerikFont = new Font("Segoe UI", 9, FontStyle.Regular);
      SolidBrush firca = new SolidBrush(Color.Black);
      Pen cizgi = new Pen(Color.Black, 1);

      int y = e.MarginBounds.Top; // Sayfanın üstten boşluğu
      int solMarj = e.MarginBounds.Left; // Sayfanın soldan boşluğu

      // 1. Z Raporu Başlığı (Sadece ilk sayfanın en üstüne yazılır)
      if (_yazdirilacakSatirIndex == 0)
      {
        string tarihAraligi = $"({dtpBaslangic.Value:dd.MM.yyyy} - {dtpBitis.Value:dd.MM.yyyy})";
        e.Graphics.DrawString("STOK HAREKET RAPORU " + tarihAraligi, baslikFont, firca, solMarj, y);
        y += 40;
      }

      // Tablonun PDF sayfasına tam sığması için oran hesaplıyoruz
      int toplamGenislik = 0;
      foreach (DataGridViewColumn col in dgvRapor.Columns)
        if (col.Visible) toplamGenislik += col.Width;

      float oran = (float)e.MarginBounds.Width / (float)toplamGenislik;

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

    private void SetupDataGridViewStyle()
    {
      dgvRapor.EnableHeadersVisualStyles = false;
      dgvRapor.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
      dgvRapor.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
      dgvRapor.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
      dgvRapor.ColumnHeadersHeight = 40;
      dgvRapor.DefaultCellStyle.Font = new Font("Segoe UI", 10);
      dgvRapor.DefaultCellStyle.SelectionBackColor = Color.FromArgb(189, 195, 199);
      dgvRapor.DefaultCellStyle.SelectionForeColor = Color.Black;
      dgvRapor.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(236, 240, 241);
    }
  }
}
