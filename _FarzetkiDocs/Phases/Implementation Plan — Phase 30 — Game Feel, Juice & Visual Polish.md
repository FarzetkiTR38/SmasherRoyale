# Implementation Plan — Phase 30 — Oynanış Hissi, Cila & Efektler (Game Feel & Juice)

**Durum:** Tamamlandı (Completed)  
**Tarih:** 26 Eylül 2026  
**Referans Dokümanlar:**  
- `_FarzetkiDocs/GAME_AUDIT_AND_TODO.md` (Faz 4: Oynanış Hissi & Cila)  
- `_FarzetkiDocs/master_build_prompt.md` (Bölüm 8: Havan/Nişan Sistemi, Bölüm 22: Görsel Efektler & Cila)  
- `_FarzetkiDocs/RoyalSmashInformationFullVersion.md` (Game Feel, Kamera ve Parçacık Efektleri)  
- `.agents/skills/phase_implementation_workflow/SKILL.md`  
- `.agents/skills/unity_3d_physics_game/SKILL.md`  
- `.agents/skills/unity_mcp_automation/SKILL.md`  

---

## 1. Genel Bakış ve Amaç

Bu faz, oyunun vuruş tatminini, nişan alma konforunu ve fiziksel yıkım hissini zirveye taşımak için **Faz 4 (Game Feel & Juice)** gereksinimlerini hayata geçirir:

1. **Parabolik Görsel Nişan Yayı (`TrajectoryPredictor`)**:
   - Oyuncu ekrana dokunup nişan aldığında veya parmağını sürüklediğinde, topun izleyeceği balistik parabolik yörüngeyi gerçek fizik yerçekimi formülüyle (`ProjectileController.Launch` ile birebir eşleşen) hesaplar ve noktalı/parıltılı bir `LineRenderer` ile çizer.
   - Blok, stand veya engellere çarpma noktasında (`Physics.Linecast`/`SphereCast`) çizgiyi keser ve hedef yüzeyde dinamik bir vuruş halkası/hedef imleci (`HitMarker`) konumlandırır.
   - Atış yapıldığında veya dokunuş sonlandığında akıcı şekilde kaybolur; 0 can veya 0 top durumunda otomatik gizlenir.

2. **Kamera Sarsıntısı ve Dokunsal Tepki (`ScreenShake`)**:
   - `Camera.main` üzerine entegre edilen, sıfır konum sapması (zero-drift garantili) Perlin gürültüsü ve sönümlü sinüs titreşimi üreten modern bir sarsıntı yöneticisi.
   - Standart blok çarpmaları için hafif (`TriggerLight`), yüksek hızlı vuruşlar ve kule devrilmeleri için orta (`TriggerMedium`), Mega Ball vuruşları için güçlü sinematik (`TriggerHeavy`) sarsıntı profilleri.

3. **Gelişmiş Çarpışma ve Zemin Tozu Partikülleri (`VFX Enhancement`)**:
   - Bloklar standdan aşağı düşüp mermer/taş zemine çakıldığında zemin tozu ve şok dalgası partikülü (`SpawnGroundDust`) oluşturur.
   - `PhysicsObject` çarpışma şiddetine göre `VFXManager` ve `ScreenShake` ile tam entegre çalışarak yıkım anını görsel bir şölene dönüştürür.

---

## 2. Sistem Mimarisi & Bileşenler

```
┌────────────────────────────────────────────────────────┐
│                   GAME FEEL & JUICE                    │
├──────────────────────────┬─────────────────────────────┤
│ 1. TRAJECTORY PREDICTOR  │ 2. SCREEN SHAKE SYSTEM      │
│ - Analytical Ballistics  │ - Perlin Noise Decay        │
│ - Obstacle Ray/SphereCast│ - Positional & Roll Shake   │
│ - Dynamic Hit Marker     │ - Zero-Drift Anchor Reset   │
│ - Smooth Fade In/Out     │ - Light / Medium / Heavy    │
├──────────────────────────┴─────────────────────────────┤
│ 3. ENHANCED PARTICLES & DESTRUCTION PHYSICS            │
│ - Ground Crash Dust Puff on Stand Fall-Off             │
│ - MegaBall Impact Kinetic Burst                        │
│ - Dynamic Particle Scaling based on Impulse            │
└────────────────────────────────────────────────────────┘
```

