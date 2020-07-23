using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CodeSearcher.Interfaces;
using CodeSearcher.BusinessLogic.Io;
using CodeSearcher.BusinessLogic.Common;
using CodeSearcher.BusinessLogic.InternalInterfaces;

namespace CodeSearcher.BusinessLogic.Indexer
{
    internal sealed class DefaultIndexer : IIndexer
    {
        private String m_SourcePath;
        private String m_IndexPath;
        private IFileReader m_FileReader;

        private static Directory m_LuceneDirectoy;
        private static Analyzer m_LuceneAnalyzer;
        private static IndexWriter m_LuceneIndexWriter;

		private HashSet<string> m_KnowFiles;

        public event IndexerProcessFileDelegate IndexerProcessFile;

        public DefaultIndexer(String idxPath, String srcPath, IList<String> fileExtensions)
        {
            if (String.IsNullOrWhiteSpace(idxPath)) throw new ArgumentNullException("idxPath");
            if (String.IsNullOrWhiteSpace(srcPath)) throw new ArgumentNullException("srcPath");
            if (fileExtensions == null || fileExtensions.Count == 0) throw new ArgumentNullException("fileExtensions");

            m_IndexPath = idxPath;
            m_SourcePath = srcPath;

            m_FileReader = Factory.GetFileReader(fileExtensions);

            m_LuceneDirectoy = Factory.GetLuceneDirectory(m_IndexPath);
            m_LuceneAnalyzer =  Factory.GetLuceneAnalyzer();
            m_LuceneIndexWriter = Factory.GetLuceneIndexWriter(m_LuceneDirectoy, m_LuceneAnalyzer);

			m_KnowFiles = new HashSet<string>();
        }

        public Task CreateIndex()
        {
            return Task.Run(() =>
            {

               //long counter = 0;

               var task = m_FileReader.ReadFilesAsync(m_SourcePath, (fileStructures) =>
               {
                   foreach (var fileStructure in fileStructures.Where(f => !f.ErrorOccurred))
                   {
                       var doc = Factory.GetLuceneDocument();

					   var fileName = fileStructure.FilePath;
					   if (!m_KnowFiles.Contains(fileName))
					   {
						   m_KnowFiles.Add(fileName);
						   var fileContent = fileStructure.Text;

						   doc.Add(
							   //files should be saved case variant to support unix filesystems
							   Factory.GetLuceneStoredAndIndexedField(
								   Names.FileNameFieldName,
								   fileName));

						   doc.Add(
							   Factory.GetLuceneNotStoredButIndexedField(
								   Names.ContentFieldName,
								   fileContent.ToLowerInvariant()));

						   m_LuceneIndexWriter.AddDocument(doc);

						   FireEventProcessFile(fileName);
					   }

                       //Interlocked.Increment(ref counter);
                   }


                   //if ((counter % 1000) == 0)
                   //{
                   //    m_LuceneIndexWriter.Optimize(false);
                   //}
               });

               task.Wait();

                m_LuceneIndexWriter.Optimize(true);

				m_KnowFiles.Clear();
            });
        }

		public Task UpdateIndex()
		{
			return Task.Run(() => 
			{
				//TODO Implement Update Index
			});
		}

        private void FireEventProcessFile(String fileName)
        {
            if (IndexerProcessFile != null)
            {
                IndexerProcessFile.Invoke(this, new IndexerProcessFileEventArgs { FileName = fileName });
            }
        }

        public void Dispose()
        {
            //without this call, no elements will found within the index
            m_LuceneIndexWriter.Optimize();
            m_LuceneIndexWriter.Flush(true, true, true);
            m_LuceneIndexWriter.Dispose();
            m_LuceneAnalyzer.Dispose();
            m_LuceneDirectoy.Dispose();
        }
    }
}
