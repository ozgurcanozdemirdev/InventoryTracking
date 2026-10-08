-- Var olan verileri silmeden transfer kaydı ve hareket bağlantısı ekler.
SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.StokTransferi', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.StokTransferi
        (
            TransferId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_StokTransferi PRIMARY KEY,
            UrunId INT NOT NULL,
            KaynakDepoId INT NOT NULL,
            HedefDepoId INT NOT NULL,
            Miktar INT NOT NULL,
            Aciklama NVARCHAR(500) NOT NULL,
            KullaniciAdi NVARCHAR(100) NULL,
            TransferTarihi DATETIME NOT NULL CONSTRAINT DF_StokTransferi_Tarih DEFAULT GETDATE(),
            CONSTRAINT FK_StokTransferi_Urun FOREIGN KEY (UrunId) REFERENCES dbo.Urun(UrunId),
            CONSTRAINT FK_StokTransferi_Kaynak FOREIGN KEY (KaynakDepoId) REFERENCES dbo.Depo(DepoId),
            CONSTRAINT FK_StokTransferi_Hedef FOREIGN KEY (HedefDepoId) REFERENCES dbo.Depo(DepoId),
            CONSTRAINT CK_StokTransferi_Miktar CHECK (Miktar > 0),
            CONSTRAINT CK_StokTransferi_Depolar CHECK (KaynakDepoId <> HedefDepoId),
            CONSTRAINT CK_StokTransferi_Aciklama CHECK (LEN(LTRIM(RTRIM(Aciklama))) > 0)
        );
    END;

    IF COL_LENGTH(N'dbo.StokHareketi', N'TransferId') IS NULL
        ALTER TABLE dbo.StokHareketi ADD TransferId INT NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StokHareketi_Transfer')
        EXEC(N'ALTER TABLE dbo.StokHareketi WITH CHECK ADD CONSTRAINT FK_StokHareketi_Transfer
            FOREIGN KEY (TransferId) REFERENCES dbo.StokTransferi(TransferId)');

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.StokHareketi')
                   AND name = N'UX_StokHareketi_TransferYonu')
        EXEC(N'CREATE UNIQUE INDEX UX_StokHareketi_TransferYonu
            ON dbo.StokHareketi(TransferId, CikisMi) WHERE TransferId IS NOT NULL');

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
