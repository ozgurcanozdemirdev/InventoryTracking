-- İlk katalog geçişinin otomatik eklediği LEGACY- önekini kaldırır.
-- Örneğin LEGACY-15 -> 15 olur; elle verilen malzeme numaraları korunur.
-- Ürün, barkod, birim, stok ve hareket ilişkileri MalzemeId üzerinden devam eder.
SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.MalzemeKatalog', N'U') IS NULL
        OR COL_LENGTH(N'dbo.Urun', N'MalzemeId') IS NULL
        THROW 50030, N'Önce malzeme kataloğu geçişini uygulayın.', 1;

    DECLARE @Numaralar TABLE
    (
        MalzemeId INT NOT NULL PRIMARY KEY,
        YeniNumara NVARCHAR(50) NOT NULL UNIQUE
    );

    -- Yalnızca eski geçişin ürettiği numara ile ilgili ürün kimliği tam eşleşirse
    -- değiştiriyoruz. Kullanıcının benzer biçimde girdiği farklı kodlara dokunulmaz.
    INSERT INTO @Numaralar (MalzemeId, YeniNumara)
    SELECT m.MalzemeId, CONVERT(NVARCHAR(20), u.UrunId)
    FROM dbo.MalzemeKatalog AS m WITH (UPDLOCK, HOLDLOCK)
    INNER JOIN dbo.Urun AS u WITH (HOLDLOCK) ON u.MalzemeId = m.MalzemeId
    WHERE m.MalzemeNo = N'LEGACY-' + CONVERT(NVARCHAR(20), u.UrunId);

    -- Hedef numara başka malzemeye aitse hiçbir kayıt değişmez.
    -- SQL benzersizlik kuralı da eşzamanlı yazımlarda son korumayı sağlar.
    IF EXISTS
    (
        SELECT 1 FROM @Numaralar AS n
        INNER JOIN dbo.MalzemeKatalog AS m WITH (UPDLOCK, HOLDLOCK)
            ON m.MalzemeNo = n.YeniNumara AND m.MalzemeId <> n.MalzemeId
    )
        THROW 50031, N'Malzeme numarası çakışması var; numaralar değiştirilmedi.', 1;

    UPDATE m SET MalzemeNo = n.YeniNumara
    FROM dbo.MalzemeKatalog AS m
    INNER JOIN @Numaralar AS n ON n.MalzemeId = m.MalzemeId;

    DECLARE @DegisenKayitSayisi INT = @@ROWCOUNT;
    COMMIT TRANSACTION;
    SELECT @DegisenKayitSayisi AS DegisenKayitSayisi;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
