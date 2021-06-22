namespace ArcUploader.Components
{
    using ArcUploader.Config;
    using log4net;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;

    /// <summary>
    /// Defines the overarching process and activties for uploading the most recently created ArcDPS logs.
    /// </summary>
    class ArcJob
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(LogUploader));

        private static readonly string LOG_EXTENSION = ".log";
        /// <summary>
        /// Config instance containing runntime configurations as found in the config.json
        /// </summary>
        private static UploaderConfig config;

        public ArcJob()
        {
            config = UploaderConfig.INSTANCE;
        }

        /// <summary>
        /// Defines and executes the runtime components for the collection and uploading of ArcDPS logs.
        /// </summary>
        public void start()
        {
            if(config.isPollingEnabled)
            {
                startPolling();
            }
            else
            {
                startStaticUpload();
            }
        }

        private void startStaticUpload()
        {
            try
            {
                Dictionary<string, FileInfo> logs = new LogCollector().collect();
                LogUploader uploader = new LogUploader();
                Dictionary<string, string> results = uploader.upload(logs);

                writeOutput(results);
                postProcessActions(results);
            }
            catch (FileNotFoundException e)
            {
                LOG.Error($"Configuration file could not be found. Ensure a {UploaderConstants.CONFIG_PATH} file is present.", e);
            }
            catch (InvalidDataException e)
            {
                LOG.Error($"Configuration contains invalid values - {e.Message}", e);
            }
            catch (Exception e)
            {
                LOG.Error(e.Message, e);
            }
        }

        private void startPolling()
        {
            LOG.Debug("Starting log polling....");
            LogUploader uploader = new LogUploader();

            while (true)
            {
                Dictionary<string, FileInfo> logs = new LogCollector().pollChanges();
                if(logs != null)
                {
                    foreach(string log in logs.Keys)
                    {
                        LOG.Debug("New log found for " + log);
                    }

                    Dictionary<string, string> results = uploader.upload(logs);

                    writeOutput(results);
                    postProcessActions(results);
                }
                
                Thread.Sleep(5000);
            }
        }

        /// <summary>
        /// Writes daily runs to output directory.
        /// </summary>
        /// <param name="results">The key-pair mapping of an encounter to URL.</param>
        private void writeOutput(Dictionary<string, string> results)
        {
            var fileName = $"{DateTime.Today.Day}-{DateTime.Today.Month}-{DateTime.Today.Year}";
            var filePath = $"{config.outputDirectory}/{fileName}{LOG_EXTENSION}";

            Directory.CreateDirectory(config.outputDirectory);

            StreamWriter file = new StreamWriter(filePath,true);
            file.WriteLine(DateTime.Now.TimeOfDay.ToString());

            foreach (string line in results.Select(result => $"{result.Key} - {result.Value}"))
            {
                file.WriteLine(line);
            }

            file.Close();
        }

        private void postProcessActions(Dictionary<string, string> results)
        {
            if(!config.isAutoBrowserOpenEnabled)
            {
                return;
            }

            LOG.Debug($"Opening browser from path {config.browserAppPath}");
            foreach(var result in results)
            {
                Process.Start(config.browserAppPath, result.Value);
            }

        }

    }
}
