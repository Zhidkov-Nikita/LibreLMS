# LibreLMS

LibreLMS — свободная система управления обучением (Learning Management System) с REST API и панелью администратора на ASP.NET Core.

## Purpose of the Project

LibreLMS предназначена для вузов, преподавателей и студентов, которым требуется открытая, расширяемая и контролируемая среда электронного обучения. Система предоставляет базовые функции LMS: управление профилями студентов, RESTful API для интеграции с внешними сервисами и веб-интерфейс администратора.

## Security Philosophy: Default Deny

Каждая конечная точка LibreLMS **неявно заблокирована**, если явно не разрешена. Конвейер middleware применяет глобальный `FallbackPolicy`, требующий аутентифицированного пользователя, а политика `AdminOnly` ограничивает все маршруты административной панели и API пользователями с ролью `Admin`.

## Role Model

| Роль      | Доступ                                      |
|-----------|---------------------------------------------|
| `Admin`   | Полный доступ к панели (`/Admin/**`) + API (`/api/v1/**`) |
| `Teacher` | Только API (будущая функциональность)       |
| `Student` | Только API (будущая функциональность)       |

Пользователи без явной роли получают **403 Forbidden** на защищённых ресурсах.

## Prerequisites

- .NET 10 SDK
- PostgreSQL 16 или новее (запущенный локально или доступный удалённо)
- Git

## Local Installation and Setup

### Windows 10 and 11 Instructions

Откройте PowerShell или Командную строку и выполните:

```powershell
git clone https://github.com/Zhidkov-Nikita/LibreLMS.git
cd LibreLMS

copy .env.example .env
# Отредактируйте .env, указав актуальные учётные данные PostgreSQL

dotnet restore
dotnet run --project LibreLMS.Api
```

После запуска приложение будет доступно по адресу `http://localhost:5000`. Панель администратора — `http://localhost:5000/Admin`.

### Ubuntu 24.04 Instructions

Установите .NET 10 SDK, если он ещё не установлен:

```bash
sudo apt-get update && sudo apt-get install -y dotnet-sdk-10.0
```

Если пакет недоступен в стандартных репозиториях, добавьте репозиторий Microsoft:

```bash
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 10.0
export PATH="$HOME/.dotnet:$PATH"
```

Клонируйте и запустите проект:

```bash
git clone https://github.com/Zhidkov-Nikita/LibreLMS.git
cd LibreLMS

cp .env.example .env
# Отредактируйте .env, указав актуальные учётные данные PostgreSQL

dotnet restore
dotnet run --project LibreLMS.Api
```

Приложение запустится на `http://localhost:5000`. Панель администратора — `http://localhost:5000/Admin`.

Примечание. База данных и таблицы создаются автоматически при первом запуске (Entity Framework Core `EnsureCreated`). Для чистой базы данных миграции не требуются.

### Существующая база данных (с таблицей `Students`)

Если у вас есть база данных от старой схемы, сначала примените SQL-миграцию:

```bash
psql -d your_database -f LibreLMS.Api/Migrations/0001_StudentsToUsers.sql
dotnet run --project LibreLMS.Api
```

### EF Core Migrations (production)

Для постоянных изменений схемы переключитесь с `EnsureCreated()` на `Database.Migrate()`:

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add MigrationName --project LibreLMS.Api
dotnet ef database update --project LibreLMS.Api
```

## Default Admin Account

| Поле     | Значение                            |
|----------|-------------------------------------|
| Email    | `admin@librelms.com`               |
| Password | `LibreLMS%`                        |

> **⚠ Замечание по безопасности** — Этот пароль задан в `Program.cs` для начальной загрузки. **Смените его сразу** после первого входа. В production отключите блок сидирования или вынесите создание учётной записи в защищённое внешнее хранилище.

## Project Structure

- `LibreLMS.Api/` — основной проект ASP.NET Core
- `LibreLMS.Api/Program.cs` — точка входа, auth-конвейер, настройка сервисов и маршрутизация
- `LibreLMS.Api/AppDbContext.cs` — контекст базы данных EF Core (Users + StudentProfiles)
- `LibreLMS.Api/User.cs` — центральная сущность пользователя
- `LibreLMS.Api/Role.cs` — enum ролей (Admin / Teacher / Student)
- `LibreLMS.Api/StudentProfile.cs` — профиль студента (FK → Users)
- `LibreLMS.Api/PasswordHasher.cs` — обёртка BCrypt
- `LibreLMS.Api/Models/` — модели представления (view models) для форм редактирования
- `LibreLMS.Api/Pages/` — Razor Pages (Login, Logout, Admin/*)
- `LibreLMS.Api/Pages/Admin/` — панель администратора (дашборд, CRUD студентов)
- `LibreLMS.Api/Migrations/` — скрипты SQL-миграций
- `LibreLMS.Api/wwwroot/` — статические файлы (CSS, HTML-заглушка SPA)
- `wwwroot/` — корень SPA-фронтенда
- `.env.example` — шаблон файла с переменными окружения

## User Management Dashboard

Администраторы управляют пользователями через панель `/Admin/Users`. Доступные операции:

- **Список пользователей** — таблица всех учётных записей с указанием роли (Admin / Teacher / Student) и поиском по email
- **Создание пользователя** — форма с указанием email, роли и пароля; для роли `Student` дополнительно заполняются имя, фамилия и дата зачисления
- **Редактирование** — смена роли, email или пароля; при переключении роли с `Student` на другую профиль студента удаляется, при переключении на `Student` — создаётся
- **Удаление** — подтверждение с отображением всех полей перед удалением

Смена пароля осуществляется через BCrypt (work factor 12). При редактировании поле пароля можно оставить пустым — текущий пароль сохраняется.

## API Endpoints

| Method | Route               | Auth       | Описание                |
|--------|---------------------|------------|-------------------------|
| GET    | `/api/v1/students`  | AdminOnly  | Список всех студентов   |

Группа API защищена политикой `AdminOnly`. Запросы без роли `Admin` получают **401 Unauthorized** или **403 Forbidden**.

## Environment Variables

| Variable                    | Required | Описание                         |
|-----------------------------|----------|----------------------------------|
| `CONNECTIONSTRINGS__POSTGRES` | Да     | Строка подключения к PostgreSQL |
| `ASPNETCORE_ENVIRONMENT`    | Нет      | `Development` / `Production`     |
| `ASPNETCORE_URLS`           | Нет      | Адрес привязки (по умолч. :5000) |
