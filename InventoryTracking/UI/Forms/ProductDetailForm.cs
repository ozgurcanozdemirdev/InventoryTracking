using InventoryTracking.UI.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using InventoryTracking.Business;
using InventoryTracking.Models;

namespace InventoryTracking.UI.Forms
{
  public partial class ProductDetailForm : LocalizedForm
  {
    private readonly Product _urun;
    private readonly int _depoId;
    private readonly StockService _stokService;
    private readonly bool? _girisMi;
    private int _currentStockAmount = 0;
    private string _stockStatus = "Kritik";

    protected override void OnLanguageChanged()
    {
      if (lblDurumValue != null)
        lblDurumValue.Text = UiText.DisplayValue("StokDurumu", _stockStatus).ToString();
    }
    public ProductDetailForm(Product urun, int depoId, StockService stokService)
      : this(urun, depoId, stokService, null)
    {
    }

    public ProductDetailForm(Product urun, int depoId, StockService stokService, bool? girisMi)
    {
      InitializeComponent();
      _urun = urun;
      _depoId = depoId;
      _stokService = stokService;
      _girisMi = girisMi;
      Shown += (sender, e) => numMiktar.Focus();
    }

    private void ProductDetailForm_Load(object sender, EventArgs e)
    {
      InitializeControls();
      DisplayProductDetails();
    }

    private void InitializeControls()
    {
      this.Text = _girisMi.HasValue ? (_girisMi.Value ? UiText.Get("StockEntry") : UiText.Get("StockExit")) : UiText.Get("ProductDetail");
      // Ana ekrandaki yön butonu yalnızca istenen işlemi açar. Çift tıklama
      // ile açılan genel ürün penceresinde her iki işlem kullanılabilir.
      btnGiris.Enabled = !_girisMi.HasValue || _girisMi.Value;
      btnCikis.Enabled = !_girisMi.HasValue || !_girisMi.Value;
      this.StartPosition = FormStartPosition.CenterParent;
      this.FormBorderStyle = FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;

      numMiktar.Minimum = 0;
      numMiktar.Maximum = 1000000;

      // Event'lerin çift tetiklenmesini (Memory Leak / Double Fire) engellemek için defansif sistem uygulandı.
      btnGiris.Click -= btnGiris_Click;
      btnGiris.Click += btnGiris_Click;

      btnCikis.Click -= btnCikis_Click;
      btnCikis.Click += btnCikis_Click;

      btnKapat.Click -= btnKapat_Click;
      btnKapat.Click += btnKapat_Click;
    }

