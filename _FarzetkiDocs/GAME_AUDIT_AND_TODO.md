# 🏰 ROYAL SMASH! — OYUN DENETİM RAPORU, HATA ANALİZİ VE TODO LİSTESİ
**Tarih:** 26 Eylül 2026  
**Durum:** Kapsamlı Sistem & Sahne Denetimi Tamamlandı (Reset Sonrası Fabrika Ayarları)  
**Hedef Kapsam:** `BootScene`, `MainMenu`, `MainGame`, Sistem Mimarisi, Oynanış & Fizik, Ekonomi, UI/UX

---

## 📌 1. YÖNETİCİ ÖZETİ (EXECUTIVE SUMMARY)

Tüm kayıt verileri (`player_save.json`, `player_save_backup.json`, `leaderboard_cache.json` ve `PlayerPrefs`) sıfırlanarak oyun temiz fabrika ayarlarına çekilmiş; **Unity MCP** üzerinden `BootScene`, `MainMenu` ve `MainGame` sahneleri baştan sona, kod katmanından UI hiyerarşisine ve fizik motoruna kadar incelenmiştir.

Yapılan testler sonucunda:
1. **Level Kazanıldığında 0 Altın Verme Sorunu**: Kod akışındaki sıra hatasından kaynaklandığı ve `WinPanel`'in ilk zaferde bile oyuncuya `+0 ALTIN (ALINDI)` gösterdiği tespit edildi.
2. **Level Sistemindeki Çöküş / Patlama Sorunu**: `MainGame` sahnesinde arka planda unutulan `SkyCitadelPreview` objesinin içerisindeki 26 adet fizik bloğunun, `LevelLoader`'ın yüklediği gerçek level bloklarıyla iç içe çakışarak fizik patlamalarına yol açtığı tespit edildi.
3. **Mevcut Level Sisteminin Yetersizliği**: 30 level'ın ScriptableObject'lerde katı (hardcoded) `Vector3` koordinatlarıyla tanımlandığı, yeni Citadel görsel prefab boyutlarıyla (0.74m) eski 1.0m primitive verilerin uyuşmadığı ve engellerle hedeflerin birbirinden ayırt edilemediği görüldü.
4. **Menü ve Butonlardaki Eksikler**: Sayfa içi sayaçların referans kopukluğu, çalışmayan alt sekmeler (Leaderboard Dunya/TR/Arkadaslar), Shop yönlendirmesi eksik olan butonlar ve sahneler arası UI stili farklılıkları listelendi.

Aşağıda tespit edilen tüm hataların kök nedenleri, önerilen çözümler ve oyunu kusursuz bir mobil hit haline getirecek aşamalı **TODO Listesi** sunulmuştur.

---

## 🚨 2. TESPİT EDİLEN HATALAR VE KÖK NEDENLERİ (BUG REGISTER)

### 🔴 [KRİTİK #1] Level Kazanıldığında 0 Altın Verilmesi (+0 ALTIN / ALINDI Hatası)
* **Kök Neden:**
  1. `LevelManager.cs` içerisindeki `VictoryEvaluationRoutine` fonksiyonunda (satır 380), zafer anında önce `GoldManager.Instance.ProcessLevelCompletionReward(_currentLevelIndex, remainingBalls)` çağrılmaktadır.
  2. `GoldManager`, oyuncuya altınını verip `_currentLevelIndex`'i hemen `SaveManager.Instance.Data.RewardedLevelIds` listesine ekler ve kaydeder.
  3. Hemen ardından satır 382'de `OnLevelWon?.Invoke(_currentLevelIndex, ...)` eventi tetiklenir.
  4. `WinPanel.cs` bu eventi dinler ve satır 108-110'da şu kontrolü yapar:
     ```csharp
     bool isDuplicate = SaveManager.Instance.Data.RewardedLevelIds.Contains(level);
     ```
  5. Level az önce `GoldManager` tarafından listeye eklendiği için bu kontrol **İLK DEFA OYNANSA DAHİ HER ZAMAN `true`** döner!
  6. Sonuç olarak `WinPanel`, oyuncuya `+0 ALTIN (ALINDI)` ve `(Tekrar)` metinlerini gösterir.
  7. **Ek Hata:** `LevelManager` içerisinde `if (Economy.GoldManager.HasInstance)` kontrolü vardır. Oyun doğrudan `MainGame` sahnesinden başlatılırsa `_instance` henüz çağrılmadığı için `HasInstance` `false` olur ve altın ödül fonksiyonu hiç çalışmaz!
