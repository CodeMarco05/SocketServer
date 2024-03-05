namespace Shared
{
    public class User
    {
        public string Name { get; set; }
        public int RoomID { get; set; }
        public bool Admin { get; set; }

        // Parameterless constructor for JSON.NET deserialization
        public User()
        {
        }

        public User(string name, int roomId, bool admin)
        {
            this.Name = name;
            RoomID = roomId;
            this.Admin = admin;
        }

        public User(string name, bool admin)
        {
            this.Name = name;
            this.Admin = admin;
        }
    }
}