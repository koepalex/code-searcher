using System.Collections.Generic;

namespace CodeSearcher
{
    internal interface ICmdLineHandler
    {
        bool WildCardSearch { get; }
        bool ExportToFile { get; }
        string SearchWord { get; }
        int NumberOfHits { get; }
        int HitsPerPage { get; }
        string IndexPath { get; }
        string SourcePath { get; }

        IList<string> GetFileExtensionsAsList();
        ProgramModes GetProgramMode();
        bool Parse(string[] cmdArgs);
    }

    /// <summary>
    /// Defines the possible modes of command line program
    /// </summary>
    internal enum ProgramModes
    {
        /// <summary>
        /// Invalid, default value
        /// </summary>
        None = 0,
        /// <summary>
        /// Create new Index
        /// </summary>
        Index = 1,
        /// <summary>
        /// Search in Existing Index
        /// </summary>
        Search = 2,
        /// <summary>
        /// CLI Menu
        /// </summary>
        Auto = 3,
    }
}