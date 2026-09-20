# Royal Smash! — GameScene Visual Design & Background Specification
> **Belge Amacı:** Hem yapay zeka görsel araçlarına (Midjourney, DALL-E, Stable Diffusion, Photoshop Generative Fill) hem de 2D/3D konsept sanatçılarına ve grafik tasarımcılara GameScene arka planının nasıl olması gerektiğini aktaran teknik şartname ve hazır prompt rehberidir.

---

## 1. Oyunun Sahne Mimarisi ve Mantığı (Sistem Nasıl Çalışıyor?)

Royal Smash, **Hibrit 3D Fizik Bulmaca** oyunudur (Mobil Dikey - 9:16 Portrait).

Ekrandaki görüntü tek bir 2D çizimden ibaret **değildir**. İki katmandan oluşur:

```
+-------------------------------------------------------------+  100% (Y)
| [UI / HUD BÖLGESİ] Can, Altın, Seviye, Ayarlar Butonları     |  Top 15%
|                                                             |
| [2D GÖRSEL BÖLGE - SADECE ARKA PLAN]                        |
| - Kraliyet şato kuleleri, surlar, bayraklar                 |  Üst 50%
| - Gökyüzü, hafif bulutlar, dağ silüeti                      |  (Saf Görsel /
| - Bu bölgede fizik veya etkileşim YOKTUR.                   |   Matte Painting)
+==================== Ufuk Çizgisi (~50%) ====================+  50% (Y)
| [GERÇEK 3D FİZİK OYUN ALANI - ETKİLEŞİMLİ DÜNYA]             |
|                                                             |
|          [3D Ahşap/Taş Sehpa (Stand)]                       |
|          [3D Kırılabilir Kutular, Taşlar, Boya Şişeleri]     |  Alt 50%
|                                                             |  (Unity 3D
|                                                             |   Fizik Alanı)
|                    [3D Havan Topu / Mortar]                 |
|                                                             |
+-------------------------------------------------------------+  0% (Y)
```

### 🔴 En Büyük Hata Neden Oluşuyor?
Tasarımcılar veya Yapay Zeka (AI), "Bize bir oyun sahnesi çiz" dediğinizde sahneyi tek parça sanıp **alt kısma sehpa, masalar, kutular, hedefler veya top (cannon/mortar)** çiziyor.
* **Sonuç:** Oyuna koyduğunuzda bizim Unity'de kodla spawn ettiğimiz gerçek 3D havan topu ve 3D kutular, görseldeki 2D kutuların ve sehpaların üzerine biniyor; çifte görüntü ve berbat bir perspektif karmaşası oluşuyor.

### 🟢 Olması Gereken Sistem:
1. **Üst %50 (Görsel Katman):** Zengin, masalsı, renkli bir kraliyet şatosu, surlar ve gökyüzü.
2. **Alt %50 (Zemin Katmanı):** Üzerinde **HİÇBİR EŞYA, MASA, KUTU VEYA SİLAH OLMAYAN**, tamamen boş, temiz, hafif dokulu bir avlu zemini (taş zemin / mermer parke / saray bahçesi çimi).
3. **Neden?** Çünkü o boş zeminin üzerine Unity motoru kendi **3D Stand (Sehpa)** modelini, **3D Hedef Kutularını** ve oyuncunun ateşlediği **3D Havan Topunu** dinamik olarak yerleştirir.

---

## 2. Teknik Özellikler (Technical Specifications)

| Parametre | Değer | Açıklama |
| :--- | :--- | :--- |
| **En/Boy Oranı (Aspect Ratio)** | `9:16` (Dikey / Portrait) | Mobil cihazlar için tam ekran dikey |
| **Çözünürlük** | `1080 x 1920 px` veya `1440 x 2560 px` | 72 DPI (Mobil) veya 300 DPI (Master PSD) |
| **Kamera Açısı** | Hafif Üstten Bakış (`~8° - 10° Downward Tilt`) | Ufuk çizgisi tam ortada (~%48-52 yükseklikte) |
| **Sanat Tarzı (Art Style)** | Casual / Stylized 3D Render Look | *Royal Match*, *Clash Royale*, *Brawl Stars* kalitesinde canlı, pürüzsüz, sıcak ışıklandırma |
| **Işık Yönü** | Sağ üstten yumuşak güneş ışığı (`Warm Sun, 48° pitch`) | Sahnedeki 3D gölgelerle uyumlu olmalı |
| **Alt Bölge Kontrastı** | Düşük / Orta Kontrast | 3D renkli kutuların ve topların zeminde net okunabilmesi için zemin göz yormamalı |

