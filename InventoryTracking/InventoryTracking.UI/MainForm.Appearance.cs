using InventoryTracking.Localization;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using InventoryTracking.Controls;
using InventoryTracking.Models;

namespace InventoryTracking.UI
{
  public partial class MainForm
  {
    private readonly List<Image> _uiImages = new List<Image>();
    private readonly List<Font> _uiFonts = new List<Font>();
    private readonly Dictionary<string, Label> _detailValues = new Dictionary<string, Label>();
    private readonly CultureInfo _searchCulture = CultureInfo.GetCultureInfo("tr-TR");
    private List<Entities.Stok> _loadedStocks = new List<Entities.Stok>();
    private Button _btnStokGiris, _btnStokCikis;
    private Label _lblClock;
    private ToolTip _toolTip;
    private bool _isBindingStocks;
    private string _sortColumn = "UrunAdi";
    private bool _sortDescending;

    private Image CreateIcon(InventoryIcon icon, Color color, int size = 24)
    {
      var image = InventoryIcons.Create(icon, color, size);
      _uiImages.Add(image);
      return image;
    }

    private Font CreateFont(float size, FontStyle style = FontStyle.Regular)
    {
      var font = new Font("Segoe UI", size, style);
      _uiFonts.Add(font);
      return font;
    }

    private void ConfigureAppearance()
    {
      // Tasarım görselindeki alanlar gerçek kontrollere bağlanır. Yerleşim
      // TableLayoutPanel ile boyuta uyarlanır; iş kuralları servislerde kalır.
      picLogo.Image = CreateIcon(InventoryIcon.Cube, Color.White, 36);
      picBarkod.Image = CreateIcon(InventoryIcon.Barcode, Color.FromArgb(76, 91, 111));
      picDepo.Image = CreateIcon(InventoryIcon.Depot, Color.FromArgb(76, 91, 111));
      picQuantity.Image = CreateIcon(InventoryIcon.Cube, Color.FromArgb(0, 117, 201), 30);

      var user = new Label { Name = "lblUser", AutoSize = true, ForeColor = Color.White, Text = Environment.UserName,
        Margin = new Padding(8, 11, 12, 0), Font = CreateFont(10F) };
      _lblClock = new Label { Name = "lblClock", AutoSize = true, ForeColor = Color.White,
        Margin = new Padding(8, 11, 12, 0), Font = CreateFont(10F) };
      pnlHeaderInfo.Controls.Add(user);
      pnlHeaderInfo.Controls.Add(_lblClock);
      pnlHeaderInfo.Controls.Add(_btnBaglanti);
      // İki dilde de aynı genişlik kullanılır; uzun İngilizce metin tek satır kalır.
      _btnBaglanti.AutoSize = false;
      _btnBaglanti.Size = new Size(200, 38);
      _btnBaglanti.Margin = new Padding(8, 2, 0, 0);
      StyleButton(_btnBaglanti, Color.FromArgb(0, 99, 171), InventoryIcon.Settings, 9F);
      // Simge solda sabittir. Yazı alanı simgeden sonra başlar ve dört kenara
      // bağlı kaldığı için düğme boyutu değişince de kendi merkezinde durur.
      var connectionIcon = new PictureBox
      {
        Image = _btnBaglanti.Image, Bounds = new Rectangle(8, 0, 22, 38),
        Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom,
        SizeMode = PictureBoxSizeMode.CenterImage, BackColor = Color.Transparent,
        TabStop = false
      };
      var connectionCaption = new Label
      {
        Name = "lblConnectionSettings", Text = _btnBaglanti.Text,
        Bounds = new Rectangle(36, 0, _btnBaglanti.ClientSize.Width - 44, 38),
        Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
        TextAlign = ContentAlignment.MiddleCenter, AutoSize = false,
        BackColor = Color.Transparent, Font = _btnBaglanti.Font,
        ForeColor = _btnBaglanti.ForeColor, Margin = new Padding(0)
      };
      _btnBaglanti.Image = null;
      _btnBaglanti.Text = string.Empty;
      _btnBaglanti.AccessibleName = connectionCaption.Text;
      connectionCaption.TextChanged += (sender, e) => _btnBaglanti.AccessibleName = connectionCaption.Text;
      // Simge ve yazıya tıklamak da aynı bağlantı ayarları düğmesini çalıştırır.
      connectionIcon.Click += (sender, e) => _btnBaglanti.PerformClick();
      connectionCaption.Click += (sender, e) => _btnBaglanti.PerformClick();
      _btnBaglanti.Controls.Add(connectionIcon);
      _btnBaglanti.Controls.Add(connectionCaption);
      // Designer, tasarımda bileşen yoksa kabı oluşturmayabilir.
      // Saat ve ipucu bu kaba eklenir; form kapanınca birlikte temizlenir.
      if (components == null) components = new System.ComponentModel.Container();
      var clock = new Timer(components) { Interval = 30000 };
      clock.Tick += (sender, e) => UpdateClock();
      clock.Start();
      UpdateClock();

      _toolTip = new ToolTip(components);
      _toolTip.SetToolTip(lblStockAmount, UiText.Get("StockQuantityHelp"));
      _toolTip.SetToolTip(txtBarkod, UiText.Get("SearchHelp"));
      txtBarkod.AccessibleDescription = UiText.Get("SearchAccessible");
      txtBarkod.TextChanged += (sender, e) =>
      {
        if (!_isFormLoading) ApplyStockFilter();
      };

      var buttons = new[] { btnYeniUrun, btnDuzenle, btnSil, btnYenile, btnZRaporu, _btnKategoriYonet, _btnDepoYonet, _btnTransfer, btnKapat };
      var colors = new[] { Color.FromArgb(36, 158, 71), Color.FromArgb(0, 120, 213), Color.FromArgb(225, 51, 65),
        Color.FromArgb(240, 128, 27), Color.FromArgb(149, 87, 206), Color.FromArgb(32, 152, 218),
        Color.FromArgb(30, 156, 174), Color.FromArgb(39, 158, 70), Color.FromArgb(225, 51, 65) };
      var icons = new[] { InventoryIcon.Add, InventoryIcon.Edit, InventoryIcon.Delete, InventoryIcon.Refresh,
        InventoryIcon.Movements, InventoryIcon.Category, InventoryIcon.Depot, InventoryIcon.Transfer, InventoryIcon.Exit };
      float[] widths = { 11, 10, 9, 10, 12, 12, 11, 13, 12 };
      for (int i = 0; i < buttons.Length; i++)
      {
        pnlButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, widths[i]));
        buttons[i].Dock = DockStyle.Fill;
        buttons[i].Margin = new Padding(0, 0, i == buttons.Length - 1 ? 0 : 10, 0);
        StyleButton(buttons[i], colors[i], icons[i]);
      }

