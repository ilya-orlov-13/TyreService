# API backend

Документ описывает backend endpoints по зонам ответственности:

- публичные endpoints
- customer API для React frontend
- внутренние API для MVC-интерфейсов и операционных сценариев

## Базовые адреса

Локальная разработка:

```text
http://localhost:5000
http://localhost:5000/api
http://localhost:5000/api/customer
```

Production URL зависит от хостинга backend.

## Публичные endpoints

Используются публичной частью сайта без внутреннего административного контекста.

### `GET /api/public/reviews`

Возвращает отзывы для публичного frontend.

### `GET /api/public/stats`

Возвращает публичную статистику.

## Customer API

Используется React frontend из [`web/`](../../../web).

Типовые зоны:

- авторизация клиента
- профиль клиента
- автомобили клиента
- заказы клиента

### Auth

Типовые маршруты:

- `/api/customer/auth/...`

Сценарии:

- вход клиента
- подтверждение по телефону или PIN
- получение текущего состояния авторизации

### Profile

Типовые маршруты:

- `/api/customer/profile/...`

Сценарии:

- чтение профиля
- обновление клиентских данных

### Cars

Типовые маршруты:

- `/api/customer/cars/...`

Сценарии:

- получить список машин клиента
- добавить машину
- обновить машину
- удалить машину

### Orders

Типовые маршруты:

- `/api/customer/orders/...`

Сценарии:

- получить список заказов клиента
- просмотреть детали заказа
- создать бронирование или клиентский заказ

## Внутренние API

Используются административными интерфейсами, внутренними экранами и операционными сценариями.

Основные группы:

- `/api/cars`
- `/api/orders`
- `/api/tires`
- `/api/posts`
- `/api/bookings`

## Cars API

### `GET /api/cars`

Возвращает список автомобилей.

### `GET /api/cars/{id}`

Возвращает детальную информацию об автомобиле.

### `POST /api/cars`

Создаёт автомобиль.

Пример тела запроса:

```json
{
  "clientId": 1,
  "brand": "Toyota",
  "model": "Camry",
  "manufactureYear": 2020,
  "licensePlate": "А123БВ777",
  "vin": "JT2BF28K0X0123456"
}
```

### `PUT /api/cars/{id}`

Обновляет автомобиль.

### `DELETE /api/cars/{id}`

Удаляет автомобиль.

### `GET /api/cars/search?licensePlate={plate}&clientId={id}`

Ищет автомобили по номеру и клиенту.

## Tires API

### `GET /api/tires`

Возвращает список шин.

Поддерживает фильтрацию, например по `carId`.

### `GET /api/tires/{id}`

Возвращает одну шину.

### `POST /api/tires`

Создаёт шину.

### `PUT /api/tires/{id}`

Обновляет шину.

### `DELETE /api/tires/{id}`

Удаляет шину.

### `GET /api/tires/stats`

Возвращает статистику по шинам.

## Orders API

### `GET /api/orders`

Возвращает список заказов с фильтрами.

Часто используются параметры:

- `status`
- `carId`
- `masterId`

### `GET /api/orders/{id}`

Возвращает подробную информацию о заказе:

- заказ
- автомобиль и клиент
- мастер
- выполненные работы
- расходники
- коэффициенты сложности

### `POST /api/orders`

Создаёт заказ.

Пример:

```json
{
  "orderDate": "2026-06-12T10:00:00",
  "carId": 1,
  "scheduledAt": "2026-06-13T14:00:00",
  "discountPercent": 10,
  "discountType": "soft",
  "complexityCoefficientIds": [1, 2],
  "consumables": [
    {
      "consumableId": 1,
      "quantity": 2
    }
  ],
  "services": [
    {
      "serviceCode": 1,
      "wheelCount": 4
    }
  ]
}
```

### `POST /api/orders/preview`

Делает предварительный расчёт заказа.

### `POST /api/orders/{id}/assign-services`

Назначает услуги заказу.

### `POST /api/orders/{id}/complete`

Завершает заказ.

## Posts API

### `GET /api/posts`

Возвращает список постов.

### `POST /api/posts`

Создаёт пост.

### `PUT /api/posts/{id}`

Обновляет пост.

### `DELETE /api/posts/{id}`

Удаляет пост.

## Bookings API

Используется для сценариев бронирования и клиентских заказов.

Типичные сценарии:

- получить доступные слоты
- создать бронирование
- получить детали бронирования

Точный набор endpoint зависит от текущей реализации контроллера.

## Формат ответа

Во многих контроллерах используется ответ вида:

```json
{
  "success": true,
  "data": {}
}
```

При ошибке:

```json
{
  "success": false,
  "error": "Описание ошибки"
}
```

Дополнительно могут возвращаться ошибки валидации.

## Аутентификация

Часть endpoints требует авторизации.

Типичный формат:

```text
Authorization: Bearer <token>
```

Для customer-сценариев также важно учитывать cookies, JWT и production HTTPS.