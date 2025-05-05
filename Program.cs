using System;
using Microsoft.Data.SqlClient;
using Spectre.Console;
using myClinic;

namespace MyApp
{
    public static class Program
    {
        private static string ConnectionString = @"Data Source=ACADEMICWEAPON;Initial Catalog=PMSmyClinic;Integrated Security=True;Trust Server Certificate=True;Encrypt=False";

        static void Main(string[] args)
        {
            // Display the logo
            Logo.Display();

            // Show loading screen while connecting to the database
            Logo.ShowLoading();

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    AnsiConsole.MarkupLine("[green]Database connected successfully![/]");
                }

                Admin.Initialize(ConnectionString);

                bool running = true;

                while (running)
                {
                    HandleUserLogin();

                    var choice = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("\n[bold yellow]Would you like to:[/]")
                            .PageSize(5)
                            .HighlightStyle("green")
                            .AddChoices(new[]
                            {
                            "Return to login screen",
                            "Exit the application"
                                            }));

                    if (choice.Equals("Exit the application", StringComparison.OrdinalIgnoreCase))
                    {
                        running = false;
                        AnsiConsole.MarkupLine("[green]Goodbye![/]");
                    }

                }

                Console.Clear();
            }
            catch (SqlException ex)
            {
                AnsiConsole.MarkupLine($"[red]An error occurred while connecting to the database: {ex.Message}[/]");
                Environment.Exit(0); // Exit if database is unavailable
            }
        }

        public static void HandleUserLogin()
        {
            Logo.Display();

            AnsiConsole.MarkupLine("[cyan]---- Welcome to myClinic! ----[/]");
            Console.ResetColor();

            bool validLogin = false;
            int attempts = 0;

            while (!validLogin && attempts < 3)
            {
                var role = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("\n[bold]Hi![/] [green]Who are you signing in as?[/]")
                        .PageSize(10)
                        .AddChoices(new[]
                        {
                            "Receptionist",
                            "Doctor",
                            "Admin"
                        }));

                Console.Clear();
                Logo.Display();
                GreetUser(role);

                switch (role)
                {
                    case "Receptionist":
                        Login.LoginRec();
                        validLogin = true;
                        break;
                    case "Doctor":
                        Login.LoginDoctor();
                        validLogin = true;
                        break;
                    case "Admin":
                        Login.LoginAdmin();
                        validLogin = true;
                        break;
                    default:
                        InvalidChoice();
                        attempts++;
                        break;
                }

                if (!validLogin)
                {
                    attempts++;
                }
            }

            if (!validLogin)
            {
                AnsiConsole.MarkupLine("[red]Too many failed attempts. Exiting...[/]");
                Environment.Exit(0);
            }
        }

        static void GreetUser(string role)
        {
            AnsiConsole.MarkupLine($"[cyan]\nHi, welcome back {role}![/]");
        }

        static void InvalidChoice()
        {
            AnsiConsole.MarkupLine("[red]\nInvalid choice. Please enter 1, 2, or 3.[/]");
        }
    }
}
