# PasswordManager — Enterprise Full-Stack Password Manager

A secure, enterprise-grade, zero-knowledge password manager built with **ASP.NET Core Web API (.NET 10 LTS)** and **React 19 (TypeScript + Vite)** following Clean Architecture, SOLID principles, and security-first engineering.

---

## 1. Zero-Knowledge Cryptographic Architecture

The core tenet of this system is that **the server never learns the user's master password, keys, or plaintext vault data**.

```
[Browser Client]
  │
  ├── 1. Master Password + Salt
  │       │
  │       ▼
  │     Argon2id KDF
  │       │
  │       ▼
  │     Key Encryption Key (KEK) ─── Encrypts ──► [Vault Encryption Key (VEK)]
  │                                                      │
  │                                                      ▼
  │                                                AES-256-GCM (Fresh 96-bit Nonce)
  │                                                      │
  │                                                      ▼
  └── Encrypted Ciphertext + Nonce + Wrapped VEK ──► [ASP.NET Core Web API]
                                                             │
                                                             ▼
                                                    [PostgreSQL Database]
```

### Key Principles:
1. **Client-Side Derivation**: The master password is processed strictly in the browser. Using Argon2id with calibrated parameters, a Key Encryption Key (KEK) is derived.
2. **Key Wrapping**: A cryptographically random 256-bit Vault Encryption Key (VEK) is generated on the client and wrapped (encrypted) with the KEK.
3. **Data Encryption**: All sensitive entry fields (title, username, password, URL, notes) are serialized into a single payload and encrypted client-side using **AES-256-GCM** with a distinct 96-bit nonce.
4. **Zero-Knowledge Storage**: The backend stores only the wrapped VEK, KDF parameters, random salt, nonces, and ciphertexts. It never decrypts vault entries.

---

## 2. Clean Architecture & Solution Structure

The backend follows Clean Architecture with strict unidirectional dependency references:

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
├── docker-compose.yml                  # Local development infrastructure (PostgreSQL 17)
├── README.md                           # Architectural documentation and setup guides
│
├── src/
│   ├── PasswordManager.Domain/         # Core business entities, value objects, domain rules
│   ├── PasswordManager.Application/    # Use cases, interfaces, DTOs, validation abstractions
│   ├── PasswordManager.Infrastructure/ # EF Core AppDbContext, PostgreSQL persistence, services
│   └── PasswordManager.Api/            # ASP.NET Core REST API, middleware, DI composition, OpenAPI
│
├── tests/
│   ├── PasswordManager.Domain.Tests/   # Unit tests for domain logic and business rules
│   ├── PasswordManager.Application.Tests/ # Use-case and application service tests
│   └── PasswordManager.Api.IntegrationTests/ # Full pipeline HTTP integration tests
│
└── frontend/
    └── password-manager-web/           # React 19 + TypeScript + Vite + Tailwind CSS SPA
```

---

## 3. Technology Stack

### Backend
- **Framework**: .NET 10 LTS (C#)
- **Architecture**: Clean Architecture / Web API with Controllers
- **Persistence**: Entity Framework Core + PostgreSQL
- **Testing**: xUnit + VSTest + Microsoft.AspNetCore.Mvc.Testing
- **Documentation**: ASP.NET Core OpenAPI / Swagger

### Frontend
- **Framework**: React 19 + TypeScript + Vite
- **Styling**: Tailwind CSS v4
- **State & Server Sync**: TanStack Query + Zustand
- **Forms & Validation**: React Hook Form + Zod
- **Icons**: Lucide React
- **Cryptography**: Web Crypto API + Argon2id client-side library

### Infrastructure & DevOps
- **Database**: PostgreSQL 17 Alpine via Docker Compose
- **Version Control**: Git

---

## 4. Getting Started

### Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- [Node.js (v20+ or v24 LTS)](https://nodejs.org/) & npm
- [Docker & Docker Compose](https://www.docker.com/) (for PostgreSQL)

### 1. Start the Database
```bash
docker compose up -d
```
PostgreSQL will run on `localhost:5432` with database `passwordmanager_db`.

### 2. Build & Test Backend
```bash
# Build entire solution
dotnet build PasswordManager.sln

# Run all test suites
dotnet test PasswordManager.sln
```

### 3. Run ASP.NET Core Web API
```bash
dotnet run --project src/PasswordManager.Api/PasswordManager.Api.csproj
```

### 4. Run Frontend Development Server
```bash
cd frontend/password-manager-web
npm install
npm run dev
```
Open [http://localhost:5173](http://localhost:5173) in your browser.

---

## 5. Implementation Roadmap

- [x] **Phase 1: Foundation** — Repository, solution, 4 Clean Architecture projects, 3 test suites, React Vite frontend, Docker Compose, build verification.
- [x] **Phase 2: Domain and Persistence** — Core entities (`User`, `VaultSettings`, `VaultEntry`, `Folder`, `AuthSession`), EF Core DbContext, PostgreSQL migrations, repository contracts, domain unit tests.
- [x] **Phase 3: Authentication** — Register, login, cookie sessions, logout, authorization, rate limiting, and HTTP integration tests.
- [ ] **Phase 4: Cryptography** — Client-side Argon2id KEK derivation, VEK wrapping, AES-256-GCM encryption/decryption, cryptographic roundtrip tests.
- [ ] **Phase 5: Vertical Slice** — End-to-end authenticate -> unlock -> encrypt entry -> save -> lock -> unlock -> retrieve -> decrypt.
- [ ] **Phase 6: MVP Features** — Credential CRUD, folders, favorites, search, strong password generator, dashboard UI.
- [ ] **Phase 7: Hardening** — CSRF/XSS defenses, session revocation, security audits, integration test suites.
- [ ] **Phase 8: Deployment** — Multi-stage Dockerfiles, GitHub Actions CI/CD, production secrets.
