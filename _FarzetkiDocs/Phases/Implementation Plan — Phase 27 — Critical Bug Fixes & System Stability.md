# Implementation Plan — Phase 27 — Critical Bug Fixes & System Stability

**Durum:** Tamamlandı (Completed — 8/8 Test PASSED)  
**Tarih:** 26 Eylül 2026  
**Referans Dokümanlar:**  
- `_FarzetkiDocs/GAME_AUDIT_AND_TODO.md` (Faz 1: Kritik Hata Düzeltmeleri)  
- `_FarzetkiDocs/master_build_prompt.md` (Bölüm 1.4, 9, 10, 13, 14, 17)  
- `_FarzetkiDocs/RoyalSmashInformationFullVersion.md` (Ekonomi, Seviye Akışı, UI/UX)  
- `.agents/skills/phase_implementation_workflow/SKILL.md`  
- `.agents/skills/unity_mcp_automation/SKILL.md`  

---

## 1. Genel Bakış ve Amaç

Sistem denetiminde tespit edilen kritik oynanış ve ekonomi blokajları tamamen çözülmüştür:
1. **Level Zaferi Altın Ödülü Senkronizasyonu**: `LevelManager`, `GoldManager` ve `WinPanel` arasındaki sıra ve referans hatası düzeltildi; zafer anında ilk defa oynanan seviyede ödül cüzdana eklenip `WinPanel`'de `+{totalGold} ALTIN` olarak altın parıltısıyla gösterilmesi sağlandı. Tekrar oynanışlarda ise `+0 ALTIN (ALINDI)` şeklinde güvenli duplicate engellemesi korundu.
2. **MainGame Hayalet Bloklarının Temizliği**: `MainGame.unity` sahnesinde aktif bırakılmış 26 adet statik `SkyCitadelPreview` bloğu ve eski pasif Stand tamamen temizlendi; `PF_CitadelStand` sahne köküne taşınarak resmi `Stand` platformu yapıldı.
3. **Seviye Veritabanı SkyCitadel Entegrasyonu**: `LevelDatabase.asset` içerisindeki 30 seviyenin tamamı, eski primitive 1.0m küp verilerinden yeni kalibre edilmiş `LevelData_Citadel01`..`30` asset'lerine yönlendirildi.
4. **TapIndicator Eksik Script Temizliği**: `MainGame.unity` sahnesindeki `TapIndicator` objesi üzerindeki kayıp script referansı (`GameObjectUtility.RemoveMonoBehavioursWithMissingScript`) ile temizlendi.
5. **MainMenu OutOfLivesDialog Modernizasyonu**: `MainGame`'deki mecha/royal tasarımlı `OutOfLivesDialog`, `Assets/_Project/Prefabs/UI/Dialogs/PF_OutOfLivesDialog.prefab` olarak kaydedildi ve `MainMenu.unity` sahnesindeki eski dialogun yerine yerleştirilerek kontrolcülere bağlandı.
6. **MainMenu Buton ve Sayaç Bağlantıları**: `MainMenuController._offerCountdownText` referansı `PagesRoot/BattlePage/Offer_Right/TimerText`'e bağlandı. Özel teklif butonları (`Offer_Left`, `Offer_Right`) ve altın barındaki `+` butonu doğrudan Mağaza (Shop) sekmesine geçiş yapacak şekilde bağlandı.
7. **NextLevel Sahne Yükleme Optimizasyonu**: `LevelManager.NextLevel()` ve `RestartLevel()` metodları tüm sahneyi baştan yükleyen (`SceneManager.LoadScene`) ağır yapıdan kurtarılarak, sahnede anında seviyeyi kuran `LoadLevelDirect` metoduna bağlandı.

---

## 2. Aşamalı Eylem Tablosu (Task Breakdown)

