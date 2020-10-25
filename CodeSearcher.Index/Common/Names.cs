using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeSearcher.BusinessLogic.Common
{
    /// <summary>
    /// Class contain constants for content of lucene entries
    /// </summary>
    internal static class Names
    {
        /// <summary>
        /// Name of Lucene field that contains the full text (will be indexed)
        /// </summary>
        public const String ContentFieldName = "content";
        /// <summary>
        /// Name of Lucene field that contains the full file path
        /// </summary>
        public const String FileNameFieldName = "path";
    }

    /// <summary>
    /// Class contain constants for well know folder names
    /// </summary>
    public static class FolderNames
    {
        /// <summary>
        /// Name of the folder, that is used to store lucene files, when using CodeSearcherManager
        /// </summary>
        public const String DefaultLuceneIndexName = ".code-searcher";

        /// <summary>
        /// Name of the folder (under LocalAppData) which is used by CodeSearcherManager to store index management information
        /// </summary>
        public const String ManagementFolder = "code-searcher";
    }

    /// <summary>
    /// Class contain constants for well known file names
    /// </summary>
    internal static class FileNames
    {
        /// <summary>
        /// Name of the file, that contains structure to describe all known indexes
        /// </summary>
        public const String IndexOverview = "IndexOverview.json";
    }
}
