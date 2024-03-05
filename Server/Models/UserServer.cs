using System.Net.Sockets;

namespace Server.Models{
    public class UserServer{
        public string Name { get; private set; }

        public int RoomId { get; set; }

        public bool Admin { get; set; }

        public Socket Socket { get; set; }


        public UserServer(string name, int roomID, bool admin, Socket socket) {
            this.Name = name;
            RoomId = roomID;
            this.Admin = admin;
            this.Socket = socket;
        }
    }
}