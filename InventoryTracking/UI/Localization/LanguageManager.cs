using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace InventoryTracking.UI.Localization
{
  public static class LanguageManager
  {
    // Dil tercihi mevcut Windows kullanıcısının klasöründe saklanır.
    private static string _settingsPath = Path.Combine(
      Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
      "InventoryTracking",
      "language.txt");

    private static string _currentLanguage = "tr";

    // Formlar bu olayı dinleyerek ekrandaki metinlerini güncelleyecek.
    public static event EventHandler LanguageChanged;

    public static bool IsEnglish => _currentLanguage == "en";

    public static void Initialize()
    {
      Initialize(_settingsPath);
    }

    internal static void Initialize(string preferencePath)
    {
      _settingsPath = preferencePath;
      string language = "tr";

      try
      {
        if (File.Exists(_settingsPath))
        {
          string savedLanguage = File.ReadAllText(_settingsPath).Trim();

          // Dosyada yalnızca desteklediğimiz dil kodlarını kabul ediyoruz.
          if (savedLanguage == "tr" || savedLanguage == "en")
            language = savedLanguage;
        }
      }
      catch (Exception ex) when (
        ex is IOException ||
        ex is UnauthorizedAccessException)
      {
        // Tercih okunamazsa uygulama Türkçe açılır.
        Trace.TraceWarning("Dil tercihi okunamadı: " + ex.Message);
      }

      ApplyCulture(language);
    }

    public static void ToggleLanguage()
    {
      SetLanguage(IsEnglish ? "tr" : "en");
    }

    public static void SetLanguage(string language)
    {
      if (language != "tr" && language != "en")
        throw new ArgumentException(
          "Desteklenmeyen dil kodu.",
          nameof(language));

      if (language == _currentLanguage)
        return;

      // Tercihi önce kaydediyoruz. Kayıt başarısız olursa çağıran form
      // hatayı gösterebilir ve mevcut dil kullanılmaya devam eder.
      string folder = Path.GetDirectoryName(_settingsPath);
      Directory.CreateDirectory(folder);
      File.WriteAllText(_settingsPath, language);

      ApplyCulture(language);

      // Formlar yeniden oluşturulmadan yalnızca metinleri güncellenir.
      // Böylece kullanıcının seçtiği ürün ve girdiği bilgiler korunabilir.
      LanguageChanged?.Invoke(null, EventArgs.Empty);
    }

    private static void ApplyCulture(string language)
    {
      _currentLanguage = language;

      CultureInfo culture = CultureInfo.GetCultureInfo(
        IsEnglish ? "en" : "tr-TR");

      // Arayüz kaynaklarının hangi dilde okunacağını belirler.
      CultureInfo.CurrentUICulture = culture;
      CultureInfo.DefaultThreadCurrentUICulture = culture;
      InventoryTracking.Properties.Resources.Culture = culture;

      // CurrentCulture ayrı tutulur. Bu adım mevcut sayı ve tarih
      // okuma kurallarını değiştirmeden arayüz dilini seçer.
    }
  }
}