using InventoryTracking.UI.Localization;
using InventoryTracking.Connection;
using InventoryTracking.DataAccess;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using InventoryTracking.Models;

namespace InventoryTracking.Business
{
  /// <summary>
  /// Kategori Business Logic Service
  /// </summary>
  public class CategoryService
  {
    private readonly CategoryRepository _kategoriRepository;
    public CategoryService()
    {
      _kategoriRepository = new CategoryRepository();
    }

    public List<Category> GetAllKategoriler()
    {
      return _kategoriRepository.GetAll();
    }

    public Category GetKategoriById(int kategoriId)
    {
      if (kategoriId <= 0)
      {
        throw new ArgumentException(UiText.Get("InvalidCategoryId"));
      }

      return _kategoriRepository.GetById(kategoriId);
    }

    public OperationResult AddKategori(string kategoriAdi, string aciklama = "")
    {
      if (string.IsNullOrWhiteSpace(kategoriAdi))
      {
        return OperationResult.Fail(UiText.Get("CategoryNameRequired"));
      }

      var kategori = new Category
      {
        KategoriAdi = kategoriAdi.Trim(),
        Aciklama = aciklama.Trim()
      };
      return _kategoriRepository.Add(kategori);
    }

    public OperationResult UpdateKategori(int kategoriId, string kategoriAdi, string aciklama = "")
    {
      if (kategoriId <= 0)
        return OperationResult.Fail(UiText.Get("InvalidCategoryId"));
      if (string.IsNullOrWhiteSpace(kategoriAdi))
        return OperationResult.Fail(UiText.Get("CategoryNameRequired"));
      var kategori = new Category
      {
        KategoriId = kategoriId,
        KategoriAdi = kategoriAdi.Trim(),
        Aciklama = aciklama.Trim()
      };
      return _kategoriRepository.Update(kategori);
    }

    public OperationResult DeleteKategori(int kategoriId)
    {
      return kategoriId > 0 ? _kategoriRepository.Delete(kategoriId) : OperationResult.Fail(UiText.Get("InvalidCategory"));
    }
  }
}
