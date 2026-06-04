using Fasally.Entities;
using Fasally.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Fasally.IntegrationTests.Infrastructure;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTesting");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
            services.RemoveAll<IEmailSender>();

            for (var i = services.Count - 1; i >= 0; i--)
            {
                var serviceType = services[i].ServiceType;

                if (IsDbContextOptionsConfiguration(serviceType) ||
                    IsSqlServerDescriptor(services[i]))
                {
                    services.RemoveAt(i);
                }
            }

            services.AddSingleton(_connection);
            services.AddScoped<IEmailSender, FakeEmailSender>();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(_connection));

            if (_connection.State != System.Data.ConnectionState.Open)
                _connection.Open();

            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            AssertUsesSqlite(context);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        });
    }

    private static bool IsDbContextOptionsConfiguration(Type serviceType) =>
        serviceType.IsGenericType &&
        serviceType.GetGenericTypeDefinition() == typeof(IDbContextOptionsConfiguration<>);

    private static bool IsSqlServerDescriptor(ServiceDescriptor descriptor) =>
        IsSqlServerType(descriptor.ImplementationType) ||
        IsSqlServerType(descriptor.ImplementationInstance?.GetType()) ||
        IsSqlServerType(descriptor.ImplementationFactory?.GetType());

    private static bool IsSqlServerType(Type? type) =>
        type?.Assembly.GetName().Name?.Contains("SqlServer", StringComparison.OrdinalIgnoreCase) is true;

    private static void AssertUsesSqlite(ApplicationDbContext context)
    {
        if (context.Database.ProviderName != "Microsoft.EntityFrameworkCore.Sqlite")
            throw new InvalidOperationException($"Integration tests must use SQLite, but resolved '{context.Database.ProviderName}'.");
    }

    public HttpClient CreateApiClient() =>
        CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

    public async Task<T> ExecuteDbContextAsync<T>(Func<ApplicationDbContext, Task<T>> action)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await action(context);
    }

    public async Task ExecuteDbContextAsync(Func<ApplicationDbContext, Task> action)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await action(context);
    }

    public async Task InitializeAsync()
    {
        await Task.CompletedTask;
    }

    public new async Task DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
