using System.Net.Sockets;

namespace Server.Models{
    public class UserServer{
        public string name { get; private set; }

        public int groupID { get; set; }

        public bool admin { get; set; }

        public Socket socket { get; set; }


        public UserServer(string name, int groupId, bool admin, Socket socket) {
            this.name = name;
            groupID = groupId;
            this.admin = admin;
            this.socket = socket;
        }
    }
}