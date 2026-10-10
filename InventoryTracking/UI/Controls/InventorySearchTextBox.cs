using InventoryTracking.UI.Localization;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace InventoryTracking.UI.Controls
{
  /// <summary>
  /// .NET Framework arama kutusu. Yer tutucu gerçek metinden ayrı çizilir;
  /// barkod okuyucu ve TextChanged yalnızca kullanıcının girişini görür.
  /// </summary>
  internal class InventorySearchTextBox : TextBox
  {
    private bool _drawingPlaceholder;
    public string PlaceholderText { get; set; } = UiText.Get("ScanPlaceholder");

    protected override void OnTextChanged(EventArgs e)
    {
      base.OnTextChanged(e);
      Invalidate();
    }

    protected override void WndProc(ref Message message)
    {
      base.WndProc(ref message);
      // Native TextBox, WinForms Paint olayını kullanmaz. WM_PAINT ve bitmap
      // çıktısının WM_PRINT/WM_PRINTCLIENT mesajlarından sonra boş kutunun
      // yer tutucusu çizilir; yazılan veya okutulan metne müdahale edilmez.
      // Önce mesaj türü kontrol edilir. Text/Font okumak kendi native mesajını
      // gönderdiğinden her mesajda Text okumak sonsuz yeniden giriş oluşturur.
      if (message.Msg != 0x000F && message.Msg != 0x0317 && message.Msg != 0x0318) return;
      if (_drawingPlaceholder || TextLength != 0) return;
      _drawingPlaceholder = true;
      try
      {
        if (message.Msg == 0x000F)
        {
          using (var graphics = Graphics.FromHwnd(Handle)) DrawPlaceholder(graphics);
        }
        else if (message.WParam != IntPtr.Zero)
        {
          using (var graphics = Graphics.FromHdc(message.WParam)) DrawPlaceholder(graphics);
        }
      }
      finally { _drawingPlaceholder = false; }
    }

    private void DrawPlaceholder(Graphics graphics)
    {
      TextRenderer.DrawText(graphics, PlaceholderText, Font, ClientRectangle,
        Color.FromArgb(100, 116, 139), TextFormatFlags.SingleLine | TextFormatFlags.NoPadding
        | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }
  }
}
