# Phase 32 — Sky Citadel referans görseline göre revizyon

**Başlangıç:** 27 Eylül 2026  
**Durum:** Devam ediyor — 32.1 tamamlandı; 32.2 uygulanıyor.  
**Son tamamlanan aşama:** 32.1  
**Devam noktası:** 32.2 ışık/yansıma temelini doğrula; ardından 32.3 stand revizyonu.  
**Kullanıcı isteği:** Mevcut mortar, stand, zemin ve blokları revize ederek verilen ilk görselin canlılığına, yüzey kalitesine ve hacimli detaylarına yaklaştırmak. Çalışmayı kalıcı aşamalara bölmek; sonraki oturumda tamamlanmayan aşamadan devam etmek.

## Referanslar ve kapsam

- Görsel hedef: `Assets/_Project/Art/_FarzetkiArts/_PanelsFullVersion/ChatGPT Image 24 Eyl 2026 23_51_58 (3).png`.
- Kullanıcının mevcut sahne ekranı: `C:/Users/Farzetki/AppData/Local/Temp/codex-clipboard-2ab55ccc-b5bf-4908-8714-9f433ef03b8d.png`.
- Başlangıç kanıtı: `_FarzetkiDocs/Art/Phase32/32-01-before-level30-1080x1920.png`.
- `_FarzetkiDocs/master_build_prompt.md`: §4 hedefleme, §5 top, §6 fizik, §7 objeler, §8 stand, §25 kamera/dünya, §31 performans, §33 görsel stil.
- `_FarzetkiDocs/RoyalSmashInformationFullVersion.md`: §296 görsel stil, §297 kamera/dünya sunumu, §298 çevre; mevcut atış, hedef ve kayıt sözleşmeleri.
- Önceki kayıtlar: Phase 25–26 sanat altyapısı; Phase 27–31 güncel sistem/level/fizik değişiklikleri.
- Workflow: `.agents/skills/phase_implementation_workflow/SKILL.md`, `unity_mcp_automation`, `unity_3d_physics_game`, `mobile_ui_ux`.

Referans bir sanat hedefidir. Görseldeki **22 bölüm, 15 top ve 5×3 blok düzeni**, oyunun canlı sayılarını veya 30 bölümün yerleşimini değiştirme talebi değildir. Bunlar mevcut oyun verisinden çalışmaya devam eder. Üç boyutlu nesnelerin yerine bütün referans resmi koyulmaz.

## 1. Mevcut durum ve mimari

- Unity 6000.4.0f1, URP, Linear; MainGame aktif; MCP doğrulandı.
- Ana kamera: `(0, 3.11, -5.43)`, X dönüşü `8.16°`, FOV `56`; HDR ve post-processing açık.
- Sekiz mevcut SkyCitadel materyali ve `CitadelVolume.asset` kullanılıyor. Altın/ahşap/safir/çelik yüzeyleri dokusuz; floor smoothness `0.27`; yerel reflection probe yok.
- Stand `PF_CitadelStand.prefab` instance'ı. `StandPresentation` ölçeği `(5.7, .25, 1.9)`; detay eklerken bu ölçek telafi edilmeli. Tablanın üst yüzeyi/collider yerinde kalmalı.
- Mortar mevcut `Mortar.prefab` instance'ı ve sahneye eklenmiş `CitadelMount` parçalarından oluşuyor. `Visuals`, `Visuals/BarrelPivot/FirePoint` ve aim/recoil bağlantıları korunmalı.
- Beş blok varyantı: wood, stone, crystal, armored, core. Mevcut materyale göre birleştirilmiş mesh yaklaşımı sürdürülmeli; dekoratif her civata ayrı renderer olmamalı.
- Runtime `PhysicsObject`, `PaintBottleObject` ve `ProjectileController` ana rengi/scale'i değiştirebilir; shaderlarda `_BaseColor` uyumu ve birinci renderer davranışı korunmalı.
- **Güncel fizik sözleşmesi:** `LevelStartPhysicsGate` ilk gerçek projectile temasına kadar hedefleri kinematic tutuyor. Boşluğa atış gate'i açmamalı. Eski dokümandaki “doğal Sleep” ifadesi güncel kodun tamamını anlatmıyor.
- PC ve Mobile URP ayarları farklı: PC ekran görüntüsü mobil performans/kalite kanıtı değildir.

