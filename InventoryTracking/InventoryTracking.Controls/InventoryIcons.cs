using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace InventoryTracking.Controls
{
  internal enum InventoryIcon { Cube, Barcode, Depot, Add, Edit, Delete, Refresh, Movements, Category, Transfer, Exit, Settings, Entry, Output }

  /// <summary>
  /// Ana ekrandaki simgeleri çizerek üretir. Emoji fontlarına veya dış resim dosyasına
  /// bağımlı olmadığı için bütün butonlarda aynı çizgi kalınlığı ve renk kullanılır.
  /// Oluşan resimlerin yaşam süresi MainForm tarafından yönetilir.
  /// </summary>
  internal static class InventoryIcons
  {
    public static Bitmap Create(InventoryIcon icon, Color color, int size)
    {
      var bitmap = new Bitmap(size, size);
      using (var g = Graphics.FromImage(bitmap))
      using (var pen = new Pen(color, 1.8F) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
      {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.ScaleTransform(size / 24F, size / 24F);
        switch (icon)
        {
          case InventoryIcon.Cube:
            g.DrawPolygon(pen, new[] { new PointF(12, 2), new PointF(21, 7), new PointF(21, 17), new PointF(12, 22), new PointF(3, 17), new PointF(3, 7) });
            g.DrawLines(pen, new[] { new PointF(3, 7), new PointF(12, 12), new PointF(21, 7) });
            g.DrawLine(pen, 12, 12, 12, 22); g.DrawLine(pen, 8, 4, 17, 9);
            break;
          case InventoryIcon.Barcode:
            for (int x = 3; x < 22; x += 3)
            {
              pen.Width = x % 2 == 0 ? 2.4F : 1.3F;
              g.DrawLine(pen, x, 3, x, 21);
            }
            break;
          case InventoryIcon.Depot:
            g.DrawLines(pen, new[] { new PointF(2, 9), new PointF(12, 3), new PointF(22, 9), new PointF(22, 21), new PointF(2, 21), new PointF(2, 9) });
            g.DrawRectangle(pen, 7, 11, 10, 10); g.DrawLine(pen, 7, 15, 17, 15); g.DrawLine(pen, 7, 18, 17, 18);
            break;
          case InventoryIcon.Add:
            g.DrawLine(pen, 12, 3, 12, 21); g.DrawLine(pen, 3, 12, 21, 12);
            break;
          case InventoryIcon.Edit:
            g.DrawPolygon(pen, new[] { new PointF(4, 16), new PointF(16, 4), new PointF(21, 9), new PointF(9, 21), new PointF(3, 22) });
            g.DrawLine(pen, 13, 7, 18, 12);
            break;
          case InventoryIcon.Delete:
            g.DrawLine(pen, 3, 6, 21, 6); g.DrawRectangle(pen, 6, 6, 12, 15);
            g.DrawRectangle(pen, 9, 2, 6, 4); g.DrawLine(pen, 10, 10, 10, 17); g.DrawLine(pen, 14, 10, 14, 17);
            break;
          case InventoryIcon.Refresh:
            g.DrawArc(pen, 4, 4, 16, 16, 220, 310);
            g.DrawLines(pen, new[] { new PointF(3, 3), new PointF(3, 9), new PointF(9, 9) });
            break;
          case InventoryIcon.Movements:
            for (int y = 5; y <= 19; y += 7) { g.DrawRectangle(pen, 2, y - 1, 2, 2); g.DrawLine(pen, 8, y, 22, y); }
            break;
          case InventoryIcon.Category:
            g.DrawLines(pen, new[] { new PointF(2, 5), new PointF(9, 5), new PointF(12, 8), new PointF(22, 8), new PointF(22, 21), new PointF(2, 21), new PointF(2, 5) });
            break;
          case InventoryIcon.Transfer:
            g.DrawLine(pen, 3, 7, 21, 7); g.DrawLines(pen, new[] { new PointF(16, 2), new PointF(21, 7), new PointF(16, 12) });
            g.DrawLine(pen, 21, 17, 3, 17); g.DrawLines(pen, new[] { new PointF(8, 12), new PointF(3, 17), new PointF(8, 22) });
            break;
          case InventoryIcon.Exit:
            g.DrawLines(pen, new[] { new PointF(10, 3), new PointF(3, 3), new PointF(3, 21), new PointF(10, 21) });
            g.DrawLine(pen, 9, 12, 22, 12); g.DrawLines(pen, new[] { new PointF(17, 7), new PointF(22, 12), new PointF(17, 17) });
            break;
          case InventoryIcon.Settings:
            // Sekiz dişli ayar simgesi, diğer simgelerle aynı ölçekte çizilir.
            var teeth = new PointF[32];
            double[] angles = { -0.30, -0.18, 0.18, 0.30 };
            float[] radii = { 8F, 10.5F, 10.5F, 8F };
            for (int tooth = 0; tooth < 8; tooth++)
              for (int corner = 0; corner < 4; corner++)
              {
                double angle = tooth * Math.PI / 4 + angles[corner];
                teeth[tooth * 4 + corner] = new PointF(
                  12 + radii[corner] * (float)Math.Cos(angle),
                  12 + radii[corner] * (float)Math.Sin(angle));
              }
            g.DrawPolygon(pen, teeth);
            g.DrawEllipse(pen, 8.5F, 8.5F, 7F, 7F);
            break;
          case InventoryIcon.Entry:
          case InventoryIcon.Output:
            bool entry = icon == InventoryIcon.Entry;
            g.DrawLine(pen, 12, 3, 12, 18);
            g.DrawLines(pen, entry
              ? new[] { new PointF(6, 12), new PointF(12, 18), new PointF(18, 12) }
              : new[] { new PointF(6, 9), new PointF(12, 3), new PointF(18, 9) });
            g.DrawLine(pen, 4, 22, 20, 22);
            break;
        }
      }
      return bitmap;
    }
  }
}
