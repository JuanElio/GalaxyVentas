using Scalar.AspNetCore;
using Ventas.Api.Comun;
using Ventas.Application;
using Ventas.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddWeb()
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

// OpenAPI solo documenta las acciones JSON que invoca jQuery
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler("/Home/Error");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference().AllowAnonymous();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets().AllowAnonymous();
app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

// Permite a WebApplicationFactory referenciar el punto de entrada en los tests de integración
public partial class Program;
