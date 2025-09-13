using UnityEngine;

/// <summary>
/// Ses sistemi kurulum rehberi - Bu script'i silmeyin, referans için kalsın
/// </summary>
public class SoundSetupGuide : MonoBehaviour
{
    [Header("🎵 SES SİSTEMİ KURULUM REHBERİ")]
    [Space]
    [TextArea(10, 20)]
    public string setupGuide = @"
🎮 OYUNCU SESLERİ (PlayerControl):
├── Dash Start Sound: 1 adet (Space tuşuna basınca)
├── Dash Impact Sound: 1 adet (Dash bitince)
├── Attack Sounds: 5 adet
│   ├── [0] Punch sesi
│   ├── [1] Kick sesi  
│   ├── [2] MMA Kick sesi
│   ├── [3] Heavy Attack 1 sesi
│   └── [4] Heavy Attack 2 sesi
├── Hit Sound: 1 adet (hasar alınca)
└── Death Sound: 1 adet (ölünce)

🔊 SES SEVİYESİ KONTROLLERİ:
├── Dash Volume: 0-1 arası (Dash seslerinin seviyesi)
├── Attack Volume: 0-1 arası (Saldırı seslerinin seviyesi)
├── Hit Volume: 0-1 arası (Hasar alma sesinin seviyesi)
└── Death Volume: 0-1 arası (Ölüm sesinin seviyesi)

📏 3D SES MESAFE AYARLARI:
├── Min Distance: Minimum mesafe (tam ses duyulur)
└── Max Distance: Maksimum mesafe (sessiz olur)

🐉 DRAGON SESLERİ (DragonEnemy):
├── Roar Sound: 1 adet (saldırı başlangıcında)
├── Explosion Sound: 1 adet (patlama anında)
└── Idle Sound: 1 adet (bekleme durumunda - 6-12 saniye aralıklarla)

🔊 DRAGON SES SEVİYESİ KONTROLLERİ:
├── Roar Volume: 0-1 arası (Kükreme ses seviyesi)
├── Explosion Volume: 0-1 arası (Patlama ses seviyesi)
└── Idle Volume: 0-1 arası (Bekleme ses seviyesi)

📏 DRAGON 3D SES MESAFE AYARLARI:
├── Min Distance: 5 metre (tam ses) - Sabit kodlanmış
└── Max Distance: 50 metre (sessiz) - Sabit kodlanmış

🧬 MUTANT SESLERİ (MutantEnemy):
├── Normal Attack Sound: 1 adet (normal saldırı)
├── Heavy Attack Sound: 1 adet (ağır saldırı)
├── Jump Attack Sound: 1 adet (zıplama saldırısı)
├── Landing Sound: 1 adet (iniş sesi)
├── Hit Sound: 1 adet (hasar alma)
├── Death Sound: 1 adet (ölüm)
└── Idle Sound: 1 adet (bekleme - 8-15 saniye aralıklarla)

🔊 MUTANT SES SEVİYESİ KONTROLLERİ:
├── Normal Attack Volume: 0-1 arası (Normal saldırı ses seviyesi)
├── Heavy Attack Volume: 0-1 arası (Ağır saldırı ses seviyesi)
├── Jump Attack Volume: 0-1 arası (Zıplama saldırısı ses seviyesi)
├── Landing Volume: 0-1 arası (İniş ses seviyesi)
├── Hit Volume: 0-1 arası (Hasar alma ses seviyesi)
├── Death Volume: 0-1 arası (Ölüm ses seviyesi)
└── Idle Volume: 0-1 arası (Bekleme ses seviyesi)

📏 MUTANT 3D SES MESAFE AYARLARI:
├── Min Distance: 4 metre (tam ses) - EnemyBase'den alınır
└── Max Distance: 30 metre (sessiz) - EnemyBase'den alınır

🧙 WIZARD SESLERİ (WizardEnemy):
├── Normal Attack Sound: 1 adet (normal saldırı)
├── Heavy Attack Sound: 1 adet (ağır saldırı)
├── Fireball Cast Sound: 1 adet (fireball fırlatma)
├── Hit Sound: 1 adet (hasar alma)
├── Death Sound: 1 adet (ölüm)
└── Idle Sound: 1 adet (bekleme - 10-18 saniye aralıklarla)

🔊 WIZARD SES SEVİYESİ KONTROLLERİ:
├── Normal Attack Volume: 0-1 arası (Normal saldırı ses seviyesi)
├── Heavy Attack Volume: 0-1 arası (Ağır saldırı ses seviyesi)
├── Fireball Cast Volume: 0-1 arası (Fireball fırlatma ses seviyesi)
├── Hit Volume: 0-1 arası (Hasar alma ses seviyesi)
├── Death Volume: 0-1 arası (Ölüm ses seviyesi)
└── Idle Volume: 0-1 arası (Bekleme ses seviyesi)

📏 WIZARD 3D SES MESAFE AYARLARI:
├── Min Distance: 4 metre (tam ses) - EnemyBase'den alınır
└── Max Distance: 30 metre (sessiz) - EnemyBase'den alınır

⚙️ KURULUM ADIMLARI:

🐉 DRAGON İÇİN:
1. Dragon prefab'ını seç
2. Inspector'da 'Sound Settings' bölümünü bul
3. Audio Source otomatik oluşturulur (3D ses ayarları otomatik)
4. 3 ses dosyasını sürükle-bırak yap:
   - Roar Sound: Kükreme sesi
   - Explosion Sound: Patlama sesi
   - Idle Sound: Bekleme sesi
5. 'Play Sounds' işaretli olduğundan emin ol

🧬 MUTANT İÇİN:
1. Mutant prefab'ını seç
2. Inspector'da 'Sound Settings' bölümünü bul
3. Audio Source otomatik oluşturulur (3D ses ayarları otomatik)
4. 7 ses dosyasını sürükle-bırak yap:
   - Normal Attack Sound: Normal saldırı sesi
   - Heavy Attack Sound: Ağır saldırı sesi
   - Jump Attack Sound: Zıplama saldırısı sesi
   - Landing Sound: İniş sesi
   - Hit Sound: Hasar alma sesi
   - Death Sound: Ölüm sesi
   - Idle Sound: Bekleme sesi
5. 'Play Sounds' işaretli olduğundan emin ol

🧙 WIZARD İÇİN:
1. Wizard prefab'ını seç
2. Inspector'da 'Sound Settings' bölümünü bul
3. Audio Source otomatik oluşturulur (3D ses ayarları otomatik)
4. 6 ses dosyasını sürükle-bırak yap:
   - Normal Attack Sound: Normal saldırı sesi
   - Heavy Attack Sound: Ağır saldırı sesi
   - Fireball Cast Sound: Fireball fırlatma sesi
   - Hit Sound: Hasar alma sesi
   - Death Sound: Ölüm sesi
   - Idle Sound: Bekleme sesi
5. 'Play Sounds' işaretli olduğundan emin ol

🔊 3D SES AYARLARI (Otomatik):
- Dragon: 5-50 metre arası (tam ses → sessiz)
- Player: 3-25 metre arası (tam ses → sessiz)  
- Diğer Düşmanlar: 4-30 metre arası (tam ses → sessiz)
- Mesafeye göre ses azalır (gerçekçi)

🎯 OYUNCU İÇİN:
1. Player prefab'ını seç  
2. Inspector'da 'Sound Settings' bölümünü bul
3. Audio Source otomatik oluşturulur
4. 7 ses dosyasını ata:
   - Dash Start Sound: Dash başlangıç sesi
   - Dash Impact Sound: Dash iniş sesi
   - Attack Sounds: 5 adet (punch, kick, mmakick, heavy1, heavy2)
   - Hit Sound: Hasar alma sesi
   - Death Sound: Ölüm sesi
5. 'Sound Volume Controls' bölümünden ses seviyelerini ayarla:
   - Dash Volume: 0-1 arası slider
   - Attack Volume: 0-1 arası slider
   - Hit Volume: 0-1 arası slider
   - Death Volume: 0-1 arası slider
6. '3D Sound Distance Settings' bölümünden mesafe ayarlarını yap:
   - Min Distance: 3 metre (tam ses)
   - Max Distance: 25 metre (sessiz)

🧬 MUTANT İÇİN:
1. Mutant prefab'ını seç
2. Inspector'da 'Sound Settings' bölümünü bul
3. Audio Source otomatik oluşturulur
4. 7 ses dosyasını ata (yukarıdaki listeye göre)
5. 'Sound Volume Controls' bölümünden ses seviyelerini ayarla
6. '3D Sound Distance Settings' bölümünden mesafe ayarlarını yap (EnemyBase'den alınır)

🧙 WIZARD İÇİN:
1. Wizard prefab'ını seç
2. Inspector'da 'Sound Settings' bölümünü bul
3. Audio Source otomatik oluşturulur
4. 6 ses dosyasını ata (yukarıdaki listeye göre)
5. 'Sound Volume Controls' bölümünden ses seviyelerini ayarla
6. '3D Sound Distance Settings' bölümünden mesafe ayarlarını yap (EnemyBase'den alınır)

🐉 DRAGON İÇİN:
1. Dragon prefab'ını seç
2. Inspector'da 'Sound Settings' bölümünü bul
3. Audio Source otomatik oluşturulur
4. 3 ses dosyasını ata (roar, explosion, idle)
5. 'Sound Volume Controls' bölümünden ses seviyelerini ayarla
6. 3D ses mesafeleri sabit kodlanmış (5m-50m)

✅ TEST ETME:
- Dragon saldırı yaparken kükreme + patlama sesi çalmalı
- Dragon bekleme durumunda 6-12 saniye aralıklarla idle sesi çalmalı
- Oyuncu dash yaparken 2 ses çalmalı (başlangıç + iniş)
- Oyuncu düşmana vurduğunda saldırı sesi çalmalı
- Hedef yokken oyuncu sesi çalmamalı

🎧 3D SES TESTİ:
- Dragon'a yaklaş → Kükreme sesi yüksek duyulmalı
- Dragon'dan uzaklaş → Ses azalmalı (50m'de sessiz)
- Mutant'a yaklaş → Saldırı sesleri yüksek duyulmalı
- Wizard'a yaklaş → Fireball sesleri yüksek duyulmalı
- Düşmanlardan uzaklaş → Sesler azalmalı (30m'de sessiz)
- Oyuncu sesleri → Sadece yakında duyulmalı (25m'de sessiz)

🎵 IDLE SES TESTİ:
- Dragon bekleme durumunda → 6-12 saniye aralıklarla kükreme
- Mutant bekleme durumunda → 8-15 saniye aralıklarla ses
- Wizard bekleme durumunda → 10-18 saniye aralıklarla ses
";
}
