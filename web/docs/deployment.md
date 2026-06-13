# Деплой frontend

Этот документ описывает деплой папки `web/` как отдельного Vite-приложения.

## Рекомендуемый хостинг

Для frontend лучше использовать Vercel:

- хорошо подходит для Vite SPA
- быстро подключается к GitHub
- удобно задавать env-переменные
- нормально работает с React Router через rewrites

## Настройки Vercel

- Root Directory: `web`
- Install Command: `npm ci`
- Build Command: `npm run build`
- Output Directory: `dist`

Если используется SPA routing, в проекте нужен `vercel.json` с rewrite на `index.html`.

## Production env

Минимальный набор:

```text
VITE_API_URL=https://your-backend.example.com/api/customer
VITE_YMAPS_API_KEY=...
```
