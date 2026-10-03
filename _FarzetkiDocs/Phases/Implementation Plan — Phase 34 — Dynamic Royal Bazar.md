# Phase 34 — Royal Bazar: düzenlenebilir ve dinamik mağaza

**Başlangıç:** 3 Ekim 2026
**Durum:** Devam ediyor — **34.1–34.2 tamamlandı (2/10)**.
**Son tamamlanan aşama:** **34.2 canlı katalog bağlantısı ve presentation veri modeli**, 3 Ekim 2026.
**Devam noktası:** **34.3 — yazısız modüler görseller ve ayrı artwork**. Sekiz mevcut buton canlı kataloğa bağlandı; presentation asset oluşturuldu. Skin/artwork alanları bu sonraki aşamada doldurulacak. Mevcut mağazanın baked görünümü henüz değiştirilmedi; aşama34.4 polished prefab preview sonrasında kontrollü görsel migration yapılacak. Aynı işi yeniden üretme; aşağıdaki son checkpoint'i oku.
**Kullanıcı isteği:** Mevcut mağazanın tarzını koruyarak tek parçalı görsellerdeki yazı/fiyat/ödül/indirim/sayaçları gerçek Unity UI bileşenlerine dönüştürmek; bütün alanları düzenlenebilir yapmak; aşamalı ve kalıcı bir uygulama planıyla devam etmek.

## Referans ve kapsam

- Görsel hedef: `Assets/_Project/Art/_FarzetkiArts/_PanelsFullVersion/ChatGPT Image 24 Eyl 2026 23_51_57 (2).png`.
- `master_build_prompt.md`: §14 mağaza/katalog, §15 No Ads/ödül güvenliği, §17 MainMenu/bottom navigation; `RoyalSmashInformationFullVersion.md`: §74–79 mağaza/paket/sayaç, §82 satın alma, §112 UI.
- Workflow: `.agents/skills/phase_implementation_workflow/SKILL.md`, `mobile_ui_ux`, `unity_mcp_automation`. Bitmap değişikliklerinde imagegen skill ve native image_gen kullanılır.
- Phase32 12/12 kapandı; kullanıcı yedek commit'i `5314740`, kapanış doküman commit'i `75629c7`. Phase33 level designer ve mevcut gameplay korunur. Phase32 otomasyonu kapanışta zaten mevcut değildi; bu faz için kullanıcı otomasyon istemedi.

Referans sanat ve düzen hedefidir; görseldeki fiyat/ödüller canlı ürün kataloğunun yerine geçmez. Görseldeki **Rift Kristali** mevcut ekonomi/save şemasında yoktur. Dinamik mağaza gerçek **Altın / Mega / Sonsuz Top / No Ads** ödüllerini gösterecek; bu faz yeni para birimi oluşturmaz. Store ProductId `gold_douch_800` mevcut yayınlanan anahtardır ve aynen korunur.

## 1. Mevcut durum ve mimari

- Unity 6000.4.0f1; MainMenu sahnesi `Assets/_Project/Scenes/MainMenu.unity`.
- UI kökü `MainMenuCanvas/PagesRoot/ShopPage`; içerik `ShopFrameContainer/ContentScrollView/Viewport/Content`.
- ScrollRect / RectMask2D / VerticalLayoutGroup / ContentSizeFitter mevcut. Sekiz Button + ShopItemCardUI, sekiz PriceText TMP ve ayrı deal TimerText çalışır durumda.
- Card_KingsDeal, Card_NoAds, Section_GoldPacks, Section_BallPacks, Section_ValueBundles görselleri başlık/ödül/rozet/fiyat içeriyor. Sekiz card'ın title/description/icon/badge referansları boş; `34-01-shop-inventory.json` gerçek Unity preview-scene envanteridir.
- `RoyalSmash.Economy.ShopConfig` / ShopConfig.asset sekiz ürünün ekonomik kaynağıdır. `IAPManager` store fiyatı, satın alma, global işlem kilidi, fulfillment, restore ve No Ads kaydını yönetir.
- `ShopItemCardUI` şu anda ürünü inline kopya olarak saklıyor; ShopConfig'teki değişiklikler eski kopyalara yansımayabilir. `ShopUI.RefreshAllCards` yeni katalog çözümüyle aynı canlı ürünü göstermeli/satın almalıdır.
- `ShopUI` timer coroutine'i yalnız Start'ta açıyor; ShopPage kapatılınca durur, yeniden açılınca başlamaz. Görsel geçişin lifecycle aşamasında düzeltilecek.

