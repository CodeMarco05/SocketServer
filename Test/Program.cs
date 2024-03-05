using Client;

namespace Testing{
    class Program{
        public static void Main(string[] args) {
            for (int i = 0; i < 1000; i++) {
                ClientMain.Main(args);  
                Thread.Sleep(500);
            }
        }
    }
}