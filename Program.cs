using System;
using Microsoft.Data.SqlClient;

namespace MyApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string connectionString = @"Data Source=DESKTOP-O7AMP6F;Initial Catalog=myClinicPMS;Integrated Security=True;Trust Server Certificate=True;encrypt=false";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    Console.WriteLine("----Welcome to myClinic!----");

                    Console.WriteLine("Hi!\nAre you signing in as:\n1. Receptionist\n2. Doctor\n3. Admin?");
                    string input = Console.ReadLine();
                    if (int.TryParse(input, out int position))
                    {
                        switch (position)
                        {
                            case 1:
                                Console.WriteLine("Hi, welcome back Receptionist!");
                                break;
                            case 2:
                                Console.WriteLine("Hi, welcome back Doctor!");
                                break;
                            case 3:
                                Console.WriteLine("Hi, welcome back Admin!");
                                break;
                            default:
                                Console.WriteLine("Invalid choice. Please enter 1, 2, or 3.");
                                break;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid input. Please enter a number.");
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"An error occurred while connecting to the database: {ex.Message}");
            }
        }
    }
}
