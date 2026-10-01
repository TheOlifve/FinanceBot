using System.Windows.Input;
using FinanceBot.Commands;
using FinanceBot.Data;
using FinanceBot.Exceptions;
using FinanceBot.Options;
using FinanceBot.Services;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.Configure<TelegramOptions>(builder.Configuration.GetSection(TelegramOptions.SectionName));

builder.Services.AddDbContext<AppDbContext>(
    options => options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<ISpendingRepository, SpendingRepository>();

builder.Services.AddKeyedScoped<ITelegramCommand, CommandStart>("/start");
builder.Services.AddKeyedScoped<ITelegramCommand, CommandToday>("/today");
builder.Services.AddKeyedScoped<ITelegramCommand, CommandMonth>("/month");

builder.Services.AddSingleton<ISpendingParser, SpendingParser>();
builder.Services.AddSingleton<ITelegramBotClient> (new TelegramBotClient(builder.Configuration["Telegram:BotToken"]!));

//builder.Services.AddHostedService<TelegramPollingWorker>();



var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

    string? baseUrl = config["PublicBaseUrl"];

    if (string.IsNullOrWhiteSpace(baseUrl))
    {
        throw new InvalidOperationException("PublicBaseUrl is not configured in appsettings.json.");
    }

    string webhookUrl = $"{baseUrl}/api/bot";

    await botClient.SetWebhook(
        url: webhookUrl,
        allowedUpdates: [UpdateType.Message]
    );

    Console.WriteLine($"Webhook successfully registered at: {webhookUrl}");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();