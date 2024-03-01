using System.Net;

namespace Shared{
    public class Utils{
        public static void PrintStatusMessage(String message) {
            Console.WriteLine("\n------------- \u001b[34m" + message + "\u001b[0m -------------");
        }
        
        public static void PrintErrorMessage(String message) {
            Console.WriteLine("\n------------- \u001b[31m" + message + "\u001b[0m -------------");
        }

        public static void UserInput(String message) {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write($"Input: {message}");
            Console.ResetColor();
        }
        

        public static void PrintForConnectedUser(EndPoint clientInfo, String message) {
            Console.WriteLine("\n------------- \u001b[34m" + clientInfo + "\u001b[0m -------------");
            Console.WriteLine($"{message}");
        }
        
    }
}

