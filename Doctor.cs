using System;
using Microsoft.Data.SqlClient;

namespace myClinic
{
    public class Doctor
    {
        public static readonly SqlConnection _connection;
        public static readonly int _doctorId;
        public static void ShowDoctorMenu()
        {
            Console.WriteLine("Welcome Doctor!");
            Console.WriteLine("1. View your appointments for today:");
            Console.WriteLine("2. Add Medical Note:");
            Console.WriteLine("3. View Patient History:");
            Console.WriteLine("4. Exit:");
            Console.Write("Select an option: ");
            if (!int.TryParse(Console.ReadLine(), out int option))
            {
                Console.WriteLine("Invalid option. Please enter a number between 1-4.");
            }
            switch (option)
            {
                case 1:
                    ViewTodaysAppointments();
                    break;
                case 2:
                    Console.WriteLine("What would you like to do?");
                    Console.WriteLine("1. Add Medical Note");
                    Console.WriteLine("2. View Most Recent Medical Note");
                    Console.Write("Enter your choice (1-2): ");
                    string choice = Console.ReadLine();

                    switch (choice)
                    {
                        case "1":
                            AddMedicalNote();
                            break;
                        case "2":
                            ViewMostRecentMedicalNote();
                            break;
                        default:
                            Console.WriteLine("Invalid choice. Please select 1 or 2.");
                            break;
                    }
                    break;
                case 3:
                    ViewPatientHistory();
                    break;
                case 4:
                    break;
                default:
                    Console.WriteLine("Invalid option.");
                    break;
            }
        }

