using DataLightViewer.ViewModels;
using System;

namespace DataLightViewer.Memento
{
    public sealed class NodeMemento
    {
        public NodeViewModel NodeViewModel { get; }
        public NodeMemento(NodeViewModel nodeViewModel)
        {
            NodeViewModel = nodeViewModel ?? throw new ArgumentException($"{nameof(nodeViewModel)}");
        }
    }
}
