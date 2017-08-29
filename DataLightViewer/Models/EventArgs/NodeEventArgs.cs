using Loader.Components;

namespace DataLightViewer.Models.EventArgs
{
    public class NodeEventArgs : System.EventArgs
    {
        public Node Node { get; }
        public NodeEventArgs(Node node)
        {
            Node = node;
        }
    }
}