myClinic 🏥
.NET Platform Database Status

myClinic is a C# console-based Clinic Management System designed for healthcare practices. It allows managing users, patients, appointments, and medical records efficiently.

✨ Features:
🔐 Secure login system with role-based access (Admin, Doctor, Receptionist)
👨‍⚕️ Manage Patient Profiles (CRUD operations)
📅 Appointment scheduling with doctors
📝 Medical Notes and Treatment Plan recording
🛠️ Admin Management for staff users
📈 View Today's Appointments

🛠️ Setup Instructions:
Requirements
.NET 9.0 SDK
SQL Server
Visual Studio 2022+ (optional but recommended)

Database Setup:
Run SQL.sql or SQL Schema.txt scripts in SQL Server to create required tables.
Ensure database connection string is properly set in the application.
string connectionString = "Server=your_server;Database=your_database;Trusted_Connection=True;";

Running the Application:
Open myClinic.sln in Visual Studio.
Build and run the solution (Ctrl + F5).
Login with appropriate user credentials.

🏛️ Project Structure:
File => Purpose
Program.cs - Main entry point of the application
Login.cs - User authentication and session management
Admin.cs - Admin-specific features like creating and deleting users
Doctor.cs - Doctor dashboard for viewing appointments and medical notes
Receptionist.cs - Receptionist tasks like booking appointments and creating patients
CreateProfile.cs - Patient profile creation
Password.cs	Password hashing and verification
LoadingScreen.cs, Logo.cs	Visual enhancements and branding

🗂️ Database Schema
Users: UserID, Username, FirstName, LastName, IDNumber, PasswordHash, Role
Patients: IDNumber, FirstName, LastName, DateOfBirth, Gender
StaffDoctors: DoctorID, FirstName, LastName, TypeDoc
Appointments: AppointmentID, PatientIDNumber, DoctorID, AppointmentDate, Status
MedicalNotes: NoteID, AppointmentID, Diagnosis, Symptoms, TreatmentPlan, DateCreated

👤 User | Roles
Role |	Actions
Admin |	Manage users and view appointments
Receptionist | Create patients, book appointments
Doctor | View and manage appointments, record medical notes

🚀 Future Enhancements:
🔒 Password recovery and reset functionality
🗓️ Advanced appointment slot management
🔍 Improved patient search and filtering
🖥️ GUI version (WPF or WinForms)
📊 Reporting and analytics features
📜 License
This project is for educational and development purposes. Contact the project maintainers for production use rights.
