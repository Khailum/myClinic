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
            bool repeat = true;
            while (repeat)
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
                    if (results > 1)
                    {
                        Console.WriteLine("You are now logged in");
                    }
                    else
                    {
                        Console.WriteLine("INCORRECT PASSWORD OR USERNAME.\n1.TRY AGAIN\n2.Exit");
                        int answer1 = int.Parse(Console.ReadLine());
                        if (answer1 == 1)
                        {
                            repeat = true;
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
            bool repeat = true;
            using SqlConnection Connection = new SqlConnection(receptanist.connection);
            {
                while (repeat)
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
                    if (results > 1)
                    {
                        Console.WriteLine("You are now logged in");
                    }
                    else
                    {
                        Console.WriteLine("INCORRECT PASSWORD OR USERNAME.\n1.TRY AGAIN\n2.Exit");
                        int answer1 = int.Parse(Console.ReadLine());
                        if (answer1 == 1)
                        {
                            repeat = true;
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
            bool repeat = true;
            using SqlConnection Connection = new SqlConnection(receptanist.connection);
            while (repeat)
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
                if (results > 1)
                {
                    Console.WriteLine("You are now logged in");
                }
                else
                {
                    Console.WriteLine("INCORRECT PASSWORD OR USERNAME.\n1.TRY AGAIN\n2.Exit");
                    int answer1 = int.Parse(Console.ReadLine());
                    if (answer1 == 1)
                    {
                        repeat = true;
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
