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
    Diagnosis VARCHAR(500),
    Symptoms VARCHAR(500),
    TreatmentPlan VARCHAR(500),
    DateCreated DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (AppointmentID) REFERENCES Appointments(AppointmentID)
);