    private void DisplayProductDetails()
    {
      try
      {
        // Ürün bilgilerini göster
        lblBarkodValue.Text = _urun.Barkod;
        lblUrunAdiValue.Text = _urun.UrunAdi;
        lblKategoriValue.Text = _urun.KategoriAdi;
        lblBirimValue.Text = _urun.BirimAdi;
        lblFiyatValue.Text = _urun.Fiyat.ToString("C2", System.Globalization.CultureInfo.GetCultureInfo("tr-TR"));
        lblMinStokValue.Text = _urun.MinimumStok.ToString();
        lblMaksStokValue.Text = _urun.MaksimumStok.ToString();

        // Depo adını göster
        var depoService = new WarehouseService();
        var depo = depoService.GetDepoById(_depoId);
        if (depo != null)
        {
          lblDepoValue.Text = depo.DepoAdi;
        }

        // Mevcut stok miktarını al ve göster
        LoadCurrentStock();

        // Ürün durumuna göre renk ayarla
        ApplyStatusColor();
      }
      catch (Exception ex)
      {
        UiMessage.Show(UiText.Format("ProductInfoException", ex.Message), UiText.Get("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void LoadCurrentStock()
    {
      try
      {
        var stoklar = _stokService.GetStokByDepo(_depoId);

        // Stoklar listesi null dönerse patlamaması için '?.' operatörü kullanıldı.
        var stok = stoklar?.Find(s => s.UrunId == _urun.UrunId);

        if (stok != null)
        {
          _currentStockAmount = stok.Miktar;
          lblMiktarValue.Text = stok.Miktar.ToString();
          _stockStatus = stok.StokDurumu;
          lblDurumValue.Text = UiText.DisplayValue("StokDurumu", _stockStatus).ToString();

          // Durum etiketini renklendir
          switch (stok.StokDurumu)
          {
            case "Kritik":
              lblDurumValue.ForeColor = System.Drawing.Color.DarkRed;
              break;
            case "Düşük":
              lblDurumValue.ForeColor = System.Drawing.Color.Orange;
              break;
            case "Normal":
              lblDurumValue.ForeColor = System.Drawing.Color.DarkGreen;
              break;
          }
        }
        else
        {
          _currentStockAmount = 0;
          lblMiktarValue.Text = "0";
          _stockStatus = "Kritik";
          lblDurumValue.Text = UiText.Get("CriticalStock");
          lblDurumValue.ForeColor = System.Drawing.Color.DarkRed;
        }
      }
      catch (Exception ex)
      {
        UiMessage.Show(UiText.Format("StockInfoException", ex.Message), UiText.Get("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void ApplyStatusColor()
    {
      var status = _stokService.GetStockStatus(_urun.UrunId, _depoId);

      switch (status)
      {
        case StockStatusType.Critical:
          pnlStatus.BackColor = System.Drawing.Color.FromArgb(255, 200, 200);
          break;
        case StockStatusType.Low:
          pnlStatus.BackColor = System.Drawing.Color.FromArgb(255, 255, 200);
          break;
        case StockStatusType.Normal:
          pnlStatus.BackColor = System.Drawing.Color.FromArgb(200, 255, 200);
          break;
      }
    }

    private void btnGiris_Click(object sender, EventArgs e)
    {
      if (numMiktar.Value <= 0)
      {
        UiMessage.Show(UiText.Get("PleaseEntryQuantity"), UiText.Get("Warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
        numMiktar.Focus();
        return;
      }

      int miktar = (int)numMiktar.Value;
      _currentStockAmount = _stokService.GetStok(_urun.UrunId, _depoId)?.Miktar ?? 0;
      int yeniMiktar = _currentStockAmount + miktar;

      if (yeniMiktar > _urun.MaksimumStok)
      {
        DialogResult result = UiMessage.Show(
            UiText.Format("MaximumStockPrompt", yeniMiktar, _urun.MaksimumStok),
            UiText.Get("Warning"),
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result != DialogResult.Yes)
          return;
      }

      string aciklama = txtAciklama.Text.Trim();
      var result_op = _stokService.UpdateStok(_urun.UrunId, _depoId, miktar, "Giriş", aciklama);

      if (result_op.Success)
      {
        yeniMiktar = _stokService.GetStok(_urun.UrunId, _depoId)?.Miktar ?? 0;
        UiMessage.Show(UiText.Format("StockUpdatedQuantity", yeniMiktar), UiText.Get("Success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        this.DialogResult = DialogResult.OK;
        this.Close();
      }
      else
      {
        UiMessage.Show(result_op.Message, UiText.Get("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void btnCikis_Click(object sender, EventArgs e)
    {
      if (numMiktar.Value <= 0)
      {
        UiMessage.Show(UiText.Get("PleaseExitQuantity"), UiText.Get("Warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
        numMiktar.Focus();
        return;
      }

      int miktar = (int)numMiktar.Value;

      string aciklama = txtAciklama.Text.Trim();
      var result = _stokService.UpdateStok(_urun.UrunId, _depoId, -miktar, "Çıkış", aciklama);

      if (result.Success)
      {
        int yeniMiktar = _stokService.GetStok(_urun.UrunId, _depoId)?.Miktar ?? 0;
        UiMessage.Show(UiText.Format("StockUpdatedQuantity", yeniMiktar), UiText.Get("Success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        this.DialogResult = DialogResult.OK;
        this.Close();
      }
      else
      {
        UiMessage.Show(result.Message, UiText.Get("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void btnKapat_Click(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.Cancel;
      this.Close();
    }
  }
}
