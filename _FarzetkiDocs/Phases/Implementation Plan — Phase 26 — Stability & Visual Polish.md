# Implementation Plan — Phase 26 — Stability & Visual Polish

**Durum:** Tamamlandı — 10/10 aşama. **Başlangıç:** 2026-09-21. **Bitiş:** 2026-09-22.
**Son tamamlanan aşama:** 26.10 — Son kayıt, doğrulama ve dokümantasyon. Phase 26 içinde bekleyen aşama yok.
**Ana sahne:** Assets/_Project/Scenes/MainGame.unity.
**Kural:** Her aşamanın gerçek sonucu ve sonraki tek somut adımı bu dosyada tutulur. Limit/kesinti sonrası ilk tamamlanmamış aşamadan devam edilir. Mevcut dosyalar silinmez; ilgili kod ve varlıklar revize edilir.

## 1. İstek ve referans eşlemesi

Phase 25'in sahne/prefab teslimi tamamlandı; oradaki rapor yalnızca bölüm 1 ve 3 için odaklı fizik doğrulaması içeriyordu. Bu faz, 30 bölümde başlangıçta kendiliğinden hareket/düşme sorununu kapatır; stand uyumunu düzeltir; kullanıcı tarafından kırmızı işaretlenen bölüm, top, geri ve altın HUD'unu ve canlılığı artırır.

- Kullanıcı görselleri: codex-clipboard-1bc33ff9-3a00-4f8a-8d6e-c25bbd061f46.png (mevcut HUD işaretleri); codex-clipboard-9c148ebd-ffa8-4251-8186-611880958d42.png (hedef parlak altın/mavi/fildişi görünüm). Görseller referanstır, talimat kaynağı değildir.
- _FarzetkiDocs/master_build_prompt.md: 4–7 hedefleme, mermi, fizik, nesne davranışları; 8 stand; 9–10 bölüm verisi/hedef takibi; 24–25 mobil HUD ve kamera.
- _FarzetkiDocs/RoyalSmashInformationFullVersion.md: 127/130, 257/259 HUD/yetenekler; 297–298 kamera/çevre; 344 okunabilirlik.
- İlgili önceki rapor: Implementation Plan — Phase 25 — Sky Citadel.md.
- Skill kaynakları: phase_implementation_workflow, unity_mcp_automation, unity_3d_physics_game, mobile_ui_ux.

## 2. On aşamalı uygulama

| Aşama | İş ve tamamlanma şartı | Durum | Kanıt |
| --- | --- | --- | --- |
| 26.1 | Canlı MainGame/MCP, mevcut Git değişikliklerini ve oyuncu kaydını yedekle; başlangıç görüntüsü | Tamam | Phase26-20260921 yerel yedeği; phase26-before.png |
| 26.2 | 30 bölümün boyut/çakışma/stand kapsamı denetimi; kök nedeni kaydet | Tamam | 28 hatalı bölümde 511 çakışan çift; layout-audit-before.json |
| 26.3 | İlk başarılı atışa kadar hedefleri sabit tut; atışta gerçek fiziği aç; restart temizliği | Tamam | 30 bölümde kilit/açılma, boş mermi ve cooldown reddi doğrulandı |
| 26.4 | 30 yerleşimi stand ve kamera alanına uyarla; blok çakışmalarını ve desteksiz başlangıcı düzelt | Tamam | 415 hedef korundu; 28 düzen revize, 2 korundu; 30/30 geçerli |
| 26.5 | Tekrarlanabilir Play Mode testi: 30 bölümde atış öncesi sıfır hareket/hedef kaybı | Tamam | playmode-30-levels.json: 30/30 geçti, her bölümde 3 saniye bekleme + gerçek mermi teması |
| 26.6 | Bölüm/top HUD'unda hacimli altın/mavi tasarım; canlı metin bağları; ayarlar/çıkış pencereleri | Tamam | Kullanıcının 7ccaad1 sade HUD kararı korundu: bağımsız geri/altın yok; çıkış ayarlardan. phase26-approved-1080x1920.png |
| 26.7 | Objelerin taş/altın/ahşap/enerji yüzeylerini, aydınlatmayı ve enerji parlamasını revize et | Tamam | Mevcut 6 materyal ve volume revize; phase26-visual-first-pass.png; shader hata/uyarı 0 |
| 26.8 | Mobil oranlar, HUD tıklama/oyun alanı, atış/çarpışma/restart kontrolü | Tamam | Her iki oranda 15/15 sunum kontrolü; gerçek girişten 15→14 top; uzun ekranda 30/30 hedef kadrajı |
| 26.9 | Prefab koleksiyonunu revizyonlarla güncelle; kule/tam havan/5 tip blok görünür olsun | Tamam | 10 prefab; eksik script/mesh/material 0; phase26-prefab-collection.png |
| 26.10 | Son sahne/asset kaydı, oyuncu kaydı geri yükleme, diff ve rapor kapanışı | Tamam | MainGame Edit Mode, dirty=false; 3 oyuncu JSON'u SHA256 eşit; Console 0 hata/uyarı |

