-- Bağlı olduğunuz InventoryTracking veritabanında çalıştırın.
-- Mevcut kayıtlar korunur. Her aktif depoya 20 örnek ürünün stok kaydı eklenir.
-- Yeniden çalıştırmak aynı ürünleri ve stok hareketlerini çoğaltmaz.
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.MalzemeKatalog', N'U') IS NULL
        OR COL_LENGTH(N'dbo.StokHareketi', N'CikisMi') IS NULL
        THROW 50100, N'Önce InventoryTracking veritabanı kurulumunu tamamlayın.', 1;

    DECLARE @Depolar TABLE (DepoId INT NOT NULL PRIMARY KEY, Sira INT NOT NULL);
    INSERT INTO @Depolar (DepoId, Sira)
    SELECT DepoId, ROW_NUMBER() OVER (ORDER BY DepoId)
    FROM dbo.Depo WITH (UPDLOCK, HOLDLOCK) WHERE AktifMi = 1;
    IF NOT EXISTS (SELECT 1 FROM @Depolar)
        THROW 50101, N'Örnek stok eklemek için en az bir aktif depo gerekir.', 1;

    DECLARE @Ornekler TABLE
    (
        Sira INT NOT NULL PRIMARY KEY,
        Barkod NVARCHAR(50) NOT NULL,
        MalzemeNo NVARCHAR(50) NOT NULL,
        UrunAdi NVARCHAR(200) NOT NULL,
        KategoriAdi NVARCHAR(100) NOT NULL,
        Fiyat DECIMAL(18,2) NOT NULL
    );
    INSERT INTO @Ornekler (Sira, Barkod, MalzemeNo, UrunAdi, KategoriAdi, Fiyat)
    VALUES
        (1, N'2000000000015', N'1001', N'Pirinç 1 kg', N'Gıda', 79.90),
        (2, N'2000000000022', N'1002', N'Bulgur 1 kg', N'Gıda', 44.90),
        (3, N'2000000000039', N'1003', N'Makarna 500 g', N'Gıda', 29.90),
        (4, N'2000000000046', N'1004', N'Un 1 kg', N'Gıda', 39.90),
        (5, N'2000000000053', N'1005', N'Toz Şeker 1 kg', N'Gıda', 54.90),
        (6, N'2000000000060', N'1006', N'Mercimek 1 kg', N'Gıda', 69.90),
        (7, N'2000000000077', N'1007', N'Nohut 1 kg', N'Gıda', 89.90),
        (8, N'2000000000084', N'1008', N'Domates Salçası 830 g', N'Gıda', 64.90),
        (9, N'2000000000091', N'1009', N'Bisküvi 200 g', N'Gıda', 24.90),
        (10, N'2000000000107', N'1010', N'Zeytinyağı 1 L', N'Gıda', 249.90),
        (11, N'2000000000114', N'1011', N'Maden Suyu 200 ml', N'İçecek', 12.50),
        (12, N'2000000000121', N'1012', N'Portakal Suyu 1 L', N'İçecek', 59.90),
        (13, N'2000000000138', N'1013', N'Soğuk Çay 1 L', N'İçecek', 44.90),
        (14, N'2000000000145', N'1014', N'Gazoz 1 L', N'İçecek', 34.90),
        (15, N'2000000000152', N'1015', N'Türk Kahvesi 100 g', N'Gıda', 74.90),
        (16, N'2000000000169', N'1016', N'Bulaşık Deterjanı 750 ml', N'Temizlik', 69.90),
        (17, N'2000000000176', N'1017', N'Sıvı Sabun 500 ml', N'Temizlik', 49.90),
        (18, N'2000000000183', N'1018', N'Yüzey Temizleyici 1 L', N'Temizlik', 54.90),
        (19, N'2000000000190', N'1019', N'Kağıt Havlu 2 Rulo', N'Temizlik', 39.90),
        (20, N'2000000000206', N'1020', N'Çöp Torbası 20 Adet', N'Temizlik', 29.90);

    -- Örnek kodları başka kayıtlarda kullanılmışsa mevcut veriyi değiştirmeyiz.
    IF EXISTS
    (
        SELECT 1 FROM @Ornekler o JOIN dbo.MalzemeKatalog m WITH (UPDLOCK, HOLDLOCK)
            ON m.Barkod = o.Barkod OR m.MalzemeNo = o.MalzemeNo
        WHERE m.Barkod <> o.Barkod OR m.MalzemeNo <> o.MalzemeNo OR m.Aciklama <> o.UrunAdi
    ) OR EXISTS
    (
        SELECT 1 FROM @Ornekler o JOIN dbo.Urun u WITH (UPDLOCK, HOLDLOCK)
            ON u.Barkod = o.Barkod OR u.UrunAdi = o.UrunAdi
        WHERE u.Barkod <> o.Barkod OR u.UrunAdi <> o.UrunAdi OR ISNULL(u.AktifMi, 0) <> 1
    )
        THROW 50102, N'Örnek ürün kodları mevcut kayıtlarla çakışıyor; hiçbir veri değiştirilmedi.', 1;

    INSERT INTO dbo.Kategori (KategoriAdi, Aciklama)
    SELECT DISTINCT o.KategoriAdi, N'Örnek ürün kategorisi'
    FROM @Ornekler o
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Kategori k WITH (UPDLOCK, HOLDLOCK) WHERE k.KategoriAdi = o.KategoriAdi);

    IF NOT EXISTS (SELECT 1 FROM dbo.Birim WITH (UPDLOCK, HOLDLOCK) WHERE BirimAdi = N'Adet')
        INSERT INTO dbo.Birim (BirimAdi, Kisaltma) VALUES (N'Adet', N'Ad');
    DECLARE @BirimId INT = (SELECT BirimId FROM dbo.Birim WHERE BirimAdi = N'Adet');
    DECLARE @IlkTarih DATETIME = DATEADD(DAY, -19, CONVERT(DATETIME, CONVERT(DATE, GETDATE())));

    INSERT INTO dbo.MalzemeKatalog (MalzemeNo, Barkod, Aciklama, BirimId, OlusturulmaTarihi)
    SELECT o.MalzemeNo, o.Barkod, o.UrunAdi, @BirimId, @IlkTarih
    FROM @Ornekler o
    WHERE NOT EXISTS (SELECT 1 FROM dbo.MalzemeKatalog m WITH (UPDLOCK, HOLDLOCK) WHERE m.Barkod = o.Barkod);

    INSERT INTO dbo.Urun (MalzemeId, Barkod, UrunAdi, KategoriId, BirimId, Fiyat,
        MinimumStok, MaksimumStok, AktifMi, OlusturulmaTarihi, GuncellemeTarihi)
    SELECT m.MalzemeId, o.Barkod, o.UrunAdi, k.KategoriId, m.BirimId, o.Fiyat,
        10, 200, 1, @IlkTarih, @IlkTarih
    FROM @Ornekler o
    JOIN dbo.MalzemeKatalog m ON m.Barkod = o.Barkod
    JOIN dbo.Kategori k ON k.KategoriAdi = o.KategoriAdi
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Urun u WITH (UPDLOCK, HOLDLOCK) WHERE u.Barkod = o.Barkod);
    DECLARE @EklenenUrun INT = @@ROWCOUNT;

    DECLARE @YeniStoklar TABLE (UrunId INT, DepoId INT, Miktar INT, Tarih DATETIME);
    -- Az, normal ve fazla stok örnekleri ana ekrandaki stok uyarılarını da gösterir.
    INSERT INTO dbo.Stok (UrunId, DepoId, Miktar, OlusturulmaTarihi, GuncellemeTarihi)
    OUTPUT inserted.UrunId, inserted.DepoId, inserted.Miktar, inserted.OlusturulmaTarihi
        INTO @YeniStoklar (UrunId, DepoId, Miktar, Tarih)
    SELECT u.UrunId, d.DepoId,
        CASE WHEN o.Sira % 5 = 0 THEN 5 WHEN o.Sira % 7 = 0 THEN 250 ELSE 20 + o.Sira * 7 + d.Sira * 10 END,
        DATEADD(HOUR, 9, DATEADD(DAY, -(o.Sira - 1), CONVERT(DATETIME, CONVERT(DATE, GETDATE())))),
        DATEADD(HOUR, 9, DATEADD(DAY, -(o.Sira - 1), CONVERT(DATETIME, CONVERT(DATE, GETDATE()))))
    FROM @Ornekler o JOIN dbo.Urun u ON u.Barkod = o.Barkod CROSS JOIN @Depolar d
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Stok s WITH (UPDLOCK, HOLDLOCK) WHERE s.UrunId = u.UrunId AND s.DepoId = d.DepoId);

    -- Başlangıç miktarları hareket kaydıyla eşleşir; raporlar da dolu açılır.
    INSERT INTO dbo.StokHareketi (UrunId, DepoId, OncekiMiktar, YeniMiktar, DegisenMiktar,
        HareketTipi, Aciklama, KullaniciAdi, HareketTarihi, CikisMi, TransferId)
    SELECT UrunId, DepoId, 0, Miktar, Miktar, N'Stok Girişi',
        N'Örnek veri: başlangıç stok girişi', N'Demo', Tarih, 0, NULL
    FROM @YeniStoklar;

    IF EXISTS
    (
        SELECT d.DepoId FROM @Depolar d CROSS JOIN @Ornekler o
        JOIN dbo.Urun u ON u.Barkod = o.Barkod
        LEFT JOIN dbo.Stok s ON s.UrunId = u.UrunId AND s.DepoId = d.DepoId
        GROUP BY d.DepoId HAVING COUNT(s.StokId) <> 20
    )
        THROW 50103, N'Her aktif depoda 20 örnek stok kaydı doğrulanamadı.', 1;

    DECLARE @EklenenStok INT = (SELECT COUNT(*) FROM @YeniStoklar);
    COMMIT TRANSACTION;
    SELECT @EklenenUrun AS EklenenUrun, @EklenenStok AS EklenenStok,
        @EklenenStok AS EklenenHareket, (SELECT COUNT(*) FROM @Depolar) AS DepoSayisi;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
