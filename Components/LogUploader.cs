namespace ArcUploader.Components
{
    using ArcUploader.Config;
    using log4net;
    using SimpleJSON;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// Component for performing the upload action to the ArcDPS upload server.
    /// </summary>
    class LogUploader
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(LogUploader));

        /// <summary>
        /// Uploads each log individually, and logs out the the ArcDps permalink for a given encounter.
        /// </summary>
        /// <param name="logs">A dictionary of the encounter names with their respective log files</param>
        /// <returns>A dictionary of the encounter names with their respective ArcDps perma links.</returns>
        public Dictionary<string,string> upload(Dictionary<string, FileInfo> logs)
        {
            LOG.Debug("\nUploading Begun...");
            Dictionary<string, string> logUrls = new Dictionary<string, string>();
            List<Thread> uploaders = new List<Thread>();
            foreach(var log in logs)
            {
                Thread uploaderThread = new Thread(() => uploaderJob(log, logUrls));
                uploaderThread.Start();
                uploaders.Add(uploaderThread);
            }

            foreach(Thread thread in uploaders)
            {
                thread.Join();
            }

            return logUrls;
        }

        /// <summary>
        /// Uploading worker that performs the actual file upload
        /// </summary>
        /// <param name="log">The log to be uploaded</param>
        /// <param name="logUrls">The result set that will be added to</param>
        private void uploaderJob(KeyValuePair<string, FileInfo> log, Dictionary<string, string> logUrls)
        {
            try
            {
                WebClient client = new WebClient();
                byte[] responseBinary = client.UploadFile(UploaderConstants.UPLOAD_ENDPOINT, log.Value.FullName);
                string response = Encoding.UTF8.GetString(responseBinary);

                JSONNode json = JSON.Parse(response);
                string url = json[UploaderConstants.PERMA_LINK_PROPERTY]?.Value;
                logUrls.Add(log.Key, url);

                Console.ForegroundColor = ConsoleColor.Cyan;
                LOG.Debug($"[{log.Key}] - {url}");
                Console.ForegroundColor = ConsoleColor.White;
            }
            catch (WebException e)
            {
                LOG.Error($"Failed to upload log file to ArcDPS upload server.", e);
            }
        }
    }
}
