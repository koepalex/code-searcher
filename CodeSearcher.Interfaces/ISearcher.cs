using System;

namespace CodeSearcher.Interfaces
{
	/// <summary>
	/// Interface for engine to find a pattern within indexed files
	/// </summary>
    public interface ISearcher : IDisposable
    {
		/// <summary>
		/// Perform the search for a given pattern/word
		/// </summary>
		/// <param name="pattern">Pattern or word to look for</param>
		/// <param name="maximumNumberOfHits">Maximum amount of findings to return</param>
		/// <param name="action">Method which should be invoked with the findings</param>
        void SearchFileContent(String pattern, int maximumNumberOfHits, Action<ISearchResultContainer> action);
    }
}
