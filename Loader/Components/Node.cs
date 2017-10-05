using System;
using System.Collections.Generic;

namespace Loader.Components
{
    public class Node
    {
        public string Name { get; internal set; }
        public string Value { get; internal set; }

        public Node Parent { get; internal set; }
        public List<Node> Children { get; internal set; }
        public Dictionary<string, string> Attributes { get; internal set; }

        public bool HasChildren() => Children.Count > 0;
        public bool HasAttributes() => Attributes != null && Attributes.Count > 0;
        public bool HasValue() => !string.IsNullOrEmpty(Value);

        public Node(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException($"{nameof(Name)}");

            Name = name;
            Children = new List<Node>(0);
        }

        public virtual void Add(Node node)
        {
            if (node == null)
                throw new ArgumentException($"{nameof(node)} is not valid.");
            if (node.Parent != null)
                throw new InvalidOperationException($"{nameof(node)} already has been added.");

            node.Parent = this;
            Children.Add(node);
        }

        public virtual void Insert(int index, Node node)
        {
            if (node == null)
                throw new ArgumentException($"{nameof(node)} is not valid.");
            if (node.Parent != null)
                throw new InvalidOperationException($"{nameof(node)} already has been added.");

            node.Parent = this;
            Children.Insert(index, node);
        }

        public virtual void Remove(Node node)
        {
            if (node == null)
                throw new InvalidOperationException($"{nameof(node)} is empty.");
            Children.Remove(node);
        }

        public void AttachAttribute(KeyValuePair<string, string> attribute)
        {
            Attributes = Attributes ?? new Dictionary<string, string>();
            Attributes.Add(attribute.Key, attribute.Value);
        }

    }
}
