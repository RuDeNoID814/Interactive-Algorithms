# Interactive Algorithms

Blazor WebAssembly приложение для эмпирического анализа временной сложности алгоритмов.
ЧелГУ, группа ПрИ-202, 2026 г.

## Запуск локально

```bash
git clone https://github.com/RuDeNoID814/Interactive-Algorithms.git
cd Interactive-Algorithms/AlgoLab/AlgoLab
dotnet run
```

Открыть: `http://localhost:5065`

## Структура проекта

```
Interactive-Algorithms/
├── .github/workflows/deploy.yml     # Автодеплой на GitHub Pages
└── AlgoLab/AlgoLab/
    ├── Pages/
    │   ├── Home.razor               # Главная — карточки алгоритмов
    │   ├── AlgorithmPage.razor      # Страница алгоритма /algorithm/{slug}
    │   └── Algorithms/              # Компоненты алгоритмов (добавлять сюда)
    ├── Layout/
    │   ├── MainLayout.razor
    │   └── NavMenu.razor
    ├── wwwroot/css/app.css          # Стили
    ├── _Imports.razor               # Глобальные using
    └── Program.cs                   # Точка входа
```

## Стек

- .NET 10, C#, Blazor WebAssembly
- Blazor-ApexCharts 7.0.0
- Bootstrap 5
- GitHub Pages (автодеплой из ветки `gh-pages`)

## Распределение алгоритмов

| Участник | Алгоритм | Slug |
|----------|----------|------|
| Максим | f(v) = 1 | `const` |
| Максим | Сумма элементов | `sum` |
| Максим | Произведение элементов | `product` |
| Максим | Наивный полином (x=1.5) | `poly-naive` |
| Максим | Метод Горнера (x=1.5) | `horner` |
| Никита | Bubble sort | `bubble` |
| Никита | Quick sort | `quick` |
| Никита | Timsort | `timsort` |
| Никита | Умножение матриц | `matrix` |
| Илья | Pow простой | `pow-simple` |
| Илья | Pow рекурсивный | `pow-recursive` |
| Илья | Pow быстрый | `pow-fast` |
| Илья | Pow классический | `pow-classic` |
| Каждый | Часть III — свой алгоритм ≥ O(n) | — |