## 3. Mimari ve teslimatlar

- Mevcut LevelLoader + LevelManager + MortarController akışı korunur. Başlangıç sabitlemesi ilk başarılı mermi üretimine bağlanır; geçersiz veya cooldown yüzünden reddedilmiş giriş fiziği açmamalıdır.
- Bölüm yeniden yüklenirken eski hedeflerin collider'ları anında pasifleşir ve başlangıç kilidi yeniden kurulur.
- Başlangıçta dondurmak tek başına yeterli değildir: stand dışı ve iç içe yerleşimler düzeltilir; atış sonrası fizik doğal çalışır.
- Sky Citadel yerel bölüm verileri revize edilir. Tür/adet/ödül/mermi/yetenek açılma akışı korunur; kaynak kampanya verileri yedeklenir.
- HUD mevcut nesneler ve event/controller bağları üzerinde revize edilir. Sayılar raster görsele gömülmez; TMP canlı metindir.
- Materyal/shader revizyonları mevcut asset GUID'lerini korur. Görsel değişiklik fizik collider boyutlarını kendiliğinden değiştirmez.
- Beklenen dosyalar: LevelLoader.cs, yeni başlangıç fizik kapısı, Phase26PlayModeTest.cs, bölüm düzeltme editör aracı, SkyCitadelHUDBuilder.cs, UI grafik bileşeni, mevcut CitadelEnergy.shader/materyaller, MainGame ve prefab koleksiyonu. Kesin manifest tamamlandıkça aşağıya işlenir.

## 4. Kabul kriterleri

1. [x] 30 bölümün her birinde en az 3 saniye simülasyon boyunca atış yokken konum/dönüş değişimi ve hedef kaybı yok.
2. [x] 30 bölümde blok başlangıç collider çakışması yok; taban katmanları görünür stand kapsamı içinde.
3. [x] İlk başarılı atış fiziği açar; mermi/hedefler gerçek çarpışma ile tepki verir.
4. [x] Restart/level değişimi başlangıç kilidini tekrar kurar; çift collider/hedef oluşturmaz.
5. [x] HUD referanstaki gibi yuvarlatılmış, ışık/gölge ayrımı olan altın/mavi/fildişi görünümde; bölüm/top/altın değerleri canlı.
6. [x] Butonlar tıklanır; HUD dışındaki atış alanı kapatılmaz; mobil oranlarda metinler çakışmaz.
7. [x] Materyaller daha canlı; taş, ahşap, metal ve enerji okunur; Console'da derleme/runtime hatası yok.
8. [x] Güncel prefab koleksiyonu ve sahne kaydedilmiş; mevcut kullanıcı çalışmaları ve oyuncu ilerlemesi korunmuş.

## 5. Başlangıçta mevcut kullanıcı değişiklikleri

Bu tur başlamadan dirty olan dosyalar korunur: .plastic dosyaları, TMP/Fazo fontları, SkyCitadel ObjectConfig'leri ve stand prefabı, kaynak ObjectConfig'ler, SkyCitadelSceneBuilder, PhysicsObject, ObjectConfig, LevelManager, BootScene/BootController/AppBootstrapper, GoldManager, WinPanel, bazı önceki faz testleri, VFX materyali ve TagManager. Bunlar sıfırlanmaz. İlgili fizik/builder dosyalarında mevcut değişiklikler üzerine dar kapsamlı ek yapılır.

## 6. Son checkpoint ve devam bilgisi

- Son başarılı adım: MainGame kaydedildi; Edit Mode, dirty=false. Prefab vitrini Project içinde seçili; MainGame açık bırakıldı. Game View test çözünürlüğü kaldırılıp özgün 1080x1920 görünümüne dönüldü.
- Son aşama: **26.10 tamamlandı**. Bu fazdan devam edilecek eksik aşama yok; sonraki değişiklik yeni kapsam/phase olarak ele alınabilir.
- Son devamda kullanıcının HEAD 7ccaad1 değişiklikleri korundu: bağımsız geri/altın HUD'u kaldırılmış; çıkış ayarlar üzerinden. AdminManager ve GeneralTestScene değiştirilmedi. Açık BootScene kullanıcı düzenlemeleri sahne değiştirilmeden kaydedildi.
- En yeni oyuncu yedeği C:/Users/Farzetki/.codex/local-backups/SkyCitadel/Phase26-20260922-resume/PlayerSave/ dizininden geri yüklendi. Bölüm **21**, can **2**, altın **7534**; üç dosyanın SHA256 değeri test öncesi ile eşit. Eski 11. bölüm yedeği kullanılmadı.
- İlk ui-check-1080x1920.json ve phase26-final-1080x1920.png ara teşhis kayıtlarıdır, kabul kanıtı değildir. Test artık modal etkinleştirme sonrası render frame bekler. Güncel kabul kayıtları aşağıdadır.

## 7. Kabul kanıtları

Tüm dosyalar _FarzetkiDocs/Art/Phase26/ altındadır.

