namespace CodeSearcher.Interfaces.API.Model.Response
{
    /// <summary>
    /// Model for reading all existing indexesResponse message
    /// </summary>
    public class GetIndexesResponse
    {
        /// <summary>
        /// All existing Indexes
        /// </summary>
        public ICodeSearcherIndex[] Indexes { get; set; }
    }
}
