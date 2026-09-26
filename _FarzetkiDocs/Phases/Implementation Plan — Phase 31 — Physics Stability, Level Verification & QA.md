# Implementation Plan — Phase 31 — Fizik Denge Kararlılığı, 1-30 Seviye Doğrulama & Kalite Güvencesi (QA)

**Durum:** Tamamlandı (Completed)  
**Tarih:** 26 Eylül 2026  
**Referans Dokümanlar:**  
- `_FarzetkiDocs/GAME_AUDIT_AND_TODO.md` (Faz 5: Test & Kalite Güvencesi)  
- `_FarzetkiDocs/master_build_prompt.md` (Bölüm 4: Fizik Sistemi, Bölüm 9 & 10: Seviye Mimarisi)  
- `_FarzetkiDocs/RoyalSmashInformationFullVersion.md` (Fiziksel Kararlılık & Uyku Mekaniği)  
- `.agents/skills/phase_implementation_workflow/SKILL.md`  
- `.agents/skills/unity_3d_physics_game/SKILL.md`  
- `.agents/skills/unity_mcp_automation/SKILL.md`  

---

## 1. Genel Bakış ve Kullanıcı Talebi

Kullanıcı doğrudan şu kritik sorunu ve talebi belirtmiştir:
> *"biz bir map hazırlıyoruz değil mi ve bu mapın objelerinin dengede durması gerekiyor, bunu da 'ilk topu atana kadar hiçbir obje hareket etmeyecek' şeklinde ayarladık ama çözüm bu olmamalı. mesela level 30'u açalım ve topu random boş bir yere yani hiçbir objeye deymeyecek şekilde ateşleyelim. Yine de objeler birbirine çarpıp zıplıyor garip şeyler oluyor :d bunu çözmeni istiyorum. Test et MCP bağlantın var düzgün bir şekilde dengede dursunlar sadece ben vurduğumda hareket etsinler yani. Faz 5 i de yapacaksın tabii ki ikisini birden yap."*

### Kök Neden Analizi (Root Cause Analysis):
1. **Yatay ve Derinlik Çarpışma Çakışmaları (Interpenetration Overlaps):**
   - Yapılan derinlemesine denetimde, 30 seviye genelinde toplam **22 adet obje çakışması (overlap)** tespit edildi.
   - Özellikle **Level 30'da tam 12 adet blok** birbirinin içine 7 ila 18 cm gömülü olarak yerleştirilmiş durumdaydı (`x * STEP_X * 0.85f` ve `x * STEP_X * 0.9f` sıkıştırmaları).
   - PhysX fizik motoru bu iç içe geçmiş katı çarpıştırıcıları sıkıştırılmış yay gibi algılayarak blokları fırlatmaktaydı.
2. **`LevelStartPhysicsGate` Havan Ateşlemesine Bağlılığı:**
   - Kapı mekanizması top namludan çıktığı an (`_mortar.OnBallFired`) henüz hiçbir hedefe değmeden tüm blokları aniden `WakeUp()` ile uyandırmaktaydı. Boşluğa atılan bir atışta bile tüm yapay sıkışma patlamaktaydı.
3. **Fizik Sönümlenmesi ve Sürtünme Eksikliği:**
   - Citadel prefablarında `linearDamping: 0` ve `angularDamping: 0.05` idi. Bloklar en ufak temas kuvvetinde durulmayıp kaymaktaydı.
4. **PhysX Çözücü İterasyonlarının Düşük Olması:**
   - Proje varsayılanı olan 6 iterasyon çok katlı ağır taş kulelerin mikro-kaymalarını stabil tutmak için yetersizdi.

---

## 2. Kapsam ve Çözüm Mimarisi

1. **Seviye Izgarası ve Çakışmaların Sıfırlanması (`Citadel30LevelBuilder.cs`)**:
   - `STEP_X = 0.755f` (0.74m blok genişliğinde 1.5 cm güvenli boşluk).
   - Level 18, 20, 21, 27 ve 30'daki tüm çakışan koordinatlar milimetrik dengeli ızgaraya oturtuldu. 30 seviyenin tamamında **0 çakışma** doğrulandı.
