# Part 3 – RaceDay.Web (ASP.NET Core MVC) + Azure Blob + Docker

`RaceDay.Web` is the web front end. It has **no database access and no business logic**: every read and write is an HTTP call to the Part 2 API (`RaceDay.Web/Services/RaceDayApi.cs`). The API's login cookie is stored in the user's server-side **session** and re-sent on each call, so the user's identity and role follow them across every page.

## Run it locally (two terminals)
1. **API** (from the repo root):
   ```
   dotnet run --project RaceDay.Api --launch-profile http --urls http://0.0.0.0:5080
   ```
   First time only: run `Update-Database` (see Part 2 steps) after adding the Part 3 migration:
   `Add-Migration Part3_Images` then `Update-Database` in the Package Manager Console (default project `RaceDay.Api`).
2. **Web app**:
   ```
   dotnet run --project RaceDay.Web
   ```
   Open http://localhost:5090. The API address comes from `ApiBaseUrl` in `RaceDay.Web/appsettings.json` (default `http://localhost:5080`).


## Run in Docker (single command)
Start the API first (step 1 above), then from the repo root:
```
docker build -f RaceDay.Web/Dockerfile -t raceday-web . && docker run --rm -p 8080:8080 --add-host=host.docker.internal:host-gateway -e ApiBaseUrl=http://host.docker.internal:5080 raceday-web
```
Open http://localhost:8080. It behaves the same as `dotnet run`; only the API address is injected through `ApiBaseUrl`.



## Pages
- **Organiser:** Dashboard, My events (create / edit / delete + banner), Categories, Enrolments, Results entry.
- **Participant:** Events (filter by text, type, dates), Event details (categories, forecast, enter), My enrolments, My results (e.g. "47th out of 312 finishers"), Profile + picture.
- Unauthenticated users are redirected to Log in; the wrong role is sent to an "access denied" page.


