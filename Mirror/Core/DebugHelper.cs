using MetroLog;
using Mirror.Logging;
using System;
using System.Diagnostics;


namespace Mirror.Core
{
    static class DebugHelper
    {
        internal static bool IsNotHandled<T>(Exception ex) where T : class
        {
            bool isHandled = true;

            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }
            else
            {
                var logger = LoggerFactory.Get<T>();
                logger?.Error(ex.Message, ex);

                isHandled = false;
            }

            return isHandled == false;
        }
    }
}