using MetroLog;


namespace Mirror.Logging
{
    class LoggerFactory
    {
        static LoggerFactory()
        {
            LogManagerFactory.DefaultConfiguration =
                LogManagerFactory.CreateLibraryDefaultSettings();
        }

        internal static ILogger Get<T>() => LogManagerFactory.DefaultLogManager.GetLogger<T>();

        internal static ILoggerAsync GetAsynchronous<T>() => Get<T>() as ILoggerAsync;
    }
}