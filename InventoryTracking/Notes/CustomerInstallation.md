# InventoryTracking V1 müşteri kurulumu

1. Bilgisayarda .NET Framework 4.8.1 ve erişilebilir bir SQL Server bulunduğunu doğrulayın. Veritabanı kurulumunu SQL Server yöneticisi yapmalıdır.
2. **Yeni ve boş kurulumda** `SQL/DatabaseSetup.Customer.sql` betiğini yalnızca yeni veritabanı için çalıştırın. Temel birimler, `Genel` kategorisi ve bir ana depo oluşturulur; örnek ürün veya stok eklenmez. Betik mevcut tabloları siler; veri bulunan veritabanında çalıştırmayın.
3. **Var olan kurulumda** önce doğrulanmış bir yedek alın. Daha önce uygulanmamış geçişleri şu sırayla çalıştırın: `2026-09-26_UniqueProductName.sql`, `2026-09-26_MaterialCatalogAndMovementDirection.sql`, `2026-09-26_V1_MaterialUnitUpdate.sql`, `2026-09-26_V1_StockTransfer.sql`, `2026-10-06_MaterialNumberSimplification.sql`. Son geçiş yalnızca otomatik oluşturulmuş `LEGACY-<UrunId>` numaralarını sadeleştirir; hedef numara kullanımda ise hiçbir değişiklik yapmaz. Mevcut veritabanınızın durumunu kontrol etmeden eski geçişleri yeniden çalıştırmayın.
4. `SQL/2026-09-26_V1_ApplicationPermissions.sql` ile sınırlı uygulama rolünü oluşturun. SQL yöneticisi uygulamayı çalıştıracak Windows hesabını bu role eklemelidir. `sa` hesabını uygulamada kullanmayın.
5. `InventoryTracking.exe` dosyasını açın. İlk açılışta SQL Server, veritabanı ve kimlik bilgilerini girip **Bağlantıyı Sına** ve **Kaydet ve Aç** düğmelerini kullanın. Sunucu daha sonra değişirse ana ekrandaki **Bağlantı Ayarları** düğmesini kullanın. Bağlantı bilgisi mevcut Windows kullanıcısına bağlı olarak şifrelenir; başka kullanıcı kendi ayarını yapmalıdır.
6. **Transfer** ekranında kaynak ve hedef depoyu seçin. Transfer açıklaması zorunludur. Kaynak stok yetersizse işlem kaydedilmez. Ürün düzenlemesinde stok miktarı değişirse düzeltme nedeni istenir. Barkod kayıtlı ve aktifse Satış / İade ekranı açılır; kayıtlı değilse barkodu doldurulmuş ürün ekleme ekranı açılır. Stok giriş/çıkış ekranı için ana listede ürüne çift tıklayın.
7. **Hareketler** ekranında tarih, depo ve ürün/barkod filtresini seçip **Raporu Getir** düğmesine basın. **Stok Özeti** sekmesi normal giriş/çıkış, sayım düzeltmesi ve iç transferi ayrı gösterir; **Hareket Detayı** sekmesinde işlem, kullanıcı, açıklama ve transfer numarası görünür. **Satış / İade Özeti** sekmesi satış ve iade adetlerini, tutarlarını ve net satış tutarını gösterir. Günlük, aylık veya yıllık gruplama seçilebilir. **Son Bir Yıl** düğmesi bugünün tamamını içeren son bir takvim yılını seçip raporu getirir. Excel, CSV ve PDF düğmeleri seçili sekmenin sonucunu aktarır; Excel aktarımı gerçek `.xlsx` dosyası üretir.

## Satış / iade ve işlem günlüğü

Mevcut veritabanını yedekledikten sonra `SQL/2026-10-10_SalesReturnsAndAuditLog.sql` geçişini uygulayın. Yeni müşteri kurulumunda bu şema zaten bulunur. Bu geçiş mevcut stok ve hareket kayıtlarını silmez; `SatisIade` ve `IslemGunlugu` tablolarını, işlem yordamını ve değişiklik tetikleyicilerini ekler. SQL uygulama rolüne yalnızca geçmişi okuma ve işlem yordamını çalıştırma izinleri verilir.

Barkod okuyucunun Enter gönderdiğini doğrulayın. Kayıtlı ürün için açılan pencerede adet ilk okutmayla 1 olur; aynı barkod tekrar okutuldukça artar. Satış stoktan düşer, iade stoğa eklenir. Seçili depodaki satış/iade geçmişi her tamamlanan işlemden sonra güncellenir. İşlem yapan kullanıcı mevcut Windows kullanıcı adıdır. İade tutarı, iade anındaki ürün fiyatından kaydedilir; önceki satış belgesine bağlanmaz.

`IslemGunlugu` veri değişikliklerini önceki/yeni değerler, tarih, Windows kullanıcısı ve SQL hesabıyla saklar. Veritabanına bağlantı kurulamadığı sırada düğme olayları SQL'e yazılamaz.

Satış/iade raporu işlem kaydındaki fiyatı kullanır; ürün fiyatının sonradan değişmesi eski tutarları değiştirmez. Transferler ve sayım düzeltmeleri finansal toplamlara girmez. Net satış tutarı satış tutarından iade tutarının çıkarılmasıdır; alış maliyeti ve diğer işletme giderleri dahil değildir. Bu rapor mevcut satış/iade şemasını kullanır; ayrıca veritabanı geçişi gerektirmez. PDF aktarımı için Windows **Microsoft Print to PDF** etkin olmalıdır. Excel aktarımı Excel kurulumu veya NuGet paketi gerektirmez.

## Yedekleme ve geri yükleme kontrolü

`BackupAndVerify.ps1` SQL Server bakım hesabıyla çalıştırılır. Windows kimliğiyle örnek:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\BackupAndVerify.ps1 -Server . -Database InventoryTracking
```

Farklı sunucu/klasör için `-Server`, `-Database`, `-BackupDirectory` ve `-RestoreDirectory` parametrelerini kullanın. SQL kimliği gerekiyorsa bağlantıyı yalnızca bakım oturumunda `INVENTORYTRACKING_MAINTENANCE_CONNECTION_STRING` ortam değişkenine verin. Klasör yolları SQL Server hizmetinin erişebildiği yollar olmalıdır. Betik yedek oluşturur, yedeği doğrular, benzersiz adlı geçici veritabanına geri yükler, bütünlük denetimi yapar ve geçici veritabanını kaldırır. Geri yükleme testi için bakım hesabında veritabanı oluşturma/silme yetkisi gerekir. `.bak` dosyası saklanır.

Yedek sıklığını kabul edilebilir veri kaybına göre belirleyin; başlangıç için günlük yedek ve güvenli ikinci kopya planlayın. Gerçek veri kaybında canlı veritabanının üzerine geri yüklemeden önce uygulamayı durdurun ve yedeği ayrı ortamda sınayın.
