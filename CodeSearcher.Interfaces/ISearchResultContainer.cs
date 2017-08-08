using System.Collections.Generic;

namespace CodeSearcher.Interfaces
{
	/// <summary>
	/// Interface which allows to enumerate all findings for a specific search
	/// </summary>
    public interface ISearchResultContainer : IEnumerable<ISearchResult>
    {
		/// <summary>
		/// Gets the number of findings
		/// </summary>
		/// <value>The number of hits; 0 if nothing was found</value>
        int NumberOfHits { get; }
    }
}
