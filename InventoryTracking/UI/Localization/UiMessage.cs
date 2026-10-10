using System;
using System.Windows.Forms;

namespace InventoryTracking.UI.Localization
{
  internal static class UiMessage
  {
    // Ekran testleri işletim sisteminin iletişim kutusunu açmadan bildirimleri denetleyebilir.
    internal static Func<string, string, MessageBoxButtons, MessageBoxIcon, DialogResult> Handler { get; set; }

    public static DialogResult Show(string text, string caption = "",
      MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None)
    {
      return Handler != null ? Handler(text, caption, buttons, icon)
        : MessageBox.Show(text, caption, buttons, icon);
    }
  }
}
