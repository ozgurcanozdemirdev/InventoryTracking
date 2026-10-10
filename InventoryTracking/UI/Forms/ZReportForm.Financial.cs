using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;
using InventoryTracking.Business;
using InventoryTracking.UI.Localization;
using InventoryTracking.Models;
using InventoryTracking.Reporting;

namespace InventoryTracking.UI.Forms
{
  public partial class ZReportForm
  {
    private readonly SalesReturnReportService _financialService = new SalesReturnReportService();
    private readonly TabPage _financialTab = new TabPage(UiText.Get("FinancialSummary"));
    private readonly DataGridView _financialGrid = new DataGridView();
    private readonly Label _financialTotals = new Label();
    private readonly Button _lastYearButton = new Button();
    private readonly Button _csvButton = new Button();
    private static readonly string[] FinancialProperties =
      { "Donem", "SatisAdedi", "IadeAdedi", "NetAdet", "SatisTutari", "IadeTutari", "NetSatisTutari", "IslemSayisi" };
    private static readonly string[] FinancialKeys =
      { "Period", "SalesQuantity", "ReturnsQuantity", "NetQuantity", "SalesAmount", "ReturnsAmount", "NetSalesAmount", "TransactionCount" };

    private bool FinancialSelected => _sekmeler.SelectedTab == _financialTab;

    private void InitializeFinancialReport()
    {
      _lastYearButton.Name = "btnSonBirYil";
      _lastYearButton.SetBounds(735, 20, 195, 32);
      StyleReportButton(_lastYearButton, Color.FromArgb(41, 128, 185));
      _lastYearButton.Text = UiText.Get("LastYearAction");
      _lastYearButton.Click += (sender, args) =>
      {
        dtpBitis.Value = DateTime.Today;
        dtpBaslangic.Value = SalesReturnReportService.SonBirYilBaslangici(DateTime.Today);
        // Dönem seçimi korunur; tek tıklamayla tarihleri seçip raporu yenileriz.
        btnGetir_Click(sender, args);
      };
      pnlFilter.Controls.Add(_lastYearButton);

      // Var olan CSV seçeneği korunur; Excel düğmesi artık gerçek .xlsx üretir.
      btnExcel.Text = UiText.Get("ExcelExportAction");
      _csvButton.Name = "btnCsv";
      _csvButton.SetBounds(465, 15, 140, 40);
      StyleReportButton(_csvButton, Color.FromArgb(39, 174, 96));
      _csvButton.Text = UiText.Get("CsvAction");
      _csvButton.Click += btnCsv_Click;
      pnlButtons.Controls.Add(_csvButton);

      _financialTab.Name = "tabSatisIadeOzeti";
      _financialTab.BackColor = Color.White;
      _financialGrid.Name = "dgvSatisIadeRapor";
      _financialGrid.Dock = DockStyle.Fill;
      _financialGrid.ReadOnly = true;
      _financialGrid.AllowUserToAddRows = false;
      _financialGrid.AllowUserToDeleteRows = false;
      _financialGrid.RowHeadersVisible = false;
      _financialGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
      _financialGrid.BackgroundColor = Color.White;
      _financialGrid.BorderStyle = BorderStyle.None;
      _financialGrid.AutoGenerateColumns = false;
      _financialGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
      for (int i = 0; i < FinancialProperties.Length; i++)
      {
        var column = new DataGridViewTextBoxColumn
        {
          Name = FinancialProperties[i], DataPropertyName = FinancialProperties[i],
          HeaderText = UiText.Get(FinancialKeys[i]), FillWeight = i >= 4 && i <= 6 ? 135 : 100,
          SortMode = DataGridViewColumnSortMode.NotSortable
        };
        column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
        column.HeaderCell.Style.WrapMode = DataGridViewTriState.True;
        if (i > 0)
        {
          column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
          column.DefaultCellStyle.Format = i >= 4 && i <= 6 ? "N2" : "N0";
        }
        _financialGrid.Columns.Add(column);
      }
      _financialTotals.Name = "lblSatisIadeToplam";
      _financialTotals.Dock = DockStyle.Bottom;
      _financialTotals.Height = 76;
      _financialTotals.Padding = new Padding(8, 7, 8, 0);
      _financialTotals.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
      _financialTotals.Text = UiText.Get("ReportHint");
      _financialTab.Controls.Add(_financialGrid);
      _financialTab.Controls.Add(_financialTotals);
      _sekmeler.TabPages.Add(_financialTab);
      _sekmeler.SelectedIndexChanged += (sender, args) => UpdateReportMode();
      UpdateReportMode();
    }

