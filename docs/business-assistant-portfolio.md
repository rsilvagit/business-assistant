# Business Assistant API — Documentacao Tecnica

> Minimal API em C# .NET 9 para cadastro de clientes, com autenticacao JWT, refresh token via Redis, rate limiting e deploy automatizado.

- **Repositorio**: [business-assistant](https://github.com/arbo/business-assistant)
- **Stack**: .NET 9 | PostgreSQL 16 | Redis 7 | Docker | GitHub Actions

---

## 1. Visao Geral

### O que e

Uma API REST minimalista para gerenciamento de clientes, construida com Minimal API do .NET 9, seguindo principios SOLID e boas praticas de design.

### Por que este projeto

- Demonstrar arquitetura limpa em projetos de pequeno porte sem over-engineering
- Implementar autenticacao JWT com refresh token gerenciado por Redis
- Aplicar o mesmo padrao de autenticacao e tratamento de excecoes do projeto `core.flashcard-master`
- Pipeline CI/CD completo com ambientes de staging e producao

---

## 2. Arquitetura

### Diagrama de Componentes

```
┌─────────────┐     HTTP/JSON      ┌──────────────────┐
│   Cliente    │ ◄──────────────── │  Swagger UI      │
│  (Frontend)  │                    │  localhost:8080   │
└──────┬──────┘                    └──────────────────┘
       │
       ▼
┌──────────────────────────────────────────────────┐
│              Business Assistant API               │
│                                                    │
│  ┌────────────┐  ┌────────────┐  ┌─────────────┐ │
│  │  Endpoints  │  │ Middleware  │  │   Validators │ │
│  │  (Minimal)  │  │ (Exception │  │  (Fluent)    │ │
│  │             │  │  + Claims) │  │              │ │
│  └──────┬─────┘  └─────┬──────┘  └──────┬──────┘ │
│         │              │                 │         │
│         ▼              ▼                 ▼         │
│  ┌─────────────────────────────────────────────┐  │
│  │              Services (Scoped)               │  │
│  │  AuthService | CustomerService | TokenService│  │
│  └──────┬──────────────┬──────────────┬────────┘  │
│         │              │              │            │
│         ▼              ▼              ▼            │
│  ┌───────────┐  ┌───────────┐  ┌──────────────┐  │
│  │ AppDbCtx  │  │  Redis    │  │ PasswordHash │  │
│  │ (EF Core) │  │  Service  │  │  (SHA256)    │  │
│  └─────┬─────┘  └─────┬─────┘  └──────────────┘  │
└────────┼──────────────┼───────────────────────────┘
         │              │
         ▼              ▼
  ┌─────────────┐  ┌─────────┐
  │ PostgreSQL  │  │  Redis  │
  │   (Port     │  │ (Port   │
  │    5432)    │  │  6379)  │
  └─────────────┘  └─────────┘
```

### Camadas

| Camada | Pasta | Responsabilidade |
|--------|-------|-----------------|
| Configuracao | `Configurations/` | Extension methods para DI, JWT, Swagger, Redis, RateLimit, Database |
| Endpoints | `Endpoints/` | Rotas da Minimal API, validacao de input |
| Middleware | `Middleware/` | Tratamento global de excecoes, extracao de claims JWT |
| Services | `Services/` | Logica de negocio, interfaces para DI |
| Data | `Data/` | DbContext com EF Core, mapeamento de entidades |
| Models | `Models/` | Entidades do dominio |
| DTOs | `DTOs/` | Records imutaveis de request/response |
| Validators | `Validators/` | Regras de validacao com FluentValidation |
| Exceptions | `Exceptions/` | Hierarquia de excecoes mapeadas para HTTP status codes |

---

## 3. Decisoes de Design

### 3.1 Arquitetura Flat (Sem DDD)

**Problema**: Projeto pequeno com escopo bem definido — DDD adicionaria complexidade desnecessaria.

**Solucao**: Estrutura flat com separacao por pastas dentro de um unico projeto, mantendo SOLID:
- Interfaces em todos os services (ISP, DIP)
- Cada classe com responsabilidade unica (SRP)
- Extension methods para configuracao (OCP)

**Por que**: Simplicidade sem sacrificar testabilidade e manutenibilidade.

### 3.2 Custom Configuration via Extension Methods

**Problema**: `Program.cs` tende a virar um arquivo gigante com toda a configuracao.

**Solucao**: Cada concern tem sua propria classe de extensao:
```csharp
builder.Services
    .AddDatabaseConfiguration(builder.Configuration)
    .AddRedisConfiguration(builder.Configuration)
    .AddJwtAuthentication(builder.Configuration)
    .AddRateLimitConfiguration()
    .AddSwaggerConfiguration()
    .AddApplicationServices();
```

**Por que**: Program.cs limpo, cada configuracao isolada e testavel independentemente.

### 3.3 Hashing com SHA256 + SaltObject

**Problema**: Precisamos de compatibilidade com o padrao de hashing do `core.flashcard-master`.

**Solucao**: SHA256 com input formatado como `{password}:{saltId}:{saltGuid}`:
```csharp
public record SaltObject(Guid Id, Guid Salt);

public string Hash(string password, SaltObject saltObject)
{
    var input = $"{password}:{saltObject.Id}:{saltObject.Salt}";
    var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
    return Convert.ToHexStringLower(bytes);
}
```

**Por que**: Consistencia entre projetos. O Salt (UUID) fica na tabela `Password`, separado do `User`.

### 3.4 Tokens Gerenciados no Redis

**Problema**: Refresh tokens no banco relacional criam overhead e dificultam revogacao.

**Solucao**: JWT e refresh token gerenciados inteiramente no Redis:

| Chave Redis | Valor | TTL |
|-------------|-------|-----|
| `token:refresh:{refreshToken}` | Access token JWT | 24h |
| `token:blacklist:{accountId}:{tokenPrefix}` | `"true"` | 7 dias |

**Fluxo de refresh**:
1. Cliente envia refresh token
2. Redis lookup: `token:refresh:{rt}` retorna o JWT armazenado
3. Extrai claims do JWT para identificar o usuario
4. Blacklista o token antigo
5. Gera novo par (access + refresh)
6. Armazena novo par no Redis

**Por que**: Redis e ideal para dados efemeros com TTL. Evita queries no banco para operacoes frequentes.

### 3.5 Tratamento Centralizado de Excecoes

**Problema**: Try-catch espalhado nos endpoints gera duplicacao e inconsistencia nas respostas.

**Solucao**: Hierarquia de excecoes mapeada para HTTP status codes + middleware global:

```
ExceptionCustomAbstract<T>
├── BadRequest400Exception
├── Unauthorized401Exception
├── Forbidden403Exception      (helpers: EmailOrPassword, RefreshToken, TokenInvalid)
├── NotFound404Exception
├── Conflict409Exception
├── UnprocessableEntity422Exception
└── InternalServer500Exception
```

O `ExceptionMiddleware` captura todas as excecoes e converte para JSON padronizado:
```json
{
  "messages": "There are validation errors in the provided fields.",
  "validationProperties": [
    { "property": "Email", "messages": ["Email is required."] }
  ]
}
```

**Por que**: Endpoints ficam limpos (sem try-catch), respostas de erro consistentes, integracao nativa com FluentValidation via `ValidationResult()`.

### 3.6 ClaimsMiddleware + IUserClaims

**Problema**: Extrair claims do JWT em cada endpoint e repetitivo e propenso a erros.

**Solucao**: Middleware extrai claims para um objeto `IUserClaims` scoped:
```csharp
// Middleware extrai automaticamente apos autenticacao
public interface IUserClaims
{
    Guid AccountId { get; set; }
    string Name { get; set; }
    string Email { get; set; }
    string Role { get; set; }
}

// Endpoints injetam diretamente
group.MapPost("/logout", async (IUserClaims userClaims, IAuthService authService) =>
{
    await authService.LogoutAsync(userClaims.AccountId, token);
});
```

**Por que**: Padrao do `core.flashcard-master`. Centraliza extracao de claims, simplifica endpoints.

---

## 4. Modelo de Dados

### Tabelas

```
┌──────────────────────┐      ┌──────────────────────┐
│      Account          │      │      Password         │
├──────────────────────┤      ├──────────────────────┤
│ Id (PK, UUID)         │◄────│ AccountId (FK, UUID)  │
│ Name (varchar 200)    │      │ Id (PK, UUID)         │
│ Email (varchar 200)   │      │ Salt (UUID)           │
│ Phone (varchar 20)    │      │ Password (varchar 100)│
│ Role (varchar 50)     │      │ Actived (bool?)       │
│ Status (smallint)     │      │ CreatedAt (timestamp)  │
│ CreatedAt (timestamp)  │      └──────────────────────┘
└──────────────────────┘

┌──────────────────────┐
│      Customer         │
├──────────────────────┤
│ Id (PK, UUID)         │
│ Name (varchar 200)    │
│ Email (varchar 200)   │  UNIQUE
│ Phone (varchar 20)    │
│ Document (varchar 20) │  UNIQUE
│ IsActive (bool)       │
│ CreatedAt (timestamp)  │
│ UpdatedAt (timestamp?) │
└──────────────────────┘
```

**Decisao**: Senha em tabela separada (`Password`) com campo `Actived` para controle de ciclo de vida. Permite multiplas senhas por conta (historico, reset).

---

## 5. Autenticacao

### Fluxo Completo

```
1. SIGNUP
   POST /api/v1/auth/signup { email, password, confirmPassword }
   → Cria Account + Password (hash SHA256 + salt UUID)
   → Retorna { accessToken: "Bearer ...", refreshToken: "..." }

2. LOGIN
   POST /api/v1/auth/login { email, password }
   → Valida credenciais (lookup Account → Password → hash + salt)
   → Retorna { accessToken: "Bearer ...", refreshToken: "..." }

3. REFRESH
   POST /api/v1/auth/refresh-token { refreshToken }
   → Redis: busca JWT armazenado pelo refresh token
   → Blacklista token antigo, gera novo par
   → Retorna { accessToken: "Bearer ...", refreshToken: "..." }

4. LOGOUT
   POST /api/v1/auth/logout [Authorization: Bearer ...]
   → Blacklista access token no Redis (TTL 7 dias)
```

### Configuracao JWT

| Parametro | Valor | Descricao |
|-----------|-------|-----------|
| Algoritmo | HmacSha256Signature | Assinatura do token |
| Key Encoding | ASCII | Encoding da chave privada |
| ValidateIssuer | false | Sem validacao de issuer |
| ValidateAudience | false | Sem validacao de audience |
| ClockSkew | Zero | Sem tolerancia de tempo |
| SaveToken | true | Token acessivel via HttpContext |
| Access Token TTL | 1 hora | Configuravel |
| Refresh Token TTL | 24 horas | Configuravel |

### Claims do JWT

```json
{
  "accountId": "guid",
  "email": "user@email.com",
  "unique_name": "username",
  "role": "User"
}
```

---

## 6. Rate Limiting

Duas politicas configuradas com fixed window:

| Politica | Limite | Janela | Fila | Uso |
|----------|--------|--------|------|-----|
| `fixed` | 100 req | 1 min | 10 | Geral |
| `auth` | 10 req | 1 min | 2 | Endpoints de autenticacao |

Resposta quando excedido: `429 Too Many Requests`.

---

## 7. Logging

### Padrao

Formato: `[ClassName:MethodName] Mensagem com {ParametrosEstruturados}`

### Exemplos

```csharp
// AuthService
_logger.LogInformation("[AuthService:LoginAsync] Login successful for {AccountId}", user.Id);
_logger.LogWarning("[AuthService:LoginAsync] Login attempt failed: invalid password for {AccountId}", user.Id);

// CustomerService
_logger.LogInformation("[CustomerService:CreateAsync] Customer created: {CustomerId}", customer.Id);

// ExceptionMiddleware
_logger.LogInformation("[ExceptionHandlingMiddleware] - Rota: {Path} - {Response}", context.Request.Path, errors);
_logger.LogError(exception, "[ExceptionHandlingMiddleware] - Rota: {Path}", context.Request.Path);
```

### Niveis por Ambiente

| Ambiente | Default | Microsoft.AspNetCore |
|----------|---------|---------------------|
| Development | Debug | Warning |
| Staging | Information | Warning |
| Production | Information | Warning |

---

## 8. Docker e Deploy

### Containers (Desenvolvimento)

```yaml
# docker-compose.yml
services:
  api:        # .NET 9 API na porta 8080
  postgres:   # PostgreSQL 16 na porta 5432
  redis:      # Redis 7 na porta 6379
```

### Deploy (Staging/Production)

```yaml
# docker-compose.deploy.yml
# Mesmo stack, mas todas as configs vem de env vars (GitHub Secrets)
environment:
  - ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT}
  - ConnectionStrings__DefaultConnection=${DB_CONNECTION_STRING}
  - ConnectionStrings__Redis=${REDIS_CONNECTION_STRING}
  - Jwt__PrivateKey=${JWT_PRIVATE_KEY}
```

### Dockerfile (Multi-stage)

```
Stage 1: SDK 9.0 → restore + publish
Stage 2: ASP.NET 9.0 runtime → copia publish output
Porta: 8080
```

---

## 9. CI/CD (GitHub Actions)

### Pipelines

| Pipeline | Trigger | Etapas |
|----------|---------|--------|
| CI | Push/PR em `main`, `develop` | Restore → Build → Test (com PostgreSQL + Redis) |
| Staging | Push em `develop` | Build image → Push GHCR → Deploy SSH |
| Production | Tag `v*` | Build image versionada → Push GHCR → Deploy SSH |

### GitHub Secrets

Cada ambiente tem secrets com prefixo:
- **Staging**: `STG_SERVER_HOST`, `STG_DB_CONNECTION_STRING`, `STG_JWT_PRIVATE_KEY`, ...
- **Production**: `PROD_SERVER_HOST`, `PROD_DB_CONNECTION_STRING`, `PROD_JWT_PRIVATE_KEY`, ...

---

## 10. Tech Stack

| Tecnologia | Versao | Funcao |
|------------|--------|--------|
| .NET | 9.0 | Runtime, Minimal API |
| C# | 13 | Linguagem |
| PostgreSQL | 16 | Banco relacional |
| Redis | 7 | Cache de tokens |
| EF Core | 9.0 | ORM |
| FluentValidation | 12.x | Validacao de DTOs |
| Swashbuckle | 6.9 | Swagger UI + OpenAPI |
| StackExchange.Redis | 2.12 | Cliente Redis direto |
| Docker | 28.x | Containerizacao |
| GitHub Actions | - | CI/CD |

---

## 11. Estrutura do Repositorio

```
business-assistant/
├── .github/
│   └── workflows/
│       ├── ci.yml                  # Build + test
│       ├── deploy-staging.yml      # Deploy staging
│       └── deploy-prod.yml         # Deploy producao
├── docs/
│   └── business-assistant-portfolio.md
├── src/
│   └── BusinessAssistant.Api/
│       ├── Configurations/         # Extension methods (6 arquivos)
│       ├── Data/                   # AppDbContext
│       ├── DTOs/                   # Request/Response records
│       ├── Endpoints/              # AuthEndpoints, CustomerEndpoints
│       ├── Exceptions/             # Hierarquia de excecoes (8 arquivos)
│       ├── Middleware/             # ExceptionMiddleware, ClaimsMiddleware
│       │   └── Model/             # ErrorsResponse
│       ├── Models/                 # User, PasswordModel, Customer
│       ├── Services/              # 10 arquivos (interfaces + implementacoes)
│       ├── Validators/            # AuthValidator, CustomerValidator
│       ├── Program.cs             # Composicao
│       ├── appsettings.json       # Config base (apenas logging)
│       ├── appsettings.Development.json
│       ├── appsettings.Staging.json
│       └── Dockerfile             # Multi-stage build
├── docker-compose.yml             # Dev local
├── docker-compose.deploy.yml      # Staging + Production
├── BusinessAssistant.sln
├── .gitignore
└── README.md
```

---

## 12. Competencias Demonstradas

- **Minimal API (.NET 9)**: Endpoints fluentes com extension methods, versionamento de rotas
- **Autenticacao JWT**: Geracao de tokens, refresh token rotation, blacklist via Redis
- **SOLID**: Interfaces em services, SRP nos endpoints, DI via extension methods, OCP nas configurations
- **Tratamento de excecoes**: Hierarquia customizada com middleware centralizado e integracao FluentValidation
- **Redis**: Cliente direto via IConnectionMultiplexer para gerenciamento de tokens
- **PostgreSQL + EF Core**: Code-first, mapeamento fluente, tabelas separadas para credentials
- **Docker**: Multi-stage build, docker-compose para dev e deploy
- **CI/CD**: GitHub Actions com ambientes separados (staging/production), secrets, GHCR
- **Observabilidade**: Logging estruturado com ILogger, niveis por ambiente
- **Seguranca**: Rate limiting, password hashing com salt, token blacklist, soft delete
