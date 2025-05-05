CREATE DATABASE PMSmyClinic;
USE PMSmyClinic;
-- USERS
CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    Username VARCHAR(100),
    FirstName VARCHAR(100),
    LastName VARCHAR(100),
    IDNumber NVARCHAR(20), -- South African ID
    PasswordHash NVARCHAR(256),
    Role VARCHAR(20) -- 'Receptionist', 'Doctor', 'Admin'
);

-- PATIENTS
CREATE TABLE Patients (
    IDNumber NVARCHAR(20) PRIMARY KEY, -- South African ID is now the PK
    FirstName VARCHAR(100),
    LastName VARCHAR(100),
    DateOfBirth DATE,
    Gender VARCHAR(10)
);

-- STAFF DOCTORS
CREATE TABLE StaffDoctors (
    DoctorID INT PRIMARY KEY IDENTITY(1,1),
    FirstName VARCHAR(100),
    LastName VARCHAR(100),
    TypeDoc VARCHAR(50) -- e.g., Cardiologist, General Practitioner
);

-- APPOINTMENTS
CREATE TABLE Appointments (
    AppointmentID INT PRIMARY KEY IDENTITY(1,1),
    PatientIDNumber NVARCHAR(20), -- Link to Patients(IDNumber)
    DoctorID INT,                 -- Link to StaffDoctors(DoctorID)
    AppointmentDate DATETIME,
    Status VARCHAR(20), -- Scheduled, Completed, Cancelled
    FOREIGN KEY (PatientIDNumber) REFERENCES Patients(IDNumber),
    FOREIGN KEY (DoctorID) REFERENCES StaffDoctors(DoctorID)
);

-- MEDICAL NOTES
CREATE TABLE MedicalNotes (
    NoteID INT PRIMARY KEY IDENTITY(1,1),
    AppointmentID INT,
    Note TEXT,
    DateCreated DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (AppointmentID) REFERENCES Appointments(AppointmentID)
);

ALTER TABLE MedicalNotes
ADD Diagnosis VARCHAR(500), 
    Symptoms VARCHAR(500), 
    TreatmentPlan VARCHAR(500);

ALTER TABLE MedicalNotes
DROP COLUMN Note;

INSERT INTO StaffDoctors (FirstName, LastName, TypeDoc) VALUES
('John', 'Doe', 'Cardiologist'),
('Emma', 'Smith', 'General Practitioner'),
('Liam', 'Johnson', 'Pediatrician'),
('Olivia', 'Williams', 'Dermatologist'),
('Noah', 'Brown', 'Orthopedic Surgeon');

INSERT INTO Users (Username, FirstName, LastName, IDNumber, PasswordHash, Role) VALUES
('mulanga@gmail', 'Jane', 'Miller', '9001015800081', 'Mulanga@24', 'Receptionist')


INSERT INTO Users (Username, FirstName, LastName, IDNumber, PasswordHash, Role) VALUES
('rec_jane', 'Jane', 'Miller', '9001015800081', 'password123', 'Receptionist'),
('rec_bob', 'Bob', 'Anderson', '8802025800082', 'password123', 'Receptionist'),
('rec_alice', 'Alice', 'Taylor', '9103035800083', 'password123', 'Receptionist');

INSERT INTO Users (Username, FirstName, LastName, IDNumber, PasswordHash, Role) VALUES
('doc_john', 'John', 'Doe', '7704045800084', 'password123', 'Doctor'),
('doc_emma', 'Emma', 'Smith', '7505055800085', 'password123', 'Doctor');

INSERT INTO Patients (IDNumber, FirstName, LastName, DateOfBirth, Gender) VALUES
('0001014800086', 'Michael', 'Scott', '1975-03-15', 'Male'),
('0002024800087', 'Pam', 'Beesly', '1979-05-02', 'Female'),
('0003034800088', 'Jim', 'Halpert', '1978-10-01', 'Male'),
('0004044800089', 'Dwight', 'Schrute', '1975-01-20', 'Male'),
('0005054800090', 'Angela', 'Martin', '1981-06-25', 'Female');

SELECT * FROM Patients;
SELECT * FROM Users;
SELECT * FROM StaffDoctors;
select count(*) from Users where Username='rec_bob' and PasswordHash='password123' and role='Receptionist'
SELECT * FROM Patients;

INSERT INTO Users (Username, FirstName, LastName, IDNumber, PasswordHash, Role) VALUES
('Admin', 'Khailum', 'Pieterseb', '9001015800081', 'Admin123', 'Admin')

INSERT INTO Users (Username, FirstName, LastName, IDNumber, PasswordHash, Role) VALUES
('Admin1', 'Khailum', 'Pieterseb', '9001015800081', 'Admin', 'Admin')