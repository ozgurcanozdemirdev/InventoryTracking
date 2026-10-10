using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using InventoryTracking.UI.Localization;
using UiResources = InventoryTracking.Properties.Resources;

namespace InventoryTracking.UI.Forms
{
  public partial class MainForm
  {
    private Button _btnLanguage;

    private void InitializeLanguageSupport()
    {
      if (_btnLanguage != null)
        return;

      // Dil düğmesi mevcut üst panelin renklerini ve yazı tipini kullanır.
      _btnLanguage = new Button
      {
        Name = "btnLanguage",
        Size = new Size(70, 38),
        Margin = new Padding(8, 2, 0, 0),
        BackColor = _btnBaglanti.BackColor,
        ForeColor = _btnBaglanti.ForeColor,
        Font = _btnBaglanti.Font,
        FlatStyle = FlatStyle.Flat,
        UseVisualStyleBackColor = false,
        Cursor = Cursors.Hand
      };
      _btnLanguage.FlatAppearance.BorderSize = 0;
      _btnLanguage.FlatAppearance.MouseOverBackColor =
        _btnBaglanti.FlatAppearance.MouseOverBackColor;
      _btnLanguage.FlatAppearance.MouseDownBackColor =
        _btnBaglanti.FlatAppearance.MouseDownBackColor;

      _btnLanguage.Click += BtnLanguage_Click;
      pnlHeaderInfo.Controls.Add(_btnLanguage);

      LanguageManager.LanguageChanged += MainForm_LanguageChanged;
      Disposed += MainFormLanguage_Disposed;
      ApplyMainLanguage();
    }

    private void BtnLanguage_Click(object sender, EventArgs e)
    {
      try
      {
        LanguageManager.ToggleLanguage();
      }
      catch (Exception ex) when (
        ex is IOException ||
        ex is UnauthorizedAccessException)
      {
        UiMessage.Show(ex.Message, UiResources.Language,
          MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void MainForm_LanguageChanged(object sender, EventArgs e)
    {
      ApplyMainLanguage();
    }

    private void ApplyMainLanguage()
    {
      // Yalnızca görünen metinler değişir. Veri yükleme yöntemleri
      // çağrılmadığı için arama metni ve mevcut seçimler korunur.
      SuspendLayout();
      try
      {
        Text = UiResources.AppTitle;
        lblTitle.Text = UiResources.AppTitle.ToUpper(
          CultureInfo.CurrentUICulture);

        btnYeniUrun.Text = UiResources.AddProduct;
        btnDuzenle.Text = UiResources.EditProduct;
        btnSil.Text = UiResources.DeleteProduct;
        btnYenile.Text = UiResources.Refresh;

        txtBarkod.PlaceholderText = UiText.Get("ScanPlaceholder");
        txtBarkod.Invalidate();
        txtBarkod.AccessibleDescription = UiText.Get("SearchAccessible");
        _toolTip.SetToolTip(txtBarkod, UiText.Get("SearchHelp"));
        _toolTip.SetToolTip(lblStockAmount, UiText.Get("StockQuantityHelp"));
        toolStripConnection.ToolTipText = UiText.Get("ConnectionOkHelp");
        if (toolStripStatusLabel.Tag is DateTime)
          toolStripStatusLabel.Text = UiText.Get("LastRefreshPrefix") + ((DateTime)toolStripStatusLabel.Tag).ToString("HH:mm:ss");
        UpdateSelectedProduct();
        dgvStoklar.Invalidate();
        RefreshSalesLanguage();

        // Düğme, basıldığında geçilecek dili gösterir.
        _btnLanguage.Text = LanguageManager.IsEnglish ? "TR" : "EN";
        _btnLanguage.AccessibleName = UiResources.Language;
        _toolTip.SetToolTip(_btnLanguage,
          CultureInfo.GetCultureInfo(
            LanguageManager.IsEnglish ? "tr-TR" : "en").NativeName);
      }
      finally
      {
        ResumeLayout(true);
      }
    }

    private void MainFormLanguage_Disposed(object sender, EventArgs e)
    {
      // Kapanan form statik olayın abonelik listesinde tutulmaz.
      LanguageManager.LanguageChanged -= MainForm_LanguageChanged;
    }
  }
}
