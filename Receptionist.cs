using System;
using System.Data;
using System.Threading;
using Microsoft.Data.SqlClient;
using Spectre.Console;

namespace myClinic
{
    public class Receptionist
    {
        private static readonly string ConnectionString = @"Data Source=ACADEMICWEAPON;Initial Catalog=PMSmyClinic;Integrated Security=True;Trust Server Certificate=True;Encrypt=False";

        public static void RegisterPatient()
        {
            using SqlConnection connection = new SqlConnection(ConnectionString);
            try
            {
                connection.Open();

                AnsiConsole.MarkupLine("[green]Enter Patient First name:[/]");
                string firstName = Console.ReadLine()?.Trim();

                AnsiConsole.MarkupLine("[green]Enter Patient Last name:[/]");
                string lastName = Console.ReadLine()?.Trim();

                AnsiConsole.MarkupLine("[green]Enter Patient Age:[/]");
                if (!int.TryParse(Console.ReadLine(), out int age) || age <= 0)
                {
                    AnsiConsole.MarkupLine("[red]Invalid age input.[/]");
                    return;
                }

                AnsiConsole.MarkupLine("[green]Enter Patient Gender:[/]");
                string gender = Console.ReadLine()?.Trim();

                string idNumber = "";
                while (true)
                {
                    AnsiConsole.MarkupLine("[green]Enter Patient ID number (13 digits):[/]");
                    idNumber = Console.ReadLine()?.Trim();
                    if (!string.IsNullOrEmpty(idNumber) && idNumber.Length == 13 && long.TryParse(idNumber, out _))
                        break;
                    AnsiConsole.MarkupLine("[red]Invalid ID number. Please try again.[/]");
                }

                string insertPatientQuery = @"INSERT INTO Patients (FirstName, LastName, DateOfBirth, Gender, IDNumber)
                                              VALUES (@FirstName, @LastName, @DateOfBirth, @Gender, @IDNumber)";

                using SqlCommand cmd = new SqlCommand(insertPatientQuery, connection);
                cmd.Parameters.AddWithValue("@FirstName", firstName);
                cmd.Parameters.AddWithValue("@LastName", lastName);
                cmd.Parameters.AddWithValue("@DateOfBirth", DateTime.Today.AddYears(-age));
                cmd.Parameters.AddWithValue("@Gender", gender);
                cmd.Parameters.AddWithValue("@IDNumber", idNumber);
                cmd.ExecuteNonQuery();

                AnsiConsole.MarkupLine("[green]Patient registered successfully.[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            }
            AnsiConsole.MarkupLine("[green]Press any key to return to the main menu[/]");
            Console.ReadKey();
        }

        public static void BookAppointment()
        {
            while (true)
            {
                using SqlConnection connection = new SqlConnection(ConnectionString);
                try
                {
                    connection.Open();

                    AnsiConsole.MarkupLine("[green]Here are all our doctors:[/]");
                    string doctorQuery = "SELECT * FROM StaffDoctors";
                    using SqlCommand cmd = new SqlCommand(doctorQuery, connection);
                    using SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        AnsiConsole.MarkupLine($"[green]DoctorID: {reader.GetInt32(0)}, Name: {reader.GetString(1)} {reader.GetString(2)}, Specialty: {reader.GetString(3)}[/]");
                    }
                    reader.Close();

                    AnsiConsole.MarkupLine("[green]Enter Patient ID number:[/]");
                    string patientId = Console.ReadLine()?.Trim();

                    string checkPatientQuery = "SELECT COUNT(*) FROM Patients WHERE IDNumber = @IDNumber";
                    using SqlCommand checkCmd = new SqlCommand(checkPatientQuery, connection);
                    checkCmd.Parameters.AddWithValue("@IDNumber", patientId);
                    if ((int)checkCmd.ExecuteScalar() == 0)
                    {
                        AnsiConsole.MarkupLine("[red]Patient not found. Try again.[/]");
                        continue;
                    }

                    AnsiConsole.MarkupLine("[green]Enter reason for the appointment:[/]");
                    string reason = Console.ReadLine();

                    AnsiConsole.MarkupLine("[green]Enter Doctor ID:[/]");
                    string doctorId = Console.ReadLine();

                    AnsiConsole.MarkupLine("[green]Enter appointment date and time (YYYY-MM-DD HH:MM:SS):[/]");
                    if (!DateTime.TryParse(Console.ReadLine(), out DateTime appointmentDate) || appointmentDate <= DateTime.Now)
                    {
                        AnsiConsole.MarkupLine("[red]Invalid or past date entered.[/]");
                        continue;
                    }

                    string checkDateQuery = "SELECT COUNT(*) FROM Appointments WHERE AppointmentDate = @Date AND DoctorID = @DoctorID";
                    using SqlCommand dateCmd = new SqlCommand(checkDateQuery, connection);
                    dateCmd.Parameters.AddWithValue("@Date", appointmentDate);
                    dateCmd.Parameters.AddWithValue("@DoctorID", doctorId);
                    if ((int)dateCmd.ExecuteScalar() > 0)
                    {
                        AnsiConsole.MarkupLine("[red]The chosen date and time is already occupied. Please choose another.[/]");
                        Thread.Sleep(2500);
                        Console.Clear();
                        continue;
                    }

                    string insertAppointmentQuery = @"INSERT INTO Appointments (PatientIDNumber, DoctorID, AppointmentDate, Status)
                                                      VALUES (@IDNumber, @DoctorID, @AppointmentDate, 'Scheduled')";
                    using SqlCommand insertCmd = new SqlCommand(insertAppointmentQuery, connection);
                    insertCmd.Parameters.AddWithValue("@IDNumber", patientId);
                    insertCmd.Parameters.AddWithValue("@DoctorID", doctorId);
                    insertCmd.Parameters.AddWithValue("@AppointmentDate", appointmentDate);
                    insertCmd.ExecuteNonQuery();

                    AnsiConsole.MarkupLine("[green]Appointment booked successfully.[/]");
                    Console.WriteLine("\n\n=========================================");
                    AnsiConsole.MarkupLine("[green]Press any key to return to the main menu[/]");
                    Console.ReadKey();
                    break;
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
                }
                Console.WriteLine("\n\n=========================================");
                AnsiConsole.MarkupLine("[green]Press any key to return to the main menu[/]");
                Console.ReadKey();
            }
        }

        public static void ViewAppointment()
        {
            using SqlConnection connection = new SqlConnection(ConnectionString);
            connection.Open();

            // Create a SelectionPrompt for the menu options
            var menuPrompt = new SelectionPrompt<string>()
                .Title("[green]Select an option[/]")
                .AddChoices(
                    "View all appointments",
                    "View appointments for a specific doctor",
                    "View appointments for a specific patient",
                    "Exit"
                );

            // Get the selected choice using arrow keys
            string choice = AnsiConsole.Prompt(menuPrompt);

            string query = choice switch
            {
                "View all appointments" => "SELECT * FROM Appointments",
                "View appointments for a specific doctor" => "SELECT * FROM Appointments WHERE DoctorID = @ID",
                "View appointments for a specific patient" => "SELECT * FROM Appointments WHERE PatientIDNumber = @ID",
                _ => null
            };

            if (query == null) return;

            using SqlCommand cmd = new SqlCommand(query, connection);

            if (choice == "View appointments for a specific doctor")
            {
                AnsiConsole.MarkupLine("[green]Enter Doctor ID:[/]");
                cmd.Parameters.AddWithValue("@ID", int.Parse(Console.ReadLine()));
            }
            else if (choice == "View appointments for a specific patient")
            {
                AnsiConsole.MarkupLine("[green]Enter Patient ID Number:[/]");
                cmd.Parameters.AddWithValue("@ID", Console.ReadLine());
            }

            using SqlDataReader reader = cmd.ExecuteReader();

            Console.Clear();
            AnsiConsole.MarkupLine("[green]Appointments:[/]");

            while (reader.Read())
            {
                AnsiConsole.MarkupLine($"[green]AppointmentID: {reader.GetInt32(0)}\nPatientID: {reader.GetString(1)}\nDoctorID: {reader.GetInt32(2)}\nAppointmentDate: {reader.GetDateTime(3)}\nStatus: {reader.GetString(4)}[/]");
            }

            Console.WriteLine("\n\n=========================================");
            AnsiConsole.MarkupLine("[green]Press any key to return to the main menu[/]");
            Console.ReadKey();
        }

        public static void UpdateAppointment()
        {
            using SqlConnection connection = new SqlConnection(ConnectionString);
            try
            {
                connection.Open();

                AnsiConsole.MarkupLine("[green]Enter Appointment ID to update:[/]");
                if (!int.TryParse(Console.ReadLine(), out int appointmentId))
                {
                    AnsiConsole.MarkupLine("[red]Invalid Appointment ID.[/]");
                    return;
                }

                string checkQuery = "SELECT COUNT(*) FROM Appointments WHERE AppointmentID = @AppointmentID";
                using SqlCommand checkCmd = new SqlCommand(checkQuery, connection);
                checkCmd.Parameters.AddWithValue("@AppointmentID", appointmentId);
                if ((int)checkCmd.ExecuteScalar() == 0)
                {
                    AnsiConsole.MarkupLine("[red]Appointment not found.[/]");
                    return;
                }

                AnsiConsole.MarkupLine("[green]Enter new appointment date and time (YYYY-MM-DD HH:MM:SS):[/]");
                if (!DateTime.TryParse(Console.ReadLine(), out DateTime newDate) || newDate <= DateTime.Now)
                {
                    AnsiConsole.MarkupLine("[red]Invalid or past date entered.[/]");
                    return;
                }

                AnsiConsole.MarkupLine("[green]Enter new status (Scheduled, Completed, Cancelled):[/]");
                string newStatus = Console.ReadLine();

                string updateQuery = "UPDATE Appointments SET AppointmentDate = @NewDate, Status = @Status WHERE AppointmentID = @AppointmentID";
                using SqlCommand updateCmd = new SqlCommand(updateQuery, connection);
                updateCmd.Parameters.AddWithValue("@NewDate", newDate);
                updateCmd.Parameters.AddWithValue("@Status", newStatus);
                updateCmd.Parameters.AddWithValue("@AppointmentID", appointmentId);

                int affected = updateCmd.ExecuteNonQuery();
                AnsiConsole.MarkupLine(affected > 0 ? "[green]Appointment updated successfully.[/]" : "[red]No changes made.[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            }
            Console.WriteLine("\n\n=========================================");
            AnsiConsole.MarkupLine("[green]Press any key to return to the main menu[/]");
            Console.ReadKey();
        }
    }
}
