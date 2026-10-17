# PasswordManager — Enterprise Zero-Knowledge Full-Stack Password Manager

A secure, enterprise-grade, zero-knowledge password manager built with **ASP.NET Core Web API (.NET 10 LTS)** and **React 19 (TypeScript + Vite + Tailwind CSS)** following Clean Architecture, SOLID principles, and defense-in-depth security.

---

## 1. Zero-Knowledge Cryptographic Architecture

The core tenet of this system is that **the server never learns the user's master password, keys, or plaintext vault data**.

```
[Browser Client]
  │
  ├── 1. Master Password + Random Salt (64-bit client salt)
  │       │
  │       ▼
  │     Argon2id KDF (m=64MB, t=3, p=4)
  │       │
  │       ▼
  │     Key Encryption Key (KEK) ─── Encrypts ──► [Vault Encryption Key (VEK)]
  │                                                      │
  │                                                      ▼
  │                                                AES-256-GCM (Fresh 96-bit Nonce)
  │                                                      │
  │                                                      ▼
  │                                          [Encrypted Credential Payload]
  │                                                      │
  │                                                      ▼
  └── Encrypted Ciphertext + Nonce + Wrapped VEK ──► [ASP.NET Core Web API]
                                                              │
                                                              ▼
                                                     [PostgreSQL Database]
```

### Key Security Principles:
1. **Client-Side Derivation**: The master password is processed strictly in the browser using WebAssembly Argon2id (`hash-wasm`) with calibrated parameters (64 MB RAM, 3 iterations, 4 lanes). A 256-bit Key Encryption Key (KEK) is derived.
2. **Key Wrapping (Envelope Encryption)**: A cryptographically random 256-bit Vault Encryption Key (VEK) is generated on the client via `crypto.getRandomValues()` and wrapped with AES-256-GCM using the KEK.
3. **Payload Encryption**: All sensitive entry fields (title, username, password, URL, notes, custom fields, favorites) are packed into a single payload and encrypted client-side using **AES-256-GCM** with a distinct 96-bit nonce per operation.
4. **Transient In-Memory VEK**: Unwrapped VEKs reside strictly in browser RAM within Zustand state and are wiped immediately upon lock or logout. They are never written to `localStorage` or `sessionStorage`.
5. **Zero-Knowledge Storage**: The backend stores only wrapped VEKs, KDF parameters, random salts, nonces, and ciphertexts. It can never decrypt vault entries.

---

## 2. Clean Architecture & Solution Structure

The backend strictly follows Clean Architecture with unidirectional dependency flow:

```
               ┌───────────────────────┐
               │  PasswordManager.Api  │
               └───────────┬───────────┘
                           │
             ┌─────────────┴─────────────┐
             ▼                           ▼
┌─────────────────────────┐  ┌──────────────────────────────┐
│ PasswordManager.Infrast.│  │ PasswordManager.Application  │
└────────────┬────────────┘  └──────────────┬───────────────┘
             │                              │
             └─────────────┬────────────────┘
                           ▼
               ┌───────────────────────┐
               │ PasswordManager.Domain│
               └───────────────────────┘
```

### Repository Layout:
```
PasswordManagerSystem/
├── PasswordManager.sln                 # .NET 10 Solution aggregating all projects
├── docker-compose.yml                  # Production & local multi-container stack
├── Dockerfile.api                      # Multi-stage .NET 10 SDK + Alpine runtime build
├── .env.example                        # Environment configuration template
├── README.md                           # Complete architectural and operational documentation
│
├── scripts/
│   ├── backup-db.sh                    # Automated PostgreSQL backup script (Linux/Docker)
│   ├── backup-db.ps1                   # Automated PostgreSQL backup script (PowerShell)
│   ├── restore-db.sh                   # Disaster recovery database restore script (Linux)
│   └── restore-db.ps1                  # Disaster recovery database restore script (PowerShell)
│
├── src/
│   ├── PasswordManager.Domain/         # Entities, Value Objects, Domain Exceptions, Rules
│   ├── PasswordManager.Application/    # Use cases, Interfaces, DTOs, FluentValidation
│   ├── PasswordManager.Infrastructure/ # EF Core AppDbContext, Repositories, Argon2, Sessions
│   └── PasswordManager.Api/            # REST API Controllers, Middleware, Health Checks, OpenAPI
│
├── tests/
│   ├── PasswordManager.Domain.Tests/   # Unit tests for domain logic and business rules (26 tests)
│   ├── PasswordManager.Application.Tests/ # Application use-case and service tests (45 tests)
│   └── PasswordManager.Api.IntegrationTests/ # Full-pipeline HTTP integration tests (19 tests)
│
└── frontend/
    └── password-manager-web/           # React 19 + TypeScript + Vite + Tailwind CSS SPA
        ├── Dockerfile                  # Multi-stage Vite build + Nginx Alpine runtime
        ├── nginx.conf                  # Nginx reverse proxy, gzip, SPA routing & security headers
        └── src/
            ├── auth/                   # Authentication context & session API
            ├── crypto/                 # Web Crypto AES-256-GCM & Argon2id KDF services
            ├── generator/              # Cryptographic password generator with entropy meter
            ├── vault/                  # Client-side vault decryption, store & API client
            └── components/             # Tailwind CSS modern dashboard, modals & UI views
```

