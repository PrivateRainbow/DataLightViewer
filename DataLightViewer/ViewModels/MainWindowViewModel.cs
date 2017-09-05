using DataLightViewer.Commands;
using DataLightViewer.Helpers;
using DataLightViewer.Mediator;
using DataLightViewer.Memento;
using DataLightViewer.Views;
using DataLightViewer.Models;
using System.Windows;
using System.Windows.Input;
using System;
using System.Threading.Tasks;

namespace DataLightViewer.ViewModels
{
    public sealed class MainWindowViewModel : BaseViewModel
    {
        #region Data

        /// <summary>
        /// Hold the current path to the folder of the project
        /// </summary>
        private string _currentProjectFolderPath;
        private SavingProjectStrategy _saveStrategy = SavingProjectStrategy.None;

        #endregion

        #region ViewModels

        public NodePropertyViewModel PropertyViewModel { get; }
        public NodeSqlScriptViewModel SqlScriptViewModel { get; }
        public NodeTreeViewModel NodeTreeViewModel { get; }
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
            NodeTreeViewModel = new NodeTreeViewModel();
            PropertyViewModel = new NodePropertyViewModel();
            SqlScriptViewModel = new NodeSqlScriptViewModel();
            StatusViewModel = new StatusViewModel();

            OpenProjectCommand = new RelayCommand(() => OpenProject());
            CreateProjectCommand = new RelayCommand(() => CreateProject());
            SaveProjectCommand = new RelayCommand(() => SaveProject());
            SaveProjectAsCommand = new RelayCommand(() => SaveProjectAs());

            Messenger.Instance.Register<NodeMemento>(MessageType.MementoInitialized, async nm => await OnProjectSaving(nm));
        }

        #endregion

        #region Project Initialization

        private void CreateProject()
        {
            if (App.IsSessionInitialized)
                RunSafeProjectCreating();
            else
                RunProjectCreating();                           
        }

        private void OpenProject()
        {
            if (App.IsSessionInitialized)
                RunSafeProjectOpening();
            else
                RunDefaultProjectOpening();
        }

        /// <summary>
        /// Provide an opening of the project file with saving ability of the current project file.
        /// </summary>
        private async void RunSafeProjectOpening()
        {
            var saveMode = DialogHelper.SaveCurrentApplicationSession();

            switch (saveMode)
            {
                case AppSaveMode.WithoutSaving:
                    await RunProjectOpening();
                    break;

                case AppSaveMode.WithSaving:
                    InitProjectSaving();
                    await RunProjectOpening();
                    break;

                case AppSaveMode.CancelSaving:
                    break;
            }
        }

        private void RunSafeProjectCreating()
        {
            var saveMode = DialogHelper.SaveCurrentApplicationSession();

            switch (saveMode)
            {
                case AppSaveMode.WithoutSaving:
                    RunProjectCreating();
                    break;

                case AppSaveMode.WithSaving:
                    SaveProject();
                    RunProjectCreating();
                    break;

                case AppSaveMode.CancelSaving:
                    break;
            }
        }
        

        private async void RunDefaultProjectOpening() => await RunProjectOpening();

        private void RunProjectCreating()
        {
            new ObjectExplorerWindow().ShowDialog();
        }


        private async Task RunProjectOpening()
        {
            try
            {
                var folder = DialogHelper.GetFolderNameFromFolderDialog();
                if (string.IsNullOrEmpty(folder))
                    return;

                var appStateService = new AppStateService();
                await appStateService.OpenProjectAsync(folder);

                _currentProjectFolderPath = folder;
                _saveStrategy = SavingProjectStrategy.CurrentProjectFile;
                App.IsSessionInitialized = true;
            }
            catch (Exception)
            {
                App.IsSessionInitialized = false;
                _saveStrategy = SavingProjectStrategy.None;
            }
        }

        #endregion

        #region Saving Project

        public void SaveProject()
        {
            _saveStrategy = _saveStrategy == SavingProjectStrategy.None
                                         ? SavingProjectStrategy.NewProjectFile
                                         : SavingProjectStrategy.CurrentProjectFile;

            InitProjectSaving();
        }

        public void SaveProjectAs()
        {
            _saveStrategy = SavingProjectStrategy.NewProjectFile;
            InitProjectSaving();
        }

        private void InitProjectSaving()
        {
            if (App.IsSessionInitialized)
                Messenger.Instance.NotifyColleagues(MessageType.OnSavingProjectFile, this);
            else
                MessageBox.Show("Project is not initialized yet. Try to create a new one or open from project file", "DataToolsLight", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private async Task OnProjectSaving(NodeMemento memento)
        {
            await RunProjectSaving(memento, _saveStrategy);
        }

        private async Task RunProjectSaving(NodeMemento memento, SavingProjectStrategy strategy)
        {
            string folder = string.Empty;

            switch (strategy)
            {
                case SavingProjectStrategy.CurrentProjectFile:

                    folder = !string.IsNullOrEmpty(_currentProjectFolderPath)
                             ? _currentProjectFolderPath
                             : DialogHelper.GetFolderNameFromFolderDialog();

                    break;

                case SavingProjectStrategy.NewProjectFile:
                    folder = DialogHelper.GetFolderNameFromFolderDialog();
                    break;
            }

            if (string.IsNullOrEmpty(folder))
                return;

            var appStateService = new AppStateService(memento);
            await new AppStateService(memento).SaveProjectAsync(folder);

            _currentProjectFolderPath = folder;
        }

    }
    #endregion

}

