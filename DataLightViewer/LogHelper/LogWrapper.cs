using DataLightViewer.LogHelper;
using DataLightViewer.Mediator;
using System;

namespace DataLightViewer
{
    public static class LogWrapper
    {       
        public static void WriteInfo(string logInfo, string statusInfo = null)
        {
            Logger.Log.Info(logInfo);
            Messenger.Instance.Notify(MessageType.ExecutionStatus, statusInfo);
        }

        public static void WriteError(string logInfo, Exception ex = null, string statusInfo = null)
        {
            Logger.Log.Error(logInfo, ex);
            Messenger.Instance.Notify(MessageType.ExecutionStatus, statusInfo);
        }
    }
}
