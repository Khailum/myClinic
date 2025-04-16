using System;
using System.Data;
using System.Threading;
using Microsoft.Data.SqlClient;

namespace myClinic
{
    public class Receptionist
    {
        private static readonly string ConnectionString = @"Data Source=ACADEMICWEAPON;Initial Catalog=PMSmyClinic;Integrated Security=True;Trust Server Certificate=True;Encrypt=False";

        public static void RegisterPatient()
        {
            using SqlConnection connection = new SqlConnection(ConnectionString);
            try
            {
                connection.Open();

                Console.WriteLine("Enter Patient First name:");
                string firstName = Console.ReadLine()?.Trim();

                Console.WriteLine("Enter Patient Last name:");
                string lastName = Console.ReadLine()?.Trim();

                Console.WriteLine("Enter Patient Age:");
                if (!int.TryParse(Console.ReadLine(), out int age) || age <= 0)
                {
                    Console.WriteLine("Invalid age input.");
                    return;
                }

                Console.WriteLine("Enter Patient Gender:");
                string gender = Console.ReadLine()?.Trim();

                string idNumber = "";
                while (true)
                {
                    Console.WriteLine("Enter Patient ID number (13 digits):");
                    idNumber = Console.ReadLine()?.Trim();
                    if (!string.IsNullOrEmpty(idNumber) && idNumber.Length == 13 && long.TryParse(idNumber, out _))
                        break;
                    Console.WriteLine("Invalid ID number. Please try again.");
                }

                string insertPatientQuery = @"INSERT INTO Patients (FirstName, LastName, DateOfBirth, Gender, IDNumber)
                                              VALUES (@FirstName, @LastName, @DateOfBirth, @Gender, @IDNumber)";

                using SqlCommand cmd = new SqlCommand(insertPatientQuery, connection);
                cmd.Parameters.AddWithValue("@FirstName", firstName);
                cmd.Parameters.AddWithValue("@LastName", lastName);
                cmd.Parameters.AddWithValue("@DateOfBirth", DateTime.Today.AddYears(-age));
                cmd.Parameters.AddWithValue("@Gender", gender);
                cmd.Parameters.AddWithValue("@IDNumber", idNumber);
                cmd.ExecuteNonQuery();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Patient registered successfully.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
            }
            Console.WriteLine("\n\n=========================================");
            Console.WriteLine(" Press any key to return to the main menu ");
            Console.WriteLine("=========================================\n");
            Console.ReadKey();

        }

        public static void BookAppointment()
        {
            while (true)
            {
                using SqlConnection connection = new SqlConnection(ConnectionString);
                try
                {
                    connection.Open();

                    Console.WriteLine("Here are all our doctors:");
                    string doctorQuery = "SELECT * FROM StaffDoctors";
                    using SqlCommand cmd = new SqlCommand(doctorQuery, connection);
                    using SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Console.WriteLine($"DoctorID: {reader.GetInt32(0)}, Name: {reader.GetString(1)} {reader.GetString(2)}, Specialty: {reader.GetString(3)}");
                    }
                    reader.Close();

                    Console.WriteLine("Enter Patient ID number:");
                    string patientId = Console.ReadLine()?.Trim();

                    string checkPatientQuery = "SELECT COUNT(*) FROM Patients WHERE IDNumber = @IDNumber";
                    using SqlCommand checkCmd = new SqlCommand(checkPatientQuery, connection);
                    checkCmd.Parameters.AddWithValue("@IDNumber", patientId);
                    if ((int)checkCmd.ExecuteScalar() == 0)
                    {
                        Console.WriteLine("Patient not found. Try again.");
                        continue;
                    }

                    Console.WriteLine("Enter reason for the appointment:");
                    string reason = Console.ReadLine();

                    Console.WriteLine("Enter Doctor ID:");
                    string doctorId = Console.ReadLine();

                    Console.WriteLine("Enter appointment date and time (YYYY-MM-DD HH:MM:SS):");
                    if (!DateTime.TryParse(Console.ReadLine(), out DateTime appointmentDate) || appointmentDate <= DateTime.Now)
                    {
                        Console.WriteLine("Invalid or past date entered.");
                        continue;
                    }

                    string checkDateQuery = "SELECT COUNT(*) FROM Appointments WHERE AppointmentDate = @Date AND DoctorID = @DoctorID";
                    using SqlCommand dateCmd = new SqlCommand(checkDateQuery, connection);
                    dateCmd.Parameters.AddWithValue("@Date", appointmentDate);
                    dateCmd.Parameters.AddWithValue("@DoctorID", doctorId);
                    if ((int)dateCmd.ExecuteScalar() > 0)
                    {
                        Console.WriteLine("The chosen date and time is already occupied. Please choose another.");
                        Thread.Sleep(2500);
                        Console.Clear();
                        continue;
                    }

                    string insertAppointmentQuery = @"INSERT INTO Appointments (PatientIDNumber, DoctorID, AppointmentDate, Status)
                                                      VALUES (@IDNumber, @DoctorID, @AppointmentDate, 'Scheduled')";
                    using SqlCommand insertCmd = new SqlCommand(insertAppointmentQuery, connection);
                    insertCmd.Parameters.AddWithValue("@IDNumber", patientId);
                    insertCmd.Parameters.AddWithValue("@DoctorID", doctorId);
                    insertCmd.Parameters.AddWithValue("@AppointmentDate", appointmentDate);
                    insertCmd.ExecuteNonQuery();

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Appointment booked successfully.");
                    Console.ResetColor();
                    Console.WriteLine("\n\n=========================================");
                    Console.WriteLine(" Press any key to return to the main menu ");
                    Console.WriteLine("=========================================\n");
                    Console.ReadKey();
                    break;
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.ResetColor();

                }
                Console.WriteLine("\n\n=========================================");
                Console.WriteLine(" Press any key to return to the main menu ");
                Console.WriteLine("=========================================\n");
                Console.ReadKey();

            }
        }

