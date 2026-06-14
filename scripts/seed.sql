\set ON_ERROR_STOP on

BEGIN;

-- Скрипт заполнения базы данных TyreService для текущей схемы.
-- Использует DELETE (вместо TRUNCATE), чтобы работать через Supabase pooler.
-- Безопасно запускается повторно.

-- ============================================================
-- 0. Справочники (миграции создают их, но гарантируем наличие)
-- ============================================================
INSERT INTO "Positions" ("PositionId", "Name") VALUES
  (1, 'Шиномонтажник'),
  (2, 'Балансировщик'),
  (3, 'Мастер-приёмщик'),
  (4, 'Старший механик'),
  (5, 'Помощник мастера')
ON CONFLICT ("PositionId") DO NOTHING;

INSERT INTO "StaffPositions" ("StaffPositionId", "Name") VALUES
  (1, 'Администратор')
ON CONFLICT ("StaffPositionId") DO NOTHING;

INSERT INTO "CarClasses" ("CarClassId", "Name", "BaseTariff", "SortOrder") VALUES
  (1, 'Эконом', 0, 1),
  (2, 'Стандарт', 200, 2),
  (3, 'Комфорт', 400, 3),
  (4, 'Бизнес', 600, 4),
  (5, 'Премиум', 1000, 5)
ON CONFLICT ("CarClassId") DO NOTHING;

-- ============================================================
-- Очистка данных (удаляем в порядке FK-зависимостей)
-- ============================================================
DELETE FROM "ServiceTariffs";
DELETE FROM "WorkTimeLogs";
DELETE FROM "CompletedWorks";
DELETE FROM "SpeedBonuses";
DELETE FROM "OrderComplexities";
DELETE FROM "OrderConsumables";
DELETE FROM "CompletedJobsPayouts";
DELETE FROM "PostActiveSessions";
DELETE FROM "MasterUsers";
DELETE FROM "CustomerReviews";
DELETE FROM "Tires";
DELETE FROM "Orders";
DELETE FROM "Cars";
DELETE FROM "Masters";
DELETE FROM "Clients";
DELETE FROM "Services";
DELETE FROM "CustomerUsers";

-- Сброс identity-последовательностей
ALTER TABLE "Services" ALTER COLUMN "ServiceCode" RESTART WITH 1;
ALTER TABLE "Masters" ALTER COLUMN "MasterId" RESTART WITH 1;
ALTER TABLE "Clients" ALTER COLUMN "ClientId" RESTART WITH 1;
ALTER TABLE "Cars" ALTER COLUMN "CarId" RESTART WITH 1;
ALTER TABLE "Tires" ALTER COLUMN "TireId" RESTART WITH 1;
ALTER TABLE "Orders" ALTER COLUMN "OrderNumber" RESTART WITH 1;
ALTER TABLE "CompletedWorks" ALTER COLUMN "WorkId" RESTART WITH 1;

-- ============================================================
-- 1. Услуги
-- ============================================================
INSERT INTO "Services" ("ServiceCode", "ServiceName", "ServiceCost") VALUES
  (1, 'Сезонная смена колёс (4 колеса)', 2500.00),
  (2, 'Комплексный шиномонтаж (4 колеса)', 3200.00),
  (3, 'Балансировка колеса', 500.00),
  (4, 'Ремонт прокола жгутом', 600.00),
  (5, 'Ремонт прокола грибком', 900.00),
  (6, 'Правка литого диска', 2200.00),
  (7, 'Проверка давления в шинах', 150.00),
  (8, 'Сезонное хранение комплекта', 3500.00);

-- ============================================================
-- 2. Мастера
-- ============================================================
INSERT INTO "Masters" ("FullName", "PositionId", "Rank", "HourlyRate") VALUES
  ('Иванов Григорий Сергеевич', 4, 6, 1200.00),  -- Старший механик
  ('Ковалёв Дмитрий Андреевич', 1, 5, 950.00),  -- Шиномонтажник
  ('Фёдоров Павел Николаевич', 1, 4, 850.00);  -- Шиномонтажник

