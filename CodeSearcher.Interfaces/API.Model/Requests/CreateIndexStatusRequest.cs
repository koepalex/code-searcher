namespace CodeSearcher.Interfaces.API.Model.Requests
{
    /// <summary>
    /// Model for request to read indexing status
    /// </summary>
    public class CreateIndexStatusRequest
    {
        /// <summary>
        /// identifier of indexing job returned by <see cref="Response.CreateIndexResponse"/>
        /// </summary>
        public string JobId { get; set; }
    }
}
