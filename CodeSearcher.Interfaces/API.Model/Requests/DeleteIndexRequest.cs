namespace CodeSearcher.Interfaces.API.Model.Requests
{
    /// <summary>
    /// Model for deleting an existing index Request
    /// </summary>
    public class DeleteIndexRequest
    {
        /// <summary>
        /// Unique identifier of existing index that should be deleted
        /// </summary>
        public int IndexID { get; set; }
    }
}
