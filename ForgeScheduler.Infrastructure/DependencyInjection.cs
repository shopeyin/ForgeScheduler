using System.Data;
using ForgeScheduler.Application.Abstractions;
using ForgeScheduler.Infrastructure.Jobs;
using ForgeScheduler.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace ForgeScheduler.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,IConfiguration configuration)
    {
        services.AddScoped<IDbConnection>(_ =>
        {
            var connectionString =
                configuration.GetConnectionString("DefaultConnection");

            return new NpgsqlConnection(connectionString);
        });

        services.AddScoped<IJobRepository, JobRepository>();

        services.AddScoped<IJobHandler, SendEmailJobHandler>();

        return services;
    }
}