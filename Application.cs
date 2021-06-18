namespace ArcUploader
{
    using ArcUploader.Components;
    using ArcUploader.Config;
    using log4net;
    using log4net.Config;
    using System;
    using System.Diagnostics;
    using System.IO;

    /// <summary>
    /// Application entry point. Initializes and begins the collection job.
    /// Loads logging configuration from file system.
    /// </summary>
    class Application
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(LogCollector));
        static void Main(string[] args)
        {
            try
            {
                FileInfo file = new FileInfo(UploaderConstants.LOG4NET_CONFIG);
                XmlConfigurator.Configure(file);
                new ArcJob().start();
            }
            catch(Exception e)
            {
                BasicConfigurator.Configure();
                LOG.Error(e.Message);
            }

            Console.ReadKey(true);
        }
    }
}
