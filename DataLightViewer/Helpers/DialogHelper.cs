using DataLightViewer.Models;
using System.Windows;
using System.Windows.Forms;
using System;
using System.Text.RegularExpressions;
using System.IO;

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

        public static readonly DialogBundle DialogSqlBundle = new DialogBundle { ExtentionByDefault = ".sql", Filter = "Sql Files (*.sql)|*.sql|All Files (*.*)|*.*" };
        public static readonly DialogBundle DialogTextBundle = new DialogBundle { ExtentionByDefault = ".txt", Filter = "Text documents (.txt)|*.txt" };
        public static readonly DialogBundle DialogProjectBundle = new DialogBundle { ExtentionByDefault = ".dtml", Filter = "Data Tools Light project (.dtml)|*.dtml" };

        #endregion

        public static string GetFileNameFromSaveTextDialog() => GetFileNameFromSaveDialog(DialogTextBundle);
        public static string GetFileNameFromSaveProjectDialog() => GetFileNameFromSaveDialog(DialogProjectBundle);

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

        public static bool ConfirmEnteredServerConnection()
        {
            var result = System.Windows.MessageBox.Show("The connection to server has changed. Would you like to continue with new connection for the current project !?",
                                                        "DataToolsLight",
                                                        MessageBoxButton.YesNo,
                                                        MessageBoxImage.Question);
            switch(result)
            {
                case MessageBoxResult.Yes:
                    return true;
                case MessageBoxResult.No:
                    return false;

                default:
                    throw new ArgumentException($"{nameof(result)}");                    
            }
        }

        public static bool IsValidFilename(string testName)
        {
            Regex unspupportedRegex = new Regex("(^(PRN|AUX|NUL|CON|COM[1-9]|LPT[1-9]|(\\.+)$)(\\..*)?$)|(([‌​\\x00-\\x1f\\\\?*:\"‌​;‌​|/<>])+)|([\\. ]+)", RegexOptions.IgnoreCase);
            if (unspupportedRegex.IsMatch(testName)) { return false; };

            return true;
        }
        #endregion 
    }
}
