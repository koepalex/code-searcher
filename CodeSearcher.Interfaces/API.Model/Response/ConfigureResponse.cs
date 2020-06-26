namespace CodeSearcher.Interfaces.API.Model.Response
{
    /// <summary>
    /// Model for configuration Response message
    /// </summary>
    public class ConfigureResponse
    {
        /// <summary>
        /// Path to store/read code-searcher meta information
        /// </summary>
        public string ManagementInformationPath { get; set; }
    }
}
