-- Katalog birimi değiştiğinde bağlı ürünün birimini aynı SQL işlemiyle günceller.
-- Mevcut verileri silmez; hata halinde şema değişikliğini geri alır.
SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.MalzemeKatalog', N'U') IS NULL
        THROW 50020, N'Önce malzeme kataloğu geçişini uygulayın.', 1;

    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys
                   WHERE name = N'FK_Urun_MalzemeBag' AND update_referential_action_desc = N'CASCADE')
    BEGIN
        IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Urun_MalzemeBag')
            ALTER TABLE dbo.Urun DROP CONSTRAINT FK_Urun_MalzemeBag;

        ALTER TABLE dbo.Urun WITH CHECK ADD CONSTRAINT FK_Urun_MalzemeBag
            FOREIGN KEY (MalzemeId, Barkod, BirimId)
            REFERENCES dbo.MalzemeKatalog(MalzemeId, Barkod, BirimId) ON UPDATE CASCADE;
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
