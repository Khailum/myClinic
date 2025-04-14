using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Myclinic
{
    public static class Log_in
    {
      
        public static void LoginDoc()
        {
            bool repeatDO = true;
            while (repeatDO)
            {


                using SqlConnection Connection = new SqlConnection(receptanist.connection);
                {
                    Connection.Open();
                    Console.WriteLine("Enter your Username");
                    string username = Console.ReadLine();
                    Console.WriteLine("Enter Your Password");
                    string password = Console.ReadLine();
                    string CheckDetails = "Select count (*) from Users where Username=@username and Passwords=@password and Role=@Doctor";
                    SqlCommand cmd = new SqlCommand(CheckDetails, Connection);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);
                    cmd.Parameters.AddWithValue("@Doctor", "Doctor");
                    int results = (int)cmd.ExecuteScalar();
                    Connection.Close(); 
                    if (results > 0)
                    {
                        Console.WriteLine("You are now logged in");
                        break;
                    }
                    else
                    {
                        Console.WriteLine("INCORRECT PASSWORD OR USERNAME.\n1.TRY AGAIN\n2.Exit");
                        int answer1 = int.Parse(Console.ReadLine());
                        if (answer1 == 1)
                        {
                            repeatDO = true;
                        }
                        else if (answer1 == 2)
                        {
                            Environment.Exit(1);
                        }

                    }
                }
            }
        }
        public static void LoginRec()
        {
            bool repeatRE = true;

            while (repeatRE)
            {
                using SqlConnection Connection = new SqlConnection(receptanist.connection);
                {
                    Connection.Open();
                    Console.WriteLine("Enter your Username");
                    string username = Console.ReadLine();
                    Console.WriteLine("Enter Your Password");
                    string password = Console.ReadLine();
                    string CheckDetails = "Select count (*) from Users where Username=@username and Passwords=@password and Role=@Receptionsit";
                    SqlCommand cmd = new SqlCommand(CheckDetails, Connection);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);
                    cmd.Parameters.AddWithValue("@Receptionsit", "receptionsit");
                    int results = (int)cmd.ExecuteScalar();
                    if (results > 0)
                    {
                        Console.WriteLine("You are now logged in");
                        repeatRE = false;
                    }
                    else
                    {
                        Console.WriteLine("INCORRECT PASSWORD OR USERNAME.\n1.TRY AGAIN\n2.Exit");
                        int answer1 = int.Parse(Console.ReadLine());
                        if (answer1 == 1)
                        {
                            repeatRE = true;
                        }
                        else if (answer1 == 2)
                        {
                            Environment.Exit(1);
                        }

                    }
                }
            }
        }
        public static void LoginAdmin()
        {
            bool repeatAD = true;
            using SqlConnection Connection = new SqlConnection(receptanist.connection);
            while (repeatAD)
            {
                Console.WriteLine("Enter your Username");
                string username = Console.ReadLine();
                Console.WriteLine("Enter Your Password");
                string password = Console.ReadLine();
                string CheckDetails = "Select count (*) from Users where Username=@username and Passwords=@password and Role=@Admin";
                SqlCommand cmd = new SqlCommand(CheckDetails, Connection);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);
                cmd.Parameters.AddWithValue("@Admin", "Admin");
                int results = (int)cmd.ExecuteScalar();
                if (results > 0)
                {
                    Console.WriteLine("You are now logged in");
                }
                else
                {
                    Console.WriteLine("INCORRECT PASSWORD OR USERNAME.\n1.TRY AGAIN\n2.Exit");
                    int answer1 = int.Parse(Console.ReadLine());
                    if (answer1 == 1)
                    {
                        repeatAD = true;
                    }
                    else if (answer1 == 2)
                    {
                        Environment.Exit(1);
                    }

                }
            }
        }
    }
}