    private static void StyleReportButton(Button button, Color color)
    {
      button.FlatStyle = FlatStyle.Flat;
      button.FlatAppearance.BorderSize = 0;
      button.BackColor = color; button.ForeColor = Color.White;
      button.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
    }

    private void UpdateReportMode()
    {
      // Stok yönü ve transfer filtresi finansal hesaplama için anlam taşımaz.
      // Kullanıcı sekmeler arasında geçerken seçimler saklanır.
      _cmbHareket.Enabled = !FinancialSelected;
      _cmbYon.Enabled = !FinancialSelected;
      lblTitle.Text = UiText.Get(FinancialSelected ? "FinancialReportTitle" : "ReportHeader");
    }

    private void RefreshFinancialLanguage()
    {
      _financialTab.Text = UiText.Get("FinancialSummary");
      _lastYearButton.Text = UiText.Get("LastYearAction");
      btnExcel.Text = UiText.Get("ExcelExportAction");
      _csvButton.Text = UiText.Get("CsvAction");
      for (int i = 0; i < _financialGrid.Columns.Count; i++)
        _financialGrid.Columns[i].HeaderText = UiText.Get(FinancialKeys[i]);
      if (_raporGetirildi) UpdateFinancialTotals();
      else _financialTotals.Text = UiText.Get("ReportHint");
      UpdateReportMode();
    }

    private void UpdateFinancialTotals()
    {
      var rows = _financialGrid.DataSource as List<SalesReturnReportSummary>;
      if (rows == null) return;
      _financialTotals.Text = UiText.Format("FinancialTotals",
        rows.Sum(x => x.SatisAdedi), rows.Sum(x => x.SatisTutari),
        rows.Sum(x => x.IadeAdedi), rows.Sum(x => x.IadeTutari),
        rows.Sum(x => x.NetAdet), rows.Sum(x => x.NetSatisTutari)) +
        Environment.NewLine + UiText.Get("FinancialScopeNote");
    }

    private string FinancialFilterDescription()
    {
      var filter = _appliedFilter ?? GetFilter();
      string warehouse = filter.DepoId.HasValue ? (_appliedWarehouse ?? _cmbDepo.Text) : UiText.Get("AllWarehouses");
      string product = string.IsNullOrWhiteSpace(filter.UrunArama) ? UiText.Get("AllProducts") : filter.UrunArama;
      return $"{filter.Baslangic:dd.MM.yyyy} - {filter.Bitis:dd.MM.yyyy} | " +
        UiText.Get(_appliedPeriod ?? "Daily") + " | " + warehouse + " | " + product;
    }

    private string ExportTitle() => UiText.Get(FinancialSelected ? "FinancialReportTitle" :
      _sekmeler.SelectedTab == _detaySekmesi ? "DetailPrintTitle" : "SummaryPrintTitle");

    private string ExportFilter() => FinancialSelected ? FinancialFilterDescription() : _uygulananFiltre;

