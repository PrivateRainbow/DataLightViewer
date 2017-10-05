using DataLightViewer.ViewModels;
using Loader.Components;
using Loader.Factories;
using Loader.Types;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DataLightViewer.Mediator;
using System.Xml.Schema;
using Loader.Scanners;
using DataLightViewer.Helpers;
using DataLightViewer.Services;

namespace DataLightViewer.Memento
{

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

        private async void SaveProject(string folderPath)
        {
            try
            {
                LogWrapper.WriteInfo($"Saving the project file by folder path ({folderPath})");

                var savingTask = new ExecutingTask("Saving project files ...");
                TaskExecutionMonitor.AttachMonitoring(savingTask);


                var dataStateFilePath = Path.Combine(folderPath, FullDataStateFileName);
                var uiStateFilePath = Path.Combine(folderPath, FullUiStateFileName);

                var dataNode = _nodeMemento.NodeViewModel.InnerNode;
                var uiNode = GetUINodeState();

                dataNode.Attributes[ProjectFileTypeLabel] = DefaultDataStateFileName;
                dataNode.Attributes[App.ServerConnectionStringLiteral] = App.ServerConnectionString.Encrypt();
                uiNode.Attributes[ProjectFileTypeLabel] = DefaultUiStateFileName;

                var writeDataTask = WriteProjectFileAsync(dataStateFilePath, dataNode, SourceSchemaType.Database);
                var writeUiTask = WriteProjectFileAsync(uiStateFilePath, uiNode, SourceSchemaType.File);

                await Task.WhenAll(writeDataTask, writeUiTask);

                LogWrapper.WriteInfo($"Project was save by path ({folderPath})");

                TaskExecutionMonitor.DetachMonitoring(savingTask);
                TaskExecutionMonitor.MonitorOneTime(new ExecutingTask("Saved."));
            }
            catch (Exception ex)
            {
                var msg = "Error on saving the project.";

                LogWrapper.WriteError(msg, ex.InnerException);
                TaskExecutionMonitor.MonitorOneTime(new ExecutingTask("Not Saved."));

                MessageBox.Show(msg, "DataToolsLight", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async Task OpenProject(string folderPath)
        {
            try
            {
                LogWrapper.WriteInfo($"Opening the project file by path ({folderPath})");
                var openingTask = new ExecutingTask("Opening project files ...");
                TaskExecutionMonitor.AttachMonitoring(openingTask);

                var dataStateFilePath = Path.Combine(folderPath, FullDataStateFileName);
                var uiStateFilePath = Path.Combine(folderPath, FullUiStateFileName);

                var readDataTask = ReadProjectFileAsync(dataStateFilePath, SourceSchemaType.Database, ProjectFileType.Data.GetSchema());
                var readUiTask = ReadProjectFileAsync(uiStateFilePath, SourceSchemaType.File, ProjectFileType.UI.GetSchema());

                await Task.WhenAll(readDataTask, readUiTask).ContinueWith(nodes => InitializeMemento(nodes));

                LogWrapper.WriteInfo($"Project was opened by path ({folderPath})");

                TaskExecutionMonitor.DetachMonitoring(openingTask);
                TaskExecutionMonitor.MonitorOneTime(new ExecutingTask("Opened."));
            }
            catch(InvalidOperationException ex)
            {
                var msg = "Error on opening the project. Incorrect project type";

                LogWrapper.WriteError(ex.Message, ex);
                TaskExecutionMonitor.MonitorOneTime(new ExecutingTask("File was not valid."));

                MessageBox.Show(msg, "DataToolsLight", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
            catch (Exception ex)
            {
                var msg = "Error on opening the project.";

                LogWrapper.WriteError(msg, ex.InnerException);
                TaskExecutionMonitor.MonitorOneTime(new ExecutingTask("Not Opened."));

                MessageBox.Show(msg, "DataToolsLight", MessageBoxButton.OK, MessageBoxImage.Error);
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
                catch (Exception ex)
                {
                    throw;
                }
            });
        }
        private Task<Node> ReadProjectFileAsync(string pathToFile, SourceSchemaType schemaType, XmlSchemaSet validationSchema = null)
        {
            return Task.Run(() =>
            {
                try
                {
                    if (!File.Exists(pathToFile))
                        throw new FileNotFoundException($"{nameof(pathToFile)} is not valid!");

                    using (var stream = File.OpenRead(pathToFile))
                    {
                        var scanner = ScannerFactory.Make(schemaType, stream, new XmlNodeValidationContext(validationSchema));
                        var node = scanner.Scan();
                        
                        return node;
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            });
        }

        private void InitializeMemento(Task<Node[]> nodes)
        {
            try
            {
                var initializedNodes = nodes.Result.ToList();
                var dataNode = initializedNodes.Find(n => n.Attributes[ProjectFileTypeLabel] == DefaultDataStateFileName);
                var uiNode = initializedNodes.Find(n => n.Attributes[ProjectFileTypeLabel] == DefaultUiStateFileName);

                dataNode.Attributes.Remove(ProjectFileTypeLabel);
                uiNode.Attributes.Remove(ProjectFileTypeLabel);

                App.ServerConnectionString = string.Copy(dataNode.Attributes[App.ServerConnectionStringLiteral]).Decrypt();
                dataNode.Attributes.Remove(App.ServerConnectionStringLiteral);

                var viewModel = new NodeViewModel(dataNode, parent: null, lazyLoadChildren: false);
                NodeStateConvertor.TransformToViewModelNode(viewModel, uiNode);

                var memento = new NodeMemento(viewModel);
                Messenger.Instance.NotifyColleagues(MessageType.OnOpeningProjectFile, memento);
            }
            catch (AggregateException)
            {
                throw;
            }
        }

        private Node GetUINodeState()
        {
            Node artificialRootNode = new Node("ArtificialRootNode");
            NodeStateConvertor.TransformToUiNode(_nodeMemento.NodeViewModel, artificialRootNode);

            var uiNode = artificialRootNode.Children.First();
            return uiNode;
        }

    }

}
