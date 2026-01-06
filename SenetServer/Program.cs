using SenetServer.Matchmaking;
using SenetServer.Shared;
using SenetServer.SignalR;
using Microsoft.AspNetCore.SignalR;
using SenetServer.Application.ComputerOpponent;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IUserConnectionManager, UserConnectionManager>();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, UserIdProvider>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
        policy.WithOrigins("https://reluttrull.github.io", "http://localhost:5173", "http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

builder.Services.AddSingleton<IMatchmakingQueue, MatchmakingQueue>();
builder.Services.AddSingleton<IComputerOpponentQueue, ComputerOpponentQueue>();

builder.Services.AddHostedService<MatchmakingService>();
builder.Services.AddHostedService<ComputerOpponentService>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddMemoryCache();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowClient");

// ensure cookie created early in the pipeline (before SignalR)
app.Use(async (context, next) =>
{
    if (!context.Request.Cookies.ContainsKey(UserIdentity.CookieName))
    {
        UserIdentity.GetOrCreateUserId(context);
    }

    await next();
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapHub<NotificationHub>("/notifications");

app.MapControllers();

app.Run();
