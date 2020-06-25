namespace CodeSearcher.Interfaces.API.Model.Requests
{
    /// <summary>
    /// Model for configuration Request
    /// </summary>
    public class ConfigureRequest
    {
        /// <summary>
        /// Path to store/read code-searcher meta information
        /// </summary>
        public string ManagementInformationPath { get; set; }
    }
}
