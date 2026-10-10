using InventoryTracking.UI.Localization;
using System;
using System.Drawing;
using System.Windows.Forms;
using InventoryTracking.Business;
using InventoryTracking.Models;

namespace InventoryTracking.UI.Forms
{
  public class WarehouseManagementForm : LocalizedForm
  {
    private readonly WarehouseService _service = new WarehouseService();
    private readonly DataGridView dgvDepolar = new DataGridView();
    private readonly TextBox txtAd = new TextBox();
    private readonly TextBox txtLokasyon = new TextBox();
    private readonly TextBox txtAciklama = new TextBox();

    public WarehouseManagementForm()
    {
      Text = UiText.Get("WarehouseManagement");
      StartPosition = FormStartPosition.CenterParent;
      FormBorderStyle = FormBorderStyle.FixedDialog;
      MaximizeBox = false;
      MinimizeBox = false;
      ClientSize = new Size(780, 540);
      BackColor = Color.White;
      Font = new Font("Segoe UI", 10F);

      var header = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.FromArgb(41, 128, 185) };
      header.Controls.Add(new Label
      {
        Text = UiText.Get("WarehouseManagementHeader"), ForeColor = Color.White,
        Font = new Font("Segoe UI", 14F, FontStyle.Bold), Dock = DockStyle.Fill,
        Padding = new Padding(15, 9, 0, 0)
      });

      dgvDepolar.Dock = DockStyle.Fill;
      dgvDepolar.BackgroundColor = Color.White;
      dgvDepolar.BorderStyle = BorderStyle.None;
      dgvDepolar.ReadOnly = true;
      dgvDepolar.AllowUserToAddRows = false;
      dgvDepolar.AllowUserToDeleteRows = false;
      dgvDepolar.AutoGenerateColumns = false;
      dgvDepolar.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
      dgvDepolar.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
      dgvDepolar.MultiSelect = false;
      dgvDepolar.RowHeadersVisible = false;
      dgvDepolar.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = UiText.Get("WarehouseBranch"), DataPropertyName = "DepoAdi" });
      dgvDepolar.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = UiText.Get("WarehouseLocationHeader"), DataPropertyName = "Lokasyon" });
      dgvDepolar.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = UiText.Get("Status"), DataPropertyName = "Durum" });
      dgvDepolar.SelectionChanged += (sender, e) => FillSelected();

      var editor = new Panel { Dock = DockStyle.Bottom, Height = 173, BackColor = Color.FromArgb(236, 240, 241) };
      AddRow(editor, UiText.Get("WarehouseNameLabel"), txtAd, 12, 100);
      AddRow(editor, UiText.Get("WarehouseLocationLabel"), txtLokasyon, 48, 200);
      AddRow(editor, UiText.Get("DescriptionLabel"), txtAciklama, 84, 500);

      var btnEkle = MakeButton(UiText.Get("AddAction"), 15, Color.FromArgb(46, 204, 113));
      var btnGuncelle = MakeButton(UiText.Get("UpdateAction"), 166, Color.FromArgb(52, 152, 219));
      var btnPasif = MakeButton(UiText.Get("DeleteDeactivate"), 317, Color.FromArgb(231, 76, 60));
      var btnAktif = MakeButton(UiText.Get("Activate"), 468, Color.FromArgb(155, 89, 182));
      var btnKapat = MakeButton(UiText.Get("Close"), 619, Color.FromArgb(149, 165, 166));
      btnEkle.Click += (sender, e) => ShowResult(_service.AddDepo(txtAd.Text, txtLokasyon.Text, txtAciklama.Text));
      btnGuncelle.Click += (sender, e) => UpdateSelected();
      btnPasif.Click += (sender, e) => SetSelectedActive(false);
      btnAktif.Click += (sender, e) => SetSelectedActive(true);
      btnKapat.Click += (sender, e) => Close();
      editor.Controls.AddRange(new Control[] { btnEkle, btnGuncelle, btnPasif, btnAktif, btnKapat });

      Controls.Add(dgvDepolar);
      Controls.Add(editor);
      Controls.Add(header);
      Shown += (sender, e) => LoadDepolar();
    }

    private static void AddRow(Panel panel, string title, TextBox input, int top, int maxLength)
    {
      panel.Controls.Add(new Label { Text = title, Location = new Point(15, top + 3), AutoSize = true });
      input.Location = new Point(145, top); input.Size = new Size(610, 25); input.MaxLength = maxLength;
      panel.Controls.Add(input);
    }

    private static Button MakeButton(string title, int left, Color color)
    {
      return new Button
      {
        Text = title, Location = new Point(left, 124), Size = new Size(136, 40),
        BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
        Font = new Font("Segoe UI", 10F, FontStyle.Bold)
      };
    }

    private Warehouse SelectedDepot()
    {
      return dgvDepolar.SelectedRows.Count > 0
        ? dgvDepolar.SelectedRows[0].DataBoundItem as Warehouse : null;
    }

    private void LoadDepolar()
    {
      try
      {
        dgvDepolar.DataSource = null;
        dgvDepolar.DataSource = _service.GetAllDepolar();
        dgvDepolar.ClearSelection();
        txtAd.Clear(); txtLokasyon.Clear(); txtAciklama.Clear();
      }
      catch (Exception ex)
      {
        UiMessage.Show(UiText.Get("WarehousesLoadError") + ex.Message, UiText.Get("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void FillSelected()
    {
      var depot = SelectedDepot();
      if (depot == null) return;
      txtAd.Text = depot.DepoAdi;
      txtLokasyon.Text = depot.Lokasyon;
      txtAciklama.Text = depot.Aciklama;
    }

    private void UpdateSelected()
    {
      var depot = SelectedDepot();
      if (depot == null) { UiMessage.Show(UiText.Get("PleaseWarehouse")); return; }
      ShowResult(_service.UpdateDepo(depot.DepoId, txtAd.Text, txtLokasyon.Text, txtAciklama.Text));
    }

    private void SetSelectedActive(bool active)
    {
      var depot = SelectedDepot();
      if (depot == null) { UiMessage.Show(UiText.Get("PleaseWarehouse")); return; }
      if (depot.AktifMi == active) { UiMessage.Show(UiText.Get("WarehouseSameStatus")); return; }
      if (!active && UiMessage.Show(UiText.Format("WarehouseDeactivatePrompt", depot.DepoAdi), UiText.Get("DeleteConfirmation"),
          MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
      ShowResult(_service.SetDepoActive(depot.DepoId, active));
    }

    private void ShowResult(OperationResult result)
    {
      UiMessage.Show(result.Message, result.Success ? UiText.Get("Success") : UiText.Get("OperationFailed"),
        MessageBoxButtons.OK, result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
      if (result.Success) LoadDepolar();
    }
  }
}
