using System;

namespace myClinic
{
    public static class Logo
    {
        public static void Display()
        {
            Console.ForegroundColor = ConsoleColor.Cyan; 

            string logo = $@"
          .         .                                                                                                       
         ,8.       ,8.   `8.`8888.      ,8'  ,o888888o.    8 8888          8 8888 b.             8  8 8888     ,o888888o.   
        ,888.     ,888.   `8.`8888.    ,8'  8888     `88.  8 8888          8 8888 888o.          8  8 8888    8888     `88. 
       .`8888.   .`8888.   `8.`8888.  ,8',8 8888       `8. 8 8888          8 8888 Y88888o.       8  8 8888 ,8 8888       `8.
      ,8.`8888. ,8.`8888.   `8.`8888.,8' 88 8888           8 8888          8 8888 .`Y888888o.    8  8 8888 88 8888          
     ,8'8.`8888,8^8.`8888.   `8.`88888'  88 8888           8 8888          8 8888 8o. `Y888888o. 8  8 8888 88 8888          
    ,8' `8.`8888' `8.`8888.   `8. 8888   88 8888           8 8888          8 8888 8`Y8o. `Y88888o8  8 8888 88 8888          
   ,8'   `8.`88'   `8.`8888.   `8 8888   88 8888           8 8888          8 8888 8   `Y8o. `Y8888  8 8888 88 8888          
  ,8'     `8.`'     `8.`8888.   8 8888   `8 8888       .8' 8 8888          8 8888 8      `Y8o. `Y8  8 8888 `8 8888       .8'
 ,8'       `8        `8.`8888.  8 8888      8888     ,88'  8 8888          8 8888 8         `Y8o.`  8 8888    8888     ,88' 
,8'         `         `8.`8888. 8 8888       `8888888P'    8 888888888888  8 8888 8            `Yo  8 8888     `8888888P'   
";

            Console.WriteLine(logo);

            Console.ResetColor(); 
        }
        public static void ShowLoading()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            for (int i = 0; i < 5; i++)
            {
                Console.Write(".");
                Thread.Sleep(400);
            }
            Console.ResetColor ();
            Console.Clear();
        }

    }
}
