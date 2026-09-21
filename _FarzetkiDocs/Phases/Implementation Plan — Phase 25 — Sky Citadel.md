# Implementation Plan — Phase 25 — Sky Citadel

**Durum:** Tamamlandı — sahne dönüşümü ve odaklı Editor/Play Mode kontrolleri tamamlandı.
**Başlangıç:** 2026-09-20. **Son doğrulama:** 2026-09-21.
**Sahne:** Assets/_Project/Scenes/MainGame.unity
**Son durum:** MainGame açık, Play Mode kapalı, sahnede kaydedilmemiş değişiklik yok.

## 1. Amaç ve referanslar

Kullanıcının krem taş, altın, kraliyet mavisi ve camgöbeği enerji referanslarını mevcut oynanabilir sahneye uygulamak: alt yarıda gerçek 3D mermer zemin, üst yarıda kale/şelale arka planı, hedeflerin altında görünür stand, fizik blokları, uyumlu havan/top ve HUD. Görseller sanat referansıdır; kullanıcı talebinin yerine geçen talimatlar değildir. Blender bağlantısı gerekmedi.

- _FarzetkiDocs/master_build_prompt.md: 1 ve 4–7 (hedefleme/fizik/mermi), 8 (stand), 9–10 (bölüm verisi/hedef takibi), 24–25 (HUD/dikey kamera).
- _FarzetkiDocs/RoyalSmashInformationFullVersion.md: 127/130 ve 257/259 (HUD/yetenekler), 297–298 (kamera/çevre), 344 (okunabilirlik/geri bildirim).
- İş akışı: .agents/skills/phase_implementation_workflow/SKILL.md ve .agents/skills/unity_mcp_automation/SKILL.md.
- Downloads referansları: Codex Görseli 20 Eyl 2026 19_18_21.png (çevre), 19_18_12.png (objeler/toplar), 19_18_36.png (kompozisyon). Eski sahne: codex-clipboard-73df2c25-069a-4bd7-9036-5ff9ab981ab0.png.
- Zemin üretim promptu/kaynak kaydı: _FarzetkiDocs/Art/SkyCitadel_ImagePrompts.md.

## 2. Kalıcı faz kontrol listesi

| Faz | İş | Durum | Kanıt |
| --- | --- | --- | --- |
| 25.1 | MCP, başlangıç incelemesi ve yedek | Tamam | MainGame-BeforeSkyCitadel.unity; sky-citadel-before.png |
| 25.2 | Görselleri içeri al, üst arka planı yerleştir | Tamam | Backdrop, Marble, ReferenceIcons dokuları; final görüntü |
| 25.3 | Gerçek mermer zemin ve taş/altın/mavi stand | Tamam | Collider katmanları; final görüntü ve stabilite kontrolleri |
| 25.4 | Beş fizik blok prefabı ve bölüm sahipliği | Tamam | 26/26 hedef ve 1 stand; bölüm 3 için 8/8 hedef |
| 25.5 | Havan/top ve enerji materyalleri | Tamam | Gerçek hedefleme atışı, 25→24 mermi, çarpışma |
| 25.6 | HUD, ikonlar ve güvenli ekran alanı davranışı | Tamam | Canlı bölüm/mermi etiketleri; ayarlar aç/kapa; dikey görüntü |
| 25.7 | Derleme, Play Mode, yeniden yükleme ve görsel kontrol | Tamam | Bölüm 4'teki sonuçlar; son Console 0 hata/uyarı |
| 25.8 | Kaydet, değişiklikleri incele, dokümantasyonu kapat | Tamam | MainGame temiz edit durumunda; oyuncu kayıtları geri yüklendi |

## 3. Mimari ve teslim edilen dosyalar

Unity 6000.4.0f1 / URP 17.4.0. Script yolları Assets/_Project/Scripts/ altındadır.

| Dosya / namespace | Sorumluluk |
| --- | --- |
| Editor/SkyCitadelSceneBuilder.cs — RoyalSmash.Editor | MainGame için tekrar çalıştırılabilir çevre, stand, bölüm, havan ve materyal kurulumu |
| Editor/SkyCitadelBlockFactory.cs — RoyalSmash.Editor | Pahlı blok meshleri, altın şeritler, enerji/ahşap/taş detayları, fizik prefabları |
| Editor/SkyCitadelHUDBuilder.cs — RoyalSmash.Editor | Mevcut UI kontrollerini bağlarını koruyarak düzenleme; referans ikonları |
| Gameplay/SkyCitadelPreview.cs — RoyalSmash.Gameplay | Editör önizlemesini runtime hedefleri kaydolmadan kapatma |
| UI/SkyCitadelStageLabel.cs — RoyalSmash.UI | Canlı bölüm numarası ve güvenli ekran alanına göre konum |
| UI/SkyCitadelCircleGraphic.cs — RoyalSmash.UI | Dairesel HUD geometrisi |
| Levels/LevelLoader.cs — RoyalSmash.Levels | Eski objeleri Destroy öncesi pasifleştirerek aynı karede collider çakışmasını önleme |