### Uygulama yaklaşımı

`RoyalSmash.Editor.Phase32VisualRevision` dar kapsamlı ve tekrar çalıştırılabilir aşamalar sunar. Mevcut materyal/prefab GUID'leri yerinde korunur; yeni dekoratif mesh ve map'ler gerektiğinde SkyCitadel altında eklenir. Oyun collider'ları, fizik/config verileri ve level yerleşimleri görsel düzenlemelerden ayrılır. `BuildAll`, `BuildGameplay`, `BuildMortar` topluca çalıştırılmaz: bunlar sonraki fazların değişikliklerini eski ayarlara döndürebilir.

## 2. Aşamalar ve teslimatlar

| Aşama | İş | Tamamlanma şartı | Durum |
| --- | --- | --- | --- |
| **32.1** | Güncel sahne, referans, runtime müdahaleleri, fizik ve asset envanteri; yedek ve aynı kamera ile başlangıç görüntüsü | MCP/başlangıç doğrulandı, riskler ve geri dönüş yolu kayıtlı | **Tamam** |
| **32.2** | Işık, yansıma ve malzeme temeli: kontrollü baked reflection probe, altın/safir/çelik ayrımı, mermer parlaklığı, ölçülü bloom | Aynı level 30 önce/sonra; materyal/ışık dışındaki oyun verileri değişmez; konsol temiz | **Devam ediyor** |
| **32.3** | Stand ön kiriş ve ayaklar: pahlı zırh, koyu şase, merkez elmas, cyan kanallar, mekanik kaideler ve arma panelleri | Mevcut stand prefabında görünür revizyon; aynı üst yüzey/collider; farklı stand ölçeklerinde bozulma yok | Bekliyor |
| **32.4** | Mortar namlu/gövde: mavi kaplama, kalın altın bilezikler, fildişi zırh, koyu bağlantılar, detaylı enerji haznesi | Mevcut pivotlar, FirePoint, nişan ve recoil korunur; ön planda referansa yakın hacim | Bekliyor |
| **32.5** | Mortar platformu: açılı yan raylar, zırh panelleri, kenar altınları, cyan ışık yuvaları | Disk görünümü azalır; hareket sınırlarında HUD/oyun alanını kapatmaz | Bekliyor |
| **32.6** | Zemin: mevcut mermeri revize eden doku, altın derz/safir kakma ve yüzeye göre parlaklık | Perspektif/tiling tutarlı; tekrar dikişi yok; yeni collider yok; aşırı ayna etkisi yok | Bekliyor |
| **32.7** | Ahşap/taş/zırhlı bloklar: damar, derz, merkez elmas, kalın metal köşeler ve pahlı kenarlar | 3 varyant ayırt edilebilir; aynı unit collider, root ve config; birleşik mesh/materyal sayısı kontrollü | Bekliyor |
| **32.8** | Crystal/core/top enerji yüzeyleri: belirgin soket, parlak çekirdek, iç akış ve kontrollü kenar ışığı | Beyaza patlamayan cyan; top/skill renk davranışı ve kırılma/kaybolma korunur | Bekliyor |
| **32.9** | Kompozisyon ve HUD uyumu: sahne ölçeği, arka plan-zemin geçişi, hedef/stand/mortar ekran payları | 1/15/22/30. bölümler kadrajda; dinamik HUD değerleri okunur; nişan alanı açık | Bekliyor |
| **32.10** | Prefab koleksiyonu ve sanat önizlemesi; güncel stand/mortar/tüm blokların tek yerde incelenmesi | Mevcut prefab/preview koleksiyonları revize ve görünür; kopya gameplay sistemleri yok | Bekliyor |
| **32.11** | Fizik/oynanış ve mobil bütçe doğrulaması | 30 yerleşim/geometri; idle + boş atış + gerçek temas; normal/Mega/Infinity; 9:16 ve 9:20; ölçülen render maliyeti | Bekliyor |
| **32.12** | Referans karşılaştırması, son düzeltmeler, kalıcı kanıt ve devir kaydı | Aşağıdaki kabul maddeleri gerçek sonuçlarla işaretli; kalan farklar açıkça yazılı | Bekliyor |

