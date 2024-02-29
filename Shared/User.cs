namespace Shared;

public class User{
    public string name {
        get;
        private set;
    }

    public int groupID {
        get;
        set;
    }

    public bool admin {
        get;
        set;
    }
    
    
    
    public User(string name, int groupId, bool admin) {
        this.name = name;
        groupID = groupId;
        this.admin = admin;
    }

   

    
}