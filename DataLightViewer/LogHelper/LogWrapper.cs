using DataLightViewer.LogHelper;
using DataLightViewer.Mediator;
using System;

namespace DataLightViewer
{
    public static class LogWrapper
    {       
        public static void WriteInfo(string logInfo)
        {
            Logger.Log.Info(logInfo);
        }

        public static void WriteError(string logInfo, Exception ex = null)
        {
            Logger.Log.Error(logInfo, ex);
        }
    }
}