## 3. Kabul kriterleri ve test yöntemi

1. [ ] Stand, mortar, zemin ve beş blok varyantının her biri aynı kamerada belirgin olarak revize edilmiş olmalı; yalnızca saturation artışı tamamlanma sayılmaz.
2. [ ] Mevcut scene/prefab/material GUID'leri ve gameplay referansları korunmalı; yeni sanat varlıkları düzenli adlandırılmalı.
3. [ ] Stand üst yüzeyi, root collider'lar ve 30 bölümün veri/ödül/top sayıları istemsiz değişmemeli.
4. [ ] Play Mode: hiç atış yokken ve hedef dışına atışta başlangıç düzeni sabit; gerçek temasta fizik tepki veriyor.
5. [ ] Mortar aim/drag, FirePoint, trajectory ve recoil; normal/Mega/Infinity; shrink ve destroy state'leri çalışıyor.
6. [ ] 1080×1920 ve 1080×2400 görüntüleri, özellikle geniş/tall yerleşimlerde kesilme ve UI engellemesi göstermiyor.
7. [ ] Altın plastik sarıya, enerji beyaz lekeye, mermer düz griye dönüşmüyor. Ahşap damarı ve stone derzleri oyun ölçeğinde okunuyor.
8. [ ] Runtime reflection render her kare çalışmıyor; dekoratif collider yok; yeni mesh/material/texture maliyeti kaydedilmiş. Gerçek cihaz testi yoksa 60 FPS garantisi verilmez.
9. [ ] Console/shader hata ve uyarıları kontrol edilmiş; mevcut uyarılar varsa ayrıştırılmış.
10. [ ] Play testleri sonunda oyuncunun kayıt dosyaları test öncesi byte'larına geri getirilmiş.

Planlanan güncel test aracı: `Assets/_Project/Scripts/Editor/Phase32VisualVerification.cs`; sonuçlar `_FarzetkiDocs/Art/Phase32/` altında. Eski `Phase26PlayModeTest` ilk atışta gate açılması beklediği için güncel kabul testi değildir. `Phase31QAVerificationTest.RunAllTests` kayıt/ekonomi değiştirir ve gerçek 30 bölümlük Play testi sayılmaz; körlemesine çağrılmaz.

## 4. Checkpoint / yeniden devam protokolü

1. Bu dosyanın başındaki **Son tamamlanan aşama / Devam noktası** okunur; proje ve MCP'nin güncel durumu kontrol edilir.
2. Aşama “Devam ediyor” ise önce mevcut çıktı/kanıt incelenir; tamamlanan işler tekrar üretilmez.
3. Her aşamada gerçek değişen yollar, doğrulama sonucu, ekran görüntüsü ve varsa kalan iş aşağıya eklenir. Test edilmemiş iş “tamam” yapılmaz.
4. Yeni Play oturumu öncesi o günkü player save yedeklenir. Önceki oturumun eski kaydı yeni kullanıcı ilerlemesinin üstüne yazılmaz.
5. Sahne değiştirilmeden önce dirty/unsaved kullanıcı değişiklikleri korunur. Git'teki kullanıcı değişiklikleri geri alınmaz.
6. “Devam et” yeni faz başlatmaz; ilk tamamlanmamış aşamadan ilerlenir.

## 5. Tamamlanma kayıtları

### 32.1 — Tamamlandı, 2026-09-27

- Başlangıç Git HEAD: `651ea7589750719c082b5c8127b5b3bb71d86514`.
- Kullanıcıya ait dirty dosyalar: font varlıkları, `.plastic/plastic.wktree`, `ProjectSettings/DynamicsManager.asset`; korunacak.
- Yerel yedek: `C:/Users/Farzetki/.codex/local-backups/Phase32-20260927-140238/` (MainGame, SkyCitadel sanat ağacı, Mortar prefab, PC pipeline, DynamicsManager, PlayerSave).
- Gerçek Play Mode'da Level 30 başlangıç görüntüsü alındı. Başlangıç konsolu: **0 hata, 0 uyarı**.
- Bağımsız görsel ve kod denetimi tamamlandı; eski builder/test riskleri yukarıya işlendi.
- Görsel hedef henüz tamamlanmadı; sonraki aşamalar gerçek mesh/yüzey revizyonlarını içeriyor.
