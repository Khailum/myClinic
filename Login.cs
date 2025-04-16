using System;
using Microsoft.Data.SqlClient;
using Spectre.Console;

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
                Console.WriteLine($"Enter your {role} Username:");
                string username = Console.ReadLine();

                Console.WriteLine($"Enter your {role} Password:");
                string password = Console.ReadLine();

                if (ValidateCredentials(username, password, role, connection))
                {
                    Console.WriteLine("\nYou are now logged in.\n");
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
                                Console.WriteLine("Doctor initialization failed. Please check if the account is linked properly.");
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


        private static int FetchDoctorIdFromUsername(string username, SqlConnection connection)
        {
            string query = "SELECT DoctorID FROM Users WHERE Username = @Username AND Role = 'Doctor';";

            using var cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@Username", username);

            var result = cmd.ExecuteScalar();

            if (result != null)
            {
                return (int)result;
            }

            return -1; // Return -1 if no DoctorID found
        }

        private static bool ValidateCredentials(string username, string password, string role, SqlConnection connection)
        {
            string query = @"
                SELECT COUNT(*) 
                FROM Users 
                WHERE Username = @username AND PasswordHash = @password AND Role = @role";

            using SqlCommand cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@password", password);
            cmd.Parameters.AddWithValue("@role", role);

            return (int)cmd.ExecuteScalar() > 0;
        }

        private static int FetchDoctorId(string username)
        {
            string query = @"
                SELECT sd.DoctorID
                FROM StaffDoctors sd
                INNER JOIN Users u ON u.Username = @username
                WHERE u.Username = @username";

            using SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            using SqlCommand cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@username", username);

            object result = cmd.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : -1;
        }

        private static void HandleLoginFailure()
        {
            Console.WriteLine("\nIncorrect username or password.");
            Console.WriteLine("1. Try Again");
            Console.WriteLine("2. Exit");

            if (int.TryParse(Console.ReadLine(), out int choice) && choice == 2)
            {
                Console.WriteLine("Exiting... Goodbye!");
                Environment.Exit(0);
            }
        }

        private static void ShowMenu(string role)
        {
            Logo.ShowLoading();
            Logo.Display();
            Console.WriteLine($"---- {role} Menu ----");
        }

        private static void ShowReceptionistMenu()
        {
            bool running = true;

            while (running)
            {
                Console.Clear();
                Logo.Display();
                AnsiConsole.Write(new Rule("[Purple]===Receptionist Menu===[/]").RuleStyle("purple").Centered());
                Console.WriteLine("1. Register New Patient");
                Console.WriteLine("2. Book Appointment");
                Console.WriteLine("3. View Appointments");
                Console.WriteLine("4. Logout");
                Console.Write("Select an option: ");

                switch (Console.ReadLine())
                {
                    case "1":
                        Receptionist.RegisterPatient();
                        break;
                    case "2":
                        Receptionist.BookAppointment();
                        break;
                    case "3":
                        Receptionist.ViewAppointment();
                        break;
                    case "4":
                        running = false;
                        Console.WriteLine("Logging out...");
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
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
                AnsiConsole.Write(new Rule("[White]===Admin Menu===[/]").RuleStyle("White").Centered());
                Console.WriteLine("1. Create Doctor's account");
                Console.WriteLine("2. Create Receptionist's account");
                Console.WriteLine("3. Delete a user");
                Console.WriteLine("4. Logout");
                Console.Write("Select an option: ");

                switch (Console.ReadLine())
                {
                    case "1":
                        Admin.CreateDoctor();
                        break;
                    case "2":
                        Admin.CreateReceptionist();
                        break;
                    case "3":
                        Admin.DeleteUser();
                        break;
                    case "4":
                        running = false;
                        Console.WriteLine("Logging out...");
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }
    }
}
