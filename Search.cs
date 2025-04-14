using System;
using Microsoft.Data.SqlClient;

namespace myClinic
{
    public class Search
    {
        private readonly SqlConnection _connection;

        public Search(SqlConnection connection)
        {
            _connection = connection;
        }

        public void StartSearch()
        {
            Console.Clear();
            Console.WriteLine("=== Patient/Doctor Search ===");
            Console.WriteLine("Search by:");
            Console.WriteLine("1. First Name");
            Console.WriteLine("2. Last Name");
            Console.WriteLine("3. Patient/Doctor ID");

            string choice = GetValidChoice();

            switch (choice)
            {
                case "1":
                    SearchByField("FirstName");
                    break;
                case "2":
                    SearchByField("LastName");
                    break;
                case "3":
                    SearchById();
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nInvalid choice. Please select 1, 2, or 3.");
                    Console.ResetColor();
                    break;
            }
        }

        private string GetValidChoice()
        {
            string choice = null;
            int attempts = 0;

            while (string.IsNullOrWhiteSpace(choice) && attempts < 3)
            {
                Console.Write("Select an option (1-3): ");
                choice = Console.ReadLine()?.Trim();
                if (string.IsNullOrWhiteSpace(choice) || !IsValidChoice(choice))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid input. Please enter 1, 2, or 3.");
                    Console.ResetColor();
                    attempts++;
                }
            }

            if (attempts == 3)
            {
                Console.WriteLine("Too many invalid attempts. Returning to main menu...");
                choice = "exit";
            }

            return choice;
        }

        private bool IsValidChoice(string choice)
        {
            return choice == "1" || choice == "2" || choice == "3";
        }

        private void SearchByField(string fieldName)
        {
            Console.Write($"\nEnter part of the {fieldName}: ");
            string searchTerm = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                Console.WriteLine("Search term cannot be empty.");
                return;
            }

            string query = $"SELECT * FROM Patients WHERE {fieldName} LIKE @SearchTerm";

            try
            {
                using (SqlCommand cmd = new SqlCommand(query, _connection))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            Console.WriteLine("\nResults:");
                            Console.WriteLine("--------------------------------------------");
                            while (reader.Read())
                            {
                                int patientId = reader.GetInt32(reader.GetOrdinal("PatientID"));
                                string firstName = reader.GetString(reader.GetOrdinal("FirstName"));
                                string lastName = reader.GetString(reader.GetOrdinal("LastName"));
                                DateTime dob = reader.GetDateTime(reader.GetOrdinal("DateOfBirth"));
                                string gender = reader.GetString(reader.GetOrdinal("Gender"));

                                Console.WriteLine($"ID: {patientId} | Name: {firstName} {lastName} | DOB: {dob:yyyy-MM-dd} | Gender: {gender}");
                            }
                            Console.WriteLine("--------------------------------------------");
                        }
                        else
                        {
                            Console.WriteLine("\nNo matching records found.");
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"An error occurred during the search: {ex.Message}");
                Console.ResetColor();
            }
        }

        private void SearchById()
        {
            Console.Write("\nEnter Patient/Doctor ID: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid ID entered.");
                return;
            }

            string query = "SELECT * FROM Patients WHERE PatientID = @ID";

            try
            {
                using (SqlCommand cmd = new SqlCommand(query, _connection))
                {
                    cmd.Parameters.AddWithValue("@ID", id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            Console.WriteLine("\nResults:");
                            Console.WriteLine("--------------------------------------------");
                            while (reader.Read())
                            {
                                int patientId = reader.GetInt32(reader.GetOrdinal("PatientID"));
                                string firstName = reader.GetString(reader.GetOrdinal("FirstName"));
                                string lastName = reader.GetString(reader.GetOrdinal("LastName"));
                                DateTime dob = reader.GetDateTime(reader.GetOrdinal("DateOfBirth"));
                                string gender = reader.GetString(reader.GetOrdinal("Gender"));

                                Console.WriteLine($"ID: {patientId} | Name: {firstName} {lastName} | DOB: {dob:yyyy-MM-dd} | Gender: {gender}");
                            }
                            Console.WriteLine("--------------------------------------------");
                        }
                        else
                        {
                            Console.WriteLine("\nNo matching record found.");
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"An error occurred during the search: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}
