# Royal Smash! – Physics Puzzle
## Kapsamlı Oyun Dokümantasyonu ve MVP Spesifikasyonu

> **Amaç:** Bu belge, mevcut konuşmada gözlemlenen ve netleştirilen oyun davranışlarını tek bir kaynakta toplamak için hazırlanmıştır. Daha sonra başka bir yapay zekâya verilerek oyunun benzer bir versiyonu için kapsamlı bir “master build prompt” oluşturulmasında kullanılabilir.
>
> **Önemli:** Kesinleşmemiş sayılar ve teknik tercihler “yaklaşık”, “örnek” veya “configurable” olarak belirtilmiştir. Bu değerler gözlemlenen oyunun kesin teknik değerleri olarak kabul edilmemelidir.

---

# 1. Orijinal Oyun Tanımı / İlk Gözlemler

Oyun, **Royal Smash! - Physics Puzzle** benzeri bir mobil fizik/puzzle oyunudur.

Temel oynanış:

- Alt tarafta bir mortar/top bulunur.
- Oyuncu mortar ile yukarıdaki platform/stand üzerinde duran objelere toplar fırlatır.
- Amaç, stand üzerindeki bütün objeleri devirmek, hareket ettirmek veya kırarak ortadan kaldırmaktır.
- Objeler arasında:
  - kutular,
  - boya şişeleri,
  - taşlar,
  - kolonlar,
  - koniler
  gibi farklı tipler bulunur.
- Oyuncunun sınırlı sayıda topu vardır.
- Tüm objeler toplar bitmeden temizlenmelidir.
- Gözleme göre bazı seviyelerde yaklaşık 20–30 civarı top bulunabilir. Bu kesin değer değildir.
- Bazı seviyelerde objelerin arkasında ikinci bir sıra/katman bulunabilir.

---

# 2. Ana Gameplay Mekaniği

Oyuncunun temel amacı:

**Sınırlı sayıdaki topu kullanarak stand üzerindeki bütün objeleri yok etmek/standdan düşürmek.**

Bir level şu temel döngüye sahiptir:

1. Level başlar.
2. Oyuncu mortar/top konumunu ayarlar.
3. Oyuncu ekrana dokunur.
4. Top hedeflenen noktaya doğru ateşlenir.
5. Top fiziksel olarak dünyada ilerler.
6. Objelere çarpar.
7. Objeler fizik kurallarına göre hareket eder, devrilir veya kırılır.
8. Oyuncu kalan toplarla devam eder.
9. Bütün objeler temizlenirse level kazanılır.
10. Toplar biter ve obje kalırsa level kaybedilir.

---

# 3. Oyuncu Kontrolü

## 3.1 Tap-to-Fire

Oyuncunun ayrı bir:

- güç barı,
- şarj sistemi,
- açı göstergesi,
- manuel power UI

kullanması gerekmez.

Oyuncu ekrana dokunduğunda ateş gerçekleşir.

## 3.2 Drag ile Mortar Kontrolü

Oyuncu parmağını ekranda hareket ettirdiğinde:

- mortar oyuncunun hareketini takip eder,
- ateş gerçekleşmez.

Yani temel kontrol mantığı:

- **Drag → Aim / mortar movement**
- **Tap → Fire**

şeklindedir.

## 3.3 Mortar Hareketi

Mortar normalde ağırlıklı olarak yatay hareket eder.

- Ana hareket ekseni X'tir.
- Çok küçük Y hareketi görsel olarak faydalı olacaksa kullanılabilir.
- Mortar map/kamera sınırlarının dışına çıkamaz.
- Kabaca standın sol ve sağ sınırları arasında yönlendirilebilir.

---

# 4. World Coordinate Targeting

Oyunun en önemli kontrol özelliklerinden biri budur.

Oyuncu ekrana dokunduğunda top yalnızca ekrandaki bir açıya göre ateş edilmemelidir.

Bunun yerine:

**Oyuncunun dokunduğu nokta gerçek WORLD COORDINATE hedefi olarak kullanılmalıdır.**

Örneğin:

- Oyuncu uzak bir objenin üst kısmına dokunursa top o dünya noktasına gider.
- Oyuncu daha arkadaki bir noktaya dokunursa top o dünya konumuna doğru ilerler.

Bu sayede oyuncu:

> “Şu objeye doğru ateş et”

yerine doğrudan:

> “Dünyadaki şu noktaya top gönder”

mantığında kontrol sahibi olur.

Bu mekanik master build prompt içinde özellikle korunmalıdır.

---

# 5. Hedefleme Sınırları

- Oyuncu standın altındaki bölgeyi hedef olarak kullanamaz.
- Standın üstüne veya arkasına dokunabilir.
- Oyuncunun dokunduğu hedefin gerçekten fiziksel olarak erişilebilir olması gerekir.
- Standın kendisine dokunmak tek başına anlamlı bir aksiyon değildir; topun gerçek trajectory'si bir objeye ulaşabiliyorsa anlamlıdır.

---

# 6. Ateş Etme Cooldown

Level başında ilk top:

**hemen hazırdır.**

Oyuncu ilk ateşi beklemeden gerçekleştirebilir.

Ateş ettikten sonra kısa bir cooldown bulunur.

Gözlemsel olarak:

**~0.5 saniye**

civarında olabilir.

Bu değer kesin değildir ve implementation sırasında configurable yapılmalıdır.

---

# 7. Projectile / Ball Physics

Top fiziksel bir projectile gibi davranmalıdır.

Temel özellikler:

- Momentum taşır.
- Impulse oluşturur.
- Objelerle fiziksel collision yaşar.
- Objeleri hareket ettirebilir.
- Objeye göre sekebilir.
- Gravity etkisiyle aşağı düşebilir.

Topun hareketi arcade tarzında yapay görünmemelidir.

Amaç:

**Basit ama doğal fizik hissi.**

---

# 8. Obje Ağırlığı ve Fizik

Objelerin ağırlıkları farklıdır.

Hafif obje:

- daha kolay itilir,
- top tarafından daha kolay hareket ettirilir.

Ağır obje:

- daha zor hareket eder,
- topun hareketini daha fazla etkileyebilir,
- top daha belirgin şekilde sekebilir.

Örneğin:

- Kutu → hafif
- Taş → ağır
- Kolon → ağır

olabilir.

Örnek fizik değerleri:

```text
Box Weight = 10
Stone Weight = 20
```

Ancak bunlar **örnektir**.

Master build prompt içinde:

> Object weight values must be configurable.

şeklinde ele alınmalıdır.

Ağırlık oyuncuya UI üzerinden gösterilmez.

---

# 9. Top ve Obje Çarpışmaları

Top bir objeye çarptığında:

- obje hareket edebilir,
- obje devrilebilir,
- top sekebilir,
- top objeyi ileri doğru itebilir.

Örneğin tek bir kutuya çarpan top kutuyu rahatça hareket ettirebilir.

Kutunun üzerinde 1–4 kutu olsa bile çarpılan kutu hâlâ hareket edebilir.

Ancak:

- daha fazla kutu,
- daha ağır obje,
- daha karmaşık stack

hareketi zorlaştırır.

Bu nedenle puzzle'ın önemli kısmı:

**doğru noktaya doğru fiziksel darbeyi göndermek**

olmalıdır.

---

# 10. Bounce / Sekme

Top objelere çarptığında sekebilir.

Ancak bu:

- özel arcade bounce sistemi,
- duvardan sekme mekaniği,
- pinball tarzı sistem

değildir.

Daha doğal bir fiziksel sekme yeterlidir.

Top:

1. Objeye çarpar.
2. Momentum aktarır.
3. Sekebilir.
4. Bir süre hareket eder.
5. Gravity etkisiyle aşağı düşebilir.
6. Sonunda yok olur.

---

# 11. Duvar Sekmesi

Özel wall-bounce sistemi gerekli değildir.

Oyunda topun sürekli duvarlardan sekmesini sağlayan bir mekanik bulunması beklenmemektedir.

---

# 12. Boşluğa Ateş Etme

Oyuncu herhangi bir objenin bulunmadığı boş bir alana ateş edebilir.

Bu durumda:

- top yine fiziksel olarak ilerler,
- herhangi bir objeye çarpmaz,
- doğal şekilde yoluna devam eder,
- gravity/fizik etkileri uygulanabilir,
- sonunda yok olur.

Topun yaklaşık yaşam süresi:

**~5 saniye**

olabilir.

Bu değer configurable olmalıdır.

---

# 13. Top Yok Olma

Gözleme göre top tamamen kaybolmadan önce bir yok olma efekti olabilir.

Örneğin:

- ~4. saniyede destroy/disappear effect başlar,
- ~5. saniyede top tamamen yok olur.

Bu değerler yaklaşık gözlemdir.

Amaç:

**Topun bir anda pop olarak kaybolmaması.**

---

# 14. 3D Dünya Hissi

Oyun 3D bir dünyada uygulanıyorsa, boşluğa giden top:

- sadece 2D ekranda küçülüp kayboluyormuş gibi görünmemeli,
- dünyanın arka tarafına doğru ilerliyormuş hissi vermeli,
- uzak background/environment'a doğru hareket ediyor gibi görünmelidir.

Teknik çözüm implementation aşamasında seçilebilir.

Burada önemli olan:

**oyuncunun topun gerçekten dünyaya doğru ilerlediğini hissetmesidir.**

---

# 15. Obje Türleri – MVP

MVP'de beş temel obje türü bulunacaktır:

1. Box
2. Paint Bottle
3. Stone
4. Column
5. Cone

---

# 16. Box

Kutu:

- fiziksel olarak hareket edebilir,
- başka kutuların altında/üstünde bulunabilir,
- çarpışmada itilebilir,
- devrilebilir,
- standdan düşebilir.

Kutu MVP'nin temel hafif fizik objesidir.

---

# 17. Paint Bottle

Boya şişesi diğer objelerden farklı davranır.

Çarpışma sonucunda:

1. Şişe kırılır.
2. Cam kırılma efekti oluşur.
3. Boya/sıvı dışarı dökülür.
4. Obje yok olur.

Şişe anında pop şeklinde yok olmamalıdır.

---

# 18. Stone

Stone ağır fizik objesidir.

- Daha yüksek mass/weight değerine sahiptir.
- Hareket ettirilmesi daha zordur.
- Çarpışmada daha ağır tepki verir.
- Topu daha fazla etkileyebilir.

Kesin mass/physics değerleri configurable olmalıdır.

---

# 19. Column

Column:

- uzun bir kutu/kolon şeklinde düşünülebilir,
- ağır bir obje olabilir,
- devrilme mekaniğinde kullanılabilir,
- diğer objelere göre daha fazla fiziksel ağırlık taşıyabilir.

Level tasarımında uzun yatay veya dikey yapılar oluşturmak için kullanılabilir.

---

# 20. Cone

Cone:

- fiziksel olarak hareket eder,
- devrilebilir,
- top tarafından itilebilir.

Diğer hareketli objeler gibi weight/mass değerine sahip olabilir.

---

# 21. Obje Yok Olma Efektleri

Normal objeler mümkün olduğunca aniden yok olmamalıdır.

Önerilen davranış:

- obje düşer,
- fiziksel hareketini tamamlar,
- yok olma durumuna girer,
- hafifçe küçülür/melt olur,
- ardından tamamen görünmez olur.

Bu davranış oyunun genel polish seviyesini artırmalıdır.

---

# 22. Stand

Objelerin bulunduğu platform:

**Stand**

olarak adlandırılacaktır.

Özellikleri:

- Sabittir.
- Kendi başına hareket etmez.
- Fiziksel objeleri üzerinde taşır.
- Bir veya iki ayrı platform olabilir.
- MVP'de maksimum 2 platform bulunur.

Stand genişliği/uzunluğu:

**level bazında configurable**

olmalıdır.

---

# 23. Stand Yerleşimleri

Level tasarımında farklı stand şekilleri kullanılabilir:

- geniş yatay stand,
- dar uzun stand,
- yüksek stand,
- iki ayrı stand,
- iki platformlu yapı,
- birbirine bağlı platform parçaları,
- uzun yatay kolonlar.

Bu farklılıklar level progression'ın görsel ve puzzle çeşitliliğini sağlar.

---

# 24. Level İçinde İki Katman

Bazı seviyelerde:

- önde bir obje sırası,
- arkada ikinci bir obje sırası

bulunabilir.

Oyuncunun amacı sadece öndeki objeleri değil, arkadaki objeleri de temizlemektir.

Bu nedenle fizik sistemi:

- depth,
- collision,
- object stacking

gibi durumları desteklemelidir.

---

# 25. Temel Puzzle Prensibi

Oyuncu yalnızca objeye doğrudan vurmak zorunda değildir.

Örneğin:

- bir kutuya vurup arkasındaki kutuyu hareket ettirmek,
- üstteki kutuyu düşürmek,
- ağır objeyi zincirleme hareketin parçası yapmak,
- bir objeyi başka objeye çarptırmak

mümkün olmalıdır.

Bu nedenle oyunun temel eğlencesi:

**physics-based chain reaction + limited shots**

kombinasyonundan gelmelidir.

# 26. Level Sistemi

MVP kapsamında toplam:

**30 level**

bulunacaktır.

Level sistemi sequential çalışır.

Oyuncu:

- Level 1'den başlar.
- Level'ları sırayla tamamlar.
- Bir sonraki level, mevcut level kazanıldıktan sonra açılır.
- Level Select ekranı bulunmaz.
- Eski level'ları tekrar seçme sistemi bulunmaz.
- Replay sistemi MVP kapsamı dışındadır.

---

# 27. Level Progression

Level progression'ın temel amacı, oyuncuya düzenli olarak yeni fiziksel puzzle öğeleri sunmaktır.

MVP'de her 5 levelde bir yeni block/object/content eklenmesi **kesin tasarım kararıdır**.

Plan:

| Level | İçerik |
|---|---|
| 1–4 | Başlangıç objeleri |
| 5 | Yeni block/object |
| 10 | Yeni block/object + nickname |
| 15 | Yeni block/object |
| 20 | Yeni block/object |
| 25 | Yeni block/object |
| 30 | Final MVP level |

Buradaki “yeni block” yeni bir obje tipi olmak zorunda değildir; level tasarımında yeni bir fiziksel davranış veya içerik grubu da olabilir.

---

# 28. Level 5

Level 5 önemli bir progression noktasıdır.

Bu seviyede:

- yeni block/object içeriği eklenir,
- Skill 1 (Mega Ball) açılır.

Oyuncu böylece ilk kez skill mekaniklerini kullanmaya başlayabilir.

---

# 29. Level 10

Level 10'da:

- yeni block/object içeriği eklenir,
- Skill 2 (Infinity Ball) açılır,
- oyuncudan nickname/username girmesi istenir.

Bu noktadan sonra oyuncu leaderboard/profile sisteminde bir kullanıcı adıyla temsil edilir.

---

# 30. Level 15

Level 15:

- yeni block/object içeriğinin eklendiği progression noktasıdır.

---

# 31. Level 20

Level 20:

- yeni block/object içeriğinin eklendiği progression noktasıdır.

---

# 32. Level 25

Level 25:

- yeni block/object içeriğinin eklendiği progression noktasıdır.

---

# 33. Level 30

Level 30 MVP'nin son seviyesidir.

Level tamamlandığında:

- normal victory akışı çalışır,
- Win Panel açılır,
- reward verilir.

Ancak:

**Next Level butonu bulunmaz.**

Oyuncu:

- Main Menu'ye dönebilir,
- veya uygun bir MVP completion aksiyonuna yönlendirilebilir.

---

# 34. İlk Açılış Akışı

İlk uygulama açılışında temel akış:

```text
Splash
   ↓
Loading
   ↓
Tutorial
   ↓
Level 1
```

---

# 35. Splash Screen

Splash ekranında:

- developer logo,
- publisher logo

bulunabilir.

Gerçek logo/branding daha sonra değiştirilebileceği için MVP implementation'ında placeholder/template logo kullanılabilir.

Logo sistemi gameplay açısından kritik değildir.

---

# 36. Loading Screen

Loading ekranında:

- loading indicator,
- tip/tavsiye metni,
- basit background/design

bulunabilir.

Orijinal gözlemde loading screen tasarımının değişebildiği görülmüştür.

Ancak MVP için:

**tek bir loading screen tasarımı yeterlidir.**

---

# 37. Tutorial

Tutorial yalnızca:

**ilk launch**

sırasında gösterilir.

Oyuncu tutorial'ı tamamladıktan sonra normal oyuna geçer.

Tutorial ayrı bir karmaşık eğitim sistemi olmak zorunda değildir.

---

# 38. Tutorial Map

Tutorial:

- normal gameplay'e benzeyen bir map üzerinde yapılır,
- gerçek mortar/top sistemi kullanılır,
- gerçek fizik sistemi kullanılır.

Tutorial sırasında oyuncuya belirli bir hedef bölgesi gösterilir.

Örneğin:

- highlight,
- circle,
- arrow,
- glow

gibi görsel bir işaret kullanılabilir.

---

# 39. Tutorial Input Restriction

Tutorial sırasında:

**yalnızca belirtilen alan aktif olmalıdır.**

Oyuncu işaretlenen alanın dışına dokunursa:

- ateş gerçekleşmez,
- tutorial akışı bozulmaz.

Bu restriction tutorial tamamlanana kadar geçerlidir.

---

# 40. Tutorial Örneği

Basitleştirilmiş akış:

1. Level/map açılır.
2. Mortar hazırdır.
3. Oyuncunun hedeflemesi gereken alan highlight edilir.
4. Oyuncu ilgili noktaya dokunur.
5. İlk top ateşlenir.
6. Obje fiziksel olarak düşer/hareket eder.
7. Tutorial oyuncunun geri kalan gameplay'i kendi başına oynamasına izin verir.
8. Tutorial tamamlanır.
9. Normal level progression başlar.

---

# 41. İlk Top

Her level başladığında ilk top:

**hemen hazır**

olmalıdır.

Oyuncu level açıldıktan sonra ilk ateşi yapmak için cooldown beklememelidir.

---

# 42. Fire Cooldown

İlk ateşten sonra normal toplarda kısa bir cooldown uygulanır.

Yaklaşık:

**0.5 saniye**

olarak gözlemlenmiştir.

Ancak bu:

**configurable parameter**

olmalıdır.

---

# 43. Victory Condition

Bir level'ın kazanılması için:

**stand üzerinde temizlenmesi gereken bütün objelerin ortadan kalkması gerekir.**

Objenin:

- kırılması,
- standdan düşmesi,
- yok olması

geçerli bir completion sonucu olabilir.

Son gerekli obje de ortadan kalktığında level tamamlanmış kabul edilir.

---

# 44. Victory Delay

Son obje düştüğü anda Win Panel anında açılmaz.

Yaklaşık:

**2 saniye**

bekleme yapılır.

Bu süre:

- son fizik hareketlerinin tamamlanması,
- oyuncunun sonucu görmesi,
- VFX/SFX'in oynatılması

için kullanılabilir.

Süre configurable olmalıdır.

---

# 45. Win Panel

Win Panel'de:

- tamamlanan level,
- reward,
- remaining ball bonus,
- Continue/Next butonu

