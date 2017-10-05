namespace DataLightViewer.Models
{
    public enum AuthenticationType
    {
        Windows,
        SqlServer
    }

    public enum AppSaveMode
    {
        WithSaving = 0,
        WithoutSaving = 1,
        CancelSaving = 2,
        None = 3
    }

    public enum SavingProjectStrategy
    {
        CurrentProjectFile = 0,
        NewProjectFile = 1,
        None = 2
    }

    public enum AppConnectionMode
    {
        New = 0,
        Reopen = 1,
    }

    public enum AppWorkState
    {
        Online = 0,
        Offline = 1
    }
}
