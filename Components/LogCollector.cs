namespace ArcUploader.Components
{
    using ArcUploader.Config;
    using log4net;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Collects logs from the log directory. The collected logs will be the latest run on the execution day.
    /// </summary>
    public class LogCollector
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(LogCollector));
        /// <summary>
        /// File extension for ArcDPS log files
        /// </summary>
        private static readonly string LOG_EXTENSION = "*.evtc";


        private Dictionary<string,DirectoryInfo> storedSnapshot;

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
            LOG.Debug("\nLog collection starting...");
            DirectoryInfo baseDirectory = new DirectoryInfo(config.baseDirectory);
            Dictionary<string, FileInfo> logFiles = new Dictionary<string, FileInfo>();

            List<DirectoryInfo> encounters = new List<DirectoryInfo>(baseDirectory.GetDirectories());
            if (encounters.Any())
            {
                encounters.RemoveAll(encounter => !config.encounters.Contains(encounter.Name));
            }
            else
            {
                throw new InvalidDataException("No encounters listed in config file. ");
            }

            foreach (var encounter in encounters)
            {
                logFiles.Add(encounter.Name, getLatestLog(encounter));
            }

            // No Logs found for the given day
            if (logFiles.All(encounter => encounter.Value == null))
            {
                throw new FileLoadException($"No daily runs found in {config.baseDirectory}.");
            }

            foreach (var log in logFiles)
            {
                if (log.Value != null)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    LOG.Debug($"Collected log for [{log.Key}] that was ran on [{log.Value.CreationTime}]");
                    Console.ForegroundColor = ConsoleColor.White;

                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    LOG.Debug($"No daily log available for {log.Key}");
                    Console.ForegroundColor = ConsoleColor.White;

                }
            }

            return logFiles;
        }


        /// <summary>
        /// Gathers all files in the given directory, then parses the creation time to obtain the latest run for a given log file.
        /// </summary>
        /// <param name="directory">The base directory of where the ArcDPs logs can be found.</param>
        /// <returns>The log file that was last created. </returns>
        private FileInfo getLatestLog(DirectoryInfo directory)
        {
            FileInfo latestFile = null;

            List<FileInfo> files = new List<FileInfo>(directory.GetFiles(LOG_EXTENSION));
            if(files.Any())
            {
                if(config.isDailyRun && !config.isPollingEnabled)
                {
                    files.RemoveAll(log => log.CreationTime.Date != DateTime.Today);
                }

                if(files.Any())
                { 
                    files.Sort((x, y) => y.CreationTime.CompareTo(x.CreationTime));
                    latestFile = files[0];
                }
            }

            return latestFile;
        }

        /// <summary>
        /// Compare each log between stored snapshot and current encoutners
        /// If any current encounters have newer "last write time" then any stored encounters, this method will:
        /// <list type="number">
        /// <item> Get the latest log and place it in the results map</item>
        /// <item> Update the snapshot.</item>
        /// </list>
        /// </summary>
        /// <param name="currentDirectory">The base directory where ArcDPS logs are stored</param>
        /// <returns></returns>
        public Dictionary<string, FileInfo> pollChanges(Dictionary<string, DirectoryInfo> currentDirectory)
        {
            if(storedSnapshot == null)
            {
                storedSnapshot = currentDirectory;
                return null;
            }

            Dictionary<string, FileInfo> newLogs = new Dictionary<string, FileInfo>();

            foreach(var storedEncounter in storedSnapshot)
            {
                if (currentDirectory[storedEncounter.Key].LastWriteTime > storedEncounter.Value.LastWriteTime)
                {
                    LOG.Debug("New log found for: " + storedEncounter.Key);
                    FileInfo log = getLatestLog(storedEncounter.Value);

                    newLogs.Add(storedEncounter.Key, log);
                }
            }

            // Update snapshot if any new logs are found.
            if(newLogs.Any())
            {
                storedSnapshot = currentDirectory;
            }
            else
            {
                newLogs = null;
            }

            return newLogs;
        }

        /// <summary>
        /// Utility method to create a dictionary which maps folder names to folder contents. 
        /// This method provides the ability to perform folder name look ups for convience of folder comparison.
        /// </summary>
        /// <param name="path">The path to create a directory map off</param>
        /// <returns>A mapping between folder names and their contents. </returns>
        public static Dictionary<string, DirectoryInfo> createDirectoryMap(string path)
        {
            DirectoryInfo directory = new DirectoryInfo(path);
            Dictionary<string, DirectoryInfo> results = new Dictionary<string, DirectoryInfo>();
            List<DirectoryInfo> directories = new List<DirectoryInfo>(directory.GetDirectories());

            directories.ForEach(encounter =>
            {
                results.Add(encounter.Name, encounter);
            });

            return results;
        }
    }
}
