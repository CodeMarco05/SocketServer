using Server.Models;

namespace Server{
    public class Room{
        public int roomID {
            get;
            private set;
        }

        public List<UserServer> connectedUsers = new();
        public Room(int roomId) {
            roomID = roomId;
        }

  
    }
}

