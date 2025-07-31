using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myClinic
{
    public static class LoadingScreen
    {
        static public void Loadingscreen()
        {
            for (int i = 0; i < 10; i++)
            {
                Console.Write(".");
                Thread.Sleep(350);
            }
            Console.Clear();
            Console.WriteLine("Thank you for waiting.");
        }
    }
}
