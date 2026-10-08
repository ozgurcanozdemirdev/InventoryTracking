# InventoryTracking V1 müşteri kurulumu

1. Bilgisayarda .NET Framework 4.8.1 ve erişilebilir bir SQL Server bulunduğunu doğrulayın. Veritabanı kurulumunu SQL Server yöneticisi yapmalıdır.
2. **Yeni ve boş kurulumda** `SQL/SqlDataBaseScriptV1_Customer.sql` betiğini yalnızca yeni veritabanı için çalıştırın. Temel birimler, `Genel` kategorisi ve bir ana depo oluşturulur; örnek ürün veya stok eklenmez. Betik mevcut tabloları siler; veri bulunan veritabanında çalıştırmayın.
3. **Var olan kurulumda** önce doğrulanmış bir yedek alın. Daha önce uygulanmamış geçişleri şu sırayla çalıştırın: `2026-09-26_UrunAdiBenzersiz.sql`, `2026-09-26_MalzemeKatalogVeHareketYonu.sql`, `2026-09-26_V1_MalzemeBirimGuncelleme.sql`, `2026-09-26_V1_StokTransferi.sql`. Mevcut veritabanınızın durumunu kontrol etmeden eski geçişleri yeniden çalıştırmayın.
4. `SQL/2026-09-26_V1_UygulamaYetkileri.sql` ile sınırlı uygulama rolünü oluşturun. SQL yöneticisi uygulamayı çalıştıracak Windows hesabını bu role eklemelidir. `sa` hesabını uygulamada kullanmayın.
5. `InventoryTracking.exe` dosyasını açın. İlk açılışta SQL Server, veritabanı ve kimlik bilgilerini girip **Bağlantıyı Sına** ve **Kaydet ve Aç** düğmelerini kullanın. Sunucu daha sonra değişirse ana ekrandaki **Bağlantı Ayarları** düğmesini kullanın. Bağlantı bilgisi mevcut Windows kullanıcısına bağlı olarak şifrelenir; başka kullanıcı kendi ayarını yapmalıdır.
6. **Transfer** ekranında kaynak ve hedef depoyu seçin. Transfer açıklaması zorunludur. Kaynak stok yetersizse işlem kaydedilmez. Ürün düzenlemesinde stok miktarı değişirse düzeltme nedeni istenir. Barkod kayıtlıysa güncelleme ekranı teklif edilir; kayıtlı değilse ürün ekleme akışı açılır. Stok giriş/çıkış ekranı için ana listede ürüne çift tıklayın.
7. **Hareketler** ekranında tarih, depo, ürün/barkod, hareket türü ve yön filtresini seçip **Raporu Getir** düğmesine basın. **Özet** sekmesi normal giriş/çıkış, sayım düzeltmesi ve iç transferi ayrı gösterir; **Hareket Detayı** sekmesinde işlem, kullanıcı, açıklama ve transfer numarası görünür. CSV/PDF düğmeleri seçili sekmenin filtrelenmiş sonucunu aktarır.

## Yedekleme ve geri yükleme kontrolü

`BackupAndVerify.ps1` SQL Server bakım hesabıyla çalıştırılır. Windows kimliğiyle örnek:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\BackupAndVerify.ps1 -Server . -Database InventoryTracking
```

Farklı sunucu/klasör için `-Server`, `-Database`, `-BackupDirectory` ve `-RestoreDirectory` parametrelerini kullanın. SQL kimliği gerekiyorsa bağlantıyı yalnızca bakım oturumunda `INVENTORYTRACKING_MAINTENANCE_CONNECTION_STRING` ortam değişkenine verin. Klasör yolları SQL Server hizmetinin erişebildiği yollar olmalıdır. Betik yedek oluşturur, yedeği doğrular, benzersiz adlı geçici veritabanına geri yükler, bütünlük denetimi yapar ve geçici veritabanını kaldırır. Geri yükleme testi için bakım hesabında veritabanı oluşturma/silme yetkisi gerekir. `.bak` dosyası saklanır.

Yedek sıklığını kabul edilebilir veri kaybına göre belirleyin; başlangıç için günlük yedek ve güvenli ikinci kopya planlayın. Gerçek veri kaybında canlı veritabanının üzerine geri yüklemeden önce uygulamayı durdurun ve yedeği ayrı ortamda sınayın.
