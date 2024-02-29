using System.Net.Sockets;

namespace Client;

public class Room{
    public int roomID {
        get;
        private set;
    }

    public List<Socket> connectedUsers = new();
    public Room(int roomId) {
        roomID = roomId;
    }

  
}