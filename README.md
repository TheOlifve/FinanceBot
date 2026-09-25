# 📊 FinanceBot

A single-process **ASP.NET Core** service featuring a Telegram Bot for personal finance and expense tracking. It allows users to log expenses directly via Telegram, sends automated daily digests, and generates anonymized web reports via secure bearer tokens.

---

## 🛠️ Architecture & Core Design

The application runs as a single ASP.NET Core web host, driving two background services alongside Web API endpoints:

```text
FinanceBot/
├── Program.cs                  — DI composition, HTTP pipeline, hosted services, controller mapping
├── appsettings.json             — Telegram options, currency, digest schedule, DB connections
├── Options/
│   └── TelegramOptions.cs       — Strongly typed Telegram settings
├── Data/
│   ├── AppDbContext.cs          — EF Core database context
│   ├── Chat.cs                  — Chat entity (holds secret report tokens)
│   └── Spending.cs               — Spending record entity
├── Services/
│   ├── SpendingParser.cs         — Raw message parsing & validation engine
│   ├── SpendingRepository.cs     — Database query abstraction (optional)
│   └── ReportBuilder.cs          — Aggregation engine for statistics & report data
├── Workers/
│   ├── TelegramPollingWorker.cs  — BackgroundService handling Telegram Long Polling
│   └── DailyDigestWorker.cs      — BackgroundService for scheduled UTC daily digests
└── Controllers/
    └── ReportController.cs       — GET /report/{token} endpoint delivering HTML reports
```

### Key Architectural Highlights
* **Single-Instance Polling:** `TelegramBotClient` is registered as a **Singleton** service.
* **Scoped Worker Resolution:** `DailyDigestWorker` utilizes `IServiceScopeFactory` to safely create a dedicated scope per execution loop for consuming the scoped `AppDbContext`.
* **Zero-Leakage Security:** `ChatId` is never exposed outside system boundaries. Web reports are accessed via cryptographically secure random tokens generated with `RandomNumberGenerator`.

---

## 🚀 Features

* **Instant Expense Logging:** Parse messages like `12.50 food lunch with colleagues` directly into structured database entries.
* **Commands Support:**
  * `/start` — Registers the chat, generates a unique secret report link, and returns input instructions.
  * `/today` — Summarizes today's total expenses and lists individual records (UTC timezone).
  * `/month` — Calculates monthly metrics on demand.
* **Automated UTC Daily Digest:** Sends a daily summary at a configured hour (`DigestHourUtc`) to active chats.
* **Web-Based Analytics:** View full non-authenticated HTML reports via token-based secret URLs (`/report/{token}`).

---

## 📝 Input Format & Validation Rules

Messages that are not bot commands are evaluated against strict rules:

1. **Amount (1st token):** Must be a positive decimal (`> 0`). Supports both `,` and `.` decimal separators. Must be the first token in the string.
2. **Category (2nd token):** Case-insensitive string matching regex `^[a-zA-Z]+$`. Saved in lowercase.
3. **Note (Remaining text):** Optional plain-text description (otherwise `null`).

### Input Examples
| Input Message | Status | Result |
| :--- | :--- | :--- |
| `15.50 coffee` | ✅ Valid | Amount: `15.50`, Category: `coffee`, Note: `null` |
| `1200 supermkt weekly groceries` | ✅ Valid | Amount: `1200`, Category: `supermkt`, Note: `weekly groceries` |
| `coffee 4.50` | ❌ Invalid | Error: Amount must be the first item in the message. |
| `-50 taxi` | ❌ Invalid | Error: Amount must be greater than zero. |

---

## 🗄️ Database Configuration & Setup (PostgreSQL)

This project uses **Entity Framework Core** configured with **PostgreSQL**.

### 1. Connection String Setup

Configure your PostgreSQL connection parameters inside `appsettings.json` (or `appsettings.Development.json`):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=finance_db;Username=postgres;Password=postgres"
  },
  "Telegram": {
    "BotToken": "YOUR_TELEGRAM_BOT_TOKEN_HERE"
  },
  "Currency": "$",
  "DigestHourUtc": 20,
  "PublicBaseUrl": "http://localhost:5000"
}
```

### 2. How Database Initialization Works

When someone clones this repository, the actual database instance is **not included in source control**. Instead, Entity Framework Core recreates the schema on any local or server environment using **EF Core Migrations**.

To set up the database locally:

1. Ensure your PostgreSQL service is running.
2. Run the EF Core update command from your terminal:
   ```bash
   dotnet ef database update
   ```
   *This command reads the migration files in `Migrations/` and automatically creates the `wishlist_db` database along with all required tables (`Chats`, `Spendings`).*

> **Developer Note:** When adding or modifying C# entity models, generate a new migration file before running updates:
> ```bash
> dotnet ef migrations add <MigrationName>
> ```

---

## 📊 Analytics & Metrics Engine

Calculated metrics share unified logic across `/month`, daily digests, and HTML reports:

* **Monthly Total & Count:** Sum and count of spendings within `[1st of current UTC month, current UTC time)`.
* **Average Daily Expense:** Calculated as `MonthTotal / UtcNow.Day` (current day of the month, not a static 30-day window).
* **Weekly Trends:**
  * Current 7 Days: `[today - 7, today)`
  * Previous 7 Days: `[today - 14, today - 7)`
* **Top Categories:** Top 3 expense categories grouped and ordered by cumulative spending.

---

## 🛠️ Getting Started

### Prerequisites
* **.NET 8.0 SDK** or higher
* **PostgreSQL** server running locally or in Docker

### Installation & Run Steps

1. **Clone the repository:**
   ```bash
   git clone https://github.com/your-username/FinanceBot.git
   cd FinanceBot
   ```
2. **Set your Telegram Bot Token:**
   ```bash
   dotnet user-secrets set "Telegram:BotToken" "YOUR_BOT_TOKEN"
   ```
3. **Apply Database Migrations:**
   ```bash
   dotnet ef database update
   ```
4. **Run the Application:**
   ```bash
   dotnet run
   ```
