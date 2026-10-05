# Phase 34 — Royal Bazar: düzenlenebilir ve dinamik mağaza

**Başlangıç:** 3 Ekim 2026
**Durum:** **34.1–34.11 tamamlandı (11/11)**, 4 Ekim 2026. Referansa uygun görsel revizyon son kodla59/59 PASS ve5/5 kayıt restore ile kapandı.
**Son tamamlanan aşama:** **34.11 düz buy, ince section başlıkları, yatay kartlar ve yeni gold artwork**, 4 Ekim 2026.
**Devam noktası:** Bu kapsamda açık iş yok. Kullanıcının mevcut Royal Bazar logo/header ve MainMenu değişiklikleri korundu. Yeni kapsam için kullanıcı isteği beklenir; Phase35 başlatılmaz.
**Kullanıcı isteği:** Mevcut mağazanın tarzını koruyarak tek parçalı görsellerdeki yazı/fiyat/ödül/indirim/sayaçları gerçek Unity UI bileşenlerine dönüştürmek; bütün alanları düzenlenebilir yapmak; aşamalı ve kalıcı bir uygulama planıyla devam etmek.

## Referans ve kapsam

- Görsel hedef: `Assets/_Project/Art/_FarzetkiArts/_PanelsFullVersion/ChatGPT Image 24 Eyl 2026 23_51_57 (2).png`.
- `master_build_prompt.md`: §14 mağaza/katalog, §15 No Ads/ödül güvenliği, §17 MainMenu/bottom navigation; `RoyalSmashInformationFullVersion.md`: §74–79 mağaza/paket/sayaç, §82 satın alma, §112 UI.
- Workflow: `.agents/skills/phase_implementation_workflow/SKILL.md`, `mobile_ui_ux`, `unity_mcp_automation`. Bitmap değişikliklerinde imagegen skill ve native image_gen kullanılır.
- Phase32 12/12 kapandı; kullanıcı yedek commit'i `5314740`, kapanış doküman commit'i `75629c7`. Phase33 level designer ve mevcut gameplay korunur. Phase32 otomasyonu kapanışta zaten mevcut değildi; bu faz için kullanıcı otomasyon istemedi.

Referans sanat ve düzen hedefidir; görseldeki fiyat/ödüller canlı ürün kataloğunun yerine geçmez. Görseldeki **Rift Kristali** mevcut ekonomi/save şemasında yoktur. Dinamik mağaza gerçek **Altın / Mega / Sonsuz Top / No Ads** ödüllerini gösterecek; bu faz yeni para birimi oluşturmaz. Store ProductId `gold_douch_800` mevcut yayınlanan anahtardır ve aynen korunur.

## 1. Başlangıç durumu ve mimari

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
| **34.3** | Yazısız modüler sanat: boş çerçeve, kart, section/header, rozet ve buy yüzeyi; ayrı artwork | Metin/fiyat/sayı baked değil; transparan alpha, uygun import ve 9-slice; referans stili korunur | **Tamam**;16 PNG,8 ayrı ürün artwork,alpha/9slice/importPASS |
| **34.4** | Düzenlenebilir kart/header prefabları ve bağımsız preview | Başlık/ödül/price/badge/artwork ayrı Inspector alanları; en az bir polished card gerçek katalogla görünür | **Tamam**;7 prefab, bağımsız polished Play preview |
| **34.5** | Limited deal ve No Ads kartlarının dönüşümü | Gerçek ödüller/TMP/sayaç; bought/pending/error durumları okunur; eski buy hook korunur | **Tamam**;live reward/store metadata/owned/pending/feedback PASS |
| **34.6** | Altın ve skill kartları; dinamik section başlıkları ve iki sütun | 4 kartta doğru canlı ürün/ödül/fiyat, ayrı artwork; shared prefab ölçüleri tutarlı | **Tamam**;4 kart, shared compact prefab ve 2 sütun |
| **34.7** | Bundle kartları ve liste presenter'ı | 2 bundle + listelerin ekle/sil/sırala değişimi; ikinci ekonomik kaynak yok; düzgün layout | **Tamam**;2 bundle; add/remove/reorder/null ve reward composition PASS |
| **34.8** | MainMenu'ye kontrollü migration; timer/open-close/navigation/feedback lifecycle | Shop aç/kapat tekrarında sayaç/event bağlantıları doğru; başka sayfalar ve top/bottom bar korunur | **Tamam**;migration/idempotence/outside-shop preservation ve5 lifecycle PASS |
| **34.9** | Responsive/safe area/scroll/uzun metin ve edit testi | 1080×1920, 1080×2400 ve küçük ekran; scroll tüm ürünlere ulaşır; düzenlenen alan hemen görülebilir | **Tamam**;21 layout+3 edited-layout PASS; Editor safe area ve synthetic insets |
| **34.10** | Kontrollü purchase regression, save restore, final görüntüler ve devir | 8 doğru ürün, tek fulfillment, owned/fail/pending; konsol temiz, kayıtlar hash restore; 10/10 kapanış | **Tamam**;12 purchase PASS;5/5 restore;final Console0/0 |
| **34.11** | Kullanıcı referansına görsel polish | Düz yeşil buy, ince section, yatay compact cards, büyük temiz bag/chest art; live binding/lifecycle korunur; responsive görsel ve işlev PASS | **Tamam**;6 yeni art,59 PASS;3 boyut görsel QA;5/5 restore |