    private void WriteExcel(string fileName)
    {
      var grid = SelectedGrid();
      var columns = grid.Columns.Cast<DataGridViewColumn>().Where(c => c.Visible).OrderBy(c => c.DisplayIndex).ToArray();
      var rows = grid.Rows.Cast<DataGridViewRow>().Where(r => !r.IsNewRow)
        .Select(r => columns.Select(c => UiText.GridValue(r.Cells[c.Index])).ToArray()).ToList();
      ExcelReportExporter.Write(fileName, ExportTitle(), ExportFilter(),
        FinancialSelected ? _financialTotals.Text : _lblToplam.Text,
        columns.Select(c => c.HeaderText).ToArray(), rows);
    }

    private void btnExcel_Click(object sender, EventArgs e)
    {
      if (!_raporGetirildi || SelectedGrid().Rows.Count == 0)
      {
        UiMessage.Show(UiText.Get("ExportEmpty"), UiText.Get("Information"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
      }
      using (var dialog = new SaveFileDialog { Filter = UiText.Get("XlsxFilter"),
        FileName = $"InventoryReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx" })
      {
        if (dialog.ShowDialog() != DialogResult.OK) return;
        try
        {
          WriteExcel(dialog.FileName);
          UiMessage.Show(UiText.Format("ReportSaved", dialog.FileName), UiText.Get("Success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        { UiMessage.Show(UiText.Get("ExportError") + ex.Message, UiText.Get("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error); }
      }
    }

    private void PrintFinancialPage(PrintPageEventArgs e)
    {
      using (var titleFont = new Font("Segoe UI", 14F, FontStyle.Bold))
      using (var headerFont = new Font("Segoe UI", 8F, FontStyle.Bold))
      using (var textFont = new Font("Segoe UI", 9F))
      using (var pen = new Pen(Color.LightGray))
      using (var center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
      using (var right = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center })
      {
        int y = e.MarginBounds.Top;
        int left = e.MarginBounds.Left;
        int width = e.MarginBounds.Width;
        e.Graphics.DrawString(ExportTitle(), titleFont, Brushes.Black, left, y); y += 35;
        string filter = FinancialFilterDescription();
        int filterHeight = (int)Math.Ceiling(e.Graphics.MeasureString(filter, textFont, width).Height);
        e.Graphics.DrawString(filter, textFont, Brushes.Black, new RectangleF(left, y, width, filterHeight)); y += filterHeight + 12;
        var columns = _financialGrid.Columns.Cast<DataGridViewColumn>().Where(c => c.Visible).ToArray();
        float scale = width / (float)columns.Sum(c => c.Width);
        float x = left;
        foreach (var col in columns)
        {
          var rect = new RectangleF(x, y, col.Width * scale, 44);
          e.Graphics.FillRectangle(Brushes.LightGray, rect);
          e.Graphics.DrawString(col.HeaderText, headerFont, Brushes.Black, rect, center);
          x += rect.Width;
        }
        y += 44;
        while (_yazdirilacakSatirIndex < _financialGrid.Rows.Count)
        {
          if (y + 26 > e.MarginBounds.Bottom) { e.HasMorePages = true; return; }
          var row = _financialGrid.Rows[_yazdirilacakSatirIndex]; x = left;
          foreach (var col in columns)
          {
            var rect = new RectangleF(x, y, col.Width * scale, 26);
            e.Graphics.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
            e.Graphics.DrawString(row.Cells[col.Index].FormattedValue?.ToString() ?? "", textFont, Brushes.Black, rect, col.Index == 0 ? center : right);
            x += rect.Width;
          }
          y += 26; _yazdirilacakSatirIndex++;
        }
        int totalHeight = (int)Math.Ceiling(e.Graphics.MeasureString(_financialTotals.Text, textFont, width).Height) + 12;
        if (y + totalHeight > e.MarginBounds.Bottom) { e.HasMorePages = true; return; }
        e.Graphics.DrawString(_financialTotals.Text, textFont, Brushes.Black, new RectangleF(left, y + 10, width, totalHeight));
        e.HasMorePages = false; _yazdirilacakSatirIndex = 0;
      }
    }
  }
}
