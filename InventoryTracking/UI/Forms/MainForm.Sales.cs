using System;
using System.Drawing;
using System.Windows.Forms;
using InventoryTracking.Business;
using InventoryTracking.UI.Localization;

namespace InventoryTracking.UI.Forms
{
  public partial class MainForm
  {
    private DataGridView _salesHistory;
    private TabControl _workspaceTabs;

    private void InitializeSalesHistory(Panel stockFrame)
    {
      _workspaceTabs=new TabControl { Name="tabWorkspace",Dock=DockStyle.Fill,Margin=stockFrame.Margin };
      var stock=new TabPage(UiText.Get("StockListCaption")) { Padding=Padding.Empty };
      var history=new TabPage(UiText.Get("SaleHistoryCaption")) { Padding=Padding.Empty };
      pnlWorkspace.Controls.Remove(stockFrame);
      stock.Controls.Add(stockFrame);
      _workspaceTabs.TabPages.Add(stock); _workspaceTabs.TabPages.Add(history);
      pnlWorkspace.Controls.Add(_workspaceTabs,0,0);
      _salesHistory=new DataGridView { Name="dgvSatisIade",Dock=DockStyle.Fill,ReadOnly=true,
        AutoGenerateColumns=false,AllowUserToAddRows=false,AllowUserToDeleteRows=false,
        RowHeadersVisible=false,BackgroundColor=Color.White,BorderStyle=BorderStyle.None,
        SelectionMode=DataGridViewSelectionMode.FullRowSelect,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,
        EnableHeadersVisualStyles=false,ColumnHeadersHeight=40 };
      _salesHistory.ColumnHeadersDefaultCellStyle=new DataGridViewCellStyle(dgvStoklar.ColumnHeadersDefaultCellStyle);
      _salesHistory.DefaultCellStyle=new DataGridViewCellStyle(dgvStoklar.DefaultCellStyle);
      _salesHistory.AlternatingRowsDefaultCellStyle.BackColor=dgvStoklar.AlternatingRowsDefaultCellStyle.BackColor;
      _salesHistory.RowTemplate.Height=38;
      AddHistoryColumn("IslemTarihi","SaleDate",160,"dd.MM.yyyy HH:mm:ss");
      AddHistoryColumn("IslemTuru","SaleType",85);
      AddHistoryColumn("UrunAdi","Product",180);
      AddHistoryColumn("Miktar","Quantity",65,"N0");
      AddHistoryColumn("BirimFiyat","UnitPrice",95,"N2");
      AddHistoryColumn("ToplamTutar","SaleAmount",105,"N2");
      AddHistoryColumn("KullaniciAdi","SaleUser",120);
      history.Controls.Add(_salesHistory);
      _salesHistory.CellFormatting+=(s,e)=>
      {
        if (e.ColumnIndex>=0 && _salesHistory.Columns[e.ColumnIndex].DataPropertyName=="IslemTuru")
          e.Value=UiText.DisplayValue("IslemTuru",e.Value);
      };
      _workspaceTabs.SelectedIndexChanged+=(s,e)=> { if (_workspaceTabs.SelectedIndex==1) RefreshSalesHistory(); };
      // Depo değişirse geçmiş de o depoya göre yenilenir.
      cmbDepo.SelectedIndexChanged+=(s,e)=> { if (!_isFormLoading) RefreshSalesHistory(); };
    }

    private void AddHistoryColumn(string property,string key,int width,string format=null)
    {
      var column=new DataGridViewTextBoxColumn { Name=property,DataPropertyName=property,
        HeaderText=UiText.Get(key),MinimumWidth=width,FillWeight=width,Tag=key };
      if (format!=null) column.DefaultCellStyle.Format=format;
      _salesHistory.Columns.Add(column);
    }

    private void RefreshSalesHistory()
    {
      if (_salesHistory==null) return;
      if (_selectedDepoId<=0) { _salesHistory.DataSource=null; return; }
      try { _salesHistory.DataSource=new SalesReturnService().GetHistory(_selectedDepoId); }
      catch (Exception ex)
      {
        // Ana stok ekranı bir rapor yükleme hatası nedeniyle kapanmaz.
        _salesHistory.DataSource=null;
        toolStripStatusLabel.Text=UiText.Get("SaleHistoryLoadError")+ex.Message;
      }
    }

    private void RefreshSalesLanguage()
    {
      if (_workspaceTabs==null) return;
      _workspaceTabs.TabPages[0].Text=UiText.Get("StockListCaption");
      _workspaceTabs.TabPages[1].Text=UiText.Get("SaleHistoryCaption");
      foreach (DataGridViewColumn column in _salesHistory.Columns)
        column.HeaderText=UiText.Get((string)column.Tag);
      _salesHistory.Invalidate();
    }
  }
}
