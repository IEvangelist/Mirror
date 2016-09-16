using MetroLog;


namespace Mirror.Logging
{
    public class LoggerFactory
    {
        static LoggerFactory()
        {
            LogManagerFactory.DefaultConfiguration =
                LogManagerFactory.CreateLibraryDefaultSettings();
        }

        public static ILogger Get<T>() => LogManagerFactory.DefaultLogManager.GetLogger<T>();

        public static ILoggerAsync GetAsynchronous<T>() => Get<T>() as ILoggerAsync;
    }
}