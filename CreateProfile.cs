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

        public int ProfileCreate()
        {
            Console.Clear();
            Console.WriteLine("Thank you for choosing to create a profile with us!");

            Console.Write("Enter your username: ");
            string username = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(username))
            {
                Console.WriteLine("Username cannot be empty.");
                return -1;
            }

            Console.Write("\nEnter your 4-digit PIN: ");
            string pin = ReadPassword();

            if (pin.Length != 4 || !pin.All(char.IsDigit))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nInvalid PIN. Please enter a 4-digit number.");
                Console.ResetColor();
                return -1;
            }

            // Ask the user what role they want to create
            Console.WriteLine("\nSelect Role:");
            Console.WriteLine("1. Receptionist");
            Console.WriteLine("2. Doctor");
            Console.Write("Enter your choice (1 or 2): ");
            string roleChoice = Console.ReadLine()?.Trim();

            string role;
            if (roleChoice == "1")
            {
                role = "Receptionist";
            }
            else if (roleChoice == "2")
            {
                role = "Doctor";
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nInvalid role selected.");
                Console.ResetColor();
                return -1;
            }

            string hashedPin = HashPassword(pin);

            string insertProfileQuery = "INSERT INTO Users (Username, PasswordHash, Role) VALUES (@Username, @PasswordHash, @Role)";

            using (SqlCommand cmd = new SqlCommand(insertProfileQuery, _connection))
            {
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@PasswordHash", hashedPin);
                cmd.Parameters.AddWithValue("@Role", role);

                cmd.ExecuteNonQuery();
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n{role} profile created successfully!");
            Console.ResetColor();

            return 1;
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

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
