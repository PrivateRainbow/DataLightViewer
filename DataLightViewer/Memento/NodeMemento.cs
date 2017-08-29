using DataLightViewer.Mediator;
using DataLightViewer.ViewModels;
using Loader.Components;
using Loader.Factories;
using Loader.Types;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

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

    public static class AppStateService
    {
        private static NodeMemento _nodeMemento;
        private static bool IsInitialized => _nodeMemento != null;

        public static void Initialize(NodeMemento memento)
        {
            _nodeMemento = memento ?? throw new ArgumentException($"{nameof(memento)}");

            //_nodeMemento.Node.AttachAttribute(new KeyValuePair<string, string>(App.ServerConnectionStringLiteral, App.ServerConnectionString));
        }

        public static Task<bool> SaveProjectFileAsync(string pathToFile) => Task.Run(() => SaveProject(pathToFile));
        public static Task<bool> OpenProjectFileAsync(string pathToFile) => Task.Run(() => OpenProject(pathToFile));

        private static bool SaveProject(string pathToFile)
        {
            try
            {
                if (!IsInitialized)
                    throw new ArgumentException($"{nameof(NodeMemento)} is not initialized. Use <code> Initialize(NodeMemento am) </code> method.");

                using (var writer = File.CreateText(pathToFile))
                {
                    var serializer = SerializerFactory.MakeSerializer(SourceSchemaType.Database, writer);
                    //serializer.Serialize(_nodeMemento.Node);

                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
            finally
            {
                _nodeMemento = null;
            }
        }
        private static bool OpenProject(string pathToFile)
        {
            try
            {
                if (!IsInitialized)
                    throw new ArgumentException($"{nameof(NodeMemento)} is not initialized. Use <code> Initialize(NodeMemento am) </code> method.");

                using (var stream = File.OpenRead(pathToFile))
                {
                    var scanner = ScannerFactory.MakeScanner(SourceSchemaType.Database);
                    //_nodeMemento = new NodeMemento(scanner.Scan(stream));

                    if (_nodeMemento != null)
                    {
                        //var connectionString = string.Copy(_nodeMemento.Node.Attributes[App.ServerConnectionStringLiteral]);

                        // Remove real connection from server node
                        //_nodeMemento.Node.Attributes.Remove(App.ServerConnectionStringLiteral);

                        //App.ServerConnectionString = connectionString;
                        Messenger.Instance.Notify(MessageType.FileProjectOpened, _nodeMemento);
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
            finally
            {
                _nodeMemento = null;
            }
        }

        private static Node GetUIState()
        {
            var viewNode = _nodeMemento.NodeViewModel;
            var uiNode = viewNode.ToUIStateNode();
            
            return null;           
        }

        private static void Transform(NodeViewModel vm, Node parent = null)
        {
            var current = vm.ToUIStateNode();
            if (parent != null)
                parent.Add(current);

            foreach (var child in vm.Children)
                Transform(vm, current);
        }

    }

    public static class NodeStateConvertor
    {
        #region Attributes for representing UI-state

        private const string IsExpandedAttr = "isExpanded";
        private const string IsSelectedAttr = "isSelected";

        #endregion

        /// <summary>
        /// Convert inner node of VM to common node with attributes 
        /// which are necessary for recovering UI-state of App after opening project file
        /// </summary>
        /// <returns></returns>
        public static Node ToUIStateNode(this NodeViewModel vm)
        {
            var node = new Node(vm.Name);

            var isExpandedState = new KeyValuePair<string, string>(IsExpandedAttr, vm.IsExpanded.ToString());
            var isSelectedState = new KeyValuePair<string, string>(IsSelectedAttr, vm.IsSelected.ToString());

            node.AttachAttribute(isExpandedState);
            node.AttachAttribute(isSelectedState);

            return node;
        }
    }
}