---

## 3. Kesin Yasaklar (Strict Negative Rules)

Görseli tasarlayacak kişiye veya AI'a iletilecek **ASLA OLMAMASI GEREKENLER** listesi:

* ❌ **ALT KISMA HİÇBİR MASA, SEHPA, PLATFORM VEYA KAİDE ÇİZMEYİN.** (Unity 3D sehpa koyacak)
* ❌ **KUTU, HEDEF, ŞİŞE, TAŞ, SANDIK, ENGEL KOYMAYIN.** (Bunlar fizik motoruyla üretiliyor)
* ❌ **TOP, SAPAN, MANCINIK, MERMİ, ATEŞLEME MEKANİZMASI KOYMAYIN.** (Alt ortada bizim 3D topumuz duruyor)
* ❌ **KULLANICI ARAYÜZÜ (UI), CAN, ALTIN, BUTON, YAZI, ÇERÇEVE EKLEMEYİN.**
* ❌ **KARAKTER VEYA DÜŞMAN KOYMAYIN.**

---

## 4. Yapay Zeka (AI) İçin Hazır Kopyala-Yapıştır Promptlar

### 🚀 A) Midjourney (v6.0) Promptu
Midjourney'e doğrudan yapıştırabilirsiniz:

```text
mobile casual game background, vertical portrait 9:16, split screen composition, vibrant fantasy royal castle courtyard, stylized 3D render art style in the aesthetic of Royal Match and Clash Royale. 
UPPER HALF: majestic royal castle battlements, grand stone towers with colorful blue and gold flags, warm sunny sky with soft stylized fluffy clouds, distant green rolling hills, bright fairytale kingdom atmosphere. 
LOWER HALF: completely empty flat stone courtyard floor, seamless clean cobblestone paving, spacious vacant castle ground, no props, no clutter, camera tilted downward 8 degrees, warm cinematic lighting, soft ambient occlusion, unreal engine 5 render, clean composition --ar 9:16 --v 6.0 --stylize 250 --no cannon, mortar, weapon, table, stand, pedestal, boxes, crates, barrels, targets, enemies, characters, UI, buttons, text, HUD, frame, border
```

---

### 🎨 B) ChatGPT / DALL-E 3 Promptu
ChatGPT'ye (DALL-E 3) doğrudan şu metni verin:

```text
Create a high-quality vertical 9:16 mobile game background illustration with a stylized 3D casual art style (similar to Royal Match and Clash Royale).

COMPOSITION RULES (CRITICAL):
1. PERSPECTIVE & HORIZON: The horizon line is exactly in the middle of the screen (50% height), with an 8-degree downward camera tilt.
2. UPPER HALF (50% to 100% vertical height): Beautiful royal kingdom environment. Majestic castle towers, battlements, royal blue and gold pennant flags, sunny blue sky with soft volumetric clouds, distant lush hills. The very top 15% should be simple and clean sky so mobile UI icons can be placed over it.
3. LOWER HALF (0% to 50% vertical height): A completely EMPTY, flat castle courtyard stone floor (or manicured royal courtyard pavers). It must have clean textures, soft warm sunlight shadows, and ZERO objects.

STRICT NEGATIVE CONSTRAINTS:
- Do NOT draw any cannons, mortars, slingshots, or weapons anywhere.
- Do NOT draw any pedestals, stands, tables, platforms, or podiums.
- Do NOT draw any boxes, crates, barrels, bricks, or targets.
- Do NOT draw any characters, soldiers, or monsters.
- Do NOT draw any UI, icons, text, health bars, or screen borders.
The ground in the lower half must remain completely open and vacant for dynamic 3D physics gameplay.
```

---

### 🛠️ C) Mevcut Görselleri Düzeltmek İçin Prompt (Photoshop Generative Fill / Inpainting)
Eğer elinizde beğendiğiniz bir şato arka planı varsa ama altında masa, kutular veya top çizilmişse:

