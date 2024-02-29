namespace Shared{
    public class Utils{
        public static void PrintStatusMessage(String message) {
            Console.WriteLine("------------- \u001b[34m" + message + "\u001b[0m -------------");
        }
        
        public static void PrintErrorMessage(String message) {
            Console.WriteLine("------------- \u001b[31m" + message + "\u001b[0m -------------");
        }
        
    }
}

