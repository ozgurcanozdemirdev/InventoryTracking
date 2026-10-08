using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static InventoryTracking.Business.Services;
using static InventoryTracking.Models.Entities;

namespace InventoryTracking.UI
{
  public class StokTransferForm : Form
  {
    private readonly StokService _stokService = new StokService();
    private readonly DepoService _depoService = new DepoService();
    private readonly UrunService _urunService = new UrunService();
    private readonly int _ilkDepoId;
    private readonly int _ilkUrunId;
    private readonly ComboBox cmbUrun = new ComboBox();
    private readonly ComboBox cmbKaynak = new ComboBox();
    private readonly ComboBox cmbHedef = new ComboBox();
    private readonly NumericUpDown numMiktar = new NumericUpDown();
    private readonly TextBox txtAciklama = new TextBox();
    private readonly Label lblMevcut = new Label();

    public StokTransferForm(int kaynakDepoId, int urunId = 0)
    {
      _ilkDepoId = kaynakDepoId;
      _ilkUrunId = urunId;
      Text = "Depolar Arası Stok Transferi";
      StartPosition = FormStartPosition.CenterParent;
      FormBorderStyle = FormBorderStyle.FixedDialog;
      MaximizeBox = false;
      MinimizeBox = false;
      ClientSize = new Size(600, 370);
      BackColor = Color.White;
      Font = new Font("Segoe UI", 10F);

      var header = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.FromArgb(41, 128, 185) };
      header.Controls.Add(new Label
      {
        Text = "DEPOLAR ARASI STOK TRANSFERİ", ForeColor = Color.White,
        Font = new Font("Segoe UI", 14F, FontStyle.Bold), Dock = DockStyle.Fill,
        Padding = new Padding(15, 9, 0, 0)
      });
      Controls.Add(header);

      AddRow("Ürün:", cmbUrun, 67);
      AddRow("Kaynak Depo:", cmbKaynak, 112);
      AddRow("Hedef Depo:", cmbHedef, 157);
      AddRow("Miktar:", numMiktar, 202);
      AddRow("Açıklama:", txtAciklama, 257);
      foreach (var combo in new[] { cmbUrun, cmbKaynak, cmbHedef })
        combo.DropDownStyle = ComboBoxStyle.DropDownList;
      numMiktar.Minimum = 1;
      numMiktar.Maximum = 1000000;
      txtAciklama.MaxLength = 500;
      lblMevcut.Text = "Kaynak stok: -";
      lblMevcut.Location = new Point(155, 236);
      lblMevcut.Size = new Size(400, 22);
      Controls.Add(lblMevcut);

      var footer = new Panel { Dock = DockStyle.Bottom, Height = 60, BackColor = Color.FromArgb(236, 240, 241) };
      var btnAktar = MakeButton("✓ Transfer Et", 305, Color.FromArgb(46, 204, 113));
      var btnIptal = MakeButton("İptal", 455, Color.FromArgb(149, 165, 166));
      btnAktar.Click += (sender, e) => Transfer();
      btnIptal.Click += (sender, e) => { DialogResult = DialogResult.Cancel; Close(); };
      footer.Controls.Add(btnAktar);
      footer.Controls.Add(btnIptal);
      Controls.Add(footer);
      AcceptButton = btnAktar;
      CancelButton = btnIptal;
      cmbKaynak.SelectedIndexChanged += (sender, e) => RefreshStock();
      cmbUrun.SelectedIndexChanged += (sender, e) => RefreshStock();
      Shown += (sender, e) => LoadLists();
    }

    private void AddRow(string text, Control control, int top)
    {
      Controls.Add(new Label { Text = text, Location = new Point(20, top + 4), Size = new Size(125, 25) });
      control.Location = new Point(155, top);
      control.Size = new Size(420, 29);
      Controls.Add(control);
    }

    private static Button MakeButton(string text, int left, Color color)
    {
      var button = new Button
      {
        Text = text, Location = new Point(left, 10), Size = new Size(130, 40),
        BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
        UseVisualStyleBackColor = false, Font = new Font("Segoe UI", 10F, FontStyle.Bold)
      };
      button.FlatAppearance.BorderSize = 0;
      return button;
    }

    private void LoadLists()
    {
      try
      {
        var depolar = _depoService.GetAllActiveDepolar();
        var urunler = _urunService.GetAllProducts();
        if (depolar.Count < 2 || urunler.Count == 0)
        {
          MessageBox.Show("Transfer için en az iki aktif depo ve bir ürün gerekir.", "Uyarı",
            MessageBoxButtons.OK, MessageBoxIcon.Warning);
          Close();
          return;
        }
        cmbKaynak.DataSource = depolar;
        cmbKaynak.DisplayMember = "DepoAdi";
        cmbKaynak.ValueMember = "DepoId";
        cmbHedef.DataSource = _depoService.GetAllActiveDepolar();
        cmbHedef.DisplayMember = "DepoAdi";
        cmbHedef.ValueMember = "DepoId";
        cmbUrun.DataSource = urunler;
        cmbUrun.DisplayMember = "UrunAdi";
        cmbUrun.ValueMember = "UrunId";
        if (depolar.Any(d => d.DepoId == _ilkDepoId)) cmbKaynak.SelectedValue = _ilkDepoId;
        if (_ilkUrunId > 0 && urunler.Any(u => u.UrunId == _ilkUrunId)) cmbUrun.SelectedValue = _ilkUrunId;
        var hedef = depolar.FirstOrDefault(d => d.DepoId != (int)cmbKaynak.SelectedValue);
        if (hedef != null) cmbHedef.SelectedValue = hedef.DepoId;
        RefreshStock();
      }
      catch (Exception ex)
      {
        MessageBox.Show("Transfer bilgileri yüklenemedi: " + ex.Message, "Hata",
          MessageBoxButtons.OK, MessageBoxIcon.Error);
        Close();
      }
    }

    private void RefreshStock()
    {
      if (!(cmbUrun.SelectedValue is int urunId) || !(cmbKaynak.SelectedValue is int depoId)) return;
      try
      {
        int miktar = _stokService.GetStok(urunId, depoId)?.Miktar ?? 0;
        lblMevcut.Text = "Kaynak stok: " + miktar;
      }
      catch (Exception ex) { lblMevcut.Text = "Stok okunamadı: " + ex.Message; }
    }

    private void Transfer()
    {
      if (!(cmbUrun.SelectedValue is int urunId) ||
          !(cmbKaynak.SelectedValue is int kaynak) || !(cmbHedef.SelectedValue is int hedef)) return;
      if (kaynak == hedef)
      {
        MessageBox.Show("Kaynak ve hedef depo farklı olmalıdır.", "Uyarı",
          MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
      }
      if (string.IsNullOrWhiteSpace(txtAciklama.Text))
      {
        MessageBox.Show("Transfer açıklaması zorunludur.", "Uyarı",
          MessageBoxButtons.OK, MessageBoxIcon.Warning);
        txtAciklama.Focus();
        return;
      }
      var result = _stokService.TransferStock(urunId, kaynak, hedef, (int)numMiktar.Value, txtAciklama.Text);
      MessageBox.Show(result.Message, result.Success ? "Başarılı" : "Transfer Yapılamadı",
        MessageBoxButtons.OK, result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
      if (result.Success) { DialogResult = DialogResult.OK; Close(); }
      else RefreshStock();
    }
  }
}
