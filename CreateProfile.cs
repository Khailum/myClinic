using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;

namespace myClinic
{
    public class CreateProfile
    {
        private readonly SqlConnection _connection;

        public CreateProfile(SqlConnection connection)
        {
            _connection = connection;
        }

        public int CreateNewProfile()
        {
            Console.Clear();
            Console.WriteLine("Thank you for choosing to create a profile with us!");

            string username = GetUsername();
            if (string.IsNullOrWhiteSpace(username))
            {
                Console.WriteLine("Username cannot be empty.");
                return -1;
            }

            string pin = GetPin();
            if (pin == null) return -1;

            string role = GetRole();
            if (role == null) return -1;

            string hashedPin = Password.HashPassword(pin);
            int userId = InsertProfileToDatabase(username, hashedPin, role);

            if (userId > 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n{role} profile created successfully!");
                Console.ResetColor();
            }

            return userId;
        }

        private string GetUsername()
        {
            Console.Write("Enter your username: ");
            return Console.ReadLine()?.Trim();
        }

        private string GetPin()
        {
            Console.Write("\nEnter your 4-digit PIN: ");
            string pin = ReadPassword();

            if (pin.Length != 4 || !pin.All(char.IsDigit))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nInvalid PIN. Please enter a 4-digit number.");
                Console.ResetColor();
                return null;
            }

            return pin;
        }

        private string GetRole()
        {
            Console.WriteLine("\nSelect Role:");
            Console.WriteLine("1. Receptionist");
            Console.WriteLine("2. Doctor");
            Console.Write("Enter your choice (1 or 2): ");
            string roleChoice = Console.ReadLine()?.Trim();

            switch (roleChoice)
            {
                case "1": return "Receptionist";
                case "2": return "Doctor";
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nInvalid role selected.");
                    Console.ResetColor();
                    return null;
            }
        }

        private int InsertProfileToDatabase(string username, string hashedPin, string role)
        {
            string insertProfileQuery = "INSERT INTO Users (Username, PasswordHash, Role) OUTPUT INSERTED.UserID VALUES (@Username, @PasswordHash, @Role)";

            using (SqlCommand cmd = new SqlCommand(insertProfileQuery, _connection))
            {
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@PasswordHash", hashedPin);
                cmd.Parameters.AddWithValue("@Role", role);

                return (int)cmd.ExecuteScalar();  // Get the inserted UserID
            }
        }

        private string ReadPassword()
        {
            StringBuilder password = new StringBuilder();
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password.Remove(password.Length - 1, 1);
                    Console.Write("\b \b");
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    password.Append(key.KeyChar);
                    Console.Write("*");
                }
            } while (key.Key != ConsoleKey.Enter);

            Console.WriteLine();
            return password.ToString();
        }
    }
}
