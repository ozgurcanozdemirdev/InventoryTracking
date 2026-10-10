using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;

namespace InventoryTracking.UI.Localization
{
  internal static class UiText
  {
    private static readonly Dictionary<string, string> CaptionKeys = CreateCaptionKeys();

    public static string Get(string key)
    {
      return Properties.Resources.ResourceManager.GetString(key, Properties.Resources.Culture)
        ?? throw new InvalidOperationException("Missing UI resource: " + key);
    }

    public static string Format(string key, params object[] values)
    {
      // Arayüz dili değişse de mevcut sayı ve tarih okuma kuralları korunur.
      return string.Format(CultureInfo.CurrentCulture, Get(key), values);
    }

    public static string CaptionKey(string text)
    {
      string key;
      return text != null && CaptionKeys.TryGetValue(text, out key) ? key : null;
    }

    private static Dictionary<string, string> CreateCaptionKeys()
    {
      var keys = new Dictionary<string, string>(StringComparer.Ordinal);
      foreach (string language in new[] { "tr-TR", "en" })
      {
        var set = Properties.Resources.ResourceManager.GetResourceSet(
          CultureInfo.GetCultureInfo(language), true, true);
        foreach (DictionaryEntry entry in set)
        {
          string caption = entry.Value as string;
          // Sayı ve kullanıcı girdisi içeren metinler çağrıldığı yerde biçimlendirilir.
          if (!string.IsNullOrEmpty(caption) && !caption.Contains("{") && !keys.ContainsKey(caption))
            keys.Add(caption, (string)entry.Key);
        }
      }
      // Çıkış düğmesi ile rapor hareket yönü aynı Türkçe kelimeyi kullanır.
      // Hareket yönü kendi anahtarıyla çevrilir; genel düğme başlığı Exit olur.
      keys["Çıkış"] = "Exit";
      keys["Exit"] = "Exit";
      return keys;
    }

    public static object DisplayValue(string property, object value)
    {
      // Veritabanındaki kodlar ve kullanıcının ürün/depo adları değiştirilmez.
      // Yalnızca bilinen durum ve hareket sütunlarının gösterimi çevrilir.
      if (!(value is string)) return value;
      string text = (string)value;
      if (property == "HareketTipi" || property == "Yon" || property == "IslemTuru")
      {
        switch (text)
        {
          case "Satış": return Get("SaleAction");
          case "İade": return Get("ReturnAction");
          case "Giriş": return Get("StockInDirection");
          case "Çıkış": return Get("StockOutDirection");
          case "Transfer Giriş": return Get("TransferIn");
          case "Transfer Çıkış": return Get("TransferOut");
          case "Sayım Düzeltmesi": return Get("CountAdjustment");
        }
      }
      if (property == "StokDurumu")
      {
        if (text == "Kritik") return Get("CriticalStock");
        if (text == "Düşük") return Get("LowStockCode");
      }
      if (property == "Durum")
      {
        if (text == "Aktif") return Get("Active");
        if (text == "Pasif") return Get("Inactive");
      }
      return value;
    }

    public static object GridValue(DataGridViewCell cell)
    {
      return DisplayValue(cell.OwningColumn.DataPropertyName, cell.Value);
    }
  }
}
