using System;
using Microsoft.Data.SqlClient;

namespace myClinic
{
    public static class Admin
    {
        private static string _connectionString;

        // Initialize the connection string once at app startup
        public static void Initialize(string connectionString)
        {
            _connectionString = connectionString;
        }

        public static void CreateDoctor()
        {
            Console.WriteLine("\n=== Create Doctor ===");

            Random random = new Random();
            int number = random.Next(100, 999);

            Console.Write("Enter Doctor's First Name: ");
            string firstName = Console.ReadLine().Trim();

            Console.Write("Enter Doctor's Last Name: ");
            string lastName = Console.ReadLine().Trim();

            Console.Write("Enter Doctor's Specialty: ");
            string specialty = Console.ReadLine().Trim();

            Console.Write("Set a Password: ");
            string password = Console.ReadLine().Trim(); // Reminder: Password hashing to be implemented later!

            string username = $"{firstName}{lastName}@myclinic.co.za";

            using SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();

            // Check if username already exists
            string checkUsernameQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
            using (SqlCommand cmd = new SqlCommand(checkUsernameQuery, connection))
            {
                cmd.Parameters.AddWithValue("@Username", username);

                int exists = (int)cmd.ExecuteScalar();
                if (exists > 0)
                {
                    username = $"{firstName}{lastName}{number}@myclinic.co.za";
                    Console.WriteLine($"Username already taken. New username: {username}");
                }
            }

            // Insert into StaffDoctors
            string insertDoctorQuery = "INSERT INTO StaffDoctors (FirstName, LastName, TypeDoc) VALUES (@FirstName, @LastName, @Specialty)";
            using (SqlCommand cmd = new SqlCommand(insertDoctorQuery, connection))
            {
                cmd.Parameters.AddWithValue("@FirstName", firstName);
                cmd.Parameters.AddWithValue("@LastName", lastName);
                cmd.Parameters.AddWithValue("@Specialty", specialty);
                cmd.ExecuteNonQuery();
            }

            // Insert into Users
            string insertUserQuery = "INSERT INTO Users (FirstName, LastName, Username, Role, PasswordHash) VALUES (@FirstName, @LastName, @Username, @Role, @Password)";
            using (SqlCommand cmd = new SqlCommand(insertUserQuery, connection))
            {
                cmd.Parameters.AddWithValue("@FirstName", firstName);
                cmd.Parameters.AddWithValue("@LastName", lastName);
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@Role", "Doctor");
                cmd.Parameters.AddWithValue("@Password", password);
                cmd.ExecuteNonQuery();
            }

            Console.WriteLine("Doctor created successfully!\n");
        }

        public static void CreateReceptionist()
        {
            Console.WriteLine("\n=== Create Receptionist ===");

            Random random = new Random();
            int number = random.Next(100, 999);

            Console.Write("Enter Receptionist's First Name: ");
            string firstName = Console.ReadLine().Trim();

            Console.Write("Enter Receptionist's Last Name: ");
            string lastName = Console.ReadLine().Trim();

            Console.Write("Set a Password: ");
            string password = Console.ReadLine().Trim(); // Reminder: Password hashing to be implemented later!

            string username = $"{firstName}{lastName}@myclinic.co.za";

            using SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();

            // Check if username already exists
            string checkUsernameQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
            using (SqlCommand cmd = new SqlCommand(checkUsernameQuery, connection))
            {
                cmd.Parameters.AddWithValue("@Username", username);

                int exists = (int)cmd.ExecuteScalar();
                if (exists > 0)
                {
                    username = $"{firstName}{lastName}{number}@myclinic.co.za";
                    Console.WriteLine($"Username already taken. New username: {username}");
                }
            }

            // Insert into Users
            string insertUserQuery = "INSERT INTO Users (FirstName, LastName, Username, Role, PasswordHash) VALUES (@FirstName, @LastName, @Username, @Role, @Password)";
            using (SqlCommand cmd = new SqlCommand(insertUserQuery, connection))
            {
                cmd.Parameters.AddWithValue("@FirstName", firstName);
                cmd.Parameters.AddWithValue("@LastName", lastName);
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@Role", "Receptionist");
                cmd.Parameters.AddWithValue("@Password", password);
                cmd.ExecuteNonQuery();
            }

            Console.WriteLine("Receptionist created successfully!\n");
        }

