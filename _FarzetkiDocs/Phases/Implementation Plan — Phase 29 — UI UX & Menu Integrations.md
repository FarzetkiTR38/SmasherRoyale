# Implementation Plan — Phase 29 — UI / UX & Menü Entegrasyonları

**Durum:** Tamamlandı (Completed)  
**Tarih:** 26 Eylül 2026  
**Referans Dokümanlar:**  
- `_FarzetkiDocs/GAME_AUDIT_AND_TODO.md` (Faz 3: UI / UX & Menü Entegrasyonları)  
- `_FarzetkiDocs/master_build_prompt.md` (Bölüm 19: Lider Tablosu, Bölüm 20: Ayarlar Modalı, Bölüm 10: Zafer/Yenilgi Panelleri)  
- `_FarzetkiDocs/RoyalSmashInformationFullVersion.md` (UI Hiyerarşisi, Animasyonlar ve Ses/Görsel Cila)  
- `.agents/skills/phase_implementation_workflow/SKILL.md`  
- `.agents/skills/mobile_ui_ux/SKILL.md`  
- `.agents/skills/unity_mcp_automation/SKILL.md`  

---

## 1. Genel Bakış ve Amaç

Kullanıcının talimatları ve denetim raporu doğrultusunda Faz 3 kapsamındaki arayüz sistemleri optimize edilir:

1. **Tek Tip Küresel (Global) Lider Tablosu**:
   - Kullanıcının doğrudan talimatı: *"artık tek çeşit leaderboard o da global :) yerel ve friend olayını kaldırdım sadece globali gösteriyoruz"*.
   - `MainMenu.unity` sahnesindeki pasif `TabsBar` kaldırılır/düzenlenir; `LeaderboardUI` tam ekran küresel sıralama moduna kilitlenir. Başlık ve durum rozetleri küresel senkronizasyona ayarlanır.
   - İlk 3 sıra (`Rank 1-3`) özel taçlı/parıltılı kartlarla, diğer sıralar ve alt yapışkan oyuncu satırı (`StickyPlayerRow`) kesintisiz listelenir.

2. **`SettingsDialog` & `InGameSettingsDialog` Çift Yönlü Durum Senkronizasyonu**:
   - `InGameSettingsDialog.cs` içerisine `AudioManager.Instance.OnMusicStateChanged`, `OnSfxStateChanged` ve `VibrationManager.Instance.OnVibrationStateChanged` olay dinleyicileri entegre edilir.
   - Menüde veya oyun içinde yapılan herhangi bir ses/müzik/titreşim değişikliği anında her iki dialoga ve kalıcı kayda (`SaveManager`) %100 senkronize yansır.

3. **Win Panel & Lose Panel Pop-In ve Altın Parıltı Efekti Cilalaması**:
   - `WinPanel.cs` ve `LosePanel.cs` pencerelerine elastik sıçrama (overshoot spring pop-in) animasyonu eklenir.
   - `WinPanel` zafer anında kazanılan altını sıfırdan hedef miktara doğru sesli ve akıcı bir sayaç animasyonu (`CountUpGoldRoutine`) ile sayar; altın metni hafifçe nabız gibi büyüyüp küçülür (`PunchScale`).

---

## 2. Aşamalı Eylem Tablosu (Task Breakdown)

| No | Görev / Teslimat | Kapsam | Durum | Doğrulama Yöntemi |
| :--- | :--- | :--- | :--- | :--- |
| **29.1** | **Küresel Lider Tablosu Konsolidasyonu** | `MainMenu.unity`, `LeaderboardUI.cs` | Tamamlandı | Tek tip Global liste, temiz UI hiyerarşisi |
| **29.2** | **Ayarlar Modalı Ses/Titreşim Tam Senkronizasyonu** | `InGameSettingsDialog.cs`, `AudioManager.cs`, `VibrationManager.cs` | Tamamlandı | State event subscription & çift yönlü sync |
| **29.3** | **Win/Lose Pop-In ve Altın Sayaç/Parıltı Animasyonu** | `WinPanel.cs`, `LosePanel.cs` | Tamamlandı | Elastik pop-in ve animasyonlu altın sayacı |
| **29.4** | **Otomasyon Testi & Konsol Doğrulaması** | `Phase29VerificationTest.cs` | Tamamlandı | Testlerin tümü PASSED (5/5), Konsol: 0 hata |

---

## 3. Kabul Kriterleri (Acceptance Criteria)

1. [x] `LeaderboardUI` yalnızca Küresel (Global) liderlik tablosunu gösterir; çalışmayan yerel/arkadaşlar alt sekmeleri tamamen arındırılmıştır.
2. [x] `InGameSettingsDialog` açıkken veya kapalıyken ses/titreşim ayarları değiştirildiğinde `AudioManager` ve `VibrationManager` durumları senkronize güncellenir.
3. [x] `WinPanel` açılışında kart elastik yay animasyonuyla ekrana girer ve altın ödülü dinamik sayaçla sayar.
4. [x] `LosePanel` açılışında kart akıcı yay animasyonuyla ekrana girer.
5. [x] Otomasyon test paketi (`Phase29VerificationTest.cs`) tüm testlerden başarıyla geçer.
6. [x] Unity Konsolunda 0 hata ve 0 uyarı bulunur.

---

## 4. Doğrulama ve Tamamlanma Kaydı (Verification Record)

- **Doğrulama Aracı**: `Phase29VerificationTest.RunAllTests` (Unity MenuItem)
- **Sonuç**: 5/5 PASSED, 0 FAILED
  - `[PASS]` Test 1: Global-Only Leaderboard (`TabsBar` kaldırıldı, tek tip global sıralama, `_rank1Prefab` - `_rank3Prefab` bağlandı).
  - `[PASS]` Test 2: Settings Dialog Synchronization (`InGameSettingsDialog` ses, müzik ve titreşim için event-driven çift yönlü senkronizasyon).
  - `[PASS]` Test 3: WinPanel Spring Pop-In & Dynamic Gold Counter (`PopInRoutine` elastik sıçrama & `CountUpGoldRoutine` sayacı).
  - `[PASS]` Test 4: LosePanel Spring Pop-In (`PopInRoutine` elastik sıçrama).
  - `[PASS]` Test 5: Scene Health (`MainMenu` ve `MainGame` sahnelerinde 0 eksik script bileşeni).
- **Konsol Sağlığı**: 0 Error, 0 Warning.
