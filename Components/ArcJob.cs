namespace ArcUploader.Components
{
    using ArcUploader.Config;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the overarching process and activties for uploading the most recently created ArcDPS logs.
    /// </summary>
    class ArcJob
    {
        private static readonly string LOG_EXTENSION = ".log";
        private static readonly string RESULTS_DIRECTORY = "results";

        /// <summary>
        /// Defines and executes the runtime components for the collection and uploading of ArcDPS logs.
        /// </summary>
        public void start()
        {
            try
            {
                Dictionary<string, FileInfo> logs = new LogCollector().collect();
                LogUploader uploader = new LogUploader();
                Dictionary<string, string> results = uploader.upload(logs);
                
                // Writes output to output directory
                writeOutput(results);
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"Configuration file could not be found. Ensure a {UploaderConstants.CONFIG_PATH} file is present.");
            }
            catch(InvalidDataException e)
            {
                Console.WriteLine($"Configuration contains invalid values - {e.Message}");
            }
            catch(Exception e)
            {       
                Console.WriteLine(e.Message);
            }
              
        }

        private void writeOutput(Dictionary<string, string> results)
        {
            var fileName = $"{DateTime.Today.Day}-{DateTime.Today.Month}-{DateTime.Today.Year}";
            var filePath = $"{RESULTS_DIRECTORY}/{fileName}{LOG_EXTENSION}";

            Directory.CreateDirectory(RESULTS_DIRECTORY);

            StreamWriter file = new StreamWriter(filePath);
            foreach (string line in results.Select(result => $"{result.Key} - {result.Value}"))
            {
                Console.WriteLine(line);
                file.WriteLine(line);
            }

            file.Close();
        }
    }
}
