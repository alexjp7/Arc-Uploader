namespace ArcUploader
{
    using ArcUploader.Components;

    /// <summary>
    /// Application entry point. Initializes and begins the collection job.
    /// </summary>
    class Application
    {
        static void Main(string[] args)
        {
            new ArcJob().start();
        }
    }
}
