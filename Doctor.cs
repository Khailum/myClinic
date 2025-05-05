// Doctor.cs with Spectre.Console enhancements
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

                _doctorId = GetDoctorIdFromUser(userId, connection);

                if (_doctorId <= 0)
                {
                    AnsiConsole.Markup("[red]No doctor found linked to this user.[/]");
                    return false;
                }

                _doctorFullName = GetDoctorFullName(_doctorId, connection);

                if (string.IsNullOrEmpty(_doctorFullName))
                {
                    AnsiConsole.Markup("[red]Doctor does not have a full name. Check the database.[/]");
                    return false;
                }

                AnsiConsole.Markup($"[green]Doctor Initialized: {_doctorFullName}[/]\n");
                return true;
            }
            catch (Exception ex)
            {
                AnsiConsole.Markup($"[red]Error: {ex.Message}[/]\n");
                return false;
            }
        }

        private static int GetDoctorIdFromUser(int userId, SqlConnection connection)
        {
            string query = "SELECT sd.DoctorID FROM Users u INNER JOIN StaffDoctors sd ON u.DoctorID = sd.DoctorID WHERE u.UserID = @UserID AND u.Role = 'Doctor';";
            using var cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@UserID", userId);
            var result = cmd.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        private static string GetDoctorFullName(int doctorId, SqlConnection connection)
        {
            string query = "SELECT FirstName, LastName FROM StaffDoctors WHERE DoctorID = @DoctorID";
            using var cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@DoctorID", doctorId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                return $"{reader["FirstName"]} {reader["LastName"]}";

            return null;
        }

        public static void ShowDoctorMenu()
        {
            if (string.IsNullOrEmpty(_connectionString) || _doctorId <= 0 || string.IsNullOrEmpty(_doctorFullName))
            {
                AnsiConsole.Markup("[red]Doctor is not properly initialized.[/]");
                return;
            }

            bool keepRunning = true;

            while (keepRunning)
            {
                Console.Clear();
                Logo.Display();

                AnsiConsole.Write(
                    new Rule($"[blue]Welcome Dr. {_doctorFullName}![/]")
                        .RuleStyle("blue")
                        .Centered()
                );

                var doctorChoice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold]Select an option:[/]")
                        .AddChoices(new[]
                        {
            "View upcoming appointments",
            "Medical Notes",
            "View Patient History",
            "Exit"
                        }));

                switch (doctorChoice)
                {
                    case "View upcoming appointments":
                        ViewUpcomingAppointments();
                        break;

                    case "Medical Notes":
                        ViewMostRecentMedicalNote();
                        break;

                    case "View Patient History":
                        ViewPatientHistory();
                        break;

                    case "Exit":
                        keepRunning = false;
                        break;
                }


                if (keepRunning) WaitForInput();
            }
        }
        private static void ViewMostRecentMedicalNote()
        {
            if (!EnsureInitialized()) return;

            // Prompting the user to enter an Appointment ID, validating the input.
            Console.Write("Enter Appointment ID: ");
            if (!int.TryParse(Console.ReadLine(), out int appointmentId) || appointmentId <= 0)
            {
                AnsiConsole.Markup("[red]Invalid Appointment ID. Please enter a valid number.[/]\n");
                return;
            }

            string query = "SELECT TOP 1 a.AppointmentID, p.FirstName, p.LastName, n.Diagnosis, n.Symptoms, n.TreatmentPlan, n.DateCreated " +
                           "FROM MedicalNotes n " +
                           "INNER JOIN Appointments a ON n.AppointmentID = a.AppointmentID " +
                           "INNER JOIN Patients p ON a.PatientIDNumber = p.IDNumber " +
                           "WHERE a.DoctorID = @DoctorID AND a.AppointmentID = @AppointmentID " +
                           "ORDER BY n.DateCreated DESC;";

            try
            {
                using var connection = CreateConnection();
                connection.Open();
                using var cmd = CreateCommand(query, connection);
                AddParameter(cmd, "@DoctorID", _doctorId);
                AddParameter(cmd, "@AppointmentID", appointmentId);

                using var reader = cmd.ExecuteReader();

                Console.Clear();

                // If the reader finds a row, show the most recent medical note.
                if (reader.Read())
                {
                    var panel = new Panel($"[bold]Appointment ID:[/] {reader.GetInt32(0)}\n" +
                                          $"[bold]Patient:[/] {reader.GetString(1)} {reader.GetString(2)}\n" +
                                          $"[bold]Diagnosis:[/] {reader.GetString(3)}\n" +
                                          $"[bold]Symptoms:[/] {reader.GetString(4)}\n" +
                                          $"[bold]Treatment Plan:[/] {reader.GetString(5)}\n" +
                                          $"[bold]Date Created:[/] {reader.GetDateTime(6)}")
                                .Border(BoxBorder.Rounded)
                                .Header("Most Recent Medical Note", Justify.Center);
                    AnsiConsole.Write(panel);
                }
                else
                {
                    AnsiConsole.Markup("[red]No medical note found for the given Appointment ID.[/]\n");
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.Markup($"[red]Error: {ex.Message}[/]\n");
            }
        }


        public static void ViewUpcomingAppointments()
        {
            if (!EnsureInitialized()) return;

            string query = @"SELECT a.AppointmentID, p.FirstName, p.LastName, a.AppointmentDate, a.Status FROM Appointments a INNER JOIN Patients p ON a.PatientIDNumber = p.IDNumber WHERE a.DoctorID = @DoctorID AND CAST(a.AppointmentDate AS DATE) >= CAST(GETDATE() AS DATE) ORDER BY a.AppointmentDate;";

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
                    var table = new Table().Border(TableBorder.Rounded);
                    table.AddColumns("ID", "Patient", "Date", "Time", "Status");

                    while (reader.Read())
                    {
                        var date = reader.GetDateTime(3);
                        table.AddRow(
                            reader["AppointmentID"].ToString(),
                            $"{reader["FirstName"]} {reader["LastName"]}",
                            date.ToString("yyyy-MM-dd"),
                            date.ToString("HH:mm"),
                            reader["Status"].ToString()
                        );
                    }

                    AnsiConsole.Write(table);
                }
                else
                {
                    AnsiConsole.Markup("[red]No upcoming appointments found.[/]\n");
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.Markup($"[red]Error: {ex.Message}[/]\n");
            }
        }

        private static void AddMedicalNote()
        {
            if (!EnsureInitialized()) return;

            try
            {
                using var connection = CreateConnection();
                connection.Open();

                Console.Write("Enter Appointment ID: ");
                if (!int.TryParse(Console.ReadLine(), out int appointmentId) || appointmentId <= 0)
                {
                    AnsiConsole.Markup("[red]Invalid Appointment ID. Please enter a valid number.[/]\n");
                    return;
                }

                Console.Write("Enter Diagnosis: ");
                string diagnosis = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(diagnosis))
                {
                    AnsiConsole.Markup("[red]Diagnosis cannot be empty.[/]\n");
                    return;
                }

                Console.Write("Enter Symptoms: ");
                string symptoms = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(symptoms))
                {
                    AnsiConsole.Markup("[red]Symptoms cannot be empty.[/]\n");
                    return;
                }

                Console.Write("Enter Treatment Plan: ");
                string treatmentPlan = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(treatmentPlan))
                {
                    AnsiConsole.Markup("[red]Treatment plan cannot be empty.[/]\n");
                    return;
                }

                string query = "INSERT INTO MedicalNotes (AppointmentID, Diagnosis, Symptoms, TreatmentPlan) VALUES (@AppointmentID, @Diagnosis, @Symptoms, @TreatmentPlan);";
                using var cmd = CreateCommand(query, connection);
                AddParameter(cmd, "@AppointmentID", appointmentId);
                AddParameter(cmd, "@Diagnosis", diagnosis);
                AddParameter(cmd, "@Symptoms", symptoms);
                AddParameter(cmd, "@TreatmentPlan", treatmentPlan);
                cmd.ExecuteNonQuery();

                AnsiConsole.Markup("[green]Medical note added successfully.[/]\n");
            }
            catch (Exception ex)
            {
                AnsiConsole.Markup($"[red]Error: {ex.Message}[/]\n");
            }
        }

        private static void ViewPatientHistory()
        {
            if (!EnsureInitialized()) return;

            Console.Write("Enter Patient ID Number: ");
            string patientId = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(patientId) || !patientId.All(char.IsDigit)) // Validate that PatientID is a numeric string
            {
                AnsiConsole.Markup("[red]Invalid Patient ID Number. Please enter a valid numeric value.[/]\n");
                return;
            }

            string query = "SELECT a.AppointmentID, a.AppointmentDate, a.Status, m.Diagnosis, m.Symptoms, m.TreatmentPlan FROM Appointments a INNER JOIN MedicalNotes m ON a.AppointmentID = m.AppointmentID WHERE a.PatientIDNumber = @PatientID ORDER BY a.AppointmentDate DESC;";

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
                    var table = new Table().Border(TableBorder.Rounded);
                    table.AddColumns("Appointment ID", "Date", "Status", "Diagnosis", "Symptoms", "Treatment");

                    while (reader.Read())
                    {
                        table.AddRow(
                            reader["AppointmentID"].ToString(),
                            Convert.ToDateTime(reader["AppointmentDate"]).ToString("yyyy-MM-dd"),
                            reader["Status"].ToString(),
                            reader["Diagnosis"].ToString(),
                            reader["Symptoms"].ToString(),
                            reader["TreatmentPlan"].ToString()
                        );
                    }

                    AnsiConsole.Write(table);
                }
                else
                {
                    AnsiConsole.Markup("[red]No patient history found.[/]\n");
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.Markup($"[red]Error: {ex.Message}[/]\n");
            }
        }

        private static void WaitForInput()
        {
            AnsiConsole.Markup("[grey]\nPress any key to return to the main menu...[/]\n");
            Console.ReadKey();
        }


        private static bool EnsureInitialized() => !string.IsNullOrEmpty(_connectionString) && _doctorId > 0;

        private static SqlConnection CreateConnection() => new SqlConnection(_connectionString);

        private static SqlCommand CreateCommand(string query, SqlConnection connection) => new SqlCommand(query, connection);

        private static void AddParameter(SqlCommand cmd, string paramName, object value)
            => cmd.Parameters.AddWithValue(paramName, value ?? DBNull.Value);
    }
}