## 3. Kabul kriterleri

1. [x] Ürün title/reward/price/badge/section/sayaç metinleri TMP; düzenlemek için raster yeniden üretmek gerekmez.
2. [x] Sekiz ürün gerçek ShopConfig'ten çözülür; Google Play SKU'ları ve mevcut ödüller değişmedi. Catalog asset/GUID byte hash PASS.
3. [x] Store metadata geldiğinde UI onu gösterir; offline fallback ayrı etiketlenir. InGameGold fiyatı CostInGold kullanır. Görsele gömülü ikinci fiyat yoktur.
4. [x] Artwork/background/frame/icon/rozetler ayrı Image alanlarıdır; shared prefab/skin değişimi ilgili kartlara yansır.
5. [x] Reward satırları katalogdan oluşur; sayı ve reward türü değişimi doğrulandı. Oyunda olmayan kristal ödülü yoktur.
6. [x] Ürün ekleme/kaldırma/liste sırası presenter ile değişir; kayıp Id satın almaya izin vermez.
7. [x] Lock/pending/success/failure, No Ads owned ve feedback mevcut IAP akışına bağlıdır. Editor'da sekiz işlemde birer doğru fulfillment ve eşzamanlı ikinci isteğin reddi PASS; duplicate store callback idempotency bu fazın doğrulaması değildir.
8. [x] Shop/battle/leaderboard, close/reopen, timer ve scroll denetimleri PASS.
9. [x] Üç portrait boyutu, uzun Türkçe başlık, euro fiyatı, büyük ödül miktarı ve viewport inset denetimi PASS. Fiziksel çentikli cihaz testi yapılmadı; Editor Screen.safeArea ve synthetic inset geometrisi kontrol edildi.
10. [x] Test JSON/prefab preview/görüntüler kayıtlı; final Console0/0; aynı Play oturumunun5JSON bytes ve SHA256 restore PASS.

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

### 34.3 — Tamamlandı, 4 Ekim 2026

