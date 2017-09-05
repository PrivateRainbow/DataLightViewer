namespace DataLightViewer.Mediator
{

    public static class MessageType
    {
        /// <summary>
        /// Trigger on each user selection on the NodeTreeView
        /// </summary>
        public const string NodeSelection = nameof(NodeSelection);

        /// <summary>
        /// Trigger when the construction of Sql was requested (lazy)
        /// </summary>
        public const string SqlPreparation = nameof(SqlPreparation);

        /// <summary>
        /// Trigger when we wanna show the execution of background operation
        /// </summary>
        public const string ExecutionStatus = nameof(ExecutionStatus);


        /// <summary>
        /// Trigger when new project has been created by establishing the connection to destination server
        /// </summary>
        public const string OnInitializingProjectFile = nameof(OnInitializingProjectFile);

        /// <summary>
        /// Trigger when working session must be saved
        /// </summary>
        public const string OnSavingProjectFile = nameof(OnSavingProjectFile);

        /// <summary>
        /// Trigger when project file is opened
        /// </summary>
        public const string OnOpeningProjectFile = nameof(OnOpeningProjectFile);


        /// <summary>
        /// Trigger when we get the current state of application (data, ui)
        /// </summary>
        public const string MementoInitialized = nameof(MementoInitialized);

    }

}