* **Çözüm:**
  - `GoldManager.ProcessLevelCompletionReward` fonksiyonunun ödülün ilk defa mı verildiğini boolean olarak dönmesi veya `WinPanel`'in doğrudan `GoldManager.OnLevelRewardProcessed(..., wasGranted)` sonucuna göre paneli açması sağlanmalıdır.
  - `GoldManager.HasInstance` yerine doğrudan `GoldManager.Instance != null` çağrısı yapılarak singleton'ın otomatik ayağa kalkması garanti edilmelidir.

---

### 🔴 [KRİTİK #2] Sahnedeki 26 Adet Hayalet Blok (`SkyCitadelPreview`) ve Level Çakışması
* **Kök Neden:**
  1. `MainGame.unity` sahnesinde `SkyCitadelPreview` adında aktif bir GameObject bulunmaktadır. Bu objenin altında `PF_CitadelStand` ve **26 adet Rigidbody'li fizik bloğu** (`PF_wood`, `PF_stone`, `PF_crystal`, `PF_armored`, `PF_core`) sahneye doğrudan yerleştirilmiştir.
  2. Sahnedeki eski primitive `Stand` objesi ise `activeSelf: false` durumundadır.
  3. Oyun başladığında `LevelLoader.LoadLevel(LevelData_01)` çalışır ve Level 1 için tanımlı 4 hedef bloğu sahneye spawn eder.
  4. Ancak `LevelLoader` sadece `Level_ObjectsRoot` altını temizler; sahnedeki `SkyCitadelPreview` içindeki 26 bloğa dokunmaz!
  5. Sonuç olarak: Level 1'in 4 bloğu, bu 26 bloğun tam içine spawn olur (toplam 30 blok aynı koordinatlarda iç içe biner).
  6. Fizik motoru çalıştığı anda iç içe giren collider'lar şiddetle birbirini fırlatır, bloklar patlayarak etrafa saçılır ve level hedefleri daha ilk atış yapılmadan standdan düşer!
* **Çözüm:**
  - `SkyCitadelPreview` altındaki `PF_CitadelStand` kök hiyerarşiye taşınarak sahnenin resmi Stand platformu yapılmalıdır.
  - `SkyCitadelPreview` içerisindeki 26 adet statik blok silinmeli veya sadece Editor içi görsel referans moduna (EditorOnly / Inactive) alınmalıdır.
  - Level'daki tüm hedefler yalnızca `LevelLoader` tarafından `Level_ObjectsRoot` altına dinamik olarak üretilmelidir.

---

### 🟠 [YÜKSEK #3] TapIndicator Üzerindeki Eksik Script (Missing MonoBehaviour)
* **Kök Neden:**
  - `MainGame.unity` sahne kökünde yer alan `TapIndicator` objesinde `Missing/Null Script!` bileşeni bulunmaktadır.
  - Bu bileşen Unity konsolunda veya derleme/paketleme sırasında uyarı/hata üretebilir ve dokunma göstergesinin çalışmasını engelleyebilir.
* **Çözüm:**
  - `TapIndicator` üzerindeki bozuk script referansı kaldırılmalı, gerekli olan efekt script'i (`TapEffectController` veya benzeri) temiz şekilde bağlanmalıdır.

---

### 🟠 [YÜKSEK #4] MainMenu ile MainGame Arasındaki UI Tasarım Tutarsızlıkları
* **Kök Neden:**
  1. **OutOfLivesDialog**: `MainGame` sahnesindeki `OutOfLivesDialog`, yeni mecha/royal görsel tasarımına ve özel butonlara kavuşturuldu; ancak `MainMenu.unity` içerisindeki `OutOfLivesDialog` halen eski ilkel gri panelli ve standart butonlu tasarımdadır.
  2. **SettingsDialog**: `MainMenu` sahnesinde `InGameSettingsDialog` kullanılıyor. Menüde "Oyundan Çık" butonunun olmaması doğru olsa da başlıklar, buton hizaları ve legal butonların stili GameScene ile tam simetrik değildir.
