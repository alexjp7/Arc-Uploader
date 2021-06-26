namespace ArcUploader.Config
{
    class UploaderConstants
    {
        public static readonly string CONFIG_PATH = "./config/";
        public static readonly string CONFIG_FILE =  CONFIG_PATH + "config.json";
        public static readonly string DEFAULT_CHROME_PATH = "C:/Program Files/Google/Chrome/Application/chrome.exe";
        public static readonly string LOGS_DIRECTORY = "./logs/";

        public static readonly string DEFAULT_OUTPUT_DIRECTORY = "./results/";

        public static readonly string ENCOUNTERS_PROPERTY = "encounters";
        public static readonly string BASE_DIRECTORY_PROPERTY = "logs_directory";
        public static readonly string PERMA_LINK_PROPERTY = "permalink";
        public static readonly string OUTPUT_DIRECTORY = "output_directory";
        public static readonly string AUTO_BROWSER_OPEN_PROPERTY = "auto_browser_open";
        public static readonly string BROWSER_APP_PATH_PROPERTY = "browser_app_path";
        public static readonly string IS_DAILY_PROPERTY = "same_day_only";
        public static readonly string IS_POLLING_ENABLED = "polling_enabled";

        public static readonly string UPLOAD_ENDPOINT = "https://dps.report/uploadContent";

        public static readonly string LOG4NET_XML = "<log4net> <appender name=\"Console\" type=\"log4net.Appender.ConsoleAppender\"> " +
            "<layout type=\"log4net.Layout.PatternLayout\"> " +
            "<conversionPattern value=\"%message%newline\" /> </layout> " +
            "</appender> " +
            "<appender name=\"RollingFile\" type=\"log4net.Appender.RollingFileAppender\"> " +
            "<file value=\"./logs/arcuploader.log\" /> " +
            "<appendToFile value=\"true\" /> " +
            "<maximumFileSize value=\"100KB\" /> " +
            "<maxSizeRollBackups value=\"2\" /> " +
            "<layout type=\"log4net.Layout.PatternLayout\"> " +
            "<conversionPattern value=\"[%-5p] %date{dd MMM yyyy HH:mm:ss,fff} %c{1}:%L - %m%n\" /> </layout> " +
            "</appender> " +
            "<root> " +
            "<level value=\"DEBUG\" /> " +
            "<appender-ref ref=\"Console\" /> " +
            "<appender-ref ref=\"RollingFile\" /> " +
            "</root> " +
            "</log4net>";
    }
}
