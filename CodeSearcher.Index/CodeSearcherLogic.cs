using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.IO;
using CodeSearcher.Interfaces;

namespace CodeSearcher.BusinessLogic
{
    public class CodeSearcherLogic
    {
        private readonly ICmdLineHandler m_CmdHandler;
        private readonly ICodeSearcherLogger m_Logger;
        private static long m_FileCounter;
        private readonly Func<string> m_GetIndexPath;
        private readonly Func<string> m_GetSourcePath;
        private readonly Func<IList<string>> m_GetFileExtension;

        public CodeSearcherLogic(
            ICmdLineHandler cmdHandler,
            ICodeSearcherLogger logger,
            Func<string> getIndexPath,
            Func<string> getSourcePath,
            Func<IList<string>> getFileExtension)
        {
            m_CmdHandler = cmdHandler;
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
            startCallback();

            var idxPath = m_GetIndexPath();
            var srcPath = m_GetSourcePath();
            var fileExtensions = m_GetFileExtension();

            using (var indexer = Factory.GetIndexer(idxPath, srcPath, fileExtensions))
            {
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
            ISingleResultPrinter printer,
            Action<TimeSpan> finishedCallback,
            Action endOfSearchCallback,
            Action<string> exportFinishedCallback = null)
        {
            startCallback();
            
            var idxPath = m_GetIndexPath();

            using(var searcher = Factory.GetSearcher(idxPath))
            {
                do
                {
                    (string searchedWord, bool shouldExit) = getSearchWord();
                    if (string.IsNullOrWhiteSpace(searchedWord) && shouldExit) break;

					int numberOfHits;

					if (!int.TryParse(m_CmdHandler[m_CmdHandler.NumberOfHits], out numberOfHits))
					{
						m_Logger.Info("Maximum hits to show will be 1000");
						numberOfHits = 1000;
					}

					int hitsPerPage;
					if (!int.TryParse(m_CmdHandler[m_CmdHandler.HitsPerPage], out hitsPerPage))
					{
                        m_Logger.Info("Maximum hits per page will be shown");
                        hitsPerPage = -1;
                    }

                    bool export;
                    if (!bool.TryParse(m_CmdHandler[m_CmdHandler.ExportToFile], out export))
                    {
                        m_Logger.Info("Results will not be exported");
                        export = false;
                    }

                    var timeSpan = RunActionWithTimings("Search For " + searchedWord, () =>
                    {
                        searcher.SearchFileContent(searchedWord, numberOfHits, (searchResultContainer) =>
                        {
                            m_Logger.Info("Found {0} hits", searchResultContainer.NumberOfHits);
                            foreach(var result in searchResultContainer)
                            {
                                printer.NumbersToShow = hitsPerPage == -1
									? int.MaxValue
									: hitsPerPage;

                                printer.Print(result.FileName, searchedWord);
                            }

                            if (export)
                            {
                                var exportFile = Path.GetTempFileName();
                                var exporter = Factory.GetResultExporter();
                                exporter.Export(searchResultContainer, exportFile, searchedWord);

                                if (exportFinishedCallback != null) exportFinishedCallback(exportFile);
                            }
                        });
                    });

                    finishedCallback(timeSpan);

                    if (shouldExit) break;

                    endOfSearchCallback();

                } while(true);
            }
        }
    }
}
