using System.IO;
using CodeSearcher.Interfaces;

namespace CodeSearcher.BusinessLogic.Exporter
{
    internal class ResultFileExporter : IResultExporter
    {
        public void Export(ISearchResultContainer searchResultContainer, string searchedWord, StreamWriter exportWriter)
        {
            foreach (var result in searchResultContainer)
            {
                exportWriter.WriteLine(result.FileName);
                var lines = File.ReadAllLines(result.FileName);

                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains(searchedWord))
                    {
                        exportWriter.Write($"{i + 1};{lines[i]}");
                    }
                }
            }
        }
    }
}