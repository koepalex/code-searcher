namespace CodeSearcher.Interfaces.API.Model.Requests
{
    /// <summary>
    /// Model for searching within existing index Request
    /// </summary>
    public class SearchIndexRequest
    {
        /// <summary>
        /// Unique identifier of index to search within 
        /// </summary>
        public int IndexID { get; set; }
        
        /// <summary>
        /// Word to look up
        /// </summary>
        public string SearchWord { get; set; }
    }
}