- `34-03-current-backup.json`: 4 Ekim güncel 16 scene/catalog/script/font dosyası byte/hash yedeği. Önceki günün player yedeği kullanılmadı. Başlangıç Git clean; kullanıcının yeni gameplay/level/fracture commit'leri korunuyor.
- `Assets/_Project/Art/UI/ShopDynamic/`:8 bağımsız artwork (deal/noads/goldbag/goldchest/mega/infinity/warrior/champion),3 blank card skin (blue/gold/purple),5 mevcut text-free yüzeyin kaynak değişmeden kopyası (frame/header/section/buy/badge).11 üretilen PNG native built-in image_gen ile; prompt set `34-03-image-prompts.json`. Mevcut raster'lar yeniden çizilmedi; hiçbir görsel fiyat/ödül miktarı/kart başlığı taşımıyor. Infinity sembolü ve crown şekilleri sanatsal ikonlardır.
- `Phase34ShopArtImporter.cs`: explicit Edit Mode importer; source PNG bytes değişmez. Sprite rect alpha bounds ile transparan margin seçilir, skin9slice, FullRect, mipmap off,Clamp,Bilinear,uncompressed,max2048. Modern SpriteDataProvider API ile stable subasset GUID korunur. `34-03-art-import.json`:16/16 gerçek alpha PASS;8 art borders0,8 skin borders>0. ShopPresentationConfig8 stableId artwork +9 skin slot bağlı.
- `Phase34DynamicShopBuilder.BuildPrefabAssets`:7 reusable prefab yazdı; kayıt sonrası8 unique stableId doğrulandı; katalog aynı. `34-04-prefab-build.json`. MainMenu henüz değişmedi.
- İlk bağımsız gerçek Play preview `34-04-independent-preview.png` (1080×1920 GameView, tool preview675×1200): yaşayan8 card görünümü incelendi. İlk Instantiate dönüşünde16 component sayısı önceki subtree Destroy() frame sonunu beklediği için geçici; presenter.Cards8, eski subtree inactive ve sonraki frame kaldırılır. Fontun geniş outline'ı polish için Fazo_Font_Settings'e çevriliyor; shared reward icons ve reward-count row height üzerinde çalışma devam ediyor.
- Play öncesi güncel5 JSON `34-04-preview-save-backup.json`; Stop sonrası5/5SHA256 restorePASS `34-04-preview-save-restoration.json`. Konsol0hata/uyarı. Editor helper importunda iki API/namespace compile sorunu düzeltildi; sonraki compile temiz. MainMenu Edit Mode temiz.

### 34.4–34.7 — Tamamlandı, 4 Ekim 2026

- `RoyalSmash.UI.ShopCatalogPresenter`: tek catalog kaynağından section ve card üretir. Stable Id binding, eksik/null/tekrarlanan Id guard, canlı field refresh ve structural değişiklikte Rebuild. Miktar değişimi mevcut card'ı korur; ürün/liste/bölüm/ödül satırı değişimi layout'u yeniden kurar. Edit Mode preview persistent prefab asset'i kendiliğinden değiştirmez. Generated subtree presenter'a aittir; eski subtree runtime'da önce inactive olur, frame sonunda silinir.
- `RoyalSmash.UI.ShopItemCardUI`: ayrı title/description/price/badge/state/timer TMP; artwork/background/buy/badge ve dört reward icon Image. Gold/Mega/Infinity/NoAds satırları gerçek katalogdan gelir, zero satır gizlenir; skill title'daki eski terminal `(x5)` canlı miktarla çelişmez. RealMoneyIAP için store localized metadata, hazır değilse etiketli fallback; InGameGold için gerçek CostInGold. Satın alma aynı IAPManager hook'undan gider; fiyat/ödül ikinci kopyada tutulmaz.
- `ShopPresentationConfig` font, renkler, başlıklar, section sırası, spacing/card heights, shared skins, dört reward icon ve stableId artwork alanlarını içerir. `Fazo_Font_Settings` kullanıldı. Font atlas/cache değişiklikleri faz commit'ine alınmaz. Shared compact/bundle reward alanı bottom146/top112; 3 reward minimum368, 4 reward406 yüksekliği ile metin/buy çakışması giderildi.
- `RoyalSmash.EditorTools.Phase34DynamicShopBuilder`: explicit BuildPrefabAssets ve ApplyToOpenMainMenu. Preview scene finally kapatılır; prefab içinde nested bağlantı ve stableId override'ları kaydedilir. Yedi çıktı: `Assets/_Project/Prefabs/UI/ShopDynamic/PF_ShopFeatured.prefab`, `PF_ShopNoAds.prefab`, `PF_ShopCompact.prefab`, `PF_ShopBundle.prefab`, `PF_ShopSectionHeader.prefab`, `PF_DynamicShopPanel.prefab`, `PF_DynamicShop_Preview.prefab`.
- `34-04-prefab-build.json` PASS. `34-04-polished-preview.png` bağımsız gerçek1080×1920 Play görüntüsü; font, reward icon ve artwork ayrı alanlar görsel olarak incelendi. Geçerli katalogla featured/NoAds, dört compact ve iki bundle oluşuyor. `34-07-mainmenu-shop-bottom.png` skill/bundle görünümünü ve scroll sonunu gösterir.
- `34-test-presentation.json`: **18/18 PASS**. Sekiz gerçek instance, SKU, TMP/Image, prefab, baked panel kaldırılması; canlı title/badge/reward/zero/fallback/metadata/InGameGold; ekleme/silme/sıralama/null, presentation label/section/skin, owned ve pending doğrulandı. Clone catalog/presentation sonunda temizlendi; source asset değiştirilmedi.
- Polished preview oturumunun5JSON yedeği ve5/5restore: `34-04-polished-save-backup.json`, `34-04-polished-save-restoration.json`.

