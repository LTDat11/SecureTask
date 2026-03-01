# SecureTaskApi
ASP.NET Core Web API (.NET 8) with PostgreSQL using Entity Framework Core, JWT Authentication and deploy with Render.

Link: https://securetaskapi.onrender.com


This project demonstrates:

- JWT Authentication
- Health Check
- Layered Architecture (Controller → Service → Repository)
- Entity Framework Core with PostgreSQL
- Filtering, Sorting, Pagination
- Custom Exception Handling
- Clean separation of concerns

---

## 🚀 Tech Stack

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- Docker
- JWT Authentication
- BCrypt

---

## 📦 Requirements

- .NET 8 SDK
- Docker
- C#

---

## Architecture

The project follows a layered architecture:

```
Controllers
    ↓
Services (Business Logic)
    ↓
Repositories (Data Access)
    ↓
Entity Framework Core
    ↓
PostgreSQL
```

### Folder Structure

```bash
SecureTaskApi/
│
├── Controllers/
├── Services/
│   ├── Interfaces/
│   └── Implementations/
│
├── Repositories/
│   ├── Interfaces/
│   └── Implementations/
│
├── DTOs/
├── Entities/
├── Data/
├── Exceptions/
└── Program.cs
```
---

## Authentication

Authentication is implemented using JWT (JSON Web Token).

- Password hashing: BCrypt
- Token generation: HMAC-SHA256
- Claims: UserId, Username
- Token expiration: 1 hour

---

## Features

- User Registration
- User Login
- Task CRUD operations
- Task filtering (title, status, deadline)
- Sorting (Title, Deadline, Status)
- Pagination
- Authorization (users can access only their own tasks)
- Global exception handling middleware

---
# How to Run

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
DATABASE_URL=Host=db;Port=5432;Database=securetaskdb;Username=postgres;Password=YourPassWord
ASPNETCORE_ENVIRONMENT=Development
```

## Database Migration:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## 🐳 Run with Docker

Verify Docker is running:

```bash
docker ps
```

Build - Run API and PostgreSQL Container:

```bash
docker-compose up --build -d
```

Swagger UI:

```bash
http://localhost:5050/swagger
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

## Health Check

Health checks for database connectivity

```bash
http://localhost:5050/health
```