gösterilebilir.

Star sistemi yoktur.

Efficiency rating yoktur.

Score rating yoktur.

Oyuncunun performansı için yıldız gibi ikinci bir değerlendirme sistemi MVP'de kullanılmaz.

---

# 46. Remaining Ball Reward

Oyuncu level'ı tamamladığında kalan toplar ekstra Gold kazandırır.

Formül:

```text
Bonus Gold = Remaining Balls × 2
```

Örnek:

```text
Remaining Balls = 15
Bonus Gold = 15 × 2 = 30 Gold
```

Bu formül MVP için kesinleştirilmiştir.

---

# 47. Level Reward

Level tamamlandığında ayrıca temel Gold reward verilebilir.

Örneğin:

```text
Base Level Reward = 50 Gold
```

Ancak 50 Gold:

**örnek/configurable değer**

olarak değerlendirilmelidir.

Kesin ekonomi değeri implementation aşamasında ayarlanabilir.

---

# 48. Lose Condition

Level şu durumda kaybedilir:

```text
Remaining Balls = 0
AND
Remaining Objects > 0
```

Yani oyuncunun tüm topları bittiğinde hâlâ temizlenmemiş obje varsa level fail olur.

---

# 49. Lose Panel

Lose Panel'de aşağıdaki bilgiler/aksiyonlar bulunabilir:

- Level failed mesajı
- Life kaybı
- Remaining life
- Retry
- Main Menu

Lose ekranı sade tutulmalıdır.

---

# 50. Lose Sonrası Reklam

Lose sonrasında mandatory/interstitial reklam veya uygun reklam akışı tetiklenebilir.

Bu davranış reklam sisteminin global ayarlarına bağlı olabilir.

No Ads satın alan oyuncularda mandatory/interstitial reklam gösterilmez.

Rewarded ad sistemi ise devam eder.

---

# 51. Level'dan Çıkış

Oyuncu level sırasında çıkmak istediğinde:

**confirmation popup**

gösterilebilir.

Örneğin:

> “Are you sure you want to leave this level?”

Ancak önemli kural:

**Level'dan çıkmak life kaybettirir.**

---

# 52. Uygulamayı Kapatma

Oyuncu uygulamayı tamamen kapatsa bile leveldan çıkmış kabul edilir.

Dolayısıyla:

**App close → level exit → life loss**

mantığı uygulanır.

Bu sistem oyuncunun:

> “Oyunu kapatayım, sonra tekrar açınca life kaybetmemiş olayım.”

şeklinde bir exploit kullanmasını engellemelidir.

---

# 53. Lives / Health Sistemi

Oyuncu:

**5 life**

ile başlar.

Life:

- level kazanıldığında harcanmaz,
- level kaybedildiğinde harcanır,
- leveldan çıkıldığında harcanır,
- uygulama kapanarak leveldan çıkıldığında harcanır.

---

# 54. Life Kaybı

Life kaybı şu durumlarda gerçekleşir:

### Lose

```text
Balls = 0
Objects remain
→ -1 Life
```

### Exit

```text
Player exits active level
→ -1 Life
```

### App Close

```text
Active level is closed
→ -1 Life
```

Victory:

```text
Level completed
→ 0 Life loss
```

---

# 55. Life Regeneration

MVP'de life otomatik olarak yenilenir.

Regeneration:

**30 dakikada +1 life**

şeklindedir.

Örneğin:

```text
4 Life
↓ 30 min
5 Life
```

Kesin maksimum life davranışı implementation sırasında configurable olabilir; başlangıç kapasitesi 5 olarak tasarlanmıştır.

---

# 56. Zero Life

Oyuncunun life sayısı:

**0**

olduğunda normal level oynanamaz.

Oyuncuya iki alternatif sunulabilir:

1. Gold ile life almak
2. Rewarded ad izleyerek life almak

Bu şekilde oyuncunun yeniden oynaması mümkün olur.

---

# 57. Life Satın Alma

Gold ile life satın alınabilir.

Fiyat:

**configurable**

olmalıdır.

Örneğin UI'da:

```text
+1 Life
[XX Gold]
```

gibi gösterilebilir.

Kesin Gold miktarı daha sonra belirlenebilir.

---

# 58. Rewarded Ad ile Life

Oyuncu 0 life olduğunda veya uygun life ekranında:

**Rewarded Ad**

izleyerek life kazanabilir.

Rewarded ad:

- kullanıcı tarafından isteyerek başlatılır,
- reklam tamamlandığında reward verilir.

Rewarded ads No Ads satın alınsa bile aktif kalır.

---

# 59. Skill Sistemi

MVP'de iki skill bulunur:

1. Mega Ball
2. Infinity Ball

İki skill de başlangıçta locked durumdadır.

---

# 60. Mega Ball

Mega Ball:

- normal toptan daha büyük bir projectile kullanır,
- daha güçlü fiziksel etki yaratabilir,
- daha büyük collision area sağlayabilir.

Unlock:

**Level 5**

Başlangıç kullanım sayısı:

**3**

Her kullanım:

```text
3 → 2 → 1 → 0
```

şeklinde azalır.

Ek kullanım Shop üzerinden alınabilir.

---

# 61. Mega Ball Boyutu

Mega Ball'ın kesin çapı henüz belirlenmemiştir.

Örneğin:

```text
Normal Ball Diameter = 10
Mega Ball Diameter = 20
```

gibi bir oran düşünülebilir.

Ancak bunlar:

**örnek değerlerdir.**

Master build prompt içinde bu değerler configurable bırakılmalıdır.

---

# 62. Infinity Ball

Infinity Ball'ın temel amacı:

**normal firing cooldown'unu geçici olarak kaldırmaktır.**

Aktif olduğu sürece oyuncu:

- çok daha hızlı,
- neredeyse arka arkaya

top ateşleyebilir.

Unlock:

**Level 10**

Başlangıç kullanım sayısı:

**3**

---

# 63. Infinity Ball Süresi

Infinity Ball timer tabanlı bir skill'dir.

Aktivasyon:

**anında**

gerçekleşir.

Örneğin:

```text
Duration ≈ 5 sec
```

olabilir.

Ancak bu değer kesin değildir.

Configurable parameter olarak ele alınmalıdır.

---

# 64. Skill Kullanım Sistemi

Her skill başlangıçta:

**3 kullanım**

ile gelir.

Bir skill aktive edildiğinde:

**1 kullanım düşer.**

Örneğin:

```text
Mega Ball
3 uses
↓ Activate
2 uses
```

Skill kullanım hakkı bittikten sonra Shop üzerinden ek kullanım satın alınabilir.

---

# 65. Skill Stack

Skills stack edebilir.

Bu, iki skill'in sistem tarafından aynı anda aktif olmasına izin verilebileceği anlamına gelir.

Örneğin:

```text
Mega Ball active
+
Infinity Ball active
```

aynı anda çalışabilir.

Teknik implementation bu iki modifier'ın birbirini bozmayacağı şekilde yapılmalıdır.

---

# 66. Skill UI

Skill butonları:

- icon,
- kullanım sayısı,
- locked state,
- aktif timer

gibi bilgileri gösterebilir.

Locked skill:

```text
🔒
```

benzeri görsel state ile gösterilebilir.

Kullanım bittiğinde:

```text
[Price]
```

gibi Shop/purchase yönlendirmesi gösterilebilir.

Infinity Ball aktifken:

```text
Timer
```

gösterilmelidir.

Skill isimlerinin oyuncuya yazıyla gösterilmesi zorunlu değildir.

Icon'lar yeterli olabilir.

---

# 67. Skill Unlock State

Başlangıç:

```text
Mega Ball      LOCKED
Infinity Ball  LOCKED
```

Level 5:

```text
Mega Ball      UNLOCKED
Infinity Ball  LOCKED
```

Level 10:

```text
Mega Ball      UNLOCKED
Infinity Ball  UNLOCKED
```

Bu unlock sistemi MVP için kesindir.

# 68. Gold Ekonomisi

Oyunun ana oyun içi para birimi:

**Gold**

Kullanıcı bazı noktalarda “coin” ifadesini kullanmış olsa da dokümantasyonda standart isim **Gold** olarak kullanılacaktır.

Gold oyuncu tarafından çeşitli sistemlerde kullanılabilir.

---

# 69. Gold Kullanım Alanları

Gold aşağıdaki amaçlarla kullanılabilir:

- Skill kullanım hakkı satın almak
- Life satın almak
- Shop paketleri satın almak
- Gold + Skill bundle'ları satın almak
- Diğer oyun içi satın alma sistemleri

---

# 70. Başlangıç Gold

Oyuncuya başlangıçta belirli miktarda Gold verilebilir.

Örnek:

```text
Starting Gold = 1000
```

Ancak **1000 kesin bir değer değildir**.

Bu değer:

**configurable**

olmalıdır.

---

# 71. Level Gold Reward

Oyuncu level tamamladığında temel Gold reward kazanabilir.

Örnek:

```text
Base Level Reward = 50 Gold
```

Bu değer de kesin değildir.

Master build prompt içerisinde:

```text
levelCompletionRewardGold
```

gibi configurable bir parametre olarak tutulabilir.

---

# 72. Remaining Ball Gold Bonus

Level tamamlandığında oyuncunun elinde kalan toplar ekstra Gold kazandırır.

Formül:

```text
Bonus Gold = Remaining Balls × 2
```

Örnek:

```text
Remaining Balls = 15

15 × 2 = 30 Gold
```

Dolayısıyla oyuncu 15 top bıraktıysa:

**30 Gold bonus**

alır.

Bu formül MVP için kesinleştirilmiştir.

---

# 73. Gold Satın Alma

Gold gerçek para karşılığında Shop üzerinden satın alınabilir.

Örnek paket yapısı:

```text
Small Gold Pack
Medium Gold Pack
Large Gold Pack
Mega Gold Pack
```

Paket miktarları ve gerçek para fiyatları:

**configurable**

olmalıdır.

---

# 74. Shop

Main Menu'deki bottom navigation üzerinden:

**Shop**

ekranına gidilebilir.

Shop içerisinde temel olarak:

- Gold paketleri
- Skill kullanım paketleri
- Limited-time bundle'lar
- Gold + Skill bundle'ları
- No Ads

bulunabilir.

---

# 75. Gold Paketleri

Shop'ta oyuncuya farklı Gold miktarları sunulabilir.

Örnek:

```text
100 Gold
500 Gold
1000 Gold
5000 Gold
```

Bunlar yalnızca örnektir.

Gerçek paket miktarları ve fiyatları daha sonra balance edilebilir.

---

# 76. Skill Paketleri

Shop üzerinden skill kullanım hakkı satın alınabilir.

Örneğin:

```text
+1 Mega Ball
+5 Mega Ball
+1 Infinity Ball
+5 Infinity Ball
```

gibi paketler oluşturulabilir.

Kesin paket içeriği implementation aşamasında belirlenebilir.

---

# 77. Gold + Skill Bundle

Shop'ta iki veya daha fazla avantajı bir araya getiren bundle'lar olabilir.

Örneğin:

```text
Gold
+
Mega Ball Uses
```

veya:

```text
Gold
+
Infinity Ball Uses
```

gibi.

Bu paketlerin fiyatları configurable olmalıdır.

---

# 78. Limited-Time Offer

Shop/Main Menu içerisinde:

**Limited-Time Offer**

bulunabilir.

Bu offer:

- belirli süre boyunca aktif olabilir,
- countdown gösterebilir,
- süre bitince yeniden başlayabilir veya yeni offer'a dönüşebilir.

MVP için aynı paketin tekrar eden cycle sistemi yeterlidir.

---

# 79. Offer Countdown

Örnek:

```text
Offer Duration = 24 hours
```

Bu değer yalnızca örnektir.

UI'da:

```text
LIMITED OFFER

23:41:12
```

gibi bir countdown gösterilebilir.

Countdown sıfıra geldiğinde offer:

- kapanabilir,
- resetlenebilir,
- aynı paket tekrar başlayabilir.

MVP'de karmaşık rotating offer backend'i gerekli değildir.

---

# 80. Daily Reward

Daily reward sistemi MVP için gerekli değildir.

MVP'de:

**Daily Reward → Out of Scope**

---

# 81. Chest System

Chest sistemi de MVP için gerekli değildir.

**Chest → Out of Scope**

---

# 82. No Ads

Shop'ta:

**No Ads**

satın alınabilir.

No Ads satın alan oyuncuda:

**mandatory/interstitial ads**

kaldırılır.

Ancak:

**Rewarded Ads devam eder.**

---

# 83. No Ads Kapsamı

No Ads satın alındığında:

### Kaldırılır

- Mandatory ads
- Interstitial ads

### Kaldırılmaz

- Rewarded ads

Örneğin oyuncu hâlâ:

> “Watch Ad → +1 Life”

seçeneğini kullanabilir.

Bu ayrım MVP için kesin bir karardır.

---

# 84. Ads Sistemi

Oyunda iki temel reklam tipi vardır:

1. Mandatory / Interstitial Ads
2. Rewarded Ads

---

# 85. Mandatory / Interstitial Ads

Mandatory/interstitial reklamlar belirli oyun ilerleme noktalarında gösterilebilir.

Temel plan:

**Yaklaşık her 5 level**

arasında reklam gösterilmesi.

Örneğin:

```text
Level 5 completed
→ possible interstitial

Level 10 completed
→ possible interstitial

Level 15 completed
→ possible interstitial
```

vb.

Ayrıca Lose sonrasında da reklam gösterilebilir.

---

# 86. Rewarded Ads

Rewarded Ads oyuncunun isteyerek başlattığı reklamlardır.

Örneğin:

```text
No Lives

[Watch Ad]
```

Oyuncu reklamı tamamladığında:

```text
+1 Life
```

alabilir.

Rewarded ad'ler No Ads satın alındığında da kullanılabilir.

---

# 87. Reklam Akışının MVP İlkesi

Reklam sistemi oyuncunun gameplay'ini gereksiz yere bozmamalıdır.

Mandatory/interstitial reklamlar:

- progression noktalarında,
- uygun ekran geçişlerinde,
- Lose sonrasında

kullanılabilir.

Rewarded ads ise:

- oyuncuya açıkça reward sunulan,
- oyuncunun kendisinin başlattığı

akış olmalıdır.

---

# 88. Main Menu

Main Menu oyunun ana hub ekranıdır.

Alt navigation:

```text
Shop | Main Menu | Leaderboard
```

şeklindedir.

Main Menu seçiliyken oyuncunun mevcut progression'ı merkezde gösterilir.

---

# 89. Main Menu Layout

Ana ekranın genel yerleşimi:

```text
┌─────────────────────────────┐
│ Profile          Settings   │
│                             │
│      Gold     Lives         │
│                             │
│                             │
│        CURRENT LEVEL        │
│                             │
│           START             │
│                             │
│ Special Offer   Special     │
│                             │
│ Shop   Main Menu   Leader   │
└─────────────────────────────┘
```

Kesin UI ölçüleri implementation aşamasında belirlenebilir.

---

# 90. Current Level

Main Menu'nin merkezinde oyuncunun mevcut level'ı gösterilir.

Örneğin:

```text
LEVEL 12

[PLAY]
```

Oyuncu buradan aktif level'a başlayabilir.

---

# 91. Special Offers

Main Menu'nin sağında/solunda special offer alanları bulunabilir.

Bu alanlar:

- limited-time bundle,
- Gold offer,
- Skill offer,
- No Ads

gibi ticari içerikleri gösterebilir.

MVP'de karmaşık carousel sistemi şart değildir.

---

# 92. Top Bar

Main Menu'nin üst bölümünde:

### Sol üst

Profile

### Sağ üst

Settings

### Aradaki bölüm

- Gold
- Life/Health

gösterilebilir.

---

# 93. Gold UI

Oyuncunun mevcut Gold miktarı Main Menu'de sürekli görünür olmalıdır.

Örneğin:

```text
🪙 1,250
```

UI iconu ve stil implementation aşamasında seçilebilir.

---

# 94. Life UI

Oyuncunun mevcut life sayısı Main Menu'de gösterilir.

Örneğin:

```text
❤️ 5/5
```

Life regeneration gerekiyorsa timer da gösterilebilir.

Örneğin:

```text
Next Life: 24:32
```

Bu detay implementation aşamasında seçilebilir.

---

# 95. Profile

Profile ekranında oyuncunun temel kimlik/progression bilgileri bulunur.

Gösterilebilecek bilgiler:

- Avatar
- Username
- Current Level
- Genel istatistikler

---

# 96. Profile Stats

Oyuncunun bazı genel istatistikleri gösterilebilir.

Örneğin:

- Current Level
- Completed Levels
- Total Shots
- Total Wins

Ancak:

**Detaylı statistics sistemi MVP için kritik değildir.**

Bu nedenle temel profile sistemi yeterlidir.

---

# 97. Edit Profile

Oyuncu profile ekranından:

- Username
- Avatar
- Frame

değiştirebilir.

---

# 98. Avatar Sistemi

MVP'de:

**8 adet default avatar**

bulunacaktır.

Avatarların tamamı başlangıçta kullanılabilir.

Unlock sistemi yoktur.

Yani:

```text
Avatar 1 → Available
Avatar 2 → Available
...
Avatar 8 → Available
```

---

# 99. Avatar Upload

Oyuncunun kendi telefon galerisinden fotoğraf yüklemesi:

**MVP kapsamı dışındadır.**

Dolayısıyla:

```text
Gallery Upload = OFF
```

sistemi kullanılacaktır.

Sadece hazır avatarlar kullanılabilir.

---

# 100. Frame Sistemi

Frame, avatarın etrafındaki yuvarlak çerçevedir.

Farklı:

- renk,
- desen,
- tasarım

seçenekleri olabilir.

MVP:

**4 frame**

içerir.

Tüm frame'ler başlangıçta kullanılabilir.

Unlock sistemi yoktur.

---

# 101. Username

Oyuncudan nickname/username:

**Level 10 sonrasında**

istenir.

Bu noktadan sonra username profile ve leaderboard'da kullanılabilir.

---

# 102. Username Değiştirme

Oyuncu username'ini değiştirebilir.

Değiştirme sayısı:

**Sınırsız**

olacaktır.

Ekonomik bir rename cost sistemi MVP'de gerekli değildir.

---

# 103. Leaderboard

Bottom navigation içerisindeki üçüncü ana bölüm:

**Leaderboard**

ekranıdır.

Leaderboard temel olarak oyuncuların level progression'ına göre sıralanmasını sağlar.

---

# 104. Leaderboard Sıralaması

Ana sıralama kriteri:

**Current Level**

olacaktır.

Daha yüksek level'daki oyuncu daha üst sırada yer alır.

Gerekirse eşit level durumlarında ikinci bir kriter daha sonra belirlenebilir; ancak MVP için karmaşık ranking sistemi gerekli değildir.

---

# 105. Leaderboard Top 1000

Leaderboard:

**Top 1000**

oyuncuyu gösterebilir.

Her oyuncu satırında:

- rank,
- avatar/profile image,
- username,
- level

gibi bilgiler bulunabilir.

---

# 106. Leaderboard Global / Local

Leaderboard global veya uygun şekilde local/region-based görünüm sağlayabilir.

