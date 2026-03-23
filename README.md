# Business Assistant API

Minimal API em .NET 9 para cadastro de clientes com autenticacao JWT, refresh token, rate limiting e Redis.

- [Documentacao detalhada](docs/business-assistant-portfolio.md)

## Quick Start

```bash
# Subir API + PostgreSQL + Redis
docker compose up -d --build

# Swagger UI
# http://localhost:8080
```

## Endpoints

| Metodo | Rota | Descricao | Auth |
|--------|------|-----------|------|
| POST | `/api/v1/auth/signup` | Cadastro de usuario | Nao |
| POST | `/api/v1/auth/login` | Login (retorna JWT + refresh token) | Nao |
| POST | `/api/v1/auth/refresh-token` | Rotacao de tokens | Nao |
| POST | `/api/v1/auth/logout` | Logout (blacklist do token) | Sim |
| GET | `/api/v1/customers` | Listar clientes ativos | Sim |
| GET | `/api/v1/customers/{id}` | Buscar cliente por ID | Sim |
| POST | `/api/v1/customers` | Criar cliente | Sim |
| PUT | `/api/v1/customers/{id}` | Atualizar cliente | Sim |
| DELETE | `/api/v1/customers/{id}` | Soft delete do cliente | Sim |

## Estrutura do Projeto

```
src/BusinessAssistant.Api/
├── Configurations/       # Extension methods (DI, Swagger, JWT, Redis, RateLimit, Database)
├── Data/                 # AppDbContext (EF Core + PostgreSQL)
├── DTOs/                 # Request/Response records
├── Endpoints/            # Minimal API route handlers
├── Exceptions/           # Custom exceptions (400, 401, 403, 404, 409, 422, 500)
├── Middleware/            # ExceptionMiddleware, ClaimsMiddleware
├── Models/               # User, PasswordModel, Customer
├── Services/             # AuthService, CustomerService, TokenService, RedisService
├── Validators/           # FluentValidation validators
└── Program.cs            # Composicao via extension methods
```

## Tech Stack

| Tecnologia | Versao | Funcao |
|------------|--------|--------|
| .NET | 9.0 | Runtime e framework |
| PostgreSQL | 16 | Banco de dados |
| Redis | 7 | Cache de tokens (JWT + refresh) |
| EF Core | 9.0 | ORM |
| FluentValidation | 12.x | Validacao de inputs |
| Swashbuckle | 6.9 | Swagger/OpenAPI |
| StackExchange.Redis | 2.12 | Cliente Redis direto |
| Docker | - | Containerizacao |

## Desenvolvimento Local

```bash
# Requisitos: .NET 9 SDK, Docker

# Subir tudo via Docker
docker compose up -d --build

# OU rodar apenas dependencias e API local
docker compose up -d postgres redis
dotnet run --project src/BusinessAssistant.Api
```

## Ambientes

| Ambiente | Trigger | Config |
|----------|---------|--------|
| Development | Local (`docker compose up`) | `appsettings.Development.json` |
| Staging | Push em `develop` | GitHub Secrets (STG_*) |
| Production | Tag `v*` | GitHub Secrets (PROD_*) |

## CI/CD

- **CI**: Build + test em push/PR para `main` e `develop`
- **Staging**: Build image + deploy via SSH em push para `develop`
- **Production**: Build image versionada + deploy via SSH em tags `v*`

## Licenca

MIT
