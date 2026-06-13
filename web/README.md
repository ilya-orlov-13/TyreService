# TyreService Web

`web/` - клиентский frontend проекта TyreService на Vite, React и TypeScript.

## Что делает frontend

- показывает публичную главную страницу
- даёт клиенту доступ к кабинету
- работает с customer API backend-приложения
- использует env-переменные для production URL и внешних ключей

## Команды

```powershell
npm ci
npm run dev
npm run build
npm run preview
```

## Переменные окружения

Создайте локальный `.env` на основе [`.env.example`](.env.example).

Минимальный пример:

```text
VITE_API_URL=http://localhost:5000/api/customer
VITE_YMAPS_API_KEY=...
```

Если нужен production backend, `VITE_API_URL` должен указывать на его customer API.

## Локальный запуск

Из корня репозитория:

```powershell
.\start.ps1
```

Отдельный запуск только frontend:

```powershell
cd web
npm run dev
```

По умолчанию dev-сервер доступен на `http://localhost:5173`.

## Сборка

```powershell
cd web
npm ci
npm run build
```

Артефакты попадают в `dist/` и не должны коммититься.

## Интеграция с backend

Frontend ожидает, что backend:

- доступен по HTTP или HTTPS
- отвечает на customer API
- настроен на CORS для frontend origin

Основная точка интеграции:

- `VITE_API_URL`

## Деплой

[`web/docs/deployment.md`](docs/deployment.md)
