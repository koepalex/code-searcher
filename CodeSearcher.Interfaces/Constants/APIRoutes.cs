namespace CodeSearcher.Interfaces.Constants
{
    /// <summary>
    /// Class containts constant definitions for Web API routes
    /// </summary>
    public class APIRoutes
    {
        /// <summary>
        /// Route to access the Open API (Swagger) Dashboard
        /// </summary>
        public const string OpenApiUiRoute = "/index.html";

        /// <summary>
        /// Route to access the Open API (Swagger) json definition
        /// </summary>
        public const string OpenApiDefinitionRoute = "/swagger/v1/swagger.json";

        /// <summary>
        /// Base route to access CodeSearcher APIs
        /// </summary>
        public const string CodeSearcherRoute = "/api/CodeSearcher";

        /// <summary>
        /// Route to read and update Code Searcher configuration
        /// </summary>
        public const string ConfigurationRoute = "/api/CodeSearcher/configure";

        /// <summary>
        /// Route to index a new folder
        /// </summary>
        public const string CreateIndexRoute = "/api/CodeSearcher/index";

        /// <summary>
        /// Route to search within existing index
        /// </summary>
        public const string SearchInIndexRoute = "/api/CodeSearcher/search";
    }
}