### Veri ve görünüm ayrımı

1. **ShopConfig:** mevcut ürün Id/ProductId, fiyat fallback'i, ekonomik ödüller, No Ads hakkı. Yeni UI fiyatı store hazırsa gerçek localized store metadata'dan alır. Inspector'daki fallback store fiyatını değiştirmez.
2. **ShopPresentationConfig (34.2/34.3):** mağaza/section başlıkları, görünür bölüm sırası, yazısız skin sprite'ları, ayrı ürün artwork'leri, ölçü/renk ve owned/loading/error etiketleri. Ekonomik ödül veya ikinci fiyat kaynağı tutmaz.
3. **ShopItemCardUI:** mevcut purchase davranışı korunur. Catalog + stable item Id üzerinden her display/purchase öncesi aynı ShopItemData çözülür; eski inline alan migration fallback'i olarak korunur. Katalog bağlıyken kayıp Id eski ürünü satın alamaz.
4. **Ayrı UI parçaları:** Image frame/background/artwork/icon, TextMeshProUGUI title/description/reward/price/badge, Button buy/close, gerçek sayaç. Reward sayıları doğrudan katalogdan oluşturulur; metin içine sabit ekonomik sayılar gömülmez.
5. **Prefablar:** featured, No Ads, compact product, bundle ve section header; bağımsız `PF_DynamicShop_Preview` içinde görünür örnekler. Ana mağaza ancak bu parçalar görsel ve binding denetimini geçince dönüştürülür.

Ürün ekleme/çıkarma/sıra değişikliği 34.7'de katalog listelerini yeniden bağlayan presenter ile yapılır. Katalogdaki ödül sayısı için ikinci bir manuel UI miktarı yoktur. Font, artwork, ölçü ve renk Inspector'dan düzenlenir. Sadece sanatsal logo dışında metin içeren eski bitmap hiçbir ürünün bilgi kaynağı olarak kalmaz; mağaza adı da TMP ile düzenlenebilir olur.

## 2. Aşamalar ve teslimatlar

| Aşama | İş | Tamamlanma şartı | Durum |
| --- | --- | --- | --- |
| **34.1** | Güncel mağaza/ürün/binding envanteri, scene/catalog/player backup, gerçek başlangıç görüntüsü | 8 ürün, mevcut bağlantılar ve baked alanlar belgeli; hash ve rollback yolu kayıtlı | **Tamam**; gerçek Play baseline,10 project byte backup,5 JSON hash restore |
| **34.2** | Catalog Id çözümü ve presentation veri modeli | Mevcut SKU/ödül/fiyat/save aynı; canlı katalog değişikliği eski kopyayı kullanmaz; kayıp Id kilitli | **Tamam**; 8/8 live binding, idempotent migration,11 gerçek Play denetimi PASS |
| **34.3** | Yazısız modüler sanat: boş çerçeve, kart, section/header, rozet ve buy yüzeyi; ayrı artwork | Metin/fiyat/sayı baked değil; transparan alpha, uygun import ve 9-slice; referans stili korunur | Bekliyor |
| **34.4** | Düzenlenebilir kart/header prefabları ve bağımsız preview | Başlık/ödül/price/badge/artwork ayrı Inspector alanları; en az bir polished card gerçek katalogla görünür | Bekliyor |
| **34.5** | Limited deal ve No Ads kartlarının dönüşümü | Gerçek ödüller/TMP/sayaç; bought/pending/error durumları okunur; eski buy hook korunur | Bekliyor |
| **34.6** | Altın ve skill kartları; dinamik section başlıkları ve iki sütun | 4 kartta doğru canlı ürün/ödül/fiyat, ayrı artwork; shared prefab ölçüleri tutarlı | Bekliyor |
| **34.7** | Bundle kartları ve liste presenter'ı | 2 bundle + listelerin ekle/sil/sırala değişimi; ikinci ekonomik kaynak yok; düzgün layout | Bekliyor |
| **34.8** | MainMenu'ye kontrollü migration; timer/open-close/navigation/feedback lifecycle | Shop aç/kapat tekrarında sayaç/event bağlantıları doğru; başka sayfalar ve top/bottom bar korunur | Bekliyor |
| **34.9** | Responsive/safe area/scroll/uzun metin ve edit testi | 1080×1920, 1080×2400 ve küçük ekran; scroll tüm ürünlere ulaşır; düzenlenen alan hemen görülebilir | Bekliyor |
| **34.10** | Kontrollü purchase regression, save restore, final görüntüler ve devir | 8 doğru ürün, tek fulfillment, owned/fail/pending; konsol temiz, kayıtlar hash restore; 10/10 kapanış | Bekliyor |

