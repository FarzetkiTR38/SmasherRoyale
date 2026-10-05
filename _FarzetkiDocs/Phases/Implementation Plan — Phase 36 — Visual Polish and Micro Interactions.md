# Phase 36 — Visual Polish and Micro-Interactions

**Başlangıç:** 4 Ekim 2026  
**Durum:** **TAMAMLANDI (Completed)**  
**Kapsam:** Oyunun görsel kalitesini AAA casual mobil standartlarına taşıyacak mikro etkileşimlerin, buton basış geri bildirimlerinin, panel açılış/kapanış animasyonlarının ve dikkat çekici görsel efektlerin hayata geçirilmesi.

---

## 1. Referans ve Kaynaklar

- `_FarzetkiDocs/master_build_prompt.md`: Section 17 (UI Architecture), Section 23 (Haptic & Micro-interactions), Section 35 (Polish).
- `_FarzetkiDocs/RoyalSmashInformationFullVersion.md`: Section 344 (UI/UX Acceptance Criteria).
- `.agents/skills/phase_implementation_workflow/SKILL.md`: Zorunlu faz dokümantasyon standardı.

---

## 2. Planlanan Görsel & Etkileşim İyileştirmeleri

### A. Taktil Buton Geri Bildirimi (`UIButtonJuice`)
1. **Sol Tık Basılı Tutulduğunda:** Buton anında ve yumuşak şekilde %15 küçülerek 0.85x ölçeğe iner (`transform.localScale = 0.85f`).
2. **İmleç Buton Dışına Çıktığında (`OnPointerExit`):** Yumuşak şekilde orijinal 1.0x ölçeğine döner.
3. **Bırakıldığında (`OnPointerUp`):** Yaylanarak hafifçe büyür (1.05x punch) ve orijinal 1.0x ölçeğine oturur.
4. **Tüm Butonlara Otomatik Uygulama:** Sahne veya prefab fark etmeksizin tüm butonların bu taktil hissi kazanması.

### B. Ana Menü Play (Savaş) Butonu Nabız / Işıma Efekti (`UIButtonPulseEffect`)
1. **Nefes Alma (Breathing Pulse):** Buton sürekli olarak nazik bir sinüs dalgasıyla (1.0x - 1.045x) nefes alıp vererek oyuncuyu oyuna girmeye davet eder.
2. **Etkileşim Uyumu:** Tıklandığında nefes alma durup anında 0.85x basış ölçeğine geçer; bırakıldığında doğal olarak devam eder.
3. **Görsel Işıma (Sheen/Shine):** Maske ve ışıma ile periyodik hafif parlama geçişi.

### C. Yumuşak Panel & Dialog Geçişleri (`UIPanelAnimator`)
1. **Ani Açılmayı Önleme (Pop-In):** Dialoglar birdenbire belirmek yerine 0.72x ölçekten ve şeffaflıktan (Alpha 0 -> 1) hafif yaylanarak (1.05x -> 1.0x) açılır.
2. **Yumuşak Kapanış (Pop-Out):** Kapanırken 1.0x -> 0.85x ölçeğe inip şeffaflaşarak kaybolur.
3. **TimeScale Bağımsızlığı:** Oyun duraklatıldığında (`Time.timeScale == 0`) bile `Time.unscaledDeltaTime` ile pürüzsüz çalışır.
4. **Entegre Edilen Paneller:** `InGameSettingsDialog`, `ExitConfirmDialog`, `OutOfLivesDialog`, `ProfileDialog`, `PurchaseFeedbackDialog`, `WinPanel`, `LosePanel`.

### D. Canlı HUD & Altın Punch Efekti
1. Altın kazanıldığında veya harcandığında metin etiketi ve altın simgesi yaylanarak parlar (`1.0x -> 1.2x -> 1.0x`).

---

## 3. Sistem Mimarisi

- `Assets/_Project/Scripts/UI/UIButtonJuice.cs`: 0.85x basış ölçeği, `IPointerExitHandler` desteği ve yaylanma animasyonu.
- `Assets/_Project/Scripts/UI/UIButtonPulseEffect.cs`: Play butonuna özel organik nefes alma ve ışıma bileşeni.
- `Assets/_Project/Scripts/UI/UIPanelAnimator.cs`: CanvasGroup ve scale tabanlı genel dialog açılış/kapanış canlandırıcısı.
- `Assets/_Project/Scripts/UI/UIJuiceAutoAttacher.cs`: Menülerde ve prefablerde eksik butonları bularak taktil etkileşimi garantiye alan editor/runtime sistemi.

---

## 4. Teslimatlar & Kabul Kriterleri

1. [x] `UIButtonJuice.cs` 0.85x basış ve pointer-exit koruması ile revize edildi.
2. [x] `UIButtonPulseEffect.cs` yazılarak MainMenu Play/Savaş butonuna entegre edildi.
3. [x] `UIPanelAnimator.cs` oluşturuldu ve tüm ana popup/diyaloglara bağlandı.
4. [x] Tüm butonların ve panellerin tepkiselliği doğrulandı.
5. [x] Unity MCP ile görsel kontrol ve ekran görüntüleri alındı.
6. [x] Konsolda 0 hata, temiz derleme.
