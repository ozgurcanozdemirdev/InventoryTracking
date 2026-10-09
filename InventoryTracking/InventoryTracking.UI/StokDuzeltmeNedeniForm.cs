using InventoryTracking.Localization;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace InventoryTracking.UI
{
  public class StokDuzeltmeNedeniForm : LocalizedForm
  {
    private readonly TextBox txtNeden = new TextBox();
    public string Neden { get; private set; }

    public StokDuzeltmeNedeniForm()
    {
      Text = UiText.Get("AdjustmentTitle");
      StartPosition = FormStartPosition.CenterParent;
      FormBorderStyle = FormBorderStyle.FixedDialog;
      MaximizeBox = false;
      MinimizeBox = false;
      ClientSize = new Size(460, 205);
      BackColor = Color.White;
      Font = new Font("Segoe UI", 10F);

      var header = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = Color.FromArgb(41, 128, 185) };
      header.Controls.Add(new Label
      {
        Text = UiText.Get("AdjustmentHeader"), ForeColor = Color.White,
        Font = new Font("Segoe UI", 12F, FontStyle.Bold), Dock = DockStyle.Fill,
        Padding = new Padding(15, 11, 0, 0)
      });
      Controls.Add(header);

      Controls.Add(new Label
      {
        Text = UiText.Get("AdjustmentReasonPrompt"),
        Location = new Point(18, 61), Size = new Size(420, 25)
      });
      txtNeden.Location = new Point(18, 90);
      txtNeden.Size = new Size(420, 43);
      txtNeden.Multiline = true;
      txtNeden.MaxLength = 500;
      Controls.Add(txtNeden);

      var btnKaydet = new Button
      {
        Text = UiText.Get("Continue"), Location = new Point(185, 150), Size = new Size(120, 38),
        BackColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White,
        FlatStyle = FlatStyle.Flat, UseVisualStyleBackColor = false
      };
      var btnIptal = new Button
      {
        Text = UiText.Get("Cancel"), Location = new Point(320, 150), Size = new Size(120, 38),
        BackColor = Color.FromArgb(149, 165, 166), ForeColor = Color.White,
        FlatStyle = FlatStyle.Flat, UseVisualStyleBackColor = false
      };
      btnKaydet.FlatAppearance.BorderSize = 0;
      btnIptal.FlatAppearance.BorderSize = 0;
      btnKaydet.Click += (sender, e) =>
      {
        if (string.IsNullOrWhiteSpace(txtNeden.Text))
        {
          UiMessage.Show(UiText.Get("ReasonRequired"), UiText.Get("Warning"),
            MessageBoxButtons.OK, MessageBoxIcon.Warning);
          txtNeden.Focus();
          return;
        }
        Neden = txtNeden.Text.Trim();
        DialogResult = DialogResult.OK;
        Close();
      };
      btnIptal.Click += (sender, e) => { DialogResult = DialogResult.Cancel; Close(); };
      Controls.Add(btnKaydet);
      Controls.Add(btnIptal);
      AcceptButton = btnKaydet;
      CancelButton = btnIptal;
    }
  }
}
