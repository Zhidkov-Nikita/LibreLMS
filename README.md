# LibreLMS
<p>
  <img alt="GitHub Actions Workflow Status" src="https://img.shields.io/github/actions/workflow/status/Zhidkov-Nikita/LibreLMS/dotnet.yml">
  <img alt="GitHub commit activity" src="https://img.shields.io/github/commit-activity/m/Zhidkov-Nikita/LibreLMS">
  <img alt="GitHub License" src="https://img.shields.io/github/license/Zhidkov-Nikita/LibreLMS">
  <img alt="GitHub repo size" src="https://img.shields.io/github/repo-size/Zhidkov-Nikita/LibreLMS">
</p>

LibreLMS — свободная система управления обучением (Learning Management System) на ASP.NET Core.

## Оглавление

- [Цель проекта](#цель-проекта)
- [Философия безопасности](#философия-безопасности)
- [Модель ролей](#модель-ролей)
- [Стек технологий](#стек-технологий)
- [Локальная установка и запуск](#локальная-установка-и-запуск)
  - [Инструкция для Windows 10 и 11](#инструкция-для-windows-10-и-11)
  - [Инструкция для Ubuntu 24.04](#инструкция-для-ubuntu-2404)
  - [Существующая база данных](#база-данных)
  - [EF Core миграции (production)](#ef-core-миграции-production)
- [Учётная запись администратора по умолчанию](#учётная-запись-администратора-по-умолчанию)
- [Структура проекта](#структура-проекта)
- [Панель управления пользователями](#панель-управления-пользователями)
- [API endpoints](#api-endpoints)
- [Переменные окружения](#переменные-окружения)

## Цель проекта

LibreLMS предназначена для вузов, преподавателей и студентов, которым требуется открытая, расширяемая и контролируемая среда электронного обучения.

## Философия безопасности

Каждая конечная точка LibreLMS **по умолчанию заблокирована**, если явно не разрешена. Конвейер middleware применяет глобальный `FallbackPolicy`, требующий аутентифицированного пользователя, а политика `AdminOnly` ограничивает все маршруты административной панели и API пользователями с ролью `Admin`.

## Модель ролей

| Роль      | Доступ                                      |
|-----------|---------------------------------------------|
| `Admin`   | Полный доступ к панели (`/Admin/**`) + API (`/api/v1/**`) |
| `Teacher` | Только API (будущая функциональность)       |
| `Student` | Только API (будущая функциональность)       |

Пользователи без явной роли получают **403 Forbidden** на защищённых ресурсах.

## Стек технологий

- .NET 10 SDK
- PostgreSQL 16
- Git

## Локальная установка и запуск

### Инструкция для Windows 10 и 11

Откройте PowerShell или Командную строку и выполните:

```powershell
git clone https://github.com/Zhidkov-Nikita/LibreLMS.git
cd LibreLMS

copy .env.example .env
# Отредактируйте .env, указав актуальные учётные данные PostgreSQL

dotnet restore
dotnet run --project LibreLMS.Api
```

После запуска приложение будет доступно по адресу `http://localhost:5000`. Панель администратора — `http://localhost:5000/admin`.

### Инструкция для Ubuntu 24.04

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

### База данных

Если у вас есть база данных от старой схемы, сначала примените SQL-миграцию:

```bash
psql -d YOUR_DATABASE -f LibreLMS.Api/Core/Data/Migrations/0001_StudentsToUsers.sql
dotnet run --project LibreLMS.Api
```

### EF Core миграции (production)

Для постоянных изменений схемы переключитесь с `EnsureCreated()` на `Database.Migrate()`:

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add MigrationName --project LibreLMS.Api
dotnet ef database update --project LibreLMS.Api
```

## Учётная запись администратора по умолчанию

| Поле     | Значение                            |
|----------|-------------------------------------|
| Email    | `admin@librelms.com`               |
| Password | `LibreLMS%`                        |

> **⚠ Замечание по безопасности** — Этот пароль задан в `Program.cs` для начальной загрузки. **Смените его сразу** после первого входа. В production отключите блок сидирования или вынесите создание учётной записи в защищённое внешнее хранилище.

## Структура проекта

```
LibreLMS/
├── .env.example                           # Шаблон переменных окружения
├── README.md
├── wwwroot/                               # Корень SPA-фронтенда
└── LibreLMS.Api/                          # Основной проект ASP.NET Core
    ├── Program.cs                         # Точка входа, DI, middleware pipeline
    ├── Properties/
    │   └── launchSettings.json
    │
    ├── Core/                              # Ядро — общая инфраструктура
    │   ├── Data/
    │   │   ├── AppDbContext.cs            # EF Core контекст (Users + StudentProfiles)
    │   │   └── Migrations/                # SQL-скрипты миграций
    │   └── Security/
    │       └── PasswordHasher.cs          # BCrypt-обёртка (work factor 12)
    │
    ├── Features/                          # Доменные модули (Django Apps style)
    │   ├── Auth/                          # ── Модуль аутентификации ──
    │   │   └── Models/
    │   │       ├── User.cs                # Центральная сущность пользователя
    │   │       └── Role.cs                # Enum: Admin / Teacher / Student
    │   └── Students/                      # ── Модуль студентов ──
    │       └── Models/
    │           └── StudentProfile.cs      # Профиль студента (FK → User)
    │
    ├── Pages/                             # Razor Pages (по конвенции)
    │   ├── _ViewImports.cshtml
    │   ├── _ViewStart.cshtml
    │   ├── Shared/
    │   │   └── _Layout.cshtml             # Мастер-страница с боковой панелью
    │   │
    │   ├── Auth/                          # ── Страницы модуля Auth ──
    │   │   ├── Login.cshtml / .cs         # Вход в систему
    │   │   └── Logout.cshtml / .cs        # Выход из системы
    │   │
    │   └── Admin/                         # ── Страницы модуля Admin ──
    │       ├── Models/
    │       │   └── UserEditModel.cs       # ViewModel для формы пользователя
    │       ├── Index.cshtml / .cs         # Дашборд (статистика по ролям)
    │       └── Users/                     # CRUD пользователей
    │           ├── Index.cshtml / .cs     # Список + поиск
    │           ├── Edit.cshtml / .cs      # Создание / редактирование
    │           └── Delete.cshtml / .cs    # Подтверждение удаления
    │
    ├── Migrations/                        # EF Core миграции (авто-генерируемые)
    │   ├── 20260616140759_InitialCleanSchema.cs
    │   └── AppDbContextModelSnapshot.cs
    │
    └── wwwroot/
        └── css/
            └── admin.css                  # Стили панели администратора
```

## Панель управления пользователями

Администраторы управляют пользователями через панель `/Admin/Users`. Доступные операции:

- **Список пользователей** — таблица всех учётных записей с указанием роли (Admin / Teacher / Student) и поиском по email
- **Создание пользователя** — форма с указанием email, роли и пароля; для роли `Student` дополнительно заполняются имя, фамилия и дата зачисления
- **Редактирование** — смена роли, email или пароля; при переключении роли с `Student` на другую профиль студента удаляется, при переключении на `Student` — создаётся
- **Удаление** — подтверждение с отображением всех полей перед удалением

Смена пароля осуществляется через BCrypt (work factor 12). При редактировании поле пароля можно оставить пустым — текущий пароль сохраняется.

## API endpoints

| Method | Route               | Auth       | Описание                |
|--------|---------------------|------------|-------------------------|
| GET    | `/api/v1/students`  | AdminOnly  | Список всех студентов   |

Группа API защищена политикой `AdminOnly`. Запросы без роли `Admin` получают **401 Unauthorized** или **403 Forbidden**.

## Переменные окружения

| Variable                    | Required | Описание                         |
|-----------------------------|----------|----------------------------------|
| `CONNECTIONSTRINGS__POSTGRES` | Да     | Строка подключения к PostgreSQL |
| `ASPNETCORE_ENVIRONMENT`    | Нет      | `Development` / `Production`     |
| `ASPNETCORE_URLS`           | Нет      | Адрес привязки (по умолч. :5000) |
