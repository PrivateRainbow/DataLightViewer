using Loader.Components;

namespace Loader.Services.Types
{
    public struct BuildContext
    {
        public Node Node { get; set; }
        public string Connection { get; set; }

        public BuildContext(Node node, string connection)
        {
            Node = node;
            Connection = connection;
        }
        public static BuildContext CreateFrom(Node node, string connection) => new BuildContext(node, connection);
    }

}