-- ============================================================
-- 3. Клиенты
-- ============================================================
INSERT INTO "Clients" ("FullName", "Phone", "Email") VALUES
  ('Иванов Сергей Петрович', '+79001234567', NULL),
  ('Петрова Анна Викторовна', '+79002345678', NULL),
  ('Смирнов Алексей Олегович', '+79003456789', NULL),
  ('Кузнецова Марина Игоревна', '+79004567890', NULL);

-- ============================================================
-- 4. Автомобили
-- ============================================================
INSERT INTO "Cars" (
  "ClientId",
  "Brand",
  "Model",
  "ManufactureYear",
  "LicensePlate",
  "Vin",
  "PhotoPath",
  "AdditionalPhotos"
) VALUES
  (1, 'Toyota', 'Camry', 2020, 'А123ВС196', 'XW7BF4FK90S123456', NULL, NULL),
  (1, 'Lada', 'Vesta', 2022, 'М456ОР196', 'XTAGFL110NY654321', NULL, NULL),
  (2, 'Hyundai', 'Solaris', 2019, 'Е789КХ196', 'Z94K241CBKR765432', NULL, NULL),
  (3, 'Kia', 'Sportage', 2021, 'Т234НА196', 'KNAPM81ABM7123456', NULL, NULL),
  (4, 'Volkswagen', 'Polo', 2018, 'С567УМ196', 'XW8ZZZ61ZJG098765', NULL, NULL);

-- ============================================================
-- 5. Шины
-- ============================================================
INSERT INTO "Tires" (
  "CarId",
  "TireType",
  "Seasonality",
  "Manufacturer",
  "TireModel",
  "Size",
  "LoadIndex",
  "WearPercentage",
  "Pressure"
) VALUES
  (1, 'Легковая', 'Зимняя', 'Michelin', 'X-Ice Snow', '215/55R17', 98, 20, 2.2),
  (1, 'Легковая', 'Летняя', 'Continental', 'PremiumContact 6', '215/55R17', 98, 35, 2.3),
  (2, 'Легковая', 'Летняя', 'Pirelli', 'Cinturato P7', '205/55R16', 91, 15, 2.2),
  (3, 'Легковая', 'Зимняя', 'Nokian', 'Hakkapeliitta 9', '195/65R15', 91, 40, 2.1),
  (4, 'Кроссовер', 'Летняя', 'Bridgestone', 'Alenza 001', '225/60R17', 99, 25, 2.4),
  (5, 'Легковая', 'Летняя', 'Yokohama', 'BluEarth-GT', '195/55R16', 87, 30, 2.2);

-- ============================================================
-- 6. Заказы
-- ============================================================
INSERT INTO "Orders" ("OrderDate", "CarId", "MasterId", "PaymentDate") VALUES
  ('2026-06-01 10:00:00', 1, 1, '2026-06-01 12:00:00'),
  ('2026-06-03 14:30:00', 3, 2, '2026-06-03 15:30:00'),
  ('2026-06-05 09:15:00', 4, 2, NULL),
  ('2026-06-07 16:45:00', 2, 3, '2026-06-07 18:00:00'),
  ('2026-06-10 11:20:00', 5, NULL, NULL);

-- ============================================================
-- 7. Выполненные работы
-- ============================================================
INSERT INTO "CompletedWorks" (
  "OrderNumber",
  "ServiceCode",
  "MasterId",
  "WheelCount",
  "CompletionTimeMin",
  "WorkTotal"
) VALUES
  (1, 1, 1, 4, 60, 2500.00),
  (1, 3, 1, 4, 25, 2000.00),
  (2, 4, 2, 1, 20, 600.00),
  (2, 7, 2, 4, 10, 150.00),
  (3, 2, 2, 4, 90, 3200.00),
  (4, 5, 3, 1, 30, 900.00),
  (4, 3, 3, 4, 25, 2000.00);

COMMIT;
