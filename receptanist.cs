using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Azure;
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;

namespace Myclinic
{
    public static class receptanist
    {
        public static string connection = @"Data Source=DESKTOP-H1DSHCI;Initial Catalog=Myclinicc;Integrated Security=True;Encrypt=False";
        public static bool repeat = true;
        public static bool repeat1 = true;
        public static int AppoitmentID;
        public static string ID;

        public static void RegisterPatinet()
        {
            using SqlConnection Connection = new SqlConnection(connection);
            bool repeat = true;
            while (repeat)
            try
            {              
                Connection.Open();
                Console.WriteLine("Enter Patient First name");
                string PFname = Console.ReadLine();
                Console.WriteLine("Enter Patient Last name");
                string PLname = Console.ReadLine();
                Console.WriteLine("Patient Age");
                int Age = int.Parse(Console.ReadLine());
                Console.WriteLine("Patient Gender");
                string gender = Console.ReadLine();
                    while (repeat)
                    {
                        Console.WriteLine("Patient ID number");
                        ID = Console.ReadLine();
                        if (ID.Length != 13)
                        {
                            Console.WriteLine("Invalid Identity number");
                        }

                        else
                        {
                            string InsertPatient = "Insert into Patients(Firstname,Lastname,Age,Gender,IDnumber)Values(@Firstname,@Lastname,@Age,@Gender,@IDnumber)";
                            SqlCommand cmd = new SqlCommand(InsertPatient, Connection);
                            cmd.Parameters.AddWithValue("@Firstname", PFname);
                            cmd.Parameters.AddWithValue("@Lastname", PLname);
                            cmd.Parameters.AddWithValue("@Age", Age);
                            cmd.Parameters.AddWithValue("@gender", gender);
                            cmd.Parameters.AddWithValue("@IDnumber", ID);
                            cmd.ExecuteNonQuery();
                            Connection.Close();
                        }
                    }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            //Show Patinet ID//
        }
        public static void BookAppointment()
        {
            bool repeat = true;
            while (repeat)
            {
                using SqlConnection Connection = new SqlConnection(connection);
                try
                {
                    Console.WriteLine("Here Are All our Doctors");
                    Connection.Open();
                    //Console.WriteLine("Enter Doctors ID");
                    //int DoctorsID = int.Parse(Console.ReadLine());
                    //Console.Clear();
                    string DoctorsAppoint = "select * from Doctors";
                    SqlCommand commamd = new SqlCommand(DoctorsAppoint, Connection);                   
                    SqlDataReader reader1 = commamd.ExecuteReader();


                    while (reader1.Read())
                    {
                        int DoctorID = reader1.GetInt32(0);
                        string FirstName = reader1.GetString(1);
                        string LastName = reader1.GetString(2);
                        string Speciality = reader1.GetString(3);                                              
                        Console.WriteLine($"DoctorID:{DoctorID} FirstName:{FirstName} LastName:{LastName} Speciality:{Speciality}");
                        Console.WriteLine("");
                        repeat = false;
                    }


                    reader1.Close();
                    Console.WriteLine("What is the Patient ID number");
                    string PatientID = Console.ReadLine();
                    string PatientCount = "select count (*) from Patients where IDnumber=@IDnumber";
                    SqlCommand Pcount = new SqlCommand(PatientCount, Connection);
                    Pcount.Parameters.AddWithValue("@IDnumber", PatientID);
                    int count=(int)Pcount.ExecuteScalar();

                    if (count > 0)
                    {
                        Console.WriteLine("The reason for the appoitment");
                        string Reason = Console.ReadLine();
                        Console.WriteLine("Doctor to be assigned (DoctorID)");
                        string DoctorID = Console.ReadLine();
                        Console.WriteLine("Date for the appointment and time (YYYY/MM/DD HH:MM:SS)");
                        DateTime Date = DateTime.Parse(Console.ReadLine());
                        if (Date <= DateTime.Now)
                        {
                            Console.WriteLine("You cannot book an appointment in past");
                        }
                        else
                        {


                            Console.WriteLine("");
                            string Datecount = "select count(*) from Appointments where AppointmentDateTime=@Date and DoctorID=@DoctorID";
                            SqlCommand cmd = new SqlCommand(Datecount, Connection);
                            cmd.Parameters.AddWithValue("@Date", Date);
                            cmd.Parameters.AddWithValue("@DoctorID", DoctorID);
                            int result3 = (int)cmd.ExecuteScalar();
                            Console.WriteLine(result3);

                            if (result3 > 0)
                            {
                                Console.WriteLine("The Date has been already occupeid.Please choose another date. Try Another Date");
                                Thread.Sleep(2500);
                                Console.Clear();
                                repeat = true;
                            }

                            else
                            {
                                string update = "insert into Appointments values(@IDnumber,@DoctorID,@Reason,@AppointmentDateTime,@status)";
                                SqlCommand Comd = new SqlCommand(update, Connection);
                                Comd.Parameters.AddWithValue("@IDnumber", PatientID);
                                Comd.Parameters.AddWithValue("@DoctorID", DoctorID);
                                Comd.Parameters.AddWithValue("@Reason", Reason);
                                Comd.Parameters.AddWithValue("@AppointmentDateTime", Date);
                                Comd.Parameters.AddWithValue("@Status", "Scheduled");
                                Comd.ExecuteNonQuery();
                                Console.WriteLine("Appointment successful");
                                repeat = false;

                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Patinet ID number is Incorrect.Try Again");
                        repeat = true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    repeat = true;
                }
            }
        }
        public static void ViewAppointment()
        {
            using SqlConnection Connection = new SqlConnection(connection);
            Connection.Open();
            Console.WriteLine("1.View all Appointments \n 2.View Specific Doctor appointments \n3.View Specific Patient Appoitments\n4.Exit");
            int view = int.Parse(Console.ReadLine());
            switch (view)
            {

                case 1:
                    string Viewall = "Select * from Appointments";                                   
                    SqlCommand Cmd = new SqlCommand(Viewall, Connection);
                    SqlDataReader reader = Cmd.ExecuteReader();
                    Console.WriteLine("Wait While We are get your Statement");                 
                    Console.Clear();
                    while (reader.Read())
                    {
                        int AppointmentID = reader.GetInt32(0);
                        string IDnumber = reader.GetString(1);
                        int DoctorID = reader.GetInt32(2);
                        string Reason = reader.GetString(3);
                        DateTime AppointmentDate =reader.GetDateTime(4);
                        string Status = reader.IsDBNull(5) ? "N/A" : reader.GetString(5);
                        Console.WriteLine($"AppointmentID:{AppointmentID}\nIDnumber:{IDnumber}\nDoctorID:{DoctorID}\nReason:{Reason}\nAppointmentDate:{AppointmentDate}\nStatus:{Status}");
                        Console.WriteLine("");
                    }
                    break;
                case 2:
                    Console.WriteLine("Enter Doctors ID");
                    int DoctorsID=int.Parse(Console.ReadLine());
                    Console.Clear();
                    string DoctorsAppoint = "select * from Appointments where DoctorID=@DoctorsID";
                    SqlCommand cmd = new SqlCommand(DoctorsAppoint, Connection);
                    cmd.Parameters.AddWithValue("@DoctorsID",DoctorsID);
                    SqlDataReader reader2 = cmd.ExecuteReader();
                    while (reader2.Read())
                    {
                        int AppointmentID = reader2.GetInt32(0);
                        string IDnumber = reader2.GetString(1);
                        int DoctorID = reader2.GetInt32(2);
                        string Reason = reader2.GetString(3);
                        DateTime AppointmentDate = reader2.GetDateTime(4);
                        string Status = reader2.IsDBNull(5) ? "N/A" : reader2.GetString(5);
                        Console.WriteLine($"AppointmentID:{AppointmentID}\nIDnumber:{IDnumber}\nDoctorID:{DoctorID}\nReason:{Reason}\nAppointmentDate:{AppointmentDate}\nStatus:{Status}");
                        Console.WriteLine("");
                    }

                    break;
                case 3:
                    Console.WriteLine("Enter Patient IDnumber");
                    string PatientID =Console.ReadLine();
                    Console.Clear();
                    string PatientAppoint = "select * from Appointments where IDnumber=@PatientID";
                    SqlCommand comd = new SqlCommand(PatientAppoint, Connection);
                    comd.Parameters.AddWithValue("@PatientID", PatientID);
                    SqlDataReader reader3 = comd.ExecuteReader();
                    while (reader3.Read())
                    {
                        int AppointmentID = reader3.GetInt32(0);
                        string IDnumber = reader3.GetString(1);
                        int DoctorID = reader3.GetInt32(2);
                        string Reason = reader3.GetString(3);
                        DateTime AppointmentDate = reader3.GetDateTime(4);
                        string Status = reader3.IsDBNull(5) ? "N/A" : reader3.GetString(5);
                        Console.WriteLine($"AppointmentID:{AppointmentID}\nIDnumber:{PatientID}\nDoctorID:{DoctorID}\nReason:{Reason}\nAppointmentDate:{AppointmentDate}\nStatus:{Status}");
                        Console.WriteLine("");
                    }
                    break;
                case 4:
                    Environment.Exit(0);
                    break;

            }

        }
        public static void UpdateAp()
        {
            using SqlConnection Connection = new SqlConnection(connection);
            {
                try
                {

                    Connection.Open();
                    while (repeat1)
                    {
                        Console.WriteLine("Enter Appoitment ID you want to update");
                        AppoitmentID = int.Parse(Console.ReadLine());

                        string IDcount = "select count(*) from Appointments where AppointmentID=@AppointmentID";
                        SqlCommand command = new SqlCommand(IDcount, Connection);
                        command.Parameters.AddWithValue("@AppointmentID", AppoitmentID);
                        int result = (int)command.ExecuteScalar();
                        Console.WriteLine(result);
                        if (result > 0)
                        {
                            Console.WriteLine("What do you want to Update?\n1.AppoitmentDateandTime");
                            int UPanswer = int.Parse(Console.ReadLine());

                            switch (UPanswer)
                            {
                                case 1:
                                    while (repeat)
                                    {
                                        Console.WriteLine("Enter new Appointment Date and Time");
                                        DateTime UPDateTime = DateTime.Parse(Console.ReadLine());
                                        string Datecount = "select count(*) from Appointments where AppointmentDateTime=@UpdatedDate";
                                        SqlCommand cmd = new SqlCommand(Datecount, Connection);
                                        cmd.Parameters.AddWithValue("@UpdatedDate", UPDateTime);                                       
                                        int result2 = (int)cmd.ExecuteScalar();                                    
                                        Console.WriteLine(result2);
                                        if (result2 > 0)
                                        {
                                            Console.WriteLine("The Date has been already occupeid.Please choose another date");
                                            repeat = true;
                                        }                                       

                                        else 
                                        {

                                            string update = "Update Appointments set AppointmentDateTime=@UpdatedDate where AppointmentID=@AppointmentID";
                                            SqlCommand Comd = new SqlCommand(update, Connection);                                           
                                            Comd.Parameters.AddWithValue("@AppointmentID", AppoitmentID);
                                            Comd.Parameters.AddWithValue("@UpdatedDate", UPDateTime);
                                            Comd.ExecuteNonQuery();
                                            Console.WriteLine("Update successful");
                                            repeat = false;

                                        }
                                    }
                                    break;

                            }
                        }
                        else
                        {
                            Console.WriteLine("Invalid AppoitnmentID.Try Again");
                            Thread.Sleep(3000);
                            repeat1 = true;
                        }
                    }
                }
                catch (Exception e)
                {

                }
            }
        }
        public static void Cancel()
        {

        }
    }
}
     

