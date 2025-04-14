using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Myclinic
{
    public static class Admin
    {       
        public static string connectionstring = receptanist.connection;
      

        public static void CreateDoctor()
        {
            string Fname, Lname, Dspeciality;
            Random random = new Random();
            int number = random.Next(0, 100);

            using SqlConnection Connection = new SqlConnection(connectionstring);
            {
                Connection.Open();
                Console.WriteLine("Please enter doctor's first name.");
                Fname = Console.ReadLine();
                Console.WriteLine("Enter doctor last name");
                Lname = Console.ReadLine();
                Console.WriteLine("What is the doctor speciality");
                Dspeciality = Console.ReadLine();
                Console.WriteLine("Doctor Passwords");
                string Password = Console.ReadLine();//to be hashed//
                string username = ($"{Fname}{Lname}@myclinic.co.za");
                string checkdoctor = "SELECT COUNT(*) FROM Users WHERE Username = @username";
                SqlCommand cmd = new SqlCommand(checkdoctor, Connection);
                cmd.Parameters.AddWithValue("@username", username);
                int results = (int)cmd.ExecuteScalar();
                if (results > 0)
                {
                    Console.WriteLine("The username is already accupied.");
                    username = ($"{Fname}{Lname}{number}@myclinic.co.za");
                    Console.WriteLine($"Your username will be:{username}");
                    string Doctorinsert = "Insert into Doctors(FirstName,LastName,TypeDoc) Values (@Fname,@Lname,@TypeDoc)";
                    SqlCommand Command = new SqlCommand(Doctorinsert, Connection);
                    Command.Parameters.AddWithValue("@Fname", Fname);
                    Command.Parameters.AddWithValue("@Lname", Lname);
                    Command.Parameters.AddWithValue("@TypeDoc", Dspeciality);
                    Command.ExecuteNonQuery();
                    string Userinsert = "Insert into Users(FirstName,LastName,Username,Role,Passwords) Values (@Fname,@lname,@Username,@Role,@passwords)";
                    SqlCommand Comd = new SqlCommand(Userinsert, Connection);
                    Comd.Parameters.AddWithValue("@Fname", Fname);
                    Comd.Parameters.AddWithValue("@Lname", Lname);
                    Comd.Parameters.AddWithValue("@Role", "Doctor");
                    Comd.Parameters.AddWithValue("@passwords", Password);
                    Comd.Parameters.AddWithValue("@Username", username);
                    Comd.ExecuteNonQuery();
                    Console.WriteLine("Doctor is add successfully");
                }
                else
                {
                    string Doctorinsert = "Insert into Doctors(FirstName,LastName,TypeDoc) Values (@Fname,@Lname,@TypeDoc)";
                    SqlCommand Command = new SqlCommand(Doctorinsert, Connection);
                    Command.Parameters.AddWithValue("@Fname", Fname);
                    Command.Parameters.AddWithValue("@Lname", Lname);
                    Command.Parameters.AddWithValue("@TypeDoc", Dspeciality);
                    Command.ExecuteNonQuery();
                    string Userinsert = "Insert into Users(FirstName,LastName,Username,Role,Passwords) Values (@Fname,@lname,@Username,@Role,@passwords)";
                    SqlCommand Comd = new SqlCommand(Userinsert, Connection);
                    Comd.Parameters.AddWithValue("@Fname", Fname);
                    Comd.Parameters.AddWithValue("@Lname", Lname);
                    Comd.Parameters.AddWithValue("@Role", "Doctor");
                    Comd.Parameters.AddWithValue("@passwords", Password);
                    Comd.Parameters.AddWithValue("@Username", username);
                    Comd.ExecuteNonQuery();
                    Console.WriteLine("Doctor is add successfully");
                }               
            }
        }
        public static void CreateRecepecianst()
        {
            Random random = new Random();
            int number = random.Next(0, 100);
            string Fname, Lname, Dspeciality;
            using SqlConnection Connection = new SqlConnection(connectionstring);
            Connection.Open();
            Console.WriteLine("Please enter Recepcinist first name.");
            Fname = Console.ReadLine();
            Console.WriteLine("Enter Recepcinist last name");
            Lname = Console.ReadLine();
            Console.WriteLine("Recepcianist Passwords");
            string Password = Console.ReadLine();//to be hashed//

            string username = ($"{Fname}{Lname}@myclinic.co.za");

            string checkdoctor = "SELECT COUNT(*) FROM Users WHERE Username = @username";
            SqlCommand cmd = new SqlCommand(checkdoctor, Connection);
            cmd.Parameters.AddWithValue("@username", username);
            int results = (int)cmd.ExecuteScalar();
            if (results > 0)
            {
                Console.WriteLine("The username is already accupied.");
                username = ($"{Fname}{Lname}{number}@myclinic.co.za");
                Console.WriteLine($"Your username will be:{username}");
                Thread.Sleep(3000);
                string Userinsert = "Insert into Users(FirstName,LastName,Username,Role,Passwords) Values (@Fname,@lname,@Username,@Role,@passwords)";
                SqlCommand Comd = new SqlCommand(Userinsert, Connection);
                Comd.Parameters.AddWithValue("@Fname", Fname);
                Comd.Parameters.AddWithValue("@Lname", Lname);
                Comd.Parameters.AddWithValue("@Role", "Receptionsit");
                Comd.Parameters.AddWithValue("@passwords", Password);
                Comd.Parameters.AddWithValue("@Username", username);
                Comd.ExecuteNonQuery();
                Console.WriteLine("Receptionsit is add successfully");
            }
            else
            {
                string Userinsert = "Insert into Users(FirstName,LastName,Username,Role,Passwords) Values (@Fname,@lname,@Username,@Role,@passwords)";
                SqlCommand Comd = new SqlCommand(Userinsert, Connection);
                Comd.Parameters.AddWithValue("@Fname", Fname);
                Comd.Parameters.AddWithValue("@Lname", Lname);
                Comd.Parameters.AddWithValue("@Role", "receptionsit");
                Comd.Parameters.AddWithValue("@passwords", Password);
                Comd.Parameters.AddWithValue("@Username", username);
                Comd.ExecuteNonQuery();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("receptionsit is add successfully");
                Console.ResetColor();

            }
        }
        public static void Delete()
        {
            using SqlConnection Connection = new SqlConnection(connectionstring);
            Connection.Open();
            try
            {
                bool repeatDE = true;
                while (repeatDE)

                {                  

                    Console.WriteLine("Enter the User ID that you want to Delete.");
                    int UserID = int.Parse(Console.ReadLine());
                    string checkdoctor = "Select count (*) from Users where UserID=@UserId";
                    SqlCommand cmd = new SqlCommand(checkdoctor, Connection);
                    cmd.Parameters.AddWithValue("@UserId",UserID);
                    int results = (int)cmd.ExecuteScalar();



                    if (results > 0)
                    {
                        string getUserDetails = "SELECT UserID, FirstName, LastName, Username, Passwords, Role FROM Users WHERE UserID = @UserID";
                        SqlCommand comd = new SqlCommand(getUserDetails, Connection);
                        comd.Parameters.AddWithValue("@userID", UserID);
                        SqlDataReader reader3 = comd.ExecuteReader();
                        while (reader3.Read())
                        {
                            int Userid = reader3.GetInt32(0);
                            string FirstName = reader3.GetString(1);
                            string LastName = reader3.GetString(2);
                            string Username = reader3.GetString(3);
                            string Password = reader3.GetString(4);
                            string Role = reader3.GetString(5);

                            Console.WriteLine($"UserID:{Userid}\nFirstName:{FirstName}\nLastName:{LastName}\nUsername:{Username}\nPasswords:{Password}\nRole:{Role}");
                            Console.WriteLine("");
                           
                        }
                        reader3.Close();
                        Console.WriteLine("Confirm if you want to delete this Users.\n1.Confirm \n2.Cancel");
                        int asnwer = int.Parse(Console.ReadLine());
                        if (asnwer == 1)
                        {
                            string Delete = "Delete from Users where UserID=@UserID";
                            SqlCommand cmd2 = new SqlCommand(Delete, Connection);
                            cmd2.Parameters.AddWithValue("@UserID", UserID);
                            cmd2.ExecuteNonQuery();
                            Console.WriteLine("User deleted");
                        }
                        else if(asnwer == 2)
                        {
                            repeatDE = true;
                        }
                        

                    }
                    else if (results <0)
                    {
                        Console.WriteLine("The User Id is ont Valid Please try again");
                        Thread.Sleep(3000);
                        repeatDE = true;

                    }
                }
            }
            catch(Exception e) 
            {
                Console.WriteLine(e.Message);
            }
        }                         
    }
}