## 3. Kabul kriterleri

1. [ ] Ürün title/reward/price/badge/section/sayaç metinleri TMP; düzenlemek için raster yeniden üretmek gerekmez.
2. [x] Sekiz ürün gerçek ShopConfig'ten çözülür; Google Play SKU'ları ve mevcut ödüller istemsiz değişmez. 34.2 actual reference test + catalog byte hash PASS; diğer görsel kriterler açık.
3. [ ] Canlı store fiyatı geldiğinde UI onu gösterir; offline fallback doğru etiketlenir. Görsele gömülü ikinci fiyat yoktur.
4. [ ] Tüm artwork/background/frame/icon/rozetler ayrı Image alanlarıdır; shared prefab/skin düzenlemesi bütün ilgili kartlara yansır.
5. [ ] Gerçek reward satırları katalogdan oluşur; farklı ürün/ödül miktarıyla doğru değişir. Referansta olup oyunda olmayan kristal ödülü gösterilmez.
6. [ ] Yeni ürün, kaldırılan ürün ve liste sırası scene elle kopyalama olmadan değişir; kayıp Id satın almaya izin vermez.
7. [ ] Satın alma lock/pending/success/failure, No Ads owned ve feedback mevcut IAP akışına bağlıdır; çift fulfillment oluşmaz.
8. [ ] Shop/battle/leaderboard ve close, tekrar açılma, timer lifecycle ve scroll davranışı doğru.
9. [ ] İki uzun portrait oranı ve küçük ekran, uzun Türkçe başlık, farklı currency fiyatı, safe-area denetimi geçti; görsel referansa yaklaştı ve hiçbir info baked kalmadı.
10. [ ] Gerçek test çıktıları/prefab preview/ekran görüntüleri kayıtlı; Console temiz; aynı Play oturumunun tüm JSON kayıtları geri yüklenip hash doğrulandı.

## 4. Güvenli uygulama ve devam protokolü

