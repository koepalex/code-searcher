using System;
using System.Collections.Generic;

namespace CodeSearcher.Interfaces
{
    /// <summary>
    /// Provides a higher level management interface to code-searcher, to handle indexes
    /// </summary>
    public interface ICodeSearcherManager
    {
        /// <summary>
        /// Method creats a new code-searcher index
        /// </summary>
        /// <param name="sourcePath">Path containing files that should be part of indexing</param>
        /// <param name="fileExtensions">Extension of file types that should be indexed</param>
        /// <returns>ID of index that was created</returns>
        /// <exception cref="NotSupportedException">If Index for same path and filetype already exist</exception>
        int CreateIndex(string sourcePath, IEnumerable<string> fileExtensions);

        /// <summary>
        /// Method deletes existing code-searcher index (including indexing files)
        /// </summary>
        /// <param name="indexId">Unique identifier of code-searcher index that should be deleted</param>
        /// <exception cref="NotSupportedException">If Index not exist</exception>
        void DeleteIndex(int indexId);
        
        /// <summary>
        /// Method returns enumeration of all known indexes
        /// </summary>
        /// <returns>Enumeration of code-searcher indexes or empty enumeration if no index exist</returns>
        IEnumerable<ICodeSearcherIndex> GetAllIndexes();

        /// <summary>
        /// Method returns index by given unique identifier
        /// </summary>
        /// <param name="indexId">Unique identifier of code-searcher index</param>
        /// <returns>Instance of code-searcher index or null if index doesn't exist</returns>
        ICodeSearcherIndex GetIndexById(int indexId);

        /// <summary>
        /// Defines the path where the Manager is storing/reading the meta information (Default: %APPDATA%\code-searcher)
        /// </summary>
        string ManagementInformationPath { get; set; }
    }
}
