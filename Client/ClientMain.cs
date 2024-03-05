using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using Newtonsoft.Json;
using Shared;

namespace Client{
    public class ClientMain{
        private static Socket _socket;
        private static String _ipAddressString = "127.0.0.1";
        private static int _port = 5050;

        private static string _messageToSend = "<none>";
        private static string _userName;

        public static void Main(string[] args) {
            //Thread messageThread = new Thread(new ThreadStart(PrintMessage));
            //messageThread.Start();


            //Start Socket init
            ConnectSocket();

            //Check if socket is really connected
            if (_socket.Connected) {
                Utils.PrintStatusMessage("Connected to the server");
            }

            //Get Information from user 
            User user = GetInformation();
            _userName = user.Name;

            

            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }


        private static void ConsoleListen() {
            while (true) {
                string message = Console.ReadLine();
                if (message != null && message != "") {
                    if (message == "exit") {
                        //TODO send server message that the user is leaving
                        break;
                    }

                    _messageToSend = message;
                }
            }
        }

        private static void ManageMessages() {
            while (true) {
                //Check if there is a message to send
                if (_messageToSend != "<none>") {
                    //Send protocol
                    SocketUtils.SendOverSocket(Protocols.MessageSent.ToString(), _socket);

                    //Send the username
                    SocketUtils.SendOverSocket(_userName, _socket);

                    //Send the message
                    SocketUtils.SendOverSocket(_messageToSend, _socket);
                    _messageToSend = "<none>";
                }
                else {
                    //Send protocol
                    SocketUtils.SendOverSocket(Protocols.NoMessageSent.ToString(), _socket);
                }


                //Get the message from the server
                string protocol = SocketUtils.RecieveOverSocket(_socket);
                if (protocol == Protocols.MessageSent.ToString()) {
                    string message = SocketUtils.RecieveOverSocket(_socket);
                    Console.WriteLine(message);
                }
            }
        }

        //Connection the socket to the server
        private static void ConnectSocket() {
            try {
                IPAddress ipAddress = IPAddress.Parse(_ipAddressString);
                IPEndPoint remoteEndPoint = new IPEndPoint(ipAddress, _port);

                _socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _socket.Connect(remoteEndPoint);
            }
            catch (Exception e) {
                Utils.PrintErrorMessage($"Failed to connect to the server: {e}");
                throw;
            }
        }


        private static User GetInformation() {
            string name = "<empty>";
            do {
                Utils.UserInput("Please enter your name: ");
                name = Console.ReadLine();
            } while (string.IsNullOrWhiteSpace(name));

            //---------------------------------------------------
            string createRoom = "y";
            do {
                Utils.UserInput("Do you want to create a room? (y/n): ");
                createRoom = Console.ReadLine();
            } while (string.IsNullOrWhiteSpace(createRoom) || createRoom != "y" && createRoom != "n");

            //---------------------------------------------------
            int roomID = 0;
            bool admin = false;
            if (createRoom == "y") {
                admin = true;
                //Create a user
                User user = new User(name, admin);

                //Send the server the command for creating a room
                roomID = CreateRoomOnServer(user);

                //Print the room id
                Utils.PrintStatusMessage($"Your room id: {roomID}");
            }
            else {
                String roomIDString;

                do {
                    Utils.UserInput("Please enter your RoomID you want to connect to: ");
                    roomIDString = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(roomIDString)) {
                        continue;
                    }

                    if (int.TryParse(roomIDString, out roomID)) {
                        break;
                    }
                } while (true);

                ConnectToRoom(roomIDString, name);
            }

            return new User(name, roomID, admin);
        }

        //Is connecting to an existing Room
        private static void ConnectToRoom(String roomId, string username) {
            
        }

        //Sends the server the command for creating a room
        private static int CreateRoomOnServer(User user) {
            int roomID = 0;

            var message = new Dictionary<string, object>();
            
            message.Add("protocol", Protocols.CreateRoom.ToString());
            message.Add("userDetails", user);
            
            string json = JsonConvert.SerializeObject(message);
            
            //Send the server the command for creating a room
            SocketUtils.SendOverSocket(json, _socket);
            
            string response = SocketUtils.RecieveOverSocket(_socket);
            if (response != null) {
                roomID = int.Parse(response);
            }
            
            return roomID;
        }
    }
}