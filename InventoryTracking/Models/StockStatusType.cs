using InventoryTracking.UI.Localization;
using System;

namespace InventoryTracking.Models
{
  /// <summary>
  /// Stok durumu enum
  /// </summary>
  public enum StockStatusType
  {
    Critical = 1, // Minimum stoğun altında
    Low = 2, // Çeyreğinin altında
    Normal = 3 // Normal seviye
  }
}
