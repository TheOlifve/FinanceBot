using System.Windows.Input;
using FinanceBot.Commands;
using FinanceBot.Data;
using FinanceBot.Exceptions;
using FinanceBot.Options;
using FinanceBot.Services;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;

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

builder.Services.AddSingleton<ISpendingParser, SpendingParser>();
builder.Services.AddSingleton<ITelegramBotClient> (new TelegramBotClient(builder.Configuration["Telegram:BotToken"]!));

builder.Services.AddHostedService<TelegramPollingWorker>();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();