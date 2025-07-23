using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chats.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            string connectionString = Environment.GetEnvironmentVariable("ConnectionString")!;
            string databaseName = Environment.GetEnvironmentVariable("DatabaseName")!;

            var settings = MongoClientSettings.FromConnectionString(connectionString);
            settings.GuidRepresentation = GuidRepresentation.Standard;

            services.AddSingleton<IMongoClient>(serviceProvider =>
            {
                return new MongoClient(connectionString);
            });

            services.AddSingleton<IMongoDatabase>(provider =>
            {
                var client = provider.GetRequiredService<IMongoClient>();
                return client.GetDatabase(databaseName);
            });

            return services;
        }
    }
}
