# Main menu — şeffaflık kontrolü

2026-09-22 tarihinde kullanıcının işaretlediği 12 UI PNG'si kontrol edildi. Dış alanlarında zaten gerçek alpha=0 bulundu; görüntülere arka plan silme veya yeniden üretme işlemi gerekmedi.

Unity import ayarları 12 dosyada güncellendi: Sprite/Single, Alpha Source=Input, Alpha Is Transparency=true, mipmap kapalı, NPOT=None ve Wrap=Clamp. Böylece özgün piksel boyutları korunur ve görseller UI Image için sprite olarak kullanılabilir. Dosya GUID'leri aynı kaldı.

Klasördeki 13. dosya, 23_28_01 (1), tam ekran menü illüstrasyonudur; siyah arka planlı UI sprite değildir ve değiştirilmedi. Tüm 13 kaynak PNG'nin SHA256 değeri işlem öncesiyle eşittir.

- Yedek: C:/Users/Farzetki/.codex/local-backups/MainMenuAlpha-20260922-174838/
- alpha-import-verification.json: alpha sayımları, kaynak/import boyutları ve GUID doğrulaması.
- source-preservation.json: kaynak PNG hash karşılaştırması.
- transparent-sprites-checkerboard.png: Unity'de geçici preview sahnesinde 12 sprite'ın açık renk dama zemin üzerindeki render'ı. Dama zemini yalnızca kontrol görüntüsündedir; kaynak PNG'lere eklenmedi.

Kontrol render'ı açılarak incelendi; siyah dikdörtgen arka plan yok. MainMenu sahnesi değiştirilmedi; Console'da hata/uyarı yok. Görsel üretim aracı kullanılmadı.
