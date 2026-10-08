-- Uygulama icin sysadmin/sa yerine sinirli veritabani rolu.
-- Bu betik login olusturmaz; musteri DBA'si Windows kullanicisini bu role ekler.
IF DATABASE_PRINCIPAL_ID(N'InventoryTrackingApp') IS NULL
    CREATE ROLE InventoryTrackingApp;

GRANT SELECT, INSERT, UPDATE ON dbo.Kategori TO InventoryTrackingApp;
GRANT DELETE ON dbo.Kategori TO InventoryTrackingApp;
GRANT SELECT ON dbo.Birim TO InventoryTrackingApp;
GRANT SELECT, INSERT, UPDATE ON dbo.Depo TO InventoryTrackingApp;
GRANT SELECT, INSERT, UPDATE ON dbo.MalzemeKatalog TO InventoryTrackingApp;
GRANT SELECT, INSERT, UPDATE ON dbo.Urun TO InventoryTrackingApp;
GRANT SELECT, INSERT, UPDATE ON dbo.Stok TO InventoryTrackingApp;
GRANT SELECT, INSERT ON dbo.StokHareketi TO InventoryTrackingApp;
GRANT SELECT, INSERT ON dbo.StokTransferi TO InventoryTrackingApp;
GRANT EXECUTE ON dbo.sp_GetUrunByBarkod TO InventoryTrackingApp;
GRANT EXECUTE ON dbo.sp_GetAllUrunler TO InventoryTrackingApp;
GRANT EXECUTE ON dbo.sp_GetStokByDepo TO InventoryTrackingApp;
