# 🏆PremierVision

PremierVision, İngiltere Premier Ligi’ne ait maçları, fikstürü, puan durumunu ve maç detaylarını takip etmek amacıyla geliştirilmiş bir web uygulamasıdır. Uygulama, kullanıcıya güncel ve detaylı futbol verilerini anlaşılır ve modern bir arayüz üzerinden sunmayı hedefler.

Proje kapsamında, ligdeki takımların maç performansları, haftalık fikstürler, canlı ve tamamlanmış karşılaşmalar ile birlikte maçlara ait detaylı olaylar ve istatistikler görüntülenebilmektedir. Ayrıca admin paneli aracılığıyla sistem içerisine manuel veri girişi yapılabilmektedir.


###⚙️ Özellikler

Haftalık maç sonuçlarının listelenmesi
Canlı, tamamlanmış ve yaklaşan maçların ayrı ayrı gösterimi
Öne çıkan maç kartı ile canlı dakika takibi
Maç haftasına göre gruplanmış fikstür yapısı
Detaylı maç sayfası:
Skor bilgisi
Maç içi olaylar (gol, kart, oyuncu değişikliği)
İstatistikler
Tamamlanan maçlara göre dinamik olarak hesaplanan puan durumu
Takımların form durumlarının (son maç performansı) gösterimi
Admin panel üzerinden:
Maç ekleme
Maç olayı ekleme
Maç istatistiği ekleme
Responsive (mobil uyumlu) kullanıcı arayüzü

###🛠️ Kullanılan Teknolojiler

.NET 10
ASP.NET Core MVC
ASP.NET Core Web API
Entity Framework Core
SQL Server
Bootstrap
Bootstrap Icons
Razor View Engine

###🏗️ Proje Mimarisi

Proje, modern web uygulama geliştirme prensiplerine uygun olarak katmanlı bir mimari ile geliştirilmiştir.

Backend (API) katmanı, veri yönetimi ve iş kurallarından sorumludur
Frontend (MVC) katmanı, kullanıcı arayüzünü oluşturur ve API üzerinden gelen verileri kullanır

Bu yapı sayesinde:
Veri erişimi ile arayüz birbirinden ayrılmıştır
Kodun sürdürülebilirliği ve okunabilirliği artırılmıştır
Farklı istemcilerin aynı API’yi kullanabilmesi mümkün hale gelmiştir

###📊 Veri Yapısı ve İşleyiş

Uygulamada temel olarak şu veri yapıları kullanılmaktadır:
Teams (Takımlar)
Fixtures (Maçlar / Fikstür)
Match Events (Maç Olayları)
Match Statistics (Maç İstatistikleri)

Puan durumu veritabanında ayrı bir tablo olarak tutulmaz.
Bunun yerine tamamlanan maç sonuçları üzerinden dinamik olarak hesaplanır:

Galibiyet: 3 puan
Beraberlik: 1 puan
Mağlubiyet: 0 puan

Ayrıca:
Atılan ve yenilen goller
Averaj (gol farkı)
Son maç performansı
gibi değerler de anlık olarak hesaplanır.

###🎯 Kullanım Amacı

Bu proje:

Futbol verisi işleme ve görselleştirme
API ve MVC mimarisini birlikte kullanma
Entity ve ViewModel ayrımını uygulama
Gerçek dünya senaryosuna yakın bir sistem geliştirme
amaçlarıyla geliştirilmiştir.

### PremierVision.API

Ana Endpoint'ler:

```text
GET  /api/home
GET  /api/home?week=38
GET  /api/fixtures
GET  /api/fixtures?week=38
GET  /api/matches/{id}
GET  /api/standings
GET  /api/admin/options
POST /api/admin/fixtures
POST /api/admin/events
POST /api/admin/statistics
```

### PremierVision.UI

PremierVisionApiClient aracılığıyla API’yi tüketen MVC frontend uygulamasıdır.

Ana sayfalar:

```text
/                 Ana sayfa
/index.html       Ana sayfa (alternatif)
/fixtures.html    Fikstür listesi
/standings.html   Puan durumu tablosu
/match-detail.html/{id}
/admin            Admin paneli