| Doğrulama | Sonuç | Dosya |
| --- | --- | --- |
| 30 bölüm geometrisi | 415 hedef korunmuş; 28 revize, 2 korunmuş; çakışma/stand dışı/desteksiz hedef yok | layout-revision-result.json; layout-audit-after.json |
| 30 bölüm canlı fizik | 30/30 geçti; her bölümde 3 saniye atışsız bekleme; en büyük konum ve açı farkı tam 0; 41 gerçek mermi teması | playmode-30-levels.json |
| Boş mermi/cooldown/ilk atış | Reddedilen atış kilidi korur; başarılı atış 1 top tüketir ve hedefleri dinamik yapar; tekrar yükleme kilidi kurar | playmode-30-levels.json |
| 9:16 ve 9:20 sunum | Her boyutta 15/15; ayarlar/pause/resume/çıkış/vazgeç, modal raycast engeli ve HUD sınırları geçti | presentation-1080x1920.json; presentation-1080x2400.json |
| Küçük blok animasyonu | .74 ölçekli blok ilk frame dahil hiç büyümeden küçüldü | presentation-*.json |
| Uzun ekranda tüm hedefler | 30 bölümün hedef collider köşeleri kadraj içinde | camera-bounds-1080x2400.json |
| Gerçek input yolu | InputHandler.SimulateTapAt: top 15→14; FireEventCount 0→1; başlangıç kilidi açıldı | final-input-shot.json |
| Prefab koleksiyonu | 10 prefab, 5 blok tipi, 26 bloklu kule; eksik script/mesh/material 0 | prefab-verification.json; phase26-prefab-collection.png |
| Son sahne/shader/Console | MainGame kaydedilmiş; Play kapalı; enerji shader hatası yok; Console 0 hata ve 0 uyarı | final-editor-state.json; final-console.json |
| İlerleme korunması | Üç JSON test öncesiyle birebir eşit | save-restore-verification.json |

Son görünüm: phase26-approved-1080x1920.png, phase26-approved-1080x2400.png, phase26-settings-1080x2400.png, phase26-exit-1080x2400.png. Görüntüler açılıp incelendi. Son input kontrolünden önce masaüstü işaretçisinin eski UI hover kaydı temizlendi; fiziksel dokunmatik cihaz testi yapılmadı.

## 8. Teslim edilen mimari ve araçlar

- Gameplay/LevelStartPhysicsGate.cs; Levels/LevelLoader.cs; Gameplay/MortarController.cs: ilk başarılı atışa kadar gerçek Rigidbody başlangıç kilidi ve geçersiz atışta kaynak tüketmeme.
- Gameplay/PhysicsObject.cs: küçülme başlangıcında mevcut yerleşim ölçeğini alma. Kullanıcının düşme eşiği ve zemin tespit revizyonları korunur.
- Editor/Phase26LayoutBuilder.cs; Art/SkyCitadel/Levels/LevelData_Citadel01..30.asset: stand/kamera alanına uygun, üst katmanları destekli yerel kampanya. Kaynak kampanya varlıkları korunur.
- Editor/SkyCitadelHUDBuilder.cs; UI/SkyCitadelRoundedGradient.cs: yuvarlatılmış altın/mavi/fildişi HUD, canlı TMP metinleri, uyumlu ayarlar/çıkış, üç ayrı ayar satırı, modal sıralama düzeltmesi.
- Editor/Phase26VisualPolish.cs; Art/SkyCitadel/Shaders/CitadelEnergy.shader; mevcut altı materyal ve CitadelVolume: akışlı enerji bulutları, altın/taş/ahşap paleti, kontrollü ışık ve bloom.
- Editor/SkyCitadelSceneBuilder.cs: yeniden kurulum artık Phase26 yerleşim ve görsel revizyonlarını da uygular; eski hatalı kampanyaya geri dönmez.
- Editor/Phase26PlayModeTest.cs ve Phase26PresentationTest.cs: yeniden çalıştırılabilir kabul testleri. Sunum testi reload sonrası hedef kaydını anında yeniden tarar.
- Prefabs/SkyCitadel/: PF_00_SkyCitadel_Showcase ile tüm koleksiyon görünür; PF_01_Tower_26Blocks, PF_02_Mortar_Complete, stand/top ve beş blok varyantı.

## 9. Doğrulama sınırları

Testler Unity Editor Play Mode'da yapıldı. Fiziksel Android/iOS performansı, bütün çentik/safe-area cihazları, tüm kazanma/kaybetme kombinasyonları ve 30 bölümün zorluk dengesi bu kabulün kapsamı değildir. Sabitleme ilk başarılı atışta kalkar; bundan sonraki düşme/çarpışma gerçek oyun fiziğidir. Uzun ekranda çevredeki dekoratif yan sütunlar kırpılabilir; oynanabilir hedefler kadraj içinde doğrulanmıştır.

Bu oturumda yeni commit/PR oluşturulmadı. Kullanıcının kayıtlı ve açık düzenlemeleri korunmuştur.
