-- Mevcut kayıtları silmeden ürün adında büyük/küçük harf farkıyla tekrarları engeller.
-- SSMS'te doğru veritabanını seçip çalıştırın; çakışma varsa işlem hata verip geri alınır.
SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.Urun', N'U') IS NULL
        THROW 50001, N'Urun tablosu bulunamadı.', 1;

    IF (SELECT collation_name FROM sys.columns
        WHERE object_id = OBJECT_ID(N'dbo.Urun') AND name = N'UrunAdi') <> N'Turkish_CI_AS'
        THROW 50002, N'UrunAdi sütununun karşılaştırma ayarı beklenen Turkish_CI_AS değil.', 1;

    IF EXISTS (SELECT 1 FROM dbo.Urun
               WHERE DATALENGTH(UrunAdi) <> DATALENGTH(LTRIM(RTRIM(UrunAdi))))
        THROW 50003, N'Başında veya sonunda boşluk bulunan ürün adları var. Önce bu kayıtları inceleyin.', 1;

    IF EXISTS (SELECT 1 FROM dbo.Urun GROUP BY UrunAdi HAVING COUNT(*) > 1)
        THROW 50004, N'Aynı adı taşıyan ürünler var. Önce bu kayıtları inceleyin.', 1;

    IF NOT EXISTS (SELECT 1 FROM sys.check_constraints
                   WHERE parent_object_id = OBJECT_ID(N'dbo.Urun') AND name = N'CK_Urun_UrunAdi_Bosluk')
        ALTER TABLE dbo.Urun WITH CHECK ADD CONSTRAINT CK_Urun_UrunAdi_Bosluk
            CHECK (DATALENGTH(UrunAdi) = DATALENGTH(LTRIM(RTRIM(UrunAdi))));

    IF NOT EXISTS (SELECT 1 FROM sys.indexes
                   WHERE object_id = OBJECT_ID(N'dbo.Urun') AND name = N'UX_Urun_UrunAdi')
        CREATE UNIQUE INDEX UX_Urun_UrunAdi ON dbo.Urun(UrunAdi);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
