namespace ArcUploader.Components
{
    using ArcUploader.Config;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Collects logs from the log directory. The collected logs will be the latest run on the execution day.
    /// </summary>
    public class LogCollector
    {
        /// <summary>
        /// File extension for ArcDPS log files
        /// </summary>
        private static readonly string LOG_EXTENSION = "*.evtc";

        /// <summary>
        /// Config instance containing runntime configurations as found in the config.json
        /// </summary>
        private static UploaderConfig config;

        /// <summary>
        /// Instantiates log colelction configuration
        /// throws FileNotFoundExeption when the config file is not found.
        /// </summary>
        public LogCollector()
        {
            config = UploaderConfig.INSTANCE;
        }

        /// <summary>
        /// Collects the list of logs that are most recent on a given day.
        /// </summary>
        /// <returns> A dictionary containing the encounter name and the actually file (in memory).</returns>
        public Dictionary<string, FileInfo> collect()
        {
            Console.WriteLine("Collection Begun...");
            DirectoryInfo baseDirectory = new DirectoryInfo(config.baseDirectory);
            Dictionary<string,FileInfo> logFiles = new Dictionary<string, FileInfo>();

            List<DirectoryInfo> encounters = new List<DirectoryInfo>(baseDirectory.GetDirectories());
            if(encounters.Any())
            {
                 encounters.RemoveAll(encounter => !config.encounters.Contains(encounter.Name));
            }
            else
            {
                throw new InvalidDataException("No encounters listed in config file. ");
            }
    
            foreach(var encounter in encounters)
            {
                logFiles.Add(encounter.Name, getLatestDailyLog(encounter));
            }

            // No Logs found for the given day
            if(encounters.All(encounter => encounter == null))
            {
                throw new FileLoadException($"No daily runs found in {config.baseDirectory}.");
            }

            foreach(var log in logFiles)
            {
                if(log.Value != null)
                {
                    Console.WriteLine($"Collected log for [{log.Key}] that was ran on [{log.Value.CreationTime}]");
                }
                else
                {
                    Console.WriteLine($"No daily log available for {log.Key}");
                }
            }

            return logFiles;
        }

        /// <summary>
        /// Gathers all files in the given directory, then parses the creation time to obtain the latest run for a given log file.
        /// </summary>
        /// <param name="directory">The base directory of where the ArcDPs logs can be found.</param>
        /// <returns>The log file that was last created. </returns>
        private FileInfo getLatestDailyLog(DirectoryInfo directory)
        {
            FileInfo latestFile = null;

            List<FileInfo> files = new List<FileInfo>(directory.GetFiles(LOG_EXTENSION));
            if(files.Any())
            {
                files.RemoveAll(log => log.CreationTime.Date != DateTime.Today);
                if(files.Any())
                { 
                    files.Sort((x, y) => y.CreationTime.CompareTo(x.CreationTime));
                    latestFile = files[0];
                }
            }

            return latestFile;
        }
    }
}
