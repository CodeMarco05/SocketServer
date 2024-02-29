using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Shared;

namespace ServerMain{
    class ServerMain{
        static void Main(string[] args) {
            int port = 5050;

            try{
                IPAddress ipAddress = IPAddress.Parse("127.0.0.1"); // Listen on any available network interface
                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

                Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                listener.Bind(localEndPoint);
                listener.Listen(10);

                Utils.PrintStatusMessage("Server is listening for connections on ip: 127.0.0.1:5050");

                while (true){
                    Socket handler = listener.Accept();
                    
                    Utils.PrintStatusMessage($"Accepted connection from {handler.RemoteEndPoint}");

                    // Handle communication with the client
                    byte[] buffer = new byte[1024];
                    int bytesRead = handler.Receive(buffer);
                    string clientMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Received message from client: {clientMessage}");

                    // Send a response back to the client
                    string response = "Hello, client! I received your message.";
                    byte[] responseBytes = Encoding.ASCII.GetBytes(response);
                    handler.Send(responseBytes);

                    // Close the connection with the client
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (Exception e){
                Utils.PrintErrorMessage($"Error: {e.Message}");
            }
        }
    }
}