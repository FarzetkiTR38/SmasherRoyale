# Bottom Navbar — 360 × 180

Altı ayrı PNG: MAĞAZA / ANA MENÜ / SIRALAMA için Normal (mavi) ve Selected (altın) durumlar.

| Menü | Normal | Seçili |
| --- | --- | --- |
| Mağaza | Nav_Shop_Normal.png | Nav_Shop_Selected.png |
| Ana menü | Nav_Home_Normal.png | Nav_Home_Selected.png |
| Sıralama | Nav_Rank_Normal.png | Nav_Rank_Selected.png |

Her dosya **360 × 180 px**, RGBA PNG. Dış şeffaf boşluk yok; köşe kesimleri şeffaftır. Tüm görünür dış sınırlar aynı 360 × 180 alanı doldurur. Üç buton = **1080 × 180 px**.

Unity import hazır: Sprite (Single), Full Rect, PPU 100, ortalı pivot, alpha açık, mipmap kapalı, sıkıştırmasız. Mevcut UI Image boyutu 360 × 180, Image Type Simple; yatay yerleşimde spacing/padding 0 kullanılabilir. Üç buton eşit boyda kalmalı; seçili durumda scale artırılmamalı.

MainMenu sahnesi ve mevcut navbar sistemi değiştirilmedi. Sprite değişimini kullanıcı yapacak.

2026-09-23: Built-in image_gen ile mevcut tasarımlardan altı varyant üretildi; ortak dış ölçüye PNG export yapıldı. Promptlar, üretim kaynakları ve kontrol raporları: _FarzetkiDocs/Art/BottomNav360/. 1080 px yan yana önizleme: BottomNav_1080_Preview.png.
