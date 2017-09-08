using log4net;
using log4net.Config;

namespace DataLightViewer.LogHelper
{
    public static class Logger
    {
        public static readonly ILog Log = LogManager.GetLogger("DataToolsLightLogger");
        static Logger()
        {
            XmlConfigurator.Configure();       
        }
    }
}