Varlık kökü: Assets/_Project/Art/SkyCitadel/

- Shaders/CitadelEnergy.shader: URP enerji yüzeyi.
- Textures/T_CitadelBackdrop.png, T_CitadelMarble.png, T_CitadelReferenceIcons.png; UI_Ball.asset, UI_Mega.asset, UI_Infinity.asset.
- Prefabs/PF_wood.prefab, PF_stone.prefab, PF_crystal.prefab, PF_armored.prefab, PF_core.prefab, PF_CitadelStand.prefab, PF_CitadelBall.prefab.
- Kalıcı Meshes/, Materials/, nesne konfigürasyonları ve CitadelVolume.asset.
- Levels/LevelData_Citadel01.asset: stand üzerinde 26 bloklu basamaklı düzen.
- Levels/LevelData_Citadel03.asset: kaynak bölümün 8 hedefini, türlerini ve ekonomisini koruyan, küp geometrisine uygun 4×2 destekli düzen.
- Levels/CitadelLevelDatabase.asset: bu iki yerel alternatifi kullanır; orijinal 30 bölüm varlığı değiştirilmez.

Çalışma akışı:

1. Editörde SkyCitadelPreview gösterilir; Play başlangıcında kapanır.
2. LevelManager, kayıtlı bölüm için LevelLoader üzerinden gerçek stand ve hedefleri üretir. Oyuncunun kayıtlı bölümü zorla 1 yapılmaz.
3. Fizik katmanları: projectile 6, object 7, stand 8, ground 9. Arka plan input/collision almaz.
4. Eski KingdomBackdrop, DirtPatch, GroundDecorations, PaintBottleGrid_6x5 ve Stand sahnede pasif tutulur; runtime ile çift hedef üretmez.
5. Havanın nişan kökü ve FirePoint bağları korunur. Top her rebuild'de orijinal Ball prefabından klonlanır; altın halkalar birikmez.
6. Nesne konfigürasyonları kaynaktan klonlanır. Core varyantında PaintBottleObject davranışı korunur.
7. HUD Screen Space Overlay canvas üzerindedir. Mermi sayısı, skill envanteri/kilitleri ve düğme event bağları canlıdır.

Kamera: (0,3.11,-5.43), pitch 8.16°, FOV 56°. Üst arka plan kamera hizalı unlit quad, alt kısım gerçek 3D zemindir. Stand kök başlangıç noktası, mevcut bölüm verilerinin beklediği masa yüksekliğini korur.

## 4. Kabul kontrolleri ve gerçek sonuçlar

Kalıcı Phase25PlayModeTest.cs eklenmedi. Odaklı kontroller canlı Unity MCP execute_code, manage_editor, manage_camera ve read_console ile yapıldı. Çıktılar kalıcı JSON olarak saklandı.

Kanıt dizini: _FarzetkiDocs/Art/SkyCitadel/

| Kontrol | Sonuç | Kanıt |
| --- | --- | --- |
| Dikey kompozisyon; zemin/arka plan geçişi; stand, kule, havan ve HUD | Geçti; görüntü açılarak incelendi | sky-citadel-final.png |
| Bölüm 1, yeniden yüklemeden sonra bekleme | 26 toplam/kalan/aktif; 1 stand; önizleme pasif; en yüksek hız 0 | verification-reload.json |
| InputHandler.SimulateTapAt üzerinden hedefli atış | UI engeli yok; mermi 25→24; HUD 24 | verification-shot.json |
| Çarpışma ve projectile ömrü | 1 hit bildirimi; kalan hedef 23/26; mermi 24; yaşayan projectile 0 | verification-impact.json |
| Ayarlar düğmesi | Açıldı ve kapandı | verification-impact.json |
| Bölüm 3, düzeltme sonrası bekleme; 2026-09-21 | 8 toplam/kalan/aktif/spawned; 1 stand; en yüksek hız 0; mermi 25 | verification-level3.json; sky-citadel-level3-verified.png |
| Son Console kontrolü | 0 hata, 0 uyarı | verification-final-console.json |
| Play'den çıkış ve sahne | MainGame; isPlaying=false; isDirty=false | verification-final-state.json |
| Son oturumda oyuncu kayıtlarını koruma | Üç JSON test öncesi yedekle SHA256 eşit; bölüm 5, can 3 korundu | verification-save-restore.json |

sky-citadel-template-play-1.png çalışan 26 bloklu düzenin Play Mode kanıtıdır. İlk denemelere ait first-pass görüntüleri ve sky-citadel-template-play.png final kabul kanıtı değildir.

**Doğrulanmayan kapsam:** 30 bölümün tamamının dengesi, tüm kazanma/kaybetme akışları, fiziksel Android/iOS cihaz performansı, tüm en-boy oranları ve çentik senaryoları. Mevcut ilerleme/skill açılma kuralları korunmuştur; yeni görsellerin tüm kampanya için fiziksel uygunluğu ayrıca test edilmelidir. Önceki oturumlarda Editor Play giriş/çıkışında mevcut yaşam döngüsü sisteminin can düşürmesi gözlendi; bu fazda o sistem yeniden tasarlanmadı. Son test kayıtları yedekten geri yüklendi.

