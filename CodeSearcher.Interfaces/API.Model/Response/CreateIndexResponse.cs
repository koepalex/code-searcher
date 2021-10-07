namespace CodeSearcher.Interfaces.API.Model.Response
{
    /// <summary>
    /// Model for creating new index Response message
    /// </summary>
    public class CreateIndexResponse
    {
        /// <summary>
        /// IndexingJobId can be used to cancel indexing job or get status updates
        /// </summary>
        public string IndexingJobId { get; set; }
    }
}
