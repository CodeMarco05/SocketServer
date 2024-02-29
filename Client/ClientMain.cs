using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Shared;

namespace Client{
    class ClientMain{
        static void Main(string[] args) {
            string ipAddressString = "127.0.0.1";
            int port = 5050;

            try{
                IPAddress ipAddress = IPAddress.Parse(ipAddressString);
                IPEndPoint remoteEndPoint = new IPEndPoint(ipAddress, port);

                Socket socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                socket.Connect(remoteEndPoint);

                if (socket.Connected){
                    Utils.PrintStatusMessage("Connected to the server");

                    // Your communication logic with the server goes here

                    // For example, sending a message to the server
                    string message = "Hello, server!";
                    byte[] messageBytes = Encoding.ASCII.GetBytes(message);
                    socket.Send(messageBytes);

                    // Receiving a response from the server
                    byte[] buffer = new byte[1024];
                    int bytesRead = socket.Receive(buffer);
                    string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Server response: {response}");

                    // Close the connection
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
                else{
                    Utils.PrintErrorMessage("Failed to connect to the server");
                }
            }
            catch (Exception e){
                Utils.PrintErrorMessage($"Error: {e.Message}");
            }
        }
    }
}