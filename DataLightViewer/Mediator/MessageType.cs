namespace DataLightViewer.Mediator
{

    public enum MessageType
    {
        /// <summary>
        /// Trigger on each user selection on the NodeTreeView
        /// </summary>
        NodeSelection,

        /// <summary>
        /// Trigger when the construction of Sql was requested (lazy)
        /// </summary>
        SqlPreparation,

        /// <summary>
        /// Trigger when we wanna show the execution of background operation
        /// </summary>
        ExecutionStatus,

        /// <summary>
        /// Trigger when new project was created and connection to destination server is succesfully established
        /// </summary>
        ConnectionEstablished,

        /// <summary>
        /// Trigger when working session must be saved
        /// </summary>
        SaveProjectFile,

        /// <summary>
        /// Trigger when project file is opened
        /// </summary>
        FileProjectOpened
    }

}
