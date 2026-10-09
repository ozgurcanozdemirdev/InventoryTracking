-- Önceden eklenen 20 ürünün kodlarını ve kategori yazımını düzenler.
-- Ürün/depo kimlikleri, stoklar ve hareketler korunur; hata halinde tüm işlem geri alınır.
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_Urun_MalzemeBag'
        AND parent_object_id=OBJECT_ID(N'dbo.Urun') AND update_referential_action=1)
        THROW 50110, N'Katalog barkodunu bağlı ürüne aktaran güncelleme kuralı bulunamadı.', 1;

    DECLARE @Kodlar TABLE (EskiBarkod NVARCHAR(50), Barkod NVARCHAR(50), MalzemeNo NVARCHAR(50));
    INSERT INTO @Kodlar (EskiBarkod, Barkod, MalzemeNo) VALUES
        (N'DEMO-0001', N'2000000000015', N'1001'),
        (N'DEMO-0002', N'2000000000022', N'1002'),
        (N'DEMO-0003', N'2000000000039', N'1003'),
        (N'DEMO-0004', N'2000000000046', N'1004'),
        (N'DEMO-0005', N'2000000000053', N'1005'),
        (N'DEMO-0006', N'2000000000060', N'1006'),
        (N'DEMO-0007', N'2000000000077', N'1007'),
        (N'DEMO-0008', N'2000000000084', N'1008'),
        (N'DEMO-0009', N'2000000000091', N'1009'),
        (N'DEMO-0010', N'2000000000107', N'1010'),
        (N'DEMO-0011', N'2000000000114', N'1011'),
        (N'DEMO-0012', N'2000000000121', N'1012'),
        (N'DEMO-0013', N'2000000000138', N'1013'),
        (N'DEMO-0014', N'2000000000145', N'1014'),
        (N'DEMO-0015', N'2000000000152', N'1015'),
        (N'DEMO-0016', N'2000000000169', N'1016'),
        (N'DEMO-0017', N'2000000000176', N'1017'),
        (N'DEMO-0018', N'2000000000183', N'1018'),
        (N'DEMO-0019', N'2000000000190', N'1019'),
        (N'DEMO-0020', N'2000000000206', N'1020');
    DECLARE @Kayitlar TABLE (UrunId INT PRIMARY KEY, MalzemeId INT, Barkod NVARCHAR(50),
        MalzemeNo NVARCHAR(50), UrunAdi NVARCHAR(200));
    INSERT INTO @Kayitlar (UrunId, MalzemeId, Barkod, MalzemeNo, UrunAdi)
    SELECT u.UrunId, m.MalzemeId, k.Barkod, k.MalzemeNo,
        CASE WHEN LEFT(u.UrunAdi,8)=N'Örnek - ' THEN SUBSTRING(u.UrunAdi,9,200) ELSE u.UrunAdi END
    FROM @Kodlar k
    JOIN dbo.MalzemeKatalog m WITH (UPDLOCK,HOLDLOCK) ON m.Barkod=k.EskiBarkod
    JOIN dbo.Urun u WITH (UPDLOCK,HOLDLOCK) ON u.MalzemeId=m.MalzemeId AND u.Barkod=m.Barkod;

    -- Yeni kod veya isim başka bir kayda aitse hiçbir kayıt değiştirilmez.
    IF EXISTS (SELECT 1 FROM @Kayitlar k JOIN dbo.MalzemeKatalog m WITH (UPDLOCK,HOLDLOCK)
        ON (m.Barkod=k.Barkod OR m.MalzemeNo=k.MalzemeNo) AND m.MalzemeId<>k.MalzemeId)
        OR EXISTS (SELECT 1 FROM @Kayitlar k JOIN dbo.Urun u WITH (UPDLOCK,HOLDLOCK)
        ON (u.Barkod=k.Barkod OR u.UrunAdi=k.UrunAdi) AND u.UrunId<>k.UrunId)
        THROW 50111, N'Yeni ürün adı, barkod veya malzeme numarası başka bir kayıtla çakışıyor.', 1;

    DECLARE @Kategoriler TABLE (EskiAd NVARCHAR(100), YeniAd NVARCHAR(100),
        EskiAciklama NVARCHAR(500), YeniAciklama NVARCHAR(500));
    INSERT INTO @Kategoriler VALUES
        (N'Gida',N'Gıda',N'Gida urunleri',N'Gıda ürünleri'),
        (N'Icecek',N'İçecek',N'Su ve diger icecekler',N'Su ve diğer içecekler');
    IF EXISTS (SELECT 1 FROM @Kategoriler k
        JOIN dbo.Kategori eski WITH (UPDLOCK,HOLDLOCK) ON eski.KategoriAdi COLLATE Latin1_General_BIN2=k.EskiAd
        JOIN dbo.Kategori yeni WITH (UPDLOCK,HOLDLOCK) ON yeni.KategoriAdi=k.YeniAd AND yeni.KategoriId<>eski.KategoriId)
        THROW 50112, N'Düzeltilmiş kategori adı başka bir kategoride kullanılıyor.', 1;

    UPDATE m SET Barkod=k.Barkod,MalzemeNo=k.MalzemeNo,Aciklama=k.UrunAdi
    FROM dbo.MalzemeKatalog m JOIN @Kayitlar k ON k.MalzemeId=m.MalzemeId;
    -- Barkod, mevcut ON UPDATE CASCADE ilişkisiyle ürün tablosuna da aktarılır.
    UPDATE u SET UrunAdi=k.UrunAdi,GuncellemeTarihi=GETDATE()
    FROM dbo.Urun u JOIN @Kayitlar k ON k.UrunId=u.UrunId;
    DECLARE @UrunSayisi INT=@@ROWCOUNT;

    UPDATE c SET KategoriAdi=k.YeniAd,
        Aciklama=CASE WHEN c.Aciklama COLLATE Latin1_General_BIN2=k.EskiAciklama THEN k.YeniAciklama ELSE c.Aciklama END,
        GuncellemeTarihi=GETDATE()
    FROM dbo.Kategori c JOIN @Kategoriler k ON c.KategoriAdi COLLATE Latin1_General_BIN2=k.EskiAd;
    DECLARE @KategoriSayisi INT=@@ROWCOUNT;

    IF EXISTS (SELECT 1 FROM @Kayitlar k JOIN dbo.Urun u ON u.UrunId=k.UrunId
        JOIN dbo.MalzemeKatalog m ON m.MalzemeId=u.MalzemeId
        WHERE u.Barkod<>k.Barkod OR m.Barkod<>k.Barkod OR m.MalzemeNo<>k.MalzemeNo
            OR u.UrunAdi<>k.UrunAdi OR m.Aciklama<>k.UrunAdi)
        THROW 50113, N'Ürün ve katalog kodları eşleşmedi.', 1;

    COMMIT TRANSACTION;
    SELECT @UrunSayisi AS DuzenlenenUrun,@KategoriSayisi AS DuzenlenenKategori;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
