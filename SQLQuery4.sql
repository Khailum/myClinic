SELECT * FROM Users;
SELECT * FROM Patients;
SELECT * FROM StaffDoctors;
SELECT * FROM Appointments;
SELECT * FROM MedicalNotes;
-- Step 1: Rename the column from PasswordHash to Password
EXEC sp_rename 'Users.PasswordHash', 'Password', 'COLUMN';

-- Step 2: Ensure the column has the correct data type (VARCHAR or NVARCHAR depending on your needs)
-- Here we assume the password will be stored as NVARCHAR(100). Adjust the length as needed.
ALTER TABLE Users
ALTER COLUMN Password NVARCHAR(100);  -- Change this if needed based on the length of your passwords

-- Step 3: Optional - if you have constraints or indexes on PasswordHash, update or drop them as needed
-- Example of removing an index if it was on PasswordHash
-- DROP INDEX IF EXISTS idx_PasswordHash ON Users;

ALTER TABLE StaffDoctors
ADD IDNumber VARCHAR(50); -

SELECT * FROM Users WHERE UserID = 1;
SELECT sd.DoctorID 
FROM Users u
INNER JOIN StaffDoctors sd ON u.DoctorID = sd.DoctorID
WHERE u.UserID = 4 AND u.Role = 'Doctor';
INSERT INTO Users (Username, FirstName, LastName, IDNumber, PasswordHash, Role, DoctorID)
VALUES ('doc1', 'John', 'Smith', '9001015009087', 'pass123', 'Doctor', 4);

-- Add the DoctorID column to the Users table
ALTER TABLE Users
ADD DoctorID INT NULL;

-- Create a foreign key constraint to link the Users table to the StaffDoctors table
ALTER TABLE Users
ADD CONSTRAINT FK_User_Doctor FOREIGN KEY (DoctorID)
REFERENCES StaffDoctors(DoctorID);

-- Example of updating the DoctorID in the Users table based on a user's username or role
UPDATE Users
SET DoctorID = (SELECT DoctorID FROM StaffDoctors WHERE FirstName = 'David' AND LastName = 'Jones')
WHERE Username = 'docjones';

UPDATE Appointments
SET DoctorID = 4
WHERE DoctorID = 5;

-- Update the PatientID to 9901011234081 for AppointmentID 8 and 9
UPDATE Appointments
SET PatientIDNumber = '9901011234081'
WHERE AppointmentID IN (8, 9);

-- Insert Medical Notes for Past Appointments
DECLARE @AppointmentID INT;
-- Insert notes for the past appointments
SET @AppointmentID = (SELECT AppointmentID FROM Appointments WHERE AppointmentDate = '2025-02-10 10:00:00' AND PatientIDNumber = 9901011234081);
INSERT INTO MedicalNotes (AppointmentID, Note)
VALUES 
(@AppointmentID, 'Patient reports mild chest pain. Prescribed a few tests for further investigation.');

SET @AppointmentID = (SELECT AppointmentID FROM Appointments WHERE AppointmentDate = '2025-01-15 11:00:00' AND PatientIDNumber = 9901011234081);
INSERT INTO MedicalNotes (AppointmentID, Note)
VALUES 
(@AppointmentID, 'Follow-up after heart surgery. No complications. Recommend lifestyle changes.');

SET @AppointmentID = (SELECT AppointmentID FROM Appointments WHERE AppointmentDate = '2024-12-20 13:30:00' AND PatientIDNumber = 9901011234081);
INSERT INTO MedicalNotes (AppointmentID, Note)
VALUES 
(@AppointmentID, 'Patient complained of irregular heartbeat. ECG scheduled for next visit.');

SET @AppointmentID = (SELECT AppointmentID FROM Appointments WHERE AppointmentDate = '2024-11-25 08:00:00' AND PatientIDNumber = 9901011234081);
INSERT INTO MedicalNotes (AppointmentID, Note)
VALUES 
(@AppointmentID, 'Patient requested a routine checkup. Blood pressure normal, no concerns.');

SET @AppointmentID = (SELECT AppointmentID FROM Appointments WHERE AppointmentDate = '2024-10-30 15:00:00' AND PatientIDNumber = 9901011234081);
INSERT INTO MedicalNotes (AppointmentID, Note)
VALUES 
(@AppointmentID, 'Patient reports anxiety-related chest discomfort. Prescribed anti-anxiety medication.');