- Her tur önce bu dosyanın başlık/son checkpoint'ini ve Unity'nin Edit/Play/dirty durumunu oku. İlk eksik aşamadan devam et; tamamlanan bitmap/prefabları tekrar üretme.
- Assets değişmeden güncel MainMenu, ShopConfig, ilgili script ve meta'ların hash/byte yedeği alınır. Dirty kullanıcı sahnesi önce bağımsız kopyayla korunur; kullanıcı değişiklikleri geri alınmaz.
- Play öncesi **o anki tüm player JSON** dosyaları (live_cloud_device_saves dahil) alınır. Stop sonrası aynı oturum bytes/hash restore; eski gün yedeği yeni ilerlemeye uygulanmaz.
- Aynı Unity'ye tek mutation işi. Agents draft/read-only çalışabilir; script importları Play ortasında domain reload başlatmaz.
- `Phase13Setup.RunFullSetup`, `ShopSystemSetup.SetupAll/CreateOrUpdateShopConfig` çağrılmaz: mevcut SKU/ödülleri eski değerlerle yazar. `ShopProductionTests.RunTests` kontrolsüz çalıştırılmaz: ödül/No Ads/save değiştirir ve restore yapmaz.
- UI'nın otomatik offline purchase fallback'i mevcut IAP davranışıdır; bu faz store güvenlik sistemini yeniden tasarlamaz. Gerçek ödeme alınmaz. Testler mevcut Editor simülasyonunda, güncel yedekle yapılır.
- Her aşama sonrası gerçek PASS/FAIL, değişen dosya, görüntü ve açık iş aşağıya yazılır. Test edilmeyen aşama tamam sayılmaz. Token/bağlantı durursa kısa checkpoint bırakılır.
- Kullanıcı "devam et" dediğinde son kayıt esas alınır. Kullanıcı ayrıca istemedikçe Phase34 otomasyonu oluşturulmaz. Faz sonunda scoped conventional Git commit; eşzamanlı kullanıcı değişiklikleri hariç tutulur.

## 5. Doğrulama kayıtları ve checkpoint

### 34.1 — Envanter, 3 Ekim 2026

- `34-01-shop-inventory.json`: MainMenu **preview scene** read-only incelendi; 26 graphic/text node,8 wired cards. Geçici scene finally içinde kapandı; aktif MainGame değişmedi, Play/purchase/save işlemi yok.
- Agent bağımsız kod/asset incelemesi aynı baked-field ve inline-copy problemlerini doğruladı. Section bitmaps iki kartı tek Image içinde taşıyor; sadece price TMP overlay kullanmak yeterli değildir.
- SKU/ödül tabanı ShopConfig.asset; gold_douch_800 ve noadspass aynen kalır. Crystal para birimi yok. Başlangıç görüntüsü/byte backup henüz bu kayıtta tamamlanmadı; aşama henüz PASS değil.

### 34.1 — Tamamlandı, 3 Ekim 2026

- `34-01-backup.json`: güncel10 scene/catalog/script/meta dosyası ve5player JSON byte yedeği `C:/Users/Farzetki/.codex/local-backups/Phase34-20261003-092320/`. PlayerSave-Baseline yalnız bu oturumun testinde kullanıldı.
- Mevcut MainMenu gerçek Play'de açıldı; bottom navigation Shop'a geçti, hiçbir buy button çağrılmadı. `34-01-shop-play-baseline.json`:8ürün/SKU/price overlay, shop active. `34-01-shop-before-portrait.png` gerçek **1080×1920** görüntüsüdür ve incelendi. İlk captura ayrı `34-01-shop-before-initial.png` adıyla tutuldu; API o sırada geçici Screen437×539 bildiriyordu fakat gerçek PNG boyutu1080×1920 olarak dosyadan doğrulandı. İkinci portrait capture ayrıca Screen1080×1920 halinde alındı. GameView önceki seçimine geri döndü.
- Baseline görsel sorunu doğruladı: Editor fake store PriceText'leri$0.01/NoAds OWNED olsa bile resim$4.99/$2.99 gibi baked fiyatları gösteriyor. Bu Editor değerleri gerçek mağaza fiyatı kabul edilmez. Bitmap fiyatları kaldırılmadan dinamik alan görünür/doğru olmaz;34.3–34.8 kapsamında.
- Konsol0 hata/uyarı. Play kapandı; aynı oturumun5JSON geri yüklendi, SHA256PASS;10project dosyası aynı: `34-01-save-restoration.json`.

### 34.2 — Tamamlandı, 3 Ekim 2026

