using System;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace myClinic
{
    public static class Admin
    {
        private static string _connectionString;

        // Initialize the database connection string
        public static void Initialize(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));
            }
            _connectionString = connectionString;
        }

        private static void EnsureConnectionInitialized()
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("The ConnectionString property has not been initialized.");
            }
        }

        // Create Doctor account
        public static void CreateDoctor()
        {
            EnsureConnectionInitialized();  // Ensure connection string is initialized

            Console.WriteLine("\n=== Create Doctor ===");

            string firstName = Prompt("Enter Doctor's First Name: ");
            string lastName = Prompt("Enter Doctor's Last Name: ");
            string specialty = Prompt("Enter Doctor's Specialty: ");
            string idNumber = Prompt("Enter Doctor's ID Number: ");
            if (!Regex.IsMatch(idNumber, @"^\d{13}$"))
            {
                Console.WriteLine("Invalid ID Number format. Operation cancelled.");
                return;
            }
            string password = Prompt("Set a Password: ");

            if (!AreValidInputs(firstName, lastName, specialty, idNumber, password)) return;

            string username = GenerateUsername(firstName, lastName);

            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                username = EnsureUniqueUsername(connection, username, firstName, lastName);

                // Insert into StaffDoctors and get DoctorID
                int doctorId = InsertDoctor(connection, firstName, lastName, specialty, idNumber);

                // Now insert into Users with the DoctorID
                InsertUser(connection, firstName, lastName, username, "Doctor", password, doctorId);

                
                Console.WriteLine("\nDoctor created successfully!");
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            Console.WriteLine("\n\n=========================================");
            Console.WriteLine(" Press any key to return to the main menu ");
            Console.WriteLine("=========================================\n");
            Console.ReadKey();

        }


        // Create Receptionist account
        public static void CreateReceptionist()
        {
            EnsureConnectionInitialized();  // Ensure connection string is initialized

            Console.WriteLine("\n=== Create Receptionist ===");

            string firstName = Prompt("Enter Receptionist's First Name: ");
            string lastName = Prompt("Enter Receptionist's Last Name: ");
            string password = Prompt("Set a Password: ");

            if (!AreValidInputs(firstName, lastName, password)) return;

            string username = GenerateUsername(firstName, lastName);

            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                username = EnsureUniqueUsername(connection, username, firstName, lastName);
                InsertUser(connection, firstName, lastName, username, "Receptionist", password);

                Console.WriteLine("\nReceptionist created successfully!");
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            Console.WriteLine("\n\n=========================================");
            Console.WriteLine(" Press any key to return to the main menu ");
            Console.WriteLine("=========================================\n");
            Console.ReadKey();

        }

        // Delete user account
        public static void DeleteUser()
        {
            EnsureConnectionInitialized();  // Ensure connection string is initialized

            Console.WriteLine("\n=== Delete User ===");

            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                Console.Write("Enter the User ID to delete: ");
                if (!int.TryParse(Console.ReadLine(), out int userId))
                {
                    Console.WriteLine("Invalid User ID. Operation cancelled.\n");
                    return;
                }

                if (!UserExists(connection, userId))
                {
                    Console.WriteLine("No user found with that ID.\n");
                    return;
                }

                DisplayUserDetails(connection, userId);

                Console.Write("Are you sure you want to delete this user? (1 = Confirm, 2 = Cancel): ");
                string input = Console.ReadLine();

                if (input == "1")
                {
                    DeleteUserById(connection, userId);
                    Console.WriteLine("User deleted successfully.\n");
                }
                else
                {
                    Console.WriteLine("Deletion cancelled.\n");
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            Console.WriteLine("\n\n=========================================");
            Console.WriteLine(" Press any key to return to the main menu ");
            Console.WriteLine("=========================================\n");
            Console.ReadKey();

        }


        // ---------- Utility Methods ----------

        private static string Prompt(string message)
        {
            Console.Write(message);
            return Console.ReadLine().Trim();
        }

        private static bool AreValidInputs(params string[] inputs)
        {
            foreach (var input in inputs)
            {
                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("All fields are required. Operation cancelled.\n");
                    return false;
                }
            }
            return true;
        }

        private static string GenerateUsername(string firstName, string lastName)
        {
            return $"{firstName}{lastName}@myclinic.co.za";
        }

        private static string EnsureUniqueUsername(SqlConnection connection, string username, string firstName, string lastName)
        {
            int attempt = 0;
            while (true)
            {
                string checkQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
                using var cmd = new SqlCommand(checkQuery, connection);
                cmd.Parameters.AddWithValue("@Username", username);

                int exists = (int)cmd.ExecuteScalar();
                if (exists == 0)
                    return username;

                attempt++;
                int randomSuffix = new Random().Next(100, 999);
                username = $"{firstName}{lastName}{randomSuffix}@myclinic.co.za";

                if (attempt > 10) throw new Exception("Failed to generate unique username after multiple attempts.");
                Console.WriteLine($"Username already taken. New username: {username}");
            }
        }


        private static int InsertDoctor(SqlConnection connection, string firstName, string lastName, string specialty, string idNumber)
        {
            string query = @"
        INSERT INTO StaffDoctors (FirstName, LastName, TypeDoc, IDNumber)
        VALUES (@FirstName, @LastName, @Specialty, @IDNumber);
        SELECT SCOPE_IDENTITY();";

            using var cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@FirstName", firstName);
            cmd.Parameters.AddWithValue("@LastName", lastName);
            cmd.Parameters.AddWithValue("@Specialty", specialty);
            cmd.Parameters.AddWithValue("@IDNumber", idNumber);

            object result = cmd.ExecuteScalar();
            return Convert.ToInt32(result);
        }


        private static void InsertUser(SqlConnection connection, string firstName, string lastName, string username, string role, string password, int? doctorId = null)
        {
            string query = @"
        INSERT INTO Users (FirstName, LastName, Username, Role, PasswordHash, DoctorID)
        VALUES (@FirstName, @LastName, @Username, @Role, @Password, @DoctorID)";

            using var cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@FirstName", firstName);
            cmd.Parameters.AddWithValue("@LastName", lastName);
            cmd.Parameters.AddWithValue("@Username", username);
            cmd.Parameters.AddWithValue("@Role", role);
            cmd.Parameters.AddWithValue("@Password", password);
            cmd.Parameters.AddWithValue("@DoctorID", (object?)doctorId ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        private static bool UserExists(SqlConnection connection, int userId)
        {
            string checkUserQuery = "SELECT COUNT(*) FROM Users WHERE UserID = @UserId";
            using var cmd = new SqlCommand(checkUserQuery, connection);
            cmd.Parameters.AddWithValue("@UserId", userId);
            return (int)cmd.ExecuteScalar() > 0;
        }

        private static void DisplayUserDetails(SqlConnection connection, int userId)
        {
            string userDetailsQuery = "SELECT FirstName, LastName, Username, Role FROM Users WHERE UserID = @UserId";
            using var cmd = new SqlCommand(userDetailsQuery, connection);
            cmd.Parameters.AddWithValue("@UserId", userId);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                Console.WriteLine("\nUser Details:");
                Console.WriteLine($"First Name: {reader.GetString(0)}");
                Console.WriteLine($"Last Name: {reader.GetString(1)}");
                Console.WriteLine($"Username: {reader.GetString(2)}");
                Console.WriteLine($"Role: {reader.GetString(3)}\n");
            }
        }

        private static void DeleteUserById(SqlConnection connection, int userId)
        {
            string deleteUserQuery = "DELETE FROM Users WHERE UserID = @UserId";
            using var cmd = new SqlCommand(deleteUserQuery, connection);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.ExecuteNonQuery();
        }
    }
}
