This document explains the Entity-Relationship Diagram (ERD) for the RaceDay event management system. The ERD defines the database structure and shows how different entities interact to support the functionality outlined in the API endpoint plan.

By mapping out entities, attributes, and relationships, the ERD ensures that the backend database can store and retrieve all necessary information for events, participants, organisers, registrations, results, and categories.

Entities and Attributes
1. Event
PK: EventID

Attributes: EventName, Location, Date, Description

Foreign Keys: OrganizerID, CategoryID, ResultID

Purpose: Stores details of each race or cycling event.

2. Organizers
PK: OrganizerID

Attributes: OrganizerName, Email, Phone, OrganizationName

Foreign Key: EventID

Purpose: Represents event organisers who create and manage events.

3. Category
PK: CategoryID

Attributes: CategoryName, Distance, AgeLimit

Foreign Keys: EventID, RegistrationID

Purpose: Defines race categories (e.g., 5km, 10km, marathon).

4. Participant
PK: ParticipantID

Attributes: FirstName, LastName, Email, Phone, Date_of_Birth

Foreign Key: RegistrationID

Purpose: Represents individuals who register for events.

5. Registration
PK: RegistrationID

Attributes: Payment_Issues

Foreign Keys: ParticipantID, CategoryID

Purpose: Links participants to categories, recording enrolments.

6. Results
PK: ResultID

Attributes: Position, FinishTime, AgeLimit, WeatherInfo

Foreign Keys: EventID, ParticipantID, OrganizerID

Purpose: Stores race outcomes, including finish times and weather conditions.

Relationships
Event → Organizers: An event is managed by organisers.

Event → Category: Each event has multiple categories.

Participant → Registration: Participants register for categories.

Category → Registration: Categories hold multiple registrations.

Registration → Results: Registrations produce race results.

Organizers → Results: Organisers capture and record results.

Participant → Results: Participants track their personal results.

RaceDay API Endpoint Plan 
The endpoints cover all required functionality: Authentication, User Profiles, Events, Categories, Enrolments, Results, and Weather Integration. Each endpoint is described with its HTTP method, route, purpose, role requirements, request body, and expected response.

File Contents
1. Authentication
Endpoints for user registration and login:

POST /api/auth/register → Registers a new participant or organiser. Returns 201 Created with user object and JWT.

POST /api/auth/login → Authenticates user. Returns 200 OK with JWT and profile details.

2. User Profile
Endpoints for managing user information:

GET /api/users/me → Retrieves logged-in user profile.

PUT /api/users/me → Updates logged-in user profile.

Both return 200 OK with profile data.

3. Events
Endpoints for event management:

GET /api/events → Lists all upcoming events.

POST /api/events → Creates a new event (201 Created).

GET /api/events/:id → Retrieves details of a specific event.

PUT /api/events/:id → Updates event details.

DELETE /api/events/:id → Deletes an event (204 No Content).

4. Categories
Endpoints for event categories:

GET /api/events/:id/categories → Lists categories for an event.

POST /api/events/:id/categories → Adds a category (201 Created).

PUT /api/categories/:id → Updates category details.

DELETE /api/categories/:id → Deletes a category (204 No Content).

5. Enrolments
Endpoints for participant registrations:

POST /api/categories/:id/enrolments → Registers participant for a category (201 Created).

GET /api/users/me/enrolments → Shows participant’s enrolments.

GET /api/categories/:id/enrolments → Shows all enrolments for a category (organiser view).

6. Results
Endpoints for recording and viewing race results:

POST /api/enrolments/:id/results → Records a participant’s result (201 Created).

GET /api/users/me/results → Shows participant’s personal race history.

GET /api/events/:id/results → Displays event leaderboard results.

7. Weather Integration
Endpoints for weather data:

GET /api/events/:id/weather → Fetches live weather forecast for event location/date.

GET /api/results/:id/weather → Retrieves stored race-day weather snapshot.

Purpose
Ensures consistency between planning and implementation.

Provides a blueprint for developers before coding begins.

Demonstrates understanding of RESTful API design and role-based access control.


SQL SCRIPT
Supports RaceDay’s goal of delivering a modern, cloud-ready platform for South Africa’s road events culture.Structure of the Script
1. Table Creation
The script creates six main tables:

Event → Stores event details.

Organizers → Stores organiser information.

Category → Defines race categories linked to events.

Participant → Stores participant details.

Registration → Links participants to categories (enrolments).

Results → Stores race outcomes, including finish times and weather info.

2. Primary Keys
Each table has a PK (e.g., EventID, OrganizerID, ParticipantID) to uniquely identify records.

3. Foreign Keys
Relationships are enforced with FK constraints:

Event ↔ Organizers

Event ↔ Category

Participant ↔ Registration

Category ↔ Registration

Registration ↔ Results

Results ↔ Event, Participant, Organizer

4. Constraints
NOT NULL constraints ensure required fields are always filled.
 Authentication → Uses the Participant and Organizer tables.

User Profile → Reads/writes Participant and Organizer data.

Events → CRUD operations on the Event table.

Categories → CRUD operations on the Category table.

Enrolments → Managed via the Registration table.

Results → Stored in the Results table, linked to participants and events.

Weather Integration → WeatherInfo attribute in Results supports historical race-day conditions.

YOUTUBE LINKS:
ERD:  https://youtu.be/fEcMnPNDjGo
ENDPOINT PLAN: https://youtu.be/kN4TUphUJdo
SQL SCRIPT: https://youtu.be/QeZvEDHchAs
