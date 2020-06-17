using System.Collections.Generic;

namespace CodeSearcher
{
    internal interface ICmdLineHandler
    {
        string this[string name] { get; }

        string ExportToFile { get; }
        string FileExtensions { get; }
        string HitsPerPage { get; }
        string IndexPath { get; }
        string NumberOfHits { get; }
        string ProgramMode { get; }
        string SearchedWord { get; }
        string SourcePath { get; }
        string WildcardSearch { get; }

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