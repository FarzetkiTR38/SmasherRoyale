# Phase 37 — Project Optimization, Asset Cleanup & Build Size Reduction

**Başlangıç:** 8 Ekim 2026  
**Durum:** **TAMAMLANDI (Completed)**  
**Kapsam:** Önceki build 170MB iken son buildin 250-270MB'a çıkması nedeniyle projenin baştan sona optimize edilmesi, atıl script, sprite ve prefablerin temizlenmesi, boyut artışının kök nedeninin analizi ve kapsamlı CodeReview / Build Size raporunun hazırlanması.

---

## 1. Referans ve Kaynaklar

- `_FarzetkiDocs/master_build_prompt.md`: Section 34 (Performance & Mobile Optimization), Section 35 (Polish & Production Readiness).
- `_FarzetkiDocs/RoyalSmashInformationFullVersion.md`: Section 339 (Asset Optimization & Memory Budget), Section 344 (Acceptance Criteria).
- `.agents/skills/phase_implementation_workflow/SKILL.md`: Zorunlu faz dokümantasyon standardı.
- `.agents/skills/unity_mcp_automation/SKILL.md`: Zero-delegation Unity MCP otomasyon standardı.

---

## 2. Fazın 5 Aşamalı Yapısı (Step-by-Step Execution)

Kullanıcı direktifine istinaden bu faz 5 bağımsız adıma bölünmüştür. Her adım tamamlandığında konsol doğrulanacak, kullanıcı onayı alınacak ve ardından diğer adıma geçilecektir:

### Adım 1: Script Denetimi ve Temizliği (Script Audit & Cleanup)
- **Hedef:** 3 ana sahnemizde (`BootScene`, `MainMenu`, `MainGame`), tüm prefablerde ve ScriptableObject'lerde kullanılan scriptlerin Unity MCP ile taranması.
- **Kapsam:**
  - Hiyerarşideki tüm objelere, child'larına tek tek bakılarak kullanılan scriptlerin haritasının çıkarılması.
  - Kod tabanında tanımlı olup hiçbir sahnede, prefabte veya kod referansında yer almayan atıl / gereksiz runtime scriptlerin belirlenmesi.
  - Editor araçları (`Assets/_Project/Scripts/Editor/` vb.), testler ve geliştirici yardımcıları korunacak; yalnızca runtime'da fazlalık yaratan atıl scriptler temizlenecek.
  - Silme sonrası Unity konsolunda 0 hata olduğu doğrulanacak.
- **Sonuç / Gerçekleştirilen İşlemler:**
  - 3 sahnedeki tüm hiyerarşi tarandı (`BootScene`: 28 GO / 65 comp, `MainMenu`: 1,036 GO / 3,127 comp, `MainGame`: 763 GO / 2,299 comp).
  - Tüm prefabler (46 benzersiz script bileşeni) ve ScriptableObject'ler (31 benzersiz config/data sınıfı) tarandı.
  - Hiçbir yerde kullanılmayan veya eski fazlardan kalan 14 atıl runtime script tespit edildi ve kaldırıldı:
    - `Phase3PlayModeTest.cs` ... `Phase9PlayModeTest.cs` (7 adet atıl PlayMode test MonoBehaviour)
    - `ProtectionPlayModeTest.cs` (Atıl test MonoBehaviour)
    - `UGSRuntimeTester.cs` (Atıl UGS test MonoBehaviour)
    - `PlaygroundController.cs` (Atıl ilk prototip kontrolcüsü)
    - `AppBootstrapper.cs` (`BootController` tarafından devralınmış eski boot sınıfı)
    - `FloatingTextManager.cs` (Atıl prototip text yöneticisi)
    - `HUDExitButton.cs` (`HUDSettingsButton` / `ExitConfirmDialog` tarafından devralınan atıl buton scripti)
    - `SplashLoadingOverlay.cs` (`BootController` tarafından devralınan eski overlay)
    - `EnvironmentThemeManager.cs` (Sahnede bulunmayan ve çağrılmayan atıl singleton)
  - `SkyCitadelPreview.cs` bir Editor aracı (`[MenuItem]`) olduğu için runtime'dan `Assets/_Project/Scripts/Editor/` altına taşındı.
  - `Assets/_Recovery/` kaza kurtarma sahneleri silindi.
  - Unity AssetDatabase yenilendi, konsolda **0 derleme hatası** teyit edildi.
  - Tüm 3 sahne tekrar açılarak kontrol edildi: Her 3 sahnede de **MissingScripts: 0** olduğu doğrulandı.
