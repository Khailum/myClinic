SELECT * FROM Patients;
SELECT * FROM Users;
SELECT * FROM StaffDoctors;
SELECT * FROM Appointments;
SELECT * FROM MedicalNotes;
-- Disable foreign key checks temporarily (if needed)
-- For SQL Server, you usually drop or delete in the right order instead

-- 1. Delete from child tables first
DELETE FROM MedicalNotes;
DELETE FROM Appointments;

-- 2. Then delete from parent tables
DELETE FROM StaffDoctors;
DELETE FROM Patients;
DELETE FROM Users;

INSERT INTO StaffDoctors (FirstName, LastName, TypeDoc)
VALUES 
('John', 'Doe', 'Cardiologist'),
('Emily', 'Smith', 'General Practitioner');
INSERT INTO Patients (IDNumber, FirstName, LastName, DateOfBirth, Gender)
VALUES 
('9801015009084', 'Sarah', 'Connor', '1998-01-01', 'Female'),
('8505055009085', 'James', 'Brown', '1985-05-05', 'Male');
-- Assuming DoctorID 1 and 2, and matching Patient IDs
INSERT INTO Appointments (PatientIDNumber, DoctorID, AppointmentDate, Status)
VALUES 
('9801015009084', 6, '2025-04-16 10:00:00', 'Scheduled'),
('8505055009085', 7, '2025-04-16 11:00:00', 'Scheduled');
-- Assuming AppointmentID 1 and 2
INSERT INTO MedicalNotes (AppointmentID, Diagnosis, Symptoms, TreatmentPlan)
VALUES 
(3, 'Hypertension', 'Headache, dizziness', 'Lifestyle changes, medication'),
(4, 'Flu', 'Fever, sore throat', 'Rest, fluids, paracetamol');


INSERT INTO Users (Username, FirstName, LastName, IDNumber, PasswordHash, Role)
VALUES
('rec1', 'Amy', 'Jones', '8801015009088', 'apple', 'Receptionist'),
('doc1', 'Brian', 'White', '7905055009081', 'banana', 'Doctor'),
('admin1', 'Carol', 'Brown', '9003035009089', 'peach', 'Admin');

SELECT * FROM Users WHERE Username = 'rec1';

INSERT INTO Users (Username, FirstName, LastName, IDNumber, PasswordHash, Role)
VALUES 
('admin1', 'John', 'Doe', '8001015009087', 'password1', 'Admin'),
('doc1', 'Dr. Sarah', 'Johnson', '7003055086091', 'password2', 'Doctor'),
('rec1', 'Jane', 'Smith', '9304015192089', 'password3', 'Receptionist');

INSERT INTO Patients (IDNumber, FirstName, LastName, DateOfBirth, Gender)
VALUES 
('8001015009087', 'Mary', 'Brown', '1990-02-15', 'Female'),
('7003055086091', 'Liam', 'Williams', '1985-07-22', 'Male'),
('9304015192089', 'Emma', 'Taylor', '2000-11-30', 'Female');

INSERT INTO StaffDoctors (FirstName, LastName, TypeDoc)
VALUES
('Dr. Alice', 'Cooper', 'Cardiologist'),
('Dr. Bob', 'White', 'General Practitioner'),
('Dr. Sarah', 'Johnson', 'Neurologist');

INSERT INTO Appointments (PatientIDNumber, DoctorID, AppointmentDate, Status)
VALUES
('8001015009087', 15, '2025-04-15 09:00:00', 'Scheduled'),
('7003055086091', 16, '2025-04-15 10:30:00', 'Scheduled'),
('9304015192089', 16, '2025-04-15 13:00:00', 'Completed');

INSERT INTO MedicalNotes (AppointmentID, Diagnosis, Symptoms, TreatmentPlan, DateCreated)
VALUES
(10, 'Hypertension', 'Headaches, Dizziness', 'Lifestyle changes, Medication', GETDATE()),
(11, 'Flu', 'Cough, Fever, Sore Throat', '7 day antibiotics', GETDATE()),
(9, 'Migraine', 'Severe headache, Nausea', 'Painkillers, Rest', GETDATE());

