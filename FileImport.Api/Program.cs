using FileImport.Api.Filters;
using FileImport.Application;
using FileImport.Infrastructure;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using Serilog;
//dotnet publish -c Release -o ./publish /p:EnvironmentName=Test
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiExceptionFilterAttribute>();
});
builder.Services.AddFluentValidationAutoValidation(configuration =>
{
    configuration.OverrideDefaultResultFactoryWith<ValidationBehaviour>();
});
builder.Services.AddInfrastructure(builder.Configuration, builder.Host);
builder.Services.AddApplication();
builder.Services.AddHttpContextAccessor();
builder.Services.AddCors(options =>
{
    //Dodajemo adresu localhost-a frontend aplikacije.
    options.AddPolicy("Development", policy =>
    {
        policy.WithOrigins(["https://localhost:7244", "https://localhost"])
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
    //Dodajemo adrese sa kojih pristupamo iz test okruženja.
    options.AddPolicy("Test", policy =>
    {
        policy.WithOrigins("https://not_on_test.com")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
    //Dodajemo adrese sa kojih pristupamo iz produkcionog okruženja.
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins("https://not_on_production.com")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();
// Primenjujemo odgovarajući CORS.
if (app.Environment.IsDevelopment())
{
    app.UseCors("Development");
}
else if (app.Environment.EnvironmentName == "Test")
{
    app.UseCors("Test");
}
else
{
    app.UseCors("Production");
}
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
try { 
    app.Run();
} catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
} finally
{
    Log.CloseAndFlush();
}