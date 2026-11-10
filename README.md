# 📦 Warehouse Management API

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-13-239120?logo=csharp&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-9.0-512BD4?logo=nuget&logoColor=white)](https://learn.microsoft.com/en-us/ef/core/)
[![SQL Server](https://img.shields.io/badge/MSSQL-2022-CC292B?logo=microsoft-sql-server&logoColor=white)](https://www.microsoft.com/sql-server)
[![Docker](https://img.shields.io/badge/Docker-Enabled-2496ED?logo=docker&logoColor=white)](https://www.docker.com/)
[![Tests](https://img.shields.io/badge/Tests-xUnit%20%7C%20FluentAssertions-success?logo=xunit)](https://xunit.net/)

> **Portfolio Showcase:** A high-performance, lightweight RESTful API for Warehouse Inventory Management built with **ASP.NET Core (.NET 9) Minimal APIs**, **Entity Framework Core 9**, **Microsoft SQL Server**, and **Docker**.

---

## 🌟 Key Highlights & Features

- ⚡ **ASP.NET Core Minimal APIs (.NET 9)** — Clean modular route mapping with extension methods (`MapAuthEndpoints`, `MapProductEndpoints`) achieving maximum throughput and low memory footprint.
- 🔐 **Authentication & Security**:
  - Secure password hashing using **BCrypt** (`BCrypt.Net-Next`).
  - **JWT (JSON Web Token)** generation and validation with claims-based identity (`NameIdentifier`, `Email`).
  - Protected resource routes gated by `.RequireAuthorization()`.
- 🛡️ **Request Validation**:
  - Strongly-typed validation using **FluentValidation** (SKU pattern regex, positive price checks, inventory stock constraints).
  - RFC-compliant structured error responses via `400 ValidationProblem`.
- 🗄️ **Database & ORM**:
  - **Entity Framework Core 9 (Code-First)** with relational modeling (Many-to-Many `User` ↔ `Role`, unique indexes).
  - Automated database migration on application startup (`db.Database.Migrate()`).
- 🚨 **Centralized Error Handling**:
  - Custom `IExceptionHandler` (`GlobalExceptionHandler`) conforming to **RFC 7807 / RFC 7231 ProblemDetails** to prevent stack-trace leakage while logging unhandled exceptions.
- 🐳 **Containerization & DevOps**:
  - Multi-stage **Dockerfile** producing a minimal production container image.
  - **Docker Compose** orchestration configuring the API service alongside Microsoft SQL Server 2022 with volume persistence.
- 🧪 **Automated Testing**:
  - Comprehensive unit test suite powered by **xUnit** and **FluentAssertions** (covering input validators and JWT token issuance).
- 📖 **Interactive API Documentation**:
  - Integrated **Swagger / OpenAPI** with Bearer token authentication configured in the UI.

---

## 🛠️ Technology Stack

| Layer / Concern | Technologies & Libraries |
|---|---|
| **Runtime & Language** | .NET 9, C# 13 |
| **Web Framework** | ASP.NET Core Minimal APIs |
| **Database** | Microsoft SQL Server 2022 |
| **ORM & Migrations** | Entity Framework Core 9 (SqlServer, Design) |
| **Authentication & Crypto** | `Microsoft.AspNetCore.Authentication.JwtBearer`, `BCrypt.Net-Next` |
| **Data Validation** | `FluentValidation.AspNetCore` |
| **Containerization** | Docker, Docker Compose |
| **Testing** | xUnit, FluentAssertions, Coverlet |
| **API Documentation** | Swashbuckle.AspNetCore (Swagger / OpenAPI) |

---

## 🏛️ Project Architecture & Directory Structure

```text
├── compose.yaml                         # Multi-container orchestration (API + SQL Server)
├── WebApplication1.sln                  # Visual Studio / Rider solution
├── WebApplication1/                     # Main Web API project
│   ├── Migrations/                      # EF Core database migrations
│   ├── AuthDtos.cs                      # Request and response DTOs for authentication
│   ├── AuthEndpoints.cs                 # Authentication route group (/api/auth)
│   ├── Dockerfile                       # Multi-stage production container build
│   ├── GlobalExceptionHandler.cs        # RFC-compliant centralized exception handler
│   ├── Product.cs                       # Product domain entity and DTO definitions
│   ├── ProductEndpoints.cs              # Product CRUD route group (/products)
│   ├── ProductValidators.cs             # FluentValidation rule definitions
│   ├── Program.cs                       # Entry point: DI container, middleware pipeline, JWT & Swagger
│   ├── Role.cs / User.cs / UserRole.cs  # Domain entities & relational mappings
│   ├── TokenService.cs                  # JWT token creation service
│   └── WarehouseDbContext.cs            # EF Core DbContext with model configuration
└── WebApplication1.Tests/               # Unit testing suite
    ├── CreateProductValidatorTests.cs   # Unit tests for input validation rules
    └── TokenServiceTests.cs             # Unit tests for JWT generation and claim verification
```

---

## 📡 API Reference

### 🔑 Authentication (`/api/auth`)

| Method | Endpoint | Description | Access |
|---|---|---|---|
| `POST` | `/api/auth/register` | Register a new user account | Public |
| `POST` | `/api/auth/login` | Authenticate user credentials and retrieve a JWT | Public |

#### 1. Register a User (`POST /api/auth/register`)
```json
{
  "email": "manager@warehouse.com",
  "password": "SecurePassword123!"
}
```

*Response (`201 Created`):*
```json
{
  "id": "e3a893cb-71bb-45e0-8a24-118e7e17d65b",
  "email": "manager@warehouse.com",
  "createdAt": "2026-11-10T19:00:00Z"
}
```

#### 2. Log In (`POST /api/auth/login`)
```json
{
  "email": "manager@warehouse.com",
  "password": "SecurePassword123!"
}
```

*Response (`200 OK`):*
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

---

### 📦 Inventory & Products (`/products`)

> 🔒 *All `/products` routes require an `Authorization: Bearer <token>` header.*

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/products` | Retrieve all inventory products |
| `GET` | `/products/{id}` | Retrieve a single product by `Guid` |
| `POST` | `/products` | Create a new product (validated via FluentValidation) |
| `PUT` | `/products/{id}` | Update an existing product's details |
| `DELETE` | `/products/{id}` | Delete a product from inventory |

#### Example: Create Product (`POST /products`)
```json
{
  "name": "Wireless Barcode Scanner",
  "sku": "SCAN-WIFI-001",
  "price": 149.99,
  "quantityInStock": 25
}
```

*Validation Constraints:*
- `Name`: Required, maximum 100 characters.
- `Sku`: Required, uppercase letters, numbers, and hyphens only (`^[A-Z0-9\-]+$`).
- `Price`: Must be greater than `0`.
- `QuantityInStock`: Must be greater than or equal to `0`.

---

## 🚀 Getting Started

### Option 1: Run with Docker Compose (Recommended)

Prerequisites: [Docker Desktop](https://www.docker.com/products/docker-desktop) installed and running.

1. Clone the repository:
   ```bash
   git clone https://github.com/your-username/warehouse-api.git
   cd warehouse-api
   ```

2. Build and launch the services:
   ```bash
   docker compose up -d --build
   ```

3. Open Swagger UI in your browser:
   ```text
   http://localhost:5000/swagger
   ```

*Database migrations run automatically on container startup.*

---

### Option 2: Run Locally (.NET SDK)

Prerequisites: [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) and a running SQL Server instance.

1. Start SQL Server via Docker:
   ```bash
   docker compose up -d warehouse-db
   ```

2. Run the application:
   ```bash
   dotnet run --project WebApplication1/WebApplication1.csproj
   ```

3. Navigate to Swagger UI:
   ```text
   http://localhost:5109/swagger
   # or https://localhost:7109/swagger
   ```

---

## 🧪 Running Automated Tests

The solution includes automated unit tests checking validation constraints and JWT token issuance:

```bash
dotnet test
```

Expected output:
```text
Passed!  - Failed: 0, Passed: 6, Skipped: 0, Total: 6
```

---

## 💼 Portfolio Highlights & Engineering Practices

This repository demonstrates real-world software engineering competencies:
1. **Modern C# / .NET 9 Idioms**: Records, primary constructors, pattern matching, async/await throughout.
2. **RESTful API Design**: Clear resource modeling, standard HTTP status codes (`200`, `201`, `400`, `401`, `404`, `409`, `500`), clean separation of concerns (DTOs, Endpoints, Validators, Services, DbContext).
3. **Security Best Practices**: BCrypt password hashing, symmetric JWT signing, claims-based authorization, defensive exception filtering.
4. **Data Modeling & EF Core**: Relational modeling, composite primary keys on join entities, unique index constraints, and migration management.
5. **DevOps & Cloud-Ready Packaging**: Multi-stage Docker packaging for minimal footprint and container orchestration with volume persistence.
6. **Testing Rigor**: Behavior verification using xUnit theory tests (`[Theory]`, `[InlineData]`) and fluent assertions.

---

## 📈 Roadmap

- [ ] Refresh token rotation mechanism
- [ ] Role-Based Access Control (RBAC) authorization policies (`Admin`, `WarehouseWorker`)
- [ ] Pagination, sorting, and filtering for product queries (`GET /products?page=1&pageSize=10`)
- [ ] Integration testing with `WebApplicationFactory` and `Testcontainers`
- [ ] CI/CD pipeline with GitHub Actions (automated build, test, and container push)

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).