### 34.8 — Tamamlandı, 4 Ekim 2026

- MainMenu migration yalnız `MainMenuCanvas/PagesRoot/ShopPage/ShopFrameContainer` altındaki baked görünümü dinamik panel prefab instance'ıyla değiştirdi. ShopUI, PurchaseFeedbackDialog, top bar, bottom navigation ve diğer sayfalar korunur. Builder sahne açmaz/save etmez; doğrulamadan sonra root sahneyi kaydetti. Undo/rollback yolu vardır.
- `34-08-mainmenu-migration.json`:8stableId, catalog unchanged. `34-08-outside-shop-before.txt` başlangıç snapshot'ıdır; `34-10-final-state.json` aynı3538serialized satırın korunduğunu doğrular (instanceID normalize). `34-08-idempotent.json`: ikinci Apply değişiklik0, card8, scene dirtyfalse. `34-08-mainmenu-shop-top.png` gerçek MainMenu mağazasıdır.
- `RoyalSmash.UI.ShopUI`: sayaç realtime deadline kullanır; kapalı sayfada geçen süre reopen'da hesaba katılır. Event subscription tek kez bağlanır ve OnDestroy'da çıkar. Presenter card/timer reference'larını yeniler. Kapalı mağazada biten satın almanın feedback'i açılışta gösterilir; close mevcut Battle navigation'a gider.
- `34-test-lifecycle.json`: **5/5 PASS**; tekrar aç/kapat, timer ilerleyişi, card sayısı/tekrarlanmayan bağlantılar, Shop/Battle/Leaderboard ve close denetlendi. Mevcut feedback modal tasarımı değiştirilmedi.

### 34.9 — Tamamlandı, 4 Ekim 2026

- `RoyalSmash.EditorTools.Phase34GameView`: explicit temporary boyut seçimi ve original selection restore. MainMenu Shop frame anchors x58, y0.12–0.88; viewport ve içerik kendi layout'uyla uyarlanır. Her boyutta tüm ürünlere scroll ile ulaşıldı.
- `34-test-layout-1080x1920.json`, `34-test-layout-1080x2400.json`, `34-test-layout-750x1334.json`: **7+7+7 PASS**. Gerçek Screen boyutu/safeArea, yeni shop buy/close96unit minimum hit target, TMP overflow, reward containment/overlap, ilk/son ürün erişimi ve content width denetlendi. Görseller: `34-10-final-shop.png`, `34-09-shop-1080x2400.png`, `34-09-shop-750x1334.png`.
- `34-test-edited-layout-1080x1920.json`: **3/3 PASS**. Geçici clone içinde uzun Türkçe title, `1.234,56 €`, büyük reward sayıları ve3/4reward büyüyen card test edildi; miktar değişimi card instance'ını korur. `34-09-edited-long-text-1.png` gerçekten düzenlenmiş clone'u gösterir. Suffix'siz `34-09-edited-long-text.png` capture'dan önceki frame'dir ve edited-text kanıtı değildir.
- `34-09-safe-insets.json`:1080×1920 viewport,32side/68bottom/88top synthetic inset içinde PASS. Editor safeArea tam ekrandır; fiziksel notch cihazı veya gerçek store locale testi yapılmadı. Localized store metadata path mock metadata ile, Editor ürünleri ise fake-store fiyatıyla doğrulandı.

