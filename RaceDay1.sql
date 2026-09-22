Create Schema RaceDay;
GO
--Organizer Table
CREATE TABLE Organiser (
    Organiser_id INT IDENTITY(1,1) PRIMARY KEY,
    Organizer_Name NVARCHAR(150) NOT NULL,
    Email NVARCHAR(150) NOT NULL UNIQUE,
    Phone NVARCHAR(20),
    Organisation_Name NVARCHAR(150)
);

--Participant Table
CREATE TABLE Participant (
    Participant_id INT IDENTITY(1,1) PRIMARY KEY,
    First_Name NVARCHAR(150) NOT NULL,
    Last_Name NVARCHAR(150) NOT NULL,
    Email NVARCHAR(150) NOT NULL UNIQUE,
    Cell NVARCHAR(20),
    DoB DATE NOT NULL
);

--Event Table
CREATE TABLE Event (
    event_id INT IDENTITY(1,1) PRIMARY KEY,
    Event_Name NVARCHAR(150) NOT NULL,
    Location NVARCHAR(150) NOT NULL,
    Event_Date DATE NOT NULL,
    Event_Description NVARCHAR(500),
    Organiser_id INT NOT NULL,
    CONSTRAINT FK_Event_Organiser FOREIGN KEY (Organiser_id) REFERENCES Organiser(Organiser_id)
);
--Category Table
CREATE TABLE Category (
    Category_id INT IDENTITY(1,1) PRIMARY KEY,
    Event_id INT NOT NULL,
    Category_Name NVARCHAR(100) NOT NULL,
    Distance DECIMAL(6,2) NOT NULL CHECK (distance > 0),
    Age_Limit INT NULL,
    CONSTRAINT FK_Category_Event FOREIGN KEY (event_id) REFERENCES Event(event_id)
);

--Registration Table
CREATE TABLE Registration (
    Registration_id INT IDENTITY(1,1) PRIMARY KEY,
    Participant_id INT NOT NULL,
    Category_id INT NOT NULL,
    Registration_date DATE NOT NULL DEFAULT GETDATE(),
    Payment_status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    CONSTRAINT FK_Registration_Participant FOREIGN KEY (participant_id) REFERENCES Participant(participant_id),
    CONSTRAINT FK_Registration_Category FOREIGN KEY (category_id) REFERENCES Category(category_id),
    CONSTRAINT UQ_Registration UNIQUE (participant_id, category_id) 
);

--Result Table
CREATE TABLE Result (
    Result_id INT IDENTITY(1,1) PRIMARY KEY,
    Registration_id INT NOT NULL UNIQUE,
    Finish_Time TIME NOT NULL,
    Position INT NOT NULL CHECK (position > 0),
    Weather_Info NVARCHAR(200),
    CONSTRAINT FK_Result_Registration FOREIGN KEY (Registration_id) REFERENCES Registration(Registration_id)
);



-- Organisers data
INSERT INTO Organiser (Organizer_Name, email, phone, organisation_name)
VALUES ('Thabo Mokoena', 'thabo@comrades.org', '0821112222', 'Comrades Association'),
       ('Sarah Jacobs', 'sarah@capetowncycle.org', '0833334444', 'Cape Town Cycle Tour Trust');

-- Participants data
INSERT INTO Participant (first_name, last_name, email, cell, dob)
VALUES ('Lindiwe', 'Nkosi', 'lindiwe@example.com', '0845556666', '1990-05-12'),
       ('Michael', 'Dlamini', 'michael@example.com', '0827778888', '1985-11-23');

-- Events data
INSERT INTO Event (Event_Name, location, Event_Date, Event_Description, organiser_id)
VALUES ('Comrades Marathon', 'Durban', '2026-06-14', 'Iconic ultramarathon between Pietermaritzburg and Durban', 1),
       ('Cape Town Cycle Tour', 'Cape Town', '2026-03-01', 'World’s largest timed cycle race', 2),
       ('Soweto Marathon', 'Johannesburg', '2026-11-01', 'The People’s Race through Soweto', 1);

-- Categories data
INSERT INTO Category (Event_id, Category_Name, Distance, Age_Limit)
VALUES (1, 'Ultra 90km', 90.00, 20),
       (2, 'Cycle 109km', 109.00, 16),
       (2, 'Fun Ride 30km', 30.00, 12),
       (3, 'Marathon 42km', 42.20, 18),
       (3, '10km Run', 10.00, 14);

-- Registrations data
INSERT INTO Registration (Participant_id, Category_id,Payment_status)
VALUES (1, 1, 'Paid'),   -- Lindiwe in Comrades Ultra
       (2, 2, 'Paid'),   -- Michael in Cycle Tour 109km
       (1, 5, 'Pending');-- Lindiwe in Soweto 10km

-- Results data
INSERT INTO Result (Registration_id, Finish_Time, Position, Weather_Info)
VALUES (1, '06:45:00', 150, 'Sunny, 22°C'),
       (2, '03:25:00', 45, 'Windy, 18°C');

       Select * from Organiser;
       Select * from Participant;
       Select * from Event;
       Select * from Category;