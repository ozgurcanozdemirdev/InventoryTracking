using System;
using System.Drawing;
using System.Windows.Forms;
using static InventoryTracking.Business.Services;
using static InventoryTracking.Models.Entities;

namespace InventoryTracking.UI
{
  public class KategoriYonetimForm : Form
  {
    private readonly KategoriService _service = new KategoriService();
    private readonly DataGridView dgvKategoriler = new DataGridView();
    private readonly TextBox txtAd = new TextBox();
    private readonly TextBox txtAciklama = new TextBox();

    public KategoriYonetimForm()
    {
      Text = "Kategori Yönetimi";
      StartPosition = FormStartPosition.CenterParent;
      FormBorderStyle = FormBorderStyle.FixedDialog;
      MaximizeBox = false;
      MinimizeBox = false;
      ClientSize = new Size(740, 490);
      BackColor = Color.White;
      Font = new Font("Segoe UI", 10F);

      var header = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.FromArgb(41, 128, 185) };
      header.Controls.Add(new Label
      {
        Text = "KATEGORİ YÖNETİMİ", ForeColor = Color.White,
        Font = new Font("Segoe UI", 14F, FontStyle.Bold), Dock = DockStyle.Fill,
        Padding = new Padding(15, 9, 0, 0)
      });

      dgvKategoriler.Dock = DockStyle.Fill;
      dgvKategoriler.BackgroundColor = Color.White;
      dgvKategoriler.BorderStyle = BorderStyle.None;
      dgvKategoriler.ReadOnly = true;
      dgvKategoriler.AllowUserToAddRows = false;
      dgvKategoriler.AllowUserToDeleteRows = false;
      dgvKategoriler.AutoGenerateColumns = false;
      dgvKategoriler.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
      dgvKategoriler.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
      dgvKategoriler.MultiSelect = false;
      dgvKategoriler.RowHeadersVisible = false;
      dgvKategoriler.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Kategori", DataPropertyName = "KategoriAdi" });
      dgvKategoriler.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Açıklama", DataPropertyName = "Aciklama" });
      dgvKategoriler.SelectionChanged += (sender, e) => FillSelected();

      var editor = new Panel { Dock = DockStyle.Bottom, Height = 137, BackColor = Color.FromArgb(236, 240, 241) };
      editor.Controls.Add(new Label { Text = "Kategori Adı:", Location = new Point(15, 16), AutoSize = true });
      editor.Controls.Add(new Label { Text = "Açıklama:", Location = new Point(15, 51), AutoSize = true });
      txtAd.Location = new Point(135, 13); txtAd.Size = new Size(580, 25); txtAd.MaxLength = 100;
      txtAciklama.Location = new Point(135, 48); txtAciklama.Size = new Size(580, 25); txtAciklama.MaxLength = 500;
      editor.Controls.Add(txtAd); editor.Controls.Add(txtAciklama);

      var btnEkle = MakeButton("✓ Ekle", 15, Color.FromArgb(46, 204, 113));
      var btnGuncelle = MakeButton("✎ Güncelle", 157, Color.FromArgb(52, 152, 219));
      var btnSil = MakeButton("✕ Sil", 299, Color.FromArgb(231, 76, 60));
      var btnTemizle = MakeButton("↻ Temizle", 441, Color.FromArgb(155, 89, 182));
      var btnKapat = MakeButton("Kapat", 583, Color.FromArgb(149, 165, 166));
      btnEkle.Click += (sender, e) => SaveNew();
      btnGuncelle.Click += (sender, e) => SaveSelected();
      btnSil.Click += (sender, e) => DeleteSelected();
      btnTemizle.Click += (sender, e) => { dgvKategoriler.ClearSelection(); txtAd.Clear(); txtAciklama.Clear(); txtAd.Focus(); };
      btnKapat.Click += (sender, e) => Close();
      editor.Controls.AddRange(new Control[] { btnEkle, btnGuncelle, btnSil, btnTemizle, btnKapat });

      Controls.Add(dgvKategoriler);
      Controls.Add(editor);
      Controls.Add(header);
      Shown += (sender, e) => LoadCategories();
    }

    private static Button MakeButton(string title, int left, Color color)
    {
      return new Button
      {
        Text = title, Location = new Point(left, 86), Size = new Size(132, 40),
        BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
        Font = new Font("Segoe UI", 10F, FontStyle.Bold)
      };
    }

    private Kategori SelectedCategory()
    {
      return dgvKategoriler.SelectedRows.Count > 0
        ? dgvKategoriler.SelectedRows[0].DataBoundItem as Kategori : null;
    }

    private void LoadCategories()
    {
      try
      {
        dgvKategoriler.DataSource = null;
        dgvKategoriler.DataSource = _service.GetAllKategoriler();
        dgvKategoriler.ClearSelection();
        txtAd.Clear(); txtAciklama.Clear();
      }
      catch (Exception ex)
      {
        MessageBox.Show("Kategoriler yüklenemedi: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void FillSelected()
    {
      var category = SelectedCategory();
      if (category == null) return;
      txtAd.Text = category.KategoriAdi;
      txtAciklama.Text = category.Aciklama;
    }

    private void SaveNew()
    {
      var result = _service.AddKategori(txtAd.Text, txtAciklama.Text);
      ShowResult(result);
    }

    private void SaveSelected()
    {
      var category = SelectedCategory();
      if (category == null) { MessageBox.Show("Önce bir kategori seçiniz."); return; }
      ShowResult(_service.UpdateKategori(category.KategoriId, txtAd.Text, txtAciklama.Text));
    }

    private void DeleteSelected()
    {
      var category = SelectedCategory();
      if (category == null) { MessageBox.Show("Önce bir kategori seçiniz."); return; }
      if (MessageBox.Show($"'{category.KategoriAdi}' kategorisi silinsin mi?", "Silme Onayı",
          MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
      ShowResult(_service.DeleteKategori(category.KategoriId));
    }

    private void ShowResult(OperationResult result)
    {
      MessageBox.Show(result.Message, result.Success ? "Başarılı" : "İşlem Yapılamadı",
        MessageBoxButtons.OK, result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
      if (result.Success) LoadCategories();
    }
  }
}