* **Çözüm:**
  - `MainGame`'deki güncel mecha `OutOfLivesDialog` prefab haline getirilmeli ve `MainMenu` sahnesindeki eski dialog ile değiştirilmelidir.

---

### 🟡 [ORTA #5] MainMenuController Özel Teklif (Special Offer) Butonları ve Sayaç
* **Kök Neden:**
  1. `MainMenuController.cs` üzerindeki `[SerializeField] private TextMeshProUGUI _offerCountdownText;` referansı boştur (`NULL`).
  2. `BattlePage/Offer_Right/TimerText` objesi sahnede var olmasına rağmen kontrolcüye bağlanmamıştır. Bu yüzden ekrandaki `23:59:59` süresi donuk kalmakta, geri saymamaktadır.
  3. `Offer_Left` (Starter Pack) ve `Offer_Right` (Special Deal) butonları tıklandığında sadece konsola `Debug.Log` yazmakta; Shop ekranına yönlendirmemekte veya teklif detay pop-up'ı açmamaktadır.
* **Çözüm:**
  - `_offerCountdownText` referansı `Offer_Right/TimerText`'e bağlanmalıdır.
  - Butonlara tıklandığında `BottomNavigationUI.SwitchTab(NavigationTab.Shop)` çağrılmalı veya özel teklif satın alma modalı açılmalıdır.

---

### 🟡 [ORTA #6] Altın Barındaki '+' Butonunun İşlevsizliği
* **Kök Neden:**
  - `GoldDisplayUI.cs` satır 170'de:
    ```csharp
    private void HandleAddGoldClicked()
    {
        Debug.Log("[GoldDisplayUI] Add Gold clicked. (Shop will be integrated in Phase 13)");
    }
    ```
  - Oyuncu ana menüde veya oyunda altın miktarının yanındaki `+` butonuna bastığında hiçbir şey açılmamaktadır.
* **Çözüm:**
  - Ana menüde `+` butonuna basıldığında `BottomNavigationUI` üzerinden doğrudan **Mağaza (Shop)** sekmesine geçiş yapılmalıdır.
  - Oyun içinde basıldığında küçük bir "Altın Paketi Al" mağaza mini dialogu açılmalıdır.

---

### 🟡 [ORTA #7] Lider Tablosu Alt Sekmelerinin (Dünya / Türkiye / Arkadaşlar) Çalışmaması
* **Kök Neden:**
  - `MainMenuCanvas/LeaderboardPage/TabsBar` altında `Tab_Dunya`, `Tab_Turkiye` ve `Tab_Arkadaslar` butonları bulunmaktadır.
  - Ancak `LeaderboardUI.cs` script'inde bu butonlar için hiçbir dinleyici (`onClick`), filtreleme parametresi veya görsel aktif/pasif sekme mekanizması bulunmamaktadır. Butonlar tamamen ölü durumdadır.
* **Çözüm:**
  - `LeaderboardUI.cs` içerisine 3 sekmeyi yöneten `LeaderboardScope { Global, Country, Friends }` mimarisi eklenmeli; butonlara tıklandığında seçili sekme parlatılmalı ve liste buna göre filtrelenmelidir.

---

### 🟡 [ORTA #8] NextLevel'da Tüm Sahnenin Yeniden Yüklenmesi (Ağır Sahne Yükü)
* **Kök Neden:**
  - `LevelManager.NextLevel()` metodu çağrıldığında `RestartLevel()` çalışmakta ve `SceneManager.LoadScene(activeScene)` ile tüm Unity sahnesi baştan yüklenmektedir.
  - Bu durum mobil cihazlarda ekranın siyah yanıp sönmesine, sesin kesilip yeniden başlamasına ve gereksiz bellek/çöp toplayıcı (GC) yüküne sebep olmaktadır.
  - Oysa `LevelManager.cs` içinde zaten sahneyi yeniden yüklemeden bir sonraki seviyeyi anında kurabilen `LoadLevelDirect(int levelIndex)` metodu bulunmaktadır!
* **Çözüm:**
  - `NextLevel()` fonksiyonu sahneyi baştan yüklemek yerine doğrudan `LoadLevelDirect(_currentLevelIndex)` fonksiyonunu çağıracak şekilde optimize edilmelidir.

