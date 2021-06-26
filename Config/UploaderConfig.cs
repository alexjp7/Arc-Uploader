namespace ArcUploader.Config
{
    using log4net;
    using SimpleJSON;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Runntime configuration for the collection job.  The config
    /// for this application can be found in the config.json file.
    /// </summary>
    class UploaderConfig
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(UploaderConfig));

        /// <summary>
        /// List of valid inputs for boolean properties. Used in input validation.
        /// </summary>
        private static readonly string[] VALID_BOOLEAN_INPUT = { "y", "n", "yes", "no" };

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

        /// <summary>
        /// A flag determine whether dynamic file uploading is enabled.
        /// </summary>
        public bool isPollingEnabled { get; private set; }

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
            encounters = new List<string>();
            init();
        }

        /// <summary>
        /// Parses the config file and sets relevant config properties.
        /// </summary>
        private void init()
        {

            bool hasExistingConfig = true;
            string configText = null;

            try
            {
                configText = File.ReadAllText(UploaderConstants.CONFIG_FILE);
            }
            catch(IOException e)
            {
                hasExistingConfig = false;
            }


            if (hasExistingConfig)
            {
                readConfig(configText);
            }
            else
            {
                createDefaultConfig();
            }

            logConfigLoaded();

        }

        /// <summary>
        /// Saves the current state of the application config to disk.
        /// </summary>
        private void saveConfig()
        {
            JSONNode json = new JSONObject();
            json[UploaderConstants.BASE_DIRECTORY_PROPERTY] = this.baseDirectory;
            json[UploaderConstants.OUTPUT_DIRECTORY] = this.outputDirectory;
            json[UploaderConstants.AUTO_BROWSER_OPEN_PROPERTY] = this.isAutoBrowserOpenEnabled;
            json[UploaderConstants.BROWSER_APP_PATH_PROPERTY] = this.browserAppPath;
            json[UploaderConstants.IS_DAILY_PROPERTY] = this.isDailyRun;
            json[UploaderConstants.IS_POLLING_ENABLED] = this.isPollingEnabled;

            JSONArray encountersNode = new JSONArray();
            foreach (var encounter in encounters)
            {
                encountersNode.Add(encounter);
            }
            json[UploaderConstants.ENCOUNTERS_PROPERTY] = encountersNode;

            Directory.CreateDirectory(UploaderConstants.CONFIG_PATH);
            StreamWriter configFile = new StreamWriter(UploaderConstants.CONFIG_FILE, true);
            string formattedConfig = JToken.Parse(json.ToString()).ToString(Formatting.Indented);
            configFile.WriteLine(formattedConfig);

            configFile.Close();
        }

        /// <summary>
        /// Prompts the user to enter config values for all application properties.
        /// </summary>
        private void createDefaultConfig()
        {
            Console.WriteLine("No existing ArcAgent configuration found. ArcAgent configuration will now begin.");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("#################CONFIGURATION SETUP##################");
            Console.WriteLine("Where are your ArcDPS logs found?  \nFor example - %USERPROFILE%/Documents/Guild Wars 2/addons/arcdps/arcdps.cbtlogs");
            

            string logsDirectory = getStringValue("Arc Dps Logs Directory");

            Console.WriteLine("\nArcAgent creates an output file that contains the URLs of your logs, where would you like this to be stored?");
            string outputDirectory = getStringValue("Output directory");

            Console.WriteLine("\nArcAgent has the option to allow you to open any uploaded logs straight into your web-browser." +
                              "\n Do you want to enable this?");
            bool autoBrowserOpen = getBooleanValue("Automatic browser launch");

            Console.WriteLine("\nIn order to open a web-browser after upload, you need to specify where your web-browser is installed.\n" +
                              "Would you like to use the default installation for the Google Chrome browser? [C:/Program Files/Google/Chrome/Application/chrome.exe]");
          
            bool isUsingDefaultAppPath = getBooleanValue("Use default chome path");
            string userAppPath = null;
            if(!isUsingDefaultAppPath)
            {
                Console.WriteLine("Please enter the application path for your desired web-browser");
                userAppPath = getStringValue("Browser app path");
            }


            Console.WriteLine("\nArcAgent can consider if you only want to upload logs from the same day. \n" +
                                "If this is disabled, then the latest log will be uploaded irrespective if you ran it today." +
                                "\n Do you want to enable this");
            
            bool isSameDay = getBooleanValue("Upload same day logs only");

            Console.WriteLine("\nArcAgent can be configured to run a single batch upload, or to use a polling mode.\n" +
                                " Polling mode will mean that ArcAgent will automatically detect when new runs have been completed. " +
                                "\nDo you want to enable this?");
            bool isPollingMode = getBooleanValue("Polling mode enabled");

            Console.WriteLine("\nArcAgent can be configured to only upload certain encounters. Please provide a list of encounters you wish to allow uploading for.\n" +
                            "For example" +
                            "\n Arkk,Artsariiv,MAMA" +
                            "\nNote - This only applies for when 'polling mode' is disabled");

            string encounterString = getStringValue("List of Encounters, each split by a comma (,)");

            string[] encountersRaw = encounterString.Split(",");
            foreach (string encounter in encountersRaw)
            {
                this.encounters.Add(encounter.Trim());
            }

            this.isDailyRun = isSameDay;
            this.isAutoBrowserOpenEnabled = autoBrowserOpen;
            this.outputDirectory = outputDirectory?.Trim();
            this.baseDirectory = logsDirectory?.Trim();
            this.browserAppPath = isUsingDefaultAppPath ? UploaderConstants.DEFAULT_CHROME_PATH : userAppPath?.Trim();

            Console.WriteLine("###################################");
            Console.ForegroundColor = ConsoleColor.White;
            saveConfig();
        }



        /// <summary>
        /// Prompts the user for a string/text value.
        /// </summary>
        /// <param name="propertyName">The name of the property to display in terminal</param>
        /// <returns>A non-empty text value.</returns>
        private string getStringValue(string propertyName)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            string userInput = null;
            do
            {
                Console.Write($"> {propertyName} = ");
                userInput = Console.ReadLine();
            }
            while (String.IsNullOrEmpty(userInput));

            Console.ForegroundColor = ConsoleColor.Yellow;

            return userInput;
        }

        /// <summary>
        /// Prompts the user for a boolean value (yes/no).
        /// </summary>
        /// <param name="propertyName">The name of the property to display in terminal</param>
        /// <returns>A boolean value that represents the users yes/no input.</returns>
        private bool getBooleanValue(string propertyName)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            string userInput = null;
            do
            {
                Console.Write($"> {propertyName} (yes / no) = ");
                userInput = Console.ReadLine();
                userInput = userInput?.ToLower();
            }
            while (!isValidBooleanAnswer(userInput));
            Console.ForegroundColor = ConsoleColor.Yellow;

            return userInput.Equals("y") || userInput.Equals("yes");
        }

        /// <summary>
        /// Utility method to determine if a given input is a valid user input for boolean configuration properties..
        /// </summary>
        /// <param name="input">A value provided by user,</param>
        /// <returns>True - If the input exists in <see cref="VALID_BOOLEAN_INPUT"> valid inputs.</see></returns>
        private bool isValidBooleanAnswer(string input)
        {
            return VALID_BOOLEAN_INPUT.Contains(input);
        }

        /// <summary>
        /// Reads the config.json file from file system, and assigns application propeties.
        /// </summary>
        /// <param name="configText"></param>
        private void readConfig(string configText)
        {

            JSONNode json = JSON.Parse(configText);

            this.baseDirectory = json[UploaderConstants.BASE_DIRECTORY_PROPERTY];

            if (String.IsNullOrEmpty(this.baseDirectory))
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
            this.isPollingEnabled = json[UploaderConstants.IS_POLLING_ENABLED].AsBool;
        }

        /// <summary>
        /// Prints out config to console/log file.
        /// </summary>
        private void logConfigLoaded()
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            LOG.Debug("############################################################");
            LOG.Debug($"Config loaded:");
            LOG.Debug($"ArcDPS logs directory: [{ this.baseDirectory}]");
            LOG.Debug($"Output directory: [{ this.outputDirectory}]");
            LOG.Debug($"Encounters: [{String.Join(",", this.encounters)}]");
            LOG.Debug($"Browser App Apth: [{this.browserAppPath}]");
            LOG.Debug($"Auto-browser open: [{this.isAutoBrowserOpenEnabled}]");
            LOG.Debug($"Daily runs only: [{this.isDailyRun}]");
            LOG.Debug($"Polling mode enabled: [{this.isPollingEnabled}]");
            LOG.Debug("############################################################");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
