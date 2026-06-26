using System.Security.Claims;
using Checklist.Application.Common;
using Checklist.Application.Interfaces;
using Checklist.Infrastructure.Data;
using Checklist.Infrastructure.Identity;
using Checklist.Infrastructure.Options;
using Checklist.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Checklist.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    private const string LocalInMemoryDatabaseName = "Checklist.Mvc.Local";

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var authenticationOptions =
            configuration.GetSection(MvcAuthenticationOptions.SectionName).Get<MvcAuthenticationOptions>()
            ?? new MvcAuthenticationOptions();

        var databaseOptions =
            configuration.GetSection(MvcDatabaseOptions.SectionName).Get<MvcDatabaseOptions>()
            ?? new MvcDatabaseOptions();

        var connectionString = ResolveConnectionString(configuration, databaseOptions);
        var hasDatabaseConnection = !string.IsNullOrWhiteSpace(connectionString);
        var useLocalInMemoryDatabase = !hasDatabaseConnection;

        services.Configure<MvcAuthenticationOptions>(configuration.GetSection(MvcAuthenticationOptions.SectionName));
        services.Configure<ActiveDirectoryOptions>(configuration.GetSection(ActiveDirectoryOptions.SectionName));
        services.Configure<MvcDatabaseOptions>(configuration.GetSection(MvcDatabaseOptions.SectionName));
        services.AddHttpContextAccessor();

        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddSingleton<PasswordHashingService>();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<ICurrentOperator, CurrentOperator>();
        services.AddScoped<SupervisorLoginGenerator>();
        services.AddScoped<ChecklistStandardCatalogService>();
        services.AddScoped<StpAreaTemplateCatalogService>();
        services.AddScoped<ChecklistMonthlyWorkbookService>();
        services.AddScoped<ChecklistMonthlyClosingService>();
        services.AddScoped<LocalMvcDatabaseBootstrapper>();

        if (hasDatabaseConnection)
        {
            var resolvedConnectionString = connectionString!;

            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(resolvedConnectionString));
            RegisterDatabaseBackedServices(services);
        }
        else
        {
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(LocalInMemoryDatabaseName));
            RegisterDatabaseBackedServices(services);
            services.AddScoped<IStpInspectionService, UnavailableStpInspectionService>();
            services.AddScoped<IStpDocumentControlService, UnavailableStpDocumentControlService>();
        }

        services.AddAuthentication(options =>
            {
                options.DefaultScheme = MvcAuthenticationSchemes.App;
                options.DefaultAuthenticateScheme = MvcAuthenticationSchemes.App;
                options.DefaultChallengeScheme = MvcAuthenticationSchemes.App;
                options.DefaultForbidScheme = MvcAuthenticationSchemes.App;
            })
            .AddPolicyScheme(MvcAuthenticationSchemes.App, "App cookie scheme", options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    return context.Request.Path.StartsWithSegments("/operador", StringComparison.OrdinalIgnoreCase)
                        ? MvcAuthenticationSchemes.Operator
                        : MvcAuthenticationSchemes.Supervisor;
                };
            })
            .AddCookie(MvcAuthenticationSchemes.Supervisor, options =>
            {
                options.LoginPath = "/account/login";
                options.AccessDeniedPath = "/account/access-denied";
                options.SlidingExpiration = true;
            })
            .AddCookie(MvcAuthenticationSchemes.Operator, options =>
            {
                options.LoginPath = "/operador/login";
                options.AccessDeniedPath = "/operador/login";
                options.SlidingExpiration = true;
            });

        if (string.Equals(authenticationOptions.Mode, MvcAuthenticationOptions.DevelopmentStubMode, StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IActiveDirectoryCredentialValidator, DevelopmentStubCredentialValidator>();
        }
        else if (OperatingSystem.IsWindows())
        {
            services.AddSingleton<IActiveDirectoryCredentialValidator, ActiveDirectoryCredentialValidator>();
        }
        else
        {
            services.AddSingleton<IActiveDirectoryCredentialValidator, UnsupportedPlatformCredentialValidator>();
        }

        if (hasDatabaseConnection || useLocalInMemoryDatabase)
        {
            services.AddScoped<ISupervisorAuthenticationService, SupervisorAuthenticationService>();
        }
        else
        {
            services.AddScoped<ISupervisorAuthenticationService, UnavailableSupervisorAuthenticationService>();
        }

        if (string.Equals(authenticationOptions.Mode, MvcAuthenticationOptions.DevelopmentStubMode, StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IOperatorAuthenticationService, OperatorAuthenticationService>();
        }
        else if (hasDatabaseConnection || useLocalInMemoryDatabase)
        {
            services.AddScoped<IOperatorAuthenticationService, OperatorAuthenticationService>();
        }
        else
        {
            services.AddScoped<IOperatorAuthenticationService, InMemoryOperatorAuthenticationService>();
        }

        services.AddAuthorization(options =>
        {
            options.AddPolicy("SupervisorReady", policy =>
            {
                policy.AuthenticationSchemes.Add(MvcAuthenticationSchemes.Supervisor);
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(context =>
                    !ReadBooleanClaim(context.User, CurrentUserClaimTypes.ForceChangePassword));
            });

            options.AddPolicy("SectorSupervisorReady", policy =>
            {
                policy.AuthenticationSchemes.Add(MvcAuthenticationSchemes.Supervisor);
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(context =>
                    !ReadBooleanClaim(context.User, CurrentUserClaimTypes.ForceChangePassword) &&
                    !ReadBooleanClaim(context.User, CurrentUserClaimTypes.IsMaster) &&
                    string.Equals(context.User.FindFirst(CurrentUserClaimTypes.UserType)?.Value, "Supervisor", StringComparison.Ordinal) &&
                    context.User.FindAll(CurrentUserClaimTypes.AccessModule).Any(claim => string.Equals(claim.Value, AccessModuleCodes.OperationalSupervision, StringComparison.OrdinalIgnoreCase)));
            });

            options.AddPolicy("SafetyWorkReady", policy =>
            {
                policy.AuthenticationSchemes.Add(MvcAuthenticationSchemes.Supervisor);
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(context =>
                    !ReadBooleanClaim(context.User, CurrentUserClaimTypes.ForceChangePassword) &&
                    !ReadBooleanClaim(context.User, CurrentUserClaimTypes.IsMaster) &&
                    string.Equals(context.User.FindFirst(CurrentUserClaimTypes.UserType)?.Value, "Inspector", StringComparison.Ordinal) &&
                    context.User.FindAll(CurrentUserClaimTypes.AccessModule).Any(claim => string.Equals(claim.Value, AccessModuleCodes.WorkSafety, StringComparison.OrdinalIgnoreCase)));
            });

            options.AddPolicy("MaterialsInspectionReady", policy =>
            {
                policy.AuthenticationSchemes.Add(MvcAuthenticationSchemes.Supervisor);
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(context =>
                    !ReadBooleanClaim(context.User, CurrentUserClaimTypes.ForceChangePassword) &&
                    !ReadBooleanClaim(context.User, CurrentUserClaimTypes.IsMaster) &&
                    string.Equals(context.User.FindFirst(CurrentUserClaimTypes.UserType)?.Value, "Inspector", StringComparison.Ordinal) &&
                    context.User.FindAll(CurrentUserClaimTypes.AccessModule).Any(claim => string.Equals(claim.Value, AccessModuleCodes.MaterialInspection, StringComparison.OrdinalIgnoreCase)));
            });

            options.AddPolicy("MasterReady", policy =>
            {
                policy.AuthenticationSchemes.Add(MvcAuthenticationSchemes.Supervisor);
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(context =>
                    !ReadBooleanClaim(context.User, CurrentUserClaimTypes.ForceChangePassword) &&
                    ReadBooleanClaim(context.User, CurrentUserClaimTypes.IsMaster));
            });

            options.AddPolicy("OperatorAuthenticated", policy =>
            {
                policy.AuthenticationSchemes.Add(MvcAuthenticationSchemes.Operator);
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(context =>
                    context.User.FindFirst(CurrentOperatorClaimTypes.OperatorId) is not null &&
                    context.User.FindFirst(CurrentOperatorClaimTypes.SectorId) is not null);
            });

            options.AddPolicy("OperatorChecklistReady", policy =>
            {
                policy.AuthenticationSchemes.Add(MvcAuthenticationSchemes.Operator);
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(context =>
                    context.User.FindFirst(CurrentOperatorClaimTypes.OperatorId) is not null &&
                    context.User.FindFirst(CurrentOperatorClaimTypes.SectorId) is not null &&
                    !ReadBooleanClaim(context.User, CurrentOperatorClaimTypes.ForceChangePassword));
            });
        });

        return services;
    }

    private static bool ReadBooleanClaim(ClaimsPrincipal user, string claimType)
    {
        return bool.TryParse(user.FindFirst(claimType)?.Value, out var value) && value;
    }

    private static void RegisterDatabaseBackedServices(IServiceCollection services)
    {
        services.AddScoped<IDashboardReader, DbDashboardReader>();
        services.AddScoped<IChecklistReader, DbChecklistReader>();
        services.AddScoped<INonOkReader, DbNonOkReader>();
        services.AddScoped<INonOkWorkflowService, DbNonOkWorkflowService>();
        services.AddScoped<IOperatorEquipmentReader, DbOperatorEquipmentReader>();
        services.AddScoped<IOperatorChecklistService, DbOperatorChecklistService>();
        services.AddScoped<IStpInspectionService, DbStpInspectionService>();
        services.AddScoped<IStpDocumentControlService, DbStpDocumentControlService>();
    }

    private static string? ResolveConnectionString(IConfiguration configuration, MvcDatabaseOptions databaseOptions)
    {
        var directConnectionString = configuration.GetConnectionString("Default")
            ?? databaseOptions.Default
            ?? configuration.GetConnectionString("AppDbConnectionString")
            ?? databaseOptions.AppDbConnectionString
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

        if (!string.IsNullOrWhiteSpace(directConnectionString))
        {
            return directConnectionString.Trim();
        }

        return null;
    }
}
