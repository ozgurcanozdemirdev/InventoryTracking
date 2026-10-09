using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace InventoryTracking.Localization
{
  public class LocalizedForm : Form
  {
    private readonly Dictionary<Control, string> _captions = new Dictionary<Control, string>();
    private readonly Dictionary<ToolStripItem, string> _items = new Dictionary<ToolStripItem, string>();
    private readonly HashSet<Control> _attached = new HashSet<Control>();
    private bool _applying;

    public LocalizedForm()
    {
      LanguageManager.LanguageChanged += HandleLanguageChanged;
      Attach(this);
    }

    private static bool IsCaption(Control control)
    {
      // Girdi alanları ve kayıttan gelen bilgi etiketleri çeviri kapsamına girmez.
      return !(control is TextBoxBase || control is ComboBox || control is UpDownBase ||
        control is DateTimePicker || control is DataGridView) &&
        !control.Name.StartsWith("lblDetail", StringComparison.Ordinal) &&
        !control.Name.EndsWith("Value", StringComparison.Ordinal) &&
        control.Name != "lblStockUnit" && control.Name != "lblUser" && control.Name != "lblClock" && control.Name != "btnLanguage";
    }

    private void Attach(Control control)
    {
      if (!_attached.Add(control)) return;
      control.ControlAdded += (sender, e) => { Attach(e.Control); };
      control.TextChanged += (sender, e) => CaptureCaption(control);
      CaptureCaption(control);
      var strip = control as ToolStrip;
      if (strip != null)
      {
        strip.ItemAdded += (sender, e) => AttachItem(e.Item);
        foreach (ToolStripItem item in strip.Items) AttachItem(item);
      }
      var grid = control as DataGridView;
      if (grid != null)
      {
        grid.ColumnAdded += (sender, e) => TranslateHeaders(grid);
        grid.DataBindingComplete += (sender, e) => TranslateHeaders(grid);
        grid.CellFormatting += (sender, e) =>
        {
          if (e.ColumnIndex < 0) return;
          e.Value = UiText.DisplayValue(grid.Columns[e.ColumnIndex].DataPropertyName, e.Value);
        };
      }
      foreach (Control child in control.Controls) Attach(child);
    }

    private void CaptureCaption(Control control)
    {
      if (_applying || !IsCaption(control)) return;
      string key = UiText.CaptionKey(control.Text);
      if (key == null) { _captions.Remove(control); return; }
      _captions[control] = key;
      _applying = true;
      try { control.Text = UiText.Get(key); }
      finally { _applying = false; }
    }

    private void AttachItem(ToolStripItem item)
    {
      if (item.Name == "toolStripDepot") return;
      item.TextChanged += (sender, e) =>
      {
        if (_applying) return;
        string key = UiText.CaptionKey(item.Text);
        if (key == null) _items.Remove(item); else _items[item] = key;
      };
      string initialKey = UiText.CaptionKey(item.Text);
      if (initialKey != null) _items[item] = initialKey;
    }

    private static void TranslateHeaders(DataGridView grid)
    {
      foreach (DataGridViewColumn column in grid.Columns)
      {
        string key = UiText.CaptionKey(column.HeaderText);
        if (key != null) column.HeaderText = UiText.Get(key);
      }
    }

    protected void ApplyLanguage()
    {
      if (_applying || IsDisposed) return;
      _applying = true;
      SuspendLayout();
      try
      {
        foreach (var caption in _captions.ToArray())
          if (!caption.Key.IsDisposed && IsCaption(caption.Key)) caption.Key.Text = UiText.Get(caption.Value);
        foreach (var item in _items.ToArray())
          if (!item.Key.IsDisposed) item.Key.Text = UiText.Get(item.Value);
        foreach (var grid in _attached.OfType<DataGridView>())
        {
          TranslateHeaders(grid);
          grid.Invalidate();
        }
        OnLanguageChanged();
      }
      finally { ResumeLayout(true); _applying = false; }
    }

    protected virtual void OnLanguageChanged() { }

    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e);
      ApplyLanguage();
    }

    private void HandleLanguageChanged(object sender, EventArgs e) => ApplyLanguage();

    protected override void Dispose(bool disposing)
    {
      if (disposing) LanguageManager.LanguageChanged -= HandleLanguageChanged;
      base.Dispose(disposing);
    }
  }
}
