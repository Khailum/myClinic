using System;
using Microsoft.Data.SqlClient;
using Spectre.Console;

namespace myClinic
{
    public class Doctor
    {
        private static string _doctorFullName;
        private static int _doctorId;
        private static string _connectionString;
        public static bool Initialize(string connectionString, int userId)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString), "Connection string cannot be null or empty.");

            if (userId <= 0)
                throw new ArgumentException("Invalid UserID. It must be greater than zero.");

            _connectionString = connectionString;

            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                Console.WriteLine($"Initializing Doctor for UserID: {userId}");

                // Fetch the DoctorID associated with this user
                _doctorId = GetDoctorIdFromUser(userId, connection);

                if (_doctorId <= 0)
                {
                    Console.WriteLine($"No doctor found linked to UserID: {userId}. Ensure the user is a valid doctor.");
                    return false;
                }

                // Fetch the Doctor's full name
                _doctorFullName = GetDoctorFullName(_doctorId, connection);

                if (string.IsNullOrEmpty(_doctorFullName))
                {
                    Console.WriteLine($"Doctor with ID {_doctorId} does not have a full name. Please check the database.");
                    return false;
                }

                Console.WriteLine($"Doctor found with ID: {_doctorId}");
                Console.WriteLine($"Doctor Name: {_doctorFullName}");

                return true;
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"SQL Error: {sqlEx.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected Error: {ex.Message}");
                return false;
            }
        }

        // Helper method to fetch doctor ID from the user
        private static int GetDoctorIdFromUser(int userId, SqlConnection connection)
        {
            string query = @"
            SELECT sd.DoctorID 
            FROM Users u
            INNER JOIN StaffDoctors sd ON u.DoctorID = sd.DoctorID
            WHERE u.UserID = @UserID AND u.Role = 'Doctor';";

            using var cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@UserID", userId);

            var result = cmd.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        // Helper method to fetch full name of the doctor
        private static string GetDoctorFullName(int doctorId, SqlConnection connection)
        {
            string query = "SELECT FirstName, LastName FROM StaffDoctors WHERE DoctorID = @DoctorID";

            using var cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@DoctorID", doctorId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                string firstName = reader["FirstName"].ToString();
                string lastName = reader["LastName"].ToString();
                return $"{firstName} {lastName}";
            }

            return null; // Return null if no record is found
        }

        public static void ShowDoctorMenu()
        {
            if (string.IsNullOrEmpty(_connectionString))

            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: Connection string not initialized.");
                Console.ResetColor();   
                return;
            }

            if (_doctorId <= 0 || string.IsNullOrEmpty(_doctorFullName))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: Doctor is not properly initialized.");
                Console.ResetColor();
                return;
            }

            bool keepRunning = true;

            while (keepRunning)
            {
                Console.Clear();
                Logo.Display();
                Console.WriteLine();
                AnsiConsole.Write(new Rule($"[Blue]=== Welcome Dr. {_doctorFullName}! ===[/]").RuleStyle("blue").Centered());

                Console.WriteLine("1. View upcoming appointments");
                Console.WriteLine("2. Medical Notes");
                Console.WriteLine("3. View Patient History");
                Console.WriteLine("4. Exit");

                Console.Write("Select an option (1-4): ");
                if (!int.TryParse(Console.ReadLine(), out int option))
                {
                    Console.WriteLine("Invalid input. Please enter a number between 1 and 4.");
                    WaitForInput();
                    continue;
                }

                switch (option)
                {
                    case 1:
                        ViewUpcomingAppointments();
                        break;
                    case 2:
                        ShowMedicalNoteMenu();
                        break;
                    case 3:
                        ViewPatientHistory();
                        break;
                    case 4:
                        Console.WriteLine("Exiting Doctor Menu...");
                        keepRunning = false;
                        break;
                    default:
                        Console.WriteLine("Invalid option. Please select a number between 1 and 4.");
                        break;
                }

                if (keepRunning)
                    WaitForInput();
            }
        }

        private static void ShowMedicalNoteMenu()
        {
            Console.Clear();
            Console.WriteLine("\n--- Medical Notes ---");
            Console.WriteLine("1. Add Medical Note");
            Console.WriteLine("2. View Most Recent Medical Note");
            Console.Write("Enter your choice (1 or 2): ");
            string choice = Console.ReadLine()?.Trim();

            switch (choice)
            {
                case "1":
                    AddMedicalNote();
                    break;
                case "2":
                    ViewMostRecentMedicalNote();
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid choice. Please enter 1 or 2.");
                    Console.ResetColor();
                    break;
            }
        }

        private static void ViewUpcomingAppointments()
        {
            if (!EnsureInitialized()) return;

            string query = @"
        SELECT a.AppointmentID, p.FirstName, p.LastName, a.AppointmentDate, a.Status
        FROM Appointments a
        INNER JOIN Patients p ON a.PatientIDNumber = p.IDNumber
        WHERE a.DoctorID = @DoctorID
        AND CAST(a.AppointmentDate AS DATE) >= CAST(GETDATE() AS DATE)
        ORDER BY a.AppointmentDate;";

            try
            {
                using var connection = CreateConnection();
                connection.Open();
                using var cmd = CreateCommand(query, connection);
                AddParameter(cmd, "@DoctorID", _doctorId);

                using var reader = cmd.ExecuteReader();
                Console.Clear();
                if (reader.HasRows)
                {
                    Console.WriteLine("=== Upcoming Appointments (From Today) ===");
                    // Adjust column widths for proper alignment
                    Console.WriteLine("| {0,-4} | {1,-15} | {2,-10} | {3,-5} | {4,-10} |",
                                      "ID", "Patient Name", "Date", "Time", "Status");

                    while (reader.Read())
                    {
                        var appointmentDate = reader.GetDateTime(3);
                        Console.WriteLine(
                            "| {0,-4} | {1,-15} {2,-15} | {3,-10:yyyy-MM-dd} | {4,-5:HH:mm} | {5,-10} |",
                            reader.GetInt32(0),
                            reader.GetString(1),
                            reader.GetString(2),
                            appointmentDate,
                            appointmentDate,
                            reader.GetString(4));
                    }
                }
                else
                {
                    Console.ForegroundColor= ConsoleColor.Red;  
                    Console.WriteLine("No upcoming appointments found.");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor
                    = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();   
            }
        }

        private static void AddMedicalNote()
        {
            if (!EnsureInitialized()) return;

            try
            {
                using var connection = CreateConnection();
                connection.Open();

                Console.WriteLine("New Medical Note:");    
                Console.Write("Enter Appointment ID: ");
                int appointmentId = int.Parse(Console.ReadLine());

                Console.Write("Enter Diagnosis: ");
                string diagnosis = Console.ReadLine();

                Console.Write("Enter Symptoms: ");
                string symptoms = Console.ReadLine();

                Console.Write("Enter Treatment Plan: ");
                string treatmentPlan = Console.ReadLine();

                string query = @"
                    INSERT INTO MedicalNotes (AppointmentID, Diagnosis, Symptoms, TreatmentPlan)
                    VALUES (@AppointmentID, @Diagnosis, @Symptoms, @TreatmentPlan);";

                using var cmd = CreateCommand(query, connection);
                AddParameter(cmd, "@AppointmentID", appointmentId);
                AddParameter(cmd, "@Diagnosis", diagnosis);
                AddParameter(cmd, "@Symptoms", symptoms);
                AddParameter(cmd, "@TreatmentPlan", treatmentPlan);
                cmd.ExecuteNonQuery();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Medical note added successfully.");
                Console.ResetColor ();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor ();
            }
        }

        private static void ViewMostRecentMedicalNote()
        {
            if (!EnsureInitialized()) return;

            string query = @"
                SELECT TOP 1 
                    a.AppointmentID, p.FirstName, p.LastName,
                    n.Diagnosis, n.Symptoms, n.TreatmentPlan, n.DateCreated
                FROM MedicalNotes n
                INNER JOIN Appointments a ON n.AppointmentID = a.AppointmentID
                INNER JOIN Patients p ON a.PatientIDNumber = p.IDNumber
                WHERE a.DoctorID = @DoctorID
                ORDER BY n.DateCreated DESC;";

            try
            {
                using var connection = CreateConnection();
                connection.Open();
                using var cmd = CreateCommand(query, connection);
                AddParameter(cmd, "@DoctorID", _doctorId);

                using var reader = cmd.ExecuteReader();
                Console.Clear();
                if (reader.Read())
                {
                    Console.WriteLine("=== Most Recent Medical Note ===");
                    Console.WriteLine($"Appointment ID: {reader.GetInt32(0)}");
                    Console.WriteLine($"Patient: {reader.GetString(1)} {reader.GetString(2)}");
                    Console.WriteLine($"Diagnosis: {reader.GetString(3)}");
                    Console.WriteLine($"Symptoms: {reader.GetString(4)}");
                    Console.WriteLine($"Treatment Plan: {reader.GetString(5)}");
                    Console.WriteLine($"Date Created: {reader.GetDateTime(6)}");
                }
                else
                {
                    Console.ForegroundColor= ConsoleColor.Red;
                    Console.WriteLine("No medical notes found.");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private static void ViewPatientHistory()
        {
            if (!EnsureInitialized()) return;

            Console.Write("Enter Patient ID Number: ");
            string patientId = Console.ReadLine();
            Console.WriteLine($"Searching history for Patient ID: {patientId}");

            string query = @"
        SELECT 
            a.AppointmentID, a.AppointmentDate, a.Status,
            m.Diagnosis, m.Symptoms, m.TreatmentPlan
        FROM Appointments a
        INNER JOIN MedicalNotes m ON a.AppointmentID = m.AppointmentID
        WHERE a.PatientIDNumber = @PatientID
        ORDER BY a.AppointmentDate DESC;";

            try
            {
                using var connection = CreateConnection();
                connection.Open();
                using var cmd = CreateCommand(query, connection);
                AddParameter(cmd, "@PatientID", patientId);

                using var reader = cmd.ExecuteReader();
                Console.Clear();
                if (reader.HasRows)
                {
                    Console.WriteLine("=== Patient History ===");
                    // Adjust column widths for proper alignment
                    Console.WriteLine("| {0,-12} | {1,-15} | {2,-10} | {3,-20} | {4,-30} | {5,-30} |",
                                      "Appointment ID", "Date", "Status", "Diagnosis", "Symptoms", "Treatment Plan");

                    while (reader.Read())
                    {
                        var appointmentDate = reader.GetDateTime(1);
                        Console.WriteLine(
                            "| {0,-12} | {1,-15:yyyy-MM-dd} | {2,-10} | {3,-20} | {4,-30} | {5,-30} |",
                            reader.GetInt32(0),
                            appointmentDate,
                            reader.GetString(2),
                            reader.GetString(3),
                            reader.GetString(4),
                            reader.GetString(5));
                    }
                }
                else
                {
                    Console.WriteLine("No patient history found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private static void WaitForInput()
        {
            Console.WriteLine("\n\n=========================================");
            Console.WriteLine(" Press any key to return to the main menu ");
            Console.WriteLine("=========================================\n");
            Console.ReadKey();

        }

        private static bool EnsureInitialized()
        {
            if (string.IsNullOrEmpty(_connectionString) || _doctorId == 0)
            {
                Console.WriteLine("Doctor is not properly initialized.");
                return false;
            }
            return true;
        }

        private static SqlConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        private static SqlCommand CreateCommand(string query, SqlConnection connection)
        {
            return new SqlCommand(query, connection);
        }

        private static void AddParameter(SqlCommand cmd, string paramName, object value)
        {
            cmd.Parameters.AddWithValue(paramName, value ?? DBNull.Value);
        }
    }
}