1. Görseli Photoshop'ta açın.
2. Seçim aracıyla (Lasso veya Marquee) **ekranın alt %50'lik alanını (masaların ve kutuların olduğu tüm zemini)** seçin.
3. **Generative Fill (Yapay Zeka ile Doldur)** kutusuna şu promptu yazıp Enter'a basın:

```text
seamless clean empty stone castle courtyard ground, flat cobblestone courtyard floor, vacant royal garden paving, warm soft lighting, no tables, no stands, no boxes, no weapons, no characters, completely empty space
```

---

## 5. Tasarımcıya Gönderilecek Mesaj Taslağı (Türkçe)

Tasarımcınıza WhatsApp, Slack veya e-posta ile doğrudan gönderebileceğiniz açıklama:

```markdown
Selam,

Royal Smash oyunumuz için GameScene (oyun içi) dikey arka plan görseline ihtiyacımız var. 
Eklediğim şartnameye göre hazırlayabilir misin?

Önemli Sistem Detayı:
Oyunumuz 3D fizik tabanlı bir mobil bulmaca oyunudur. Bu yüzden ekran iki parçalı çalışmaktadır:

1. ÜST YARI (%50 - %100):
   - Tamamen görsel atmosferdir.
   - Sevimli, masalsı, renkli bir kraliyet şatosu, surlar, kuleler, bayraklar ve güneşli gökyüzü içermelidir (Royal Match / Clash Royale tarzında pürüzsüz 3D render hissi).
   - En üst %15'lik alanda can, altın ve seviye butonları duracağı için çok karmaşık detaylar olmamalıdır.

2. ALT YARI (%0 - %50):
   - BURASI ÇOK KRİTİK: Bu alan gerçek 3D oyun alanımızdır.
   - Bu zeminin üzerine bizim Unity 3D motorumuz kendi ahşap sehpasını, kırılacak 3D kutularını ve en alta da oyuncunun ateşleyeceği 3D havan topunu kodla yerleştirmektedir.
   - Bu yüzden görselin alt yarısı KESİNLİKLE BOŞ OLMALIDIR. Hiçbir masa, sehpa, kutu, mermi, silah veya engel çizilmemelidir. Sadece temiz, şık bir taş avlu/saray bahçesi zemin dokusu olmalıdır.

Format Detayları:
- Boyut: 9:16 Dikey (1080x1920 veya 1440x2560 px).
- Kamera Açısı: ~8° hafif yukarıdan bakış, ufuk çizgisi tam ortada.
- Tercihen PSD katmanlı (Arka plan şato kulesi ayrı katman, zemin ayrı katman, gökyüzü ayrı katman).
```

---

## 6. Brief for Foreign / Freelance Artist (English)

If working with an artist on Fiverr, Upwork, or ArtStation:

```markdown
Hi,

We need a vertical background environment (matte painting) for our mobile casual physics puzzle game "Royal Smash" (similar to Royal Match / Clash Royale).

### 📐 Technical Specs:
- **Aspect Ratio:** 9:16 Portrait (1080x1920 or 1440x2560 px).
- **Camera Perspective:** ~8° downward tilt, horizon line located around the 50% screen height.
- **Art Style:** Vibrant stylized casual 3D look, warm sunlight, smooth textures, cheerful royal kingdom theme.

### ⚠️ Architecture & Layout Rule (Crucial!):
Our game uses real-time 3D physics inside Unity. Therefore, the screen is functionally split into two halves:

1. **UPPER HALF (50% to 100% Height) — Visual Backdrop:**
   - Royal castle battlements, towers, flags, distant rolling hills, and a sunny sky with soft clouds.
   - Keep the top 15% relatively uncluttered for HUD overlay (lives, coins, level badge).

2. **LOWER HALF (0% to 50% Height) — Empty Playfield Ground:**
   - **DO NOT DRAW ANY OBJECTS HERE.**
   - No tables, no pedestals, no stands.
   - No boxes, crates, barrels, or targets.
   - No cannons, mortars, weapons, or slingshots.
   - This bottom area MUST BE a clean, vacant, low-contrast castle courtyard ground (paved cobblestone or royal courtyard floor).
   - *Reason:* Our Unity engine dynamically spawns real interactive 3D physics props (pedestal, destructible boxes, and player cannon) right on top of this ground.

Layered PSD delivery (Sky, Castle Horizon, and Ground on separate layers) is strongly preferred.
```
