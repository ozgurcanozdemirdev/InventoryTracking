-- Mevcut ürünleri ve hareketleri silmeden katalog ile BIT yön bilgisini ekler.
-- SSMS'te InventoryTracking veritabanında çalıştırın; hata olursa tüm işlem geri alınır.
SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.Urun', N'U') IS NULL OR OBJECT_ID(N'dbo.StokHareketi', N'U') IS NULL
        THROW 50010, N'Urun veya StokHareketi tablosu bulunamadı.', 1;

    IF OBJECT_ID(N'dbo.MalzemeKatalog', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.MalzemeKatalog
        (
            MalzemeId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MalzemeKatalog PRIMARY KEY,
            MalzemeNo NVARCHAR(50) NOT NULL,
            Barkod NVARCHAR(50) NOT NULL,
            Aciklama NVARCHAR(200) NOT NULL,
            BirimId INT NOT NULL,
            OlusturulmaTarihi DATETIME NOT NULL CONSTRAINT DF_MalzemeKatalog_OlusturulmaTarihi DEFAULT GETDATE(),
            CONSTRAINT UQ_MalzemeKatalog_MalzemeNo UNIQUE (MalzemeNo),
            CONSTRAINT UQ_MalzemeKatalog_Barkod UNIQUE (Barkod),
            CONSTRAINT FK_MalzemeKatalog_Birim FOREIGN KEY (BirimId) REFERENCES dbo.Birim(BirimId),
            CONSTRAINT CK_MalzemeKatalog_MalzemeNo CHECK (LEN(LTRIM(RTRIM(MalzemeNo))) > 0),
            CONSTRAINT CK_MalzemeKatalog_Aciklama CHECK (LEN(LTRIM(RTRIM(Aciklama))) > 0)
        );
    END;

    IF COL_LENGTH(N'dbo.Urun', N'MalzemeId') IS NULL
        ALTER TABLE dbo.Urun ADD MalzemeId INT NULL;

    -- Mevcut ürün kimliğini sade malzeme numarası olarak kullanıyoruz.
    -- Hedef numara önceden başka barkoda verilmişse geçişin tamamı geri alınır.
    IF EXISTS
    (
        SELECT 1 FROM dbo.Urun AS u
        INNER JOIN dbo.MalzemeKatalog AS m WITH (UPDLOCK, HOLDLOCK)
            ON m.MalzemeNo = CONVERT(NVARCHAR(20), u.UrunId)
        WHERE NOT EXISTS (SELECT 1 FROM dbo.MalzemeKatalog AS mevcut WHERE mevcut.Barkod = u.Barkod)
    )
        THROW 50013, N'Malzeme numarası başka bir barkoda ait; katalog geçişi geri alındı.', 1;

    -- Barkod, açıklama ve birim korunur; yeni numaraya metin öneki eklenmez.
    INSERT INTO dbo.MalzemeKatalog (MalzemeNo, Barkod, Aciklama, BirimId)
    SELECT CONVERT(NVARCHAR(20), u.UrunId), u.Barkod, u.UrunAdi, u.BirimId
    FROM dbo.Urun AS u
    WHERE NOT EXISTS (SELECT 1 FROM dbo.MalzemeKatalog AS m WHERE m.Barkod = u.Barkod);

    -- Yeni eklenen sütun aynı SQL batch'inde statik sorguyla okunamaz; dinamik batch kullanılır.
    EXEC sys.sp_executesql N'
        UPDATE u SET MalzemeId = m.MalzemeId
        FROM dbo.Urun AS u
        INNER JOIN dbo.MalzemeKatalog AS m ON m.Barkod = u.Barkod
        WHERE u.MalzemeId IS NULL;
        IF EXISTS (SELECT 1 FROM dbo.Urun AS u
                   LEFT JOIN dbo.MalzemeKatalog AS m ON m.MalzemeId = u.MalzemeId
                   WHERE m.MalzemeId IS NULL OR m.Barkod <> u.Barkod OR m.BirimId <> u.BirimId)
            THROW 50011, N''Ürün ve malzeme kataloğu eşleşmiyor; geçiş geri alındı.'', 1;
        IF EXISTS (SELECT MalzemeId FROM dbo.Urun GROUP BY MalzemeId HAVING COUNT(*) > 1)
            THROW 50012, N''Bir malzemeye birden fazla ürün bağlı; geçiş geri alındı.'', 1;';

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Urun')
               AND name = N'MalzemeId' AND is_nullable = 1)
        ALTER TABLE dbo.Urun ALTER COLUMN MalzemeId INT NOT NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Urun_MalzemeKatalog')
        ALTER TABLE dbo.Urun WITH CHECK ADD CONSTRAINT FK_Urun_MalzemeKatalog
            FOREIGN KEY (MalzemeId) REFERENCES dbo.MalzemeKatalog(MalzemeId);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Urun')
                   AND name = N'UX_Urun_MalzemeId')
        CREATE UNIQUE INDEX UX_Urun_MalzemeId ON dbo.Urun(MalzemeId);

    -- Uygulama dışından yazılan kayıtta da ürünün barkodu ve birimi katalogla aynı olmalı.
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.MalzemeKatalog')
                   AND name = N'UX_MalzemeKatalog_Bag')
        CREATE UNIQUE INDEX UX_MalzemeKatalog_Bag ON dbo.MalzemeKatalog(MalzemeId, Barkod, BirimId);

    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Urun_MalzemeBag')
        ALTER TABLE dbo.Urun WITH CHECK ADD CONSTRAINT FK_Urun_MalzemeBag
            FOREIGN KEY (MalzemeId, Barkod, BirimId)
            REFERENCES dbo.MalzemeKatalog(MalzemeId, Barkod, BirimId) ON UPDATE CASCADE;

    IF COL_LENGTH(N'dbo.StokHareketi', N'CikisMi') IS NULL
        ALTER TABLE dbo.StokHareketi ADD CikisMi BIT NULL;

    -- Eski hareketlerin yönü işaretli miktardan hesaplanır: 0 giriş, 1 çıkış.
    EXEC sys.sp_executesql N'UPDATE dbo.StokHareketi
        SET CikisMi = CASE WHEN DegisenMiktar < 0 THEN 1 ELSE 0 END
        WHERE CikisMi IS NULL;';

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.StokHareketi')
               AND name = N'CikisMi' AND is_nullable = 1)
        ALTER TABLE dbo.StokHareketi ALTER COLUMN CikisMi BIT NOT NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE parent_object_id = OBJECT_ID(N'dbo.StokHareketi')
                   AND name = N'CK_StokHareketi_CikisMi')
        EXEC sys.sp_executesql N'ALTER TABLE dbo.StokHareketi WITH CHECK ADD CONSTRAINT CK_StokHareketi_CikisMi
            CHECK ((CikisMi = 0 AND DegisenMiktar >= 0) OR (CikisMi = 1 AND DegisenMiktar < 0));';

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