Ana hedef:

**global/local top 1000**

bilgisini gösterebilmektir.

Kesin sekme yapısı implementation aşamasında seçilebilir.

---

# 107. Online Leaderboard

İnternet bağlantısı mevcutsa:

- güncel leaderboard verisi alınır,
- oyuncunun current progress'i uygun şekilde sıralamaya yansır.

Leaderboard backend'i implementation aşamasında seçilebilir.

---

# 108. Offline Leaderboard

Oyuncu internetsiz olduğunda leaderboard tamamen boş bırakılmamalıdır.

Bunun yerine:

**son başarılı şekilde senkronize edilmiş/cached leaderboard**

gösterilir.

Örneğin:

```text
Last Synced Leaderboard
```

verisi local cache'de tutulabilir.

---

# 109. Offline Progress ve Leaderboard Sync

Örneğin:

```text
Cloud Level = 10
```

Oyuncu offline oynayarak:

```text
Level 12
```

olursa:

İnternet geri geldiğinde:

```text
Local Level 12
       ↓
Cloud Level 12
       ↓
Leaderboard update
```

gerçekleşir.

---

# 110. Cloud Sync Conflict Rule

MVP'de karmaşık conflict resolution sistemi bulunmayacaktır.

Ana kural:

**Latest local progress wins.**

Yani oyuncunun offline ilerlemesi cloud'a geri döndüğünde güncel kabul edilir.

---

# 111. Cross-Device Sync

MVP'de kullanıcı hesabı/login sistemi bulunmadığı için:

**Cross-device account syncing**

kapsam dışıdır.

Amaç tek cihaz üzerinde:

- local save,
- cloud backup/sync

sağlamaktır.

---

# 112. Shop / Main Menu / Leaderboard Navigation

Bottom navigation:

```text
┌────────┬────────────┬─────────────┐
│  Shop  │ Main Menu  │ Leaderboard │
└────────┴────────────┴─────────────┘
```

şeklinde düşünülebilir.

Aktif sekme görsel olarak diğerlerinden ayrılmalıdır.

---

# 113. Genel Monetization Özeti

MVP monetization sistemleri:

```text
Gold Purchase
      │
      ├── Skills
      ├── Lives
      └── Bundles

Skill Bundles
      │
      ├── Mega Ball Uses
      └── Infinity Ball Uses

Limited-Time Offers
      │
      └── Gold + Skills

No Ads
      │
      └── Removes Mandatory/Interstitial Ads

Rewarded Ads
      │
      └── Optional Life Reward
```

---

# 114. Monetization'da Kesin Olmayan Değerler

Aşağıdaki değerler henüz kesin değildir:

- Gold paket miktarları
- Gold gerçek para fiyatları
- Skill kullanım fiyatları
- Life Gold fiyatı
- Bundle fiyatları
- Limited offer fiyatı
- No Ads fiyatı
- Reklam sıklığının kesin interval'i

Bunların tamamı:

**configurable / balanceable**

olmalıdır.

# 115. Settings

Settings ekranında temel oyuncu tercihleri bulunur.

MVP'de:

- Music
- Sound / SFX
- Vibration

ayarları bulunacaktır.

---

# 116. Music

Oyuncu müziği açıp kapatabilir.

Örneğin:

```text id="j2t1c8"
Music
[ ON / OFF ]
```

kullanılabilir.

Orijinal gözlemde basit toggle davranışı vardır.

Slider kullanılması da implementation açısından mümkündür.

---

# 117. Sound / SFX

Oyuncu oyun içi ses efektlerini açıp kapatabilir.

Örneğin:

```text id="j8yd5u"
Sound
[ ON / OFF ]
```

Ses sisteminin ayrıntılı mixer yapısı MVP için kritik değildir.

---

# 118. Vibration

Oyuncu haptic/vibration özelliğini açıp kapatabilir.

Özellikle:

- ateş,
- güçlü çarpışma,
- obje kırılması,
- önemli UI feedback

gibi olaylarda kullanılabilir.

Ancak kesin vibration pattern'leri implementation aşamasında belirlenebilir.

---

# 119. Audio System

MVP'de ses efektleri objeye göre farklılaşmalıdır.

Temel ses grupları:

### Mortar / Fire

- Ateşleme sesi

### Box

- Hafif çarpışma
- Ahşap/kutu benzeri hareket sesi

### Stone

- Daha ağır çarpışma sesi

### Paint Bottle

- Cam kırılma sesi

### UI

- Button click
- Reward
- Win
- Lose

Kesin ses assetleri daha sonra seçilebilir.

---

# 120. Object-Specific Audio

Objenin fiziksel karakteri ses üzerinden de anlaşılmalıdır.

Örneğin:

```text id="f7pvzq"
Box → light impact
Stone → heavy impact
Bottle → glass break
```

Bu sayede oyuncu objenin ağırlığı/kırılabilirliği konusunda görselin yanında ses feedback'i de alır.

---

# 121. VFX System

MVP'de aşağıdaki temel efektler bulunmalıdır:

- Mortar fire effect
- Ball trail / movement effect
- Impact effect
- Object destruction effect
- Glass break effect
- Paint/liquid spill
- Skill activation effect
- Win effect
- UI reward effect

Exact VFX style implementation aşamasında seçilebilir.

---

# 122. Ball Fire VFX

Top ateşlendiğinde:

- kısa bir muzzle/fire effect,
- projectile hareket efekti,
- uygun SFX,
- gerekiyorsa vibration

kullanılabilir.

Amaç:

**ateşleme hissini güçlü ve anlaşılır yapmak.**

---

# 123. Impact VFX

Top bir objeye çarptığında:

- kısa impact effect,
- obje tipine uygun particle,
- SFX,
- gerektiğinde haptic

tetiklenebilir.

Impact effect objenin türüne göre değişebilir.

---

# 124. Object Destruction VFX

Normal objelerde:

1. fiziksel hareket,
2. düşme/devrilme,
3. destroy state,
4. shrink/melt/disappear

sıralaması kullanılabilir.

Obje bir anda yok olmamalıdır.

---

# 125. Paint Bottle VFX

Paint Bottle için özel bir kırılma akışı bulunmalıdır.

Önerilen sıra:

```text id="yqkj60"
Impact
   ↓
Glass Break
   ↓
Paint Spill
   ↓
Object Disappear
```

Boya rengi/tasarımı daha sonra belirlenebilir.

---

# 126. Haptic Feedback

Vibration aşağıdaki durumlarda kullanılabilir:

- Mortar fire
- Güçlü impact
- Stone impact
- Bottle break
- Skill activation
- Win

Haptic intensity objenin ağırlığına göre değişebilir.

Örneğin:

```text id="ckkv3h"
Light object → light vibration
Heavy object → stronger vibration
```

Bu yalnızca önerilen davranıştır; exact tuning implementation aşamasında yapılabilir.

---

# 127. Gameplay HUD – Ayrıntılı Yerleşim

Gameplay sırasında ana HUD sade tutulmalıdır.

Genel yerleşim:

```text id="0u7k5u"
┌───────────────────────────────┐
│ Balls Left             Settings│
│                               │
│                               │
│          GAMEPLAY             │
│                               │
│                               │
│         ─────────             │
│          STAND                │
│                               │
│ Skill 1    MORTAR     Skill 2 │
└───────────────────────────────┘
```

---

# 128. Remaining Balls UI

Sol üst:

**Remaining Balls**

gösterilir.

Örneğin:

```text id="c3d8yi"
🏀 24
```

Exact icon/design değişebilir.

Sayı her ateşte güncellenmelidir.

---

# 129. Settings During Gameplay

Sağ üstte Settings butonu bulunabilir.

Oyuncu buradan:

- Music
- Sound
- Vibration

ayarlarına ulaşabilir.

Ayrıca leveldan çıkış aksiyonu burada bulunabilir.

---

# 130. Skill HUD

Alt köşelerde iki skill bulunur.

Örneğin:

```text id="73k4i0"
[ Mega ]                 [ Infinity ]
   3                         🔒
```

Skill state'e göre:

- locked,
- available,
- active,
- depleted

durumları görsel olarak ayrıştırılmalıdır.

---

# 131. Mortar Position

Mortar:

- ekranın alt orta bölgesinde,
- standın altında,
- oyuncunun kolayca kontrol edebileceği bir konumda

bulunmalıdır.

Oyuncu drag yaparak mortar'ın yatay konumunu değiştirebilir.

---

# 132. Camera / Gameplay Framing

Kamera:

- standı,
- mortar'ı,
- projectile trajectory'sini,
- objeleri

aynı gameplay alanında anlaşılır şekilde göstermelidir.

Exact:

- FOV,
- camera position,
- orthographic size,
- perspective

değerleri implementation aşamasında seçilebilir.

---

# 133. 3D Gameplay Hissi

Gameplay 3D bir fizik dünyası hissi vermelidir.

Özellikle:

- depth,
- arka sıradaki objeler,
- topun uzaklaşması,
- obje stacking,
- fiziksel düşme

gibi davranışlar oyuncuya gerçek bir 3D sahne hissi vermelidir.

---

# 134. Offline Gameplay

Oyun internet bağlantısı olmadan oynanabilir.

Offline durumda:

- mevcut local level,
- Gold,
- lives,
- skill usage,
- profile,
- settings

gibi temel bilgiler local olarak çalışmalıdır.

Online bağlantı gameplay için sürekli zorunlu olmamalıdır.

---

# 135. Local Save

Temel oyuncu ilerlemesi local olarak saklanmalıdır.

Saklanabilecek temel veriler:

```text id="oz2l5f"
Current Level
Gold
Lives
Life Timer
Skill Uses
Unlocked Skills
Username
Selected Avatar
Selected Frame
Settings
No Ads Purchase State
```

Exact save formatı implementation aşamasında seçilebilir.

---

# 136. Cloud Sync

İnternet geri geldiğinde local progress cloud'a gönderilir.

Temel örnek:

```text id="h8l4bq"
Cloud:
Level 10

Offline:
Level 12

Reconnect:
Local Level 12
      ↓
Cloud Level 12
```

---

# 137. Cloud Sync Conflict

MVP'de karmaşık merge sistemi bulunmayacaktır.

Ana karar:

**Latest local progress wins.**

Yani offline oynanan en güncel local progression cloud'a yazılır.

---

# 138. Cloud Sync Kapsamı

Sync edilebilecek temel veriler:

- Current Level
- Gold
- Lives/state
- Skill inventory/uses
- Username
- Avatar
- Frame
- Relevant progression

Exact backend data model implementation aşamasında belirlenebilir.

---

# 139. Login / Account

MVP'de:

**Login/account system yoktur.**

Bu nedenle:

- email login,
- password,
- social login,
- account registration

gibi sistemler yapılmayacaktır.

---

# 140. Cross-Device

Cross-device account progression MVP kapsamı dışındadır.

Oyunun hedefi:

**tek cihaz üzerinde local + cloud persistence**

sağlamaktır.

---

# 141. Leaderboard Offline Cache

Leaderboard verisi online iken alınabilir.

Oyuncu offline olduğunda:

**son başarılı leaderboard snapshot**

gösterilir.

Bu veri local cache'te tutulabilir.

---

# 142. Leaderboard Reconnect

Oyuncu offline ilerledikten sonra internet geldiğinde:

1. Local progress okunur.
2. Cloud progress güncellenir.
3. Leaderboard progression güncellenir.
4. Gerekirse yeni leaderboard snapshot alınır.

---

# 143. First Launch State

Yeni oyuncu için başlangıç durumu:

```text id="s4ml5c"
Level = 1
Lives = 5
Skill 1 = Locked
Skill 2 = Locked
```

Gold miktarı configurable başlangıç değeridir.

Avatar/frame:

- 8 avatar içinden başlangıç seçimi yapılabilir,
- 4 frame içinden seçim yapılabilir,
- tüm seçenekler başlangıçta available'dır.

---

# 144. Progression State

Örnek:

```text id="m6d1tb"
Level 1–4
→ Base content

Level 5
→ New block
→ Mega Ball unlocked

Level 10
→ New block
→ Infinity Ball unlocked
→ Username required

Level 15
→ New block

Level 20
→ New block

Level 25
→ New block

Level 30
→ Final level
```

---

# 145. Kullanıcı Tarafından Netleştirilen Kritik Kurallar

Aşağıdaki noktalar konuşma sırasında özellikle doğrulanmıştır:

### Cloud Sync

- Offline progress local tutulur.
- Reconnect olduğunda cloud'a sync edilir.
- Latest local progress wins.
- Offline leaderboard cached veri gösterir.

### New Block Progression

- Her 5 levelde yeni block/object/content eklenmesi kesin MVP kararıdır.

### No Ads

- Sadece mandatory/interstitial ads kaldırılır.
- Rewarded ads aktif kalır.

### Level 30

- Normal Win Panel açılır.
- Next Level yoktur.
- Main Menu/uygun completion aksiyonu sunulur.

---

# 146. Kullanıcıdan Alınan İlk Gameplay Gözlemlerinin Özeti

İlk gözlemde belirtilen temel noktalar:

- Mortar/cannon altta.
- Toplar yukarıdaki objelere gönderiliyor.
- Amaç stand üzerindeki tüm objeleri temizlemek.
- Obje çeşitleri fiziksel davranış açısından farklı.
- Box hafif.
- Stone/column daha ağır.
- Paint bottle kırılabilir.
- Bazı level'larda objelerin arkasında ikinci sıra bulunuyor.
- Toplar sınırlı.
- Toplar bitmeden obje temizlenmeli.
- Top objeye çarpınca sekebilir.
- Top boşluğa ateşlenebilir.
- Top bir süre sonra yok olur.
- Objeler anında pop olarak yok olmamalı.
- Fizik oyunun ana eğlence unsurudur.

---

# 147. İlk Gözlemden Sonra Netleştirilen Kontrol Sistemi

Kontrol sistemi:

```text id="p3tq4r"
DRAG
 ↓
Move Mortar / Aim

RELEASE / TAP
 ↓
Fire Ball
```

Ancak temel önemli detay:

**Tap hedefi world-coordinate bazında belirlenir.**

Bu nedenle implementation yalnızca:

```text
screen tap → angle
```

yaklaşımına indirgenmemelidir.

Daha doğru yaklaşım:

```text
screen tap
    ↓
camera ray
    ↓
world position
    ↓
target point
    ↓
projectile trajectory
```

olmalıdır.

Exact implementation daha sonra seçilebilir.

---

# 148. Fiziksel Puzzle'ın Ana Değer Önerisi

Oyunun temel value proposition'ı:

> Oyuncu sınırlı sayıdaki atışını kullanarak fiziksel objeler arasında zincirleme reaksiyon oluşturur ve bütün yapıyı devirmeye çalışır.

Bu nedenle gameplay:

- skill kullanımından,
- shop sisteminden,
- monetization'dan

daha önemli bir çekirdek sistemdir.

Öncelik:

**Physics + Aim + Object Interaction + Level Design**

olmalıdır.

---

# 149. Gereksiz Complexity'den Kaçınma

MVP'de aşağıdaki sistemler gereksiz yere karmaşıklaştırılmamalıdır:

- gelişmiş account sistemi,
- cross-device profile,
- karmaşık cloud conflict resolution,
- gelişmiş daily reward,
- chest sistemi,
- level select,
- replay,
- star rating,
- karmaşık ranking algoritması,
- çok sayıda map,
- çok sayıda skill,
- karmaşık wall bounce.

Amaç:

**oynanabilir ve polish edilmiş 30 level'lık çekirdek deneyimdir.**

# Royal Smash! - Physics Puzzle
# MVP Game Documentation
## Part 5A — MVP Scope, Gameplay Requirements & Technical Specification

---

# 150. MVP Scope

Bu dokümanda tanımlanan MVP'nin amacı, **Royal Smash! - Physics Puzzle** benzeri physics-based casual mobile puzzle deneyimini baştan sona oynanabilir şekilde tanımlamaktır.

MVP'nin temel kapsamı:

- 30 playable level
- Physics-based projectile gameplay
- Mortar/cannon aiming
- World-coordinate targeting
- Limited ammunition
- Physics-based object interaction
- Multiple object types
- Object destruction
- Paint bottle special destruction
- Win/Lose system
- Lives system
- Two skills
- Gold economy
- Shop
- Ads
- No Ads purchase
- Main Menu
- Profile
- Leaderboard
- Settings
- Local save
- Offline gameplay
- Cloud synchronization
- Basic VFX
- Basic SFX
- Haptic feedback

---

# 151. MVP'de Olmayacak Sistemler

Aşağıdaki sistemler MVP kapsamı dışındadır:

- Login / registration
- Email account
- Social login
- Cross-device progression
- Level selection
- Level replay
- Star rating
- Efficiency rating
- Daily chest
- Daily reward
- Complex achievement system
- Multiple currencies
- Multiple maps with independent progression
- Advanced clan/social system
- Multiplayer
- PvP
- Complex wall-bounce mechanics
- Large skill system
- Complex inventory system

Bunlar ileride eklenebilir ancak MVP'nin temel oynanışını oluşturmamalıdır.

---

# 152. Core Gameplay Priority

Master build prompt hazırlanırken sistemlerin öncelik sırası:

```text
1. Physics
2. Projectile / Ball
3. Aim & Targeting
4. Object Interaction
5. Level Completion
6. Level Failure
7. Level Progression
8. Lives
9. Skills
10. Economy
11. UI
12. Ads / Monetization
13. Profile
14. Leaderboard
15. Cloud / Offline
16. Polish
```

Özellikle ilk 6 madde doğru çalışmadan monetization veya sosyal sistemlere odaklanılmamalıdır.

---

# 153. Physics Acceptance Criteria

Physics sistemi başarılı kabul edilebilmesi için:

- Ball gerçekten fiziksel impulse ile hareket etmelidir.
- Ball objeye çarptığında objeyi hareket ettirebilmelidir.
- Hafif objeler daha kolay itilmelidir.
- Ağır objeler daha zor itilmelidir.
- Birden fazla obje birbirini etkileyebilmelidir.
- Stack halinde objeler fiziksel olarak davranmalıdır.
- Objeler düşebilmeli/devrilebilmelidir.
- Ball sekebilmeli ve geri gelebilmelidir.
- Ball boşluğa atıldığında dünyada gerçekten ilerlemelidir.
- Ball belirli bir süre sonra yok olmalıdır.
- Physics sonucu doğal görünmelidir.
- Gameplay hissi yalnızca arcade projectile sistemi gibi olmamalıdır.
- Physics oyunun ana puzzle mekaniklerinden biri olmalıdır.

---

# 154. Aiming Acceptance Criteria

Aim sistemi:

- Tap ile çalışmalıdır.
- Drag sırasında mortar takip etmelidir.
- Drag sırasında yanlışlıkla ateş olmamalıdır.
- Mortar gameplay sınırları içerisinde kalmalıdır.
- Oyuncunun tap ettiği world coordinate hedeflenmelidir.
- Kamera açısı değiştiğinde targeting mantığı bozulmamalıdır.
- Top hedef noktaya doğru fiziksel trajectory ile gönderilmelidir.
- Oyuncu standın ulaşılabilir olmayan bölgelerine anlamsız şekilde ateş edememelidir.