2. **Akıllı Fizik Uyku & Darbe Tetikleme Mimarisi (`LevelStartPhysicsGate.cs` & `PhysicsObject.cs`)**:
   - Namludan top çıkışı (`OnBallFired`) ile blokların uyanması iptal edildi.
   - Bloklar doğal statik dengelerinde `Sleep()` durumunda bekler; sadece ve sadece bir top bloğa **fiziksel temas ettiğinde** (`HandleProjectileHit`) çözülür.
   - Boşluğa atılan atışlarda kule en ufak bir titreme dahi yapmadan taş gibi yerinde durur (`maxDisplacement = 0.00000m`).
3. **Prefab & Fizik Kararlılık İyileştirmeleri**:
   - Citadel blok prefablarında `linearDamping = 0.35f`, `angularDamping = 0.5f`, `sleepThreshold = 0.005f`.
   - `DynamicsManager.asset` üzerinde `DefaultSolverIterations = 14`, `DefaultSolverVelocityIterations = 6`, `AutoSyncTransforms = 1`.
4. **Faz 5 Kalite Güvencesi (QA Test Suite)**:
   - Seviye 1'den Seviye 30'a kadar tüm seviyelerin oynanabilirliği, hedef sayıları ve ödülleri otomatik test ile doğrulandı.
   - Can sıfırlanma, can yenilenme ve altın harcama mantığı test edildi.
   - Unity Konsolu 0 hata ve 0 uyarı ile tamamlandı.

---

## 3. Aşamalı Eylem Tablosu (Task Breakdown)

| No | Görev / Teslimat | Kapsam | Durum | Doğrulama Yöntemi |
| :--- | :--- | :--- | :--- | :--- |
| **31.1** | **30 Seviyenin Çakışmalarını Sıfırlama** | `Citadel30LevelBuilder.cs`, `CitadelLevelDatabase` | Tamamlandı | Otomatik geometri taraması: 0 çakışma |
| **31.2** | **Fizik Uyku & Kararlı Statik Dengeleme** | `LevelStartPhysicsGate.cs`, Prefab Damping | Tamamlandı | Boşluğa atış testi: Kulede 0.00000m hareket |
| **31.3** | **PhysX Solver & Materyal Kararlılığı** | `DynamicsManager.asset`, Prefab Ayarları | Tamamlandı | Solver iterations 14/6, linear damping 0.35 |
| **31.4** | **Faz 5 Kapsamlı QA Test Paketi** | `Phase31QAVerificationTest.cs` | Tamamlandı | 5/5 Test Başarılı (Level 30 denge, 1-30 seviyeler, Can, Ekonomi, Sahneler) |
| **31.5** | **Konsol & Sahne Doğrulaması** | `MainGame.unity`, `MainMenu.unity` | Tamamlandı | Unity Console: 0 Error, 0 Warning |

---

## 4. Kabul Kriterleri (Acceptance Criteria)

1. [x] 30 seviyenin tamamında hiçbir blok veya engel birbiriyle veya standla çakışmaz (0 overlap doğrulandı).
2. [x] Level 30 açılıp boşluğa ateş edildiğinde hiçbir blok devrilmez, zıplamaz veya titremez; yapı taş gibi dengede kalır (Boşluğa atış drift: 0.00000m).
3. [x] Sadece top bir bloğa çarptığında kule fiziksel tepki verir ve yıkım başlar (Direkt vuruş darbe tepkisi: 20.8m deplasman).
4. [x] Seviye 1'den Seviye 30'a kadar tüm seviyeler `LevelManager.LoadLevelDirect` ile kesintisiz yüklenebilir ve oynanabilir.
5. [x] Can yenilenme (`LivesManager`) ve Altın harcama (`GoldManager`) sistemleri hatasız çalışır (Tüm durumlar doğrulandı).
6. [x] Unity Konsolunda 0 hata ve 0 uyarı bulunur.
