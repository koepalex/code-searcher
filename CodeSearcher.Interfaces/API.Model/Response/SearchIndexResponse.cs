namespace CodeSearcher.Interfaces.API.Model.Response
{
    /// <summary>
    /// Model for searching in existing index response
    /// </summary>
    public class SearchIndexResponse
    {
        /// <summary>
        /// Query results
        /// </summary>
        public IDetailedSearchResult[] Results { get; set; }
    }
}
