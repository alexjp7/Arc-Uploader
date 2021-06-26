namespace ArcUploader.Components
{
    using ArcUploader.Config;
    using log4net;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
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
        /// The upload and collection of logs can either be performed in bulk based on the currently stored logs,
        /// or dynamically through file system polling.
        /// </summary>
        public void start()
        {
            if(config.isPollingEnabled)
            {
                startPolling();
            }
            else
            {
                startBulkUpload();
            }
        }

        /// <summary>
        /// Performs a bulk uploader with the latest logs stored.
        /// </summary>
        private void startBulkUpload()
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
            finally
            {
                LOG.Debug("\nArcAgent finnished!");
            }
        }

        /// <summary>
        /// Runs polling mode that scans for new uploads through 
        /// comparing last write time to a stored snapshot of the ArcDPS boss folders.
        /// </summary>
        private void startPolling()
        {
            LOG.Debug("\nStarting log polling....");
            LogUploader uploader = new LogUploader();
            LogCollector logCollector = new LogCollector();

            while (true)
            {
                Dictionary<string, DirectoryInfo> baseDirectoryMap = LogCollector.createDirectoryMap(config.baseDirectory);
                Dictionary<string, FileInfo> logs = logCollector.pollChanges(baseDirectoryMap);

                if (logs != null)
                {
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

            StreamWriter file = new StreamWriter(filePath, true);
            file.WriteLine(DateTime.Now.TimeOfDay.ToString());

            foreach (string line in results.Select(result => $"{result.Key} - {result.Value}"))
            {
                file.WriteLine(line);
            }

            file.Close();
        }

        /// <summary>
        /// Performs any actions after uploading/collection is completed. 
        /// </summary>
        /// <param name="results">The list of results that were uploaded.</param>
        private void postProcessActions(Dictionary<string, string> results)
        {
            if(!config.isAutoBrowserOpenEnabled)
            {
                return;
            }

            LOG.Debug($"\nOpening browser from path {config.browserAppPath}");
            foreach(var result in results)
            {
                Process.Start(config.browserAppPath, result.Value);
            }

        }

    }
}
