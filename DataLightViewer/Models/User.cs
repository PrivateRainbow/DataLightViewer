namespace DataLightViewer.Models
{
    public struct User
    {
        public string Id { get; }
        public string Password { get; }

        public User(string id, string password)
        {
            Id = id;
            Password = password;
        }
    }
}
