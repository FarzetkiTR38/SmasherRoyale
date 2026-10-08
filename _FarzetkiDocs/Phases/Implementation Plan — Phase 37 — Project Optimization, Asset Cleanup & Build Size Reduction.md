# Phase 37 — Project Optimization, Asset Cleanup & Build Size Reduction

**Başlangıç:** 8 Ekim 2026  
**Durum:** **GELİŞTİRİLİYOR (In Progress - Adım 1 Aktif)**  
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
- **Durum:** BEKLEMEDE (Pending Approval of Step 2)

### Adım 4: Build Boyutu Artışının Kök Neden Analizi (Root Cause Analysis)
- **Hedef:** 150-170MB olan build boyutunun 250-270MB'a fırlamasının kesin teknik nedeninin tespiti.
- **Kapsam:**
  - Son eklenen assetlerin (özellikle Phase 35 Audio Overhaul ses dosyaları, WAV/uncompressed formatlar, 3D modeller, doku import ayarları, mipmap ayarları, Android ASTC sıkıştırma profilleri) incelenmesi.
  - Hangi dosya veya klasörün kaç MB yer tuttuğunun tam tablosu çıkarılacak.
  - Boyutu şişiren temel faktörler ve çözümleri netleştirilecek.
- **Durum:** BEKLEMEDE (Pending Approval of Step 3)

### Adım 5: CodeReview, Build Boyutu Raporu & Final Optimizasyon (CodeReview & Build Size Report)
- **Hedef:** Projenin baştan sona genel mimari CodeReview'ı ve build boyutu kırılım raporu.
- **Kapsam:**
  - .apk ve .aab çıktı boyutlarının tahmini ve analizi.
  - Proje bileşenlerinin MB dağılımı (Sprite/Texture MB, 3D Models/Meshes MB, Audio MB, Scripts/Assemblies MB, Shaders/Materials MB).
  - Nihai optimizasyon önerileri ve tamamlanma özeti.
- **Durum:** BEKLEMEDE (Pending Approval of Step 4)

---

## 3. Kabul Kriterleri (Acceptance Criteria)

1. [x] **Adım 1:** 3 sahnedeki tüm hiyerarşi taranarak atıl scriptler temizlendi, Unity konsolunda 0 hata.
2. [x] **Adım 2:** Inspector / On-Off referansları korunarak atıl sprite'lar temizlendi.
3. [ ] **Adım 3:** Kullanılmayan eski prefabler tespit edilip güvenli şekilde kaldırıldı.
4. [ ] **Adım 4:** 150MB -> 270MB artışının kök nedeni tam verilerle kanıtlandı ve belgelendi.
5. [ ] **Adım 5:** Kapsamlı CodeReview, .apk / .aab boyutu ve varlık MB dökümü raporlandı.
6. [ ] Her adım kullanıcı onayı alınarak ilerletildi.
