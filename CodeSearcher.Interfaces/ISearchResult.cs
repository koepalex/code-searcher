using System;

namespace CodeSearcher.Interfaces
{
	/// <summary>
	/// Interface which represent one finding
	/// </summary>
    public interface ISearchResult
    {
		/// <summary>
		/// Filename (Relative path) of the file within SourcePath
		/// </summary>
		/// <value>The name of the file.</value>
        String FileName { get; }
		/// <summary>
		/// Represent the Lucene.NET score which indicates the quality of the result
		/// </summary>
		/// <value>The search score.</value>
        float SearchScore { get; }
    }
}
