namespace CodeSearcher.Interfaces.API.Model.Response
{
    /// <summary>
    /// Model for creating index status response
    /// </summary>
    public class CreateIndexStatusResponse
    {
        /// <summary>
        /// Indicates if indexing request is still running
        /// </summary>
        public bool IndexingFinished { get; set; }
        /// <summary>
        /// Identifier of created index, only valid if <see cref="IndexingFinished" /> returns true 
        /// </summary>
        public int IndexId { get; set; }
        /// <summary>
        /// Indicates if status for job id exists
        /// </summary>
        public bool Exists { get; set; }
        
    }
}
