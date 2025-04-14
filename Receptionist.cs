using System;
using System.Data;
using System.Threading;
using Microsoft.Data.SqlClient;

namespace myClinic
{
    public class Receptionist
    {
        private static readonly string ConnectionString = @"Data Source=DESKTOP-O7AMP6F;Initial Catalog=myClinicPMS;Integrated Security=True;Trust Server Certificate=True;encrypt=false";

        public static void RegisterPatient()
        {
            using SqlConnection connection = new SqlConnection(ConnectionString);
            try
            {
                connection.Open();
                Console.WriteLine("Enter Patient First name:");
                string firstName = Console.ReadLine();
                Console.WriteLine("Enter Patient Last name:");
                string lastName = Console.ReadLine();
                Console.WriteLine("Enter Patient Age:");
                if (!int.TryParse(Console.ReadLine(), out int age))
                {
                    Console.WriteLine("Invalid age input.");
                    return;
                }
                Console.WriteLine("Enter Patient Gender:");
                string gender = Console.ReadLine();

                bool validId = false;
                string idNumber = "";

                while (!validId)
                {
                    Console.WriteLine("Enter Patient ID number (13 digits):");
                    idNumber = Console.ReadLine();
                    if (idNumber.Length == 13)
                    {
                        validId = true;
                    }
                    else
                    {
                        Console.WriteLine("Invalid ID number. Please try again.");
                    }
                }

                string insertPatientQuery = @"INSERT INTO Profiles (FirstName, LastName, Age, Gender, IDNumber, Role)
                                              VALUES (@FirstName, @LastName, @Age, @Gender, @IDNumber, 'Patient')";
                using SqlCommand cmd = new SqlCommand(insertPatientQuery, connection);
                cmd.Parameters.AddWithValue("@FirstName", firstName);
                cmd.Parameters.AddWithValue("@LastName", lastName);
                cmd.Parameters.AddWithValue("@Age", age);
                cmd.Parameters.AddWithValue("@Gender", gender);
                cmd.Parameters.AddWithValue("@IDNumber", idNumber);
                cmd.ExecuteNonQuery();

                Console.WriteLine("Patient registered successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public static void BookAppointment()
        {
            bool booking = true;

            while (booking)
            {
                using SqlConnection connection = new SqlConnection(ConnectionString);
                try
                {
                    connection.Open();
                    Console.WriteLine("Here are all our doctors:");
                    string doctorQuery = "SELECT * FROM Doctors";
                    using SqlCommand cmd = new SqlCommand(doctorQuery, connection);
                    using SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        Console.WriteLine($"DoctorID: {reader.GetInt32(0)}, FirstName: {reader.GetString(1)}, LastName: {reader.GetString(2)}, Speciality: {reader.GetString(3)}");
                        Console.WriteLine();
                    }
                    reader.Close();

                    Console.WriteLine("Enter Patient ID number:");
                    string patientId = Console.ReadLine();

                    string checkPatientQuery = "SELECT COUNT(*) FROM Patients WHERE IDNumber = @IDNumber";
                    using SqlCommand checkCmd = new SqlCommand(checkPatientQuery, connection);
                    checkCmd.Parameters.AddWithValue("@IDNumber", patientId);
                    int patientExists = (int)checkCmd.ExecuteScalar();

                    if (patientExists == 0)
                    {
                        Console.WriteLine("Patient ID number is incorrect. Try again.");
                        continue;
                    }

                    Console.WriteLine("Enter reason for the appointment:");
                    string reason = Console.ReadLine();
                    Console.WriteLine("Enter Doctor ID:");
                    string doctorId = Console.ReadLine();
                    Console.WriteLine("Enter appointment date and time (YYYY/MM/DD HH:MM:SS):");

                    if (!DateTime.TryParse(Console.ReadLine(), out DateTime appointmentDate) || appointmentDate <= DateTime.Now)
                    {
                        Console.WriteLine("Invalid or past date entered.");
                        continue;
                    }

                    string checkDateQuery = "SELECT COUNT(*) FROM Appointments WHERE AppointmentDateTime = @Date AND DoctorID = @DoctorID";
                    using SqlCommand dateCmd = new SqlCommand(checkDateQuery, connection);
                    dateCmd.Parameters.AddWithValue("@Date", appointmentDate);
                    dateCmd.Parameters.AddWithValue("@DoctorID", doctorId);
                    int isOccupied = (int)dateCmd.ExecuteScalar();

                    if (isOccupied > 0)
                    {
                        Console.WriteLine("The chosen date and time is already occupied. Please choose another.");
                        Thread.Sleep(2500);
                        Console.Clear();
                        continue;
                    }

                    string insertAppointmentQuery = @"INSERT INTO Appointments (IDNumber, DoctorID, Reason, AppointmentDateTime, Status)
                                                      VALUES (@IDNumber, @DoctorID, @Reason, @AppointmentDateTime, 'Scheduled')";
                    using SqlCommand insertCmd = new SqlCommand(insertAppointmentQuery, connection);
                    insertCmd.Parameters.AddWithValue("@IDNumber", patientId);
                    insertCmd.Parameters.AddWithValue("@DoctorID", doctorId);
                    insertCmd.Parameters.AddWithValue("@Reason", reason);
                    insertCmd.Parameters.AddWithValue("@AppointmentDateTime", appointmentDate);
                    insertCmd.ExecuteNonQuery();

                    Console.WriteLine("Appointment booked successfully.");
                    booking = false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    booking = true;
                }
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
                3 => "SELECT * FROM Appointments WHERE IDNumber = @ID",
                _ => null
            };

            if (query == null)
            {
                Environment.Exit(0);
            }

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
            Console.WriteLine("Fetching data...");

            while (reader.Read())
            {
                Console.WriteLine($"AppointmentID: {reader.GetInt32(0)}\n" +
                                  $"IDNumber: {reader.GetString(1)}\n" +
                                  $"DoctorID: {reader.GetInt32(2)}\n" +
                                  $"Reason: {reader.GetString(3)}\n" +
                                  $"AppointmentDate: {reader.GetDateTime(4)}\n" +
                                  $"Status: {(reader.IsDBNull(5) ? "N/A" : reader.GetString(5))}\n");
            }
        }

        public static void UpdateAppointment()
        {
            using SqlConnection connection = new SqlConnection(ConnectionString);
            try
            {
                connection.Open();
                bool updating = true;
                while (updating)
                {
                    Console.WriteLine("Enter Appointment ID you want to update:");
                    if (!int.TryParse(Console.ReadLine(), out int appointmentId))
                    {
                        Console.WriteLine("Invalid Appointment ID.");
                        continue;
                    }

                    Console.WriteLine("Enter new appointment date and time (YYYY/MM/DD HH:MM:SS):");
                    if (!DateTime.TryParse(Console.ReadLine(), out DateTime newDateTime) || newDateTime <= DateTime.Now)
                    {
                        Console.WriteLine("Invalid or past date entered.");
                        continue;
                    }

                    Console.WriteLine("Enter new appointment status:");
                    string newStatus = Console.ReadLine();

                    string updateQuery = @"UPDATE Appointments 
                                           SET AppointmentDateTime = @AppointmentDateTime, Status = @Status 
                                           WHERE AppointmentID = @AppointmentID";

                    using SqlCommand cmd = new SqlCommand(updateQuery, connection);
                    cmd.Parameters.AddWithValue("@AppointmentDateTime", newDateTime);
                    cmd.Parameters.AddWithValue("@Status", newStatus);
                    cmd.Parameters.AddWithValue("@AppointmentID", appointmentId);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        Console.WriteLine("Appointment updated successfully.");
                        updating = false;
                    }
                    else
                    {
                        Console.WriteLine("Appointment ID not found. Try again.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
