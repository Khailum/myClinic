using System;
using Microsoft.Data.SqlClient;

namespace myClinic
{
    public class Login
    {
       public  static string connectionString = @"Data Source=DESKTOP-O7AMP6F;Initial Catalog=PMSmyClinic;Integrated Security=True;Trust Server Certificate=FALSE;encrypt=false";

        public static void LoginUser(string role)
        {
            bool repeat = true;

            using SqlConnection connection = new SqlConnection(connectionString);
            {
                connection.Open();

                while (repeat)
                {
                    Console.WriteLine($"Enter your {role} Username:");
                    string username = Console.ReadLine();

                    Console.WriteLine($"Enter your {role} Password:");
                    string password = Console.ReadLine();

                    //Hash the password before checking it
                    string hashedPassword = Password.HashPassword(password);

                    string checkDetailsQuery = @"SELECT COUNT(*) FROM Users WHERE Username = @username AND PasswordHash = @password AND Role = @role";

                    using SqlCommand cmd = new SqlCommand(checkDetailsQuery, connection);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", hashedPassword); 
                    cmd.Parameters.AddWithValue("@role", role);


                    int result = (int)cmd.ExecuteScalar();
                    Console.WriteLine(result);

                    if (result > 0)
                    {
                        Console.WriteLine("\nYou are now logged in.\n");
                        repeat = false;

                        // After successful login, go to their menu
                        if (role == "Receptionist")
                        {
                            ReceptionistMenu();
                        }
                        else if (role == "Doctor")
                        {
                            Doctor.ShowDoctorMenu();
                        }
                        else if (role == "Admin")
                        {
                            AdminMenu();
                        }
                    }
                    else
                    {
                        Console.WriteLine("\nINCORRECT USERNAME OR PASSWORD.");
                        Console.WriteLine("1. Try Again");
                        Console.WriteLine("2. Exit");

                        if (int.TryParse(Console.ReadLine(), out int choice))
                        {
                            if (choice == 1)
                            {
                                repeat = true;
                            }
                            else if (choice == 2)
                            {
                                Environment.Exit(0);
                            }
                            else
                            {
                                Console.WriteLine("Invalid choice. Trying again...");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Invalid input. Trying again...");
                        }
                    }
                }
            }
        }

        public void LoginDoctor()
        {
            LoginUser("Doctor");
        }

        public static void LoginRec()
        {
            LoginUser("Receptionist");
        }

        public static void LoginAdmin()
        {
            LoginUser("Admin");
        }

        private static void ReceptionistMenu()
        {
            bool running = true;
            while (running)
            {
                Console.Clear();
                Console.WriteLine("---- Receptionist Menu ----");
                Console.WriteLine("1. Register New Patient");
                Console.WriteLine("2. Book Appointment");
                Console.WriteLine("3. View Appointments");
                Console.WriteLine("4. Logout");
                Console.Write("Select an option: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.WriteLine("Registering new patient...");
                        Receptionist.RegisterPatient();
                        break;
                    case "2":
                        Console.WriteLine("Booking appointment...");
                        Receptionist.BookAppointment();
                        break;
                    case "3":
                        Console.WriteLine("Viewing Appointments...");
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
        private static void AdminMenu()
        {
            bool running = true;
            while (running)
            {
                Console.Clear();
                Console.WriteLine("---- Admin Menu ----");
                Console.WriteLine("1. Create Doctor's account");
                Console.WriteLine("2. Create Receptionist's account");
                Console.WriteLine("3. Delete a user");
                Console.WriteLine("4. View today's appointments");
                Console.WriteLine("5. Logout");
                Console.Write("Select an option: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.WriteLine("Creating Doctor's account...");
                        Admin.CreateDoctor();
                        break;
                    case "2":
                        Console.WriteLine("Creating Receptionist's account...");
                        Admin.CreateReceptionist();
                        break;
                    case "3":
                        Console.WriteLine("Delete a user...");
                        Admin.DeleteUser();
                        break;
                    case "4":
                        Console.WriteLine("Viewing Today's appointments");
                        Admin.ViewAppointmentsForToday();
                        break;
                    case "5":
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
