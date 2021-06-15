namespace ArcUploader.Config
{
    using SimpleJSON;
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Runntime configuration for the collection job.  The config
    /// for this application can be found in the config.json file.
    /// </summary>
    class UploaderConfig
    {
        /// <summary>
        /// An inclusive list of encounters which will be used to determine which boss encounters to collect.
        /// </summary>
        public List<string> encounters { get; private set; }

        /// <summary>
        /// The base ArcDPS log directory, where the boss encounter sub-directories can be found.
        /// </summary>
        public string baseDirectory { get; private set; }

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
        }
    }
}