---

# 155. Input Priority

Input sistemi için ana davranış:

```text
Tap
 ↓
Fire

Hold + Drag
 ↓
Move/Aim
 ↓
Release
 ↓
No automatic fire
```

Bu davranış net şekilde ayrılmalıdır.

---

# 156. Fire Cooldown

Normal ball kullanımında ateşler arasında kısa cooldown vardır.

Yaklaşık:

```text
~0.5 sec
```

Bu değer configurable olmalıdır.

Infinity Ball aktifken normal cooldown geçici olarak kaldırılır.

---

# 157. Projectile Lifetime

Normal ball için yaklaşık:

```text
Lifetime ≈ 5 sec
```

kullanılabilir.

Bu değer:

```text
CONFIGURABLE
```

olmalıdır.

Örneğin son saniyede:

- fade,
- dissolve,
- destroy effect

başlatılabilir.

---

# 158. Ball Count

Level başına ball sayısı yaklaşık:

```text
20–30
```

aralığındadır.

Kesin değer level bazında configurable olmalıdır.

Örneğin:

```text
LevelData:
    ballCount
```

gibi bir parametre bulunabilir.

---

# 159. Object Parameters

Her obje için en azından aşağıdaki parametrelerin configurable olması faydalıdır:

```text
Mass
Scale
Health / Durability
Breakable
Movement
Physics Material
Friction
Bounciness
Destroy Behaviour
Destroy Effect
Impact Sound
```

Bunlar tek bir global değer yerine obje türüne göre değişebilmelidir.

---

# 160. Box Parameters

Box:

- hafif/orta ağırlıkta,
- hareketli,
- fiziksel,
- gerektiğinde destroy edilebilir

olmalıdır.

Örnek:

```text
Mass = configurable
Breakable = true/optional
DestroyAnimation = shrink/melt
```

Exact değerler level tuning sırasında belirlenebilir.

---

# 161. Stone Parameters

Stone:

- Box'tan daha ağır,
- daha zor hareket eden,
- güçlü impact alan,
- gerektiğinde devrilebilen

bir obje olmalıdır.

Örnek:

```text
Mass > Box
ImpactSound = Heavy
```

---

# 162. Column Parameters

Column:

- dikey,
- devrilmeye uygun,
- ağır,
- diğer objeleri etkileyebilen

bir obje olarak kullanılabilir.

Column'ın önemli gameplay amacı:

**devrilerek başka objelere zincirleme etki yaratmak.**

---

# 163. Cone Parameters

Cone:

- daha hafif,
- kolay hareket edebilen,
- farklı collision profile'a sahip

bir obje olabilir.

Level design'da variety oluşturmak için kullanılmalıdır.

---

# 164. Paint Bottle Parameters

Paint Bottle:

- kırılabilir,
- cam benzeri fiziksel davranış,
- impact sonrası özel VFX,
- paint spill

içerir.

Temel state:

```text
Intact
   ↓
Impact
   ↓
Broken
   ↓
Paint Spill
   ↓
Destroyed
```

---

# 165. Object State Machine

Genel obje state modeli:

```text
IDLE
 ↓
HIT
 ↓
MOVING / FALLING
 ↓
DESTROY CONDITION
 ↓
DESTROYING
 ↓
DESTROYED
```

Bazı objelerde `BREAK` state'i bulunabilir.

---

# 166. Level Data

Her level için data-driven yapı tercih edilmelidir.

Level aşağıdaki bilgileri içerebilir:

```text
Level ID
Ball Count
Object List
Object Positions
Object Rotations
Object Scales
Stand Configuration
Background
Unlocked Features
Tutorial State
Reward Configuration
```

Bu sayede 30 level tek bir sahnede hard-code edilmek yerine kolayca düzenlenebilir.

---

# 167. Level Design Philosophy

Level tasarımı:

**“Oyuncunun doğru hedefi bulması”**

üzerine kurulmalıdır.

Her level yalnızca:

```text
Shoot all objects directly
```

olmamalıdır.

Bunun yerine:

```text
Shoot
 ↓
Object moves
 ↓
Object hits another object
 ↓
Chain reaction
 ↓
Multiple objects fall
```

gibi fiziksel çözüm yolları teşvik edilmelidir.

---

# 168. Level Difficulty

Difficulty aşağıdaki değişkenlerle artırılabilir:

- Daha fazla obje
- Daha ağır objeler
- Daha karmaşık stacking
- İkinci sıra/layer
- Daha zor açı
- Daha az ball
- Daha karmaşık chain reaction
- Daha hassas hedefleme
- Yeni obje türleri

Difficulty yalnızca obje sayısını artırarak oluşturulmamalıdır.

---

# 169. Level 1–4

İlk dört level temel sistemleri öğretmelidir.

Öncelik:

- Aim
- Fire
- Ball physics
- Object movement
- Win condition

Bu seviyelerde puzzle complexity düşük olmalıdır.

---

# 170. Level 5

Level 5:

- Yeni block/object/content getirir.
- Mega Ball unlock olur.

Oyuncuya Mega Ball'ın ne işe yaradığı doğal şekilde gösterilebilir.

---

# 171. Level 6–9

Yeni content kullanılarak temel mechanic daha fazla test edilir.

Amaç:

**Level 5'te öğrenilen yeni içeriği oyuncuya kullandırmak.**

---

# 172. Level 10

Level 10:

- Yeni block/object/content
- Infinity Ball unlock
- Username/nickname interaction

içerir.

Bu seviye ikinci büyük progression milestone'dur.

---

# 173. Level 11–14

Yeni block ve Infinity Ball mechanics'i kullanılabilir.

Infinity Ball'ın avantajı:

**normal fire cooldown'ını geçici olarak kaldırmak.**

---

# 174. Level 15

Level 15:

- Yeni block/object/content

getirir.

Difficulty önceki seviyelere göre yükseltilebilir.

---

# 175. Level 20

Level 20:

- Yeni block/object/content

getirir.

Physics interaction daha karmaşık hale getirilebilir.

---

# 176. Level 25

Level 25:

- Yeni block/object/content

getirir.

Oyuncu artık temel sistemlerin tamamına hakim olmalıdır.

---

# 177. Level 26–29

Final level öncesi progression.

Bu seviyelerde:

- multiple object combinations,
- layered structures,
- limited ball usage,
- skill usage

daha yoğun şekilde kullanılabilir.

---

# 178. Level 30

Level 30:

- MVP'nin final level'ıdır.
- Normal gameplay kurallarını kullanır.
- Obje temizlenince normal Win Panel açılır.
- Next Level butonu gösterilmez.
- Oyuncu Main Menu'ye dönebilir veya uygun completion action kullanabilir.

---

# 179. Tutorial Acceptance Criteria

Tutorial:

- yalnızca ilk launch'ta gösterilir.
- normal gameplay map'i kullanır.
- belirli bir bölgeyi işaret eder.
- oyuncudan o bölgeye tap etmesini ister.
- ilgili obje düşer.
- ardından kontrol oyuncuya bırakılır.
- tutorial alanı dışındaki tap'ler disabled olabilir.
- ilk ball hazırdır.

Tutorial:

**oyuncunun gerçek gameplay sistemini öğrenmesini sağlamalıdır.**

---

# 180. Victory Acceptance Criteria

Win:

```text
All required objects destroyed / cleared
        ↓
Short delay
        ↓
Win Panel
```

Final obje düştükten sonra yaklaşık:

```text
~2 sec
```

beklenebilir.

Bu değer configurable olmalıdır.

---

# 181. Win Reward

Ana reward:

```text
Remaining Balls × 2 Gold
```

Örnek:

```text
15 balls remaining
15 × 2
= 30 Gold
```

Bu formül MVP'de sabit tasarım kararıdır.

---

# 182. Lose Acceptance Criteria

Lose:

```text
Balls = 0
AND
Objects remain
```

durumunda tetiklenir.

Lose panel:

- Retry
- Main Menu
- Remaining lives

gibi bilgileri gösterebilir.

Life:

```text
-1
```

olur.

---

# 183. Exit Acceptance Criteria

Aktif leveldan çıkmak:

**life kaybettirir.**

Önerilen akış:

```text
Exit
 ↓
Confirmation Popup
 ↓
Confirm
 ↓
-1 Life
 ↓
Main Menu
```

Cancel seçilirse level devam eder.

---

# 184. App Close Rule

Oyuncu aktif level sırasında uygulamayı kapatırsa:

**exit olarak değerlendirilir.**

Amaç:

**uygulamayı kapatarak life kaybından kaçınma exploit'ini engellemektir.**

---

# 185. Life System

Başlangıç:

```text
5 Lives
```

Maximum:

```text
5
```

olarak düşünülebilir.

Life kazanımı:

```text
+1 every 30 minutes
```

Bu değer configurable olmalıdır.

---

# 186. Zero Lives

Lives:

```text
0
```

olduğunda oyuncu normal level başlatamaz.

Alternatif:

- Gold ile life satın alma
- Rewarded ad ile life kazanma

sunulabilir.

---

# 187. Skill System

MVP'de iki skill vardır:

```text
Mega Ball
Infinity Ball
```

---

# 188. Mega Ball

Mega Ball:

- daha büyük projectile,
- daha güçlü fiziksel etki,
- normal ball'dan belirgin şekilde daha büyük

olmalıdır.

Örnek:

```text
Normal Ball Diameter = 10
Mega Ball Diameter = 20
```

Bu değerler yalnızca örnektir ve configurable'dır.

Unlock:

```text
Level 5
```

Başlangıç kullanım hakkı:

```text
3
```

---

# 189. Infinity Ball

Infinity Ball:

**geçici olarak normal fire cooldown'ını kaldırır.**

Unlock:

```text
Level 10
```

Başlangıç kullanım hakkı:

```text
3
```

Örnek duration:

```text
~5 sec
```

Bu değer configurable'dır.

---

# 190. Skill Stacking

Mega Ball ve Infinity Ball aynı anda aktif olabilir.

Örneğin:

```text
Mega Ball = Active
Infinity Ball = Active
```

Bu durumda:

- projectile büyük olur,
- cooldown kaldırılmış olur.

---

# 191. Skill Consumption

Skill kullanımı:

```text
Tap Skill
 ↓
Activate
 ↓
Consume 1 use
```

Extra uses shop üzerinden alınabilir.

Skill inventory negatif olmamalıdır.

---

# 192. Skill Locked State

Skill unlock edilmeden önce:

```text
[ 🔒 ]
```

gibi locked state gösterilebilir.

Unlock olduğunda:

```text
[ ICON ]
[ COUNT ]
```

gösterilebilir.

---

# 193. Depleted Skill State

Skill usage:

```text
0
```

olduğunda:

- price,
- shop shortcut,
- purchase option

gösterilebilir.

Exact UI daha sonra belirlenebilir.

---

# 194. Performance Requirement

MVP'de aynı anda çok yüksek sayıda rigidbody bulunması gerekmez.

Level'lar yaklaşık:

- 20–30 projectile,
- sınırlı sayıda object

içerebilir.

Physics optimization buna göre yapılmalıdır.

Gereksiz:

- sürekli Instantiate/Destroy spam,
- ağır particle,
- gereksiz physics calculations

engellenmelidir.

---

# 195. Object Pooling

Projectile sayısı fazla olmadığı için pooling zorunlu değildir.

Ancak implementation sırasında performans açısından avantajlı görülürse:

**ball/projectile pooling**

kullanılabilir.

Bu bir MVP gameplay requirement değil, teknik optimizasyondur.

---

# 196. Data-Driven Level Design

Level'ların kod içine gömülmesi yerine configurable data yaklaşımı tercih edilmelidir.

Örneğin her level:

```text
LevelData
```

olarak tanımlanabilir.

LevelData:

```text
levelId
ballCount
background
standType
objects[]
```

gibi bilgiler içerebilir.

---

# 197. Vertical Slice Requirement

Tüm 30 level yapılmadan önce:

**1 adet fully playable vertical slice**

oluşturulmalıdır.

Vertical slice şunları içermelidir:

- Main Menu
- Level start
- Mortar
- Aim
- Ball
- Physics
- Object destruction
- Win
- Lose
- Life deduction
- Retry
- Basic UI
- Basic SFX/VFX

Bu slice başarılı olduktan sonra sistem 30 level'a genişletilmelidir.

---

# 198. Prototype vs Final

Prototype aşamasında:

- primitive meshes,
- placeholder textures,
- basic particles,
- simple UI

kullanılabilir.

Ancak final MVP'de:

- polished UI,
- coherent visual style,
- object-specific VFX,
- object-specific SFX,
- responsive haptics

eklenmelidir.

---

# 199. Placeholder Asset Rule

Eksik asset varsa AI:

**asset varmış gibi davranmamalıdır.**

Bunun yerine:

```text
PLACEHOLDER
```

kullanmalıdır.

Örneğin:

```text
Placeholder Box
Placeholder Stone
Placeholder Bottle
Placeholder Avatar
Placeholder Logo
```

Sonradan gerçek assetlerle değiştirilebilmelidir.

---

# 200. Visual Style

Görsel stil:

- mobile-friendly,
- temiz,
- okunabilir,
- renkli,
- casual puzzle

hissinde olabilir.

Ancak exact:

- color palette,
- shader,
- lighting,
- texture style

bu dokümanda kesinleştirilmemiştir.

Bunlar configurable/design decision olarak bırakılmalıdır.

---

# 201. UI Philosophy

UI:

- sade,
- hızlı anlaşılır,
- mobile touch için uygun,
- gameplay'i engellemeyen

olmalıdır.

Özellikle gameplay sırasında UI mümkün olduğunca az dikkat dağıtmalıdır.

---

# 202. Mobile Requirement

Oyun mobil cihaz odaklıdır.

Bu nedenle:

- touch input,
- farklı aspect ratio,
- safe area,
- UI scaling

dikkate alınmalıdır.

Exact orientation bu dokümanda kesinleştirilmemiştir ve implementation aşamasında belirlenebilir.

---

# 203. World Coordinate Requirement

Bu sistem özellikle kritik kabul edilmelidir.

Oyuncu ekranda bir noktaya dokunduğunda:

```text
Screen Point
 ↓
Camera Ray
 ↓
World Position
 ↓
Target Point
 ↓
Ball Trajectory
```

oluşmalıdır.

Sadece mouse/touch X pozisyonundan açı hesaplamak yeterli değildir.

---

# 204. Empty-Space Trajectory

Oyuncu objenin olmadığı boş bir noktaya ateş edebilir.

Bu durumda ball:

- arka plana,
- gameplay world'ünün derinliğine

doğru ilerlemelidir.

Ball:

**sanki görünmez bir 2D çizgiye çarpıp aniden kayboluyormuş gibi davranmamalıdır.**

---

# 205. Projectile Boundary

Ball belirli koşullardan biri gerçekleştiğinde destroy edilebilir:

```text
Lifetime exceeded
OR
Out of gameplay bounds
OR
Explicit cleanup condition
```

Ancak gameplay hissi bozulmaması için lifetime yaklaşık 5 saniye civarında tutulabilir.

---

# 206. Level Completion Detection

Level tamamlanma sistemi yalnızca:

```text
object count == 0
```

gibi basit bir koşula bağlı olmak zorunda değildir.

Fiziksel obje:

- düşmüş,
- stand dışına çıkmış,
- destroy olmuş

gibi durumlar da “cleared” olarak kabul edilebilir.

Kesin detection yöntemi implementation sırasında seçilebilir.

---

# 207. Physics Settling

Level completion kontrolünde fizik sisteminin kısa süre settle olması gerekebilir.

Örneğin:

```text
Last object falls
 ↓
Physics settles
 ↓
Completion confirmed
 ↓
~2 sec delay
 ↓
Win Panel
```

Bu davranış yanlışlıkla erken Win oluşmasını engeller.

---

# 208. Final Part 5A Boundary

Bu bölümde:

- MVP scope
- physics
- aiming
- projectile
- objects
- level structure
- skills
- performance
- vertical slice
- placeholder assets
- visual/UI requirements
- world-coordinate targeting

kapsanmıştır.


---

# 209. Final Ekonomi Spesifikasyonu

MVP ekonomisi kasıtlı olarak sade ve anlaşılır tutulmuştur.

Birincil soft currency (oyun içi para birimi):

**Altın (Gold)**

Altın şu alanlarda kullanılır:

- Skill (yetenek) kullanımı
- Ekstra can satın alma
- Dükkan paketleri (Shop bundles)
- Skill kullanım paketleri
- Altın + Skill paketleri
- Diğer yapılandırılabilir (configurable) gelir/teklif modelleri

Ekonomi, MVP kapsamında gereksiz ikincil para birimleri (hard currency vb.) içermemelidir.

---

# 210. Başlangıç Altını

Oyuncu oyuna belirli bir başlangıç altını ile başlayabilir.

Örnek:

```text
Başlangıç Altını: 1000
```

Bu değer yapılandırılabilirdir (configurable).

Kesin prodüksiyon değeri daha sonra dengelenebilir (tune edilebilir).

---

# 211. Seviye Tamamlama Altını

Bir seviyeyi tamamlamak temel bir Altın ödülü kazandırabilir.

Örnek:

```text
Temel Seviye Ödülü: 50 Altın
```

Bu değer yapılandırılabilirdir.

---

# 212. Kalan Top Ödülü

MVP için kesinleşen formül şudur:

```text
Kalan Top Sayısı × 2 = Bonus Altın
```

Örnek:

```text
15 top kaldı
15 × 2 = 30 Altın
```

Oyuncu bu bonusu seviyeyi başarıyla tamamladıktan sonra kazanır.

---

# 213. Toplam Seviye Ödülü

Toplam seviye ödülü formülü şu şekilde ifade edilir:

```text
Toplam Altın Ödülü =
Temel Seviye Ödülü
+
(Kalan Top Sayısı × 2)
```

Örnek:

```text
Temel Ödül: 50
Kalan Top: 15
Top Bonusu: 30

Toplam: 80 Altın
```

Kesin temel ödül değeri yapılandırılabilir kalmaya devam eder.

---

# 214. Altın Satın Alma

Altın gerçek para (IAP) ile satın alınabilir.

Kesinleşmemiş değerler:

- Fiyat
- Para birimi
- Mağaza ürün ID'si (Store product ID)
- Altın miktarı
- Bölgesel fiyatlandırma
- Platforma özel konfigürasyonlar

uygulama ve gelir modeli kararlarıdır; yapılandırılabilir (configurable) olmalıdır.

---

# 215. Dükkan MVP

Dükkan (Shop) şunları içermelidir:

- Altın paketleri
- Skill kullanım paketleri
- Altın + Skill paketleri (Bundle)
- Süreli teklifler (Limited-time offers)
- Reklam Kaldırma (No Ads) satın alımı

Dükkan görsel olarak sade ve kolay anlaşılır kalmalıdır.

---

# 216. Altın Paketleri

Altın paketleri, gerçek para karşılığında farklı miktarlarda Altın sağlar.

Örnek yapı:

```text
Küçük Altın Paketi (Small)
Orta Altın Paketi (Medium)
Büyük Altın Paketi (Large)
Premium Altın Paketi
```

Kesin miktarlar ve fiyatlar yapılandırılabilirdir.

