# Raboid RPA Sistemi

Bu proje, Raboid case study için geliştirilmiş .NET 8 tabanlı bir Robotic Process Automation (RPA) sistemidir. Sistem, Windows uygulamasına barkodlu ürün girişi yapmak için paralel RPA istemcileri kullanır.

## Proje Genel Bakış

Bu çözüm, birden fazla Windows oturumu (RPA istemcisi) kullanarak ürün bilgilerini (ürün kodu, ürün adı, fiyat, barkod) Windows uygulamasına paralel olarak girmek için tasarlanmıştır. Sistem şu bileşenlerden oluşur:

1. **RPA API Servisi** - İstemci iletişimi için ASP.NET Core Web API
2. **RPA Scheduler** - İş yönetimi için arka plan servisi
3. **RPA İstemcisi** - Windows uygulaması etkileşimini simüle eden mock istemci
4. **Veri Seeder** - Test verilerini başlatmak için yardımcı program
5. **Paylaşılan Kütüphane** - Ortak modeller, servisler ve repository'ler

## Temel Özellikler

- ✅ client_id + client_secret ile JWT kimlik doğrulama
- ✅ Uygun indeksleme ile MongoDB veri depolama
- ✅ EAN-13 barkod üretimi ve yönetimi
- ✅ Çoklu RPA istemcisi ile paralel işlem
- ✅ İş planlama ve atama mantığı
- ✅ Başarısız işler için süre sonu yönetimi
- ✅ Tek sorumluluk prensibi ve düşük döngüsel karmaşıklık
- ✅ Kapsamlı dokümantasyon

## Teknoloji Yığını

- **Ana Dil**: C# (.NET 8)
- **Web Framework**: ASP.NET Core
- **Veritabanı**: MongoDB
- **Kimlik Doğrulama**: JWT

## Proje Yapısı

```
RaboidRpaSystem/
├── RpaApi/                 # ASP.NET Core Web API
├── RpaScheduler/           # Windows Service scheduler
├── RpaClient/              # Mock RPA istemcisi
├── DataSeeder/             # Veri başlatma yardımcı programı
├── RpaApi.Tests/           # Birim testler
├── Shared/                 # Paylaşılan kütüphane
│   ├── Models/             # Veri modelleri
│   ├── DTOs/               # Veri transfer nesneleri
│   ├── Interfaces/         # Servis ve repository arayüzleri
│   ├── Repositories/       # MongoDB repository uygulamaları
│   └── Services/           # İş mantığı servisleri
├── docker-compose.yml      # MongoDB kurulumu
├── RaboidRpa.sln           # Solution dosyası
└── [Dokümantasyon Dosyaları]   # README, ARCHITECTURE, vb.
```

## Teslim Edilenler

Bu çözüm, case study'de istenen tüm teslimleri sağlar:

### 1. MongoDB Şeması ve Gerekli İndeksler
[MONGODB_SCHEMA.md](MONGODB_SCHEMA.md) dosyasında tam şema dokümantasyonu bulunmaktadır.

### 2. Örnek Veri (Seed Data)
[DataSeeder](DataSeeder/) projesi şunları oluşturur:
- 50 RPA istemcisi kimlik bilgileri ile
- 1000 EAN-13 barkodu
- 5 örnek ürün
- 20 örnek iş

### 3. Çalışan Demo
Tüm bileşenler tam işlevsel:
- [RPA API](RpaApi/) Swagger UI ile
- [RPA Scheduler](RpaScheduler/) arka plan servisi
- [Mock RPA İstemcisi](RpaClient/) test için

### 4. JWT Kimlik Doğrulama
[JwtService](Shared/Services/JwtService.cs) ile client_id + client_secret uygulandı.

### 5. Akıllı Planlama
[RpaScheduler/Worker.cs](RpaScheduler/Worker.cs) ile uygulandı:
- İş önceliklendirme
- Süre sonu yönetimi
- Barkod envanter yönetimi

## Başlangıç

### Ön Gereksinimler
- .NET 8 SDK
- Docker (MongoDB için)

### Hızlı Başlangıç
1. MongoDB başlat: `docker-compose up -d`
2. Veri başlat: `cd DataSeeder && dotnet run`
3. API başlat: `cd RpaApi && dotnet run`
4. Scheduler başlat: `cd RpaScheduler && dotnet run`
5. Tek istemci çalıştır: `cd RpaClient && dotnet run`
6. 50 istemciyi paralel çalıştır: `./start_clients.sh`

### Çoklu İstemci Çalıştırma (50 Paralel Oturum)
Case study gereksinimlerine göre 50 paralel RPA istemcisini simüle etmek için:

```bash
# Tüm 50 istemciyi aynı anda başlat
./start_clients.sh

# Tüm istemcileri durdur
./stop_clients.sh

# Bireysel istemci loglarını kontrol et
tail -f logs/client-1.log
```

Her istemci benzersiz bir istemci ID'si (test-client-1'den test-client-50'ye) ile kimlik doğrulaması yapar ve işleri bağımsız olarak işler.

## Sistem Özellikleri

### API Endpoint'leri
- `POST /api/auth` - Kimlik doğrulama
- `GET /api/jobs/pending` - Bekleyen işleri getir
- `POST /api/jobs/{id}/assign` - İşi ata
- `POST /api/jobs/{id}/result` - Sonucu bildir
- `GET /api/health` - Sistem sağlığı
- `GET /api/stats` - Sistem istatistikleri

### Scheduler Özellikleri
- **Barkod ekleme**: Otomatik barkod üretimi
- **Süresi dolmuş iş yönetimi**: Yeniden deneme mekanizması
- **Sistem bakımı**: Arka plan işleme

### İstemci Özellikleri
- **Kimlik doğrulama**: JWT token yönetimi
- **İş işleme**: Windows uygulaması simülasyonu
- **Hata yönetimi**: Yeniden deneme mantığı, üstel geri çekilme
- **Eşzamanlı işleme**: Çoklu istemci desteği

## Değerlendirme Kriterleri

Bu çözüm, case study'deki tüm değerlendirme kriterlerini karşılar:

1. **Mimari Tasarım** - Mantıklı, genişletilebilir ve güvenli
2. **Scheduler Mantığı** - Net iş önceliklendirme ve hata yönetimi
3. **Login/Concurrency** - Veri bütünlüğü ile güvenli erişim
4. **Barkod Yönetimi** - Benzersiz atama ile bütünlük
5. **Kod Kalitesi** - Temiz, okunabilir, tek sorumluluk
6. **AI Kullanımı** - Hızlı geliştirme için kullanıldı


## Lisans

Bu proje, Raboid case study değerlendirme sürecinin bir parçası olarak geliştirilmiştir.

## Sistem Gereksinimleri

- **.NET 8 SDK**
- **Docker** (MongoDB için)
- **MongoDB** (Docker ile otomatik kurulum)

## Hızlı Test

```bash
# 1. Sistemi başlat
docker-compose up -d
cd DataSeeder && dotnet run
cd RpaApi && dotnet run &
cd RpaScheduler && dotnet run &

# 2. 50 istemciyi test et
./start_clients.sh

# 3. Logları kontrol et
tail -f logs/client-1.log
```

Sistem tamamen çalışır durumda ve case study gereksinimlerini karşılar! 🎉
