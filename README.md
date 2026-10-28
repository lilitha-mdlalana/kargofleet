# KargoFleet

KargoFleet is a small educational fleet-management demo: a Flutter driver app backed by an
ASP.NET Core (C#) API, built to learn Flutter, API architecture, authentication, PostgreSQL,
Docker, and open-source mapping. It is not a production fleet-management platform.

## Repository structure

```
kargofleet/
├── apps/
│   ├── driver/       # Flutter mobile application
│   └── api/          # ASP.NET Core (C#) Web API
├── packages/
│   └── api-contract/ # OpenAPI specification
├── docker-compose.yml
└── README.md
```

## Prerequisites

- .NET 8 SDK
- Flutter SDK
- Docker (for PostgreSQL)

## Running the auth demo end to end

### 1. Start PostgreSQL

```bash
docker compose up -d
```

### 2. Set up and run the API

```bash
cd apps/api/src/Api
cp ../../.env.example .env   # JWT_SECRET, ConnectionStrings__Default, ASPNETCORE_URLS
dotnet restore
dotnet run
```

The API auto-loads `.env` from its working directory on startup (if present) and applies it
before reading configuration, so editing `.env` is enough — no need to export variables into
your shell. Migrations and demo-data seeding run automatically on startup in the Development
environment. The API listens on `http://localhost:3000`. For a faster edit/run loop, use
`dotnet watch run` instead of `dotnet run`.

Seeding creates a demo driver you can log in as:

- Phone: `+27821234567`
- Driver: Thando Mokoena (license C1)
- Vehicle: Toyota Quantum, registration CA 123-456
- Trip: Cape Town → Stellenbosch (status: scheduled)

There is no external SMS provider in development — the OTP code for signup/login is printed to
the API server's console log instead of being sent by SMS.

### 3. Run the Flutter driver app

```bash
cd apps/driver
flutter run
```

The app's API base URL defaults to `http://10.0.2.2:3000`, which resolves to your host machine's
`localhost` from the Android emulator. For iOS simulator, web, or a physical device, point it at
your machine's reachable address instead, e.g.:

```bash
flutter run --dart-define=API_BASE_URL=http://localhost:3000
```

### Demo flow

Sign up or log in with the seeded phone number `+27821234567`, check the API server logs for the
OTP code, and enter it on the verification screen.

## Running tests

API tests (xUnit: unit tests plus integration tests against a throwaway Postgres container via
Testcontainers, so Docker must be running):

```bash
cd apps/api
dotnet test
```

Flutter tests:

```bash
cd apps/driver
flutter test
```

## API contract

The OpenAPI specification for the implemented endpoints lives at
`packages/api-contract/openapi.yaml`.
