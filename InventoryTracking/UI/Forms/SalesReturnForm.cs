using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using InventoryTracking.Business;
using InventoryTracking.UI.Localization;
using InventoryTracking.Models;

namespace InventoryTracking.UI.Forms
{
  public class SalesReturnForm : LocalizedForm, IMessageFilter
  {
    private readonly Product _urun;
    private readonly int _depoId;
    private readonly SalesReturnService _service = new SalesReturnService();
    private readonly NumericUpDown _adet = new NumericUpDown();
    private readonly TextBox _scan = new TextBox();
    private readonly Button _satis = new Button(), _iade = new Button();
    private readonly StringBuilder _buffer = new StringBuilder();
    private Guid _islemNo = Guid.NewGuid();
    private DateTime _lastKey;
    private decimal _quantityBeforeTyping;
    private bool _typingQuantity, _saving, _filterAttached, _retryPending;
    private bool? _pendingReturn;
    private int _pendingQuantity;
    public event EventHandler IslemTamamlandi;

    public SalesReturnForm(Product urun, int depoId)
    {
      _urun = urun ?? throw new ArgumentNullException(nameof(urun));
      _depoId = depoId;
      Name = "SalesReturnForm";
      Text = UiText.Get("SaleReturnTitle");
      ClientSize = new Size(540, 320);
      StartPosition = FormStartPosition.CenterParent;
      FormBorderStyle = FormBorderStyle.FixedDialog;
      MaximizeBox = false; MinimizeBox = false;
      BackColor = Color.FromArgb(245, 248, 252);
      Font = new Font("Segoe UI", 10F);
      var title = new Label { Dock=DockStyle.Top,Height=52,Text=UiText.Get("SaleReturnTitle"),
        BackColor=Color.FromArgb(0,117,201),ForeColor=Color.White,TextAlign=ContentAlignment.MiddleLeft,
        Padding=new Padding(18,0,0,0),Font=new Font("Segoe UI",14F,FontStyle.Bold) };
      Controls.Add(title);
      Controls.Add(new Label { Name="lblProductValue",Text=urun.UrunAdi,Location=new Point(18,66),
        Size=new Size(504,30),AutoEllipsis=true,Font=new Font("Segoe UI",12F,FontStyle.Bold) });
      Controls.Add(new Label { Name="lblBarcodeValue",Text=urun.Barkod,Location=new Point(18,99),Size=new Size(504,24) });
      Controls.Add(new Label { Text=UiText.Get("Quantity"),Location=new Point(18,140),Size=new Size(150,28) });
      _adet.Name="numAdet"; _adet.Bounds=new Rectangle(175,136,150,32);
      _adet.Minimum=1; _adet.Maximum=1000000; _adet.Value=1;
      _adet.Font=new Font("Segoe UI",12F,FontStyle.Bold);
      Controls.Add(_adet);
      Controls.Add(new Label { Text=UiText.Get("RepeatScanHint"),Location=new Point(18,181),Size=new Size(504,24) });
      _scan.Name="txtTekrarBarkod"; _scan.Bounds=new Rectangle(18,208,504,29);
      Controls.Add(_scan);
      ConfigureButton(_satis,"btnSatis","SaleAction",new Rectangle(18,258,247,44),Color.FromArgb(0,120,213));
      ConfigureButton(_iade,"btnIade","ReturnAction",new Rectangle(275,258,247,44),Color.FromArgb(36,158,71));
      _satis.Click+=(s,e)=>Complete(false); _iade.Click+=(s,e)=>Complete(true);
      _adet.Leave+=(s,e)=>ResetScan();
      Shown+=(s,e)=> { Application.AddMessageFilter(this); _filterAttached=true; _scan.Focus(); };
    }

    private void ConfigureButton(Button button,string name,string key,Rectangle bounds,Color color)
    {
      button.Name=name; button.Text=UiText.Get(key); button.Bounds=bounds;
      button.BackColor=color; button.ForeColor=Color.White; button.FlatStyle=FlatStyle.Flat;
      button.FlatAppearance.BorderSize=0; button.Font=new Font("Segoe UI",11F,FontStyle.Bold);
      Controls.Add(button);
    }

