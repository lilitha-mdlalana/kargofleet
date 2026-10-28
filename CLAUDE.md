You are helping me build KargoFleet, a small educational fleet-management application.

IMPORTANT:
This is NOT intended to be a production fleet-management platform. It is a deliberately scoped demo project whose primary purpose is to help me learn Flutter, C#/.NET, PostgreSQL, API architecture, authentication, testing, Docker, and open-source mapping.

I want the project structured as a monorepo.

## Product

KargoFleet is a fleet execution and operations platform designed to replace disconnected spreadsheets, WhatsApp messages, and paper logs with a single system connecting daily fleet operations such as driver activity, trip dispatch, vehicle information, operational expenses, and eventually financial accounting.

For this demo, we are only building the DRIVER MOBILE APP and BACKEND.

A future ADMIN WEB DASHBOARD may be added later using TanStack Start, but DO NOT implement it now.

## Repository structure

Use this structure:

kargofleet/
├── apps/
│   ├── driver/       # Flutter mobile application
│   └── api/          # ASP.NET Core (C#) Web API
├── packages/
│   └── api-contract/ # OpenAPI specification
├── docker-compose.yml
├── README.md
└── .gitignore

Do not introduce Nx, Turborepo, or other monorepo tooling unless there is a clear technical reason.

## Driver app

Build the mobile application in Flutter.

Use clean, understandable architecture rather than excessive abstraction.

The initial screens should be:

1. Splash
2. Login
3. OTP verification
4. Driver dashboard
5. Vehicle details
6. Trips list
7. Trip details
8. Active trip
9. Map/route view
10. Expense list
11. Add expense
12. Profile/settings

The UI should take visual inspiration from the provided KargoFleet reference design:

- clean white interface
- green primary accent
- rounded cards
- generous spacing
- simple typography
- minimal visual noise
- bottom navigation where appropriate

Do not blindly reproduce the reference image. Treat it as design inspiration.

## Core user journey

The main demo flow should be:

Login
→ OTP verification
→ Driver dashboard
→ View assigned vehicle
→ View assigned trip
→ Start trip
→ View route on map
→ Add operational expense
→ End trip
→ Return to dashboard with updated trip information

This is the primary vertical slice of the application.

## Backend

Use:

- C# / ASP.NET Core (.NET 8 LTS)
- PostgreSQL
- Entity Framework Core (Npgsql provider)

Structure the API approximately as:

apps/api/src/Api/
├── Controllers/
├── Services/
├── Data/
│   ├── Entities/
│   ├── Seed/
│   └── Migrations/
├── DTOs/
├── Exceptions/
└── Program.cs

Keep controllers thin and put business logic in services.

Use environment variables for configuration.

Provide a .env.example file.

## Authentication

Implement a simple development-friendly authentication flow.

Login should accept a phone number.

OTP verification should work without an external SMS provider.

For development, use a predictable or logged OTP rather than integrating Twilio or another paid service.

Use JWT authentication for authenticated API requests.

Do not build a complicated authentication system.

## Database

Use PostgreSQL with Entity Framework Core.

Initial models should include approximately:

User
Driver
Vehicle
Trip
Expense

Relationships:

User → Driver
Driver → Vehicle
Driver → Trip
Trip → Expense
Trip → Vehicle

Keep the schema intentionally small.

Seed the database with realistic demo data so the application can be used immediately.

Example:

Driver:
- Name: Thando Mokoena
- License: C1

Vehicle:
- Toyota Quantum
- Registration: CA 123-456

Trip:
- Cape Town → Stellenbosch
- Status: scheduled

Expenses:
- Fuel
- Toll
- Parking

## API

Implement approximately:

POST /auth/login
POST /auth/verify

GET /me
GET /driver/dashboard

GET /vehicle

GET /trips
GET /trips/:id

POST /trips/:id/start
POST /trips/:id/end

GET /expenses
POST /expenses
DELETE /expenses/:id

Validate authenticated users and ensure drivers can only access their own relevant resources.

Business rules should be enforced on the backend.

For example:
- A scheduled trip can be started.
- An active trip can be ended.
- A completed trip cannot be started again.
- A driver cannot modify another driver's trip.

## API contract

Create an OpenAPI specification under:

packages/api-contract/openapi.yaml

Document the API endpoints, request bodies, responses, authentication, and data models.

Keep the OpenAPI specification synchronized with the implementation.

## Maps

The project must avoid proprietary mapping SDKs.

Use open-source mapping technology.

Use:
- OpenStreetMap data
- flutter_map or an equivalent open-source Flutter map library
- OSRM or another open routing engine for route calculation

Do not use Google Maps.

The demo only needs to display a route between an origin and destination.

Do not implement live GPS tracking.

Be mindful that public OpenStreetMap/OSRM infrastructure has usage policies and rate limits. Structure the application so the routing/tile provider can be changed later through configuration.

## Expenses

Allow the driver to:

- view expenses
- add an expense
- delete an expense

Expense fields:

category
amount
description
created_at

Categories can initially be:

Fuel
Toll
Parking
Other

Display a simple daily expense total.

Do not build full accounting functionality.

## Testing

Include a useful but reasonable testing setup.

Backend:
- service/unit tests
- API/integration tests for important endpoints

Flutter:
- widget tests for important screens
- tests for repositories/services where appropriate

Prioritize tests around:

Authentication
Trip state transitions
Expense creation
Authorization

## Docker

Use Docker Compose for PostgreSQL.

The developer should be able to run:

docker compose up -d

to start the database.

Document setup instructions in README.md.

## Code quality

Use C# nullable reference types (`<Nullable>enable</Nullable>`).

Use linting and formatting.

Prefer simple, readable code over premature abstractions.

Do not introduce unnecessary design patterns.

Do not over-engineer the repository.

Do not implement features outside the defined scope.

## Development approach

Build this incrementally.

First establish:

1. Monorepo structure
2. Flutter app
3. ASP.NET Core API
4. PostgreSQL + Entity Framework Core
5. Database schema
6. Seed data
7. Authentication
8. Driver dashboard
9. Trips
10. Map integration
11. Expenses
12. Testing
13. Documentation

At each stage, keep the application runnable.

Do not generate the entire project as one giant implementation.

Explain important architectural decisions as we go, because the purpose of this project is learning.

When there are multiple reasonable approaches, prefer the simpler approach and briefly explain the trade-off.

The final result should feel like a small but coherent real-world fleet-management product rather than a collection of disconnected tutorials.