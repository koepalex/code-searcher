using System;
using CodeSearcher.Interfaces;

namespace CodeSearcher.BusinessLogic
{
    /// <summary>
    /// Main interface to use code-seacher functionality
    /// </summary>
    public interface ICodeSearcherLogic
    {
        /// <summary>
        /// Looking in a folder (SourcePath) for well know files (FileExtensions), those files are
        /// tokenized and analyzed by Lucene engine and Lucene intermediate files are stored in a folder
        /// (IndexPath).
        /// </summary>
        /// <remarks>
        /// SourcePath, IndexPath and FileExtensions are mandatory parameter to create CodeSearcherLogic
        /// <see cref="CodeSearcher.BusinessLogic.Factory"/>
        /// </remarks>
        /// <param name="startCallback">Callback which is called when creating index is about to start</param>
        /// <param name="fileProccessedCallback">Callback which is called for each file that was analyzed</param>
        /// <param name="finishedCallback">
        /// Callback which is called when creating index has finished
        /// with parameter numberOfFiles(long) and time needed(TimeSpan)
        /// </param>
        void CreateNewIndex(Action startCallback, Action<string> fileProccessedCallback, Action<long, TimeSpan> finishedCallback);

        /// <summary>
        /// Look into existing index, for a given word
        /// </summary>
        /// <param name="startCallback">Callback which is called when lookup is about to start</param>
        /// <param name="getSearchWord">Callback that have to provide a tuple of the word to lookup (string - word) and bool indicating if this was the last word to lookup (bool - EndOfSearch)</param>
        /// <param name="getMaximumNumberOfHits">Defines the absolut number of findings</param>
        /// <param name="getHitsPerPage">Defines the amount of findings of word to lookup which are shown at once, will be passed into <see cref="ISingleResultPrinter.NumbersToShow"/></param>
        /// <param name="getExporter">Callback to inject <see cref="IResultExporter"/> instance, when tuple shouldExport (bool) control that export is required </param>
        /// <param name="getSingleResultPrinter">Callback to inject <see cref="ISingleResultPrinter"/> instance to be used</param>
        /// <param name="finishedCallback">Callback which is called when a single lookup is finished</param>
        /// <param name="endOfSearchCallback">Callback which is called when lookup is completly finished</param>
        /// <param name="exportFinishedCallback">[Optional] Callback which is called when exporter is completed</param>
        /// <param name="wildcardSearch">[Optional] Control if default searcher (exact match) or searcher supporting wildcard (?, *) have to be used</param>
        void SearchWithinExistingIndex(Action startCallback, Func<(string, bool)> getSearchWord, Func<int> getMaximumNumberOfHits, Func<int> getHitsPerPage, Func<(bool, IResultExporter)> getExporter, Func<ISingleResultPrinter> getSingleResultPrinter, Action<TimeSpan> finishedCallback, Action endOfSearchCallback, Action exportFinishedCallback = null, bool wildcardSearch = false);
    }
}