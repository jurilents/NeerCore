using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NeerCore.Data.EntityFramework.Abstractions;

namespace NeerCore.Data.EntityFramework;

public static class DependencyInjection
{
    /// <summary>Adds a <see cref="DbContext"/> to DI container as <see cref="IDatabaseContext"/> abstraction.</summary>
    public static void AddDatabase<TContext>(this IServiceCollection services, Action<DbContextOptionsBuilder>? optionsAction)
            where TContext : DbContext, IDatabaseContext
    {
        services.AddDbContext<TContext>(optionsAction);
        services.AddScoped<IDatabaseContext, TContext>();
    }
}