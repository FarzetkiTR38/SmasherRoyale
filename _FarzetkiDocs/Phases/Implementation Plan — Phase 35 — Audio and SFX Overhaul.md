# Phase 35 — Audio & SFX Overhaul

**Başlangıç:** 4 Ekim 2026  
**Durum:** **TAMAMLANDI (Completed)**  
**Kapsam:** Tüm menü ve oyun içi ses efektlerinin (SFX), arka plan müziklerinin (BGM) baştan sona eksiksiz revizyonu; her ses için ana ve 2 alternatif (`_alt1`, `_alt2`) ses klipi oluşturulması; BootScene'den başlayarak tüm sahnelerde kesintisiz çalışan kalıcı `AudioManager` mimarisi; tüm buton ve panellerin otomatik ses entegrasyonu.

---

## 1. Referans ve Kaynaklar

- `_FarzetkiDocs/master_build_prompt.md`: Section 21 (Audio System), Section 35 (Audio Polish & Anti-clipping).
- `_FarzetkiDocs/RoyalSmashInformationFullVersion.md`: Section 90-95 (Ses efektleri ve miksaj).
- `.agents/skills/phase_implementation_workflow/SKILL.md`: Zorunlu faz dokümantasyon standardı.

---

## 2. Ses Envanteri ve Gereksinimler

Her bir ses için 1 aktif ve 2 alternatif (`_alt1`, `_alt2`) klip sağlanmıştır:

### A. Müzikler (BGM)
1. `BgmMainMenu` (Ana Menü Müziği) + `Alt1` + `Alt2`
2. `BgmGameplay` (Oyun İçi Müziği) + `Alt1` + `Alt2`

### B. UI ve Menü Sesleri
3. `ButtonClick` (Buton Tıklama — yumuşak ve tatmin edici casual pop/click) + `Alt1` + `Alt2`
4. `PanelOpen` (Dialog/Panel Açılış — hafif hava/whoosh geçişi) + `Alt1` + `Alt2`
5. `PanelClose` (Dialog/Panel Kapanış — yumuşak geri çekilme) + `Alt1` + `Alt2`
6. `ShopPurchase` (Mağaza Satın Alma Onayı — altın/kristal başarı tınısı) + `Alt1` + `Alt2`
7. `RewardCoin` (Altın Toplama / Kazanç — parlak tınılı kaskat) + `Alt1` + `Alt2`
8. `StarReward` (Yıldız Kazanma — 1., 2., 3. yıldız chimes) + `Alt1` + `Alt2`

### C. Oynanış ve Top Mekaniği
9. `BallLaunch` (Top Ateşleme — tok mancınık/top patlaması) + `Alt1` + `Alt2`
10. `CannonAim` (Nişan Alma / Gerilme Tensi — yay/mandal sesi) + `Alt1` + `Alt2`
11. `SkillMegaBall` (Mega Top Aktif/Fırlatma — ağır güç patlaması) + `Alt1` + `Alt2`
12. `SkillInfinityBall` (Sonsuz Top Aktif/Fırlatma — kozmik enerjik efekt) + `Alt1` + `Alt2`
13. `BallBounce` (Topun Duvar/Zeminden Sekmesi) + `Alt1` + `Alt2`

### D. Blok Çarpışma ve Kırılma Sesleri (Her Blok Türü İçin Ayrı)
14. `BoxImpact` (Ahşap Kutu Vuruşu) + `Alt1` + `Alt2`
15. `BoxDestroy` (Ahşap Kutu Parçalanması) + `Alt1` + `Alt2`
16. `StoneImpact` (Taş Blok Vuruşu — sert tok darbe) + `Alt1` + `Alt2`
17. `StoneDestroy` (Taş Blok Parçalanması — kaya ufalanması) + `Alt1` + `Alt2`
18. `ColumnImpact` (Sütun Vuruşu — derin rezonans) + `Alt1` + `Alt2`
19. `ColumnDestroy` (Sütun Yıkılması — ağır çöküş) + `Alt1` + `Alt2`
20. `ConeImpact` (Koni/Metal Vuruşu — metalik tınlama) + `Alt1` + `Alt2`
21. `ConeDestroy` (Koni/Metal Parçalanması) + `Alt1` + `Alt2`
22. `PaintBottleBreak` (Boya Kutusu / Core Kırılması — cam kırılması ve sıvı sıçraması) + `Alt1` + `Alt2`
23. `ObjectDestruction` (Genel Obje Parçalanması) + `Alt1` + `Alt2`
24. `ComboDestroy` (Çoklu/Kombo Kırılma — yükselen akor) + `Alt1` + `Alt2`

### E. Oyun Durumu (Game States)
25. `GameStart` (Bölüm Başlangıcı Borusu) + `Alt1` + `Alt2`
26. `WinFanfare` (Zafer Paneli — kraliyet boru/orkestra fanfarı) + `Alt1` + `Alt2`
27. `LoseSound` (Kaybetme Paneli — hafif hüzünlü ve motive edici) + `Alt1` + `Alt2`
28. `NextLevelTransition` (Sonraki Bölüme Geçiş Whoosh) + `Alt1` + `Alt2`

---

## 3. Sistem Mimarisi

- `AudioConfig.cs`: Tüm birincil ve alternatif klipleri, ses ayarlarını, pitch randomizasyonunu ve anti-clipping parametrelerini tutan ScriptableObject.
- `AudioManager.cs`: Singleton, `DontDestroyOnLoad`, çoklu kanal ses havuzu (8-16 voice), dinamik pitch çeşitliliği (doğal his), minimum çarpışma eşiği.
- `UIButtonSound` / `UIButtonJuice`: UI butonları ve panellerinde haptik ve tıklama sesleri.
- `BootController.cs`: `BootScene` başlangıcında `AudioManager`'ı başlatır ve ayarları `SaveManager`'dan yükler.

---

## 4. Teslimatlar & Kabul Kriterleri

1. [x] `AudioConfig.cs` ve `AudioConfig.asset` tüm ses ve alternatif slotları ile genişletildi.
2. [x] Eksik seslerin tümü (ana + 2 alternatif) yüksek kaliteli 16-bit 44.1kHz WAV olarak oluşturuldu ve `Assets/_Project/Audio/` altına eklendi (toplam 99 klip).
3. [x] `AudioManager.cs` yeni ses metotları ve çoklu kanal desteğiyle güncellendi.
4. [x] UI butonları ve panelleri (Shop, Win, Lose, Dialogs, BottomNav) ses yöneticisine bağlandı.
5. [x] BootScene ve diğer sahnelerde kesintisiz müzik ve ses geçişi doğrulandı.
6. [x] Konsolda 0 hata, temiz derleme.
