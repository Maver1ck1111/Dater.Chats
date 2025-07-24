using Chats.Application.RepositoryContracts;
using Chats.Application.Services;
using Chats.Application.ServicesContracts;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chats.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IEncryptionService, EncryptorService>();

            return services;
        }
    }
}
