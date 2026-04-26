# 🏆PremierVision

PremierVision, İngiltere Premier Ligi’ne ait maçları, fikstürü, puan durumunu ve maç detaylarını takip etmek amacıyla geliştirilmiş bir web uygulamasıdır. Uygulama, kullanıcıya güncel ve detaylı futbol verilerini anlaşılır ve modern bir arayüz üzerinden sunmayı hedefler.

Proje kapsamında, ligdeki takımların maç performansları, haftalık fikstürler, canlı ve tamamlanmış karşılaşmalar ile birlikte maçlara ait detaylı olaylar ve istatistikler görüntülenebilmektedir. Ayrıca admin paneli aracılığıyla sistem içerisine manuel veri girişi yapılabilmektedir.


### ⚙️ Özellikler 

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

### 🛠️ Kullanılan Teknolojiler

.NET 10
ASP.NET Core MVC
ASP.NET Core Web API
Entity Framework Core
SQL Server
Bootstrap
Bootstrap Icons
Razor View Engine

### 🏗️ Proje Mimarisi

Proje, modern web uygulama geliştirme prensiplerine uygun olarak katmanlı bir mimari ile geliştirilmiştir.

Backend (API) katmanı, veri yönetimi ve iş kurallarından sorumludur
Frontend (MVC) katmanı, kullanıcı arayüzünü oluşturur ve API üzerinden gelen verileri kullanır

Bu yapı sayesinde:
Veri erişimi ile arayüz birbirinden ayrılmıştır
Kodun sürdürülebilirliği ve okunabilirliği artırılmıştır
Farklı istemcilerin aynı API’yi kullanabilmesi mümkün hale gelmiştir

### 📊 Veri Yapısı ve İşleyiş

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

### 🎯 Kullanım Amacı

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

### Ana sayfalar

```text
/                       Ana sayfa
/index.html             Ana sayfa (alternatif)
/fixtures.html          Fikstür listesi
/standings.html         Puan durumu tablosu
/match-detail.html/{id} Maç detayı
/admin                  Admin paneli
```

### 📸 Ekran Görüntüleri

<img width="1903" height="953" alt="Home Page" src="https://github.com/user-attachments/assets/f9323ebf-d08d-4a40-b7d2-ecc8c4d623fc" />

<img width="1906" height="948" alt="Fixtures Page" src="https://github.com/user-attachments/assets/ff806f68-d102-4d2a-a9ee-d2d7afa7e90c" />
<img width="941" height="2962" alt="Mobile View" src="https://github.com/user-attachments/assets/1c405d63-eed3-4cb9-8dbe-a22d5eece632" />

<img width="1904" height="950" alt="Standings Page" src="https://github.com/user-attachments/assets/e86d7b50-ffdb-47cd-98d6-25d1cdcc980b" />


<img width="1906" height="952" alt="Match Detail Page" src="https://github.com/user-attachments/assets/01e568b3-d752-4a24-9b62-228c25cd6ab7" />

<img width="1904" height="954" alt="Match Events" src="https://github.com/user-attachments/assets/9a573421-ffd7-4bfd-a76a-076669574883" />

<img width="1906" height="952" alt="Match Statistics" src="https://github.com/user-attachments/assets/d69571df-15ea-41f7-8798-0289c0f6c0ed" />

<img width="1901" height="948" alt="Admin Page" src="https://github.com/user-attachments/assets/5eb2723f-3b13-46cf-ae37-5ed580157528" />

<img width="1901" height="948" alt="Admin Fixture Form" src="https://github.com/user-attachments/assets/952b6a3a-e067-4f4b-9442-5c227180dd93" />

<img width="1906" height="951" alt="Admin Match Event Form" src="https://github.com/user-attachments/assets/21794431-4c78-45b8-bc51-c072056063a0" />

<img width="1907" height="951" alt="Admin Statistics Form" src="https://github.com/user-attachments/assets/963b5708-e126-424b-963c-13b9c3af5dda" />

<img width="1907" height="952" alt="Fixture Detail View" src="https://github.com/user-attachments/assets/3416c3f7-62c0-4655-aa18-d03b27650ac9" />

<img width="1905" height="949" alt="League Overview" src="https://github.com/user-attachments/assets/4c4dddc5-9980-48b5-92a7-2b235824c43a" />

<img width="1045" height="1521" alt="Responsive View" src="https://github.com/user-attachments/assets/a71e7212-55e3-45c7-988a-5a114cace06c" />

<img width="1903" height="950" alt="Final Screenshot" src="https://github.com/user-attachments/assets/c091e85f-41a0-4564-83bc-e884aa1750b2" />


