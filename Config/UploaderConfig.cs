namespace ArcUploader.Config
{
    using log4net;
    using SimpleJSON;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Runntime configuration for the collection job.  The config
    /// for this application can be found in the config.json file.
    /// </summary>
    class UploaderConfig
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(UploaderConfig));

        /// <summary>
        /// An inclusive list of encounters which will be used to determine which boss encounters to collect.
        /// </summary>
        public List<string> encounters { get; private set; }

        /// <summary>
        /// The base ArcDPS log directory, where the boss encounter sub-directories can be found.
        /// </summary>
        public string baseDirectory { get; private set; }

        /// <summary>
        /// Output directory of where to save output file containing the list of daily encounter URLs
        /// If no output directory is provided, a default wil be used as <see cref="UploaderConstants.DEFAULT_OUTPUT_DIRECTORY" >Default directory.</see>
        /// </summary>
        public string outputDirectory { get; private set; }

        /// <summary>
        /// Flag provided by user configuration to automatically open a web-browser window show each encoutner in seperate tabs
        /// </summary>
        public bool isAutoBrowserOpenEnabled { get; private set; }

        /// <summary>
        /// The directory path to the user's web-browser executable.
        /// </summary>
        public string browserAppPath {get;private set;}

        public bool isDailyRun { get; private set; }

        //Singleton instance holder
        private static UploaderConfig _instance;
        public static UploaderConfig INSTANCE
        {
            get
            { 
                if(_instance == null)
                {
                    _instance = new UploaderConfig();
                }

                return _instance;
            }
        }

        /// <summary>
        /// Default construction which is privately constructed upon access of the INSTANCE member.
        /// Initilisation will set all of the config properties.
        /// </summary>
        private UploaderConfig()
        {
            init();
        }

        /// <summary>
        /// Parses the config file and sets relevant config properties.
        /// </summary>
        private void init()
        {
            string configText = File.ReadAllText(UploaderConstants.CONFIG_PATH);
            JSONNode json = JSON.Parse(configText);

            this.baseDirectory = json[UploaderConstants.BASE_DIRECTORY_PROPERTY];

            if(String.IsNullOrEmpty(this.baseDirectory))
            {
                throw new InvalidDataException("Base directory for ArcDPS logs was not provided.");
            }

            JSONArray encountersNode = json[UploaderConstants.ENCOUNTERS_PROPERTY].AsArray;

            if (encountersNode.Count == 0)
            {
                throw new InvalidDataException("No encounters provided in the configuration file. log collection terminated.");
            }

            encounters = new List<string>();
            foreach (var encounter in encountersNode)
            {
                encounters.Add(encounter.Value);
            }

            this.outputDirectory = json[UploaderConstants.OUTPUT_DIRECTORY];

            if (String.IsNullOrEmpty(this.outputDirectory))
            {
                this.outputDirectory = UploaderConstants.DEFAULT_OUTPUT_DIRECTORY;
                LOG.Warn($"No Output directory provided. Using default output directory  {UploaderConstants.OUTPUT_DIRECTORY}");
            }

            this.isAutoBrowserOpenEnabled = json[UploaderConstants.AUTO_BROWSER_OPEN_PROPERTY].AsBool;
            this.browserAppPath = json[UploaderConstants.BROWSER_APP_PATH_PROPERTY];

            if (String.IsNullOrEmpty(this.browserAppPath))
            {
                this.isAutoBrowserOpenEnabled = false;
                LOG.Warn($"No web-browser application path provided. The auto-browser open feature is disabled.");
            }

            this.isDailyRun = json[UploaderConstants.IS_DAILY_PROPERTY].AsBool;


            logConfigLoaded();

        }

        private void logConfigLoaded()
        {
            LOG.Debug("############################################################");
            LOG.Debug($"Config loaded:");
            LOG.Debug($"ArcDPS logs directory: [{ this.baseDirectory}]");
            LOG.Debug($"Output directory: [{ this.outputDirectory}]");
            LOG.Debug($"Encounters: [{String.Join(",", this.encounters)}]");
            LOG.Debug($"Browser App Apth: [{this.browserAppPath}]");
            LOG.Debug($"Auto-browser open: [{this.isAutoBrowserOpenEnabled}]");
            LOG.Debug($"Daily runs only: [{this.isDailyRun}]");
            LOG.Debug("############################################################");
        }
    }
}