| No | Görev / Teslimat | Kapsam | Durum | Kanıt |
| :--- | :--- | :--- | :--- | :--- |
| **27.1** | **Level Zafer Altın Ödülü Senkronizasyonu** | `GoldManager.cs`, `LevelManager.cs`, `WinPanel.cs` | Tamamlandı | Test 4 & Test 5 PASSED |
| **27.2** | **MainGame Sahne Hiyerarşisi & Stand Temizliği** | `MainGame.unity` (`Stand`, `SkyCitadelPreview`) | Tamamlandı | Test 2 PASSED; `screenshot-20260926-200106.png` |
| **27.3** | **TapIndicator Missing Script Düzeltmesi** | `MainGame.unity` (`TapIndicator`) | Tamamlandı | Missing scripts: 0 |
| **27.4** | **MainMenu OutOfLivesDialog UI Eşitlemesi** | `MainMenu.unity`, `PF_OutOfLivesDialog.prefab` | Tamamlandı | Test 8 PASSED |
| **27.5** | **MainMenuController Teklif Sayacı & Shop Yönlendirmeleri** | `MainMenuController.cs`, `GoldDisplayUI.cs` | Tamamlandı | Test 7 PASSED; 0 null fields |
| **27.6** | **NextLevel Hafif Seviye Geçiş Optimizasyonu** | `LevelManager.cs` (`NextLevel`, `RestartLevel`) | Tamamlandı | Direct Level Loading active |
| **27.7** | **LevelDatabase Citadel Seviyeleri Güncellemesi** | `LevelDatabase.asset` (30 seviye) | Tamamlandı | Test 1 PASSED |
| **27.8** | **Play Mode Otomasyon Testi & Konsol Doğrulaması** | `Phase27VerificationTest.cs` | Tamamlandı | 8/8 PASSED, Konsol: 0 hata |

---

## 3. Kabul Kriterleri Doğrulama Sonuçları (Verification Results)

1. [x] **Level 1 Zaferi Altın Ödülü**: İlk zaferde temel ödül (50) + kalan top bonusu oyuncu bakiyesine eklenir ve `WinPanel`'de `+{totalGold} ALTIN` gösterilir.
2. [x] **Tekrar Oynanış Güvenliği**: Aynı seviye tekrar kazanıldığında bakiye değişmez ve `+0 ALTIN (ALINDI)` gösterilir.
3. [x] **Hayalet Blokların Temizliği**: `SkyCitadelPreview` silindi; `Stand` tek ve kök objedir; level hedefleri sadece `LevelLoader` ile dinamik yüklenir.
4. [x] **30 Seviye Citadel Uyumu**: `LevelDatabase.asset` 30 seviyenin tamamında `LevelData_Citadel01`..`30` kullanır.
5. [x] **0 Missing Script**: Sahnelerde hiçbir kayıp/bozuk MonoBehaviour referansı bulunmamaktadır.
6. [x] **MainMenu Dialog Paritesi**: `MainMenu.unity` mecha tasarımlı `PF_OutOfLivesDialog` kullanır.
7. [x] **Shop Buton Yönlendirmeleri**: Altın barındaki `+` butonu ve özel teklif butonları tıklandığında Mağaza sekmesini açar.
8. [x] **Teklif Sayacı**: `TimerText` referansı bağlıdır ve canlı geri sayım aktiftir.
9. [x] **Hafif Seviye Geçişi**: `NextLevel()` sahneyi baştan yüklemeden `LoadLevelDirect` ile kesintisiz sonraki seviyeye geçer.
10. [x] **Unity Konsol Sağlığı**: Derleme ve çalışma zamanında 0 hata ve 0 uyarı.

---

## 4. Tamamlama Kaydı (Completion Record)

- **Test Sonucu:** `Phase 27 Test Results: 8/8 PASSED, 0 FAILED.`
- **Konsol:** 0 Hata, 0 Uyarı.
- **Ekran Görüntüsü:** `Assets/Screenshots/screenshot-20260926-200106.png` (Temizlenmiş mermer stand ve oyun ekranı).