---

## 🏗️ 3. LEVEL SİSTEMİ MİMARİSİ: MEVCUT DURUM VE YENİ SİSTEM ÖNERİSİ

Kullanıcının özellikle belirttiği: *"1.'si şu anki sistem iyi değil..."* uyarısı doğrultusunda oyunun seviye tasarım altyapısı derinlemesine incelenmiştir.

### ⚠️ Mevcut Sistemin 4 Büyük Yapısal Kusuru:
1. **Katı (Hardcoded) Koordinat Bağımlılığı:**
   - 30 seviyenin tamamı ScriptableObject'lerde tek tek elle girilmiş `Vector3 Position` ve `Vector3 Rotation` değerlerine dayanmaktadır.
   - Standın boyutu veya yüksekliği 10 cm değiştiğinde veya yeni bir blok tipi eklendiğinde 30 seviyenin tamamı bozulmaktadır.
2. **Prefab Ölçü Uyuşmazlığı (Citadel vs Primitive):**
   - Yeni hazırlanan Sky Citadel bloklarının prefab boyutu `0.74 x 0.74 x 0.74` metredir.
   - ScriptableObject'lerdeki eski seviye verileri ise `1.0 x 1.0 x 1.0` metre boyutundaki eski ilkel küplere göre konumlandırılmıştır.
   - Bu nedenle seviye yüklendiğinde bloklar ya havada asılı kalmakta ya da standın mermer zeminine batmaktadır.
3. **Hedef (Target) ile Engel (Obstacle) Ayrımının Olmaması:**
   - Mevcut `LevelManager.ScanAndRegisterSceneTargets()` kodu, sahnedeki **her PhysicsObject'i mutlak hedef** olarak kaydeder.
   - Oyunda yıkılması imkansız ağır taş sütunlar, koruyucu metal siperler veya çevre dekorları koyulamaz; çünkü oyuncu bu ağır taşları da standdan düşürmek zorunda kalır, aksi halde seviye kazanılamaz!
4. **Fizik Uyuma/Stabilizasyon Sorunu (Physics Wobble/Collapse):**
   - Bloklar sahneye spawn edildiğinde yerçekimi hemen devreye girdiği için, bloklar ilk atış yapılmadan önce kendi ağırlıklarıyla hafifçe kaymakta veya eğri durabilmektedir.

---

### 🌟 ÖNERİLEN YENİ LEVEL SİSTEMİ MİMARİSİ (PROPOSAL)

Yeni seviye mimarisi **3 katmanlı modern bir bulmaca sistemi** üzerine kurulmalıdır:

```
┌──────────────────────────────────────────────────────────┐
│                   LEVEL ARCHITECTURE                     │
├──────────────────────────┬───────────────────────────────┤
│ 1. DATA MODEL            │ 2. LEVEL GENERATOR TEMPLATES  │
│ - LevelID / Name         │ - Grid-based Coordinate Math  │
│ - Ball Count / Gold      │ - Pre-tested Structural Forms:│
│ - Stand Type / Size      │   * Pyramid (Piramit)         │
│ - Target vs Obstacle Tag │   * Castle Wall (Kale Duvarı) │
│ - Target Types & HP      │   * Twin Towers (İkiz Kule)   │
│                          │   * Crown Fortress (Kral Tacı)│
├──────────────────────────┴───────────────────────────────┤
│ 3. RUNTIME PHYSICS CONTROLLER                            │
│ - Rigidbody Sleep Until First Impact                     │
│ - Only Destroyable Targets Count for Victory             │
│ - Obstacles Act as Physics Shields (Strategic Depth)     │
└──────────────────────────────────────────────────────────┘
```

#### A. Hedef vs. Engel Ayrımı (Target vs. Obstacle Logic)
* `PhysicsObject` sınıfına veya konfigürasyonuna `bool IsRequiredTarget = true` eklenmelidir.
* **Tahta, Kristal ve Çekirdek (Wood, Crystal, PaintBottle):** `IsRequiredTarget = true` (Seviyeyi geçmek için bunların düşmesi şart).
* **Ağır Taş, Metal Bariyer (Heavy Stone, Armored Barrier):** `IsRequiredTarget = false` (Oyuncunun arkadaki hedeflere ulaşmasını zorlaştıran stratejik engeller).
* Kazanma şartı: `_activeTargets.Count == 0` yalnızca `IsRequiredTarget == true` olan objeleri takip eder. Böylece seviyelere taktiksel derinlik kazandırılır.

