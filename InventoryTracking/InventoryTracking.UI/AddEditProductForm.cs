using InventoryTracking.Localization;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using static InventoryTracking.Business.Services;
using static InventoryTracking.Models.Entities;

namespace InventoryTracking.UI
{
  public partial class AddEditProductForm : LocalizedForm
  {
    private readonly UrunService _urunService;
    private readonly KategoriService _kategoriService;
    private readonly BirimService _birimService;
    private readonly StokService _stokService;
    private readonly MalzemeService _malzemeService;
    private Button _btnMalzemeKaydi;
    public string GelenBarkod
    {
      set { txtBarkod.Text = value; }
    }
    private readonly int _selectedDepoId;
    private Urun _editingProduct;
    private bool _isEditMode = false;
    public AddEditProductForm(int selectedDepoId)
    {
      InitializeComponent();
      _selectedDepoId = selectedDepoId;
      _urunService = new UrunService();
      _kategoriService = new KategoriService();
      _birimService = new BirimService();
      _stokService = new StokService();
      _malzemeService = new MalzemeService();
      _isEditMode = false;
    }
    /// <summary>
    /// Ürün düzenleme modu
    /// </summary>
    public AddEditProductForm(Urun product, int selectedDepoId)
    {
      InitializeComponent();
      _selectedDepoId = selectedDepoId;
      _editingProduct = product;
      _urunService = new UrunService();
      _kategoriService = new KategoriService();
      _birimService = new BirimService();
      _stokService = new StokService();
      _malzemeService = new MalzemeService();
      _isEditMode = true;
    }

    private void AddEditProductForm_Load(object sender, EventArgs e)
    {
      InitializeControls();
      LoadComboBoxes();

      if (_isEditMode)
      {
        LoadProductData();
      }
      else
      {
        LoadKatalogBilgisi();
      }
    }

    private void InitializeControls()
    {
      this.Text = _isEditMode ? UiText.Get("EditProductTitle") : UiText.Get("NewProductTitle");
      this.StartPosition = FormStartPosition.CenterParent;
      this.FormBorderStyle = FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;

      // Event'lerin çift tetiklenmesini engellemek için defansif abonelik.
      btnKaydet.Click -= BtnKaydet_Click;
      btnKaydet.Click += BtnKaydet_Click;

      btnIptal.Click -= BtnIptal_Click;
      btnIptal.Click += BtnIptal_Click;

      numMiktar.Minimum = 0;
      numMiktar.Maximum = 1000000;
      numMinStok.Minimum = 0;
      numMinStok.Maximum = 1000000;
      numMaksStok.Minimum = 1;
      numMaksStok.Maximum = 1000000;
      numFiyat.Minimum = 0;
      numFiyat.Maximum = 9999999;
      txtUrunAdi.ReadOnly = true;
      cmbBirim.Enabled = false;

      // Katalog kaydı ürün kaydından ayrı tutulur; yeni barkod önce burada tanımlanır.
      {
        _btnMalzemeKaydi = new Button
        {
          Text = _isEditMode ? UiText.Get("EditMaterial") : UiText.Get("MaterialRecord"),
          // Ã–lÃ§eklenmiÅŸ Kaydet ve Ä°ptal butonlarÄ±na gÃ¶re yerleÅŸir; metin ortada kalÄ±r.
          Location = new System.Drawing.Point(btnKaydet.Right + 7, btnKaydet.Top),
          Size = new System.Drawing.Size(btnIptal.Left - btnKaydet.Right - 14, btnKaydet.Height),
          Anchor = btnKaydet.Anchor,
          TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
          Padding = Padding.Empty,
          UseCompatibleTextRendering = false,
          BackColor = System.Drawing.Color.FromArgb(52, 152, 219),
          ForeColor = System.Drawing.Color.White,
          FlatStyle = FlatStyle.Flat,
          Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold),
          UseVisualStyleBackColor = false
        };
        _btnMalzemeKaydi.FlatAppearance.BorderSize = 0;
        _btnMalzemeKaydi.Click += (sender, e) =>
        {
          var kayit = _malzemeService.GetByBarkod(txtBarkod.Text);
          if (_isEditMode && kayit == null)
          {
            UiMessage.Show(UiText.Get("ProductMaterialMissing"), UiText.Get("Error"),
              MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
          }
          using (var form = _isEditMode
            ? new MalzemeKayitForm(kayit)
            : new MalzemeKayitForm(txtBarkod.Text))
          {
            if (form.ShowDialog(this) == DialogResult.OK)
            {
              txtBarkod.Text = form.KaydedilenMalzeme.Barkod;
              LoadKatalogBilgisi();
            }
          }
        };
        pnlButtons.Controls.Add(_btnMalzemeKaydi);
      }
      txtBarkod.Leave += (sender, e) => LoadKatalogBilgisi();
    }