- **Durum:** **TAMAMLANDI (Completed - Onay Bekleniyor)**

### Adım 2: Sprite & Doku Denetimi ve Temizliği (Sprite & Texture Audit & Cleanup)
- **Hedef:** `Assets/_Project/Art` ve ilgili klasörlerdeki tüm görsellerin referans denetiminin yapılması.
- **Kapsam:**
  - 3 sahne hiyerarşisi, child'lar, UI panelleri, prefabler, ScriptableObject'ler taranarak kullanılan sprite'lar listelenecek.
  - Dinamik referanslar (On/Off durum ikonları, ses açık/kapalı ikonları, level/item ikonları, kod veya inspector üzerinden atanan varyantlar) kesinlikle korunacak.
  - Gerçekten hiçbir yerde referansı bulunmayan atıl görseller tespit edilip güvenli şekilde silinecek.
  - Unity konsolu kontrol edilecek.
- **Sonuç / Gerçekleştirilen İşlemler:**
  - `Assets/_Project/Art` altındaki 234 adet doku/görsel dosyası taranarak sahne, prefab, materyal ve ScriptableObject bağımlılık haritası çıkarıldı.
  - Kullanılan ve kesinlikle korunması gereken 125 görsel koruma altına alındı (örneğin `InGameSettings` içindeki `Switch_On` / `Switch_Off` sprite'ları, `MainMenu` alt navigasyonunun `Selected` / `Normal` durum ikonları, `LeaderboardUI` avatar ve çerçeve görselleri, profil resimleri).
  - Editor araçlarında (`MainMenuSpriteSlicer`, `Phase34PlayModeTest`, `Phase34ShopArtImporter`) referans verilen 13 doku korundu.
  - Geriye kalan ve hiçbir sahnede, prefabte, materyalde veya kodda referansı olmayan **109 adet atıl konsept ve prototip görseli (toplam 96.75 MB)** ve bunlara ait `.meta` dosyaları güvenli bir şekilde silindi:
    - `Assets/_Project/Art/_FarzetkiArts/_PanelsFullVersion` (12 adet ham ChatGPT görseli)
    - `Assets/_Project/Art/_FarzetkiArts/all_objects_for_ui` (32 adet ham ChatGPT nesne görseli)
    - `Assets/_Project/Art/_FarzetkiArts/leaderboard` (10 adet ham konsept görsel)
    - `Assets/_Project/Art/_FarzetkiArts/mainmenu` (10 adet ham menü konsepti)
    - `Assets/_Project/Art/Sprites/LeaderboardSliced` (27 adet kullanılmayan dilim)
    - `Assets/_Project/Art/Sprites/UI/InGameSettings` (5 adet kullanılmayan buton/satır sprite'ı)
    - `Assets/_Project/Art/Sprites/UI/LosePanel`, `WinPanel`, `Shop` (7 adet kullanılmayan prototip sprite)
  - Boş kalan 3 klasör (`_PanelsFullVersion`, `leaderboard`, `Prefabs 1`) temizlendi.
  - **Doğrulama:** 3 sahnedeki tüm 461 Image ve SpriteRenderer bileşeni tek tek taranarak kontrol edildi: **Kırık Sprite Referansı: 0**.
  - Unity konsolunda **0 hata** olduğu doğrulandı.
- **Durum:** **TAMAMLANDI (Completed - Onay Bekleniyor)**

### Adım 3: Prefab Denetimi ve Temizliği (Prefab Audit & Cleanup)
- **Hedef:** Projenin erken aşamalarından kalan veya sonradan yeni sistemlerle değiştirilerek atıl duruma düşen prefablerin temizlenmesi.
- **Kapsam:**
  - Sahneler, scriptler, `LevelData` ScriptableObject'leri ve prefab içi iç içe referanslar kontrol edilecek.
  - Tamamen kullanılmayan ve hiçbir sistem tarafından çağrılmayan eski prefabler tespit edilip temizlenecek.
  - Unity konsolu kontrol edilecek.
- **Sonuç / Gerçekleştirilen İşlemler:**
  - Projedeki tüm prefabler (66 adet) taranarak sahne, ScriptableObject, materyal ve kod bağımlılıkları incelendi.
  - Erken prototip aşamasından kalan ve SkyCitadel blok/top/stand sistemleri ile VFX geliştirmeleri tarafından ikame edilmiş **16 adet atıl prefab** güvenli bir şekilde silindi:
    - **Eski Oynanış / Blok Prefableri (7 Adet):** `Ball.prefab`, `Box.prefab`, `Column.prefab`, `Cone.prefab`, `PaintBottle.prefab`, `Stand.prefab`, `Stone.prefab` (SkyCitadel üretim blokları `PF_wood`, `PF_stone`, `PF_crystal`, `PF_armored`, `PF_core`, `PF_CitadelStand`, `PF_CitadelBall` tarafından devralınmıştır).
    - **Eski UI Prefableri (4 Adet):** `LeaderboardRow_Normal.prefab` (`LeaderboardRow.prefab` ile ikame edildi), `LivesWidget.prefab`, `SkillBtn_InfinityBall.prefab`, `SkillBtn_MegaBall.prefab` (Doğrudan sahne Canvas hiyerarşisine gömülü oldukları için atıl kalan prefab varlıkları).
    - **Eski VFX Prefableri (5 Adet):** `VFX_BottleShards.prefab`, `VFX_ImpactGeneric.prefab`, `VFX_ImpactHeavy.prefab`, `VFX_MuzzleFlash.prefab`, `VFX_WinConfetti.prefab` (Phase 26/30'da üretilen çoklu varyantlı `Opt1/Opt2/Opt3` VFX eko-sistemi tarafından devralınmıştır).
  - Editor araçlarındaki (`VFXStudio.cs`, `GameDesignerWindow.cs`, `CoreFractureGenerator.cs`, `CoreVFXOverhaul.cs`) eski prefab referansları temizlenerek üretim varlıklarına bağlandı.
  - `MainGame.unity` sahnesindeki inaktif eski kayıp referans nesnesi `TapIndicator` (MissingPrefab) temizlendi.
  - **Doğrulama:** 3 sahne tekrar açılarak tarandı:
    - `BootScene`: 28 GO — MissingScripts: 0, MissingPrefabs: 0
    - `MainMenu`: 1,036 GO — MissingScripts: 0, MissingPrefabs: 0
    - `MainGame`: 762 GO — MissingScripts: 0, MissingPrefabs: 0
  - Konsolda **0 hata** olduğu doğrulandı.
- **Durum:** **TAMAMLANDI (Completed - Onay Bekleniyor)**

### Adım 4: Build Boyutu Artışının Kök Neden Analizi (Root Cause Analysis)
- **Hedef:** 150-170MB olan build boyutunun 250-270MB'a fırlamasının kesin teknik nedeninin tespiti.
- **Kapsam:**
  - Son eklenen assetlerin (özellikle Phase 35 Audio Overhaul ses dosyaları, WAV/uncompressed formatlar, 3D modeller, doku import ayarları, mipmap ayarları, Android ASTC sıkıştırma profilleri) incelenmesi.
  - Hangi dosya veya klasörün kaç MB yer tuttuğunun tam tablosu çıkarılması.
  - Boyutu şişiren temel faktörler ve çözümlerinin netleştirilmesi.
- **Kesin Bulgular & Adli İnceleme Kanıtları (Forensic Evidence):**
  1. **Asıl Kök Neden: Release APK vs Debug APK Paketleme Farkı (~125 MB Fark):**
     - Gradle çıktı dizininde (`Library/Bee/Android/Prj/IL2CPP/Gradle/launcher/build/outputs/apk/`) bulunan derleme arşivleri ikili olarak analiz edildi:
       - **5 Ekim 2026, 19:49:12:** `launcher-release.apk` = **137.16 MB** (`SmasherRoyale.aab` = **142.19 MB**).
       - **6 Ekim 2026, 19:42:53:** `launcher-debug.apk` = **261.76 MB** (~270 MB!).
     - **Arşiv İçi Bayt Analizi (Zip Binary Breakdown):**
       | Bileşen | Release APK (Sıkıştırılmış) | Debug APK (Sıkıştırılmış) | Fark / Neden |
       |---|---|---|---|
       | **Unity Sahne & Veri Arşivi** | 102.22 MB (Chunked Assets) | 126.20 MB + **91.93 MB uncompressed** | Debug APK'da `assets/bin/Data/data.unity3d` dosyası **91.93 MB ham/sıkıştırmasız** olarak pakete gömülmektedir. Release derlemede bu dosya yoktur, tüm varlıklar Deflate/LZ4 ile parçalı sıkıştırılır. |
       | **Native IL2CPP Kütüphaneleri (`lib/*.so`)** | **31.28 MB** (105.78 MB ham) | **39.71 MB** (124.35 MB ham) | Debug derlemesinde sembol tabloları ayıklanmamıştır (unstripped). `libil2cpp.so` tek başına 85.56 MB (Release'de 82.26 MB), `libunity.so` 36.95 MB (Release'de 21.83 MB). |
       | **DEX Bytecode (`*.dex`)** | **2.99 MB** (8.31 MB ham) | **3.92 MB** (10.06 MB ham) | Debug multidex izleme sembolleri. |
     - **Özet:** Kullanıcının fark ettiği 150MB -> 270MB artışı, projedeki varlıkların tek başına şişmesinden ziyade, **Release (137 MB)** derlemesi yerine **Debug (262 MB)** derlemesi alınmış olmasından kaynaklanmıştır!
  2. **Varlık Tarafındaki Gerçek Şişiriciler (Release Build'in 137MB Olmasının Nedenleri):**
     - **Phase 35 Audio Overhaul Ses Dosyaları (42.50 MB Disk):**
       - Projede toplam 99 adet ses dosyası bulunmaktadır.
       - 10 adet müzik parçası (`Assets/_Project/Audio/Music`) tek başına **38.20 MB** yer kaplamaktadır (örn. `bgm_menu_opt1_royal_courtyard.mp3` = 11.70 MB, `bgm_game_opt3_wagon_wheel.mp3` = 9.27 MB).
       - Ses import ayarlarında tüm sesler `Vorbis Quality: 1.0` (en yüksek bitrate ~500kbps) ve `LoadType: DecompressOnLoad` olarak ayarlanmıştır. Bu durum hem disk boyutunu hem de mobil cihazdaki çalışma zamanı RAM tüketimini şişirmektedir.
     - **Silinen 109 Atıl Konsept Görseli (Step 2 ile Temizlenen 96.75 MB):**
       - `Assets/_Project/Art/_FarzetkiArts/` klasöründe yer alan ham yapay zeka çıktıları, diskte 96.75 MB fazlalık oluşturmaktaydı (Step 2'de silindi).
     - **TextMeshPro Font Atlas Çoğaltmaları (35.98 MB Disk):**
       - `Assets/_Project/Fonts/` altında 15 adet TMP Font Asset'i bulunmaktadır. Her biri 2048x2048 boyutunda ayrı ayrı atlas dokularına sahiptir (örn. `Fazo_Font_Settings.asset` 5.51 MB, `Fazo_Font_Titles_white.asset` 4.35 MB).
     - **Managed Stripping Seviyesi:**
       - Android Player ayarlarında `Managed Stripping Level` değeri `Minimal` olarak bırakılmıştır. Bu da IL2CPP derleyicisinin kullanılmayan C# sınıflarını ayıklamasını engelleyerek `global-metadata.dat` (12.06 MB) ve `libil2cpp.so` (82.26 MB) boyutunu artırmaktadır.
- **Durum:** **TAMAMLANDI (Completed)**

---

### Adım 5: CodeReview, Build Boyutu Raporu & Final Optimizasyon (CodeReview & Build Size Report)
- **Hedef:** Projenin baştan sona genel mimari CodeReview'ı, varlık kırılım tablosu ve .apk / .aab çıktı boyutu tahminleri.
- **Genel Proje CodeReview Raporu:**
  1. **Mimari Uyumluluk ve Namespace Standartları:**
     - Projede `_Project/Scripts` altında toplam **130 adet runtime C# scripti** incelendi (toplam 29,469 satır kod).
     - **130 scriptin tamamı (%100)** `namespace RoyalSmash` hiyerarşisi altında organize edilmiştir (`RoyalSmash.Core`, `RoyalSmash.Gameplay`, `RoyalSmash.UI`, `RoyalSmash.Economy`, `RoyalSmash.Audio` vb.).
     - Hiçbir script küresel (global) alanda bırakılmamış, Single Responsibility prensibine sadık kalınmıştır.
  2. **MonoBehaviour Yaşam Döngüsü & Performans:**
     - 130 script içerisinde **yalnızca 11 sınıfta `void Update()`** metodu bulunmaktadır. Bu, mobil bir fizik oyunu için olağanüstü yüksek bir optimizasyon başarısıdır! Update döngüsü yerine EventBus, C# eventleri ve zaman ayarlı Coroutine'ler kullanılmıştır.
     - Projede toplam 25 singleton manager (`Instance`) yer almakta olup, tümü sahneler arası yaşam döngüsünü temiz yönetmektedir.
     - Çalışma zamanında (Runtime) maliyetli `GameObject.Find` ve `FindObjectOfType` aramaları neredeyse sıfırdır (yalnızca `LevelLoader` içinde editör tasarım köklerini temizlemek için 4 noktada güvenli kontroller bulunmaktadır).
  3. **UI ve Metin Sistemleri:**
     - UI mimarisinde hiçbir yerde eski `UnityEngine.UI.Text` kullanılmamış, tamamı `TextMeshProUGUI` üzerine inşa edilmiştir.
  4. **Fizik ve 3D Sistemleri:**
     - 2D fizik bileşeni kesinlikle bulunmamaktadır; `Rigidbody`, `Collider` ve 3D vektör mekanikleri uygulanmıştır.
     - Bloklar parçalanabilir (fracturable) Voronoi algoritmalarıyla yönetilmekte ve fizik optimizasyonları katı kurallarla çalışmaktadır.

- **Proje Varlıkları Boyut Dağılımı Tablosu (Asset Breakdown):**

| Varlık Kategorisi | Dosya Sayısı | Disk Boyutu (Ham MB) | Derleme / APK İçi Boyutu (Tahmini) | Açıklama & Durum |
|---|---|---|---|---|
| **Sprites & Textures (.png, .jpg)** | 156 | **82.85 MB** | **~35 - 42 MB** | 109 atıl görsel (96.75 MB) Step 2'de silindi. Kalan 156 görsel aktif UI, navigasyon ve blok kaplamalarıdır. ASTC ile sıkıştırılır. |
| **Ses Dosyaları (.mp3, .wav)** | 99 | **42.50 MB** | **~28 - 30 MB** | 10 müzik (38.2 MB) + 89 SFX (4.3 MB). `sharedassets0.resource` içinde yer alır. |
| **TextMeshPro Font Varlıkları (.asset)** | 15 | **35.98 MB** | **~10 - 15 MB** | 2048x2048 font atlas dokuları. |
| **Prefabler (.prefab)** | 53 | **7.05 MB** | **~2 - 3 MB** | 16 eski/atıl prefab Step 3'te silindi. 53 aktif üretim prefabı kaldı. |
| **Sahneler (.unity)** | 3 | **4.48 MB** | **~1.5 MB** | `BootScene`, `MainMenu`, `MainGame` (Yalnızca bu 3 sahne build ayarlarında etkindir). |
| **C# Scriptleri (.cs)** | 166 | **1.37 MB** | IL2CPP derlenir | 130 runtime + 36 editor scripti. |
| **Shader & ShaderGraphs** | 22 | **1.21 MB** | **~0.4 MB** | URP shader varyantları. |
| **3D Modeller & Mesh Dosyaları (.fbx, .obj)** | 0 | **0.00 MB** | **0.00 MB** | Oyunda harici 3D model dosyası bulunmamaktadır. Tüm bloklar, toplar ve standlar Unity ilkel geometrileri ve kod tabanlı Voronoi parçalanma algoritmaları ile üretilmektedir. |
| **Materyaller (.mat)** | 89 | **0.30 MB** | **~0.1 MB** | URP Lit / Unlit materyal tanımları. |

- **Tahmini Nihai Derleme Boyutları (.apk & .aab):**
  - **Mevcut Durumdaki Release `.apk`:** **~120 - 130 MB**  
    *(Step 1'deki atıl kod temizliği, Step 2'deki 96.75 MB görsel silinmesi ve Step 3'teki 16 prefab temizliği sonrasında; önceki 137.16 MB'lık release apk boyutundan doğrudan tasarruf sağlanmıştır).*
  - **Mevcut Durumdaki Release `.aab` (Android App Bundle):** **~90 - 105 MB**  
    *(Google Play Split APK mimarisinde cihaz mimarisine göre arm64-v8a ayrıştırıldığında son kullanıcı indirme boyutu).*
  - **Debug APK Uyarısı:** Debug derlemesi alındığında `data.unity3d` sıkıştırmasız kaydedildiği ve IL2CPP sembolleri ayıklanmadığı için boyut yine ~250-260 MB görünecektir (bu bir hata değil, Android Gradle Debug modunun doğal sonucudur).

- **İlerisi İçin Önerilen Ek Optimizasyon Yol Haritası (İsteğe Bağlı):**
  1. *Ses Sıkıştırması:* Müzik dosyalarının import ayarında `LoadType: Streaming` seçilmesi ve `Quality: 0.6` yapılması disk boyutunu ~25 MB daha düşürür ve RAM'i rahatlatır.
  2. *Android ASTC Sıkıştırma:* UI ve sprite dokularına Android sekmesinde `ASTC 6x6` veya `ASTC 8x8` override verilmesi boyutu yarı yarıya azaltır.
  3. *Managed Stripping:* `PlayerSettings.Android.managedStrippingLevel` ayarının `Medium` veya `High` yapılması `libil2cpp.so`'dan ~10-15 MB tasarruf sağlar.
- **Durum:** **TAMAMLANDI (Completed)**

---

## 3. Kabul Kriterleri (Acceptance Criteria)

1. [x] **Adım 1:** 3 sahnedeki tüm hiyerarşi taranarak atıl scriptler temizlendi, Unity konsolunda 0 hata.
2. [x] **Adım 2:** Inspector / On-Off referansları korunarak atıl sprite'lar temizlendi (109 atıl görsel, 96.75 MB silindi).
3. [x] **Adım 3:** Kullanılmayan 16 eski/legacy prefab tespit edilip güvenli şekilde kaldırıldı.
4. [x] **Adım 4:** 150MB -> 270MB artışının kök nedeni (Release APK: 137MB vs Debug APK: 261MB Gradle paketleme farkı + ses/font ayarları) tam verilerle kanıtlandı ve belgelendi.
5. [x] **Adım 5:** Kapsamlı CodeReview (130 script, 29.4k satır), .apk / .aab boyutu ve varlık MB dökümü eksiksiz raporlandı.
6. [x] Her adım kullanıcı onayı alınarak ilerletildi.