      // Tablo yazısı özellikle kalın tutulur. Seçili satır açık mavi kalır;
      // durum yalnızca renk ile anlatılmaz, okunabilir bir metin de gösterilir.
      // Ã‡erÃ§eve sabit panelde kalÄ±r; tablo kaydÄ±rÄ±lÄ±rken fazladan Ã§izim yapÄ±lmaz.
      var stockFrame = new Panel
      {
        Name = "pnlStockFrame", Dock = DockStyle.Fill, Margin = dgvStoklar.Margin,
        Padding = new Padding(1), BackColor = Color.FromArgb(205, 216, 229)
      };
      pnlWorkspace.Controls.Remove(dgvStoklar);
      dgvStoklar.Margin = Padding.Empty;
      stockFrame.Controls.Add(dgvStoklar);
      pnlWorkspace.Controls.Add(stockFrame, 0, 0);

      dgvStoklar.BackgroundColor = Color.White;
      dgvStoklar.BorderStyle = BorderStyle.None;
      // YalnÄ±zca tanÄ±mlÄ± sÃ¼tunlar gÃ¶sterilir; modelin kimlik alanlarÄ± eklenmez.
      dgvStoklar.AutoGenerateColumns = false;
      dgvStoklar.CellBorderStyle = DataGridViewCellBorderStyle.Single;
      dgvStoklar.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
      // KaydÄ±rmadan kalan gÃ¶rÃ¼ntÃ¼ parÃ§alarÄ±nÄ± standart Paint akÄ±ÅŸÄ± yeniler.
      // Invalidate Ã§izimi sÄ±raya alÄ±r; burada elle hÃ¼cre veya Ã§izgi Ã§izilmez.
      dgvStoklar.Scroll += (sender, e) => dgvStoklar.Invalidate();
      dgvStoklar.GridColor = Color.FromArgb(226, 234, 243);
      dgvStoklar.EnableHeadersVisualStyles = false;
      dgvStoklar.RowHeadersVisible = false;
      dgvStoklar.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
      dgvStoklar.ColumnHeadersHeight = 40;
      dgvStoklar.RowTemplate.Height = 38;
      dgvStoklar.AllowUserToResizeRows = false;
      dgvStoklar.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
      dgvStoklar.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(232, 238, 245),
        ForeColor = ForeColor, Font = CreateFont(10F, FontStyle.Bold), Padding = new Padding(8, 0, 0, 0),
        SelectionBackColor = Color.FromArgb(232, 238, 245), SelectionForeColor = ForeColor };
      dgvStoklar.DefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.White, ForeColor = ForeColor,
        Font = CreateFont(11F, FontStyle.Bold), SelectionBackColor = Color.FromArgb(206, 230, 251),
        SelectionForeColor = ForeColor, Padding = new Padding(8, 0, 0, 0), NullValue = "—" };
      dgvStoklar.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 246, 252);
      dgvStoklar.SelectionChanged += (sender, e) => { if (!_isBindingStocks) UpdateSelectedProduct(); };
      dgvStoklar.ColumnHeaderMouseClick += (sender, e) =>
      {
        if (e.ColumnIndex < 0) return;
        string column = dgvStoklar.Columns[e.ColumnIndex].Name;
        _sortDescending = column == _sortColumn && !_sortDescending;
        _sortColumn = column;
        ApplyStockFilter();
      };

      BuildProductPanel();
      statusStrip1.BackColor = Color.FromArgb(232, 238, 245);
      statusStrip1.Font = CreateFont(9F);
      toolStripVersion.Text = "v" + typeof(MainForm).Assembly.GetName().Version.ToString(3);
      toolStripConnection.Padding = new Padding(8, 0, 8, 0);
      toolStripDepot.Padding = new Padding(8, 0, 8, 0);
      pnlHint.BackColor = Color.FromArgb(235, 243, 251);
      lblHint.Padding = new Padding(10, 0, 0, 0);
      lblHint.Font = CreateFont(9F);
      lblRecordCount.Padding = new Padding(8, 0, 10, 0);
      lblRecordCount.Font = CreateFont(9F);
      UpdateSelectedProduct();
    }

    private void StyleButton(Button button, Color color, InventoryIcon icon, float fontSize = 10F)
    {
      button.BackColor = color;
      button.ForeColor = Color.White;
      button.FlatStyle = FlatStyle.Flat;
      button.FlatAppearance.BorderSize = 0;
      button.FlatAppearance.MouseOverBackColor = ControlPaint.Light(color, 0.12F);
      button.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(color, 0.1F);
      button.Font = CreateFont(fontSize, FontStyle.Bold);
      button.Image = CreateIcon(icon, Color.White, 22);
      button.ImageAlign = ContentAlignment.MiddleLeft;
      button.TextAlign = ContentAlignment.MiddleCenter;
      button.TextImageRelation = TextImageRelation.ImageBeforeText;
      button.Padding = new Padding(6, 0, 6, 0);
      button.UseVisualStyleBackColor = false;
      button.Cursor = Cursors.Hand;
    }

    private void UpdateClock() => _lblClock.Text = DateTime.Now.ToString("dd.MM.yyyy  HH:mm");

    private void BuildProductPanel()
    {
      pnlProductDetail.BackColor = Color.FromArgb(248, 250, 253);
      pnlProductDetail.BorderStyle = BorderStyle.None;
      pnlProductDetail.AutoScroll = false;
      pnlProductDetail.Paint += PaintSurfaceBorder;
      var container = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2,
        Padding = new Padding(14, 10, 14, 12), Margin = new Padding(0) };
      container.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
      container.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
      container.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
      // Küçük pencere / büyük ölçek durumunda bilgi alanı kaydırılır; giriş ve
      // çıkış butonları panelin altında her zaman ulaşılabilir kalır.
      var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Margin = new Padding(0) };
      var layout = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true,
        ColumnCount = 1, Margin = new Padding(0) };
      layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
      var title = new Label { Text = UiText.Get("ProductInformation"), Image = CreateIcon(InventoryIcon.Cube, Color.FromArgb(0, 117, 201), 28),
        ImageAlign = ContentAlignment.MiddleLeft,
        TextAlign = ContentAlignment.MiddleCenter, Font = CreateFont(13F, FontStyle.Bold), Dock = DockStyle.Fill,
        Height = 42, Margin = new Padding(0, 0, 0, 8) };
      layout.Controls.Add(title);
      AddDetailField(layout, "Barkod", UiText.Get("Barcode"));
      AddDetailField(layout, "MalzemeNo", UiText.Get("MaterialNumber"));
      AddDetailField(layout, "UrunAdi", UiText.Get("Product"), 48);
      AddDetailField(layout, "KategoriAdi", UiText.Get("Category"));
      AddDetailField(layout, "Miktar", UiText.Get("Quantity"));
      AddDetailField(layout, "Fiyat", UiText.Get("UnitPrice"));
      AddDetailField(layout, "SonHareketTarihi", UiText.Get("LastMovement"));
      AddDetailField(layout, "StokDurumu", UiText.Get("Status"));
      _detailValues["Miktar"].BackColor = Color.FromArgb(228, 241, 255);
      _detailValues["Miktar"].Font = CreateFont(11F, FontStyle.Bold);
      var actions = new TableLayoutPanel { Dock = DockStyle.Fill, Height = 62,
        ColumnCount = 2, RowCount = 1, Margin = new Padding(0, 8, 0, 0) };
      actions.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
      actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
      actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
      _btnStokGiris = new Button { Name = "btnStokGiris", Text = UiText.Get("StockEntry"), Dock = DockStyle.Fill, Margin = new Padding(0, 0, 8, 0) };
      _btnStokCikis = new Button { Name = "btnStokCikis", Text = UiText.Get("StockExit"), Dock = DockStyle.Fill, Margin = new Padding(0) };
      StyleButton(_btnStokGiris, Color.FromArgb(36, 158, 71), InventoryIcon.Entry);
      StyleButton(_btnStokCikis, Color.FromArgb(0, 120, 213), InventoryIcon.Output);
      _btnStokGiris.Click += (sender, e) => OpenStockOperation(true);
      _btnStokCikis.Click += (sender, e) => OpenStockOperation(false);
      actions.Controls.Add(_btnStokGiris, 0, 0);
      actions.Controls.Add(_btnStokCikis, 1, 0);
      var hint = new Label { Dock = DockStyle.Fill, Height = 30, ForeColor = Color.FromArgb(100, 116, 139),
        Font = CreateFont(9F), TextAlign = ContentAlignment.MiddleLeft, Text = UiText.Get("SelectedProductHint") };
      layout.Controls.Add(hint);
      scroll.Controls.Add(layout);
      container.Controls.Add(scroll, 0, 0);
      container.Controls.Add(actions, 0, 1);
      pnlProductDetail.Controls.Add(container);
    }

    private void AddDetailField(TableLayoutPanel layout, string key, string caption, int height = 36)
    {
      var row = new TableLayoutPanel { Dock = DockStyle.Fill, Height = height, ColumnCount = 2,
        RowCount = 1, Margin = new Padding(0, 0, 0, 4) };
      row.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
      row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 94));
      row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
      row.Controls.Add(new Label { Text = caption, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft,
        Font = CreateFont(9.5F), Margin = new Padding(0) }, 0, 0);
      var value = new InventoryValueLabel { Name = "lblDetail" + key, Text = "—", Dock = DockStyle.Fill,
        BackColor = Color.FromArgb(237, 241, 246),
        Padding = new Padding(7, 0, 4, 0), TextAlign = ContentAlignment.MiddleLeft,
        AutoEllipsis = true, Font = CreateFont(10F), Margin = new Padding(0) };
      _detailValues.Add(key, value);
      row.Controls.Add(value, 1, 0);
      layout.Controls.Add(row);
    }

    private Entities.Stok SelectedStock => dgvStoklar.SelectedRows.Count > 0
      ? dgvStoklar.SelectedRows[0].DataBoundItem as Entities.Stok : null;

    private static void PaintSurfaceBorder(object sender, PaintEventArgs e)
    {
      var control = (Control)sender;
      using (var pen = new Pen(Color.FromArgb(205, 216, 229)))
        e.Graphics.DrawRectangle(pen, 0, 0, Math.Max(0, control.ClientSize.Width - 1), Math.Max(0, control.ClientSize.Height - 1));
    }

    private static string StockStatusText(Entities.Stok stock)
    {
      // Kritik stok pozitif de olabilir. "Stokta Yok" yalnızca sıfır miktarı
      // anlatır; minimum veya düşük stok sınırındaki pozitif miktar "Az Stok"tur.
      if (stock.Miktar == 0) return UiText.Get("OutOfStock");
      return stock.StokDurumu == "Normal" ? UiText.Get("InStock") : UiText.Get("LowStock");
    }

    private static Color StockStatusColor(Entities.Stok stock) => stock.Miktar == 0
      ? Color.FromArgb(209, 41, 59) : stock.StokDurumu == "Normal"
        ? Color.FromArgb(21, 138, 76) : Color.FromArgb(176, 110, 0);

    private void UpdateSelectedProduct()
    {
      if (_btnStokGiris == null) return;
      var stock = SelectedStock;
      foreach (var label in _detailValues.Values) label.Text = "—";
      _btnStokGiris.Enabled = stock != null;
      _btnStokCikis.Enabled = stock != null && stock.Miktar > 0;
      btnDuzenle.Enabled = stock != null;
      btnSil.Enabled = stock != null;
      lblStockAmount.Text = stock == null ? "—" : stock.Miktar.ToString("N0");
      lblStockUnit.Text = stock?.BirimAdi ?? "";
      if (stock != null)
      {
        _detailValues["Barkod"].Text = stock.Barkod;
        _detailValues["MalzemeNo"].Text = stock.MalzemeNo;
        _detailValues["UrunAdi"].Text = stock.UrunAdi;
        _detailValues["KategoriAdi"].Text = stock.KategoriAdi;
        _detailValues["Miktar"].Text = stock.Miktar.ToString("N0") + "  " + stock.BirimAdi;
        _detailValues["Fiyat"].Text = stock.Fiyat.ToString("C2", _searchCulture);
        _detailValues["SonHareketTarihi"].Text = stock.SonHareketTarihi?.ToString("dd.MM.yyyy HH:mm") ?? UiText.Get("NoMovement");
        _detailValues["StokDurumu"].Text = "●  " + StockStatusText(stock);
        _detailValues["StokDurumu"].ForeColor = StockStatusColor(stock);
        _toolTip.SetToolTip(_detailValues["StokDurumu"], UiText.Get("StockLevelPrefix") + UiText.DisplayValue("StokDurumu", stock.StokDurumu));
      }
      foreach (var label in _detailValues.Values)
        if (label != _detailValues["StokDurumu"]) _toolTip.SetToolTip(label, label.Text);
      lblRecordCount.Text = UiText.Get("ShownPrefix") + dgvStoklar.Rows.Count + " / " + _loadedStocks.Count + UiText.Get("SelectedSuffix") + (stock == null ? 0 : 1);
    }

    private bool ContainsSearch(string value, string search) => _searchCulture.CompareInfo.IndexOf(
      value ?? "", search, CompareOptions.IgnoreCase) >= 0;

    private object StockSortValue(Entities.Stok stock)
    {
      switch (_sortColumn)
      {
        case "Barkod": return stock.Barkod;
        case "MalzemeNo": return stock.MalzemeNo;
        case "KategoriAdi": return stock.KategoriAdi;
        case "Miktar": return stock.Miktar;
        case "BirimAdi": return stock.BirimAdi;
        case "SonHareketTarihi": return stock.SonHareketTarihi ?? DateTime.MinValue;
        case "StokDurumu": return StockStatusText(stock);
        default: return stock.UrunAdi;
      }
    }

    private void ApplyStockFilter(int preferredProductId = 0)
    {
      // Tuş başına SQL çağrısı yapılmaz. Depodan gelen kayıtlar Türkçe harf
      // kurallarına göre süzülür; yenileme ve sıralamada seçili ürün korunur.
      int selectedId = preferredProductId > 0 ? preferredProductId : SelectedStock?.UrunId ?? 0;
      string search = txtBarkod.Text.Trim();
      var rows = _loadedStocks.Where(s => search.Length == 0 || ContainsSearch(s.Barkod, search)
        || ContainsSearch(s.UrunAdi, search) || ContainsSearch(s.MalzemeNo, search));
      rows = _sortDescending ? rows.OrderByDescending(StockSortValue) : rows.OrderBy(StockSortValue);
      _isBindingStocks = true;
      try
      {
        dgvStoklar.DataSource = rows.ToList();
        foreach (DataGridViewColumn column in dgvStoklar.Columns)
          column.HeaderCell.SortGlyphDirection = column.Name == _sortColumn
            ? (_sortDescending ? SortOrder.Descending : SortOrder.Ascending) : SortOrder.None;
        var selectedRow = dgvStoklar.Rows.Cast<DataGridViewRow>()
          .FirstOrDefault(r => (r.DataBoundItem as Entities.Stok)?.UrunId == selectedId);
        if (selectedRow != null) dgvStoklar.CurrentCell = selectedRow.Cells["UrunAdi"];
      }
      finally { _isBindingStocks = false; }
      UpdateSelectedProduct();
    }

    private void DisposeAppearanceResources()
    {
      foreach (var image in _uiImages) image.Dispose();
      foreach (var font in _uiFonts) font.Dispose();
      _uiImages.Clear();
      _uiFonts.Clear();
    }
  }
}
