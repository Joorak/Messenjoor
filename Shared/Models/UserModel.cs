namespace Messenjoor.Shared.Models
{
    public class UserModel
    {
        public UserModel(int id, string name, bool isOnline = false)
        {
            Id = id;
            Name = name;
            IsOnline = isOnline;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsOnline { get; set; }
        public bool IsSelected { get; set; }
        public bool HasUnreadMessage { get; set; }
    }
}
