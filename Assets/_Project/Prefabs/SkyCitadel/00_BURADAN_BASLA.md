# Sky Citadel — Prefab koleksiyonu

**Önce PF_00_SkyCitadel_Showcase.prefab:** Beş blok çeşidi, kule, stand, havanın tam hali ve enerji topunu etiketleriyle birlikte gösterir. Unity menüsü: **Royal Smash > Sky Citadel > Open Prefab Collection**.

| Prefab | İçerik |
| --- | --- |
| PF_00_SkyCitadel_Showcase | İnceleme vitrini; dekoratif örnekler, fizik/oyun davranışı yok |
| PF_01_Tower_26Blocks | 26 gerçek fizik bloğu + stand; alt merkezden taşınabilen tek prefab |
| PF_02_Mortar_Complete | Tam havan: gövde, namlu, döner taban/kaide, FirePoint ve MortarController |
| PF_03_Stand | Mermer, altın ve mavi stand |
| PF_04_EnergyBall | Fizik ve ProjectileController içeren enerji topu |
| Blocks/PF_Block_01_Wood | Ahşap blok — kulede 6 adet |
| Blocks/PF_Block_02_Stone | Taş blok — kulede 6 adet |
| Blocks/PF_Block_03_Crystal | Mavi kristal blok — kulede 4 adet |
| Blocks/PF_Block_04_Armored | Metal köşeli zırhlı blok — kulede 4 adet |
| Blocks/PF_Block_05_Core | Enerji çekirdekli blok — kulede 6 adet |

**Toplam:** 5 çeşit blok, kulede 26 blok, koleksiyonda 10 prefab.

Prefab Mode içinde objeleri döndürüp bütün yüzlerini inceleyebilirsin. Tekil blok, stand ve top prefabları mevcut Art/SkyCitadel kaynaklarına bağlı prefab varyantlarıdır; materyal ve meshler paylaşılır. Vitrin mevcut görünümün sabit bir görsel kopyasıdır.

Havanın kök noktası özgün Mortar köküdür; kaidesi kökten yaklaşık 1 birim aşağı uzanır. Oynanabilir sahnede InputHandler ve MainCamera kullanır. Nişan, FirePoint, konfigürasyon ve top prefabı bağlantıları kayıtlıdır. Kamera/HUD/sahne yöneticileri havana dahil değildir.

Kule, sahnede yeni fizik objeleri oluşturur. MainGame'in LevelLoader sistemi zaten kendi hedeflerini üretir; kuleyi ayrıca oraya eklemek hedefleri iki kez oluşturur. Koleksiyonu incelemek için PF_00 vitrini kullanılır. MainGame'in mevcut kurulumu değiştirilmedi.

Görünüm değişince koleksiyonu yeniden dışa aktarmak için MainGame Edit Mode'da **Royal Smash > Sky Citadel > Export Prefab Collection** kullanılabilir. Bu işlem koleksiyon prefablarını günceller.
