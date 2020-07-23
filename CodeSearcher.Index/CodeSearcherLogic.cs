using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using CodeSearcher.Interfaces;

namespace CodeSearcher.BusinessLogic
{
    internal class CodeSearcherLogic : ICodeSearcherLogic
    {
        private readonly ICodeSearcherLogger m_Logger;
        private static long m_FileCounter;
        private readonly Func<string> m_GetIndexPath;
        private readonly Func<string> m_GetSourcePath;
        private readonly Func<IList<string>> m_GetFileExtension;

        public CodeSearcherLogic(
            ICodeSearcherLogger logger,
            Func<string> getIndexPath,
            Func<string> getSourcePath,
            Func<IList<string>> getFileExtension)
        {
            m_Logger = logger;
            m_GetIndexPath = getIndexPath;
            m_GetSourcePath = getSourcePath;
            m_GetFileExtension = getFileExtension;
        }

        public void CreateNewIndex(
            Action startCallback,
            Action<string> fileProccessedCallback,
            Action<long, TimeSpan> finishedCallback)
        {
            Interlocked.Exchange(ref m_FileCounter, 0);
            startCallback();

            var idxPath = m_GetIndexPath();
            var srcPath = m_GetSourcePath();
            var fileExtensions = m_GetFileExtension();

            using var indexer = Factory.GetIndexer(idxPath, srcPath, fileExtensions);
            indexer.IndexerProcessFile += (sender, args) =>
            {
                Interlocked.Increment(ref m_FileCounter);
                fileProccessedCallback(args.FileName);
            };

            var timeSpan = RunActionWithTimings("Create New Index", () =>
            {
                var task = indexer.CreateIndex();
                task.Wait();
            });

            finishedCallback(m_FileCounter, timeSpan);
        }

        private TimeSpan RunActionWithTimings(String name, Action action)
        {
            var sw = new Stopwatch();
            m_Logger.Debug("> start running action: {0}", name);

            sw.Start();
            action();
            sw.Stop();

            m_Logger.Debug(string.Empty);
            m_Logger.Debug("> action has finised and took:");
            m_Logger.Debug(">> Complete {0} ticks", sw.ElapsedTicks);

            var timespan = TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds);
            m_Logger.Debug(">> {0:00}:{1:00}:{2:00}.{3:000}",
                timespan.Hours,
                timespan.Minutes,
                timespan.Seconds,
                timespan.Milliseconds);

            m_Logger.Debug(string.Empty);

            return timespan;
        }

        public void SearchWithinExistingIndex(
            Action startCallback,
            Func<(string, bool)> getSearchWord,
            Func<int> getMaximumNumberOfHits,
            Func<int> getHitsPerPage,
            Func<(bool, IResultExporter)> getExporter,
            Func<ISingleResultPrinter> getSingleResultPrinter,
            Action<TimeSpan> finishedCallback,
            Action endOfSearchCallback,
            Action exportFinishedCallback = null,
            bool useWildcardSearch = false)
        {
            startCallback();

            var idxPath = m_GetIndexPath();

#pragma warning disable 618 //Pragma can be removed when ISearcher is moved into BusinessLogic and Factory.GetSearcher is internal
            using var searcher = useWildcardSearch ? Factory.GetWildcardSearcher(idxPath) : Factory.Get().GetSearcher(idxPath);
#pragma warning restore 618
            int numberOfHits = getMaximumNumberOfHits();
            int hitsPerPage = getHitsPerPage();
            (bool export, IResultExporter resultExporter) = getExporter();
            var printer = getSingleResultPrinter();

            do
            {
                (string searchedWord, bool shouldExit) = getSearchWord();
                if (string.IsNullOrWhiteSpace(searchedWord) && shouldExit) break;

                var timeSpan = RunActionWithTimings("Search For " + searchedWord, () =>
                {
                    searcher.SearchFileContent(searchedWord, numberOfHits, (searchResultContainer) =>
                    {
                        m_Logger.Info("Found {0} hits", searchResultContainer.NumberOfHits);
                        foreach (var result in searchResultContainer)
                        {
                            if (printer != null)
                            {
                                printer.NumbersToShow = hitsPerPage == -1
                                    ? int.MaxValue
                                    : hitsPerPage;

                                printer.Print(result.FileName, searchedWord);
                            }
                        }

                        if (export)
                        {
                            resultExporter.Export(searchResultContainer, searchedWord);

                            exportFinishedCallback?.Invoke();
                        }
                    });
                });

                finishedCallback(timeSpan);

                if (shouldExit) break;

            } while (true);

            endOfSearchCallback();
        }
    }
}