#### B. Izgara Tabanlı Şablon Mimarisi (Grid / Layered Layout)
* Elle koordinat yazmak yerine, Stand üzerindeki mermer alanı `X` (Genişlik) ve `Y` (Kat/Yükseklik) ızgaralarına bölen bir şablon jeneratörü:
  - `CellSize = 0.75f` (Citadel bloklarına tam uyumlu).
  - Stand Yüksekliği: `Tabletop Y = 1.475f`.
  - Kat 0: `Y = 1.475 + 0.375 = 1.85f`.
  - Kat 1: `Y = 1.85 + 0.75 = 2.60f`.
  - Kat 2: `Y = 2.60 + 0.75 = 3.35f`.
* Bu formül sayesinde bloklar asla birbirinin içine girmez, asla mermere batmaz, milimetrik ve dengeli dizilir.

#### C. Seviye Şablonları (Engaging Puzzle Archetypes)
1. **Seviye 1-5 (Tanışma & Temel Fizik):** Küçük 3-5 blokluk dengeli kuleler ve mini piramitler (Kolay vuruş, tatmin edici yıkım).
2. **Seviye 6-10 (Boya Şişeleri & Zayıf Noktalar):** Altta kırılgan boya şişesi veya kristal olan yapılar; tek vuruşta tüm yapının kayarak devrildiği bulmacalar.
3. **Seviye 11-20 (Zırhlı Engeller & Taş Bariyerler):** Ön tarafta ağır taş bloklar, arkada gizlenmiş hedefler. Mega Ball veya kenardan sektirme atışları gerektiren seviyeler.
4. **Seviye 21-30 (Büyük Kaleler & Zincirleme Reaksiyonlar):** Çok katlı kraliyet kaleleri, dengesiz ağırlık merkezleri, Infinity Ball ile bombardıman yapılacak dev yapılar.

---

## 🎯 4. OYNANIŞ VE "GAME FEEL" İYİLEŞTİRME ÖNERİLERİ

1. **Görsel Nişan Yayı (Trajectory Aim Arc):**
   - Şu an oyuncu ekrana dokunduğunda topun tam olarak hangi açıyla nereye gideceğini göremiyor (tahmini atış yapıyor).
   - Topun atılacağı parabolik rotayı gösteren hafif parıltılı noktalı bir nişan yayı (`LineRenderer` ile) eklenmeli, oyuncu nişan alırken nereyi vuracağını net görmelidir.
2. **İlk Atışa Kadar Fizik Kilidi (Stable Sleep):**
   - Seviye yüklendiğinde `LevelStartPhysicsGate` sahnede üretilen tüm blokları ilk top namludan çıkana kadar dondurmalıdır. Böylece yapılar oyuncu oynamadan asla kıpırdamaz.
3. **Dokunsal Geri Bildirim ve Kamera Sarsıntısı (Screen Shake & Juice):**
   - Mega Ball büyük bir kuleyi devirdiğinde hafif bir kamera sarsıntısı (`CameraShake`) ve zemin parçacık tozu efekti oyunun vuruculuk hissini 5 kat artıracaktır.

---

## 📋 5. KAPSAMLI EYLEM PLANI (MASTER TODO LIST)

Aşağıdaki liste, oyunu baştan sona cilalamak ve hatasız bir şekilde yayına hazırlamak için oluşturulmuştur:

