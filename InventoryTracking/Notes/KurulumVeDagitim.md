# Kurulum ve dağıtım

- `SqlDataBaseScriptV1.txt` yalnızca boş bir veritabanını kurmak içindir. Var olan tablolarda `DROP TABLE` çalıştırır; müşteri veritabanında kullanmayın. Önce yedek alın, mevcut veritabanını yalnızca tarihli, veri koruyan migrasyonlarla yükseltin.
- Mevcut kurulumda `2026-09-26_MalzemeKatalogVeHareketYonu.sql` geçişini uygulayın. Eski ürünler `LEGACY-<UrunId>` malzeme numarasıyla kataloğa alınır; gerçek kurumsal numaralar bilindiğinde bu değerleri inceleyip güncelleyin. Yeni ürün açmadan önce malzemeyi numara, benzersiz barkod, açıklama ve birimle kataloğa kaydedin.
- `StokHareketi.CikisMi` SQL `BIT` alanıdır: `0` giriş, `1` çıkış. `HareketTipi` alanı giriş/çıkış veya sayım düzeltmesi gibi açıklayıcı işlem adını korur.
- Yerel geliştirmede `App.config` makinedeki bağlantıyı taşır ve Git'e eklenmez. Yeni kurulumda `App.config.example` dosyasını `App.config` olarak kopyalayıp kendi SQL Server ayarlarını girin. Bu dosyayı veya `bin` çıktısını kaynak kod deposuna yüklemeyin.
- Release derlemesi örnek yapılandırmayı çıktıdaki `.exe.config` dosyasına yazar; yerel parolayı dağıtmaz. Müşteri ortamında `INVENTORYTRACKING_CONNECTION_STRING` ortam değişkenini ayarlayın veya müşterinin kendi Windows kimliğiyle erişebileceği bir SQL Server yapılandırın.
- Daha önce paylaşılmış SQL parolasını değiştirin. Uygulamaya veritabanı için yalnızca gerekli izinleri verin; `sa` hesabını dağıtımda kullanmayın.
- Hareket raporu yalnızca stok giriş/çıkış adetlerini gösterir. Satış ve finansal Z raporu için ayrıca satış belgesi, fiyat anlık görüntüsü ve iade modeli gerekir.
