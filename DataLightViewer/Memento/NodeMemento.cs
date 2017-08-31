using DataLightViewer.ViewModels;
using Loader.Components;
using Loader.Factories;
using Loader.Types;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows;
using DataLightViewer.Mediator;

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

    public class AppStateService
    {
        #region Data

        private const string ProjectFileTypeLabel = "file-type";
        private const string DefaultUiStateFileName = "UI";
        private const string DefaultDataStateFileName = "Data";
        private const string ProjectFileExtention = ".dtl";       

        private readonly NodeMemento _nodeMemento;

        private readonly string FullUiStateFileName;
        private readonly string FullDataStateFileName;

        #endregion

        public AppStateService()
        {
            FullUiStateFileName = string.Concat(DefaultUiStateFileName, ProjectFileExtention);
            FullDataStateFileName = string.Concat(DefaultDataStateFileName, ProjectFileExtention);
        }

        public AppStateService(NodeMemento memento) : this()
        {
            _nodeMemento = memento ?? throw new ArgumentException($"{nameof(memento)}");
        }

        public Task SaveProjectAsync(string pathToFile) => Task.Run(() => SaveProject(pathToFile));
        public Task OpenProjectAsync(string folderPath) => Task.Run(() => OpenProject(folderPath));

        private async Task SaveProject(string folderPath)
        {
            try
            {
                LogWrapper.WriteInfo($"Saving the project file by folder path ({folderPath})", "Saving ...");

                var dataStateFilePath = Path.Combine(folderPath, FullDataStateFileName);
                var uiStateFilePath = Path.Combine(folderPath, FullUiStateFileName);

                var dataNode = _nodeMemento.NodeViewModel.InnerNode;
                dataNode.AttachAttribute(new KeyValuePair<string, string>(ProjectFileTypeLabel, DefaultDataStateFileName));
                dataNode.AttachAttribute(new KeyValuePair<string, string>(App.ServerConnectionStringLiteral, App.ServerConnectionString));

                var uiNode = GetUINodeState();
                uiNode.AttachAttribute(new KeyValuePair<string, string>(ProjectFileTypeLabel, DefaultUiStateFileName));

                var writeDataTask = WriteProjectFileAsync(dataStateFilePath, dataNode, SourceSchemaType.Database);
                var writeUiTask = WriteProjectFileAsync(uiStateFilePath, uiNode, SourceSchemaType.File);

                await Task.WhenAll(writeDataTask, writeUiTask);

                LogWrapper.WriteInfo($"Project was save by path ({folderPath})", "Saved.");
            }
            catch (Exception ex)
            {
                var msg = "Error on saving the project.";
                LogWrapper.WriteError(msg, ex.InnerException, "Not Saved.");
                MessageBox.Show(msg, "Error");
            }
        }
        private async Task OpenProject(string folderPath)
        {
            try
            {
                LogWrapper.WriteInfo($"Opening the project file by path ({folderPath})", "Opening ...");

                var dataStateFilePath = Path.Combine(folderPath, FullDataStateFileName);
                var uiStateFilePath = Path.Combine(folderPath, FullUiStateFileName);

                var readDataTask = ReadProjectFileAsync(dataStateFilePath, SourceSchemaType.Database);
                var readUiTask = ReadProjectFileAsync(uiStateFilePath, SourceSchemaType.File);

                await Task.WhenAll(readDataTask, readUiTask).ContinueWith(nodes => InitializeMemento(nodes));

                LogWrapper.WriteInfo($"Project was opened by path ({folderPath})", "Opened.");
            }
            catch (Exception ex)
            {
                var msg = "Error on opening the project.";
                LogWrapper.WriteError(msg, ex.InnerException, "Not Opened.");
                MessageBox.Show(msg, "Error");
            }
        }
        private void InitializeMemento(Task<Node[]> nodes)
        {
            try
            {
                var initializedNodes = nodes.Result.ToList();
                var dataNode = initializedNodes.Find(n => n.Attributes[ProjectFileTypeLabel] == DefaultDataStateFileName);
                var uiNode = initializedNodes.Find(n => n.Attributes[ProjectFileTypeLabel] == DefaultUiStateFileName);

                App.ServerConnectionString = string.Copy(dataNode.Attributes[App.ServerConnectionStringLiteral]);

                var viewModel = new NodeViewModel(dataNode, parent: null, lazyLoadChildren: false);
                NodeStateConvertor.TransformToViewModelNode(viewModel, uiNode);

                var memento = new NodeMemento(viewModel);
                Messenger.Instance.NotifyColleagues(MessageType.OnOpeningProjectFile, memento);
            }
            catch(Exception)
            {
                throw;
            }
        }


        private Task WriteProjectFileAsync(string pathToFile, Node node, SourceSchemaType schemaType)
        {
            return Task.Run(() =>
            {
                try
                {
                    using (var writer = new StreamWriter(pathToFile))
                    {
                        var serializer = SerializerFactory.MakeSerializer(schemaType, writer);
                        serializer.Serialize(node);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            });
        }
        private Task<Node> ReadProjectFileAsync(string pathToFile, SourceSchemaType schemaType)
        {
            return Task.Run(() =>
            {
                try
                {
                    using (var stream = File.OpenRead(pathToFile))
                    {
                        var scanner = ScannerFactory.MakeScanner(schemaType);
                        var node = scanner.Scan(stream);

                        return node;
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            });
        }

        private Node GetUINodeState()
        {
            Node artificialRootNode = new Node("ArtificialRootNode");
            NodeStateConvertor.TransformToUiNode(_nodeMemento.NodeViewModel, artificialRootNode);

            var uiNode = artificialRootNode.Children.First();
            return uiNode;
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
            var node = new Node(vm.Name.Replace(' ','_'));

            var isExpandedState = new KeyValuePair<string, string>(IsExpandedAttr, vm.IsExpanded.ToString());
            var isSelectedState = new KeyValuePair<string, string>(IsSelectedAttr, vm.IsSelected.ToString());

            node.AttachAttribute(isExpandedState);
            node.AttachAttribute(isSelectedState);

            return node;
        }

        public static void FillViewNodeStateFrom(this NodeViewModel vm, Node sourceNode)
        {
            vm.IsSelected = Convert.ToBoolean(sourceNode.Attributes[IsSelectedAttr]);
            vm.IsExpanded = Convert.ToBoolean(sourceNode.Attributes[IsExpandedAttr]);
        }


        public static void TransformToUiNode(NodeViewModel vmNode, Node uiNode)
        {
            var currentUiNode = vmNode.ToUIStateNode();
            uiNode.Add(currentUiNode);

            foreach (var child in vmNode.Children)
                TransformToUiNode(child, currentUiNode);
        }

        public static void TransformToViewModelNode(NodeViewModel vmNode, Node uiNode)
        {
            vmNode.FillViewNodeStateFrom(uiNode);

            if (vmNode.Children.Count == 0 && vmNode.IsExpandable)
                vmNode.Children.Add(NodeViewModel.ArtificialChildNode);
            else
            {
                for (var i = 0; i < vmNode.Children.Count; i++)
                    TransformToViewModelNode(vmNode.Children[i], uiNode.Children[i]);
            }
        }
    }
    
}