---

# 217. Skill Paketleri

Skill paketleri, oyuncuların kilidi açılmış yetenekleri için ek kullanım hakları satın almasını sağlar.

Örnek:

```text
Mega Ball +X Kullanım
Infinity Ball +X Kullanım
```

Kesin değerler yapılandırılabilirdir.

---

# 218. Altın + Skill Paketleri

Dükkan, şu ikisini birleştiren paketler içerebilir:

```text
Altın
+
Skill Kullanım Hakları
```

Bu paketler oyuncuya daha avantajlı teklifler olarak sunulabilir.

Kesin içerik ve fiyatlar yapılandırılabilirdir.

---

# 219. Süreli Teklifler

Dükkan süreli özel bir teklif içerebilir.

Örnek:

```text
Teklif süresi: 24 saat
```

Teklif, süresi dolduktan sonra yenilenebilir veya döngüye girebilir.

Kesin teklif içerikleri yapılandırılabilirdir.

---

# 220. Günlük Giriş Ödülü

Günlük giriş ödülü (Daily Reward) **MVP için zorunlu değildir**.

Daha sonra eklenebilir.

Uygulama, temel oyun döngüsünü günlük ödül sistemine bağımlı kılmamalıdır.

---

# 221. Sandık Sistemi

Ayrı bir sandık/ödül açma sistemi **MVP için zorunlu değildir**.

Daha sonra açıkça talep edilmedikçe uygulanmamalıdır.

---

# 222. Reklam Kaldırma (No Ads)

Oyuncu şu ürünü satın alabilir:

```text
Reklam Kaldırma (No Ads)
```

No Ads şunları kaldırır:

- Zorunlu / Geçiş reklamları (Mandatory / Interstitial ads)

No Ads şunları **kaldırmaz**:

- Ödüllü reklamlar (Rewarded ads)

Ödüllü reklamlar, oyuncunun kendi isteğiyle başlattığı ödül odaklı etkileşimler olduğu için aktif kalmaya devam eder.

---

# 223. Reklam Sistemi

MVP iki ana reklam kategorisi içerebilir:

```text
Zorunlu / Geçiş Reklamları (Interstitial)
Ödüllü Reklamlar (Rewarded)
```

Zorunlu reklamlar uygun oyun olaylarından sonra (örneğin seviye bitişi veya kaybı) gösterilebilir.

Ödüllü reklamlar ise doğrudan oyuncunun tercihi ve etkileşimiyle başlar.

---

# 224. Ödüllü Reklamlar

Ödüllü reklamlar şu avantajlar için kullanılabilir:

- Can kurtarma / yenileme (Recovering a life)
- İleride eklenebilecek opsiyonel ödüller

Oyuncu ödüllü reklamı izlemeyi bilinçli olarak seçmelidir.

Ödül, yalnızca reklam başarıyla tamamlandığında ve doğrulandığında verilmelidir.

---

# 225. Reklam Güvenliği

Uygulama şunları kesinlikle engellemelidir:

- Çift ödül verilmesi
- Yinelenen can yenileme
- Mükerrer Altın ödülleri
- İptal edilen veya başarısız olan reklamlara ödül verilmesi
- Aynı reklam tamamlanmasının birden fazla kez ödüllendirilmesi

Ödül işlemleri mümkün olduğunca idempotent (tekil ve güvenli) olmalıdır.

---

# 226. Ana Menü

Ana Menü merkezi navigasyon ekranıdır.

Ana yerleşim alanları:

```text
Sol üst:
Profil (Profile)

Üst orta / üst alan:
Altın (Gold)
Canlar / Hak (Lives / Health)

Sağ üst:
Ayarlar (Settings)

Merkez:
Mevcut Seviye / Başlat (Current Level / Start)

Alt kısım:
Dükkan (Shop)
Ana Menü (Main Menu)
Lider Tablosu (Leaderboard)
```

Özel teklifler ana içerik alanının sol veya sağ tarafında gösterilebilir.

---

# 227. Mevcut Seviye Gösterimi

Ana Menü, oyuncunun mevcut seviyesini net bir şekilde göstermelidir.

Örnek:

```text
LEVEL 12
OYNA (PLAY)
```

Oyuncu ana oynanış butonuna bastığında ne olacağını anında anlamalıdır.

---

# 228. Ana Menü Özel Teklifleri

Özel teklifler ana oynanış alanının çevresinde gösterilebilir.

Bunlar şunlarla bağlantılı olabilir:

- Dükkan
- Süreli paketler
- Skill teklifleri
- Altın teklifleri

Bu teklifler birincil Oyna (Play) butonunun kullanımını engellememelidir.

---

# 229. Alt Navigasyon

MVP alt navigasyon barı şunları içerir:

```text
Dükkan (Shop)
Ana Menü (Main Menu)
Lider Tablosu (Leaderboard)
```

Ana Menü merkezi ve varsayılan sekmedir.

---

# 230. Profil Sistemi

Profil ekranı şunları içerir:

- Avatar
- Kullanıcı adı (Username)
- Mevcut seviye
- Genel oyuncu bilgileri
- İsteğe bağlı genel istatistikler

Detaylı istatistikler MVP için gerekli değildir.

---

# 231. Profil Düzenleme

Oyuncu şunları düzenleyebilir:

- Kullanıcı adı (Username)
- Avatar
- Avatar çerçevesi (Frame)

Değişiklikler yerel olarak kaydedilmelidir (local save).

---

# 232. Avatar Sistemi

MVP yaklaşık olarak şu içeriği barındırır:

```text
8 varsayılan avatar
```

Tüm MVP avatarları baştan itibaren erişilebilirdir.

Avatar açma (progression) sistemi yoktur.

Şunlar için hiçbir gereksinim yoktur:

- Galeriden resim yükleme
- Kameradan resim yükleme
- Özel resim yükleme

---

# 233. Avatar Çerçeve Sistemi

MVP yaklaşık olarak şu içeriği barındırır:

```text
4 çerçeve
```

Tüm çerçeveler varsayılan olarak açık olabilir.

Çerçeveler, avatarın etrafındaki görsel süslemelerdir.

Şu açılardan farklılık gösterebilir:

- Renk
- Şekil
- Süsleme detayları
- Görsel stil

---

# 234. Kullanıcı Adı

Oyuncu bir kullanıcı adı belirleyebilir.

İlgili ilerleme noktasına ulaşıldığında (örneğin Seviye 10) oyuncudan bir takma ad/kullanıcı adı girmesi istenir.

Kullanıcı adı sistemi MVP'de karmaşık bir hesap altyapısına bağımlı olmamalıdır.

---

# 235. Kullanıcı Adı Değişikliği

Kullanıcı adı değişiklikleri MVP'de sınırlandırılmamıştır.

Oyuncu kullanıcı adını birden fazla kez değiştirebilir.

Kötüye kullanımı engelleme (rate-limiting) sistemleri gerekirse daha sonra prodüksiyon aşamasında eklenebilir.

---

# 236. Lider Tablosu

MVP bir lider tablosu içerir.

Lider tablosu kayıtları şunları içerir:

- Sıralama (Rank)
- Avatar / profil resmi
- Kullanıcı adı
- Seviye (Level)

Oyuncular öncelikli olarak şuna göre sıralanır:

```text
Mevcut Seviye (Current Level)
```

---

# 237. Lider Tablosu Boyutu

Görünür lider tablosu veri seti şunu destekleyebilir:

```text
İlk 1000 (Top 1000)
```

Kesin backend uygulaması yapılandırılabilirdir (configurable).

---

# 238. Lider Tablosu Kapsamı

Lider tablosu küresel veya yerel bir rekabet tablosu olarak sunulabilir.

MVP, gerçek bir çok oyunculu (multiplayer) altyapı gerektirmeden oyuncuya paylaşılan bir sıralama deneyimi sunmalıdır.

Gerçek zamanlı PvP gereksinimi yoktur.

---

# 239. Çevrimiçi Lider Tablosu

Oyuncunun internet bağlantısı olduğunda:

- Güncel lider tablosu verileri çekilmeli / senkronize edilmelidir.
- Oyuncunun ilerlemesi tabloda uygun şekilde temsil edilmelidir.
- Önbelleğe alınan veriler, başarılı yeni verilerle güncellenmelidir.

---

# 240. Çevrimdışı Lider Tablosu

Çevrimdışıyken:

- Başarıyla senkronize edilen son lider tablosu verisi gösterilebilir.
- Oyunun ana oynanışı kesinlikle internet bağlantısı gerektirmemelidir.

Çevrimdışı lider tablosu verileri eski (stale) olabilir.

Arayüz, eski verileri garanti canlı veriymiş gibi yansıtmamalıdır.

---

# 241. Çevrimdışı İlerleme Senkronizasyonu

Oyun çevrimdışı ilerlemeyi tam olarak destekler.

Örnek:

```text
Bulut:
Seviye 10

Oyuncu çevrimdışı olur.

Oyuncu şu seviyeleri tamamlar:
Seviye 11
Seviye 12

Oyuncu tekrar internete bağlanır.

Bulut şu değere güncellenir:
Seviye 12
```

MVP için kesinleşen kural şudur:

```text
En son yerel ilerleme geçerlidir (Latest local progress wins).
```

---

# 242. Bulut Kayıt

Oyun, internet bağlantısı sağlandığında oyuncunun ilgili ilerlemesini senkronize etmelidir.

Bulut senkronizasyonu şunları içerebilir:

- Mevcut seviye
- İlgili ilerleme durumu
- Ekonomi durumu (Altın)
- Skill kullanım hakları
- Profil bilgileri
- Kalıcı olması gereken diğer oyuncu verileri

Kesin backend teknolojisi bir mühendislik kararıdır.

---

# 243. Hesap Sistemi

MVP, geleneksel ve görünür bir giriş/hesap oluşturma sistemi gerektirmez.

Oyuncu karmaşık hesap işlemleriyle uğraşmadan hemen oyuna başlayabilmelidir.

---

# 244. Cihazlar Arası Senkronizasyon

Tam cihazlar arası (cross-device) hesap senkronizasyonu **MVP kapsamı dışındadır**.

MVP yalnızca tanımlanan yerel/çevrimdışı/bulut senkronizasyon davranışını gerektirir.

---

# 245. Ayarlar

Ayarlar ekranı şunları içerir:

- Müzik (Music)
- Ses Efektleri (SFX)
- Titreşim (Vibration)

Orijinal gözlemlenen uygulama basit açma/kapama (on/off) kontrolleri kullanır.

Kontrol sunumu için:

```text
Toggle (Aç/Kapa)
```

veya benzeri basit bir kontrol yeterlidir.

---

# 246. Müzik

Arka plan müziği bağımsız olarak kontrol edilebilmelidir.

Kapatıldığında:

```text
Müzik = KAPALI (OFF)
```

Oyun, sahneler ve oynanış boyunca bu tercihe sadık kalmalıdır.

---

# 247. Ses Efektleri

Ses efektleri (SFX) bağımsız olarak kontrol edilebilmelidir.

Bu ayar şunları etkiler:

- Top ateşleme
- Çarpışmalar
- Obje yıkımları
- Cam kırılması
- Diğer oynanış geri bildirimleri

---

# 248. Titreşim

Titreşim ve dokunsal geri bildirim (haptic feedback) bağımsız olarak kontrol edilebilmelidir.

Kapatıldığında oynanış titreşim tetiklememelidir.

---

# 249. Ses Tasarımı

Oyun, ses yoluyla net bir oynanış geri bildirimi sunmalıdır.

Önemli SFX kategorileri:

```text
Top Ateşleme (Ball Fire)
Obje Çarpışması (Object Impact)
Kutu / Ağır Obje Çarpışması
Taş Çarpışması
Cam Kırılması (Glass Break)
Obje Yıkımı (Destruction)
Arayüz Etkileşimi (UI Interaction)
Zafer (Victory)
Yenilgi (Failure)
```

Kesin ses varlıkları yapılandırılabilirdir.

---

# 250. Objelere Özel Sesler

Farklı obje kategorileri ses yoluyla farklı hissettirmelidir.

Örnekler:

```text
Kutu (Box):
ahşap/karton benzeri darbe sesi

Taş (Stone):
ağır/tok ve sert darbe sesi

Boya Şişesi (Paint Bottle):
cam kırılması + sıvı dökülme sesi
```

Kesin ses tasarımı geliştirme aşamasında netleştirilebilir.

---

# 251. Görsel Efektler (VFX)

Oyun, önemli oynanış olaylarını iletmek için VFX kullanmalıdır.

En azından:

- Top ateşleme
- Top çarpışması
- Obje yıkımı
- Boya şişesi kırılması
- Sıvı sıçraması
- Zafer (Victory)
- Yenilgi (Failure)

---

# 252. Top Ateşleme VFX

Oyuncu ateş ettiğinde:

- Top belirgin bir şekilde mortardan çıkmalıdır.
- Hafif bir namlu/ateş efekti gösterilebilir.
- Eylem duyarlı ve anlık hissettirmelidir.

Efekt, merminin hareket rotasını (trajectory) engellememelidir.

---

# 253. Çarpışma VFX

Top bir objeye çarptığında:

- Çarpışma geri bildirimi görünür olmalıdır.
- Efekt, uygun durumlarda obje türüne göre uyarlanmalıdır.

Ağır objeler daha belirgin ve tok geri bildirim efektlerine sahip olabilir.

---

# 254. Obje Yıkım VFX

Normal objelerin aniden kaybolması gerekmez.

Obje türüne bağlı olarak şunlar gerçekleşebilir:

```text
Hasar alma
Dengesini kaybetme
Küçülme (Shrink)
Erime (Melt)
Solma (Fade)
Parçalanma (Break)
Düşme
Yok olma
```

---

# 255. Boya Şişesi VFX

Boya şişeleri özel bir yıkım geri bildirimine sahiptir.

Belirgin bir darbe aldığında:

```text
Şişe kırılır
+
Cam parçacık efekti
+
Sıvı/boya sıçraması
```

Dökülen sıvı kısa bir süre sahnede görsel olarak kalabilir.

---

# 256. Dokunsal Geri Bildirim (Haptics)

Haptik geri bildirim önemli etkileşimleri güçlendirmelidir.

Olası tetikleyici olaylar:

- Ateş etme
- Şiddetli darbe
- Ağır obje çarpışması
- Şişe kırılması
- Zafer
- Yenilgi

Kesin haptik şiddeti yapılandırılabilirdir.

---

# 257. Oynanış Arayüzü (Gameplay HUD)

Gameplay HUD şu öğeleri önceliklendirmelidir:

```text
Kalan Top Sayısı
Skill'ler (Yetenekler)
Ayarlar Butonu
```

HUD, kritik objelerin veya atış rotalarının üzerini kapatmamalıdır.

---

# 258. Kalan Toplar Arayüzü

Oyuncu kaç normal atış hakkı kaldığını her an net olarak görebilmelidir.

Örnek:

```text
TOPLAR: 18
```

veya eşdeğer bir görsel sayaç.

Sayaç, her atıştan hemen sonra anında güncellenmelidir.

---

# 259. Skill HUD (Yetenek Arayüzü)

Her yetenek şunu göstermelidir:

```text
Skill İkonu
+
Kalan Kullanım Sayısı
```

Kilitli skill'ler kilitli durumlarını görsel olarak belli etmelidir.

Bir skill'in kullanım hakkı kalmadığında:

```text
Fiyat / Satın alma seçeneği
```

gösterilebilir.

Zamanlayıcı tabanlı skill'lerde (örneğin Infinity Ball):

```text
Aktif Geri Sayım Süresi
```

açıkça görünmelidir.

---

# 260. Oynanış Sırasında Ayarlara Erişim

Ayarlar oynanış sırasında açılabilmelidir.

Oyuncu en azından şunlara erişebilmelidir:

- Müzik
- Ses
- Titreşim

Oyun, ayarlar arayüzüne girilip çıkıldığında oynanış durumunu güvenle korumalıdır.

---

# 261. Mobil UX Gereksinimi

Oyun öncelikle mobil cihazlar için tasarlanmıştır.

Önemli gereksinimler:

- Dokunmatik öncelikli kontroller (Touch-first)
- Yeterince büyük dokunma hedefleri
- Dokunuşlara anında tepki
- Net görsel geri bildirim
- Gereksiz menülerden kaçınma
- Dikey/yatay yönelim davranışı, hedeflenen platform yönelimine göre geliştirme aşamasında kesinleştirilmelidir.

Orijinal oyun deneyimi hızlı ve basit kalmalıdır.

---

# 262. Çevrimdışı Öncelikli Oynanış

Ana oynanış sürekli bir internet bağlantısı gerektirmemelidir.

Oyuncu çevrimdışıyken de şunları yapabilmelidir:

- Seviyeleri başlatma
- Hedef alma (Aim)
- Ateş etme
- Seviyeleri tamamlama
- Seviyeleri kaybetme
- Uygun yerel kaynakları harcama
- Seviye ilerlemesini sürdürme

Bulut senkronizasyonu ve güncel lider tablosu verileri gibi ağa bağımlı özellikler bağlantı gelene kadar bekleyebilir.

---

# 263. Yerel Kayıt

Kalıcı oyuncu durumu yerel olarak saklanmalıdır.

En azından şunlar kaydedilmelidir:

- Mevcut seviye
- Altın
- Canlar
- Skill kullanım sayıları
- Kullanıcı adı
- Avatar
- Avatar çerçevesi
- Ayarlar
- İlgili ilerleme verileri

---

# 264. Kayıt Güvenliği

Oyun önemli durum değişikliklerini güvenli noktalarda kaydetmelidir.

Örnekler:

- Seviye tamamlama
- Seviye kaybı
- Can eksilmesi
- Altın harcama / kazanma
- Skill satın alımı
- Skill tüketimi
- Kullanıcı adı değişikliği
- Profil değişikliği

Uygulama, geçerli ilerlemenin kaybolma riskini en aza indirmelidir.

---

# 265. Mükerrer Ödül Engelleme

Ödüller tamamlanan belirli bir olay için yalnızca BİR KEZ verilmelidir.

Örnekler:

```text
Seviye tamamlama ödülü
Kalan top ödülü
Ödüllü reklam ödülü
Dükkan satın alımı
```

Aynı olay şu sebeplerle ödülü yanlışlıkla iki kez vermemelidir:

- Sahne yeniden yüklenmesi (Scene reload)
- Uygulama yeniden başlatması
- İnternet yeniden bağlantısı
- Mükerrer callback tetiklenmesi
- Arayüze çift tıklama (Double-click)

---

# 266. Can Düşme Güvenliği

Can, yalnızca gerçek bir yenilgi veya seviyeden çıkış olayı başına BİR KEZ düşülmelidir.

Uygulama şu durumu engellemelidir:

```text
Kaybetme
→ -1 can

Sahne yeniden yüklendi
→ tekrar -1 can
```

Gerçekten yeni bir kayıp yaşanmadıkça fazladan can düşülmemelidir.

---

# 267. Skill Harcama Güvenliği

Skill kullanımı, geçerli aktivasyon başına kesinlikle bir hak tüketmelidir.

Arayüze çift dokunulması veya callback'in tekrarlanması, oyuncu gerçekten iki kez aktif etmedikçe iki kullanım düşmemelidir.

