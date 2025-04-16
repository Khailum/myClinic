using System;
using Microsoft.Data.SqlClient;
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
                }

                Admin.Initialize(ConnectionString);

                bool running = true;

                while (running)
                {
                    HandleUserLogin();

                    Console.WriteLine("\nWould you like to:");
                    Console.WriteLine("1. Return to login screen");
                    Console.WriteLine("2. Exit the application");
                    Console.Write("Enter your choice: ");
                    string input = Console.ReadLine();

                    if (input != "1")
                    {
                        running = false;
                        Console.WriteLine("Goodbye!");
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"An error occurred while connecting to the database: {ex.Message}");
                Console.ResetColor();
                Environment.Exit(0); // Exit if database is unavailable
            }
        }

        public static void HandleUserLogin()
        {
            Logo.Display();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("---- Welcome to myClinic! ----");
            Console.ResetColor();
            bool validLogin = false;
            int attempts = 0;

            while (!validLogin && attempts < 3)
            {
                Console.WriteLine("\nHi!\nAre you signing in as:");
                Console.WriteLine("1. Receptionist");
                Console.WriteLine("2. Doctor");
                Console.WriteLine("3. Admin");
                Console.Write("Please enter your choice (1, 2, or 3): ");

                string input = Console.ReadLine();
                if (int.TryParse(input, out int position))
                {
                    switch (position)
                    {
                        case 1:
                            Console.Clear();
                            Logo.Display();
                            GreetUser("Receptionist");
                            Login.LoginRec();
                            validLogin = true;
                            break;
                        case 2:
                            Console.Clear();
                            Logo.Display();
                            GreetUser("Doctor");
                            Login.LoginDoctor();
                            validLogin = true;
                            break;
                        case 3:
                            Console.Clear();
                            Logo.Display();
                            GreetUser("Admin");
                            Login.LoginAdmin();
                            validLogin = true;
                            break;
                        default:
                            InvalidChoice();
                            attempts++;
                            break;
                    }
                }
                else
                {
                    InvalidChoice();
                    attempts++;
                }
            }

            if (!validLogin)
            {
                Console.WriteLine("Too many failed attempts. Exiting...");
                Environment.Exit(0);
            }
        }

        static void GreetUser(string role)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\nHi, welcome back {role}!");
            Console.ResetColor();
        }

        static void InvalidChoice()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\nInvalid choice. Please enter 1, 2, or 3.");
            Console.ResetColor();
        }
    }
}