-- Insert Medical Notes for Future Appointments (for demonstration purposes, only adding 2 future notes)
SET @AppointmentID = (SELECT AppointmentID FROM Appointments WHERE AppointmentDate = '2025-04-20 10:00:00' AND PatientIDNumber = 9901011234081);
INSERT INTO MedicalNotes (AppointmentID, Note)
VALUES 
(@AppointmentID, 'Routine checkup. Patient due for annual blood tests.');

SET @AppointmentID = (SELECT AppointmentID FROM Appointments WHERE AppointmentDate = '2025-05-05 14:00:00' AND PatientIDNumber = 9901011234081);
INSERT INTO MedicalNotes (AppointmentID, Note)
VALUES 
(@AppointmentID, 'Follow-up on heart disease. Monitor cholesterol levels.');

-- Insert Future Appointments (e.g., 3 upcoming appointments)
INSERT INTO Appointments (PatientIDNumber, DoctorID, AppointmentDate, Status)
VALUES 
(9901011234081, 4, '2025-04-20 10:00:00', 'Scheduled'),
(9901011234081, 4, '2025-05-05 14:00:00', 'Scheduled'),
(9901011234081, 4, '2025-06-15 09:30:00', 'Scheduled');

-- Insert Past Appointments (e.g., 5 past appointments)
INSERT INTO Appointments (PatientIDNumber, DoctorID, AppointmentDate, Status)
VALUES 
(9901011234081, 4, '2025-02-10 10:00:00', 'Completed'),
(9901011234081, 4, '2025-01-15 11:00:00', 'Completed'),
(9901011234081, 4, '2024-12-20 13:30:00', 'Completed'),
(9901011234081, 4, '2024-11-25 08:00:00', 'Completed'),
(9901011234081, 4, '2024-10-30 15:00:00', 'Completed');

INSERT INTO MedicalNotes (AppointmentID, Diagnosis, Symptoms, TreatmentPlan, DateCreated)
VALUES
(10, 'Preventive care consultation', 'No new symptoms', 'Follow-up on health and lifestyle habits, monitor overall health', '2025-04-20');

-- Insert Medical Notes for AppointmentID 11
INSERT INTO MedicalNotes (AppointmentID, Diagnosis, Symptoms, TreatmentPlan, DateCreated)
VALUES
(11, 'Routine check-up', 'No issues found', 'Continue health monitoring and lifestyle review', '2025-05-05');

-- Insert Medical Notes for AppointmentID 12
INSERT INTO MedicalNotes (AppointmentID, Diagnosis, Symptoms, TreatmentPlan, DateCreated)
VALUES
(12, 'General health check', 'No major symptoms', 'Routine health screening and preventive care', '2025-06-15');

-- Insert Medical Notes for AppointmentID 13
INSERT INTO MedicalNotes (AppointmentID, Diagnosis, Symptoms, TreatmentPlan, DateCreated)
VALUES
(13, 'Routine check-up', 'No symptoms, patient in good health', 'Continue regular monitoring', '2025-02-10');

-- Insert Medical Notes for AppointmentID 14
INSERT INTO MedicalNotes (AppointmentID, Diagnosis, Symptoms, TreatmentPlan, DateCreated)
VALUES
(14, 'Annual health review', 'No significant symptoms', 'Routine health maintenance and monitoring', '2025-01-15');

-- Insert Medical Notes for AppointmentID 15
INSERT INTO MedicalNotes (AppointmentID, Diagnosis, Symptoms, TreatmentPlan, DateCreated)
VALUES
(15, 'Follow-up visit completed', 'No new concerns raised', 'Routine health monitoring and follow-up after 6 months', '2024-12-20');

-- Insert Medical Notes for AppointmentID 16
INSERT INTO MedicalNotes (AppointmentID, Diagnosis, Symptoms, TreatmentPlan, DateCreated)
VALUES
(16, 'Post-treatment check-up', 'No significant symptoms', 'Monitor for any side effects and follow-up treatment if necessary', '2024-11-25');

-- Insert Medical Notes for AppointmentID 17
INSERT INTO MedicalNotes (AppointmentID, Diagnosis, Symptoms, TreatmentPlan, DateCreated)
VALUES
(17, 'Preventive care check', 'No significant findings', 'Continue with regular health screenings', '2024-10-30');