- `Assets/_Project/Scripts/UI/ShopItemCardUI.cs`: `_catalog` + `_catalogItemId` stable bağlantısı. Display, store/state event ve purchase başlangıcında aynı canlı ürün çözülür. Inline `_itemData` legacy/rollback için korundu. Katalog kaybolur/ürün kaldırılırsa eski kopyaya düşmez, dinamik alanlar temizlenir ve buy kilitlenir. `Setup(data)` bilinçli legacy moda geri döner; invalid BindProduct girdisi mevcut geçerli bağlantıyı bozmaz.
- InGameGold fiyatı gerçek CostInGold'dan oluşur; sadece RealMoneyIAP store metadata kullanır. Owned tint kendi önceki rengini saklar/geri alır. Mevcut purchase/IAP/NoAds/save işleyişi yeniden yazılmadı.
- `Assets/_Project/Scripts/Economy/ShopConfig.cs`: üç FindItemById liste döngüsü null elemanı atlıyor; mevcut ürün/veri veya fulfillment değişmedi. Inspector'da boş liste girdisi kayıp Id çözümünü exception'a çevirmesin diye küçük guard eklendi.
- `Assets/_Project/Scripts/Editor/Phase34ShopCatalogMigration.cs` (`RoyalSmash.EditorTools`): açık MainMenu ve8current ürün doğrulaması; yalnız her ShopItemCardUI'nin `_catalog` ve `_catalogItemId` alanları bağlanır. Auto open/save, eski builder veya RefreshDisplay çağrısı yapmaz; Edit Mode SaveManager oluşturmaz. Root reviewed save bir kez yapıldı. `34-02-catalog-migration.json`:8changed/catalog unchanged; ikinciApply0change: `34-02-catalog-idempotent.json`. MainMenu serialized farkı sadece16yeni binding satırıdır; eski layout/buy/ref/inline fields korundu.
- `Assets/_Project/Scripts/UI/ShopPresentationConfig.cs` ve `Assets/_Project/ScriptableObjects/Config/ShopPresentationConfig.asset`: düzenlenebilir başlık/section/owned/pending/reward-label/font/renk alanları;8stableId artwork slotu, text-free skin alanları. **Skin/artwork henüz boş ve mevcut baked UI henüz bunu kullanmıyor.** Bunlar34.3–34.4 teslimatıdır; çalışan yeni görsel mağaza diye sunulmaz. Asset Inspector'da seçilip ping edildi.
- `34-02-catalog-binding-smoke.json`: gerçek Play'de **11/11 PASS**. Sekiz mevcut card authoritative catalog instance'ını kullanıyor. Geçici ayrı UI + deep-cloned katalogda canlı title/description/badge/reward value, current price fallback, InGameGold cost, replaced product definition, ProductId alias rejection, null list entry, removed item/missing catalog fail-closed ve legacy Setup geri dönüşü doğrulandı. Buy/purchase/fulfillment çağrılmadı. Reward **veri erişimi** test edildi; görünür reward-row widget'ları34.4'te geliyor.
- Ayrı taze PlayerSave-CatalogTest yedeği5JSON için alındı. Test sonrası Play kapatıldı, aynı oturum5JSON hashPASS: `34-02-save-restoration.json`. MainMenu Edit Mode temiz; Konsol0 hata/uyarı. `34-02-preservation.json`: yalnız beklenenMainMenu/ShopItemCardUI/ShopConfig.cs değişti; catalog.asset/GUID,MainGame,IAPManager,ShopUI,BottomNavigation aynı.
- Bağımsız kod denetimi PASS; null lookup edge'i root tarafından giderilip test edildi. Sonraki aşama34.3: reference raster'ı font/fiyat/sayı üretmeden modüler boş skinlere ve ayrı ürün artwork'lerine revize et, alpha/9slice/import kaydını doğrula. Sonra34.4'te polished editable prefab preview oluştur; mevcut mağazanın tamamı henüz dinamik görünüme geçirilmedi.
- Son Git durumunda TMP/Fonts cache varlıkları ve Plastic metadata dirty görünüyor; bunlar bu scoped checkpoint commit'ine dahil edilmedi ve geri alınmadı. Sonraki görsel oturumda güncel font dosyaları da yeni başlangıç yedeğine dahil edilecek; kullanıcıya ait font/atlas değişiklikleri eski HEAD ile ezilmez.