### 34.10 — Tamamlandı, 4 Ekim 2026

- `RoyalSmash.EditorTools.Phase34PlayModeTest` explicit Editor test driver'ıdır. Otomatik setup/import veya kontrolsüz ekonomi reseti yapmaz. Purchase suite fresh manifest ister; mevcut Editor simülasyonu ve sekiz gerçek buy button kullanılır; `UseFakeStoreUIInEditor` geçici false ve sonunda eski değerine döner. Gerçek ödeme alınmadı.
- `34-test-purchase.json`: **12/12 PASS**. Sekiz doğru product/reward delta ve birer success event; global lock ile ikinci eşzamanlı istek reddi; owned NoAds yeniden satın alamama; simulated failure ödül vermeden lock release ve görünür failure feedback. Bu denetim IAPManager'ın duplicate store callback/transaction-id güvenliğini yeniden tasarlamaz veya garanti etmez; mevcut IAPManager bytes aynı.
- Pending/owned, failure ve success görsel doğrulaması: `34-10-pending.png`, `34-10-failure.png`, `34-10-success.png`. Son iki görüntü mevcut feedback API'sini görsel kontrol için çağırır; ek fulfillment yapmaz. Son normal görünüm `34-10-final-shop.png`; Editor `$0.01` metadata gerçek satış fiyatı kabul edilmez.
- MCP screenshot aracı `ScreenshotUtility.cs:197` içindeki `EditorApplication.Step()` ile zaten çalışan PlayerLoop'u tekrar çağırdı. `34-10-capture-diagnostic.json` stack kaydı tool package'ını gösterir; Phase34 runtime stack'i değildir. Son görüntüler asenkron `UnityEngine.ScreenCapture.CaptureScreenshot` ile alındı. Son denetim `34-10-final-console.json`: **0 hata / 0 uyarı**. PackageCache değiştirilmedi.
- Play öncesi aynı oturumun5güncelJSON yedeği `34-10-mainmenu-save-backup.json`. Stop sonrası `SaveSession.ps1` aynı bytes'ı geri yükledi; `34-10-mainmenu-save-restoration.json`: **5/5 SHA256 PASS**, yeni JSON0. Önceki günün ilerleme yedeği kullanılmadı. GameView original selection restore; EditMode,MainMenu clean, temporary clone yok ve purchase lock false.
- `34-10-preservation.json`: **7/7 unchanged PASS**; ShopConfig.asset/GUID, MainMenu GUID, MainGame, IAPManager, ShopConfig.cs ve BottomNavigationUI korunur. Migration dışındaki scene serialization aynı. `.plastic` ve dynamic font atlas/cache değişimleri scoped commit dışında bırakıldı; kullanıcı ilerlemesi/gameplay commit'leri geri alınmadı.
- Bağımsız agent son kod incelemesi:6production/draft hash aynı,59PASS/0FAIL raporu doğrulandı, kapanış blocker yok. Unity YAML serializer boş `m_Name:`/`value:` alanlarının sonunda bir boşluk üretir; generated scene/prefab formatı elle değiştirilmedi. Whitespace kontrolü bu standart boş-değer formatını hariç tutan `git -c core.whitespace=-blank-at-eol diff --check` ile PASS; C# dosyaları normal `git diff --check` ile denetlenir. Faz10/10 kapandı; sonraki faz için yeni kapsam başlatılmaz.

## 6. Düzenleme ve devir

