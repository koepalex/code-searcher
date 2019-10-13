using System;
using System.IO;
using CodeSearcher.Interfaces;

namespace CodeSearcher.BusinessLogic
{
    public interface ICodeSearcherLogic
    {
        void CreateNewIndex(Action startCallback, Action<string> fileProccessedCallback, Action<long, TimeSpan> finishedCallback);
        void SearchWithinExistingIndex(Action startCallback, Func<(string, bool)> getSearchWord, Func<int> getMaximumNumberOfHits, Func<int> getHitsPerPage, Func<(bool, IResultExporter)> getExporter, Func<ISingleResultPrinter> getSingleResultPrinter, Action<TimeSpan> finishedCallback, Action endOfSearchCallback, Action exportFinishedCallback = null);
    }
}