---

# 268. Sahne Geçiş Güvenliği

Oyun geçişleri oyuncu durumunu güvenli şekilde korumalıdır.

Önemli geçişler:

```text
Ana Menü
→ Oynanış

Oynanış
→ Zafer (Win)

Oynanış
→ Yenilgi (Lose)

Oynanış
→ Çıkış (Exit)

Zafer
→ Sonraki Seviye

Zafer
→ Ana Menü
```

---

# 269. Hata Yönetimi

MVP yaygın karşılaşılan sorunları zarif bir şekilde yönetmelidir.

Örnekler:

- İnternet bağlantısının kopması
- Lider tablosu isteğinin başarısız olması
- Bulut senkronizasyonu hatası
- Reklam yüklenememesi
- IAP (satın alma) hatası
- Uygulamanın arka plana atılması / kesintiye uğraması
- Sahne yükleme sorunları

Ana oynanış mümkün olan her durumda çalışmaya devam etmelidir.

---

# 270. Ağ Hatası Davranışı

Ağ erişimi başarısız olursa:

```text
Ana oynanışı gereksiz yere engelleme.
```

Oyun yerel durumu kullanarak devam etmelidir.

Bulut senkronizasyonu daha sonra tekrar deneyebilir.

Lider tablosu önbelleğe alınmış veriyi gösterebilir.

---

# 271. Satın Alma Hatası

Gerçek para ile satın alma işlemi başarısız olursa:

- Ürünü hesaba tanımlama.
- Oyuncunun mevcut kaynaklarını koru.
- Net bir hata mesajı göster.
- Tekrar denemeye izin ver.

Kesin mağaza uygulaması yapılandırılabilirdir.

---

# 272. Ödüllü Reklam Başarısızlığı

Ödüllü reklam başarısız olursa veya iptal edilirse:

```text
Ödül verilmez
```

Reklam başarıyla tamamlanırsa:

```text
Yapılandırılan ödül bir defaya mahsus verilir
```

---

# 273. Seviye Test Gereksinimleri

Her seviye şunlar açısından test edilmelidir:

- Tamamlanabilirlik (Clearability)
- Doğru fizik davranışı
- Doğru top sayısı
- Doğru zafer algılama
- Doğru yenilgi algılama
- Doğru ödül hesaplaması
- Doğru seviye ilerlemesi
- Doğru kayıt davranışı

---

# 274. Seviye 1–4 Testleri

Başlangıç seviyeleri şunları doğrulamalıdır:

- Temel nişan alma (Aim)
- Temel ateş etme
- Temel obje etkileşimi
- Top sayısı dengesi
- Basit fiziksel zincirleme reaksiyonlar
- Zafer akışı
- Yenilgi akışı

Bu seviyeler temel öğrenme eğrisini oluşturur.

---

# 275. Seviye 5 Testleri

Seviye 5 ek olarak şunları doğrulamalıdır:

- Yeni blok/obje türünün tanıtımı
- Mega Ball kilidinin açılması
- Mega Ball arayüzü
- Mega Ball kullanımı ve etkisi
- Mega Ball kullanım hakkının düşmesi
- Normal oynanışın kesintisiz devam etmesi

---

# 276. Seviye 10 Testleri

Seviye 10 ek olarak şunları doğrulamalıdır:

- Yeni blok/obje türünün tanıtımı
- Infinity Ball kilidinin açılması
- Infinity Ball arayüzü
- Infinity Ball zamanlayıcısı (timer)
- Infinity Ball kullanım hakkının düşmesi
- Yeteneklerin üst üste binmesi (Skill stacking) davranışı
- Uygun durumlarda kullanıcı adı/nickname isteme akışı

---

# 277. Seviye 15 Testleri

Seviye 15 şunları doğrulamalıdır:

- Yeni içerik
- Artan bulmaca karmaşıklığı
- Çoklu obje etkileşimleri
- Fizik kararlılığı

---

# 278. Seviye 20 Testleri

Seviye 20 şunları doğrulamalıdır:

- Yeni içerik
- Daha yüksek zorluk seviyesi
- Daha karmaşık obje dizilimleri
- Yoğun fiziksel etkileşimlerde sistem kararlılığı

---

# 279. Seviye 25 Testleri

Seviye 25 şunları doğrulamalıdır:

- Yeni içerik
- İleri seviye bulmaca tasarımları
- Çok katmanlı yerleşimler (iki sıra objeler)
- Güvenilir seviye tamamlama tespiti

---

# 280. Seviye 30 Testleri

Seviye 30 MVP'nin son seviyesidir.

Şunları doğrulamalıdır:

- Son bulmaca tamamlanabilir olmalıdır
- Zafer koşulu doğru çalışmalıdır
- Ödül eksiksiz verilmelidir
- Win Paneli açılmalıdır
- "Sonraki Seviye" (Next Level) butonu gösterilmemelidir
- Oyuncu uygun Ana Menü / tamamlama durumuna dönebilmelidir
- İlerleme "Seviye 30 Tamamlandı" olarak kaydedilmelidir

---

# 281. Reklam Kaldırma Testi

Test:

```text
No Ads KAPALI
```

ve:

```text
No Ads AÇIK
```

Beklenen davranış:

```text
No Ads KAPALI:
Zorunlu / geçiş reklamları görünebilir.

No Ads AÇIK:
Zorunlu / geçiş reklamları kesinlikle görünmez.

Ödüllü reklamlar:
Her iki durumda da kullanılabilir kalır.
```

---

# 282. Çevrimdışı Senkronizasyon Testi

Test adımları:

```text
1. Çevrimiçi ol
2. Seviye 10'a ulaş
3. İnterneti kes
4. Seviye 12'ye ulaş
5. İnterneti tekrar bağla
6. Senkronize et
```

Beklenen sonuç:

```text
Bulut ilerlemesi → Seviye 12
```

(Onaylanan "en son yerel ilerleme kazanır" kuralına göre).

---

# 283. Çevrimdışı Lider Tablosu Testi

Test adımları:

```text
1. Lider tablosunu çevrimiçiyken senkronize et.
2. İnternet bağlantısını kes.
3. Lider tablosunu aç.
```

Beklenen sonuç:

```text
Başarıyla alınan son lider tablosu verisi gösterilir.
```

Tekrar bağlandıktan sonra:

```text
Yeni lider tablosu verileri senkronize edilebilir.
```

---

# 284. Can Sistemi Testi

Test:

```text
5 can ile başla
```

Ardından doğrula:

```text
Kazanma (Win):
Can sayısı değişmez.

Kaybetme (Lose):
-1 can.

Çıkış yapma (Exit):
-1 can.

Aktif seviyedeyken uygulamayı kapatma:
-1 can.

Yenilenme:
Yaklaşık 30 dakika sonra +1 can.
```

Sıfır canda:

```text
Normal seviye başlatma engellenir.
```

---

# 285. Top Sistemi Testi

Doğrulanacaklar:

- Top sayısı normal atış başına tam bir adet azalmalıdır.
- Top hedeflenen dünya koordinatına doğru ilerlemelidir.
- Top doğru çarpışma tepkisi vermelidir.
- Top doğal bir şekilde sekebilmelidir.
- Top oyuncuya doğru geri sekebilmelidir.
- Top belirli bir süre sonra yok olmalıdır.
- Top sahnede sonsuza kadar kalmamalıdır.
- Boşluğa atılan atışlar ortam derinliğine doğru ilerlemeli ve kaybolmalıdır.

---

# 286. Hedefleme Testi

Aşağıdaki durumlar test edilmelidir:

```text
Sol tarafa yakın dokunma
Merkeze dokunma
Sağ tarafa yakın dokunma
Objelerin üstüne dokunma
Objelerin arkasına dokunma
Kullanışlı obje alanlarının dışına dokunma
Parmağı yatay olarak sürükleme (Drag)
Bırakmadan önce parmağı hareket ettirme
```

Beklenen:

- Tap (Dokunma) ateş eder.
- Drag (Sürükleme) mortarı hareket ettirir/hedefler.
- Drag yanlışlıkla ateş tetiklemez.
- Top dokunulan dünya koordinatını hedefler.
- Mortar izin verilen hareket sınırlarına uyar.

---

# 287. Fizik Testi

Fizik sistemi şunlar için test edilmelidir:

- Tek kutu
- Çoklu kutular
- Ağır taş
- Kolonlar
- Koniler
- Boya şişesi
- Karışık obje kuleleri/yığınları
- İki sıralı/katmanlı yerleşimler
- Çoklu eşzamanlı çarpışmalar

Amaç kusursuz bir simülasyon değil; kararlı, inandırıcı ve tatmin edici bir fizik hissidir.

---

# 288. Fizik Kararlılığı

Oyun şunlardan kaçınmalıdır:

- Objelerin sonsuza fırlaması
- Patlayıcı / tahmin edilemez aşırı impulslar
- Objelerin birbirinin içinden geçmesi (tunneling)
- Sürekli titreme / jitter
- Fizik patlamaları
- Aşırı CPU yükü
- Topların bir yerlere sonsuza dek sıkışıp kalması

Fizik, seviye tasarımı ve testleri için yeterince deterministik kalmalıdır.

---

# 289. Performans Gereksinimi

MVP hedeflenen makul mobil donanımlarda akıcı bir şekilde çalışmalıdır.

Performans öncelikleri:

1. Kararlı kare hızı (Stable frame rate)
2. Kararlı fizik hesaplamaları
3. Hızlı sahne yükleme süreleri
4. Gereksiz bellek tahsislerinin (garbage allocation) önlenmesi
5. Kontrollü VFX kullanımı
6. Verimli obje yönetimi

---

# 290. Obje Havuzu (Object Pooling)

Toplar ve geçici görsel efektler gibi sık üretilen nesneler için uygun yerlerde obje havuzu (pooling) kullanılmalıdır.

Potansiyel havuz sistemleri:

```text
Projectile Pool (Mermi Havuzu)
Impact VFX Pool (Çarpışma Efekt Havuzu)
Destruction VFX Pool (Yıkım Efekt Havuzu)
Paint Effects Pool (Boya Efekt Havuzu)
```

Kesin mimari mühendislik kararına bırakılmıştır.

---

# 291. Veri Odaklı Seviye Tasarımı

Seviye tanımları kod içine gömülmek yerine veri odaklı olarak yapılandırılmalıdır.

Bir seviye kavramsal olarak şu verileri barındırmalıdır:

```text
Level ID
Obje Listesi
Obje Pozisyonları
Obje Rotasyonları
Obje Türleri
Obje Parametreleri
Stand Konfigürasyonu
Top Sayısı
Arka Plan / Ortam
Özel İçerik
Zorluk Derecesi
```

Kesin serileştirme formatı yapılandırılabilirdir (ScriptableObject, JSON vb.).

---

# 292. Seviye Verisi Genişletilebilirliği

Sistem, ana oyun mantığını yeniden yazmadan yeni seviyelerin eklenmesine izin vermelidir.

Yeni bir seviye eklemek temel olarak şunları içermelidir:

```text
Seviye verisi oluşturma
+
Objeleri yerleştirme ve yapılandırma
+
Parametreleri belirleme
```

Sıfırdan kod yazmak gerekmemelidir.

---

# 293. Dikey Dilim Gereksinimi (Vertical Slice)

30 seviyelik tam MVP üretimine geçmeden önce, küçük bir dikey dilim (vertical slice) ile tam oyun döngüsü kanıtlanmalıdır.

Önerilen dilim:

```text
Tutorial
+
Seviye 1
+
Seviye 5 (Mega Ball açılışı)
+
Seviye 10 (Infinity Ball açılışı)
+
Zafer (Win)
+
Yenilgi (Lose)
+
Can sistemi (Lives)
+
Temel ekonomi (Gold)
```

Amaç, içerik üretimi ölçeklenmeden önce ana deneyimi doğrulamaktır.

---

# 294. Prototip vs Final

Erken geliştirme aşamasında:

```text
Yer tutucu (placeholder) asset kullanımı tamamen kabul edilebilirdir.
```

Ancak:

```text
Ana oynanış davranışları dokümante edilen tasarıma şimdiden tam olarak uymalıdır.
```

Bir prototip şunlardan asla kalıcı olarak ödün vermemelidir:

- Fizik
- Kontrol / Input
- Seviye ilerlemesi
- Kayıt durumu
- Temel UI akışı

---

# 295. Yer Tutucu Varlık Kuralı (Placeholder Assets)

Orijinal varlıklar henüz mevcut olmadığında:

Şunlar için görsel olarak uygun yer tutucular (placeholder) kullanılmalıdır:

- Toplar (Balls)
- Mortar
- Kutular
- Taşlar
- Kolonlar
- Koniler
- Boya şişeleri
- Stand
- UI ikonları
- Avatar
- Çerçeveler
- Arka planlar
- Logolar

Yer tutucu sistemi daha sonra gerçek varlıkların kolayca entegre edilmesini sağlamalıdır.

---

# 296. Görsel Stil

Genel görsel yönelim şunları yansıtmalıdır:

- Casual mobil bulmaca hissi
- Parlak, net ve okunabilir oynanış
- Temiz 3D sunum
- Yüksek obje okunabilirliği
- Anlaşılır fizik etkileşimleri
- Tatmin edici yıkım hissi
- Sade arayüz

Oyun görsel olarak aşırı karmaşık hale getirilmemelidir.

---

# 297. Kamera ve Dünya Sunumu

Oynanış görsel olarak bir 3D sahne hissi vermelidir.

Oyuncu şu katmanları algılamalıdır:

```text
Ön Plan (Foreground):
Mortar / oyuncu konumu

Orta Plan (Middle):
Topun hareket rotası (Trajectory)

Arka Plan (Background):
Stand + objeler + çevre ortamı
```

Birbirinin arkasında duran objeler derinlik hissini net olarak hissettirmelidir.

---

# 298. Ana Harita ve Çevre

MVP tek bir ana oynanış ortamı kullanabilir.

Görsel arka plan seviyelere veya seviye gruplarına göre değişebilir.

Stand ve obje dizilimleri oynanış çeşitliliğinin büyük kısmını sağlar.

Karmaşık bir dünya haritası sistemi gerekli değildir.

---

# 299. Harita Geçişi

MVP için özel bir harita geçiş sistemi zorunlu değildir.

Seviye geçişleri basit bir sahne/ortam geçişiyle gerçekleştirilebilir.

---

# 300. Açılış Ekranı (Splash Screen)

Başlangıç akışı:

```text
Splash (Açılış)
→ Loading (Yükleme)
→ Tutorial veya Ana Oynanış
```

Geliştirici ve yayıncı logoları geliştirme sürecinde geçici şablonlarla temsil edilebilir.

---

# 301. Yükleme Ekranı (Loading Screen)

Yükleme ekranı şunları içerebilir:

- İlerleme çubuğu / göstergesi
- Oynanış ipucu
- Sade görsel arka plan

Orijinal oyun farklı yükleme tasarımları veya ipuçları kullanabilir.

MVP için tek bir yeniden kullanılabilir yükleme tasarımı yeterlidir.

---

# 302. İlk Açılış Akışı

İlk açılış:

```text
Uygulama Başlatma
↓
Splash
↓
Loading
↓
Tutorial
↓
Seviye 1
```

Tutorial yalnızca ilk uygun açılışta gösterilmelidir.

---

# 303. Geri Dönen Oyuncu Akışı

Geri dönen oyuncu:

```text
Uygulama Başlatma
↓
Splash
↓
Loading
↓
Ana Menü (Main Menu)
↓
Mevcut Seviye
```

Oyuncunun kaydedilmiş ilerlemesi aktif seviyeyi belirler.

---

# 304. Tutorial Tamamlanma Durumu

Tutorial bir kez tamamlandığında:

```text
TutorialCompleted = true
```

Tutorial, geri dönen oyuncunun normal akışını tekrar tekrar kesintiye uğratmamalıdır.

---

# 305. Temel Oynanış Önceliği

Oyun şu öncelik sırasını takip etmelidir:

```text
1. Fizik (Physics)
2. Nişan alma (Aiming)
3. Mermi davranışı (Projectile)
4. Obje etkileşimi
5. Sınırlı atışlı bulmaca tasarımı
6. Net görsel/işitsel geri bildirim
7. İlerleme (Progression)
8. Ekonomi
9. Gelir modelleri (Monetization)
```

Gelir modelleri ana bulmaca deneyimini gölgede bırakmamalıdır.

---

# 306. Ana Oyun Döngüsü

Eksiksiz oynanış döngüsü şudur:

```text
Oyunu Aç
↓
Ana Menü
↓
Mevcut Seviyeyi Başlat
↓
Nişan Al (Aim)
↓
Ateş Et (Fire)
↓
Fiziği İzle
↓
Objeler Hareket Eder / Kırılır / Düşer
↓
Ateş Etmeye Devam Et
↓
Sonuç:
    Tüm Objeler Temizlendi (Zafer)
    VEYA
    Toplar Tükendi (Yenilgi)
↓
Zafer / Yenilgi
↓
Ödül Kazanımı veya Can Kaybı
↓
Devam Et / Menüye Dön
↓
Sonraki Seviye
```

---

# 307. Oyuncu Deneyimi Hedefi

Hedeflenen deneyim şudur:

> “Ekrana dokunurum, top tam olarak amaçladığım dünya noktasına uçar, fizik kuralları doğal bir şekilde tepki verir, objeler tatmin edici şekilde birbirine çarpıp devrilir ve ben sadece bir butona basmış gibi değil, gerçek bir fizik bulmacasını çözmüş gibi hissederim.”

Oyun şu hisleri vermelidir:

- Anında tepki veren
- Fiziksel ve organik
- Öğrenilebilir derecede öngörülebilir
- Tatmin edici
- Casual ve rahat
- Kolay anlaşılır
- Kademeli olarak zorlaşan

---

# 308. Tasarım Temeli — Fizik Bulmacası

Fizik etkileşimi oyunun birincil temel değeridir.

Oyuncu şu sayede kazanmalıdır:

```text
İyi bir hedef seçmek
+
Objeler arasındaki ilişkiyi anlamak
+
Sınırlı atışları verimli kullanmak
```

Karmaşık kontroller sayesinde değil.

---

# 309. Tasarım Temeli — Basit Kontroller

Kontrol modeli sade kalmalıdır:

```text
Tap (Dokun) = Ateş Et
Drag (Sürükle) = Hedef Al / Mortarı Hareket Ettir
```

Şu tür gereksiz karmaşıklıklardan kaçınılmalıdır:

- Güç barı / göstergesi
- Manuel mermi doldurma butonu
- Karmaşık nişangah arayüzleri
- Çoklu ateşleme modları
- Ayrı yörünge çizim editörleri

---

# 310. Tasarım Temeli — Sınırlı Kaynaklar

Normal toplar sınırlıdır.

Bu kısıtlama bulmaca baskısını ve heyecanını yaratır.

Oyuncu şunu düşünmelidir:

```text
“En büyük zincirleme reaksiyonu başlatmak için nereye vurmalıyım?”
```

Şunu değil:

```text
“Sonsuza kadar ateş edebilirim.”
```

---

# 311. Tasarım Temeli — Tatmin Edici Yıkım

