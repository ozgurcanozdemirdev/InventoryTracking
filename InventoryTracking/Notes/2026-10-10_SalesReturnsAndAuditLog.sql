-- Mevcut veritabanında çalıştırılır. Eski ürün, stok ve hareket kayıtları silinmez.
-- Satış/iade kaydı ve değişiklik günlüğü aynı veritabanında tutulur.
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;
    IF OBJECT_ID(N'dbo.StokHareketi',N'U') IS NULL THROW 50210,N'Önce temel şemayı kurun.',1;
    IF OBJECT_ID(N'dbo.IslemGunlugu',N'U') IS NULL
    CREATE TABLE dbo.IslemGunlugu
    (
        LogId BIGINT IDENTITY PRIMARY KEY, IslemTarihi DATETIME2(3) NOT NULL DEFAULT SYSDATETIME(),
        TabloAdi NVARCHAR(128) NOT NULL, KayitId BIGINT NULL, Islem NVARCHAR(50) NOT NULL,
        Basarili BIT NOT NULL DEFAULT 1, KullaniciAdi NVARCHAR(100) NOT NULL, SqlKullanici NVARCHAR(128) NOT NULL,
        OncekiVeri NVARCHAR(MAX) NULL, YeniVeri NVARCHAR(MAX) NULL, Aciklama NVARCHAR(1000) NULL
    );
    IF OBJECT_ID(N'dbo.SatisIade',N'U') IS NULL
    CREATE TABLE dbo.SatisIade
    (
        IslemId BIGINT IDENTITY PRIMARY KEY, IslemNo UNIQUEIDENTIFIER NOT NULL UNIQUE,
        HareketId INT NOT NULL UNIQUE REFERENCES dbo.StokHareketi(HareketId),
        UrunId INT NOT NULL REFERENCES dbo.Urun(UrunId), DepoId INT NOT NULL REFERENCES dbo.Depo(DepoId),
        IadeMi BIT NOT NULL, UrunAdi NVARCHAR(200) NOT NULL, Barkod NVARCHAR(50) NOT NULL,
        Miktar INT NOT NULL CHECK(Miktar>0), BirimFiyat DECIMAL(18,2) NOT NULL CHECK(BirimFiyat>=0),
        ToplamTutar AS (CONVERT(DECIMAL(28,2),BirimFiyat*CONVERT(DECIMAL(10,0),Miktar))) PERSISTED,
        OncekiMiktar INT NOT NULL, YeniMiktar INT NOT NULL CHECK(YeniMiktar>=0),
        KullaniciAdi NVARCHAR(100) NOT NULL, IslemTarihi DATETIME2(3) NOT NULL DEFAULT SYSDATETIME(),
        CONSTRAINT CK_SatisIade_Miktar CHECK (CONVERT(BIGINT,YeniMiktar)-OncekiMiktar=CASE WHEN IadeMi=1 THEN Miktar ELSE -Miktar END)
    );
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.SatisIade') AND name=N'IX_SatisIade_Tarih')
        CREATE INDEX IX_SatisIade_Tarih ON dbo.SatisIade(DepoId,IslemTarihi DESC) INCLUDE(UrunId,IadeMi,Miktar,BirimFiyat);

    -- Her satır ayrı kaydedilir; toplu ekleme/güncellemede de kayıt kaybı olmaz.
    DECLARE @Tablolar TABLE(Sira INT IDENTITY,Tablo SYSNAME,Kimlik SYSNAME);
    INSERT @Tablolar(Tablo,Kimlik) VALUES (N'Kategori',N'KategoriId'),(N'Birim',N'BirimId'),(N'Depo',N'DepoId'),
        (N'MalzemeKatalog',N'MalzemeId'),(N'Urun',N'UrunId'),(N'Stok',N'StokId'),
        (N'StokTransferi',N'TransferId'),(N'StokHareketi',N'HareketId'),(N'SatisIade',N'IslemId');
    DECLARE @Sira INT=1,@Tablo SYSNAME,@Kimlik SYSNAME,@Sql NVARCHAR(MAX);
    WHILE @Sira<=(SELECT COUNT(*) FROM @Tablolar)
    BEGIN
        SELECT @Tablo=Tablo,@Kimlik=Kimlik FROM @Tablolar WHERE Sira=@Sira;
        SET @Sql=N'CREATE OR ALTER TRIGGER dbo.'+QUOTENAME(N'tr_Log_'+@Tablo)+N' ON dbo.'+QUOTENAME(@Tablo)+N'
        AFTER INSERT,UPDATE,DELETE AS
        BEGIN
            IF ROWCOUNT_BIG()=0 RETURN;
            SET NOCOUNT ON;
            INSERT dbo.IslemGunlugu(TabloAdi,KayitId,Islem,KullaniciAdi,SqlKullanici,OncekiVeri,YeniVeri)
            SELECT N'''+@Tablo+N''',COALESCE(i.'+QUOTENAME(@Kimlik)+N',d.'+QUOTENAME(@Kimlik)+N'),
                CASE WHEN d.'+QUOTENAME(@Kimlik)+N' IS NULL THEN N''Ekleme'' WHEN i.'+QUOTENAME(@Kimlik)+N' IS NULL THEN N''Silme'' ELSE N''Güncelleme'' END,
                COALESCE(CONVERT(NVARCHAR(100),SESSION_CONTEXT(N''KullaniciAdi'')),ORIGINAL_LOGIN()),ORIGINAL_LOGIN(),
                CASE WHEN d.'+QUOTENAME(@Kimlik)+N' IS NOT NULL THEN (SELECT d.* FOR JSON PATH,INCLUDE_NULL_VALUES,WITHOUT_ARRAY_WRAPPER) END,
                CASE WHEN i.'+QUOTENAME(@Kimlik)+N' IS NOT NULL THEN (SELECT i.* FOR JSON PATH,INCLUDE_NULL_VALUES,WITHOUT_ARRAY_WRAPPER) END
            FROM inserted i FULL OUTER JOIN deleted d ON i.'+QUOTENAME(@Kimlik)+N'=d.'+QUOTENAME(@Kimlik)+N';
        END;';
        EXEC sys.sp_executesql @Sql;
        SET @Sira+=1;
    END;
    EXEC(N'CREATE OR ALTER PROCEDURE dbo.sp_SatisIadeKaydet
    @IslemNo UNIQUEIDENTIFIER, @UrunId INT, @DepoId INT, @Miktar INT, @IadeMi BIT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    DECLARE @Kullanici NVARCHAR(100) = COALESCE(CONVERT(NVARCHAR(100), SESSION_CONTEXT(N''KullaniciAdi'')), ORIGINAL_LOGIN());
    DECLARE @Tip NVARCHAR(20) = CASE WHEN @IadeMi=1 THEN N''İade'' ELSE N''Satış'' END;
    BEGIN TRY
        BEGIN TRANSACTION;
        IF @IslemNo IS NULL OR @UrunId<=0 OR @DepoId<=0 OR @Miktar<=0 OR @Miktar>1000000 OR @IadeMi IS NULL
            THROW 50200, N''Geçerli ürün, depo ve pozitif adet gereklidir.'', 1;

        -- Aynı istek tekrar gönderilirse stok ikinci kez değiştirilmez.
        IF EXISTS (SELECT 1 FROM dbo.SatisIade WITH (UPDLOCK,HOLDLOCK) WHERE IslemNo=@IslemNo)
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM dbo.SatisIade WHERE IslemNo=@IslemNo AND UrunId=@UrunId
                AND DepoId=@DepoId AND Miktar=@Miktar AND IadeMi=@IadeMi AND KullaniciAdi=@Kullanici)
                THROW 50201, N''İşlem numarası başka bir işlem için kullanılmış.'', 1;
            COMMIT TRANSACTION;
            SELECT IslemId, Miktar, YeniMiktar, BirimFiyat, ToplamTutar FROM dbo.SatisIade WHERE IslemNo=@IslemNo;
            RETURN;
        END;

        DECLARE @Ad NVARCHAR(200), @Barkod NVARCHAR(50), @Fiyat DECIMAL(18,2);
        SELECT @Ad=UrunAdi,@Barkod=Barkod,@Fiyat=Fiyat FROM dbo.Urun WITH (UPDLOCK,HOLDLOCK)
        WHERE UrunId=@UrunId AND AktifMi=1;
        IF @Ad IS NULL OR NOT EXISTS (SELECT 1 FROM dbo.Depo WITH (UPDLOCK,HOLDLOCK) WHERE DepoId=@DepoId AND AktifMi=1)
            THROW 50202, N''Ürün ve depo aktif olmalıdır.'', 1;
        DECLARE @Eski INT=0, @StokId INT;
        SELECT @Eski=Miktar,@StokId=StokId FROM dbo.Stok WITH (UPDLOCK,HOLDLOCK)
        WHERE UrunId=@UrunId AND DepoId=@DepoId;
        IF @IadeMi=0 AND @Eski<@Miktar
            THROW 50203, N''Satış için yeterli stok bulunmuyor.'', 1;
        DECLARE @Yeni BIGINT=CONVERT(BIGINT,@Eski)+CASE WHEN @IadeMi=1 THEN @Miktar ELSE -@Miktar END;
        IF @Yeni>2147483647 THROW 50204, N''Stok miktarı izin verilen sınırı aşıyor.'', 1;
        IF @StokId IS NULL
            INSERT dbo.Stok (UrunId,DepoId,Miktar) VALUES (@UrunId,@DepoId,@Yeni);
        ELSE UPDATE dbo.Stok SET Miktar=@Yeni,GuncellemeTarihi=GETDATE() WHERE StokId=@StokId;
        DECLARE @Tarih DATETIME2(3)=SYSDATETIME();
        INSERT dbo.StokHareketi (UrunId,DepoId,OncekiMiktar,YeniMiktar,DegisenMiktar,CikisMi,HareketTipi,Aciklama,KullaniciAdi,HareketTarihi)
        VALUES (@UrunId,@DepoId,@Eski,@Yeni,CONVERT(INT,@Yeni-@Eski),CASE WHEN @IadeMi=1 THEN 0 ELSE 1 END,
            @Tip,N''Barkod işlemi: ''+CONVERT(NVARCHAR(36),@IslemNo),@Kullanici,@Tarih);
        DECLARE @HareketId INT=SCOPE_IDENTITY();
        -- İsim ve fiyat anlık görüntüdür; sonradan ürün değişse de geçmiş değişmez.
        INSERT dbo.SatisIade (IslemNo,HareketId,UrunId,DepoId,IadeMi,UrunAdi,Barkod,Miktar,BirimFiyat,OncekiMiktar,YeniMiktar,KullaniciAdi,IslemTarihi)
        VALUES (@IslemNo,@HareketId,@UrunId,@DepoId,@IadeMi,@Ad,@Barkod,@Miktar,@Fiyat,@Eski,@Yeni,@Kullanici,@Tarih);
        COMMIT TRANSACTION;
        SELECT IslemId,Miktar,YeniMiktar,BirimFiyat,ToplamTutar FROM dbo.SatisIade WHERE IslemNo=@IslemNo;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
        -- Reddedilen deneme stok geçmişine yazılmaz; ayrı günlükte hata olarak saklanır.
        INSERT dbo.IslemGunlugu (TabloAdi,KayitId,Islem,Basarili,KullaniciAdi,SqlKullanici,Aciklama)
        VALUES (N''SatisIade'',@UrunId,@Tip,0,@Kullanici,ORIGINAL_LOGIN(),ERROR_MESSAGE());
        THROW;
    END CATCH;
END;
');
    EXEC(N'CREATE OR ALTER PROCEDURE dbo.sp_UygulamaOlayiKaydet @Eylem NVARCHAR(50),@Aciklama NVARCHAR(1000)
    AS BEGIN SET NOCOUNT ON;
      INSERT dbo.IslemGunlugu(TabloAdi,Islem,KullaniciAdi,SqlKullanici,Aciklama)
      VALUES(N''Uygulama'',@Eylem,COALESCE(CONVERT(NVARCHAR(100),SESSION_CONTEXT(N''KullaniciAdi'')),ORIGINAL_LOGIN()),ORIGINAL_LOGIN(),@Aciklama);
    END;');
    IF DATABASE_PRINCIPAL_ID(N'InventoryTrackingApp') IS NULL CREATE ROLE InventoryTrackingApp;
    GRANT SELECT ON dbo.SatisIade TO InventoryTrackingApp;
    GRANT SELECT ON dbo.IslemGunlugu TO InventoryTrackingApp;
    GRANT EXECUTE ON dbo.sp_SatisIadeKaydet TO InventoryTrackingApp;
    GRANT EXECUTE ON dbo.sp_UygulamaOlayiKaydet TO InventoryTrackingApp;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
