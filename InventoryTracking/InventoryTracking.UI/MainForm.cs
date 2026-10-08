using InventoryTracking.DataAccess;
using System;
using System.Windows.Forms;
using static InventoryTracking.Business.Services;
using InventoryTracking.Models;
using InventoryTracking.UI;


namespace InventoryTracking.UI
{
  public partial class MainForm : Form
  {
    private readonly StokService _stokService;
    private readonly UrunService _urunService;
    private readonly DepoService _depoService;
    private Button _btnKategoriYonet;
    private Button _btnDepoYonet;
    private Button _btnTransfer;
    private Button _btnBaglanti;
    private int _selectedDepoId = 1;


    //Form yüklenirken event'lerin gereksiz tetiklenmesini engellemek için
    private bool _isFormLoading = true;
    public MainForm()
    {
      InitializeComponent();
      _stokService = new StokService();
      _urunService = new UrunService();
      _depoService = new DepoService();
      // Görünüm bir kez kurulur; Load yalnızca veriyi yükler. Böylece yenilemede
      // veya tasarım kontrolünde butonlar ve olay abonelikleri çoğalmaz.
      InitializeControls();
    }

    //bool formloading = false;

    private void MainForm_Load(object sender, EventArgs e)
    {
      _isFormLoading = true; // Yükleme başladı

      LoadDepolar();
      LoadStokList();

      _isFormLoading = false; // Yükleme bitti, artık kullanıcı etkileşimlerine açık
      txtBarkod.Focus();
    }

    private void InitializeControls()
    {
      this.WindowState = FormWindowState.Maximized;
      dgvStoklar.AllowUserToOrderColumns = false;
      ConfigureDataGridViewColumns();

      _btnKategoriYonet = new Button { Name = "btnKategoriYonet", Text = "Kategoriler" };
      _btnDepoYonet = new Button { Name = "btnDepoYonet", Text = "Depolar" };
      _btnTransfer = new Button { Name = "btnTransfer", Text = "Transfer" };
      _btnBaglanti = new Button { Name = "btnBaglanti", Text = "Bağlantı Ayarları" };
      _btnKategoriYonet.Click += (sender, e) =>
      {
        using (var form = new KategoriYonetimForm()) form.ShowDialog(this);
        LoadStokList();
      };
      _btnDepoYonet.Click += (sender, e) =>
      {
        using (var form = new DepoYonetimForm()) form.ShowDialog(this);
        _isFormLoading = true;
        try { LoadDepolar(_selectedDepoId); }
        finally { _isFormLoading = false; }
        LoadStokList();
      };
      _btnTransfer.Click += (sender, e) =>
      {
        using (var form = new StokTransferForm(_selectedDepoId, SelectedStock?.UrunId ?? 0))
          if (form.ShowDialog(this) == DialogResult.OK) LoadStokList();
      };
      _btnBaglanti.Click += (sender, e) =>
      {
        using (var form = new ConnectionSetupForm())
          if (form.ShowDialog(this) != DialogResult.OK) return;
        _isFormLoading = true;
        try { LoadDepolar(); }
        finally { _isFormLoading = false; }
        LoadStokList();
      };
      pnlButtons.Controls.Add(_btnKategoriYonet, 5, 0);
      pnlButtons.Controls.Add(_btnDepoYonet, 6, 0);
      pnlButtons.Controls.Add(_btnTransfer, 7, 0);
      ConfigureAppearance();
      dgvStoklar.CellFormatting += DgvStoklar_CellFormatting;
      dgvStoklar.CellDoubleClick += DgvStoklar_CellDoubleClick;
    }

