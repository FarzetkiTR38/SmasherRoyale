# Royal Smash! — AdMob & Ads SDK Entegrasyon Rehberi 📱📢

Bu rehber, **Phase 14 (Ads Framework)** altyapısına Google Mobile Ads (AdMob) veya Unity Ads gerçek SDK'sını daha sonra bağlamak istediğinde izleyeceğin **adım adım kurulum ve konfigürasyon talimatlarını** içerir.

Şu anda oyunda tüm reklam akışı, sayaçlar, ödül verme güvenliği, No Ads koruması ve iptal mekanizmaları `SimulatedAdProvider` ile **%100 hazır ve çalışır** durumdadır. Canlı reklamları bağlamak için aşağıdaki 5 adımı uygulaman yeterlidir:

---

## 1. Google Mobile Ads Unity Plugin İndirme ve Kurulumu

1. **İndirme**:
   - Resmi Google Mobile Ads Unity SDK releases sayfasına git:
     [Google Mobile Ads Unity Releases](https://github.com/googleads/googleads-mobile-unity/releases)
   - En son stabil sürümün `.unitypackage` dosyasını indir (örneğin: `GoogleMobileAds-v9.x.x.unitypackage`).

2. **Projeye Dahil Etme (Import)**:
   - Unity Editor üst menüsünden: `Assets` → `Import Package` → `Custom Package...` yolunu izle.
   - İndirdiğin `.unitypackage` dosyasını seç ve açılan pencerede tüm bileşenleri (`All`) seçerek `Import` butonuna bas.
   - Play Services Resolver açılırsa `Resolve` işlemini onayla.

---

## 2. AdMob Uygulama Kimliklerini (App ID) Tanımlama

1. Unity Editor üst menüsünden:
   - `Assets` → `Google Mobile Ads` → `Settings...` menüsünü aç.
2. Inspector panelinde:
   - **Google Mobile Ads** altında `Enable Google Mobile Ads` kutucuğunu işaretle.
   - **Android App ID**: AdMob konsolundan aldığın `ca-app-pub-XXXXXXXXXXXXXXXX~YYYYYYYYYY` formatındaki Android uygulama kimliğini yapıştır.
   - **iOS App ID**: AdMob konsolundan aldığın `ca-app-pub-XXXXXXXXXXXXXXXX~ZZZZZZZZZZ` formatındaki iOS uygulama kimliğini yapıştır.

> 💡 **Geliştirme / Test ID'leri**:
> - Android Test App ID: `ca-app-pub-3940256099942544~3347511713`
> - iOS Test App ID: `ca-app-pub-3940256099942544~1458002511`

---

## 3. Reklam Birimi Kimliklerini (Ad Unit IDs) Yapılandırma

Projendeki merkezi reklam ScriptableObject'ini aç:
📁 `Assets/_Project/ScriptableObjects/Config/AdConfig.asset`

Inspector panelinde şu alanları doldur:
1. **Android Reklam Birimleri**:
   - `InterstitialAdUnitIdAndroid`: Geçiş reklamı birim kimliğin (Örnek Test ID: `ca-app-pub-3940256099942544/1033173712`)
   - `RewardedAdUnitIdAndroid`: Ödüllü reklam birim kimliğin (Örnek Test ID: `ca-app-pub-3940256099942544/5224354917`)
2. **iOS Reklam Birimleri**:
   - `InterstitialAdUnitIdIOS`: `ca-app-pub-3940256099942544/4411468910`
   - `RewardedAdUnitIdIOS`: `ca-app-pub-3940256099942544/1712485313`

---

## 4. Simülasyon Modundan Canlı SDK Moduna Geçiş

1. `Assets/_Project/ScriptableObjects/Config/AdConfig.asset` dosyasında:
   - `EnableSimulationMode` onay kutusunun işaretini kaldır (`false`).
2. Kod tarafında `AdManager.cs`:
   - `IAdProvider` arabirimi zaten hazırdır. AdMob SDK'sı kurulduğunda `GoogleMobileAdsProvider` otomatik devreye girer veya `AdManager.Instance.SetProvider(new GoogleMobileAdsProvider())` şeklinde bağlanır.

---

## 5. Canlıya Çıkmadan Önce Dikkat Edilmesi Gereken Güvenlik Kuralları (LOCKED)

1. **Test Cihazı (Test Device ID)**:
   - Kendi telefonunu AdMob konsolunda veya kodda test cihazı olarak kaydetmeden asla kendi reklamlarına tıklama (AdMob hesap askıya alınma riski).
2. **No Ads Koruması**:
   - Kodumuzda `PlayerSaveData.NoAdsPurchased == true` olduğunda geçiş reklamları otomatik olarak engellenmektedir.
   - Ödüllü reklamlar ise No Ads satın alınmış olsa bile çalışmaya devam eder (Section 15.2 kuralı).
3. **Ödül Çiftleme Koruması**:
   - `AdManager.cs` içerisindeki `rewardClaimed` bayrağı sayesinde aynı reklamdan iki kez can yenilenmesi kesin olarak engellenmiştir.