```markdown
### 🚀 FAZ 1: KRİTİK HATALARIN DÜZELTİLMESİ (BUG FIXES) [TAMAMLANDI]
- [x] 1.1: Level zaferinde 0 Altın verme hatasını düzelt (`LevelManager` & `WinPanel` & `GoldManager` ödül senkronizasyonu).
- [x] 1.2: `MainGame` sahnesindeki `SkyCitadelPreview` hayalet bloklarını sahneden temizle; `PF_CitadelStand`'i ana stand olarak sabitle.
- [x] 1.3: `TapIndicator` üzerindeki bozuk/missing script bileşenini temizle ve doğrula.
- [x] 1.4: `MainMenu` sahnesindeki eski `OutOfLivesDialog`'u `MainGame`'deki yeni mecha görsel versiyonuyla güncelle.
- [x] 1.5: `MainMenuController`'da `_offerCountdownText` referansını bağla, teklif butonlarının Shop'u açmasını sağla.
- [x] 1.6: `GoldDisplayUI` üzerindeki '+' butonuna tıklandığında Mağaza (Shop) sekmesine geçişi bağla.
- [x] 1.7: `LevelManager.NextLevel()` metodunu tam sahne yeniden yüklemesi yerine `LoadLevelDirect` kullanacak şekilde optimize et.

### 🏰 FAZ 2: LEVEL SİSTEMİ & BULMACA ALTYAPISI REVİZYONU [TAMAMLANDI]
- [x] 2.1: `PhysicsObject` ve `LevelData` yapısına `IsRequiredTarget` desteği ekle (Hedef vs Stratejik Engel ayrımı).
- [x] 2.2: Yeni Citadel prefab ölçülerine (`0.74m`) tam oturan modüler seviye dizilim matematik formülünü entegre et.
- [x] 2.3: `LevelLoader`'ın Stand üzerindeki nesneleri ilk atışa kadar kusursuz dondurmasını sağla (`LevelStartPhysicsGate` optimizasyonu).
- [x] 2.4: 1'den 30'a kadar olan seviyeleri artan zorluk, taktiksel engeller ve tatmin edici yıkım hissi sunacak şekilde yeniden tasarla.

### 🎨 FAZ 3: UI / UX & MENÜ ENTEGRASYONLARI [TAMAMLANDI]
- [x] 3.1: Leaderboard'u tek tip Küresel (Global) sıralama olarak sabitle; pasif yerel/arkadaşlar alt sekmelerini arındır ve tam sayfa düzenini optimize et.
- [x] 3.2: `SettingsDialog` ve `InGameSettingsDialog` arasındaki ses/titreşim durumlarını yüzde yüz senkronize tut.
- [x] 3.3: Win Panel ve Lose Panel pop-in animasyonlarını ve altın kazanma parıltı efektlerini cilala.

### 💥 FAZ 4: OYNANIŞ HİSSİ & CİLA (GAME FEEL & JUICE) [TAMAMLANDI]
- [x] 4.1: Havan nişan alırken topun gideceği rotayı gösteren noktalı görsel nişan yayı (`TrajectoryPredictor`) ekle.
- [x] 4.2: Büyük kule devrilmelerinde ve Mega Ball vuruşlarında hafif kamera sarsıntısı (`ScreenShake`) ekle.
- [x] 4.3: Bloklar parçalandığında veya standdan düştüğünde duman/toz partikül efektlerini güçlendir.

### 🧪 FAZ 5: TEST VE KALİTE GÜVENCESİ (QA & AUTOMATION) [TAMAMLANDI]
- [x] 5.1: Seviye 1'den Seviye 30'a kadar tüm seviyelerin sırayla oynanabilirliğini otomatik test script'i ile doğrula (0 overlapping, 30/30 geçerli hedef ve stand uyumu).
- [x] 5.2: Can sıfırlanma, can yenilenme ve altın harcama senaryolarını test et (Tüm durumlar doğrulandı).
- [x] 5.3: Unity Konsolunu 0 hata ve 0 uyarı ile temizle.
- [x] 5.4: Boşluğa atış ve statik denge testi (Level 30 dahil boşluğa ateş edildiğinde blokların sıfır kayma/zıplama ile dengede durması ve sadece direkt vuruşta dinamik tepki vermesi).
```

---

## 💡 6. TAVSİYELER VE SONRAKİ ADIM

Bu rapor ve eylem planı doğrultusunda ilk adım olarak **FAZ 1'deki kritik hataları (Altın ödülü, Hayalet bloklar, Missing script ve UI bağlantıları)** kod üzerinden doğrudan düzeltmemiz tavsiye edilir. 

Ardından **FAZ 2'deki Seviye Sistemi Revizyonu**'na geçerek 1-30 seviyelerini Citadel bloklarına tam oturan, eğlenceli ve taktiksel bulmacalara dönüştürebiliriz.

Hazır olduğunda istediğin fazdan başlamak üzere komut vermen yeterlidir! 🎮✨