    private void LoadComboBoxes()
    {
      try
      {
        // Kategorileri yükle (Null koruması eklendi)
        var kategoriler = _kategoriService.GetAllKategoriler() ?? new List<Kategori>();
        cmbKategori.DataSource = kategoriler;
        cmbKategori.DisplayMember = "KategoriAdi";
        cmbKategori.ValueMember = "KategoriId";

        // Birimleri yükle (Null koruması eklendi)
        var birimler = _birimService.GetAllBirimler() ?? new List<Birim>();
        cmbBirim.DataSource = birimler;
        cmbBirim.DisplayMember = "BirimAdi";
        cmbBirim.ValueMember = "BirimId";
      }
      catch (Exception ex)
      {
        UiMessage.Show(UiText.Format("LoadException", ex.Message), UiText.Get("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void LoadProductData()
    {
      try
      {
        txtBarkod.Text = _editingProduct.Barkod;
        txtBarkod.ReadOnly = true; // Barkod düzenlenemez
        txtUrunAdi.Text = _editingProduct.UrunAdi;
        cmbKategori.SelectedValue = _editingProduct.KategoriId;
        cmbBirim.SelectedValue = _editingProduct.BirimId;
        numFiyat.Value = (decimal)_editingProduct.Fiyat;
        numMinStok.Value = _editingProduct.MinimumStok;
        numMaksStok.Value = _editingProduct.MaksimumStok;

        // Mevcut stoku yükle
        var stoklar = _stokService.GetStokByDepo(_selectedDepoId);
        var stok = stoklar?.Find(s => s.UrunId == _editingProduct.UrunId);

        if (stok != null)
        {
          numMiktar.Value = stok.Miktar;
        }
        LoadKatalogBilgisi();
      }
      catch (Exception ex)
      {
        UiMessage.Show(UiText.Format("ProductDataException", ex.Message), UiText.Get("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private bool ValidateForm()
    {
      if (string.IsNullOrWhiteSpace(txtBarkod.Text))
      {
        UiMessage.Show(UiText.Get("PleaseBarcode"), UiText.Get("Warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
        txtBarkod.Focus();
        return false;
      }

      // Kaydetmeden hemen önce yeniden bakıyoruz; ekranda görünen eski katalog bilgisine güvenmiyoruz.
      if (!LoadKatalogBilgisi())
      {
        UiMessage.Show(UiText.Format("MaterialNumberMissing", txtBarkod.Text.Trim()),
          UiText.Get("MaterialNotFoundTitle"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return false;
      }

      if (string.IsNullOrWhiteSpace(txtUrunAdi.Text))
      {
        UiMessage.Show(UiText.Get("PleaseProductName"), UiText.Get("Warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
        txtUrunAdi.Focus();
        return false;
      }

      if (cmbKategori.SelectedValue == null)
      {
        UiMessage.Show(UiText.Get("PleaseCategory"), UiText.Get("Warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return false;
      }

      if (cmbBirim.SelectedValue == null)
      {
        UiMessage.Show(UiText.Get("PleaseUnit"), UiText.Get("Warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return false;
      }

      if (numFiyat.Value < 0)
      {
        UiMessage.Show(UiText.Get("PriceNegative"), UiText.Get("Warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
        numFiyat.Focus();
        return false;
      }

      if (numMinStok.Value > numMaksStok.Value)
      {
        UiMessage.Show(UiText.Get("StockLimitsOrder"), UiText.Get("Warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return false;
      }

      return true;
    }

    private bool LoadKatalogBilgisi()
    {
      if (string.IsNullOrWhiteSpace(txtBarkod.Text)) return false;
      try
      {
        var malzeme = _malzemeService.GetByBarkod(txtBarkod.Text);
        if (malzeme == null)
        {
          txtUrunAdi.Clear();
          return false;
        }
        txtUrunAdi.Text = malzeme.Aciklama;
        cmbBirim.SelectedValue = malzeme.BirimId;
        return true;
      }
      catch (Exception ex)
      {
        UiMessage.Show(UiText.Get("MaterialCatalogError") + ex.Message, UiText.Get("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
      }
    }

    private void BtnKaydet_Click(object sender, EventArgs e)
    {
      if (!ValidateForm())
        return;

      try
      {
        var model = new UrunViewModel
        {
          UrunId = _isEditMode ? _editingProduct.UrunId : 0,
          Barkod = txtBarkod.Text.Trim(),
          UrunAdi = txtUrunAdi.Text.Trim(),
          KategoriId = (int)cmbKategori.SelectedValue,
          BirimId = (int)cmbBirim.SelectedValue,
          Fiyat = numFiyat.Value,
          MinimumStok = (int)numMinStok.Value,
          MaksimumStok = (int)numMaksStok.Value,
          DepoId = _selectedDepoId,
          Miktar = (int)numMiktar.Value,
          AktifMi = true
        };

        OperationResult result;

        if (_isEditMode)
        {
          string neden = null;
          int mevcutMiktar = _stokService.GetStok(model.UrunId, model.DepoId)?.Miktar ?? 0;
          if (model.Miktar != mevcutMiktar)
          {
            using (var nedenFormu = new StokDuzeltmeNedeniForm())
            {
              if (nedenFormu.ShowDialog(this) != DialogResult.OK) return;
              neden = nedenFormu.Neden;
            }
          }
          result = _urunService.UpdateProduct(model, neden);
        }
        else
        {
          result = _urunService.AddProduct(model);
        }

        if (result.Success)
        {
          UiMessage.Show(result.Message, UiText.Get("Success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
          this.DialogResult = DialogResult.OK;
          this.Close();
        }
        else
        {
          UiMessage.Show(result.Message, UiText.Get("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
      }
      catch (Exception ex)
      {
        UiMessage.Show(UiText.Format("OperationException", ex.Message), UiText.Get("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void BtnIptal_Click(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.Cancel;
      this.Close();
    }

    private void AddEditProductForm_Load_1(object sender, EventArgs e)
    {

    }
  }
}
