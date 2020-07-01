using System.Collections.Generic;

namespace CodeSearcher.Interfaces.API.Model.Requests
{
    /// <summary>
    /// Model for creating new index Request
    /// </summary>
    public class CreateIndexRequest
    {
        /// <summary>
        /// Path to read files to index
        /// </summary>
        public string SourcePath { get; set; }

        /// <summary>
        /// Extensions of files that should be indexed
        /// </summary>
        public string[] FileExtensions { get; set; }
    }
}
