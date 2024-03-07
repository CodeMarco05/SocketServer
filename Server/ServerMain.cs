using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Security.Authentication;
using System.Text;
using Newtonsoft.Json;
using Server;
using Server.Models;
using Shared;


namespace ServerMain{
    class ServerMain{
        private static List<Room> _rooms = new();

        private static void Main(string[] args) {
            int port = 5050;
            string serverIPAdress = "127.0.0.1";

            try {
                IPAddress ipAddress = IPAddress.Parse(serverIPAdress); // Listen on any available network interface
                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

                Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                listener.Bind(localEndPoint);
                listener.Listen(100);

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
            Utils.PrintForConnectedUser(clientInfo, "Connected to the server");

            string? message = null;
            

            while (true) {
                message = SocketUtils.RecieveOverSocket(socket);
                
                if(message != null) {
                    Utils.PrintForConnectedUser(clientInfo, message);
                    if(message == "exit") {
                        break;
                    }
                }
                
                //check if client still connected
                if (socket.Poll(1000, SelectMode.SelectRead) && socket.Available == 0) {
                    break;
                }
            }

            /*if (socket.Poll(1000, SelectMode.SelectRead) && socket.Available == 0) {
                DisconnectClient(clientInfo, socket);
                return;
            }

            while (true) {
                message = SocketUtils.RecieveOverSocket(socket);

                //check if client still connected
                if (socket.Poll(1000, SelectMode.SelectRead) && socket.Available == 0) {
                    break;
                }
            }*/


            // Close the connection with the client
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();

            Utils.PrintStatusMessage($"Socket closed: {clientInfo}");
        }

        private static bool HandleUserDecision(Socket socket, EndPoint? clientInfo) {
            string? message = SocketUtils.RecieveOverSocket(socket);

            if (message != null) {
                //Deserialize message
                Dictionary<string, object>? messageObject =
                    JsonConvert.DeserializeObject<Dictionary<string, object>>(message);
                User? userDetails = JsonConvert.DeserializeObject<User>(messageObject["userDetails"].ToString());
                
                //Create room
                if (messageObject?["protocol"].ToString() == Protocols.CreateRoom.ToString()) {
                    //Generate roomID
                    int roomID = genRoomID();

                    
                    bool success = SocketUtils.SendOverSocket($"{roomID}", socket);


                    Room r = new Room(roomID);
                    r.connectedUsers.Add(new UserServer(userDetails.Name, roomID, userDetails.Admin, socket));
                    lock (_rooms) {
                        _rooms.Add(new Room(roomID));
                    }

                    Utils.PrintForConnectedUser(clientInfo, $"{userDetails.Name} created room with ID: {roomID}");
                    return true;
                }
                else {
                    //Join room
                    int roomID = userDetails.RoomID;
                    bool admin = userDetails.Admin;
                    string name = userDetails.Name;
                    
                    
                    UserServer user = new UserServer(name, roomID, admin, socket);
                    
                }
            }
            else {
                Utils.PrintErrorMessage("Error during recieving message from client");
                return false;
            }

            return false;
        }

        private static void DisconnectClient(EndPoint? clientInfo, Socket socket) {
            Utils.PrintErrorMessage($"Client {clientInfo} disconnected based on closed Connection, or no data was sent.");
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        private static int genRoomID() {
            Random random = new Random();
            int randomNumber = random.Next(1_000, 10_000);

            bool exists = false;
            lock (_rooms) {
                while (true) {
                    foreach (Room r in _rooms) {
                        if (r.roomID == randomNumber) {
                            randomNumber = random.Next(1_000, 10_000);
                            exists = true;
                        }
                    }

                    if (!exists) {
                        break;
                    }
                }
            }

            return randomNumber;
        }
    }
}