- Ürün title/description/badge, ödül miktarı/türü, fallback price, CostInGold ve liste sırası: `Assets/_Project/ScriptableObjects/Config/ShopConfig.asset`. Mevcut yayınlanmış ProductId'ları değiştirmek store ürününü değiştirir; UI fallback store metadata'yı override etmez. Yeni stableId farklı ve benzersiz olmalıdır.
- Mağaza/section başlığı ve sırası, owned/pending/fallback/reward etiketleri, font, renk, boyut ve spacing, text-free skin/artwork: `Assets/_Project/ScriptableObjects/Config/ShopPresentationConfig.asset`. Yeni artwork için stableId eşleşen ItemArt girişi eklenir; ayrı Image field'ları prefab Inspector'ında da düzenlenebilir.
- Shared card düzeni: `Assets/_Project/Prefabs/UI/ShopDynamic/` altındaki featured/noads/compact/bundle prefabları. Panel ve MainMenu prefab instance bağlantıları korunur. Runtime presenter düzenlenen katalog/presentation alanlarını0.5sn aralıkla yeniler; structural alanlarda Rebuild yapar. Edit Mode'da presenter context menu **Refresh Preview** ile görünüm güncellenir.
- Bağımsız görünüm: `PF_DynamicShop_Preview.prefab`; canlı sahne: `Assets/_Project/Scenes/MainMenu.unity`. Importer/builder yalnız explicit Editor çağrısı içindir; normal preview refresh için prefabları yeniden üretmek gerekmez.
- Tüm doğrulama/görüntü/prompt/import/backup manifestleri `_FarzetkiDocs/Art/Phase34/` içinde kalıcıdır.11native built-in imagegen üretimin prompt/tool/source/çıktı listesi `34-03-image-prompts.json`; nihai PNG yolları `Assets/_Project/Art/UI/ShopDynamic/` içindedir.

## 7. 34.11 — Kullanıcı referansına görsel revizyon, 4 Ekim 2026

**Tamamlandı.** Referans `Revision/reference.png`, başlangıç kullanıcı ekranı `Revision/before-user.png`. Buy yüzeyi düz yeşil/gold-edge, section başlıkları ince merkezi plaque ve yan accents oldu. Compact/bundle kartlarında artwork büyütülüp sola, title/reward/buy sağa yerleştirildi. Red ribbon yazıları TMP; NoAds hero bağımsız Image.

