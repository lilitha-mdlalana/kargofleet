# Auth flow: login, signup, OTP verification

Date: 2026-09-16
Status: Approved

## Context

KargoFleet driver app has a Splash screen and a scaffolded monorepo
(`apps/driver` Flutter, `apps/api` Express/TS) but no database, no
Prisma, and no auth. CLAUDE.md specifies phone + OTP login (no
password, no external SMS provider) and lists Login and OTP
verification as the first real screens after Splash. This spec adds a
Signup screen on top of that (an intentional extension beyond
CLAUDE.md, approved by the user), and brings forward the DB/Prisma
setup that CLAUDE.md's build order otherwise places before auth.

## Goals

- Postgres running via Docker Compose, Prisma schema for all five
  CLAUDE.md models (User, Driver, Vehicle, Trip, Expense), seeded with
  the demo driver/vehicle/trip from CLAUDE.md.
- `POST /auth/signup`, `POST /auth/login`, `POST /auth/verify`
  endpoints backed by the real DB, issuing a JWT on successful
  verification.
- Flutter Login, Signup, and OTP verification screens wired to those
  endpoints, in `apps/driver/lib/features/auth/`.

## Non-goals

- No real SMS provider — OTP is generated and logged to the server
  console only.
- No protected-route usage of the JWT yet (dashboard etc. come later);
  the verification middleware is scaffolded but not yet applied
  anywhere.
- No vehicle/license capture at signup — just name + phone.
- No password-based auth.

## Data layer

`docker-compose.yml` (repo root):

```yaml
services:
  postgres:
    image: postgres:16-alpine
    restart: unless-stopped
    environment:
      POSTGRES_USER: kargofleet
      POSTGRES_PASSWORD: kargofleet
      POSTGRES_DB: kargofleet
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
volumes:
  postgres_data:
```

Prisma schema (`apps/api/prisma/schema.prisma`), models per CLAUDE.md:

- `User`: id, phone (unique), name, otpCode (nullable), otpExpiresAt
  (nullable), createdAt. Relation to one `Driver`.
- `Driver`: id, userId (unique FK), license, relation to `Vehicle`
  (nullable FK, a driver may be unassigned) and many `Trip`.
- `Vehicle`: id, make/model as a single `name` field, registration.
- `Trip`: id, driverId FK, vehicleId FK, origin, destination, status
  (`scheduled`/`active`/`completed`), relation to many `Expense`.
- `Expense`: id, tripId FK, category, amount, description, createdAt.

OTP lives directly on `User` (otpCode/otpExpiresAt) rather than a
separate table — one active OTP per user at a time is enough for a
dev-only flow, and it avoids an extra join.

Seed script (`apps/api/prisma/seed.ts`) creates: driver Thando
Mokoena (license C1), vehicle Toyota Quantum (CA 123-456), one
scheduled trip Cape Town → Stellenbosch. Seed's User gets a fixed
known phone number (e.g. `+27821234567`) documented in the API
README so login is immediately usable.

## API

### `POST /auth/signup`

Request: `{ name: string, phone: string }`

- 400 if name/phone missing or phone already registered.
- Creates `User` + `Driver` (license left blank/null — filled in
  later by an admin flow that doesn't exist yet).
- Generates OTP, stores on user, logs `OTP for +27...: 123456` to
  console.
- Response `202 { message: "OTP sent" }`.

### `POST /auth/login`

Request: `{ phone: string }`

- 404 if no user with that phone.
- Generates OTP, stores on user, logs to console.
- Response `202 { message: "OTP sent" }`.

### `POST /auth/verify`

Request: `{ phone: string, code: string }`

- 404 if no user with that phone.
- 401 if code doesn't match or `otpExpiresAt` has passed.
- On success: clears otpCode/otpExpiresAt, signs a JWT
  (`{ sub: user.id }`, `JWT_SECRET` env var, 7 day expiry), returns
  `200 { token, user: { id, name, phone } }`.

OTP generation: random 6-digit string, 5 minute expiry
(`services/auth.service.ts`). Controllers stay thin — parse request,
call service, map result to HTTP response.

`middleware/auth.middleware.ts`: reads `Authorization: Bearer <jwt>`,
verifies with `JWT_SECRET`, attaches `req.userId`. Exported now,
applied to routes starting with the dashboard phase.

## Flutter

`lib/features/auth/`:

- `data/auth_api.dart` — thin wrapper over `package:http`, one method
  per endpoint (`signup`, `login`, `verify`), throws a typed
  `AuthApiException(message)` on non-2xx.
- `login_screen.dart` — phone field, submit → calls `login`, on
  success navigates to `OtpScreen(phone: ..., mode: OtpMode.login)`.
  Link to Signup screen.
- `signup_screen.dart` — name + phone fields, submit → calls
  `signup`, on success navigates to `OtpScreen(mode: OtpMode.signup)`.
  Link back to Login.
- `otp_screen.dart` — shared, 6-digit code field, submit → calls
  `verify`; on success stores the returned JWT (in-memory holder for
  now, e.g. a simple static/singleton `AuthSession` — persistent
  storage like `flutter_secure_storage` is a later concern) and shows
  a placeholder "you're in" state (no dashboard screen exists yet).

State handling: plain `StatefulWidget` + local `bool isLoading` /
`String? error`, no external state management package — consistent
with CLAUDE.md's "clean architecture rather than excessive
abstraction."

`lib/core/config/api_config.dart` — `baseUrl` constant, defaults to
`http://10.0.2.2:3000` (Android emulator loopback) overridable via
`--dart-define=API_BASE_URL=...` for iOS simulator / web / device.

## Error handling

- API: every error response is `{ message: string }` with an
  appropriate status code (400/401/404). No stack traces leaked.
- Flutter: each screen shows the error message inline below the form
  (a `Text` in the error color), not a snackbar/dialog — keeps focus
  on the field to fix.

## Testing

API (`apps/api`):
- `services/auth.service.test.ts` — OTP generation shape, expiry
  logic, verify success/failure, unit-level with a mocked Prisma
  client.
- `routes/auth.routes.test.ts` — integration test against a real test
  DB (or Prisma's in-memory-friendly approach) covering
  signup→verify and login→verify happy paths plus the 400/401/404
  cases.

Flutter (`apps/driver`):
- Widget tests for `LoginScreen` and `SignupScreen` — empty-field
  validation blocks submit, valid input triggers the API call (mocked
  `AuthApi`).
- Widget test for `OtpScreen` — 6-digit validation, success path
  navigates away.

## Open questions / deferred

- Where the JWT is persisted client-side (secure storage) is deferred
  to whenever a "stay logged in" requirement shows up.
- Admin-side driver license/vehicle assignment isn't built yet —
  signup produces a `Driver` with a null license, which is fine for
  this demo phase.
