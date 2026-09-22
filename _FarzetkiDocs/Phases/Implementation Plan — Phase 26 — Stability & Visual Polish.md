# Implementation Plan — Phase 26 — Stability & Visual Polish

**Durum:** Devam ediyor. **Başlangıç:** 2026-09-21.
**Aktif aşama:** 26.8 — Mobil oranlar ve UI etkileşim kontrolü. 2026-09-22 tarihinde devam edildi.
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
| 26.6 | Bölüm, top sayısı, geri ve altın HUD'unda yuvarlatılmış hacimli altın/mavi tasarım; canlı metin bağları | Tamam | MainGame kaydedildi; phase26-hud-first-pass.png; mobil/input kontrolü 26.8 |
| 26.7 | Objelerin taş/altın/ahşap/enerji yüzeylerini, aydınlatmayı ve enerji parlamasını revize et | Tamam | Mevcut 6 materyal ve volume revize; phase26-visual-first-pass.png; shader hata/uyarı 0 |
| 26.8 | Mobil oranlar, HUD tıklama/oyun alanı, atış/çarpışma/restart kontrolü | Devam | Eski modal sıralaması/ayarlar satır çakışması bulundu; revizyon ve frame bekleyen test hazırlanıyor |
| 26.9 | Prefab koleksiyonunu revizyonlarla güncelle; kule/tam havan/5 tip blok görünür olsun | Bekliyor | Prefab referans kontrolü ve güncel vitrin |
| 26.10 | Son sahne/asset kaydı, oyuncu kaydı geri yükleme, diff ve rapor kapanışı | Bekliyor | Kanıtlar, kalan sınırlar ve devam noktası |

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
6. [ ] Butonlar tıklanır; HUD dışındaki atış alanı kapatılmaz; mobil oranlarda metinler çakışmaz.
7. [ ] Materyaller daha canlı; taş, ahşap, metal ve enerji okunur; Console'da derleme/runtime hatası yok.
8. [ ] Güncel prefab koleksiyonu ve sahne kaydedilmiş; mevcut kullanıcı çalışmaları ve oyuncu ilerlemesi korunmuş.

## 5. Başlangıçta mevcut kullanıcı değişiklikleri

Bu tur başlamadan dirty olan dosyalar korunur: .plastic dosyaları, TMP/Fazo fontları, SkyCitadel ObjectConfig'leri ve stand prefabı, kaynak ObjectConfig'ler, SkyCitadelSceneBuilder, PhysicsObject, ObjectConfig, LevelManager, BootScene/BootController/AppBootstrapper, GoldManager, WinPanel, bazı önceki faz testleri, VFX materyali ve TagManager. Bunlar sıfırlanmaz. İlgili fizik/builder dosyalarında mevcut değişiklikler üzerine dar kapsamlı ek yapılır.

## 6. Canlı checkpoint

- Son başarılı adım: 30/30 canlı fizik testi geçti (2026-09-22 07:07 UTC); Play Mode durduruldu. HUD uygulanıp MainGame kaydedildi.
- Aktif aşama: 26.8. Materyaller, shader ve ilk HUD sürümü uygulandı; MainGame Edit Mode.
- Sonraki somut adım: SkyCitadelHUDBuilder modal revizyonunu derle ve uygula. Play Mode'da Phase26PresentationTest.Begin() ile 1080x1920 ve 1080x2400 ayarlar/geri/cancel/raycast/small-block shrink kontrollerini yap. Sonra 26.9 prefab yenilemesi.
- İlk senkron UI denemesindeki ui-check-1080x1920.json kabul kanıtı değildir: modal etkinleştirme sonrası render frame beklemiyordu. Yeni test, olayların arasına gerçek Editor frame beklemesi ekler. Ekran görüntüsü ayrıca eski ayarlar titreşim/çıkış çakışmasını ve HUD/modal sıralama sorununu ortaya çıkardı; bunlar revize ediliyor.
- Küçük blokların küçülme animasyonunda Awake'taki 1 ölçeğine büyüme riski düzeltildi; gerçek mevcut ölçek DestroyRoutine girişinde alınır. PhysicsObject'teki mevcut kullanıcı değişiklikleri korunur.
- Güncel test öncesi oyuncu yedeği: C:/Users/Farzetki/.codex/local-backups/SkyCitadel/Phase26-20260922/PlayerSave/. Tüm Play testleri bittikten sonra bu kopyayı geri yükleyip hash doğrula. Eski 20260921 oyuncu yedeğini kullanma.
- Kanıt dizini: _FarzetkiDocs/Art/Phase26/.
- Bitiş/sonuç: Henüz tamamlanmadı; doğrulanmayan aşamalar açık tutulur.
