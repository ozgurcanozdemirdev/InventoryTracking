using System.Drawing;
using System.Windows.Forms;

namespace InventoryTracking.UI.Controls
{
  /// <summary>Ürün panelindeki değer kutularının kenarlığını tema rengiyle çizer.</summary>
  internal class InventoryValueLabel : Label
  {
    protected override void OnPaint(PaintEventArgs e)
    {
      base.OnPaint(e);
      using (var pen = new Pen(Color.FromArgb(213, 223, 234)))
        e.Graphics.DrawRectangle(pen, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
    }
  }
}