---

## 3. Technology Stack

### Backend
- **Framework**: ASP.NET Core Web API (.NET 10 LTS)
- **Architecture**: Clean Architecture / Domain-Driven Design (DDD)
- **Persistence**: Entity Framework Core 10 + PostgreSQL 17 (Npgsql)
- **Authentication**: Stateful sessions via `HttpOnly`, `SameSite=Lax`, `Secure` cookies with SHA-256 token hashing
- **Security**: Anti-CSRF verification middleware, rate limiting (15 req/min on auth), security headers (`nosniff`, `DENY` clickjacking, strict referrer, disabled Kestrel `Server` header)
- **Health Checks**: ASP.NET Core Health Checks on `/health` and `/api/health` verifying real-time database connectivity
- **Testing**: xUnit, FluentValidation, WebApplicationFactory integration test harness

### Frontend
- **Framework**: React 19 + TypeScript (strict mode) + Vite
- **Styling**: Tailwind CSS v4 + Lucide React icons
- **State & Sync**: TanStack Query (React Query) + Zustand
- **Cryptography**: Web Crypto API (`SubtleCrypto` AES-256-GCM) + WebAssembly Argon2id (`hash-wasm`)
- **Testing**: Vitest + JSDOM

### Infrastructure & DevOps
- **Orchestration**: Docker Compose with health checks and dedicated bridge network (`pm-network`)
- **Web Server / Reverse Proxy**: Nginx Alpine serving SPA static assets and proxying `/api/` and `/health`
- **CI/CD**: GitHub Actions pipeline (`.github/workflows/ci.yml`) building, testing, and containerizing backend and frontend

---

## 4. Getting Started

### Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- [Node.js (v20+ or v24 LTS)](https://nodejs.org/) & npm
- [Docker & Docker Compose](https://www.docker.com/)

### Quickstart with Docker Compose (Recommended)
Clone the repository and run:
```bash
# 1. Copy configuration template
cp .env.example .env

# 2. Build and start PostgreSQL, API, and Web containers
docker compose up --build -d

# 3. View status
docker compose ps
```
- Web Application: [http://localhost:3000](http://localhost:3000)
- API Documentation (OpenAPI): [http://localhost:8080/openapi/v1.json](http://localhost:8080/openapi/v1.json)
- Health Check: [http://localhost:3000/health](http://localhost:3000/health) or [http://localhost:8080/health](http://localhost:8080/health)

---

### Local Development Setup

#### 1. Start PostgreSQL
```bash
docker compose up postgres -d
```

#### 2. Run Backend API
```bash
dotnet run --project src/PasswordManager.Api/PasswordManager.Api.csproj
```
The API starts at `http://localhost:8080` (or `http://localhost:5000` / `https://localhost:7000` depending on launch profile).

#### 3. Run Frontend
```bash
cd frontend/password-manager-web
npm install
npm run dev
```
Open [http://localhost:5173](http://localhost:5173) in your browser.

---

## 5. Security & Hardening Features

- **Anti-CSRF Protection**: Stateful session cookies are complemented by custom request header validation (`X-Requested-With: XMLHttpRequest` or `X-CSRF-Protection`). Cross-origin websites cannot forge requests because browsers block custom headers on cross-origin requests unless explicitly permitted by CORS preflight.
- **Defense-in-Depth Headers**: Nginx and Kestrel emit:
  - `X-Content-Type-Options: nosniff`
  - `X-Frame-Options: DENY`
  - `Referrer-Policy: strict-origin-when-cross-origin`
  - `Permissions-Policy: camera=(), microphone=(), geolocation=()`
- **Memory Safety**: Decrypted master passwords, KEKs, and VEKs are never persisted in Web Storage (`localStorage` or `sessionStorage`). Memory is wiped on lock or logout.
- **Tenant Isolation**: All repository queries strictly enforce `UserId == currentUserId`.
- **Soft-Delete with Purge**: Trash items have `DeletedAt` timestamps with global query filters. Users can restore items or purge them permanently.
- **Real-Time Health Monitoring**: `/health` endpoint checks database connectivity and returns HTTP 200 `Healthy` when operational.

---

## 6. Automated Backups & Disaster Recovery

### Creating a Backup
Automated scripts create timestamped binary PostgreSQL archives and enforce a configurable retention policy:

**Linux / macOS:**
```bash
chmod +x ./scripts/backup-db.sh
./scripts/backup-db.sh ./backups
```

**Windows (PowerShell):**
```powershell
.\scripts\backup-db.ps1 -BackupDir ".\backups" -RetentionDays 7
```

### Restoring from Backup
Restores an existing `.dump` archive to the target database:

**Linux / macOS:**
```bash
chmod +x ./scripts/restore-db.sh
./scripts/restore-db.sh ./backups/passwordmanager_db_YYYYMMDD_HHMMSS.dump
```

**Windows (PowerShell):**
```powershell
.\scripts\restore-db.ps1 -BackupFile ".\backups\passwordmanager_db_YYYYMMDD_HHMMSS.dump"
```

---

## 7. Testing & Quality Assurance

The codebase includes **123 automated tests** spanning all tiers:

```bash
# Run backend tests (90 tests across Domain, Application, and API integration)
dotnet test PasswordManager.sln

# Run frontend tests (33 tests covering AES-256-GCM, Argon2id, VEK wrapping, password generator)
cd frontend/password-manager-web
npm test

# Build frontend production bundle with TypeScript type-checking
npm run build
```

### Test Suite Breakdown:
- **`PasswordManager.Domain.Tests`**: 26 unit tests (Entity creation, invariants, domain events, domain exceptions).
- **`PasswordManager.Application.Tests`**: 45 use case tests (Auth, session management, vault entry validation, folders).
- **`PasswordManager.Api.IntegrationTests`**: 19 full-stack tests (Auth lifecycle, cookie validation, CSRF defenses, health checks, vault CRUD, soft-delete, restore, purge).
- **`Vitest Frontend Suite`**: 33 tests (Argon2id KEK derivation, VEK key wrapping, AES-256-GCM authenticated cipher with tamper detection, password generator entropy, client-side encryption/decryption service).

---

## 8. Implementation Roadmap Verification

All 8 phases have been completed:

- [x] **Phase 1: Foundation** — Repository, solution, 4 Clean Architecture projects, test projects, React Vite frontend, Docker Compose configuration.
- [x] **Phase 2: Domain and Persistence** — Core entities (`User`, `VaultSettings`, `VaultEntry`, `Folder`, `AuthSession`), EF Core mappings, PostgreSQL migrations, repository abstractions, unit tests.
- [x] **Phase 3: Authentication** — Register, login, `HttpOnly` cookie sessions, logout, authorization, Argon2id password hashing, rate limiting, validation.
- [x] **Phase 4: Cryptography** — Client-side Argon2id KEK derivation, VEK generation, KEK-based key wrapping, AES-256-GCM authenticated encryption, tamper detection, cryptographic test suite.
- [x] **Phase 5: Vertical Slice** — Complete end-to-end zero-knowledge flow: authenticate, unlock vault, encrypt entry client-side, persist to database, lock vault, unlock again, retrieve and decrypt.
- [x] **Phase 6: MVP Features** — Credential CRUD, folders, favorites (starring & filtering), instant search, custom fields, secure password generator with entropy meter, responsive dashboard UI.
- [x] **Phase 7: Hardening** — Full integration test suite, Anti-CSRF custom header defenses, real-time `/health` check endpoints, XSS defenses, security headers, rate limiting, global exception handling.
- [x] **Phase 8: Deployment** — Multi-stage Docker builds, Nginx reverse proxy with SPA routing, Docker Compose orchestration, GitHub Actions CI/CD pipeline, environment configuration template (`.env.example`), automated database backup and restore scripts.
