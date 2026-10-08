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
    }

    //bool formloading = false;

    private void MainForm_Load(object sender, EventArgs e)
    {
      _isFormLoading = true; // Yükleme başladı

      InitializeControls();
      LoadDepolar();
      LoadStokList();

      _isFormLoading = false; // Yükleme bitti, artık kullanıcı etkileşimlerine açık
    }

    private void InitializeControls()
    {
      // Form ayarları
      this.Text = "Stok Takip Sistemi";
      this.StartPosition = FormStartPosition.CenterScreen;
      this.WindowState = FormWindowState.Maximized;

      // DataGridView ayarları
      dgvStoklar.AutoGenerateColumns = false;
      dgvStoklar.AllowUserToAddRows = false;
      dgvStoklar.AllowUserToDeleteRows = false;
      dgvStoklar.AllowUserToOrderColumns = false;
      dgvStoklar.ReadOnly = true;
      dgvStoklar.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
      dgvStoklar.MultiSelect = false;

      ConfigureDataGridViewColumns();

      // Var olan butonların konumunu değiştirmeden boş alana iki yönetim girişi ekliyoruz.
      _btnKategoriYonet = MakeManagementButton("🏷️ Kategoriler", 661, System.Drawing.Color.FromArgb(52, 152, 219));
      _btnDepoYonet = MakeManagementButton("🏬 Depolar", 791, System.Drawing.Color.FromArgb(155, 89, 182));
      _btnTransfer = MakeManagementButton("⇄ Transfer", 921, System.Drawing.Color.FromArgb(230, 126, 34));
      _btnKategoriYonet.Click += (sender, e) =>
      {
        using (var form = new KategoriYonetimForm()) form.ShowDialog(this);
        LoadStokList();
      };
      _btnDepoYonet.Click += (sender, e) =>
      {
        using (var form = new DepoYonetimForm()) form.ShowDialog(this);
        _isFormLoading = true;
        LoadDepolar(_selectedDepoId);
        _isFormLoading = false;
        LoadStokList();
      };
      _btnTransfer.Click += (sender, e) =>
      {
        var selected = dgvStoklar.SelectedRows.Count > 0
          ? dgvStoklar.SelectedRows[0].DataBoundItem as Entities.Stok : null;
        using (var form = new StokTransferForm(_selectedDepoId, selected?.UrunId ?? 0))
        {
          if (form.ShowDialog(this) == DialogResult.OK) LoadStokList();
        }
      };
      pnlButtons.Controls.Add(_btnKategoriYonet);
      pnlButtons.Controls.Add(_btnDepoYonet);
      pnlButtons.Controls.Add(_btnTransfer);

      _btnBaglanti = MakeManagementButton("Bağlantı Ayarları", 350, System.Drawing.Color.FromArgb(52, 152, 219));
      _btnBaglanti.Location = new System.Drawing.Point(350, 32);
      _btnBaglanti.Size = new System.Drawing.Size(160, 32);
      _btnBaglanti.Click += (sender, e) =>
      {
        using (var form = new ConnectionSetupForm())
        {
          if (form.ShowDialog(this) != DialogResult.OK) return;
        }
        _isFormLoading = true;
        LoadDepolar();
        _isFormLoading = false;
        LoadStokList();
      };
      pnlFilters.Controls.Add(_btnBaglanti);

      // TextBox - Barkod okuma
      txtBarkod.Focus();

      // Çift tetiklenmeyi (Memory Leak / Double Fire) engellemek için defansif abonelik.
      txtBarkod.KeyPress -= txtBarkod_KeyPress;
      txtBarkod.KeyPress += txtBarkod_KeyPress;

      dgvStoklar.CellFormatting -= DgvStoklar_CellFormatting;
      dgvStoklar.CellFormatting += DgvStoklar_CellFormatting;
      dgvStoklar.CellDoubleClick -= DgvStoklar_CellDoubleClick;
      dgvStoklar.CellDoubleClick += DgvStoklar_CellDoubleClick;
    }

    private static Button MakeManagementButton(string title, int left, System.Drawing.Color color)
    {
      var button = new Button
      {
        Text = title,
        Location = new System.Drawing.Point(left, 10),
        Size = new System.Drawing.Size(120, 40),
        BackColor = color,
        ForeColor = System.Drawing.Color.White,
        FlatStyle = FlatStyle.Flat,
        Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold),
        UseVisualStyleBackColor = false
      };
      button.FlatAppearance.BorderSize = 0;
      return button;
    }

    private void ConfigureDataGridViewColumns()
    {
      #region Eski kod
      /*
       dgvStoklar.Columns.Clear();

      dgvStoklar.Columns.Add("Barkod", "Barkod");
      dgvStoklar.Columns.Add("UrunAdi", "Ürün Adı");
      dgvStoklar.Columns.Add("KategoriAdi", "Kategori");
      dgvStoklar.Columns.Add("BirimAdi", "Birim");
      dgvStoklar.Columns.Add("Fiyat", "Fiyat");
      dgvStoklar.Columns.Add("Miktar", "Miktar");
      dgvStoklar.Columns.Add("StokDurumu", "Durum");
      dgvStoklar.Columns.Add("GuncellemeTarihi", "Son Güncelleme");

      // Sütun genişlikleri
      dgvStoklar.Columns["Barkod"].Width = 120;
      dgvStoklar.Columns["UrunAdi"].Width = 200;
      dgvStoklar.Columns["KategoriAdi"].Width = 100;
      dgvStoklar.Columns["BirimAdi"].Width = 80;
      dgvStoklar.Columns["Fiyat"].Width = 80;
      dgvStoklar.Columns["Miktar"].Width = 80;
      dgvStoklar.Columns["StokDurumu"].Width = 80;
      dgvStoklar.Columns["GuncellemeTarihi"].Width = 130;

      // Fiyat ve Miktar sütunlarını sağa hizala
      dgvStoklar.Columns["Fiyat"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
      dgvStoklar.Columns["Miktar"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

      */
      #endregion

      // Sütunlar oluşturulurken DataPropertyName özellikleri modelinizle eşleştirilerek atanıyor
      dgvStoklar.Columns.Add(new DataGridViewTextBoxColumn { Name = "Barkod", HeaderText = "Barkod", DataPropertyName = "Barkod" });
      dgvStoklar.Columns.Add(new DataGridViewTextBoxColumn { Name = "UrunAdi", HeaderText = "Ürün Adı", DataPropertyName = "UrunAdi" });
      dgvStoklar.Columns.Add(new DataGridViewTextBoxColumn { Name = "KategoriAdi", HeaderText = "Kategori", DataPropertyName = "KategoriAdi" });
      dgvStoklar.Columns.Add(new DataGridViewTextBoxColumn { Name = "BirimAdi", HeaderText = "Birim", DataPropertyName = "BirimAdi" });
      dgvStoklar.Columns.Add(new DataGridViewTextBoxColumn { Name = "Fiyat", HeaderText = "Fiyat", DataPropertyName = "Fiyat" });
      dgvStoklar.Columns.Add(new DataGridViewTextBoxColumn { Name = "Miktar", HeaderText = "Miktar", DataPropertyName = "Miktar" });
      dgvStoklar.Columns.Add(new DataGridViewTextBoxColumn { Name = "StokDurumu", HeaderText = "Durum", DataPropertyName = "StokDurumu" });
      dgvStoklar.Columns.Add(new DataGridViewTextBoxColumn { Name = "GuncellemeTarihi", HeaderText = "Son Güncelleme", DataPropertyName = "GuncellemeTarihi" });
      //
      // Sütun genişlikleri
      dgvStoklar.Columns["Barkod"].Width = 120;
      dgvStoklar.Columns["UrunAdi"].Width = 200;
      dgvStoklar.Columns["KategoriAdi"].Width = 100;
      dgvStoklar.Columns["BirimAdi"].Width = 80;
      dgvStoklar.Columns["Fiyat"].Width = 80;
      dgvStoklar.Columns["Miktar"].Width = 80;
      dgvStoklar.Columns["StokDurumu"].Width = 80;
      dgvStoklar.Columns["GuncellemeTarihi"].Width = 130;

      // Fiyat ve Miktar sütunlarını sağa hizala
      dgvStoklar.Columns["Fiyat"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
      dgvStoklar.Columns["Miktar"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
    }


    private void LoadDepolar(int preferredDepoId = 0)
    {
      try
      {
        var depolar = _depoService.GetAllActiveDepolar();

        if (depolar == null || depolar.Count == 0)
        {
          cmbDepo.DataSource = null;
          MessageBox.Show("Aktif depo bulunamadı. Depolar butonundan yeni depo ekleyiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
          return;
        }

        var bindingSource = new System.Windows.Forms.BindingSource();
        bindingSource.DataSource = depolar;

        cmbDepo.DataSource = bindingSource;
        cmbDepo.DisplayMember = "DepoAdi";
        cmbDepo.ValueMember = "DepoId";

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
      try
      {
        // 1. GÜVENLİ TİP DÖNÜŞÜMÜ (Cast hatasını engellemek için)
        int seciliDepoId = 0;
        if (cmbDepo.SelectedValue != null)
        {
          // SelectedValue'yu string'e çevirip, sayıya dönüşüp dönüşemediğine bakıyoruz
          int.TryParse(cmbDepo.SelectedValue.ToString(), out seciliDepoId);
        }

        // Depo seçili değilse veya ID <= 0 ise grid'i boşaltıp çık
        if (seciliDepoId <= 0)
        {
          dgvStoklar.DataSource = null;
          return;
        }

        _selectedDepoId = seciliDepoId;

        // 2. Stokları yükle
        var stoklar = _stokService.GetStokByDepo(_selectedDepoId);

        // 3. Veri bağlantısı (Eski kalıntıları silip taze listeyi veriyoruz)
        dgvStoklar.DataSource = null;

        // Eğer DataGridView'a sütunları tasarım ekranından (elle) eklediyseniz 
        // AutoGenerateColumns'ı false yapmalısınız ki yan yana ekstra sütunlar oluşmasın.
        dgvStoklar.AutoGenerateColumns = false;

        dgvStoklar.DataSource = stoklar;

        // 4. Stok durumuna göre renklendirme (Eğer veri varsa çalıştır)
        //if (stoklar != null && stoklar.Count > 0)
        //{
        //  ApplyStockColorCoding();
        //}

        // 5. Status bar'da bilgi göster
        int urunSayisi = stoklar != null ? stoklar.Count : 0;
        toolStripStatusLabel.Text = $"Toplam {urunSayisi} ürün yüklendi - Depo: {cmbDepo.Text} | Stok işlemi için ürüne çift tıklayın";
      }
      catch (Exception ex)
      {
        dgvStoklar.DataSource = null;
        MessageBox.Show($"Stoklar yüklenirken hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        toolStripStatusLabel.Text = "Hata oluştu!";
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
          if (editForm.ShowDialog() == DialogResult.OK)
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
      LoadStokList();
      txtBarkod.Clear();
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
      string barkod = dgvStoklar.Rows[e.RowIndex].Cells["Barkod"].Value?.ToString();
      if (string.IsNullOrWhiteSpace(barkod)) return;
      try
      {
        var urun = _urunService.SearchByBarkod(barkod);
        if (urun == null)
        {
          MessageBox.Show("Ürün bulunamadı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
          LoadStokList();
          return;
        }
        using (var detailForm = new ProductDetailForm(urun, _selectedDepoId, _stokService))
        {
          if (detailForm.ShowDialog(this) == DialogResult.OK) LoadStokList();
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show("Stok işlem ekranı açılamadı: " + ex.Message, "Hata",
          MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void DgvStoklar_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
    {
      // Sadece "StokDurumu" sütununu boya
      if (dgvStoklar.Columns[e.ColumnIndex].Name == "StokDurumu" && e.Value != null)
      {
        string durum = e.Value.ToString();

        switch (durum)
        {
          case "Kritik":
            e.CellStyle.BackColor = System.Drawing.Color.FromArgb(255, 200, 200);
            e.CellStyle.ForeColor = System.Drawing.Color.DarkRed;
            break;
          case "Düşük":
            e.CellStyle.BackColor = System.Drawing.Color.FromArgb(255, 255, 200);
            e.CellStyle.ForeColor = System.Drawing.Color.Orange;
            break;
          case "Normal":
            e.CellStyle.BackColor = System.Drawing.Color.FromArgb(200, 255, 200);
            e.CellStyle.ForeColor = System.Drawing.Color.DarkGreen;
            break;
        }
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