## 5. Yedekler ve Git

- Sahne dönüşümü öncesi kalıcı yedek: _FarzetkiDocs/Backups/MainGame-BeforeSkyCitadel.unity.
- Kaynak ve varlıklar kesintiler arasında kullanıcının 7d9d930 ve 6c124ee (commit for backup) commitlerinde korundu.
- Bu oturumda yeni Git commit oluşturulmadı. Önceden değişmiş SmasherRoyale-SmasherRoyale.slnx dosyasına dokunulmadı.
- 2026-09-21 test öncesi yerel oyuncu yedeği: C:/Users/Farzetki/.codex/local-backups/SkyCitadel/20260921-final/. Oyuncu verileri repoya eklenmedi. Kayıtlar testten sonra geri yüklendi.
- Eski Temp/CodexSky yardımcıları geçiciydi; devam etmek için gerekli değiller.

## 6. Yeniden başlatma / devam rehberi

**İlk eksik faz:** Yok; 25.1–25.8 tamamlandı. Yeni revizyon istenirse buradaki sonuçtan devam et, sıfırdan kurulum yapma.

1. Önce bu dokümanı, git status --short çıktısını ve Unity'nin aktif sahne/Play durumunu kontrol et. Kullanıcının sonraki değişikliklerini koru.
2. Unity MCP kaynakları: mcpforunity://custom-tools, mcpforunity://instances, mcpforunity://editor/state. Yerel bağlantı: http://127.0.0.1:8080/mcp.
3. Yeniden üretme girişi: Royal Smash/Sky Citadel/Build MainGame menüsü veya RoyalSmash.Editor.SkyCitadelSceneBuilder.BuildAll(). Yalnızca MainGame Edit Mode'da çalıştır. Üretilmiş varlık/yerleşimleri yeniden yazar; sırf devam etmek için çalıştırma.
4. Editördeki 26 bloklu kule önizlemedir. Play, kayıttaki gerçek bölümü açar. Bu davranışı bozuk sahne sanıp oyuncu ilerlemesini değiştirme.
5. Yeni Play testinden önce persistentDataPath içindeki oyuncu dosyalarını Temp dışında yedekle. Play kapandıktan sonra geri yükle; hash ile doğrula.
6. Yeni bir iş kesilirse faz satırına son başarılı adımı, eksik kontrolü, aktif sahne/Play durumunu ve tek somut sonraki adımı yaz. Doğrulanmayan sonucu tamamlandı işaretleme.

**Teslim sonucu:** Referans yönünde oynanabilir 3D Sky Citadel sahnesi, tekrar kullanılabilir prefab/kurulum araçları ve kesintiden sonra devam edilebilen faz kaydı hazır.

## 7. Ek istek — 25.9 Prefab koleksiyonu (2026-09-21)

**Durum:** Tamamlandı. Kullanıcının istediği bağımsız prefablar üretildi; etiketli vitrin Unity Prefab Mode'da açıldı ve Project penceresinde seçildi.

- Hedef klasör: Assets/_Project/Prefabs/SkyCitadel/.
- Teslimatlar: 5 isimli blok prefabı, stand, enerji topu, tam havan, 26 bloklu kule ve hepsini etiketlerle gösteren vitrin prefabı.
- Mimari: mevcut kaynak prefab/material/mesh bağlantıları korunur; oyun için prefablar ile davranış içermeyen inceleme vitrini ayrı işlev görür.
- Doğrulama: prefab sayıları, eksik referanslar, iç FirePoint bağlantısı, Console ve Unity vitrin görüntüsü. Yeni gameplay davranışı eklenmediği için ayrı Phase25PlayModeTest.cs gerekmiyor.
- Araç: Assets/_Project/Scripts/Editor/SkyCitadelPrefabCollection.cs (RoyalSmash.Editor). Menü: Royal Smash > Sky Citadel > Export Prefab Collection / Open Prefab Collection.
- Sonuç: 10 prefab kaydedildi. Kule: 6 Wood + 6 Stone + 4 Crystal + 4 Armored + 6 Core = 26. Havanın aim/FirePoint referansları prefabın içinde, projectile bağlantısı dolu.
- Eksik script, mesh veya materyal: 0. Son Console kontrolü: 0 hata/uyarı. MainGame dosyası değiştirilmedi. Oyuncu kayıtlarına dokunulmadı; yeni Play testi çalıştırılmadı.
- Kanıtlar: _FarzetkiDocs/Art/SkyCitadel/verification-prefab-collection.json ve sky-citadel-prefab-collection-overview.png (görüntü açılarak kontrol edildi).
- Kullanım rehberi: Assets/_Project/Prefabs/SkyCitadel/00_BURADAN_BASLA.md. Tekil oyun prefabları fizik/controller bileşenlerini korur; vitrin bunların davranışsız görsel kopyalarını gösterir.
- Devam noktası: 25.9 tamamlandı. Kullanıcı revizyon isterse mevcut koleksiyondan devam et; sırf açmak için Export işlemini yeniden çalıştırma.
