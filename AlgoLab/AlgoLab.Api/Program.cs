using AlgoLab.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


// =============================================
// DATABASE
// =============================================

builder.Services.AddDbContext<AlgoLabDbContext>(
    options =>
        options.UseSqlite(
            builder.Configuration.GetConnectionString(
                "DefaultConnection")));


// =============================================
// CONTROLLERS
// =============================================

builder.Services.AddControllers();


// =============================================
// CORS
// =============================================

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "BlazorClient",
        policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});


var app = builder.Build();


// =============================================
// DATABASE MIGRATION
// =============================================

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<AlgoLabDbContext>();
    db.Database.Migrate();
}


// =============================================
// HTTP PIPELINE
// =============================================

app.UseHttpsRedirection();

app.UseCors("BlazorClient");

app.MapControllers();


// =============================================
// ПРОВЕРКА API
// =============================================

app.MapGet(
    "/api/test",
    () => Results.Ok(new
    {
        message = "AlgoLab API работает"
    }));


app.Run();