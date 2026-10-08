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
  public partial class ProductDetailForm : Form
  {
    private readonly Urun _urun;
    private readonly int _depoId;
    private readonly StokService _stokService;
    private readonly bool? _girisMi;
    private int _currentStockAmount = 0;
    public ProductDetailForm(Urun urun, int depoId, StokService stokService)
      : this(urun, depoId, stokService, null)
    {
    }

    public ProductDetailForm(Urun urun, int depoId, StokService stokService, bool? girisMi)
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
      this.Text = _girisMi.HasValue ? (_girisMi.Value ? "Stok Girişi" : "Stok Çıkışı") : "Ürün Detayı";
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
        lblFiyatValue.Text = _urun.Fiyat.ToString("C2");
        lblMinStokValue.Text = _urun.MinimumStok.ToString();
        lblMaksStokValue.Text = _urun.MaksimumStok.ToString();

        // Depo adını göster
        var depoService = new DepoService();
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
        MessageBox.Show($"Ürün bilgileri yüklenirken hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
          lblDurumValue.Text = stok.StokDurumu;

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
          lblDurumValue.Text = "Kritik";
          lblDurumValue.ForeColor = System.Drawing.Color.DarkRed;
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show($"Stok bilgisi yüklenirken hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void ApplyStatusColor()
    {
      var status = _stokService.GetStockStatus(_urun.UrunId, _depoId);

      switch (status)
      {
        case StokDurumuTipi.Kritik:
          pnlStatus.BackColor = System.Drawing.Color.FromArgb(255, 200, 200);
          break;
        case StokDurumuTipi.Düşük:
          pnlStatus.BackColor = System.Drawing.Color.FromArgb(255, 255, 200);
          break;
        case StokDurumuTipi.Normal:
          pnlStatus.BackColor = System.Drawing.Color.FromArgb(200, 255, 200);
          break;
      }
    }

    private void btnGiris_Click(object sender, EventArgs e)
    {
      if (numMiktar.Value <= 0)
      {
        MessageBox.Show("Lütfen giriş yapılacak miktarı giriniz", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        numMiktar.Focus();
        return;
      }

      int miktar = (int)numMiktar.Value;
      _currentStockAmount = _stokService.GetStok(_urun.UrunId, _depoId)?.Miktar ?? 0;
      int yeniMiktar = _currentStockAmount + miktar;

      if (yeniMiktar > _urun.MaksimumStok)
      {
        DialogResult result = MessageBox.Show(
            $"Yeni miktar ({yeniMiktar}) maksimum stok ({_urun.MaksimumStok}) değerini aşacaktır.\nDevam etmek istiyor musunuz?",
            "Uyarı",
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
        MessageBox.Show($"Stok başarıyla güncellendi.\nYeni Miktar: {yeniMiktar}", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
        this.DialogResult = DialogResult.OK;
        this.Close();
      }
      else
      {
        MessageBox.Show(result_op.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void btnCikis_Click(object sender, EventArgs e)
    {
      if (numMiktar.Value <= 0)
      {
        MessageBox.Show("Lütfen çıkış yapılacak miktarı giriniz", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        numMiktar.Focus();
        return;
      }

      int miktar = (int)numMiktar.Value;

      string aciklama = txtAciklama.Text.Trim();
      var result = _stokService.UpdateStok(_urun.UrunId, _depoId, -miktar, "Çıkış", aciklama);

      if (result.Success)
      {
        int yeniMiktar = _stokService.GetStok(_urun.UrunId, _depoId)?.Miktar ?? 0;
        MessageBox.Show($"Stok başarıyla güncellendi.\nYeni Miktar: {yeniMiktar}", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
        this.DialogResult = DialogResult.OK;
        this.Close();
      }
      else
      {
        MessageBox.Show(result.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void btnKapat_Click(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.Cancel;
      this.Close();
    }
  }
}
