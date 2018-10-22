using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using System.Reflection;
using log4net.Config;

namespace ShareP
{
    static class Log
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        static Log()
        {
            XmlConfigurator.Configure();  //Configure logging
        }
        public static void LogInfo(string text)
        {
            _log.Info(text);
        }

        public static void LogException(Exception ex, String message = "")
        {
            _log.Error(message, ex);
        }

        public static void LogUnhandled(Exception ex)
        {
            _log.Fatal("Unhandled exception! ", ex);
        }
    }
}
