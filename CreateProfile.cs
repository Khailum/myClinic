using System;
using System.Text;
using System.Linq;
using Microsoft.Data.SqlClient;
using Spectre.Console;

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

            Console.Write("Enter your username: ");
            string username = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(username))
            {
                Console.WriteLine("Username cannot be empty.");
                return -1;
            }

            Console.Write("\nEnter your 4-digit PIN: ");
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

            string pin = password.ToString();

            if (pin.Length != 4 || !pin.All(char.IsDigit))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid PIN. Please enter a 4-digit number.");
                Console.ResetColor();
                return -1;
            }

            // Use Spectre.Console for role selection with arrow keys
            var roleChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select Role:")
                    .AddChoices("Receptionist", "Doctor")
            );

            string role = roleChoice == "Receptionist" ? "Receptionist" : "Doctor";

            // Now we store the plaintext PIN (no hashing)
            string insertQuery = "INSERT INTO Users (Username, Password, Role) OUTPUT INSERTED.UserID VALUES (@Username, @Password, @Role)";
            int userId;

            using (SqlCommand cmd = new SqlCommand(insertQuery, _connection))
            {
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@Password", pin); // Store the plaintext PIN
                cmd.Parameters.AddWithValue("@Role", role);

                userId = (int)cmd.ExecuteScalar();
            }

            if (userId > 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n{role} profile created successfully!");
                Console.ResetColor();
            }

            return userId;
        }
    }
}
