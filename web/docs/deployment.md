# Деплой frontend

## Vercel

Настройки проекта:

- Root Directory: `web`
- Install Command: `npm ci`
- Build Command: `npm run build`
- Output Directory: `dist`

## Production env

```text
VITE_API_URL=https://your-backend.example.com/api/customer
VITE_YMAPS_API_KEY=...
APP_URL=https://your-frontend.vercel.app
```

## Проверка

```powershell
cd web
npm ci
npm run build
```

После деплоя проверьте:

- главная страница открывается по Vercel URL;
- авторизация клиента обращается к production backend;
- CORS на backend разрешает домен frontend;
- карты получают ключи из env.
