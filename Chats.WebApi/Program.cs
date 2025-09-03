using Chats.WebApi.Hubs;
using Chats.Application;
using Chats.Infrastructure;
using Microsoft.Extensions.Options;
using Chats.Domain;
using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddSingleton(new ConcurrentDictionary<Guid, List<Message>>());

builder.Services.AddSignalR();

var app = builder.Build();

app.MapControllers();

app.MapHub<ChatHub>("/chatHub");

app.Run();
