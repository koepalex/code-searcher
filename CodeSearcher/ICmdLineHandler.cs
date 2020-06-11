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
        int GetProgramModeAsInt();
        bool Parse(string[] cmdArgs);
    }
}