    private void ConfigureDataGridViewColumns()
    {
      dgvStoklar.Columns.Clear();
      AddStockColumn("Barkod", "Barkod", 155, 145);
      AddStockColumn("MalzemeNo", "Malzeme No", 125, 110);
      AddStockColumn("UrunAdi", "Ürün", 230, 180);
      AddStockColumn("KategoriAdi", "Kategori", 125, 100);
      AddStockColumn("Miktar", "Miktar", 75, 75);
      AddStockColumn("BirimAdi", "Birim", 85, 85);
      AddStockColumn("SonHareketTarihi", "Son Hareket", 170, 160);
      AddStockColumn("StokDurumu", "Durum", 125, 125);
      dgvStoklar.Columns["Miktar"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
      dgvStoklar.Columns["Miktar"].DefaultCellStyle.Format = "N0";
      dgvStoklar.Columns["Miktar"].DefaultCellStyle.Padding = new Padding(0, 0, 8, 0);
      dgvStoklar.Columns["SonHareketTarihi"].DefaultCellStyle.Format = "dd.MM.yyyy HH:mm";
    }

    private void AddStockColumn(string property, string title, float weight, int minimumWidth)
    {
      dgvStoklar.Columns.Add(new DataGridViewTextBoxColumn { Name = property, HeaderText = title,
        DataPropertyName = property, FillWeight = weight, MinimumWidth = minimumWidth,
        SortMode = DataGridViewColumnSortMode.Programmatic });
    }

    private void LoadDepolar(int preferredDepoId = 0)
    {
      try
      {
        var depolar = _depoService.GetAllActiveDepolar();

        if (depolar == null || depolar.Count == 0)
        {
          cmbDepo.DataSource = null;
          _selectedDepoId = 0;
          MessageBox.Show("Aktif depo bulunamadı. Depolar butonundan yeni depo ekleyiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
          return;
        }

        cmbDepo.DisplayMember = "DepoAdi";
        cmbDepo.ValueMember = "DepoId";
        cmbDepo.DataSource = depolar;

        if (preferredDepoId > 0 && depolar.Exists(d => d.DepoId == preferredDepoId))
          cmbDepo.SelectedValue = preferredDepoId;
        else if (cmbDepo.Items.Count > 0)
        {
          cmbDepo.SelectedIndex = 0;
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show($"Depolar yüklenirken hata: {ex.Message}\n\nStackTrace: {ex.StackTrace}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        // Form_Load içindeyken this.Close() çağırmak tehlikelidir. Application.Exit() daha güvenlidir.
        Application.Exit();
      }
    }


    private void LoadStokList()
    {
      int selectedId = SelectedStock?.UrunId ?? 0;
      try
      {
        int depoId;
        if (cmbDepo.SelectedValue == null || !int.TryParse(cmbDepo.SelectedValue.ToString(), out depoId) || depoId <= 0)
        {
          _selectedDepoId = 0;
          _loadedStocks.Clear();
          ApplyStockFilter();
          toolStripDepot.Text = "Depo seçilmedi";
          return;
        }
        _selectedDepoId = depoId;
        _loadedStocks = _stokService.GetStokByDepo(depoId);
        ApplyStockFilter(selectedId);
        toolStripStatusLabel.Text = "Son yenileme: " + DateTime.Now.ToString("HH:mm:ss");
        toolStripConnection.Text = "● Bağlı";
        toolStripConnection.ForeColor = System.Drawing.Color.FromArgb(21, 138, 76);
        toolStripConnection.ToolTipText = "Son veri yüklemesinde SQL Server bağlantısı başarılı.";
        toolStripDepot.Text = cmbDepo.Text;
      }
      catch (Exception ex)
      {
        // Bağlantı veya sorgu başarısızsa eski depoya ait bilgi ekranda kalmaz.
        _loadedStocks.Clear();
        ApplyStockFilter();
        toolStripConnection.Text = "● Veri alınamadı";
        toolStripConnection.ForeColor = System.Drawing.Color.DarkRed;
        toolStripStatusLabel.Text = "Yenileme başarısız";
        MessageBox.Show("Stoklar yüklenirken hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void cmbDepo_SelectedIndexChanged(object sender, EventArgs e)
    {

      // Form yüklenirken gereksiz tetiklemeyi önlemek için kontrol
      if (_isFormLoading) return;

      try
      {
        if (cmbDepo.SelectedValue != null && int.TryParse(cmbDepo.SelectedValue.ToString(), out int depoId))
        {
          if (depoId > 0)
          {
            LoadStokList();
            txtBarkod.Focus();
          }
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
    }

    private void btnYeniUrun_Click(object sender, EventArgs e)
    {
      using (var addForm = new AddEditProductForm(_selectedDepoId))
      {
        if (addForm.ShowDialog(this) == DialogResult.OK)
        {
          LoadStokList();
        }
      }
    }

    private void btnDuzenle_Click(object sender, EventArgs e)
    {
      if (dgvStoklar.SelectedRows.Count == 0)
      {
        MessageBox.Show("Lütfen bir ürün seçiniz", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
      }

      var selectedRow = dgvStoklar.SelectedRows[0];
      string barkod = selectedRow.Cells["Barkod"].Value?.ToString();

      if (string.IsNullOrWhiteSpace(barkod))
      {
        MessageBox.Show("Ürün bilgisi alınamadı", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }

      try
      {
        var urun = _urunService.SearchByBarkod(barkod);
        if (urun == null)
        {
          MessageBox.Show("Ürün bulunamadı", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
          return;
        }

        using (var editForm = new AddEditProductForm(urun, _selectedDepoId))
        {
          if (editForm.ShowDialog(this) == DialogResult.OK)
          {
            LoadStokList();
          }
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show($"İşlem sırasında hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void btnSil_Click(object sender, EventArgs e)
    {
      if (dgvStoklar.SelectedRows.Count == 0)
      {
        MessageBox.Show("Lütfen bir ürün seçiniz", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
      }

      var selectedRow = dgvStoklar.SelectedRows[0];
      string urunAdi = selectedRow.Cells["UrunAdi"].Value?.ToString();
      string barkod = selectedRow.Cells["Barkod"].Value?.ToString();

      DialogResult result = MessageBox.Show(
          $"'{urunAdi}' ürününü silmek istediğinize emin misiniz?",
          "Silme Onayı",
          MessageBoxButtons.YesNo,
          MessageBoxIcon.Question);

      if (result != DialogResult.Yes)
        return;

      try
      {
        var urun = _urunService.SearchByBarkod(barkod);
        if (urun != null)
        {
          var deleteResult = _urunService.DeleteProduct(urun.UrunId);

          if (deleteResult.Success)
          {
            MessageBox.Show(deleteResult.Message, "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadStokList();
          }
          else
          {
            MessageBox.Show(deleteResult.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
          }
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show($"Silme işlemi sırasında hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void btnYenile_Click(object sender, EventArgs e)
    {
      txtBarkod.Clear();
      LoadStokList();
      txtBarkod.Focus();
    }

    private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
    {
      Application.Exit();
    }

    private void txtBarkod_KeyPress(object sender, KeyPressEventArgs e)
    {
      if (e.KeyChar == (char)Keys.Enter)
      {
        e.Handled = true;

        string arananBarkod = txtBarkod.Text.Trim();

        if (string.IsNullOrWhiteSpace(arananBarkod))
          return;

        // Ürün adı / malzeme numarası araması listeyi süzer. Yalnızca barkod
        // araması eski kayıt açma akışına geçer; bir ürün adı barkod diye kaydedilmez.
        bool matchesProductText = _loadedStocks.Exists(s =>
          ContainsSearch(s.UrunAdi, arananBarkod) || ContainsSearch(s.MalzemeNo, arananBarkod));
        bool exactBarcode = _loadedStocks.Exists(s => string.Equals(s.Barkod, arananBarkod, StringComparison.OrdinalIgnoreCase));
        if (matchesProductText && !exactBarcode)
        {
          ApplyStockFilter();
          dgvStoklar.Focus();
          return;
        }

        try
        {
          var urunService = new UrunService();
          var urun = urunService.SearchByBarkod(arananBarkod);

          if (urun != null)
          {
            DialogResult cevap = MessageBox.Show(
                $"Bu barkod numarasına sahip bir ürün zaten mevcut: {urun.UrunAdi}.\nÜrün güncelleme formunu açmak ister misiniz?",
                "Ürün Zaten Kayıtlı",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (cevap == DialogResult.Yes)
            {
              using (var editForm = new AddEditProductForm(urun, _selectedDepoId))
              {
                if (editForm.ShowDialog(this) == DialogResult.OK) LoadStokList();
              }
            }

            txtBarkod.Clear();
            txtBarkod.Focus();
          }
          else
          {
            var pasifUrun = urunService.SearchInactiveByBarkod(arananBarkod);
            if (pasifUrun != null)
            {
              var cevap = MessageBox.Show(
                $"{pasifUrun.UrunAdi} pasif durumda. Ürünü yeniden etkinleştirmek ister misiniz? Mevcut stok kayıtları korunur.",
                "Pasif Ürün", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
              if (cevap == DialogResult.Yes)
              {
                var sonuc = urunService.ReactivateProduct(pasifUrun.UrunId);
                MessageBox.Show(sonuc.Message, sonuc.Success ? "Başarılı" : "Hata",
                  MessageBoxButtons.OK, sonuc.Success ? MessageBoxIcon.Information : MessageBoxIcon.Error);
                if (sonuc.Success) LoadStokList();
              }
              txtBarkod.Clear();
              txtBarkod.Focus();
              return;
            }
            MessageBox.Show("Böyle bir ürün bulunmamaktadır.", "Ürün Bulunamadı",
              MessageBoxButtons.OK, MessageBoxIcon.Information);
            var malzemeService = new MalzemeService();
            var malzeme = malzemeService.GetByBarkod(arananBarkod);
            if (malzeme == null)
            {
              MessageBox.Show($"{arananBarkod} numaralı malzeme bulunamadı. Önce yeni malzeme kaydı oluşturun.",
                "Malzeme Bulunamadı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
              using (var malzemeFormu = new MalzemeKayitForm(arananBarkod))
              {
                if (malzemeFormu.ShowDialog(this) != DialogResult.OK)
                  return;
                malzeme = malzemeFormu.KaydedilenMalzeme;
              }
            }

            // Ana katalogda bulunan malzeme için şubedeki ürün/stok kaydı açılır.
            using (var yeniUrunFormu = new AddEditProductForm(_selectedDepoId))
            {
              yeniUrunFormu.GelenBarkod = malzeme.Barkod;
              if (yeniUrunFormu.ShowDialog(this) == DialogResult.OK)
                LoadStokList();
            }

            txtBarkod.Clear();
            txtBarkod.Focus();
          }
        }
        catch (Exception ex)
        {
          MessageBox.Show($"İşlem sırasında hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
      }
    }


    private void DgvStoklar_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
    {
      if (e.RowIndex < 0 || e.RowIndex >= dgvStoklar.Rows.Count) return;
      dgvStoklar.CurrentCell = dgvStoklar.Rows[e.RowIndex].Cells["UrunAdi"];
      OpenStockOperation(null);
    }

    private void OpenStockOperation(bool? girisMi)
    {
      var selected = SelectedStock;
      if (selected == null) return;
      try
      {
        var urun = _urunService.SearchByBarkod(selected.Barkod);
        if (urun == null)
        {
          MessageBox.Show("Ürün bulunamadı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
          LoadStokList();
          return;
        }
        // Miktar girişi ve kayıt yine mevcut işlem penceresinde yapılır.
        // Sağ panel yalnızca bu pencereyi seçilen işlem yönüyle açar.
        using (var detailForm = new ProductDetailForm(urun, _selectedDepoId, _stokService, girisMi))
          if (detailForm.ShowDialog(this) == DialogResult.OK) LoadStokList();
      }
      catch (Exception ex)
      {
        MessageBox.Show("Stok işlem ekranı açılamadı: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void DgvStoklar_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
    {
      if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
      var stock = dgvStoklar.Rows[e.RowIndex].DataBoundItem as Entities.Stok;
      if (stock == null) return;
      string column = dgvStoklar.Columns[e.ColumnIndex].Name;
      if (column == "StokDurumu")
      {
        e.Value = "●  " + StockStatusText(stock);
        e.CellStyle.ForeColor = StockStatusColor(stock);
        e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor;
        e.FormattingApplied = true;
      }
      else if (column == "SonHareketTarihi" && !stock.SonHareketTarihi.HasValue)
      {
        e.Value = "Hareket yok";
        e.FormattingApplied = true;
      }
    }
    private void btnKapat_Click(object sender, EventArgs e)
    {
      DialogResult result = MessageBox.Show(
        "Uygulamadan çıkış yapmak istediğinize emin misiniz?",
        "Çıkış Onayı",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question);

      if (result == DialogResult.Yes)
      {
        // Güvenli kapanış için Application.Exit() kullanıyoruz
        Application.Exit();
      }
    }

    private void btnZRaporu_Click(object sender, EventArgs e)
    {
      // Hareket raporu mevcut ana ekranın üzerinden ayrı bir pencere olarak açılır.
      using (var raporFormu = new ZReportForm())
        raporFormu.ShowDialog(this);
    }
  }
}