    // Barkod okuyucunun karakterleri, adet alanı veya düğme odaktayken de yakalanır.
    // Enter işlem düğmesini tetiklemez; yalnızca tamamlanan barkodu değerlendirir.
    public bool PreFilterMessage(ref Message message)
    {
      if (_saving || !Visible || (message.Msg!=0x0102 && message.Msg!=0x0100)) return false;
      var control=Control.FromChildHandle(message.HWnd);
      if (control==null || control.FindForm()!=this) return false;
      char key=(char)message.WParam.ToInt32();
      if (message.Msg==0x0100 && key!='\r') return false; // WM_KEYDOWN: Enter
      if (key=='\r')
      {
        string barcode=_buffer.ToString();
        bool manualQuantity=_typingQuantity && barcode.Length<_urun.Barkod.Length;
        decimal previous=_quantityBeforeTyping;
        bool restore=_typingQuantity && !manualQuantity;
        ResetScan();
        if (restore) _adet.Value=Math.Max(_adet.Minimum,Math.Min(_adet.Maximum,previous));
        if (barcode.Length>0 && !manualQuantity) RegisterBarcode(barcode);
        _scan.Focus();
        return true;
      }
      if (char.IsControl(key)) return false;
      if ((DateTime.UtcNow-_lastKey).TotalMilliseconds>750) ResetScan();
      _lastKey=DateTime.UtcNow;
      if (_buffer.Length==0) { _typingQuantity=_adet.ContainsFocus; _quantityBeforeTyping=_adet.Value; }
      if (_buffer.Length>=50) { ResetScan(); return true; }
      _buffer.Append(key);
      if (!_typingQuantity) { _scan.Text=_buffer.ToString(); return true; }
      return false;
    }

    public bool RegisterBarcode(string barcode)
    {
      if (_retryPending)
      {
        UiMessage.Show(UiText.Get("SaleRetryLocked"),UiText.Get("Warning"),MessageBoxButtons.OK,MessageBoxIcon.Warning);
        return false;
      }
      if (!string.Equals(barcode?.Trim(),_urun.Barkod,StringComparison.OrdinalIgnoreCase))
      {
        UiMessage.Show(UiText.Get("DifferentBarcode"),UiText.Get("Warning"),MessageBoxButtons.OK,MessageBoxIcon.Warning);
        return false;
      }
      if (_adet.Value>=_adet.Maximum) return false;
      _adet.Value+=1;
      return true;
    }

    private void ResetScan() { _buffer.Clear(); _scan.Clear(); _typingQuantity=false; }

    private void Complete(bool iadeMi)
    {
      if (_saving) return;
      if (_retryPending && (_pendingReturn!=iadeMi || _pendingQuantity!=(int)_adet.Value)) return;
      _saving=true; _satis.Enabled=false; _iade.Enabled=false;
      try
      {
        var result=_service.Kaydet(_islemNo,_urun.UrunId,_depoId,(int)_adet.Value,iadeMi);
        if (!result.Success)
        {
          // İsteğin sunucuda tamamlanıp cevabın kaybolması ihtimaline karşı
          // aynı işlemi tekrar denemek gerekir; yön/adet değiştirilemez.
          _retryPending=result.Message.StartsWith(UiText.Get("SaleDatabaseErrorPrefix"),StringComparison.CurrentCulture);
          if (_retryPending) { _pendingReturn=iadeMi; _pendingQuantity=(int)_adet.Value; }
          UiMessage.Show(result.Message,UiText.Get("Error"),MessageBoxButtons.OK,MessageBoxIcon.Error);
          return;
        }
        UiMessage.Show(result.Message,UiText.Get("Success"),MessageBoxButtons.OK,MessageBoxIcon.Information);
        IslemTamamlandi?.Invoke(this,EventArgs.Empty);
        DialogResult=DialogResult.OK;
        Close();
      }
      finally
      {
        _saving=false;
        _adet.Enabled=!_retryPending;
        _satis.Enabled=!_retryPending || _pendingReturn==false;
        _iade.Enabled=!_retryPending || _pendingReturn==true;
      }
    }

    protected override bool ProcessDialogKey(Keys keyData)
    {
      if (keyData==Keys.Enter) return true;
      if (keyData==Keys.Escape) { Close(); return true; }
      return base.ProcessDialogKey(keyData);
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && _filterAttached) { Application.RemoveMessageFilter(this); _filterAttached=false; }
      base.Dispose(disposing);
    }
  }
}
