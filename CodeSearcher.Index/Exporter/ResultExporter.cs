using System.IO;
using CodeSearcher.Interfaces;

namespace CodeSearcher.BusinessLogic.Exporter
{
    internal sealed class ResultFileExporter : IResultExporter
    {
        private readonly StreamWriter m_ExportWriter;

        public ResultFileExporter(StreamWriter exportWriter)
        {
            m_ExportWriter = exportWriter;
        }

        public void Export(ISearchResultContainer searchResultContainer, string searchedWord)
        {
            foreach (var result in searchResultContainer)
            {
                m_ExportWriter.WriteLine(result.FileName);
                var lines = File.ReadAllLines(result.FileName);
                var searchWordLower = searchedWord.ToLowerInvariant();
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].ToLowerInvariant().Contains(searchWordLower))
                    {
                        m_ExportWriter.WriteLine($"{i + 1};{lines[i]}");
                    }
                }
                m_ExportWriter.Flush();
            }
        }

        public void Dispose()
        {
            m_ExportWriter?.Close();
        }
    }
}