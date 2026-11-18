# Interactive Algorithms

Интерактивные лабораторные работы по алгоритмам и структурам данных. ЧелГУ, группа ПрИ-202, 2026 год.

## Локальный запуск

Нужен .NET SDK 10. Команды выполняются из корня репозитория.

```bash
dotnet run --project AlgoLab/AlgoLab/AlgoLab.csproj --launch-profile http
```

Открой [сайт на localhost:5065](http://localhost:5065). Для истории экспериментов, сохранения замеров и сравнения сохранённых результатов запусти API во втором терминале:

```bash
dotnet run --project AlgoLab/AlgoLab.Api/AlgoLab.Api.csproj --launch-profile http
```

Клиент обращается к API по адресу `http://localhost:5225`. Сервер хранит результаты в SQLite.

## Структура проекта

```text
AlgoLab/
├── AlgoLab.sln
├── AlgoLab/                        # клиент Blazor WebAssembly
│   ├── Labs/
│   │   ├── Lab1/
│   │   │   ├── Lab1Catalog.razor    # каталог первой лабораторной
│   │   │   ├── Basics/             # константа, сумма и произведение
│   │   │   ├── Graphs/             # BFS и задача коммивояжёра
│   │   │   ├── Matrix/             # экран, умножение и замеры
│   │   │   ├── Polynomials/        # прямой расчёт и схема Горнера
│   │   │   ├── Powers/             # возведение в степень
│   │   │   ├── Sorting/            # Bubble Sort, Quick Sort, Timsort
│   │   │   └── Huffman.razor       # кодирование Хаффмана
│   │   ├── Lab2/
│   │   │   ├── Lab2.razor          # каталог на /lab2
│   │   │   ├── DragonCurve/        # фрактал и его стили
│   │   │   └── HanoiTowers/        # игра, замеры и стили башен
│   │   └── Lab3/
│   │       ├── Lab3.razor          # каталог на /lab3
│   │       └── Stack/
│   │           ├── MyStack.cs      # стек на своём связном списке
│   │           ├── StackCommands.cs # команды, наборы входов и замеры
│   │           ├── StackPage.razor # ввод, результаты и график замеров
│   │           ├── Calculator/
│   │           │   ├── PostfixCalc.razor
│   │           │   └── ExpressionService.cs
│   │           └── Brackets/
│   │               ├── BracketChecker.razor
│   │               └── BracketValidator.cs
│   ├── Components/Charts/         # общий компонент графика
│   ├── Layout/                    # каркас сайта и меню
│   ├── Models/                    # точки графиков и данные экспериментов
│   ├── Pages/                     # главная, маршруты, история и сравнение
│   ├── Services/                  # каталог, аппроксимация и работа с API
│   └── wwwroot/
│       ├── css/app.css              # общие стили сайта
│       └── js/charts/plotlyInterop.js # отрисовка через Plotly
└── AlgoLab.Api/                    # ASP.NET Core API для истории замеров
    ├── Controllers/               # чтение и сохранение экспериментов
    ├── Data/                      # контекст SQLite
    ├── Migrations/                # изменения схемы базы
    └── Models/                    # эксперименты и точки замеров
```

## Сборка и публикация

```bash
dotnet build AlgoLab/AlgoLab.sln
```

После пуша в `main` [GitHub Actions](.github/workflows/deploy.yml) собирает клиент и записывает статические файлы в ветку `gh-pages`. API запускается отдельно.
