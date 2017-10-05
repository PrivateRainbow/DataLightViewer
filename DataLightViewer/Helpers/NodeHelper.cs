using DataLightViewer.ViewModels;
using Loader.Services.Types;

namespace DataLightViewer.Helpers
{
    public static class NodeHelper
    {
        public static BuildContext GetBuildContext(this NodeViewModel node) 
            => new BuildContext(node.InnerNode, node.InnerNode.GetConnectionString());
    }
}
