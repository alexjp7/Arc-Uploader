﻿namespace ArcUploader.Components
{
    using ArcUploader.Config;
    using log4net;
    using SimpleJSON;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text;

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
            WebClient client = new WebClient();

            foreach(var log in logs)
            {
                byte[] responseBinary = client.UploadFile(UploaderConstants.UPLOAD_ENDPOINT, log.Value.FullName);
                string response = Encoding.UTF8.GetString(responseBinary);

                JSONNode json = JSON.Parse(response);
                string url = json[UploaderConstants.PERMA_LINK_PROPERTY]?.Value;
                logUrls.Add(log.Key,url);

                LOG.Debug($"[{log.Key}] - {url}");
            }

            return logUrls;
        }
    }
}
