using System.Reflection;
using System.Text;
using Fasally.Abstractions.Consts;
using Fasally.Authentication;
using Fasally.Authentication.Filters;
using Fasally.Entities;
using Fasally.Persistence;
using Fasally.Services;
using Fasally.Settings;
using FluentValidation;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

namespace Fasally;

public static class DependencyInjection
{
    public static IServiceCollection AddDependencies(this IServiceCollection services, IConfiguration config)
    {
        services.AddControllers();

        services.AddAuthConfig(config);

        var connectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmailSender, EmailService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();

        services.AddOpenApi();

        services.Configure<MailSettings>(config.GetSection(nameof(MailSettings)));
        services.Configure<GoogleAuthSettings>(config.GetSection(nameof(GoogleAuthSettings)));

        return services;
    }
    private static IServiceCollection AddMapsterConfig(this IServiceCollection services)
    {
        var mappingConfig = TypeAdapterConfig.GlobalSettings;
        mappingConfig.Scan(Assembly.GetExecutingAssembly());

        services.AddSingleton<IMapper>(new Mapper(mappingConfig));

        return services;
    }
    private static IServiceCollection AddFluentValidationConfig(this IServiceCollection services)
        {
        services.AddFluentValidationAutoValidation()
        .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
        }

    private static IServiceCollection AddAuthConfig(this IServiceCollection services,
        IConfiguration config)
    {
        services.AddSingleton<IJWTProvider, JWTProvider>();

        services.AddIdentity<ApplicationUser, ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();


        services.AddOptions<JWTOptions>()
            .BindConfiguration(JWTOptions.SectionName)
            .ValidateDataAnnotations();

        var jwtSettings = config.GetSection(JWTOptions.SectionName).Get<JWTOptions>();

        services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(o =>
    {
        o.SaveToken = true;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.Key!)),
            ValidIssuer = jwtSettings?.Issuer,
            ValidAudience = jwtSettings?.Audience
        };
    });

        services.Configure<IdentityOptions>(options =>
           {
               options.Password.RequiredLength = 8;
               options.SignIn.RequireConfirmedEmail = true;
               options.User.RequireUniqueEmail = true;
           });

        services.AddAuthorization(o =>
     {
         o.AddPolicy("ApiAdminPolicy", b => b.RequireRole(DefaultRoles.Admin.Name));
     });
        return services;
    }
}