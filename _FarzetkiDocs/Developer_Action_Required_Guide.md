# Royal Smash! — Geliştirici Aksiyon ve Entegrasyon Rehberi (User Action Required Guide)

> **Önemli:** Bu dokümantasyon, yapay zekânın Unity MCP ve kodlama yetenekleri dışında kalan; doğrudan harici platform hesapları, gerçek ödeme sistemleri, mağaza panelleri ve 3D sanatçı varlıklarının entegrasyonu için geliştiricinin (kullanıcının) manuel olarak yapması gereken tüm adımları listeler.

---

## 1. 3D Model ve Sanat Varlıklarının Entegrasyonu

Projede şu anda tüm 3D nesneler (Kutu, Taş, Sütun, Koni, Boya Şişesi, Havan/Mortar, Stand) fiziksel ve materyal olarak tam uyumlu placeholder mesh'lerle çalışmaktadır. Gerçek 3D modelleri projeye eklemek için:

1. `_FarzetkiDocs/Phases/Prompts_for_3D_Models.md` dosyasında her model için hazır 3D üretim promptları mevcuttur (Mortar, Box, Stone, Column, Cone, Paint Bottle, Stand).
2. Üretilen veya satın alınan `.fbx` / `.obj` 3D modellerini `Assets/_Project/Models/` klasörüne aktarın.
3. İlgili prefabları (`Assets/_Project/Prefabs/Gameplay/`) açarak `MeshFilter` ve `MeshRenderer` bileşenlerindeki placeholder mesh'leri yeni 3D modeller ile değiştirin.
4. Çarpışma ve fizik dengesinin bozulmaması için `BoxCollider`, `CapsuleCollider` veya `MeshCollider (Convex)` ayarlarının orijinal boyutlara (~1x1x1) denk geldiğini doğrulayın.

---

## 2. Google AdMob & Reklam Ağları Entegrasyonu

Proje şu anda `SimulatedAdProvider` ile simüle edilmekte ve tüm reklam akışları (Interstitials her 5 seviyede bir, Rewarded can/altın ödülleri, No-Ads satın alım engeli) %100 test edilmiş durumdadır. Canlı reklamları açmak için:

1. **AdMob Hesabı Açma**: [Google AdMob Console](https://admob.google.com/) üzerinden bir hesap oluşturun ve uygulamayı kaydedin (Android & iOS).
2. **Unity Google Mobile Ads SDK**:
   - Unity için en güncel `GoogleMobileAds.unitypackage` dosyasını indirin ve projeye import edin.
   - `Assets/GoogleMobileAds/Settings` penceresinden **Google AdMob App ID** (Android ve iOS için ayrı ayrı) değerlerini girin.
3. **Ad Unit ID Eşlemesi**:
   - `Assets/_Project/ScriptableObjects/Config/AdConfig.asset` dosyasını açın.
   - `InterstitialAdUnitId_Android` ve `RewardedAdUnitId_Android` alanlarına AdMob'dan aldığınız canlı Unit ID'leri girin.
   - `EnableSimulationMode` kutucuğundaki işareti kaldırın.
4. **AdManager Entegrasyonu**:
   - `AdManager.cs` içerisindeki `IAdProvider` referansını AdMob SDK'sını çağıran prodüksiyon provider'ına bağlayın.

---

## 3. In-App Purchase (IAP) & Google Play Billing / Apple StoreKit

Oyun içi dükkan (`ShopManager.cs` ve `IAPManager.cs`), No-Ads ve altın/skill paketlerini simüle edilmiş olarak güvenle çalıştırmaktadır. Gerçek para ile satışı etkinleştirmek için:

1. **Unity IAP Paketini Doğrulama**:
   - Unity Package Manager üzerinden `In-App Purchasing` paketini etkinleştirin.
2. **Google Play Console / App Store Connect Tanımları**:
   - Aşağıdaki Product ID'leri mağaza konsollarında oluşturun:
     - `com.royalsmash.noads` (Non-Consumable / Kaldırılamaz ürün)
     - `com.royalsmash.gold.tier1` (Consumable / 1,000 Gold)
     - `com.royalsmash.gold.tier2` (Consumable / 2,500 Gold)
     - `com.royalsmash.gold.tier3` (Consumable / 6,000 Gold)
     - `com.royalsmash.skill.starter` (Consumable / 3 Mega, 3 Infinity)
     - `com.royalsmash.bundle.royalkit` (Consumable / Mega Bundle)
3. **Fiyat ve Vergi Yapılandırması**:
   - Mağaza konsolunda fiyatları yerel para birimlerinde belirleyin.
   - `IAPManager.cs` üzerindeki `SimulatePurchases` bayrağını `false` olarak ayarlayın.

---

## 4. Bulut Kaydı & Lider Tablosu (Resmi Unity Gaming Services — UGS)

Projede harici hiçbir 3. parti sunucuya (Firebase, AWS, PlayFab) gerek kalmadan, tamamen Unity'nin resmi bulut servisi olan **Unity Gaming Services (UGS)** entegrasyonu tamamlanmıştır:
- `com.unity.services.core` (Core SDK)
- `com.unity.services.authentication` (Cihaz bazlı şifresiz Anonim Giriş)
- `com.unity.services.cloudsave` (Atomik JSON bulut kaydı)
- `com.unity.services.leaderboards` (Gerçek zamanlı global lider tablosu)

### Kod Mimarisi:
- `UGSCloudProvider.cs`: `ICloudSyncProvider` arayüzünü uygulayarak Unity Gaming Services API'leri ile konuşur.
- `CloudSyncManager.cs`: `_useUGS = true` seçeneği ile otomatik olarak UGS sağlayıcısını çalıştırır. Cihaz çevrimdışıysa veya bağlantı koparsa otomatik olarak yerel önbelleğe (`leaderboard_cache.json`) döner.

### Geliştiricinin Unity Dashboard'da (cloud.unity.com) Yapması Gerekenler:
1. **Authentication**: *Sign-in Methods* sekmesinde **Anonymous** seçeneğinin `Enabled` olduğunu doğrulayın.
2. **Cloud Save**: Cloud Save sekmesinde servisi `Enable` edip kurulum sihirbazını `Finish` ile tamamlayın.
3. **Leaderboards**: *Create Leaderboard* butonuna basarak:
   - **Leaderboard ID:** `global_highest_level`
   - **Sort Order:** `Descending` (Yüksek seviye en üstte)
   kaydını oluşturun.

---

## 5. Android & iOS İmzalanmış Build (Release Signing)

Oyunu cihazlarda veya mağazada test etmek üzere paketlemek için:

1. **Android Keystore Oluşturma**:
   - `Project Settings > Player > Publishing Settings` bölümüne gidin.
   - **Keystore Manager** ile yeni bir `.keystore` dosyası üretin ve şifresini güvenli bir yerde saklayın.
   - **Target API Level**: Android 14 (API Level 34) veya üzeri seçildiğinden emin olun.
2. **iOS Build Ayarları**:
   - `Project Settings > Player > iOS` sekmesinden **Bundle Identifier** (`com.YourStudio.RoyalSmash`) ve **Signing Team ID** değerlerini girin.
   - Xcode üzerinden Archive alarak App Store Connect'e gönderin.
3. **Ekran Yönü Kilidi (Orientation)**:
   - `Project Settings > Player > Resolution and Presentation` altında yönün yalnızca **Portrait** olarak kilitlendiğini doğrulayın.

---

## 6. Mağaza Varlıkları ve Görseller

Mağazaya çıkış için gereken grafik materyalleri:

1. **App Icon (512x512 PNG)**: Havan ve altın taç temalı parlak 3D uygulama ikonu.
2. **Feature Graphic (1024x500 PNG)**: Google Play mağaza başlık görseli.
3. **Ekran Görüntüleri (Screenshots)**:
   - En az 4 adet portrait ekran görüntüsü (Seviye 1 kutular, Seviye 10 Infinity Ball ateşlemesi, Seviye 30 finali, Win Panel kutlaması).
