using InventoryTracking.Localization;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using static InventoryTracking.Business.Services;
using static InventoryTracking.Models.Entities;

namespace InventoryTracking.UI
{
  /// <summary>
  /// Ürün açılmadan önce barkodun yetkili malzeme kataloğuna kaydedildiği ekran.
  /// </summary>
  public class MalzemeKayitForm : LocalizedForm
  {
    private readonly TextBox txtMalzemeNo = new TextBox();
    private readonly TextBox txtBarkod = new TextBox();
    private readonly TextBox txtAciklama = new TextBox();
    private readonly ComboBox cmbBirim = new ComboBox();
    private readonly MalzemeService _malzemeService = new MalzemeService();
    private readonly BirimService _birimService = new BirimService();
    private readonly MalzemeKatalog _editing;

    public MalzemeKatalog KaydedilenMalzeme { get; private set; }

    public MalzemeKayitForm(string barkod = "")
    {
      Text = UiText.Get("NewMaterial");
      StartPosition = FormStartPosition.CenterParent;
      FormBorderStyle = FormBorderStyle.FixedDialog;
      MaximizeBox = false;
      MinimizeBox = false;
      ClientSize = new Size(440, 268);
      Font = new Font("Segoe UI", 10F);

      AddRow(UiText.Get("MaterialNumberLabel"), txtMalzemeNo, 20);
      AddRow("Barkod:", txtBarkod, 65);
      AddRow(UiText.Get("DescriptionLabel"), txtAciklama, 110);
      AddRow("Birim:", cmbBirim, 155);
      txtMalzemeNo.MaxLength = 50;
      txtBarkod.MaxLength = 50;
      txtAciklama.MaxLength = 200;
      txtBarkod.Text = barkod?.Trim() ?? "";
      cmbBirim.DropDownStyle = ComboBoxStyle.DropDownList;

      var btnKaydet = new Button { Text = UiText.Get("Save"), Location = new Point(170, 215), Size = new Size(115, 36) };
      var btnIptal = new Button { Text = UiText.Get("Cancel"), Location = new Point(300, 215), Size = new Size(115, 36) };
      btnKaydet.Click += BtnKaydet_Click;
      btnIptal.Click += (sender, e) => { DialogResult = DialogResult.Cancel; Close(); };
      Controls.Add(btnKaydet);
      Controls.Add(btnIptal);
      AcceptButton = btnKaydet;
      CancelButton = btnIptal;

      Shown += (sender, e) => LoadBirimler();
    }

    public MalzemeKayitForm(MalzemeKatalog malzeme) : this(malzeme?.Barkod)
    {
      _editing = malzeme ?? throw new ArgumentNullException(nameof(malzeme));
      Text = UiText.Get("EditMaterial");
      txtMalzemeNo.Text = malzeme.MalzemeNo;
      txtAciklama.Text = malzeme.Aciklama;
      txtBarkod.ReadOnly = true;
    }

    private void AddRow(string title, Control input, int top)
    {
      Controls.Add(new Label { Text = title, AutoSize = true, Location = new Point(18, top + 4) });
      input.Location = new Point(155, top);
      input.Size = new Size(260, 29);
      Controls.Add(input);
    }

    private void LoadBirimler()
    {
      try
      {
        cmbBirim.DataSource = _birimService.GetAllBirimler() ?? new List<Birim>();
        cmbBirim.DisplayMember = "BirimAdi";
        cmbBirim.ValueMember = "BirimId";
        if (_editing != null) cmbBirim.SelectedValue = _editing.BirimId;
        txtMalzemeNo.Focus();
      }
      catch (Exception ex)
      {
        UiMessage.Show(UiText.Get("UnitsLoadError") + ex.Message, UiText.Get("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void BtnKaydet_Click(object sender, EventArgs e)
    {
      if (!(cmbBirim.SelectedValue is int birimId))
      {
        UiMessage.Show(UiText.Get("SelectUnitDot"), UiText.Get("Warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
      }

      var malzeme = new MalzemeKatalog
      {
        MalzemeId = _editing?.MalzemeId ?? 0,
        MalzemeNo = txtMalzemeNo.Text.Trim(),
        Barkod = txtBarkod.Text.Trim(),
        Aciklama = txtAciklama.Text.Trim(),
        BirimId = birimId
      };

      // UNIQUE kısıtları son güvencedir; aynı barkodu başka kullanıcı kaydederse hata gösterilir.
      var result = _editing == null ? _malzemeService.Add(malzeme) : _malzemeService.Update(malzeme);
      if (!result.Success)
      {
        UiMessage.Show(result.Message, UiText.Get("MaterialSaveFailed"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
      }
      KaydedilenMalzeme = malzeme;
      DialogResult = DialogResult.OK;
      Close();
    }
  }
}
