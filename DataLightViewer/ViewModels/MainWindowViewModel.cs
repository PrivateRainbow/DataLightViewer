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
    /// <summary>
    /// Hello, comrades
    /// I hope u enjoy (=
    /// </summary>
    public class MainWindowViewModel : BaseViewModel
    {
        #region Data

        private string _currentProjectFolderPath;
        private SavingProjectStrategy _saveStrategy = SavingProjectStrategy.None;

        #endregion

        #region ViewModels

        public NodePropertyViewModel PropertyViewModel { get; }
        public NodeSqlScriptViewModel SqlScriptViewModel { get; }
        public NodeTreeViewModel NodeTreeViewModel { get; }
        public StatusViewModel StatusViewModel { get; }

        public ExplorerViewModel ExplorerViewModel { get; internal set; }
        #endregion

        #region Commands

        public ICommand CreateProjectCommand { get; }
        public ICommand OpenProjectCommand { get; }
        public ICommand SaveProjectCommand { get; }
        public ICommand SaveProjectAsCommand { get; }

        public ICommand ConnectToServerCommand { get; }

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
            SaveProjectCommand = new RelayCommand(() => SaveProject(), (o) => App.IsSessionInitialized);
            SaveProjectAsCommand = new RelayCommand(() => SaveProjectAs(), (o) => App.IsSessionInitialized);

            ConnectToServerCommand = new RelayCommand(() => RunProjectInitialization(), (o) => App.WorkState == AppWorkState.Offline);

            Messenger.Instance.Register<NodeMemento>(MessageType.MementoInitialized, async nm => await OnProjectSaving(nm));
        }

        #endregion

        #region Project Initialization

        private void CreateProject()
        {
            if (App.IsSessionInitialized)
                RunSafeProjectCreating();
            else
                RunProjectInitialization();
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
        private async void RunDefaultProjectOpening() => await RunProjectOpening();

        private void RunSafeProjectCreating()
        {
            var saveMode = DialogHelper.SaveCurrentApplicationSession();

            switch (saveMode)
            {
                case AppSaveMode.WithoutSaving:
                    RunProjectInitialization();
                    break;

                case AppSaveMode.WithSaving:
                    SaveProject();
                    RunProjectInitialization();
                    break;

                case AppSaveMode.CancelSaving:
                    break;
            }
        }

        private void RunProjectInitialization()
        {
            Refresh();

            var connectionMode = App.WorkState == AppWorkState.Offline
                                                  ? AppConnectionMode.Reopen
                                                  : AppConnectionMode.New;

            ExplorerViewModel = new ExplorerViewModel(connectionMode);
            var explorer = new ObjectExplorerWindow(ExplorerViewModel);
            explorer.ShowDialog();
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
                App.WorkState = AppWorkState.Offline;
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

        private void Refresh()
        {
            SqlScriptViewModel.ClearScriptCommand.Execute(null);
        }

        #endregion

        public void AppShutDownHandler(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var result = DialogHelper.SaveCurrentApplicationSession();

            switch (result)
            {
                case AppSaveMode.WithSaving:
                    SaveProject();
                    break;

                case AppSaveMode.CancelSaving:
                    e.Cancel = true;
                    break;

                default:
                    break;
            }
        }

    }


}

