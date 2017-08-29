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
        public static string GetFileNameFromOpenTextDialog() => GetFileNameFromOpenDialog(_dialogTextBundle);

        public static string GetFileNameFromSaveProjectDialog() => GetFileNameFromSaveDialog(_dialogProjectBundle);
        public static string GetFileNameFromOpenProjectDialog() => GetFileNameFromOpenDialog(_dialogProjectBundle);

        #region Helpers

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
        public static string GetFileNameFromOpenDialog(DialogBundle bundle)
        {
            var dialog = new SaveFileDialog() { DefaultExt = bundle.ExtentionByDefault, Filter = bundle.Filter };
            if (dialog.ShowDialog() == DialogResult.OK)
                return dialog.FileName;
            else
                return string.Empty;
        }

        #endregion 
    }
}
