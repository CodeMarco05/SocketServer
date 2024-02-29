using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Security.Authentication;
using System.Text;
using Client;
using Shared;

namespace ServerMain{
    class ServerMain{
        private static List<Room> _rooms = new();

        private static void Main(string[] args) {
            int port = 5050;
            string serverIPAdress = "127.0.0.1";

            try {
                //IPAddress ipAddress = IPAddress.Parse("127.0.0.1"); // Listen on any available network interface
                IPAddress ipAddress = IPAddress.Parse(serverIPAdress); // Listen on any available network interface
                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

                Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                listener.Bind(localEndPoint);
                listener.Listen(10);

                Utils.PrintStatusMessage($"Server is listening for connections on ip: {serverIPAdress} port: {port}");

                while (true) {
                    Socket socket = listener.Accept();

                    Thread clientThread = new Thread(() => HandleClient(socket));
                    clientThread.Start();
                }
            }
            catch (Exception e) {
                Utils.PrintErrorMessage($"Error: {e.Message}");
            }
        }

        private static void HandleClient(Socket socket) {
            EndPoint? clientInfo = socket.RemoteEndPoint;
            Utils.PrintStatusMessage($"Accepted connection from {clientInfo}");
            
            // Handle communication with the client
            /*byte[] buffer = new byte[65_536];
            int bytesRead = socket.Receive(buffer);
            string clientMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"Received message from client: {clientMessage}");*/

            String protocol = GetProtocol(socket);
            
            //Validate protocols
            if (protocol == Protocols.CreateRoom.ToString()) {
                int roomID = genRoomID();
                lock (_rooms) {
                    Room newRoom = new Room(roomID);
                    newRoom.connectedUsers.Add(socket);
                    _rooms.Add(newRoom);
                }
                Utils.PrintForConnectedUser(clientInfo, $"Client created Room. ID: {roomID}");
                SendRoomID(socket, roomID);
            }else if (protocol == Protocols.ConnectToRoom.ToString()) {
                //get the room id, which user wants to connect
                int roomID = getRoomIDFromMessage(socket);
                
                //TODO put user in room
                
                Utils.PrintForConnectedUser(clientInfo, $"Client connected to Room. ID: {roomID}");
                //send ack
                SendStatusResponse(socket, 0);
            }
            

            // Close the connection with the client
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        private static void SendStatusResponse(Socket socket, int i) {
            string response = $"{i}";
            byte[] responseBytes = Encoding.ASCII.GetBytes(response);
            socket.Send(responseBytes);
        }

        private static int getRoomIDFromMessage(Socket socket) {
            byte[] buffer = new byte[4];
            int bytesRead = socket.Receive(buffer);
            string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"Message: {message}");
            return int.Parse(message);
        }

        private static void SendRoomID(Socket socket, int roomID) {
            string response = $"{roomID}";
            byte[] responseBytes = Encoding.ASCII.GetBytes(response);
            socket.Send(responseBytes);
        }

        private static int genRoomID() {
            Random random = new Random();
            int randomNumber = random.Next(1_000, 10_000);

            while (true) {
                foreach (Room r in _rooms) {
                    if (r.roomID == randomNumber) {
                        randomNumber = random.Next(1_000, 10_001);
                    }
                }
                break;
            }
            return randomNumber;
        }

        private static String GetProtocol(Socket socket) {
            byte[] buffer = new byte[1_024];
            int bytesRead = socket.Receive(buffer);
            string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            return message;
        }
    }
}