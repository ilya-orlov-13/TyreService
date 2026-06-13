@echo off
setlocal
chcp 65001 >nul

set "SCRIPT_DIR=%~dp0"
set "PROJECT_DIR=%SCRIPT_DIR%..\TyreServiceApp\TyreServiceApp"
set "SEED_FILE=%SCRIPT_DIR%seed.sql"

if not defined PGHOST set "PGHOST=localhost"
if not defined PGPORT set "PGPORT=5432"
if not defined PGUSER set "PGUSER=postgres"
if not defined PGDATABASE set "PGDATABASE=tyreservice"

echo ============================================
echo  TyreService — настройка и заполнение БД
echo ============================================
echo.
echo Параметры подключения:
echo   Host:     %PGHOST%
echo   Port:     %PGPORT%
echo   User:     %PGUSER%
echo   Database: %PGDATABASE%
echo.

where dotnet >nul 2>nul
if errorlevel 1 (
    echo [ОШИБКА] dotnet SDK не найден в PATH.
    exit /b 1
)

where psql >nul 2>nul
if errorlevel 1 (
    echo [ОШИБКА] psql не найден в PATH.
    echo Установите PostgreSQL client tools или добавьте psql в PATH.
    exit /b 1
)

if not exist "%SEED_FILE%" (
    echo [ОШИБКА] Не найден файл seed: "%SEED_FILE%"
    exit /b 1
)

if not defined PGPASSWORD (
    set /p PGPASSWORD=Введите пароль PostgreSQL для пользователя %PGUSER%: 
)

set "DbPassword=%PGPASSWORD%"

echo.
echo [1/2] Применяю миграции EF Core...
pushd "%PROJECT_DIR%" >nul || exit /b 1
dotnet ef database update
if errorlevel 1 (
    popd >nul
    echo [ОШИБКА] Не удалось применить миграции.
    exit /b 1
)
popd >nul

echo.
echo [2/2] Заполняю базу тестовыми данными...
psql -v ON_ERROR_STOP=1 -h "%PGHOST%" -p "%PGPORT%" -U "%PGUSER%" -d "%PGDATABASE%" -f "%SEED_FILE%"
if errorlevel 1 (
    echo [ОШИБКА] Не удалось выполнить seed.sql.
    exit /b 1
)

echo.
echo ============================================
echo  Готово: миграции применены, БД заполнена
echo ============================================
exit /b 0
