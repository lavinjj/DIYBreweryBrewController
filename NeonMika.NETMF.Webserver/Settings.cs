namespace NeonMika.Webserver
{
    /// <summary>
    /// Static settings for the webserver
    /// </summary>
    internal static class Settings
    {
        /// <summary>
        /// Buffersize for response file sending
        /// </summary>
        public const int FILE_BUFFERSIZE = 1024;

        /// <summary>
        /// Maximum byte size for a HTTP request sent to the server
        /// </summary>
        public const int MAX_REQUESTSIZE = 1024;

        /// <summary>
        /// "" for complete access, otherwise @"[drive]:\[folder]\[folder]\[...]\"
        /// Watch the '\' at the end of the path
        /// </summary>
        public const string ROOT_PATH = @"\SD\";
    }
}