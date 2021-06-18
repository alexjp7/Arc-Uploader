namespace ArcUploader.Config
{
    class UploaderConstants
    {
        public static readonly string CONFIG_PATH = "config/config.json";
        public static readonly string LOG4NET_CONFIG = "config/log4net.config";

        public static readonly string DEFAULT_OUTPUT_DIRECTORY = "./logs/";

        public static readonly string ENCOUNTERS_PROPERTY = "encounters";
        public static readonly string BASE_DIRECTORY_PROPERTY = "logs_directory";
        public static readonly string PERMA_LINK_PROPERTY = "permalink";
        public static readonly string OUTPUT_DIRECTORY = "output_directory";
        public static readonly string AUTO_BROWSER_OPEN_PROPERTY = "auto_browser_open";
        public static readonly string BROWSER_APP_PATH_PROPERTY = "browser_app_path";
        public static readonly string IS_DAILY_PROPERTY = "same_day_only";

        public static readonly string UPLOAD_ENDPOINT = "https://dps.report/uploadContent";
    }
}
