namespace ArcUploader
{
    using ArcUploader.Components;
    using ArcUploader.Config;
    using log4net;
    using log4net.Config;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Xml;

    /// <summary>
    /// Application entry point. Initializes and begins the collection job.
    /// Loads logging configuration from file system.
    /// </summary>
    class Application
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(LogCollector));
        static void Main(string[] args)
        {
            configureLoggers();
            printStartUp();

            new ArcJob().start();
            Console.ReadKey(true);
        }

        private static void printStartUp()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("ArcAgent Starting...");
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <summary>
        ///Configure Log4 properties - This is done as an in-memory literal to not require any physical files to be present on user's machhine
        ///Ultimately simplifying the instllation/start up process.
        /// </summary>
        private static void configureLoggers()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(UploaderConstants.LOG4NET_XML);
            XmlConfigurator.Configure(doc.DocumentElement);
        }
    }
}
