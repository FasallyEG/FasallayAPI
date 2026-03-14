using Fasally;
using Fasally.Persistence.Seed;
using Fasally.Persistence;
using Fasally.Persistence.Seeders;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDependencies(builder.Configuration);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

var app = builder.Build();

// Create it Auto
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    
  
    await RoleSeeder.SeedRolesAsync(services);
    
     await UserSeeder.SeedAdminAsync(services);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
     app.MapScalarApiReference();
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseAuthentication();  // Authentication must come before Authorization
app.UseAuthorization();

app.UseStaticFiles();

app.MapControllers();

app.Run();
