namespace DataLightViewer.Models
{
    public struct Authentication
    {
        public string Description { get; }
        public AuthenticationType Type { get; }

        public Authentication(string description, AuthenticationType type)
        {
            Description = description;
            Type = type;
        }
    }
}
