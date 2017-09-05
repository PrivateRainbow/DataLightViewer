namespace DataLightViewer.Models
{
    public enum SavingProjectStrategy
    {
        /// <summary>
        /// App saves project by using current project folder path
        /// </summary>
        CurrentProjectFile = 0,

        /// <summary>
        /// App saves project to new folder which will be chosen by user
        /// </summary>
        NewProjectFile = 1,

        None = 2
    }
}
