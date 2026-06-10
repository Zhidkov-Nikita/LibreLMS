# LibreLMS

LibreLMS (также LibreLMS) -- свободная система управления обучением (Learning Management System) с REST API и панелью администратора на ASP.NET Core.

## Purpose of the Project

LibreLMS предназначена для вузов, преподавателей и студентов, которым требуется открытая, расширяемая и контролируемая среда электронного обучения. Система предоставляет базовые функции LMS: управление профилями студентов, RESTful API для интеграции с внешними сервисами и веб-интерфейс администратора.

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

После запуска приложение будет доступно по адресу `http://localhost:5000`. Панель администратора -- `http://localhost:5000/Admin`.

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

Приложение запустится на `http://localhost:5000`. Панель администратора -- `http://localhost:5000/Admin`.

Примечание. База данных и таблицы создаются автоматически при первом запуске (Entity Framework Core `EnsureCreated`). Применять миграции вручную не требуется. В `.env.example` указана строка подключения для локального PostgreSQL с учётной записью `postgres`.

## Project Structure

- `LibreLMS.Api/` -- основной проект ASP.NET Core
- `LibreLMS.Api/Program.cs` -- точка входа, настройка сервисов, middleware и маршрутизация
- `LibreLMS.Api/AppDbContext.cs` -- контекст базы данных Entity Framework Core
- `LibreLMS.Api/StudentProfile.cs` -- доменная сущность профиля студента
- `LibreLMS.Api/Models/` -- модели представления (view models) для форм редактирования
- `LibreLMS.Api/Pages/Admin/` -- Razor Pages панели администратора (дашборд, CRUD студентов)
- `LibreLMS.Api/Pages/Shared/_Layout.cshtml` -- мастер-страница с боковой панелью
- `LibreLMS.Api/wwwroot/` -- статические файлы (CSS, HTML-заглушка SPA)
- `.env.example` -- шаблон файла с переменными окружения (строка подключения к PostgreSQL)
