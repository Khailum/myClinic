using System;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Spectre.Console;

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

            AnsiConsole.MarkupLine("[green]=== Create Doctor ===[/]");

            string firstName = Prompt("Enter Doctor's First Name: ");
            string lastName = Prompt("Enter Doctor's Last Name: ");
            string specialty = Prompt("Enter Doctor's Specialty: ");
            string idNumber = Prompt("Enter Doctor's ID Number: ");

            if (!Regex.IsMatch(firstName, @"^[a-zA-Z]+$"))
            {
                AnsiConsole.MarkupLine("[red]First Name must contain only letters.[/]");
                return;
            }

            if (!Regex.IsMatch(lastName, @"^[a-zA-Z]+$"))
            {
                AnsiConsole.MarkupLine("[red]Last Name must contain only letters.[/]");
                return;
            }

            if (!Regex.IsMatch(specialty, @"^[a-zA-Z\s]+$"))
            {
                AnsiConsole.MarkupLine("[red]Specialty must contain only letters and spaces.[/]");
                return;
            }

            if (!ValidateSouthAfricanID(idNumber))
            {
                AnsiConsole.MarkupLine("[red]Invalid South African ID number format. Please ensure it is 13 digits and valid.[/]");
                return;
            }

            string password = Prompt("Set a Password: ");

            if (!AreValidPassword(password)) return;

            if (!AreValidInputs(firstName, lastName, specialty, idNumber, password)) return;

            string username = GenerateUsername(firstName, lastName);

            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                username = EnsureUniqueUsername(connection, username, firstName, lastName);

                // Insert into StaffDoctors and get DoctorID
                int doctorId = InsertDoctor(connection, firstName, lastName, specialty, idNumber);

                // Now insert into Users with the DoctorID and plain-text password
                InsertUser(connection, firstName, lastName, username, "Doctor", password, doctorId);

                AnsiConsole.MarkupLine("[green]Doctor created successfully![/]");
            }
            catch (SqlException ex)
            {
                AnsiConsole.MarkupLine($"[red]Database error: {ex.Message}[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            }
            AnsiConsole.MarkupLine("[green]\n\n=========================================");
            AnsiConsole.MarkupLine(" Press any key to return to the main menu ");
            AnsiConsole.MarkupLine("=========================================[/]");

            Console.ReadKey();
        }
        public static void CreateReceptionist()
        {
            EnsureConnectionInitialized();  // Ensure connection string is initialized

            AnsiConsole.MarkupLine("[green]=== Create Receptionist ===[/]");

            string firstName = Prompt("Enter Receptionist's First Name: ");
            string lastName = Prompt("Enter Receptionist's Last Name: ");
            string idNumber = Prompt("Enter Receptionist's ID Number: ");

            if (!Regex.IsMatch(firstName, @"^[a-zA-Z]+$"))
            {
                AnsiConsole.MarkupLine("[red]First Name must contain only letters.[/]");
                return;
            }

            if (!Regex.IsMatch(lastName, @"^[a-zA-Z]+$"))
            {
                AnsiConsole.MarkupLine("[red]Last Name must contain only letters.[/]");
                return;
            }

            if (!ValidateSouthAfricanID(idNumber))
            {
                AnsiConsole.MarkupLine("[red]Invalid South African ID number format. Please ensure it is 13 digits and valid.[/]");
                return;
            }

            string password = Prompt("Set a Password: ");
            if (!AreValidPassword(password)) return;

            if (!AreValidInputs(firstName, lastName, idNumber, password)) return;

            string username = GenerateUsername(firstName, lastName);

            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                username = EnsureUniqueUsername(connection, username, firstName, lastName);

                // Insert into StaffReceptionists and get ReceptionistID
                int receptionistId = InsertReceptionist(connection, firstName, lastName, idNumber);

                // Now insert into Users with the ReceptionistID and plain-text password
                InsertUser(connection, firstName, lastName, username, "Receptionist", password, receptionistId);

                AnsiConsole.MarkupLine("[green]Receptionist created successfully![/]");
            }
            catch (SqlException ex)
            {
                AnsiConsole.MarkupLine($"[red]Database error: {ex.Message}[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            }
            AnsiConsole.MarkupLine("[green]\n\n=========================================");
            AnsiConsole.MarkupLine(" Press any key to return to the main menu ");
            AnsiConsole.MarkupLine("=========================================[/]");

            Console.ReadKey();
        }

        // Delete User account
        public static void DeleteUser()
        {
            EnsureConnectionInitialized();  // Ensure connection string is initialized

            AnsiConsole.MarkupLine("[red]=== Delete User ===[/]");

            string username = Prompt("Enter Username of the user to delete: ");

            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                string query = "DELETE FROM Users WHERE Username = @Username";
                using var cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Username", username);

                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    AnsiConsole.MarkupLine("[green]User deleted successfully![/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]No user found with the provided username.[/]");
                }
            }
            catch (SqlException ex)
            {
                AnsiConsole.MarkupLine($"[red]Database error: {ex.Message}[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            }
            AnsiConsole.MarkupLine("[green]\n\n=========================================");
            AnsiConsole.MarkupLine(" Press any key to return to the main menu ");
            AnsiConsole.MarkupLine("=========================================[/]");

            Console.ReadKey();
        }

        // ---------- South African ID Validation ----------
        private static bool ValidateSouthAfricanID(string idNumber)
        {
            // Check if the ID number is exactly 13 digits long and numeric
            if (idNumber.Length != 13 || !Regex.IsMatch(idNumber, @"^\d{13}$"))
            {
                return false;
            }

            // Extract the birth date from the first 6 digits (YYMMDD format)
            string birthDateStr = idNumber.Substring(0, 6);
            if (!DateTime.TryParseExact(birthDateStr, "yyMMdd", null, System.Globalization.DateTimeStyles.None, out _))
            {
                return false; // Invalid birthdate
            }

            // Check gender (7th digit should be between 0-4 for female, 5-9 for male)
            char genderDigit = idNumber[6];
            if (!char.IsDigit(genderDigit) || !((genderDigit >= '0' && genderDigit <= '4') || (genderDigit >= '5' && genderDigit <= '9')))
            {
                return false; // Invalid gender digit
            }

            // Check citizenship (10th digit should be 0 for South African citizen or 1 for permanent resident)
            char citizenshipDigit = idNumber[9];
            if (citizenshipDigit != '0' && citizenshipDigit != '1')
            {
                return false; // Invalid citizenship digit
            }

            // Checksum validation using the Luhn algorithm
            return IsValidLuhn(idNumber);
        }

        // Luhn algorithm for checksum validation
        private static bool IsValidLuhn(string idNumber)
        {
            int sum = 0;
            bool alternate = false;

            // Loop through the digits in reverse order (excluding the last digit which is the checksum)
            for (int i = idNumber.Length - 2; i >= 0; i--)
            {
                int n = int.Parse(idNumber[i].ToString());
                if (alternate)
                {
                    n *= 2;
                    if (n > 9)
                    {
                        n -= 9; // Subtract 9 if the result is greater than 9
                    }
                }
                sum += n;
                alternate = !alternate;
            }

            // The checksum digit (last digit) should make the sum divisible by 10
            int checksum = int.Parse(idNumber[12].ToString());
            sum += checksum;

            return sum % 10 == 0;
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
                    AnsiConsole.MarkupLine("[red]All fields are required. Operation cancelled.[/]");
                    return false;
                }
            }
            return true;
        }

        private static bool AreValidPassword(string password)
        {
            if (password.Length < 8)
            {
                AnsiConsole.MarkupLine("[red]Password must be at least 8 characters long.[/]");
                return false;
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
                AnsiConsole.MarkupLine($"[yellow]Username already taken. New username: {username}[/]");
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
        INSERT INTO Users (FirstName, LastName, Username, Role, Password, DoctorID)
        VALUES (@FirstName, @LastName, @Username, @Role, @Password, @DoctorID)";

            using var cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@FirstName", firstName);
            cmd.Parameters.AddWithValue("@LastName", lastName);
            cmd.Parameters.AddWithValue("@Username", username);
            cmd.Parameters.AddWithValue("@Role", role);
            cmd.Parameters.AddWithValue("@Password", password);  // Store plain-text password
            cmd.Parameters.AddWithValue("@DoctorID", (object?)doctorId ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }
        private static int InsertReceptionist(SqlConnection connection, string firstName, string lastName, string idNumber)
        {
            string query = @"
        INSERT INTO StaffReceptionists (FirstName, LastName, IDNumber)
        VALUES (@FirstName, @LastName, @IDNumber);
        SELECT SCOPE_IDENTITY();";

            using var cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@FirstName", firstName);
            cmd.Parameters.AddWithValue("@LastName", lastName);
            cmd.Parameters.AddWithValue("@IDNumber", idNumber);

            object result = cmd.ExecuteScalar();
            return Convert.ToInt32(result);
        }

    }
}
