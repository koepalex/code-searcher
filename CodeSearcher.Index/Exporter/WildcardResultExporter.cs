using CodeSearcher.BusinessLogic.Common;
using CodeSearcher.Interfaces;
using System.IO;
using System.Text.RegularExpressions;

namespace CodeSearcher.BusinessLogic.Exporter
{
    public class WildcardResultExporter : IResultExporter
    {
        private readonly StreamWriter m_ExportWriter;

        public WildcardResultExporter(StreamWriter exportWriter)
        {
            m_ExportWriter = exportWriter;
        }

        public void Export(ISearchResultContainer searchResultContainer, string wildcardPattern)
        {
            foreach (var result in searchResultContainer)
            {
                m_ExportWriter.WriteLine(result.FileName);
                var lines = File.ReadAllLines(result.FileName);

                var resolvedPattern = Helper.WildcardResolver(wildcardPattern);

                for (int i = 0; i < lines.Length; i++)
                {
                    if (Regex.IsMatch(lines[i], resolvedPattern))
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
