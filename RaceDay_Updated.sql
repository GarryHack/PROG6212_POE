
CREATE TABLE Organizer (
    Organizer_id      INT IDENTITY(1,1) PRIMARY KEY,
    Organizer_Name    NVARCHAR(150) NOT NULL,
    Email             NVARCHAR(150) NOT NULL UNIQUE,
    Phone             NVARCHAR(20),
    Organization_Name NVARCHAR(150),
    Password_Hash     NVARCHAR(MAX) NOT NULL          
);

CREATE TABLE Participant (
    Participant_id INT IDENTITY(1,1) PRIMARY KEY,
    First_Name     NVARCHAR(150) NOT NULL,
    Last_Name      NVARCHAR(150) NOT NULL,
    Email          NVARCHAR(150) NOT NULL UNIQUE,
    Cell           NVARCHAR(20),
    DoB            DATE NOT NULL,
    Password_Hash  NVARCHAR(MAX) NOT NULL             
);

CREATE TABLE Event (
    event_id          INT IDENTITY(1,1) PRIMARY KEY,
    Event_Name        NVARCHAR(150) NOT NULL,
    Location          NVARCHAR(150) NOT NULL,
    Event_Date        DATE NOT NULL,
    Event_Description NVARCHAR(500),
    Event_Type        NVARCHAR(10) NOT NULL           
        CONSTRAINT CK_Event_Type CHECK (Event_Type IN ('Run','Walk','Cycle')),
    Distance          DECIMAL(6,2) NOT NULL          
        CONSTRAINT CK_Event_Distance CHECK (Distance > 0),
    Organiser_id      INT NOT NULL,
    CONSTRAINT FK_Event_Organizer FOREIGN KEY (Organizer_id) REFERENCES Organiser(Organizer_id)
);

CREATE TABLE Category (
    Category_id   INT IDENTITY(1,1) PRIMARY KEY,
    Event_id      INT NOT NULL,
    Category_Name NVARCHAR(100) NOT NULL,
    Distance      DECIMAL(6,2) NOT NULL CONSTRAINT CK_Category_Distance CHECK (Distance > 0),
    Age_Limit     INT NULL,                          
    CONSTRAINT FK_Category_Event FOREIGN KEY (Event_id) REFERENCES Event(event_id)
);

CREATE TABLE Registration (              
    Registration_id   INT IDENTITY(1,1) PRIMARY KEY,
    Participant_id    INT NOT NULL,
    Category_id       INT NOT NULL,
    Registration_date DATE NOT NULL DEFAULT GETDATE(),
    Payment_status    NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    CONSTRAINT FK_Registration_Participant FOREIGN KEY (Participant_id) REFERENCES Participant(Participant_id),
    CONSTRAINT FK_Registration_Category    FOREIGN KEY (Category_id)    REFERENCES Category(Category_id),
    CONSTRAINT UQ_Registration UNIQUE (Participant_id, Category_id)
);

CREATE TABLE Result (
    Result_id       INT IDENTITY(1,1) PRIMARY KEY,
    Registration_id INT NOT NULL UNIQUE,              
    Finish_Time     TIME NOT NULL,
    Position        INT NOT NULL CONSTRAINT CK_Result_Position CHECK (Position > 0),
    Weather_Info    NVARCHAR(200),
    CONSTRAINT FK_Result_Registration FOREIGN KEY (Registration_id) REFERENCES Registration(Registration_id)
);


INSERT INTO Organizer (Organizer_Name, Email, Phone, Organization_Name, Password_Hash)
VALUES ('Thabo Mokoena', 'thabo@comrades.org', '0821112222', 'Comrades Association', 'SEED-PLACEHOLDER'),
       ('Sarah Jacobs', 'sarah@capetowncycle.org', '0833334444', 'Cape Town Cycle Tour Trust', 'SEED-PLACEHOLDER');

INSERT INTO Participant (First_Name, Last_Name, Email, Cell, DoB, Password_Hash)
VALUES ('Lindiwe', 'Nkosi', 'lindiwe@example.com', '0845556666', '1990-05-12', 'SEED-PLACEHOLDER'),
       ('Michael', 'Dlamini', 'michael@example.com', '0827778888', '1985-11-23', 'SEED-PLACEHOLDER');

INSERT INTO Event (Event_Name, Location, Event_Date, Event_Description, Event_Type, Distance, Organiser_id)
VALUES ('Comrades Marathon', 'Durban', '2026-06-14', 'Iconic ultramarathon between Pietermaritzburg and Durban', 'Run', 90.00, 1),
       ('Cape Town Cycle Tour', 'Cape Town', '2026-03-01', 'World''s largest timed cycle race', 'Cycle', 109.00, 2),
       ('Soweto Marathon', 'Johannesburg', '2026-11-01', 'The People''s Race through Soweto', 'Run', 42.20, 1);

INSERT INTO Category (Event_id, Category_Name, Distance, Age_Limit)
VALUES (1, 'Ultra 90km', 90.00, 20),
       (2, 'Cycle 109km', 109.00, 16),
       (2, 'Fun Ride 30km', 30.00, 12),
       (3, 'Marathon 42km', 42.20, 18),
       (3, '10km Run', 10.00, 14);

INSERT INTO Registration (Participant_id, Category_id, Payment_status)
VALUES (1, 1, 'Paid'), (2, 2, 'Paid'), (1, 5, 'Pending');

INSERT INTO Result (Registration_id, Finish_Time, Position, Weather_Info)
VALUES (1, '06:45:00', 150, 'Sunny, 22°C'),
       (2, '03:25:00', 45, 'Windy, 18°C');
