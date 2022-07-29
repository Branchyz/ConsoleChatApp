using System;
using RiptideNetworking.Utils;

namespace ConsoleChatApp
{
    static class EntryPoint
    {
        private const int RecursionLimit = 10;
        private static int recursion = 0;

        static void Main()
        {
            if (recursion > RecursionLimit)
            {
                Console.WriteLine("Recursion limit reached!");
                return;
            }

            Console.Title = "Chat App";
            RiptideLogger.Initialize(Console.WriteLine, true);

            Console.WriteLine("Do you want to host a server or connect to a server?");
            Console.WriteLine("Type 'host' or 'connect'");
            string input = Console.ReadLine();
            
            if(String.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("Invalid input!");
                recursion++;
                Main();
                return;
            }

            if (input.ToLower() == "host")
            {
                AppServer.Start();
            }
            else if(input.ToLower() == "connect")
            {
                Console.WriteLine("Enter the IP address of the server you want to connect to:");
                string ip = Console.ReadLine();
                Console.WriteLine("Enter you're username:");
                string username = Console.ReadLine();
                AppClient.Start(ip, username);
            }
            else
            {
                Console.WriteLine("Invalid input!");
                recursion++;
                Main();
            }
        }
    }
}
