using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Security.Authentication;
using System.Text;
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
            Utils.PrintStatusMessage($"Accepted connection from {clientInfo}");

            //Get The protocol
            string protocol = SocketUtils.RecieveOverSocket(socket);
            int roomID = -1;
            //Validate protocols
            if (protocol == Protocols.CreateRoom.ToString()) {
                //Generate random roomID
                roomID = genRoomID();
                lock (_rooms) {
                    //Get the user information
                    string userName = SocketUtils.RecieveOverSocket(socket);
                    UserServer user = new UserServer(userName, roomID, true, socket);

                    Room newRoom = new Room(roomID);
                    newRoom.connectedUsers.Add(user);

                    _rooms.Add(newRoom);
                }

                Utils.PrintForConnectedUser(clientInfo, $"Client created Room. ID: {roomID}");

                //Send room ID to user
                SendRoomID(socket, roomID);
            }
            else if (protocol == Protocols.ConnectToRoom.ToString()) {
                //get the room id, which user wants to connect
                String roomIDString = SocketUtils.RecieveOverSocket(socket);
                roomID = int.Parse(roomIDString);

                Room room = null;
                lock (_rooms) {
                    foreach (Room r in _rooms) {
                        if (r.roomID == roomID) {
                            room = r;
                            //Get the user information
                            string userName = SocketUtils.RecieveOverSocket(socket);
                            UserServer user = new UserServer(userName, roomID, false, socket);
                            r.connectedUsers.Add(user);
                            break;
                        }
                    }
                }

                Utils.PrintForConnectedUser(clientInfo,
                    $"Client connected to Room. ID: {roomID}. There are {room.connectedUsers.Count} members in the room");
            }

            //Save room in a variable 
            //search through the rooms and get the room with the roomID
            Room connectedRoom = null;
            lock (_rooms) {
                foreach (Room r in _rooms) {
                    if (r.roomID == roomID) {
                        connectedRoom = r;
                    }
                }
            }

            while (true) {
                //check if message was sent
                protocol = SocketUtils.RecieveOverSocket(socket);
                
                if (protocol == Protocols.MessageSent.ToString()) {
                    //get the username
                    string username = SocketUtils.RecieveOverSocket(socket);

                    //get the message
                    string message = SocketUtils.RecieveOverSocket(socket);

                    Utils.PrintForConnectedUser(clientInfo,
                        $"Message sent from {username}: {message} in room {roomID}");

                    //send the message to all the users in the room
                    foreach (UserServer user in connectedRoom.connectedUsers) {
                        //send protocol
                        SocketUtils.SendOverSocket(Protocols.MessageSent.ToString(), user.socket);
                        SocketUtils.SendOverSocket($"{username}: {message}", user.socket);
                    }
                }
                else {
                    //send protocol for no Message sent
                    SocketUtils.SendOverSocket(Protocols.NoMessageSent.ToString(), socket);
                }
            }


            // Close the connection with the client
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();

            Utils.PrintStatusMessage($"Socket closed: {clientInfo}");
        }
        


        private static void SendRoomID(Socket socket, int roomID) {
            string response = $"{roomID}";
            byte[] responseBytes = Encoding.ASCII.GetBytes(response);
            socket.Send(responseBytes);
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