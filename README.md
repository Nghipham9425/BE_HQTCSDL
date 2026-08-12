# TCG E-commerce API

Backend API for a trading-card e-commerce platform, built with ASP.NET Core 8 and Oracle Database. It covers the customer purchase flow, role-based administration, transactional order processing, and the foundation for realtime customer support chat.

## Highlights

- JWT authentication with refresh-token rotation and role-based authorization
- Products, categories, TCG sets, cards, inventory, vouchers and wishlist
- Checkout, payment webhook and order lifecycle management
- Oracle EF Core migrations, PL/SQL transactions, RBAC and auditing scripts
- Customer/support conversations with message history, unread state and staff handoff
- Health endpoints, correlation IDs and RFC 7807 error responses

## Technology

- .NET 8 / ASP.NET Core Web API
- Entity Framework Core 8
- Oracle Database Free 23ai
- JWT Bearer authentication
- Swagger / OpenAPI

## Project structure

Business areas are grouped consistently by feature. Repository and service interfaces live beside their implementations.

```text
Controllers/<Feature>/
Dtos/<Feature>/
Models/<Feature>/
Repositories/<Feature>/
  Interface/
Services/<Feature>/
  Interface/
Database/
  connection/
  Migrations/
  sql/
```

## Local setup

### Prerequisites

- .NET 8 SDK
- Oracle Database 23ai, locally or in Docker
- EF Core CLI: `dotnet tool install --global dotnet-ef --version 8.*`

### Environment

Copy the example file and replace every placeholder with a local value:

```powershell
Copy-Item BE_HQTCSDL/.env.example BE_HQTCSDL/.env
```

Required variables:

```dotenv
ORACLE_CONNECTION_STRING=User Id=your_user;Password=your_password;Data Source=your_oracle_service
JWT_SECRET=replace-with-a-long-random-secret
JWT_ISSUER=your_issuer
JWT_AUDIENCE=your_audience
JWT_EXPIRE_HOURS=1
REFRESH_TOKEN_EXPIRE_DAYS=7
```

Real `.env` files are ignored by Git. Never commit production credentials.

### Database and API

From the repository root:

```powershell
dotnet restore
dotnet ef database update --project BE_HQTCSDL/BE_HQTCSDL.csproj --startup-project BE_HQTCSDL/BE_HQTCSDL.csproj
dotnet run --project BE_HQTCSDL/BE_HQTCSDL.csproj --launch-profile http
```

Open Swagger at [http://localhost:5079/swagger](http://localhost:5079/swagger).

## Main endpoints

| Area | Base path |
| --- | --- |
| Authentication | `/api/v1/auth` |
| Products | `/api/v1/products` |
| Orders | `/api/v1/orders` |
| Inventory | `/api/v1/inventory` |
| Wishlist | `/api/v1/wishlist` |
| Support chat | `/api/v1/chat/conversations` |

Chat endpoints require JWT authentication. Customers can only access their own conversations. Admins and order managers act as support staff, with assignment checks before staff replies.

## Health checks

- `GET /api/v1/health` checks the API process.
- `GET /api/v1/health/db` checks Oracle connectivity.

Unexpected errors use Problem Details without exposing stack traces. Responses include `X-Request-Id` for log correlation.

## Verification

```powershell
dotnet build --no-restore
```

Before opening a pull request, confirm the build succeeds, migrations apply to a clean database, and no local `.env`, logs, build output or IDE files are staged.

## Roadmap

- SignalR realtime messaging
- AI-assisted FAQ with Oracle AI Vector Search
- Automated integration and end-to-end tests
- OpenTelemetry observability and containerized deployment