- Altı yeni sibling PNG native built-in imagegen ile üretildi: `Skin_BuyStraight_v2`, `Skin_SectionSlim_v2`, `Skin_RibbonRed_v2`, `Art_GoldBag_v2`, `Art_GoldChest_v2`, `Art_NoAdsHero_v2`. Nihai PNG/meta dosyaları `Assets/_Project/Art/UI/ShopDynamic/` içinde. `Revision/image-prompts.json` gerçek tool/reference/prompt/source/output kaydıdır. Fiyat, ödül ve ürün yazıları raster içine taşınmadı.
- Explicit `Phase34VisualArtImporter.ImportAndBind` yalnız bu altı asseti işler. `Revision/art-import.json`:6/6 alpha/import doğrulaması, kaynak PNG pixels/bytes değişmedi. Alpha sınırına göre sprite rect; buy9-slice, diğer border0; FullRect, Clamp/Bilinear, mipmap off, uncompressed, max4096. Reimport sprite kimliğini korur. Bag/chest mevcut stableId artwork alanlarına bağlandı.
- `Phase34ShopVisualRevision.ApplyPrefabVisuals` beş shared prefabı ve presentation ölçülerini düzenledi. `Revision/prefab-revision.json`: serialized card binding ve purchase Button aynı, NoAds hero bağlı, `sceneSaved=false`. Featured340, NoAds260, compact230, bundle304, section56, spacing14; buy minimum96unit. MainMenu, panel ve preview yeniden oluşturulmadı. Uzun başlıklar64unit alanda min18pt compact/bundle, min25pt NoAds ile taşmadan yerleşir.
- Presenter configured minimum ile gerçek reward alanını karşılaştırır: compact202/bundle210 +28N +3×max(0,N−1); featured210 +34N +3×max(0,N−1); NoAds204 +28N +3×max(0,N−1). Compact3/4 reward292/323; bundle4 reward331 olur. Miktar değişimi card instance'ını korur; reward composition değişimi Rebuild yapar. Eski34.4–34.7 vertical card ölçüleri bu revizyonla değiştirildi.
- Price/State mevcut BuyButton altında ayrı TMP alanlarıdır. `ShopItemCardUI.SetState` yalnız her iki metnin doğrudan parent'ı mevcut BuyButton olduğunda fiyatı hizalar: state boşsa simetrik6unit inset ile merkez, pending/fallback varsa State17–38unit +2unit boşluk sonrası Price40unit. Eski kartların düzeni ve satın alma kodu korunur. `Revision/buy-label-layout.json`:8/8 normal/owned centered; `buy-label-pending-layout.json`:8/8 separated, overflow0. `final-pending.png` son kodun bekleme/owned görünümüdür.
- Legacy `Phase34DynamicShopBuilder.BuildPrefabAssets`, revised StateText'in BuyButton altındaki serialized bağlantısını tespit edip bütün yazmalardan önce eski vertical üretimi reddeder. `Revision/legacy-builder-guard.json`: rejection PASS, yedi prefab/presentation SHA değişmedi. Görsel bakım için scoped `ApplyPrefabVisuals`, katalog/presentation değişikliklerinde presenter **Refresh Preview** kullanılır.
- `Revision/backup.json` güncel başlangıç bytes/SHA yedeğidir. Kullanıcının önceden dirty MainMenu ve3font değişikliği geri alınmadı. MainMenu SHA `DAC94012031226B34A60A0DFD0F8EE7747A391424ADD4CE71BFD642967AAB96E` aynı; yeniden save edilmedi. Header sprite presentation'da mevcut `Logo_RoyalBazar.png` ile eşleşir. `Revision/final-editor-state.json`: header Rect/title component/close Rect/title active/sprite5/5 unchanged, Edit Mode, MainMenu clean. `preservation.json`:7/7 aynı SHA; MainMenu, ShopConfig, ShopUI, IAPManager, MainGame, panel/preview. Ekonomi/SKU/reward/save şeması, top bar ve navigation değiştirilmedi.
- Son closure Play oturumunda **59/59 PASS**: presentation18, edited long-title/currency/reward layout3, üç boyut7+7+7, lifecycle5, purchase12. `Revision/34-test-*.json` son sürüme aittir. Sekiz gerçek buy button, doğru birer reward fulfillment, global lock, owned ve failure Editor simülasyonu ile doğrulandı; gerçek ödeme alınmadı. Duplicate store callback idempotency veya fiziksel notch cihazı testi iddia edilmez. Ekranlardaki `$0.01` Editor fake-store metadata'dır; satış fiyatları değiştirilmedi.
- Son koddan alınan ve görsel olarak incelenen gerçek Play PNG'leri: `Revision/final-shop-top-1080x1920.png`, `final-shop-bottom-1080x1920.png`, `final-shop-1080x2400.png`, `final-shop-750x1334.png`, `final-pending.png`. Üst/alt scroll, ilk/son ürün erişimi, title/reward/art/button ayrımı üç boyutta PASS. Görüntüler asenkron `ScreenCapture.CaptureScreenshot` ile alındı; MCP Step screenshot workaround'u kullanılmadı. `Revision/final-console.json`:0 hata/0 uyarı.
- Her Play oturumu için ayrı taze5JSON backup alındı; preview/final/regression/polish/closure restoration raporları5/5 SHA256 PASS. İlk final oturumundaki Unity domain reload test state'ini sıfırlayınca oturum durdurulup kendi yedeği restore edildi; temiz oturumda regresyon tamamlandı. Nihai `Revision/closure-save-restoration.json`:5/5 PASS, yeni JSON0. Pending lock false, geçici clone0; GameView original selection restore edildi. Önceki günün ilerleme yedeği kullanılmadı.
- Scoped commit yalnız yeni PNG/meta, beş prefab, presentation, presenter/card display/test/builder ve iki explicit Editor helper/meta ile seçili rapor/görselleri içerir. Kullanıcının MainMenu/font değişiklikleri, `.plastic`, geçici `.pending` draftlar dahil edilmez. 34.11 kapanışıyla faz11/11 tamam; bu kapsamda açık iş ve Phase35 başlangıcı yok.
