using DataLightViewer.Models;
using System.Windows;
using System.Windows.Forms;

namespace DataLightViewer.Helpers
{
    public struct DialogBundle
    {
        public string ExtentionByDefault { get; set; }
        public string Filter { get; set; }
    }

    public static class DialogHelper
    {
        #region Data

        private static readonly DialogBundle _dialogTextBundle = new DialogBundle { ExtentionByDefault = ".txt", Filter = "Text documents (.txt)|*.txt" };
        private static readonly DialogBundle _dialogProjectBundle = new DialogBundle { ExtentionByDefault = ".dtml", Filter = "Data Tools Light project (.dtml)|*.dtml" };

        #endregion

        public static string GetFileNameFromSaveTextDialog() => GetFileNameFromSaveDialog(_dialogTextBundle);
        public static string GetFileNameFromSaveProjectDialog() => GetFileNameFromSaveDialog(_dialogProjectBundle);

        #region Helpers

        /// <summary>
        /// Getting/setting the directory for opening/saving project file.
        /// </summary>
        public static string GetFolderNameFromFolderDialog()
        {
            string selectedPath = null;
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                    selectedPath = dialog.SelectedPath;
            }
            return selectedPath;
        }

        public static string GetFileNameFromSaveDialog(DialogBundle bundle)
        {
            var dialog = new SaveFileDialog() { DefaultExt = bundle.ExtentionByDefault, Filter = bundle.Filter };

            if (dialog.ShowDialog() == DialogResult.OK)
                return dialog.FileName;
            else
                return string.Empty;
        }
        
        public static AppSaveMode SaveCurrentApplicationSession(bool withCancelation = true)
        {
            var msgBoxBtns = withCancelation ? MessageBoxButton.YesNoCancel : MessageBoxButton.YesNo;
            var result = System.Windows.MessageBox.Show("Would you like to save the current working session?", 
                                                        "DataToolsLight",
                                                        msgBoxBtns, 
                                                        MessageBoxImage.Question);

            switch(result)
            {
                case MessageBoxResult.Yes:
                    return AppSaveMode.WithSaving;

                case MessageBoxResult.No:
                    return AppSaveMode.WithoutSaving;

                case MessageBoxResult.Cancel:
                    return AppSaveMode.CancelSaving;

                default:
                    return AppSaveMode.None;
            }
        }

        #endregion 
    }
}
