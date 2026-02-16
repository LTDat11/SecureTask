# SecureTaskApi
ASP.NET Core Web API (.NET 8) with PostgreSQL using Entity Framework Core and JWT Authentication.

---

## 🚀 Tech Stack

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- Docker (Colima)

---

## 📦 Requirements

- .NET 8 SDK
- Docker
- Colima (for macOS)

---

## 🐳 Start Docker (Colima)

Start Colima:

```bash
colima start
```

Verify Docker is running:

```bash
docker ps
```

Run PostgreSQL Container:
Compose V1.

```bash
docker-compose up -d
```

## Environment Configuration (IMPORTANT)
This project uses environment variables for security.

Generate JWT Secret Key

```bash
openssl rand -base64 32
```

Create .env file (root project)
DO NOT commit this file.

Create .env:
```
JWT_KEY=YOUR_GENERATED_SECRET
JWT_ISSUER=SecureTaskApi
JWT_AUDIENCE=SecureTaskApiUser
DATABASE_URL="Host=localhost;Port=5432;Database=securetaskdb;Username=postgres;Password=YourPassWord"
```

Load ENV into shell (macOS / Linux)
Before running the app:

```bash
set -a
source .env
set +a
```

Verify:

```bash
echo $DATABASE_URL
```

## Run Database Migration:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```
Run Application:

```bash
dotnet run
```

Swagger UI:

```bash
https://localhost:7125/swagger
```

## Verify Database
Access PostgreSQL inside container:

```bash
docker exec -it securetask-postgres psql -U postgres -d securetaskdb
```

List tables:

```bash
\dt
```

Stop PostgreSQL:
```bash
docker stop securetask-postgres
```




