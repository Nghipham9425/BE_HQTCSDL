# TCG E-commerce API

ASP.NET Core 8 REST API for the TCG e-commerce platform. The API uses Oracle with Entity Framework Core and JWT authentication with role-based authorization.

## Main modules

- Authentication and user profiles
- Products, categories, TCG sets and cards
- Cart, wishlist, vouchers and addresses
- Orders, payments and inventory
- Admin, order-manager and inventory-manager workflows

## Run locally

1. Copy `BE_HQTCSDL/.env.example` to `BE_HQTCSDL/.env` and fill in local values.
2. Ensure the Oracle database is available and the required schema has been created.
3. Run:

```bash
dotnet restore
dotnet run --project BE_HQTCSDL/BE_HQTCSDL.csproj
```

Swagger is available at `/swagger`.

## Health endpoints

- `GET /api/v1/health` checks that the API process is running.
- `GET /api/v1/health/db` checks connectivity to Oracle.

Unexpected server errors are returned as RFC 7807 Problem Details without exposing stack traces.

## Environment variables

See `.env.example`. Secrets must be provided through local environment configuration or the deployment platform and must never be committed.
