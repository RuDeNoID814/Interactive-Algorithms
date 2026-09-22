using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using AlgoLab;
using AlgoLab.Services;
using ApexCharts;


var builder =
    WebAssemblyHostBuilder
        .CreateDefault(args);


builder.RootComponents
    .Add<App>("#app");


builder.RootComponents
    .Add<HeadOutlet>(
        "head::after");


// =============================================
// BACKEND API
// =============================================

// Сейчас backend запускается здесь.
// Позже адрес вынесем в конфигурацию.

builder.Services.AddScoped(
    _ => new HttpClient
    {
        BaseAddress =
            new Uri(
                "https://localhost:7182/")
    });


// =============================================
// SERVICES
// =============================================

builder.Services
    .AddScoped<ExperimentHistoryService>();

builder.Services
    .AddScoped<ExperimentSaveService>();


// =============================================
// CHARTS
// =============================================

builder.Services.AddApexCharts();


await builder
    .Build()
    .RunAsync();