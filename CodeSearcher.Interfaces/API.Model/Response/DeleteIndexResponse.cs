namespace CodeSearcher.Interfaces.API.Model.Response
{
    /// <summary>
    /// Model for deleting existing index Response message
    /// </summary>
    public class DeleteIndexResponse
    {
        /// <summary>
        /// Indicates if index could be deleted
        /// </summary>
        public bool Succeeded { get; set; }
    }
}