Objeler oyuncunun eylemlerine belirgin şekilde tepki vermelidir.

Önemli tepkiler:

- Geri itilme (Knockback)
- Devrilme ve düşme
- Birbirine çarpma
- Kırılma
- Yuvarlanma
- Sekme
- Boya dökülmesi
- İkincil çarpışmalar

---

# 312. Tasarım Temeli — Okunabilirlik

Oyuncu şunları bir bakışta anlayabilmelidir:

- Nelerin hareket ettirilebilir olduğunu
- Nelerin kırılabilir olduğunu
- Stand'ın nerede durduğunu
- Mortar'ın konumunu
- Kaç top kaldığını
- Hangi skill'lerin kullanılabilir olduğunu
- Seviyenin kazanılıp kazanılmadığını

---

# 313. Tasarım Temeli — İlerleme

Oyun yeni içerikleri kademeli olarak tanıtmalıdır.

Kesinleşen yapı şudur:

```text
Seviye 1–4:
Başlangıç içeriği

Seviye 5:
Yeni içerik + Mega Ball açılışı

Seviye 10:
Yeni içerik + Infinity Ball açılışı

Seviye 15:
Yeni içerik

Seviye 20:
Yeni içerik

Seviye 25:
Yeni içerik

Seviye 30:
Final MVP seviyesi
```

---

# 314. Tasarım Temeli — Minimum Karmaşıklık

Temel deneyimi üretmek için gerekli olmayan hiçbir sistemi eklemeyin.

Açıkça talep edilmedikçe şu sistemleri MVP kapsamına almaktan kaçının:

- PvP
- Çok oyunculu modlar (Multiplayer)
- Klanlar / Loncalar
- Karmaşık görev sistemleri
- Can sistemi dışındaki enerji para birimleri
- Çoklu para birimi sistemleri
- Karakter yetenek ağaçları
- Ekipman sistemleri
- Eşya üretimi (Crafting)
- Karmaşık envanter yönetimi
- Günlük görevler
- Sandık sistemleri
- Battle pass
- Karmaşık hesap/giriş akışları

---

# 315. Kesinlikle Kaybedilmemesi Gereken Kurallar

Bu dokümantasyon gelecekte bir Master Build Prompt'a dönüştürülürken şu maddeler zorunlu olarak korunmalıdır:

### A. Dünya Koordinatı Hedefleme (World-Space Targeting)

Mermi, ekranda dokunulan gerçek dünya koordinatını hedeflemelidir.

### B. Fizik Odaklı Oynanış

Objeler top darbelerine fizik kurallarıyla tepki vermelidir.

### C. Sınırlı Atış Sayısı

Oyuncu sonsuza kadar ateş edemez; toplar sınırlıdır.

### D. Doğal Sekme

Toplar objelerden sekebilir ve oyuncuya doğru geri gelebilir.

### E. Boşluk Atışının Görünürlüğü

Boşluğa atılan top, 3D ortam derinliğinde ilerleyip kaybolmalıdır.

### F. Drag ile Mortar Yönlendirme

Sürükleme hareketi mortarı taşır ve yanlışlıkla ateş etmez.

### G. Skill İlerlemesi

Mega Ball Seviye 5'te, Infinity Ball Seviye 10'da açılır.

### H. Can Kuralları

Onaylanan can düşme ve yenilenme kuralları korunmalıdır.

### I. Çevrimdışı Oynanış

Ana oynanış internetsiz kusursuz çalışmalıdır.

### J. Çevrimdışından Buluta Senkronizasyon

MVP'de "en son yerel ilerleme kazanır" kuralı geçerlidir.

### K. No Ads Davranışı

No Ads zorunlu reklamları kaldırır, ödüllü reklamları kaldırmaz.

### L. Seviye 30 Davranışı

Seviye 30'da normal Win Paneli açılır ancak "Sonraki Seviye" butonu bulunmaz.

---

# 316. Dokümantasyon Güvenilirlik Etiketleri

Gelecek teknik spesifikasyonlar üç kategoriyi birbirinden açıkça ayırmalıdır:

## KİLİTLİ / ONAYLANMIŞ (LOCKED / CONFIRMED)

Kullanıcı tarafından kesinleştirilmiş kurallardır; değiştirilmemelidir.

Örnekler:

```text
30 MVP seviyesi
Sıralı ilerleme (Sequential)
Seviye tekrarı (replay) yok
Her 5 seviyede yeni içerik
Seviye 5'te Mega Ball açılışı
Seviye 10'da Infinity Ball açılışı
Kalan Top × 2 Altın formülü
5 başlangıç canı
30 dakikada 1 can yenilenmesi
Çevrimdışı oynanış desteği
En son yerel ilerleme geçerlidir kuralı
No Ads ödüllü reklamları kaldırmaz
Seviye 30'da Sonraki Seviye butonu yoktur
```

---

# 317. YAPILANDIRILABİLİR DEĞERLER (CONFIGURABLE VALUES)

Temel oyun tasarımını bozmadan dengelenebilecek (tune edilebilecek) değerlerdir:

```text
Top sayısı
Ateş cooldown süresi
Topun yaşam süresi (lifetime)
Topun hızı
Obje kütlesi (mass)
Obje sağlığı / direnci
Hasar miktarı
Geri itme kuvveti (Knockback)
Temel seviye tamamlama altını
Başlangıç altını
Skill fiyatları
Skill süreleri
Skill kullanım paket miktarları
Can satın alma fiyatı
Reklam sıklığı
Özel teklif süreleri
VFX süreleri
Ses seviyeleri
```

---

# 318. UYGULAMA / GELİŞTİRME KARARLARI (IMPLEMENTATION DECISIONS)

Gelecekteki geliştirici veya yapay zekâ şu teknik çözümleri seçmekte serbesttir:

- Unity versiyonu
- Render pipeline (URP vb.)
- Fizik konfigürasyonu
- Kayıt teknolojisi (PlayerPrefs, JSON, Binary vb.)
- Bulut backend servisi
- IAP sağlayıcısı
- Reklam SDK'sı
- Analitik altyapısı
- Obje havuzu (pooling) mimarisi
- Sahne mimarisi
- Veri serileştirme yöntemi
- UI altyapısı
- Addressables veya AssetBundle kullanımı
- Build pipeline

Bu tercihler dokümante edilen oynanış gereksinimlerini eksiksiz karşılamalıdır.

---

# 319. Gözlem ile Kesin Gereksinimi Birbirine Karıştırmayın

Bazı detaylar orijinal oyunun incelenmesinden elde edilen gözlemlerdir.

Eğer kesin bir sayı netleştirilmemişse, katı bir kural gibi uygulanmamalıdır.

Örneğin:

```text
“Yaklaşık 20–30 top”
```

ifadesi:

```text
Yapılandırılabilir top sayısı
```

anlamına gelir; şu anlama gelmez:

```text
Her seviye kesinlikle 25 top içermek zorundadır.
```

---

# 320. Orijinal Varlıkların Birebir Kopyalanması

Geliştirme süreci mevcut olmayan orijinal 3D modelleri veya dokuları pikseli pikseline yeniden üretmek zorunda değildir.

Bunun yerine şunlar eksiksiz üretilmelidir:

- Oynanış davranışı
- Etkileşim modeli
- Kullanıcı deneyimi (UX) akışı
- Fizik bulmacası yapısı
- İlerleme yapısı
- Sistem davranışları

Gerekli yerlerde orijinal varlıklar veya uygun yer tutucu (placeholder) varlıklar kullanılabilir.

---

# 321. Orijinal Oyun vs MVP Klon Spesifikasyonu

Dokümantasyon gözlemlenen oyun deneyimini ve hedeflenen MVP klonunu tanımlar.

Kesin prodüksiyon değerlerinin bilinmediği durumlarda:

```text
Davranış zorunludur.
Sayısal değer yapılandırılabilirdir.
```

Bu ayrım Master Build Prompt hazırlanırken mutlaka korunmalıdır.

---

# 322. Master Build Prompt Devri

Bu dokümantasyon tamamlandıktan sonraki aşama şudur:

```text
Dokümantasyon
↓
Eksik Analizi (Gap Analysis)
↓
Master Build Prompt
↓
Yapay Zekâ ile Geliştirme
```

Master Build Prompt bu dokümanı mutlak referans kaynağı (source of truth) olarak kullanmalıdır.

---

# 323. Master Build Prompt Amacı

Master Build Prompt, başka bir yapay zekâya dokümante edilen oynanıştan ilham alan oynanabilir bir mobil oyun inşa etmesi için eksiksiz talimat vermelidir.

Ortaya çıkacak proje şunları içermelidir:

- Eksiksiz oyun döngüsü
- 30 seviyelik MVP
- Fizik tabanlı atış sistemi
- Obje etkileşimleri
- Skill'ler
- Can sistemi
- Altın ekonomisi
- Dükkan
- Ana Menü
- Profil
- Lider Tablosu
- Ayarlar
- Çevrimdışı kayıt
- Bulut senkronizasyon mantığı
- Reklamlar
- Ödüllü reklamlar
- Reklam kaldırma satın alımı
- Gerekli tüm arayüzler
- VFX / SFX / Dokunsal geri bildirimler
- Test altyapısı

---

# 324. Master Build Prompt Yapısı

Gelecekteki Master Build Prompt ideal olarak şu başlıklarla yapılandırılmalıdır:

```text
1. Rol ve Amaç
2. Oyuna Genel Bakış
3. Tartışılamaz Zorunlu Gereksinimler
4. Temel Oynanış (Core Gameplay)
5. Kontrol Sistemi (Input)
6. Hedefleme Sistemi (Aiming)
7. Mermi Sistemi (Projectile)
8. Fizik Sistemi (Physics)
9. Obje Sistemi (Objects)
10. Stand Sistemi (Stand)
11. Seviye Sistemi (Levels)
12. Seviye İlerlemesi (Progression)
13. Tutorial
14. Zafer Koşulu (Victory)
15. Yenilgi Koşulu (Lose)
16. Can Sistemi (Lives)
17. Skill'ler (Skills)
18. Ekonomi (Economy)
19. Dükkan (Shop)
20. Reklamlar (Ads)
21. Reklam Kaldırma (No Ads)
22. Ana Menü (Main Menu)
23. Profil (Profile)
24. Lider Tablosu (Leaderboard)
25. Ayarlar (Settings)
26. Ses Tasarımı (Audio)
27. Görsel Efektler (VFX)
28. Titreşim ve Dokunsal Geri Bildirim (Haptics)
29. Kayıt Sistemi (Save System)
30. Çevrimdışı Mod (Offline Mode)
31. Bulut Senkronizasyonu (Cloud Sync)
32. Hata Yönetimi (Error Handling)
33. Performans (Performance)
34. Test Süreci (Testing)
35. Kabul Kriterleri (Acceptance Criteria)
36. Build Gereksinimleri
```

---

# 325. Master Prompt Yapay Zekâ Davranışı

Master Build Prompt'u alan yapay zekâ şunları yapmalıdır:

- Onaylanmış kurallara harfiyen uymalıdır.
- Dokümanla çelişen sistemler uydurmaktan kaçınmalıdır.
- Yapılandırılabilir değerleri ayarlanabilir parametreler olarak tutmalıdır.
- Yaptığı teknik varsayımları açıkça belirtmelidir.
- Oynanışı veri odaklı (data-driven) inşa etmelidir.
- Sistemleri modüler tasarlamalıdır.
- Kapsam dışına çıkmaktan kaçınmalıdır.
- Kritik etkileşimleri test etmelidir.
- Mobil cihaz ergonomisini ve kullanılabilirliğini korumalıdır.

---

# 326. Master Prompt Halüsinasyon Engelleme Kuralı

Gelecekteki yapay zekâ şu dönüşümü sessizce yapmamalıdır:

```text
Bilinmeyen (Unknown)
```

durumunu:

```text
Onaylanmış (Confirmed)
```

haline getirmemelidir.

Bir değer bilinmiyorsa, mantıklı ve yapılandırılabilir bir varsayılan değer seçmeli ve bunu "yapılandırılabilir" olarak etiketlemelidir.

---

# 327. Master Prompt Değişiklik Kuralı

Eğer teknik bir uygulama seçimi gerekiyorsa ancak dokümanda belirtilmemişse:

```text
Mantıklı bir teknik yaklaşım seç.
Modüler tut.
Oyuncuya yansıyan oynanış kuralını ASLA değiştirme.
```

Örnek:

```text
Gereksinim:
Mermi dokunulan dünya koordinatını hedeflemelidir.

Uygulama:
Yapay zekâ raycast + dünya hedef noktası yöntemini seçebilir.
```

Teknik uygulama farklılık gösterebilir.

Oyuncunun deneyimlediği oynanış kuralı DEĞİŞTİRİLEMEZ.

---

# 328. Geliştirme Önceliği

Eğer geliştirme süresi kısıtlıysa öncelik sıralaması şu olmalıdır:

```text
P0 — Temel oynanış (Core gameplay)
P0 — Fizik (Physics)
P0 — Nişan alma (Aiming)
P0 — Mermi davranışı (Projectile)
P0 — Obje etkileşimi
P0 — Seviye kazanma / kaybetme mantığı

P1 — Tutorial
P1 — Can sistemi (Lives)
P1 — Skill'ler
P1 — Seviye ilerlemesi
P1 — Temel ekonomi (Gold)

P2 — Dükkan (Shop)
P2 — Reklamlar
P2 — Profil
P2 — Lider Tablosu
P2 — Bulut senkronizasyonu

P3 — Cila / Polish
P3 — Ekstra VFX
P3 — Ekstra SFX
P3 — Gelişmiş mağaza sunumları
```

Kesin sıralama prodüksiyon sırasında ayarlanabilir ancak temel oynanış her zaman ilk sırada olmalıdır.

---

# 329. Temel Oynanış Kabul Testi

Bir build şu akış kusursuz çalışana kadar oynanış açısından tamamlanmış sayılamaz:

```text
Oyunu Başlat
↓
Seviyeyi Aç
↓
Mortar Görünür
↓
Top Hazır
↓
Hedefe Dokun (Tap)
↓
Top Ateşlenir
↓
Top Dünya Hedefine Ulaşır
↓
Obje Fiziksel Olarak Tepki Verir
↓
Cooldown Sonrası Oyuncu Tekrar Ateş Edebilir
↓
Objeler Devrilebilir / Kırılabilir
↓
Gerekli Tüm Objeler Temizlenebilir
↓
Win Paneli Görünür
↓
Ödül Hesaplanır ve Verilir
↓
Sonraki Seviye Başlatılabilir
```

---

# 330. Yenilgi Kabul Testi

Şu akış doğrulanmalıdır:

```text
Seviyeyi Başlat
↓
Toplar Sıfıra İnene Kadar Ateş Et
↓
Stand Üzerinde Obje Kaldı
↓
Yenilgi Algılandı
↓
Can Sayısı Tam Olarak Bir Azalır
↓
Lose Paneli Görünür
↓
Yeniden Dene / Ana Menü Seçenekleri Aktif Olur
```

---

# 331. Çıkış Kabul Testi

Şu akış doğrulanmalıdır:

```text
Seviyeyi Başlat
↓
Çıkış Yap
↓
Seviye Terk Edilmiş Sayılır
↓
Can Sayısı Tam Olarak Bir Azalır
↓
Ana Menüye Dönülür
```

Eğer bir onay penceresi varsa:

```text
İptal:
Seviyede kal.

Onayla:
Çıkış yap ve 1 can düş.
```

---

# 332. Uygulamayı Kapatma Kabul Testi

Aktif bir seviye sırasında:

```text
Seviyeyi Aç
↓
Uygulamayı Kapat (Kill)
↓
Uygulamayı Tekrar Başlat
```

Onaylanan kurala göre seviye terk edilmiş sayılmalıdır.

Bu aktif seviyeden çıkış için 1 can düşülmelidir.

Uygulama aynı olaydan dolayı mükerrer can düşülmesini engellemelidir.

---

# 333. Skill Kabul Testi — Mega Ball

Test adımları:

```text
Seviye 5'ten önce:
Kilitli

Seviye 5:
Kilidi açılır

Başlangıç hakkı:
3 kullanım

Aktif et:
1 kullanım harcanır

Mermi:
Mega Ball fiziksel davranışı ve görünümü
```

Ek kullanımlar satın alınabilir olmalıdır.

---

# 334. Skill Kabul Testi — Infinity Ball

Test adımları:

```text
Seviye 10'dan önce:
Kilitli

Seviye 10:
Kilidi açılır

Başlangıç hakkı:
3 kullanım

Aktif et:
1 kullanım harcanır

Sayaç süresince:
Normal ateş etme cooldown'ı kaldırılır/devre dışı kalır

Süre bitiminde:
Normal cooldown geri döner
```

---

# 335. Skill Üst Üste Binme Kabul Testi

Test:

```text
Mega Ball aktif
+
Infinity Ball aktif
```

Beklenen sonuç:

```text
Her iki etki de eşzamanlı olarak devrede kalır.
```

---

# 336. Ekonomi Kabul Testi

Test:

```text
Başlangıç Altını
+
Seviye Ödülü
+
Kalan Top Bonusu
-
Altın Harcamaları
=
Doğru kalıcı bakiye
```

İşlemler, sahne yeniden yüklemelerinden veya uygulama yeniden başlatmalarından sonra asla yinelenmemelidir.

---

# 337. Kayıt Kabul Testi

Test:

```text
Seviyeyi tamamla
Uygulamayı kapat
Yeniden başlat
```

Beklenen:

```text
Tamamlanan ilerleme kayıtlı kalır.
```

Aynı test şunlar için tekrarlanır:

- Altın
- Canlar
- Skill kullanım hakları
- Kullanıcı adı
- Avatar
- Çerçeve
- Ayarlar

---

# 338. Bulut Senkronizasyon Kabul Testi

Test adımları:

```text
Çevrimiçi Seviye 10
↓
Çevrimdışı ol
↓
Seviye 11'i tamamla
↓
Seviye 12'yi tamamla
↓
Çevrimiçi ol
↓
Senkronize et
```

Beklenen:

```text
Bulut = Seviye 12
```

(Onaylanan "en son yerel ilerleme kazanır" MVP kuralına göre).

---

# 339. Lider Tablosu Kabul Testi

Test:

```text
Çevrimiçi:
Lider tablosunu çek

Çevrimdışı:
Önbellekteki lider tablosunu göster

Tekrar Bağlan:
Daha yeni lider tablosunu senkronize et
```

Lider tablosu servisi erişilemez olduğunda sistem çökmek yerine durumu zarifçe yönetmelidir.

---

# 340. Reklam Kaldırma Kabul Testi

Test:

```text
No Ads satın alınmadı:
Zorunlu / geçiş reklamları görünebilir.

No Ads satın alındı:
Zorunlu / geçiş reklamları tamamen devre dışı kalır.

Ödüllü reklamlar:
Kullanılabilir kalmaya devam eder.
```

---

# 341. Performans Kabul Testi

Oyun şu durumlarda kabul edilebilir mobil performansı korumalıdır:

- Birden fazla obje çarpıştığında
- Çok sayıda Rigidbody aktif olduğunda
- VFX oynatıldığında
- Toplar uçuşurken
- Objeler devrilirken
- Boya efektleri etkinken

