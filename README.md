# TyreService

TyreService - система для шиномонтажной мастерской с двумя интерфейсами:

- `TyreServiceApp/` - ASP.NET Core MVC приложение для сотрудников, владельца и администратора
- `web/` - Vite/React frontend для клиентского кабинета и публичной части

Дополнительно в репозитории есть локальный OCR-сервис и служебные скрипты для разработки и деплоя.

## Структура репозитория

- `TyreServiceApp/` - backend, Razor Views, EF Core, SignalR, миграции
- `web/` - клиентский frontend на React + Vite
- `ocr-service/` - локальный FastAPI/EasyOCR сервис
- `scripts/` - вспомогательные SQL и setup-скрипты
- `docs/` - документация по деплою
- `start.ps1` - локальный запуск основных сервисов

## Что входит в проект

- админские и рабочие интерфейсы на Razor Pages/MVC
- клиентский API и клиентский кабинет
- управление заказами, автомобилями, шинами, отзывами и справочниками
- OCR для распознавания документов
- S3-compatible хранение файлов

## Ссылки

- фронтенд: [https://tyre-service.vercel.app](https://tyre-service.vercel.app)
- бэкенд: [https://tyreservice-production.up.railway.app](https://tyreservice-production.up.railway.app)

Владелец:
Логин: owner
Пароль: owner

Администратор:
Логин: admin
Пароль: admin

## Требования для локального запуска

- .NET SDK 9
- Node.js 22+
- PostgreSQL
- Python и зависимости для `ocr-service`
- MinIO для локального S3-compatible хранения

## Быстрый старт

Из корня репозитория:

```powershell
.\start.ps1 -DbPassword "пароль_бд"
```

После запуска используются адреса:

- frontend: `http://localhost:5173`
- backend: `http://localhost:5000`
- MinIO API: `http://localhost:9000`
- MinIO Console: `http://localhost:9001`
- OCR service: `http://localhost:5003`

## Локальная сборка по частям

Backend:

```powershell
dotnet restore TyreServiceApp\TyreServiceApp.sln
dotnet build TyreServiceApp\TyreServiceApp.sln
```

Frontend:

```powershell
cd web
npm ci
npm run build
cd ..
```

## Конфигурация

Секреты и production-настройки не должны храниться в репозитории. Используйте:

- переменные окружения
- `appsettings.json` вне Git
- user secrets для локальной разработки

Ключевые переменные:

- `ConnectionStrings__DefaultConnection`
- `Jwt__Key`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Frontend__Origins__0`
- `Ocr__Url`
- `Minio__Endpoint`
- `Minio__AccessKey`
- `Minio__SecretKey`
- `Minio__Bucket`
- `Minio__UseSSL`
- `App__TimeZone`
- `VITE_API_URL`
- `VITE_YMAPS_API_KEY`

## CI

В репозитории настроен общий GitHub Actions workflow:

- backend: `dotnet restore` + `dotnet build`
- frontend: `npm ci` + `npm run build`

Файл workflow: [`.github/workflows/ci.yml`](.github/workflows/ci.yml)

## Документация

- общая схема деплоя: [`docs/deployment.md`](docs/deployment.md)
- frontend: [`web/README.md`](web/README.md)
- frontend deploy: [`web/docs/deployment.md`](web/docs/deployment.md)
- backend API: [`TyreServiceApp/TyreServiceApp/docs/api_endpoints.md`](TyreServiceApp/TyreServiceApp/docs/api_endpoints.md)
- backend сущности: [`TyreServiceApp/TyreServiceApp/docs/entities.md`](TyreServiceApp/TyreServiceApp/docs/entities.md)
