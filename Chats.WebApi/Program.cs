using Chats.WebApi.Hubs;
using Chats.Application;
using Chats.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices();

builder.Services.AddSignalR();

var app = builder.Build();

app.MapControllers();

app.MapHub<ChatHub>("/chatHub");

app.Run();
