# Деплой TyreService

Этот документ описывает рекомендуемую схему деплоя, ограничения бесплатных хостингов и минимальный набор переменных окружения.

## Рекомендуемая архитектура

TyreService лучше разворачивать раздельно:

- [`web/`](../web) - на Vercel как Vite SPA
- `TyreServiceApp/TyreServiceApp` - на ASP.NET-хостинге
- PostgreSQL - отдельно как managed database
- файлы - во внешнем S3-compatible storage
- OCR - отдельным сервисом или отключаемой опцией

## Рекомендуемые варианты

### Вариант 1: Vercel + RunASP

- frontend: Vercel
- backend: RunASP
- database: PostgreSQL
- storage: внешний S3-compatible сервис

Что учесть:

- проверить поддержку .NET 9 или использовать совместимую среду публикации
- задать timezone через переменные приложения, а не через локальное время сервера
- не хранить пользовательские файлы на локальном диске хостинга

### Вариант 2: Vercel + Render

- frontend: Vercel
- backend: Render Web Service
- database: Render PostgreSQL, Neon или Supabase

Что учесть:

- free-tier может усыплять backend после простоя
- первый запрос после сна может быть медленным
- OCR лучше выносить отдельно

### Вариант 3: Vercel + Railway

- frontend: Vercel
- backend: Railway
- database: Railway PostgreSQL или внешний PostgreSQL

Что учесть:

- лимиты бесплатного или пробного тарифа могут меняться
- локальное файловое хранение не подходит для production

## Таймзона

Основная бизнес-таймзона проекта: `Asia/Yekaterinburg`.

Дату и время нужно фиксировать через конфиг приложения.

Рекомендуемые значения:

```text
App__TimeZone=Asia/Yekaterinburg
TZ=Asia/Yekaterinburg
```

## Frontend на Vercel

Настройки проекта:

- Root Directory: `web`
- Install Command: `npm ci`
- Build Command: `npm run build`
- Output Directory: `dist`

Переменные окружения:

```text
VITE_API_URL=https://your-backend.example.com/api/customer
VITE_YMAPS_API_KEY=...
```

После деплоя нужно проверить:

- открывается публичный frontend
- запросы уходят на production backend
- клиентская авторизация работает через HTTPS

## Backend на ASP.NET-хостинге

Публикация:

```powershell
dotnet publish TyreServiceApp\TyreServiceApp\TyreServiceApp.csproj -c Release -o artifacts\publish\backend
```

Минимальный production env:

```text
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Host=...;Port=5432;Database=...;Username=...;Password=...
Jwt__Key=long-production-secret-at-least-32-chars
Jwt__Issuer=TyreServiceApp
Jwt__Audience=premium-shinomontazh
Frontend__Origins__0=https://your-frontend.vercel.app
Ocr__Url=https://your-ocr-service.example.com/ocr
Minio__Endpoint=...
Minio__AccessKey=...
Minio__SecretKey=...
Minio__Bucket=uploads
Minio__UseSSL=true
App__TimeZone=Asia/Yekaterinburg
```

Если backend работает не из корня домена, дополнительно проверьте:

- base URL клиентского API
- CORS
- callback URL для авторизации, если она зависит от домена

## OCR и файлы

Для production не стоит опираться на локальные dev-артефакты:

- `ocr-service/` подходит для локальной среды и тестов
- `MinIO` в репозитории не должен считаться production storage
- `tessdata/` и OCR-модели нужно хранить осознанно, с учётом размера и способа запуска

Рекомендуется:

- выносить OCR в отдельный сервис
- хранить файлы в S3-compatible storage
- не писать пользовательские данные на локальный диск контейнера