        public static void ViewTodaysAppointments()
        {
            string query = @"
                        SELECT 
                            a.AppointmentID, 
                            p.FirstName, 
                            p.LastName, 
                            a.AppointmentDate, 
                            a.Status
                        FROM Appointments a
                        INNER JOIN Patients p ON a.PatientIDNumber = p.IDNumber
                        WHERE a.DoctorID = @DoctorID
                        AND CAST(a.AppointmentDate AS DATE) = CAST(GETDATE() AS DATE)
                        ORDER BY a.AppointmentDate;
                    ";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@DoctorID", _doctorId);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    Console.Clear();
                    if (reader.HasRows)
                    {
                        Console.WriteLine("=============================================");
                        Console.WriteLine("                   myClinic                  ");
                        Console.WriteLine("=============================================");
                        Console.WriteLine("| Appointment ID | Patient Name     | Time   | Status  |");
                        Console.WriteLine("-------------------------------------------------------------");

                        while (reader.Read())
                        {
                            int appointmentId = reader.GetInt32(0);
                            string patientFirstName = reader.GetString(1);
                            string patientLastName = reader.GetString(2);
                            DateTime appointmentDate = reader.GetDateTime(3);
                            string status = reader.GetString(4);

                            Console.WriteLine($"| {appointmentId,-15} | {patientFirstName} {patientLastName,-13} | {appointmentDate:HH:mm} | {status,-7} |");
                            Console.WriteLine("-------------------------------------------------------------");
                        }
                    }
                    else
                    {
                        Console.WriteLine("\nNo appointments scheduled for today.");
                    }
                }
            }
        }

        public static void AddMedicalNote()
        {
            Console.WriteLine("Enter Appointment ID:");
            if (!int.TryParse(Console.ReadLine(), out int appointmentId))
            {
                Console.WriteLine("Invalid Appointment ID.");
                return;
            }

            Console.Write("Enter Diagnosis: ");
            string diagnosis = Console.ReadLine()?.Trim();

            Console.Write("Enter Symptoms: ");
            string symptoms = Console.ReadLine()?.Trim();

            Console.Write("Enter Treatment Plan: ");
            string treatmentPlan = Console.ReadLine()?.Trim();

            // Validation
            if (string.IsNullOrWhiteSpace(diagnosis) ||
                string.IsNullOrWhiteSpace(symptoms) ||
                string.IsNullOrWhiteSpace(treatmentPlan))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nAll fields must be filled in.");
                Console.ResetColor();
                return;
            }

            string query = @"
        INSERT INTO MedicalNotes 
        (AppointmentID, Diagnosis, Symptoms, TreatmentPlan)
        VALUES 
        (@AppointmentID, @Diagnosis, @Symptoms, @TreatmentPlan);
    ";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@AppointmentID", appointmentId);
                cmd.Parameters.AddWithValue("@Diagnosis", diagnosis);
                cmd.Parameters.AddWithValue("@Symptoms", symptoms);
                cmd.Parameters.AddWithValue("@TreatmentPlan", treatmentPlan);

                cmd.ExecuteNonQuery();
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nMedical note added successfully.");
            Console.ResetColor();
        }

        public static void ViewMostRecentMedicalNote()
        {
            string query = @"
                    SELECT TOP 1 
                        a.AppointmentID, 
                        p.FirstName, 
                        p.LastName, 
                        n.Diagnosis, 
                        n.Symptoms, 
                        n.TreatmentPlan, 
                        n.DateCreated
                    FROM MedicalNotes n
                    INNER JOIN Appointments a ON n.AppointmentID = a.AppointmentID
                    INNER JOIN Patients p ON a.PatientIDNumber = p.IDNumber
                    WHERE a.DoctorID = @DoctorID
                    ORDER BY n.DateCreated DESC;
                ";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@DoctorID", _doctorId);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    Console.Clear();
                    if (reader.HasRows)
                    {
                        Console.WriteLine("=============================================");
                        Console.WriteLine("          Most Recent Medical Note          ");
                        Console.WriteLine("=============================================");
                        Console.WriteLine("| Appointment ID | Patient Name | Diagnosis      | Symptoms       | Treatment Plan  | Date Created |");
                        Console.WriteLine("--------------------------------------------------------------------------------------------------");

                        reader.Read();
                        int appointmentId = reader.GetInt32(0);
                        string patientFirstName = reader.GetString(1);
                        string patientLastName = reader.GetString(2);
                        string diagnosis = reader.IsDBNull(3) ? "No diagnosis available." : reader.GetString(3);
                        string symptoms = reader.IsDBNull(4) ? "No symptoms recorded." : reader.GetString(4);
                        string treatmentPlan = reader.IsDBNull(5) ? "No treatment plan recorded." : reader.GetString(5);
                        DateTime noteDate = reader.GetDateTime(6);

                        Console.WriteLine($"| {appointmentId,-15} | {patientFirstName} {patientLastName,-10} | {diagnosis,-15} | {symptoms,-15} | {treatmentPlan,-15} | {noteDate:yyyy-MM-dd} |");
                        Console.WriteLine("--------------------------------------------------------------------------------------------------");
                    }
                    else
                    {
                        Console.WriteLine("\nNo medical notes found for this doctor.");
                    }
                }
            }
        }

        public static void ViewPatientHistory()
        {
            Console.Write("Enter Patient ID to view history: ");
            if (!int.TryParse(Console.ReadLine(), out int patientId))
            {
                Console.WriteLine("Invalid Patient ID.");
                return;
            }

            string query = @"
                    SELECT 
                        a.AppointmentID,
                        a.AppointmentDate,
                        a.Status,
                        n.Diagnosis,
                        n.Symptoms,
                        n.TreatmentPlan,
                        n.DateCreated
                    FROM Appointments a
                    LEFT JOIN MedicalNotes n ON a.AppointmentID = n.AppointmentID
                    WHERE a.PatientIDNumber = @PatientID
                    AND a.Status = 'Completed'
                    ORDER BY a.AppointmentDate;
                ";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@PatientID", patientId);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    Console.Clear();
                    if (reader.HasRows)
                    {
                        Console.WriteLine("=============================================");
                        Console.WriteLine("                Patient History              ");
                        Console.WriteLine("=============================================");
                        Console.WriteLine("| Appointment ID | Date       | Status  | Diagnosis      | Symptoms       | Treatment Plan  | Note Date  |");
                        Console.WriteLine("--------------------------------------------------------------------------------------------------");

                        while (reader.Read())
                        {
                            int appointmentId = reader.GetInt32(0);
                            DateTime appointmentDate = reader.GetDateTime(1);
                            string status = reader.GetString(2);
                            string diagnosis = reader.IsDBNull(3) ? "No diagnosis available." : reader.GetString(3);
                            string symptoms = reader.IsDBNull(4) ? "No symptoms recorded." : reader.GetString(4);
                            string treatmentPlan = reader.IsDBNull(5) ? "No treatment plan recorded." : reader.GetString(5);
                            DateTime? noteDate = reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6);

                            Console.WriteLine($"| {appointmentId,-15} | {appointmentDate:yyyy-MM-dd} | {status,-7} | {diagnosis,-15} | {symptoms,-15} | {treatmentPlan,-15} | {(noteDate.HasValue ? noteDate.Value.ToString("yyyy-MM-dd") : "N/A"),-10} |");
                            Console.WriteLine("--------------------------------------------------------------------------------------------------");
                        }
                    }
                    else
                    {
                        Console.WriteLine("\nNo completed appointments found for this patient.");
                    }
                }
            }
        }
    }
}
