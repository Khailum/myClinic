using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace myClinic
{
    public class Doctor
    {
        private readonly SqlConnection _connection;
        private readonly int _doctorId;

        public Doctor(SqlConnection connection, int doctorId)
        {
            _connection = connection;
            _doctorId = doctorId;
        }
        public void ShowDoctorMenu()
        {
            Console.WriteLine("Welcome Doctor!");
            Console.WriteLine("1.View your appointments for today:\n2.Add Medical Note:\n3.View Patient History:\n4.Exit:");
            Console.Write("Select an option:");
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
                    AddMedicalNote();
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

        private void ViewTodaysAppointments()
        {
            string query = @"
                                SELECT 
                                    a.AppointmentID, 
                                    p.FirstName, 
                                    p.LastName, 
                                    a.AppointmentDate, 
                                    a.Status
                                FROM Appointments a
                                INNER JOIN Patients p ON a.PatientID = p.PatientID
                                WHERE a.DoctorID = @DoctorID
                                AND CAST(a.AppointmentDate AS DATE) = CAST(GETDATE() AS DATE)
                                ORDER BY a.AppointmentDate;
                            ";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@DoctorID", _doctorId);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        Console.WriteLine("\nToday's Appointments:");
                        Console.WriteLine("----------------------------------------------------");
                        while (reader.Read())
                        {
                            int appointmentId = reader.GetInt32(0);
                            string patientFirstName = reader.GetString(1);
                            string patientLastName = reader.GetString(2);
                            DateTime appointmentDate = reader.GetDateTime(3);
                            string status = reader.GetString(4);

                            Console.WriteLine($"Appointment ID: {appointmentId}");
                            Console.WriteLine($"Patient: {patientFirstName} {patientLastName}");
                            Console.WriteLine($"Time: {appointmentDate:HH:mm}");
                            Console.WriteLine($"Status: {status}");
                            Console.WriteLine("----------------------------------------------------");
                        }
                    }
                    else
                    {
                        Console.WriteLine("\nNo appointments scheduled for today.");
                    }
                }
            }
        }


        private void AddMedicalNote()
        {
            Console.WriteLine("Enter Appointment ID:");
            if (!int.TryParse(Console.ReadLine(), out int appointmentId))
            {
                Console.WriteLine("Invalid Appointment ID.");
                return;
            }

            Console.WriteLine("Enter Patient ID:");
            if (!int.TryParse(Console.ReadLine(), out int patientId))
            {
                Console.WriteLine("Invalid Patient ID.");
                return;
            }

            Console.Write("Enter Patient First Name: ");
            string firstName = Console.ReadLine()?.Trim();

            Console.Write("Enter Patient Last Name: ");
            string lastName = Console.ReadLine()?.Trim();

            Console.Write("Enter Diagnosis: ");
            string diagnosis = Console.ReadLine()?.Trim();

            Console.Write("Enter Symptoms: ");
            string symptoms = Console.ReadLine()?.Trim();

            Console.Write("Enter Treatment Plan: ");
            string treatmentPlan = Console.ReadLine()?.Trim();

            // Validation
            if (string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(diagnosis) ||
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
        (AppointmentID, PatientID, FirstName, LastName, Diagnosis, Symptoms, TreatmentPlan)
        VALUES 
        (@AppointmentID, @PatientID, @FirstName, @LastName, @Diagnosis, @Symptoms, @TreatmentPlan);
    ";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@AppointmentID", appointmentId);
                cmd.Parameters.AddWithValue("@PatientID", patientId);
                cmd.Parameters.AddWithValue("@FirstName", firstName);
                cmd.Parameters.AddWithValue("@LastName", lastName);
                cmd.Parameters.AddWithValue("@Diagnosis", diagnosis);
                cmd.Parameters.AddWithValue("@Symptoms", symptoms);
                cmd.Parameters.AddWithValue("@TreatmentPlan", treatmentPlan);

                cmd.ExecuteNonQuery();
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nMedical note added successfully.");
            Console.ResetColor();
        }

        private void ViewPatientHistory()
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
                                    n.Note, 
                                    n.DateCreated
                                FROM Appointments a
                                LEFT JOIN MedicalNotes n ON a.AppointmentID = n.AppointmentID
                                WHERE a.PatientID = @PatientID
                                AND a.Status = 'Completed'
                                ORDER BY a.AppointmentDate;
                            ";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@PatientID", patientId);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        Console.WriteLine("\nPatient History:");
                        Console.WriteLine("--------------------------------------------");
                        while (reader.Read())
                        {
                            string patientFirstName = reader.GetString(0);
                            string patientLastName = reader.GetString(1);
                            int appointmentId = reader.GetInt32(2);
                            DateTime appointmentDate = reader.GetDateTime(3);
                            string status = reader.GetString(4);
                            string note = reader.IsDBNull(5) ? "No notes available." : reader.GetString(6);
                            DateTime? noteDate = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(7);

                            Console.WriteLine($"Patient: {patientFirstName} {patientLastName}");
                            Console.WriteLine($"Appointment ID: {appointmentId}");
                            Console.WriteLine($"Date: {appointmentDate:yyyy-MM-dd}");
                            Console.WriteLine($"Status: {status}");
                            Console.WriteLine($"Note: {note}");
                            if (noteDate.HasValue)
                                Console.WriteLine($"Note Date: {noteDate.Value:yyyy-MM-dd}");
                            Console.WriteLine("--------------------------------------------");
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
