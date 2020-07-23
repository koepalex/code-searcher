using CodeSearcher.BusinessLogic.Common;
using CodeSearcher.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace CodeSearcher.BusinessLogic.Exporter
{
    internal class VirtualExporter : IResultExporter
    {
        public void Dispose()
        {
            //Nothing to do
        }

        /// <inheritdoc />
        public void Export(ISearchResultContainer searchResultContainer, string wildcardPattern)
        {
            var results = new List<IDetailedSearchResult>();

            foreach (var result in searchResultContainer)
            {
                var detailedResult = Factory.GetDetailedSearchResult(result.FileName);
                var findings = new List<IFindingInFile>();

                var lines = File.ReadAllLines(result.FileName);

                var resolvedPattern = Helper.WildcardResolver(wildcardPattern);

                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    if (Regex.IsMatch(line, resolvedPattern))
                    {
                        var match = Regex.Match(line, resolvedPattern);

                        var matchedValue = match.Value.Trim();
                        var position = line.IndexOf(matchedValue);

                        findings.Add(Factory.GetFindingInFile(i, position, matchedValue.Length));
                    }
                }

                detailedResult.Findings = findings.ToArray();
                results.Add(detailedResult);
            }

            DetailedResult = results;
        }

        /// <summary>
        /// Get access to detailed search results
        /// </summary>
        public IEnumerable<IDetailedSearchResult> DetailedResult { get; private set; }
    }
}
