# CoinVista – Cryptocurrency Investment Tracker

CoinVista is a full-stack ASP.NET Core MVC web application that allows users to track, analyze, and visualize their cryptocurrency investments using real-time and historical data from the [CoinGecko API](https://www.coingecko.com/en/api).

---

## Live Features

- User registration and login (session-based, stored locally)
- Add/Edit/Delete crypto investments
- Dashboard with analytics:  
  - Portfolio value trend  
  - Daily Profit/Loss %  
  - Coin allocation pie chart  
  - 7-day price trend line chart  
- Local JSON storage for data persistence
- Coin history caching to reduce API rate limits

---

## API Integration

### CoinGecko API
| Endpoint | Purpose |
|----------|---------|
| `/api/v3/coins/markets` | Get list of top 250 coins |
| `/api/v3/coins/{id}` | Fetch current price & coin details |
| `/api/v3/coins/{id}/market_chart` | Fetch 7-day price history |

To avoid rate-limiting errors (HTTP 429), historical data is cached in a local JSON file (`coin_history.json`) using a singleton `CoinHistoryCacheService`.

---

## Data Model (ER Diagram)

```plaintext
+------------------+        +------------------------+
|      User        |        |      Investment        |
+------------------+        +------------------------+
| Email (PK)       |<-------| Email (FK)             |
| FullName         |        | Id (PK)                |
| PasswordHash     |        | CryptoId               |
+------------------+        | CryptoName             |
                            | AmountInvested         |
                            | Quantity               |
                            | PurchasePrice          |
                            | PurchaseDate           |
                            +------------------------+
```

> **Note**: Both models are stored in local JSON files:
- `users.json` for authentication
- `investments.json` for portfolio entries

---

## CRUD Implementation Overview

- **Create**: Add new investment entry (via `/Investment/Create`)
- **Read**: Dashboard reads all entries belonging to the logged-in user
- **Update**: Edit existing investment entry
- **Delete**: Remove investment entry from local JSON

All changes are immediately written to disk via `InvestmentService.cs`.

---

## Technical Challenges & Solutions

| Challenge | Solution |
|----------|----------|
| **API Rate Limits (429 Errors)** | Implemented coin history cache (`coin_history.json`) with fallback logic |
| **Duplicate Investments** | Aggregated coins with the same ID on the dashboard to show unified trends |
| **Secure Local Storage** | (Optional) Planned field-level encryption, but decided to keep it JSON & readable |
| **Chart Scaling** | Converted all graphs to percentage-based metrics for better visual comparison |
| **Scoped vs Singleton Services** | Avoided EF Core & used JSON + Singleton pattern to maintain local persistence |

---

## Tech Stack

- ASP.NET Core MVC (.NET 7/8)
- C#
- Chart.js (for interactive data visualization)
- CoinGecko REST API
- Session Authentication
- JSON file-based persistence

---

## Folder Structure

```
/Controllers
/Models
/Services
/Views
    /Dashboard
    /Investment
    /Account
wwwroot/
    /img
    /data
```

---

## Run Locally

```bash
git clone https://github.com/yourusername/CoinVista.git
cd CoinVista
dotnet run
```