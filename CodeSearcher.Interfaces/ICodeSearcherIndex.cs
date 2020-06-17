using System;
using System.Collections.Generic;

namespace CodeSearcher.Interfaces
{
    /// <summary>
    /// Interface describing one path and fileextension to Index
    /// </summary>
    public interface ICodeSearcherIndex : IEquatable<ICodeSearcherIndex>, IEqualityComparer<ICodeSearcherIndex>
    {
        /// <summary>
        /// Unique identifier of this index
        /// </summary>
        int ID { get; set; }
        
        /// <summary>
        /// Path that should be indexed
        /// </summary>
        string SourcePath { get; set; }
        
        /// <summary>
        /// Path where the index files are stored
        /// </summary>
        string IndexPath { get; set; }

        /// <summary>
        /// time when the index was last created or updated
        /// </summary>
        DateTime LastUpdate { get; set; }

        /// <summary>
        /// Extensions of files that need to be used while indexing
        /// </summary>
        IList<string> FileExtensions { get; set; }
    }
}