---

## 3. Aşamalı Eylem Tablosu (Task Breakdown)

| No | Görev / Teslimat | Kapsam | Durum | Doğrulama Yöntemi |
| :--- | :--- | :--- | :--- | :--- |
| **30.1** | **Görsel Nişan Yayı (`TrajectoryPredictor.cs`)** | `Assets/_Project/Scripts/Gameplay/` | Tamamlandı | Balistik matematik, obstacle clipping, LineRenderer |
| **30.2** | **Kamera Sarsıntısı Sistemi (`ScreenShake.cs`)** | `Assets/_Project/Scripts/Gameplay/` | Tamamlandı | Zero-drift camera shake, MegaBall & impact triggers |
| **30.3** | **Zemin Tozu & Çarpışma Partikül Entegrasyonu** | `VFXManager.cs`, `PhysicsObject.cs` | Tamamlandı | Düşen blok zemin çarpması, toz dalgası, vuruş cila |
| **30.4** | **Sahne Bağlantıları & Prefab Yapılandırması** | `MainGame.unity`, `MortarController.cs` | Tamamlandı | Unity MCP bileşen kurulumu ve test |
| **30.5** | **Otomasyon Test Paketi & Konsol Denetimi** | `Phase30VerificationTest.cs` | Tamamlandı | 5/5 PASSED, Unity Console: 0 Error, 0 Warning |

---

## 4. Kabul Kriterleri (Acceptance Criteria)

1. [x] `TrajectoryPredictor`, `ProjectileController.Launch` ile aynı kinematik parabolik formülle yörünge çizer ve hedef blok üzerinde sonlanır.
2. [x] Nişan alma başlatıldığında çizgi görünür hale gelir; atış yapıldığında veya parmak kaldırıldığında akıcı şekilde kaybolur.
3. [x] `ScreenShake` kamerayı sarsarken asıl kamera koordinatlarından asla sapma yapmaz (`zero-drift`).
4. [x] Mega Ball vuruşunda güçlü sarsıntı (`TriggerHeavy`), yüksek ivmeli darbelerde ve blok devrilmelerinde orta sarsıntı (`TriggerMedium`) tetiklenir.
5. [x] Standdan zemine düşen bloklar zemin tozu partikülü yayar ve yıkım hissi güçlendirilir.
6. [x] Otomatik test paketi (`Phase30VerificationTest.cs`) tüm testlerden başarıyla geçer.
7. [x] Unity Konsolunda 0 hata ve 0 uyarı bulunur.

---

## 5. Doğrulama ve Tamamlanma Kaydı (Verification Record)

- **Doğrulama Aracı**: `Phase30VerificationTest.RunAllTests` (Unity MenuItem)
- **Sonuç**: 5/5 PASSED, 0 FAILED
  - `[PASS]` Test 1: TrajectoryPredictor Ballistics & Obstacle Clipping (Analitik yerçekimi formülü, engellerde kırpılma, görünürlük kontrolü).
  - `[PASS]` Test 2: InputHandler & TrajectoryPredictor Event Integration (Hedef değişimi dinleyicisi, dinamik yay çizimi).
  - `[PASS]` Test 3: ScreenShake Zero-Drift & Intensity Levels (Perlin sarsıntı sönümlenmesi ve sıfır konum kayması garantisi).
  - `[PASS]` Test 4: VFXManager Ground Dust Particle System (`SpawnGroundDust` zemin çarpma şok dalgası).
  - `[PASS]` Test 5: Scene Health & Component Integrity (`MainGame` ve `MainMenu` sahnelerinde 0 eksik script, `ScreenShake` ve `TrajectoryPredictor` kurulumu).
- **Konsol Sağlığı**: 0 Error, 0 Warning.
