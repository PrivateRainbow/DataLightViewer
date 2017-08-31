using DataLightViewer.Commands;
using DataLightViewer.Helpers;
using DataLightViewer.Mediator;
using DataLightViewer.Memento;
using DataLightViewer.Views;
using System.Windows;
using System.Windows.Input;

namespace DataLightViewer.ViewModels
{
    public sealed class MainWindowViewModel : BaseViewModel
    {
        #region Data

        /// <summary>
        /// Store the current folder path of the project if it was saved.
        /// </summary>
        private string _currentProjectFolderPath;

        #endregion

        #region ViewModels

        public NodePropertyViewModel PropertyViewModel { get; }
        public NodeSqlScriptViewModel SqlScriptViewModel { get; }
        public StatusViewModel StatusViewModel { get; }

        #endregion

        #region Commands

        public ICommand CreateProjectCommand { get; }
        public ICommand OpenProjectCommand { get; }
        public ICommand SaveProjectCommand { get; }
        public ICommand SaveProjectAsCommand { get; }
        public ICommand ExitCommand { get; }

        #endregion

        #region Init

        public MainWindowViewModel()
        {
            PropertyViewModel = new NodePropertyViewModel();
            SqlScriptViewModel = new NodeSqlScriptViewModel();
            StatusViewModel = new StatusViewModel();

            OpenProjectCommand = new RelayCommand(() => OpenProject());
            CreateProjectCommand = new RelayCommand(() => CreateProject());
            SaveProjectCommand = new RelayCommand(() => SaveProject());
            SaveProjectAsCommand = new RelayCommand(() => SaveProjectAs());

            Messenger.Instance.Register<NodeMemento>(MessageType.MementoInitialized, nm => OnSaveProject(nm));
        }

        #endregion

        #region Methods

        public void SaveProject()
        {
            Messenger.Instance.NotifyColleagues(MessageType.OnSavingProjectFile, this);
        }

        private async void OpenProject()
        {
            var folder = DialogHelper.GetFolderNameFromFolderDialog();
            var appStateService = new AppStateService();
            await appStateService.OpenProjectAsync(folder);
        }

        private async void OnSaveProject(NodeMemento memento)
        {
            if (memento == null)
                MessageBox.Show("Project is currently empty. Try to create new one or open from project file", "Warning");

            var folder = DialogHelper.GetFolderNameFromFolderDialog();
            var appStateService = new AppStateService(memento);
            await appStateService.SaveProjectAsync(folder);
        }

        public void CreateProject()
        {
            new ObjectExplorerWindow().ShowDialog();
        }

        public void SaveProjectAs()
        {
            try
            {
                var filename = DialogHelper.GetFolderNameFromFolderDialog();
            }
            catch
            {

            }
        }

        #endregion

    }
}
