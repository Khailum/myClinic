using System;
using Microsoft.Data.SqlClient;
using myClinic;

namespace MyApp
{
    internal class Program
    {
        public static string ConnectionString = @"Data Source=DESKTOP-O7AMP6F;Initial Catalog=PMSmyClinic;Integrated Security=True;Trust Server Certificate=True;encrypt=false";

        static void Main(string[] args)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    Console.WriteLine("---- Welcome to myClinic! ----");

                    HandleUserLogin();
                }
            }
            catch (SqlException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"An error occurred while connecting to the database: {ex.Message}");
                Console.ResetColor();
            }
        }

        static void HandleUserLogin()
        {
            bool validLogin = false;

            while (!validLogin)
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
                            GreetUser("Receptionist");
                            Login.LoginRec(); 
                            validLogin = true;
                            break;
                        case 2:
                            GreetUser("Doctor");
                            new Login().LoginDoctor();
                            validLogin = true;
                            break;
                        case 3:
                            GreetUser("Admin");
                            Login.LoginAdmin();
                            validLogin = true;
                            break;
                        default:
                            InvalidChoice();
                            break;
                    }
                }
                else
                {
                    InvalidChoice();
                }
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
