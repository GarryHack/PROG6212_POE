# RaceDay API – Part 2


## Prerequisites
- Visual Studio 2022 with the "ASP.NET and web development" workload
- .NET 8 SDK
- SQL Server LocalDB (installed with Visual Studio) or any SQL Server

## Setup and run
Swagger opens at `https://localhost:<port>/swagger`.
Run tests: **Test → Run All Tests** (or `dotnet test`).

## Roles
| Role | Can do |
|---|---|
| **Organizer** | Create/update/delete OWN events and categories; view enrolments for OWN categories; record results for OWN events; view/update own profile |
| **Participant** | View events/categories/leaderboards; enrol in a category; view own enrolments and results; view/update own profile |

Organisers register with a secret `organiser_code` (set in `appsettings.json` → `OrganiserSignupCode`) so nobody can simply pick the Organiser role.

## Authentication
Cookie-based session. `POST /api/auth/login` creates a session cookie holding the user id and role. Passwords are hashed (ASP.NET Core `PasswordHasher`), never stored in plain text.



## Changes from the Part 1 plan (and why)
- Added `Event_Type` and `Distance` columns to Event (required by the Part 2 brief).
- Added `Password_Hash` to Organiser and Participant (needed for login).
- Session cookie instead of JWT (Part 2 brief asks for session management).
- Organiser registration requires a secret code.
