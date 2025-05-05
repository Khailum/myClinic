using System;
using Microsoft.Data.SqlClient;
using Spectre.Console;
using System.Security.Cryptography;
using System.Text;

namespace myClinic
{
    public class Login
    {
        private static readonly string connectionString = @"Data Source=ACADEMICWEAPON;Initial Catalog=PMSmyClinic;Integrated Security=True;Trust Server Certificate=True;Encrypt=False";

        public static void LoginDoctor() => LoginUser("Doctor");
        public static void LoginRec() => LoginUser("Receptionist");
        public static void LoginAdmin() => LoginUser("Admin");

        private static void LoginUser(string role)
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            bool isLoggedIn = false;

            while (!isLoggedIn)
            {
                AnsiConsole.MarkupLine($"[cyan]Enter your {role} Username:[/]");
                string username = Console.ReadLine();

                AnsiConsole.MarkupLine($"[cyan]Enter your {role} Password:[/]");
                string password = ReadPassword();  // Get the password input

                if (ValidateCredentials(username, password, role, connection))  // Pass plaintext password
                {
                    AnsiConsole.MarkupLine("[green]You are now logged in.[/]");
                    isLoggedIn = true;

                    ShowMenu(role);

                    switch (role)
                    {
                        case "Receptionist":
                            ShowReceptionistMenu();
                            break;
                        case "Doctor":
                            int userId = FetchUserId(username);
                            if (Doctor.Initialize(connectionString, userId))
                            {
                                Doctor.ShowDoctorMenu();
                            }
                            else
                            {
                                AnsiConsole.MarkupLine("[red]Doctor initialization failed. Please check if the account is linked properly.[/]");
                            }
                            break;
                        case "Admin":
                            ShowAdminMenu();
                            break;
                    }
                }
                else
                {
                    HandleLoginFailure();
                }
            }
        }

        private static string ReadPassword()
        {
            StringBuilder password = new StringBuilder();
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                    break;

                if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password.Length--; // Remove the last character
                    Console.Write("\b \b"); // Remove the asterisk
                }
                else
                {
                    password.Append(key.KeyChar);
                    Console.Write("*"); // Display asterisk for each character
                }
            }
            Console.WriteLine();
            return password.ToString();
        }

        private static int FetchUserId(string username)
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();

            string query = "SELECT UserID FROM Users WHERE Username = @Username";
            using var cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@Username", username);

            var result = cmd.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        private static bool ValidateCredentials(string username, string password, string role, SqlConnection connection)
        {
            string query = @"
                SELECT COUNT(*) 
                FROM Users 
                WHERE Username = @username AND Password = @password AND Role = @role";

            using SqlCommand cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@password", password);
            cmd.Parameters.AddWithValue("@role", role);

            return (int)cmd.ExecuteScalar() > 0;
        }

        private static void HandleLoginFailure()
        {
            AnsiConsole.MarkupLine("[red]\nIncorrect username or password.[/]");
            AnsiConsole.MarkupLine("[yellow]1. Try Again[/]");
            AnsiConsole.MarkupLine("[yellow]2. Exit[/]");

            if (int.TryParse(Console.ReadLine(), out int choice) && choice == 2)
            {
                AnsiConsole.MarkupLine("[green]Exiting... Goodbye![/]");
                Environment.Exit(0);
            }
        }

        private static void ShowMenu(string role)
        {
            Logo.ShowLoading();
            Logo.Display();
            AnsiConsole.MarkupLine($"[green]---- {role} Menu ----[/]");
        }

        private static void ShowReceptionistMenu()
        {
            bool running = true;

            while (running)
            {
                Console.Clear();
                Logo.Display();
                AnsiConsole.Write(new Rule("[purple]===Receptionist Menu===[/]").RuleStyle("purple").Centered());

                var menuPrompt = new SelectionPrompt<string>()
                    .Title("[purple]Select an option[/]")
                    .AddChoices(
                        "Register New Patient",
                        "Book Appointment",
                        "View Appointments",
                        "Logout"
                    );

                string choice = AnsiConsole.Prompt(menuPrompt);

                switch (choice)
                {
                    case "Register New Patient":
                        Receptionist.RegisterPatient();
                        break;
                    case "Book Appointment":
                        Receptionist.BookAppointment();
                        break;
                    case "View Appointments":
                        Receptionist.ViewAppointment();
                        break;
                    case "Logout":
                        running = false;
                        AnsiConsole.MarkupLine("[green]Logging out...[/]");
                        break;
                }
            }
        }

        private static void ShowAdminMenu()
        {
            bool running = true;

            while (running)
            {
                Console.Clear();
                Logo.Display();
                AnsiConsole.Write(new Rule("[white]===Admin Menu===[/]").RuleStyle("white").Centered());

                var menuPrompt = new SelectionPrompt<string>()
                    .Title("[white]Select an option[/]")
                    .AddChoices(
                        "Create Doctor's account",
                        "Create Receptionist's account",
                        "Delete a user",
                        "Logout"
                    );

                string choice = AnsiConsole.Prompt(menuPrompt);

                switch (choice)
                {
                    case "Create Doctor's account":
                        Admin.CreateDoctor();
                        break;
                    case "Create Receptionist's account":
                        Admin.CreateReceptionist();
                        break;
                    case "Delete a user":
                        Admin.DeleteUser();
                        break;
                    case "Logout":
                        running = false;
                        AnsiConsole.MarkupLine("[green]Logging out...[/]");
                        break;
                }
            }
        }

    }
}
