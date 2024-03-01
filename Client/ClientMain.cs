using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using Shared;

namespace Client{
    class ClientMain{
        private static Socket _socket;
        private static String _ipAddressString = "127.0.0.1";
        private static int _port = 5050;

        private static void Main(string[] args) {
            

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
                Utils.PrintErrorMessage("Failed to connect to the server");
                throw;
            }
            
        }


        private static User GetInformation() {
            string name;
            do {
                Utils.UserInput("Plese enter your name: ");
                name = Console.ReadLine();
            } while (string.IsNullOrWhiteSpace(name));

            //---------------------------------------------------
            string createRoom;
            do {
                Utils.UserInput("Do you want to create a room y/n: ");
                createRoom = Console.ReadLine();
            } while (string.IsNullOrWhiteSpace(createRoom) || createRoom != "y" && createRoom != "n");
            
            //---------------------------------------------------
            int roomID;
            bool admin = false;
            if (createRoom == "y") {
                roomID = CreateRoomOnServer(name);
                admin = true;
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
                
                ConnectToRoom(roomID);
            }

            
            Console.WriteLine($"ID: {roomID}");

            return new User(name, roomID, admin);
        }

        //Is connecting to an existing Room
        private static void ConnectToRoom(int roomId) {
            //Send protocol
            string protocol = Protocols.ConnectToRoom.ToString();
            byte[] data = Encoding.ASCII.GetBytes(protocol);
            _socket.Send(data);
            
            //Send the roomID
            string message = roomId.ToString();
            data = Encoding.ASCII.GetBytes(message);
            _socket.Send(data);
            
            //ask for the status
            byte[] buffer = new byte[1];
            int bytesRead = _socket.Receive(buffer);
            string response = (Encoding.ASCII.GetString(buffer, 0, bytesRead));
            
            //status 0 is good 1 is went wrong
            int status = int.Parse(response);

            if (status == 1) {
                Utils.PrintErrorMessage("Error during connecting to existing room.");
                Environment.FailFast("Critical error occured.");
            }
            
        }

        //Sends the server the command for creating a room
        private static int CreateRoomOnServer(string name) {
            
            UtilsSocket.SendOverSocket(Protocols.CreateRoom.ToString(), _socket);
            
            UtilsSocket.SendOverSocket(name, _socket);

            //Get four digit code
            byte[] buffer = new byte[4];
            int bytesRead = _socket.Receive(buffer);
            string response = (Encoding.ASCII.GetString(buffer, 0, bytesRead));
            int roomID = int.Parse(response);
            
            return roomID;
        }
    }
}