        public static void ViewAppointment()
        {
            using SqlConnection connection = new SqlConnection(ConnectionString);
            connection.Open();

            Console.WriteLine("1. View all appointments\n2. View appointments for a specific doctor\n3. View appointments for a specific patient\n4. Exit");
            if (!int.TryParse(Console.ReadLine(), out int choice))
            {
                Console.WriteLine("Invalid choice.");
                return;
            }

            string query = choice switch
            {
                1 => "SELECT * FROM Appointments",
                2 => "SELECT * FROM Appointments WHERE DoctorID = @ID",
                3 => "SELECT * FROM Appointments WHERE PatientIDNumber = @ID",
                _ => null
            };

            if (query == null) return;

            using SqlCommand cmd = new SqlCommand(query, connection);

            if (choice == 2)
            {
                Console.WriteLine("Enter Doctor ID:");
                cmd.Parameters.AddWithValue("@ID", int.Parse(Console.ReadLine()));
            }
            else if (choice == 3)
            {
                Console.WriteLine("Enter Patient ID Number:");
                cmd.Parameters.AddWithValue("@ID", Console.ReadLine());
            }

            using SqlDataReader reader = cmd.ExecuteReader();

            Console.Clear();
            Console.WriteLine("Appointments:");

            while (reader.Read())
            {
                Console.WriteLine($"AppointmentID: {reader.GetInt32(0)}\nPatientID: {reader.GetString(1)}\nDoctorID: {reader.GetInt32(2)}\nAppointmentDate: {reader.GetDateTime(3)}\nStatus: {reader.GetString(4)}\n");
            }
            Console.WriteLine("\n\n=========================================");
            Console.WriteLine(" Press any key to return to the main menu ");
            Console.WriteLine("=========================================\n");
            Console.ReadKey();


        }

        public static void UpdateAppointment()
        {
            using SqlConnection connection = new SqlConnection(ConnectionString);
            try
            {
                connection.Open();

                Console.WriteLine("Enter Appointment ID to update:");
                if (!int.TryParse(Console.ReadLine(), out int appointmentId))
                {
                    Console.WriteLine("Invalid Appointment ID.");
                    return;
                }

                string checkQuery = "SELECT COUNT(*) FROM Appointments WHERE AppointmentID = @AppointmentID";
                using SqlCommand checkCmd = new SqlCommand(checkQuery, connection);
                checkCmd.Parameters.AddWithValue("@AppointmentID", appointmentId);
                if ((int)checkCmd.ExecuteScalar() == 0)
                {
                    Console.WriteLine("Appointment not found.");
                    return;
                }

                Console.WriteLine("Enter new appointment date and time (YYYY-MM-DD HH:MM:SS):");
                if (!DateTime.TryParse(Console.ReadLine(), out DateTime newDate) || newDate <= DateTime.Now)
                {
                    Console.WriteLine("Invalid or past date entered.");
                    return;
                }

                Console.WriteLine("Enter new status (Scheduled, Completed, Cancelled):");
                string newStatus = Console.ReadLine();

                string updateQuery = "UPDATE Appointments SET AppointmentDate = @NewDate, Status = @Status WHERE AppointmentID = @AppointmentID";
                using SqlCommand updateCmd = new SqlCommand(updateQuery, connection);
                updateCmd.Parameters.AddWithValue("@NewDate", newDate);
                updateCmd.Parameters.AddWithValue("@Status", newStatus);
                updateCmd.Parameters.AddWithValue("@AppointmentID", appointmentId);

                int affected = updateCmd.ExecuteNonQuery();
                Console.WriteLine(affected > 0 ? "Appointment updated successfully." : "No changes made.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
            }
            Console.WriteLine("\n\n=========================================");
            Console.WriteLine(" Press any key to return to the main menu ");
            Console.WriteLine("=========================================\n");
            Console.ReadKey();


        }
    }
}