Performans zincirleme reaksiyonlar sırasında çökmemelidir.

---

# 342. Fizik Kabul Testi

Fizik sistemi şu durumlarda testi geçer:

- Top çarpmaları duyarlıdır.
- Objeler doğal tepki verir.
- Ağır objeler daha tok hissettirir.
- Birden fazla obje birbirini itebilir.
- Objeler stand üzerinden düşebilir.
- Toplar sekebilir.
- Toplar oyuncuya geri dönebilir.
- Boşluğa atılan toplar ortamda ilerleyip kaybolur.
- Fizik kararlı kalır.

---

# 343. Görsel Kabul Testi

Oyuncu tutorial sonrasında ekstra bir açıklamaya ihtiyaç duymadan şunları ayırt edebilmelidir:

```text
Top (Ball)
Mortar
Stand
Objeler
Hedef bölgesi
Kalan top sayısı
Skill'ler
Kazanma / Kaybetme durumu
```

---

# 344. Kullanıcı Deneyimi (UX) Kabul Testi

Yeni bir oyuncu dışarıdan bir anlatım olmadan şunları anlayabilmelidir:

```text
Nereye dokunacağını
Nasıl nişan alacağını
Nasıl ateş edeceğini
Kaç topu kaldığını
Darbe sonrasında ne olduğunu
Ne zaman kazandığını
Ne zaman kaybettiğini
Bir sonraki adımda ne yapacağını
```

---

# 345. Nihai MVP Tanımı

MVP şu bileşenler eksiksiz geliştirilip test edildiğinde tamamlanmış kabul edilir:

```text
30 oynanabilir seviye
+
İşlevsel tutorial
+
Fizik tabanlı atış sistemi
+
Dünya koordinatı hedefleme
+
Obje etkileşimleri
+
Sınırlı top mekaniği
+
Kazanma / Kaybetme akışı
+
Can sistemi (Lives)
+
Mega Ball
+
Infinity Ball
+
Altın ekonomisi
+
Temel Dükkan (Shop)
+
Zorunlu reklamlar
+
Ödüllü reklamlar
+
Reklam Kaldırma (No Ads)
+
Ana Menü
+
Profil
+
Lider Tablosu
+
Ayarlar
+
Yerel kayıt (Local save)
+
Çevrimdışı oynanış
+
Bulut senkronizasyonu
+
Gerekli VFX / SFX / Titreşim
```

---

# 346. Nihai MVP Kapsam Sınırı

Dokümante edilen MVP'nin dışındaki her sistem gelecekteki kapsam (future scope) olarak kabul edilmelidir.

Örnekler:

- 30'dan fazla seviye
- Gelişmiş canlı etkinlikler (Live events)
- PvP
- Multiplayer
- Klanlar / Loncalar
- Battle Pass
- Karmaşık görevler
- Günlük görevler
- Gelişmiş avatar gelişim sistemleri
- Cihazlar arası hesap sistemi
- Karmaşık sosyal sistemler
- Gelişmiş analitik panelleri
- Büyük ölçekli backend altyapısı

---

# 347. Ürün Tanımı

Ürün şudur:

> Oyuncunun bir mortar/top kontrol ettiği, ekranda dokunduğu dünya koordinatlarına sınırlı sayıda top fırlattığı ve stand üzerindeki nesneleri devirip yok etmek için fizik tabanlı çarpışmaları kullandığı casual bir mobil 3D fizik bulmaca oyunu.

Oyun şu öğeleri harmanlar:

```text
Basit Kontroller
+
Fizik
+
Bulmaca Çözme
+
Sınırlı Kaynaklar
+
İlerleme
+
Hafif Ekonomi
```

---

# 348. Temel Ayrıştırıcı Faktör

Oyunun en önemli ayırt edici özelliği arayüzü veya gelir modeli değildir.

Şudur:

```text
Her atışta nesnelerin verdiği fiziksel tepki.
```

Her atış anlamlı hissettirmelidir.

İyi bir atış şunlara yol açabilir:

```text
Top Darbesi
→
Objenin İtilmesi
→
İkincil Çarpışma
→
Yığının Dengesini Kaybetmesi
→
Objelerin Devrilmesi / Düşmesi
→
Zincirleme Reaksiyon
```

---

# 349. Hedeflenen Oyuncu Duygusu

Oyuncunun deneyimlemesi gereken duygu akışı:

```text
“Nereyi vurmak istediğimi biliyorum.”
↓
“Ateş ettim.”
↓
“Harika, kutuyu ileri itti.”
↓
“O da taşa çarptı.”
↓
“Her şey devriliyor!”
↓
“Bulmacayı çözdüm!”
```

Bu tepki hedeflenen deneyimin merkezinde yer alır.

---

# 350. Oyunun Kaçınması Gereken Şeyler

Oyunun şu tür oyunlar gibi hissettirmesinden kaçının:

- Geleneksel bir hassasiyet nişancı oyunu (Precision shooter)
- Karmaşık bir topçu simülatörü (Artillery simulator)
- Saf bir match-3 oyunu
- Bir clicker oyunu
- Bir sonsuz koşu oyunu (Endless runner)
- Çok oyunculu bir nişancı oyunu

Oyun temelde bir:

**Fizik Bulmaca Oyunudur (Physics Puzzle Game).**

---

# 351. Dokümantasyon Eksiksizliği

Bu aşamada dokümantasyon şunları eksiksiz kapsamaktadır:

```text
Oynanış (Gameplay)
Girdi / Kontrol (Input)
Hedef Alma (Aiming)
Fizik (Physics)
Mermi (Projectile)
Objeler (Objects)
Stand (Platform)
Seviyeler (Levels)
Tutorial
Zafer (Victory)
Yenilgi (Failure)
Can Sistemi (Lives)
Skill'ler
Ekonomi
Dükkan
Reklamlar
Ana Menü
Profil
Lider Tablosu
Ayarlar
Ses (Audio)
Görsel Efektler (VFX)
Dokunsal Geri Bildirim (Haptics)
Çevrimdışı Oynanış
Kayıt (Save)
Bulut Senkronizasyonu
Test Süreci
Performans
MVP Kapsamı
Master Prompt Devri
```

---

# 352. Yapılandırılabilir Kalan Alanlar

Aşağıdaki alanlar prodüksiyon aşamasında dengeleme (tuning) gerektirebilir:

```text
Kesin seviye dizilimleri
Kesin obje boyutları
Kesin obje kütleleri
Kesin mermi hızı
Kesin mermi yaşam süresi
Seviye başına kesin top sayısı
Kesin Altın dengesi
Kesin Dükkan fiyatları
Kesin IAP ürün değerleri
Kesin reklam sıklığı
Kesin VFX tasarımları
Kesin SFX varlıkları
Kesin UI çizimleri
Kesin kamera açı ve değerleri
Farklı mobil çözünürlüklere kesin adaptasyon
```

Bunlar eksik oynanış konseptleri değildir.

Bunlar geliştirme ve dengeleme detaylarıdır.

---

# 353. Kesin Seviye Taslak Gereksinimi

Final Master Build Prompt hazırlanmadan önce genişletilmesi gereken alanlardan biri, 30 seviyenin tamamının kesin yerleşim planıdır.

Mevcut dokümantasyon şunları tanımlar:

```text
İlerleme yapısı
Zorluk yönü
Yeni içerik tanıtım noktaları
```

Ancak her seviye için her bir objenin kesin koordinatlarını tek tek belirlemez.

Bu nedenle gelecekteki Master Build Prompt iki seçenekten birini seçmelidir:

1. Kesin seviye taslaklarını (blueprint) içermek, veya
2. Geliştirici yapay zekâya tüm seviye parametrelerini düzenlenebilir tutarak mantıklı veri odaklı seviye düzenleri oluşturması talimatını vermek.

İkinci seçenek MVP prototipi için tamamen uygundur.

---

# 354. Kesin Fizik Değerleri Gereksinimi

Dokümantasyon kasıtlı olarak prodüksiyondaki kesin fizik değerlerini iddia etmez.

Geliştirme süreci şu parametreleri yapılandırılabilir olarak dışa açmalıdır:

```text
Kütle (Mass)
Sürtünme / Hava Direnci (Drag)
Açısal Direnç (Angular Drag)
Sekme Oranı (Bounciness)
Sürtünme (Friction)
İtme Kuvveti (Impulse)
Yerçekimi (Gravity)
Mermi Hızı
Mermi Boyutu
Mermi Yaşam Süresi
Çarpışma Eşikleri (Collision Thresholds)
```

Bu değerler oynanış testleri (playtesting) sırasında dengelenmelidir.

---

# 355. Kesin UI Çizim Gereksinimi

Dokümante edilen UI, piksel kusursuzluğunda bitmiş çizimler yerine davranış ve yapıyı tanımlar.

Geliştirme süreci şunları destekleyen tutarlı bir casual-mobil arayüz oluşturmalıdır:

```text
Ana Menü
Oynanış
Zafer Paneli (Win)
Yenilgi Paneli (Lose)
Dükkan (Shop)
Profil
Lider Tablosu
Ayarlar
Tutorial
Yükleme Ekranı (Loading)
```

Nihai çizimler daha sonra yer tutucuların yerine entegre edilebilir.

---

# 356. Kesin Gelir Modeli Gereksinimi

Gelir modeli değerleri yapılandırılabilir kalır.

Master Build Prompt nihai ticari fiyatları kesinleşmiş kurallarmış gibi uydurmamalıdır.

Bunun yerine:

```text
Yapılandırılabilir IAP ürünleri oluştur.
Yapılandırılabilir reklam yerleşimleri oluştur.
Yapılandırılabilir ödül değerleri oluştur.
```

---

# 357. Dokümantasyon Durumu

Durum:

**MVP TASARIMI İÇİN OYUN DOKÜMANTASYONU TAMAMLANMIŞTIR**

Bir sonraki aşama oyunu yeniden keşfetmek değildir.

Bir sonraki aşama bu dokümantasyonu kesin ve uygulanabilir bir geliştirme talimatına dönüştürmektir.

---

# 358. Master Build Prompt Hazırlığı

Master Build Prompt oluşturulmadan önce bu dokümantasyon mutlak referans kaynağı (source of truth) kabul edilmelidir.

Master Prompt şu kategorileri açıkça ayırmalıdır:

```text
KİLİTLİ GEREKSİNİMLER (LOCKED REQUIREMENTS)
YAPILANDIRILABİLİR DEĞERLER (CONFIGURABLE VALUES)
UYGULAMA KARARLARI (IMPLEMENTATION DECISIONS)
```

Bu sayede çelişkiler engellenir.

---

# 359. Master Prompt İnşa Felsefesi

Gelecekteki Master Build Prompt yapay zekâya şunu söylemelidir:

> Kilitli her gereksinimi karşılayan en basit ve en sağlam uygulamayı inşa et. İlgisiz sistemler ekleme. Bir değer yapılandırılabilir olduğunda, varsayımları koda gömmek yerine yapılandırma (configuration) olarak dışa aç.

---

# 360. Master Prompt Geliştirme Stratejisi

Gelecekteki yapay zekâ projeyi aşamalar halinde inşa etmelidir.

Önerilen sıra:

```text
Aşama 1:
Temel sahne + mortar + kamera

Aşama 2:
Mermi + dünya koordinatı hedefleme

Aşama 3:
Fizik objeleri

Aşama 4:
Stand + seviye tamamlama mantığı

Aşama 5:
Seviye ilerlemesi

Aşama 6:
Tutorial

Aşama 7:
Can sistemi + kaybetme akışı

Aşama 8:
Skill'ler

Aşama 9:
Ekonomi

Aşama 10:
Arayüz / Ana Menü / Profil

Aşama 11:
Dükkan / Reklamlar

Aşama 12:
Lider Tablosu / Bulut

Aşama 13:
Cila / Polish

Aşama 14:
Testler
```

---

# 361. Geliştirme Kontrol Kapısı

Her ana aşamadan sonra yapay zekâ daha önce çalışan işlevlerin bozulmadığını (regresyon olmadığını) doğrulamalıdır.

Sonraki bir sistem şunları kesinlikle bozmamalıdır:

- Ateş etme
- Hedef alma
- Fizik
- Seviye tamamlama
- Kayıt durumu

---

# 362. Çekirdek Sistem Bağımsızlığı

İsteğe bağlı harici sistemler başarısız olsa bile oynanış çalışmaya devam etmelidir.

Örneğin:

```text
Lider tablosu erişilemez:
Oynanış yine de çalışır.

Reklamlar yüklenemedi:
Oynanış yine de çalışır.

Bulut erişilemez:
Yerel ilerleme yine de çalışır.

Dükkan erişilemez:
Önceden sahip olunan kaynaklar güvende kalır.
```

---

# 363. Oynanış Otoritesi Olarak Yerel Durum

Çevrimdışı oynanış için yerel durum anında erişilebilir oynanış otoritesi kabul edilmelidir.

Ağ servisleri ana oynanışı gereksiz yere engellemek yerine arka planda senkronize olmalıdır.

---

# 364. Bulut Senkronizasyon Felsefesi

Bulut senkronizasyonu şu niteliklerde olmalıdır:

```text
Mümkün olan yerlerde engellemesiz (Non-blocking)
Tekrar denenebilir (Retryable)
Mükerrer yazmalara karşı korumalı
Mükerrer ödüllere karşı korumalı
MVP'de "en son yerel ilerleme kazanır" kuralıyla tutarlı
```

---

# 365. Nihai Mimari Bağımsızlığı

Master Build Prompt zorunlu olmadıkça tek bir mimariyi dayatmamalıdır.

Yapay zekâ şu konularda seçim yapabilir:

- Sahne yapısı
- Script organizasyonu
- Veri formatı
- Dependency injection yaklaşımı
- Servis mimarisi
- UI mimarisi

Yeter ki oyuncuya yansıyan davranışlar dokümantasyona tam olarak uysun.

---

# 366. Gelecek İçerik Genişlemesi

MVP mimarisi şunların eklenmesini kolaylaştırmalıdır:

```text
Seviye 31+
Yeni obje türleri
Yeni arka planlar
Yeni skill'ler
Yeni teklifler
Yeni kozmetikler
```

Bunun için çekirdek mermi/fizik sistemini yeniden yazmak gerekmemelidir.

---

# 367. Gelecek Obje Genişlemesi

Yeni objeler kavramsal olarak aynı arayüzü uygulamalıdır:

```text
Obje Türü
+
Fiziksel Özellikler
+
Hasar / Kırılma Davranışı
+
Görsel Durum
+
Ses
+
VFX
```

Bu sayede ana oyun döngüsünü değiştirmeden yeni içerikler eklenebilir.

---

# 368. Gelecek Skill Genişlemesi

Skill'ler ileride yeni yeteneklerin eklenmesine izin verecek şekilde tasarlanmalıdır.

Mevcut MVP yalnızca şunları gerektirir:

```text
Mega Ball
Infinity Ball
```

Şu anda gereksiz ekstra skill'ler inşa etmeyin.

---

# 369. Gelecek Ekonomi Genişlemesi

Altın ekonomisi gelecekteki içerikleri destekleyecek kadar modüler olmalıdır.

Bununla birlikte MVP tek para birimi odaklı kalmalıdır.

---

# 370. Nihai Tasarım Özeti

Oyun temelde şu döngü etrafında inşa edilmiştir:

```text
DOKUN (TAP)
↓
HEDEFLE (TARGET)
↓
ATEŞ ET (FIRE)
↓
FİZİK (PHYSICS)
↓
ZİNCİRLEME REAKSİYON (CHAIN REACTION)
↓
OBJELERİ TEMİZLE (CLEAR OBJECTS)
↓
ÖDÜL KAZAN (REWARD)
↓
SONRAKİ SEVİYE (NEXT LEVEL)
```

Geriye kalan her şey bu döngüyü destekler.

---

# 371. Nihai Kritik Kurallar

Aşağıdaki kurallar özellikle kritik kabul edilir:

1. Tap (dokunma) anında ateş eder.
2. Drag (sürükleme) mortarı hareket ettirir/hedefler ve ateş etmez.
3. Top, ekranda dokunulan gerçek dünya koordinatını hedefler.
4. Top fizik kurallarını kullanır.
5. Objeler darbelere fiziksel tepki verir.
6. Toplar sınırlıdır.
7. Toplar sekebilir.
8. Toplar oyuncuya doğru geri gelebilir.
9. Toplar belirli bir süre sonra yok olur.
10. Boşluğa atılan toplar 3D dünyada ilerleyip kaybolur.
11. Kazanmak için gerekli tüm objelerin temizlenmesi gerekir.
12. Toplar bittiğinde objeler kalırsa seviye kaybedilir.
13. Kazanmak can tüketmez.
14. Kaybetmek 1 can tüketir.
15. Aktif bir seviyeden çıkmak 1 can tüketir.
16. Aktif seviyedeyken uygulamayı kapatmak çıkış sayılır ve 1 can tüketir.
17. Can zamanla yenilenir (30 dakikada 1 can).
18. Mega Ball Seviye 5'te açılır.
19. Infinity Ball Seviye 10'da açılır.
20. Skill'ler üst üste kullanılabilir (stack olabilir).
21. Kalan toplar Altın kazandırır (Kalan Top × 2).
22. Altın ana para birimidir.
23. Çevrimdışı oynanış desteklenir.
24. Çevrimdışı ilerleme bulutla senkronize olabilir.
25. MVP'de en son yerel ilerleme kazanır.
26. Lider tablosu çevrimdışı önbellek gösterimini destekler.
27. No Ads yalnızca zorunlu/geçiş reklamlarını kaldırır.
28. Ödüllü reklamlar her zaman kullanılabilir kalır.
29. Tam olarak 30 MVP seviyesi vardır.
30. Seviye 30'da normal Win Paneli açılır ancak "Sonraki Seviye" butonu bulunmaz.

---

# 372. Nihai Dokümantasyon Bildirimi

Bu dokümantasyon, gelecekteki bir **Master Build Prompt** için kaynak materyal olarak kullanılmak üzere hazırlanmıştır.

Master Build Prompt bu belgeyi görmezden gelerek veya belgelenen davranışları genel oyun geliştirme varsayımlarıyla değiştirerek oluşturulmamalıdır.

Bu spesifikasyonu net, kesin ve uygulanabilir geliştirme talimatlarına dönüştürmelidir.

---

# 373. Nihai Devir Kuralı

Master Build Prompt oluşturulurken:

```text
Gereksinimleri kaybetmeyin.
Onaylanmış değerler uydurmayın.
Yaklaşık değerleri kesin kural gibi ele almayın.
Gereksiz kapsam eklemeyin.
Önemli davranışları çıkarmayın.
Dünya koordinatı hedeflemeyi basit açı nişanlamasıyla değiştirmeyin.
Fizik sistemini açıkça gerekmedikçe sahte animasyonlarla değiştirmeyin.
Çevrimdışı desteğini kaldırmayın.
Onaylanan can kurallarını kaldırmayın.
Onaylanan skill açılış ilerlemesini kaldırmayın.
Seviye 30 tamamlama davranışını değiştirmeyin.
```

---