        public static void DeleteUser()
        {
            Console.WriteLine("\n=== Delete User ===");

            using SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();

            bool continueDeleting = true;

            while (continueDeleting)
            {
                Console.Write("Enter the User ID to delete: ");
                if (!int.TryParse(Console.ReadLine(), out int userId))
                {
                    Console.WriteLine("Invalid User ID. Please enter a number.\n");
                    continue;
                }

                string checkUserQuery = "SELECT COUNT(*) FROM Users WHERE UserID = @UserId";
                using (SqlCommand cmd = new SqlCommand(checkUserQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    int exists = (int)cmd.ExecuteScalar();

                    if (exists == 0)
                    {
                        Console.WriteLine("No user found with that ID. Please try again.\n");
                        continue;
                    }
                }

                // Show user details
                string userDetailsQuery = "SELECT FirstName, LastName, Username, Role FROM Users WHERE UserID = @UserId";
                using (SqlCommand cmd = new SqlCommand(userDetailsQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    using SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        Console.WriteLine($"\nUser Details:");
                        Console.WriteLine($"First Name: {reader.GetString(0)}");
                        Console.WriteLine($"Last Name: {reader.GetString(1)}");
                        Console.WriteLine($"Username: {reader.GetString(2)}");
                        Console.WriteLine($"Role: {reader.GetString(3)}\n");
                    }
                }

                Console.WriteLine("Are you sure you want to delete this user? (1 = Confirm, 2 = Cancel)");
                string input = Console.ReadLine();

                if (input == "1")
                {
                    string deleteUserQuery = "DELETE FROM Users WHERE UserID = @UserId";
                    using (SqlCommand cmd = new SqlCommand(deleteUserQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.ExecuteNonQuery();
                    }

                    Console.WriteLine("User deleted successfully.\n");
                    continueDeleting = false;
                }
                else
                {
                    Console.WriteLine("Deletion cancelled.\n");
                    continueDeleting = false;
                }
            }
        }

        public static void ViewAppointmentsForToday()
        {
            Console.WriteLine("\n=== Today's Appointments ===");

            using SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = @"
                SELECT a.AppointmentID,
                       p.FirstName + ' ' + p.LastName AS PatientName,
                       d.FirstName + ' ' + d.LastName AS DoctorName,
                       a.AppointmentDate,
                       a.Status
                FROM Appointments a
                JOIN Patients p ON a.PatientIDNumber = p.IDNumber
                JOIN StaffDoctors d ON a.DoctorID = d.DoctorID
                WHERE CAST(a.AppointmentDate AS DATE) = CAST(GETDATE() AS DATE)
                ORDER BY a.AppointmentDate";

            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                using SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    Console.WriteLine("\nAppointments for today:\n");

                    while (reader.Read())
                    {
                        int appointmentId = reader.GetInt32(0);
                        string patientName = reader.GetString(1);
                        string doctorName = reader.GetString(2);
                        DateTime appointmentDate = reader.GetDateTime(3);
                        string status = reader.GetString(4);

                        Console.WriteLine($"Appointment ID: {appointmentId}");
                        Console.WriteLine($"Patient: {patientName}");
                        Console.WriteLine($"Doctor: {doctorName}");
                        Console.WriteLine($"Date: {appointmentDate}");
                        Console.WriteLine($"Status: {status}\n");
                    }
                }
                else
                {
                    Console.WriteLine("No appointments scheduled for today.\n");
                }
            }
        }
    }
}
