using System.IO;
using CodeSearcher.Interfaces;

namespace CodeSearcher.BusinessLogic.Exporter
{
    internal class ResultFileExporter : IResultExporter
    {
        public void Export(ISearchResultContainer searchResultContainer, string exportFileName, string searchedWord)
        {
            using (var writer = File.CreateText(exportFileName))
            {
                foreach (var result in searchResultContainer)
                {
                    writer.WriteLine(result.FileName);
                    var lines = File.ReadAllLines(result.FileName);

                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (lines[i].Contains(searchedWord))
                        {
                            writer.Write($"{i + 1};{lines[i]}");
                        }
                    }

                }
            }
        }
    }
}