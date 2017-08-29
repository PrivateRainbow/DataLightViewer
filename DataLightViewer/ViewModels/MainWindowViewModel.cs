using DataLightViewer.Commands;
using DataLightViewer.Helpers;
using DataLightViewer.Views;
using System.Windows.Input;

namespace DataLightViewer.ViewModels
{
    public sealed class MainWindowViewModel : BaseViewModel
    {
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
        }

        #endregion

        #region Methods

        public void OpenProject()
        {
            try
            {
                var filename = DialogHelper.GetFolderNameFromFolderDialog();
            }
            catch
            {

            }
        }
        public void CreateProject()
        {
            new ObjectExplorerWindow().ShowDialog();
        }
        public void SaveProject()
        {
            try
            {
                //var filename = DialogHelper.GetFolderNameFromFolderDialog();
            }
            catch
            {

            }
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
