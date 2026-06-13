# TyreService Web

Публичный frontend TyreService на Vite, React и TypeScript.

## Команды

```powershell
npm ci
npm run dev
npm run build
npm run preview
```

## Переменные окружения

Создайте локальный `.env` на основе `.env.example`.

```text
VITE_API_URL=http://localhost:5000/api/customer
VITE_YMAPS_API_KEY=...
APP_URL=http://localhost:5173
```

`VITE_API_URL` должен указывать на customer API backend-приложения.

## Локальный запуск

Из корня репозитория предпочтительно запускать общий скрипт:

```powershell
.\start.ps1
```

Для запуска только frontend:

```powershell
cd web
npm run dev
```

## Сборка

```powershell
cd web
npm ci
npm run build
```

Результат сборки находится в `dist/` и не коммитится.
