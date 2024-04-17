namespace Messenjoor.Data
{

    public static class SeedData
    {
        public static void Initialize(ChatContext db)
        {
            var users = new List<User>()
            { 
            new User
            {
                Username = "joorak",
                AddedOn = DateTime.Now,
                Name = "Joorak Rezapour",
                Password = "123", // Plain Password.  Implement your own secure password mechanism
            }, 
            new User
            {
                Username = "afsaneh",
                AddedOn = DateTime.Now,
                Name = "afsaneh entezami",
                Password = "123", // Plain Password.  Implement your own secure password mechanism
            }

            };

            db.Users.AddRange(users);
            db.SaveChanges();
        }
    }
}