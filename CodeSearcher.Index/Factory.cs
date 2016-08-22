using CodeSearcher.BusinessLogic.InternalInterfaces;
using CodeSearcher.BusinessLogic.Ninject;
using CodeSearcher.Interfaces;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Ninject;
using Ninject.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CodeSearcher.BusinessLogic
{
    public static class Factory
    {
        private static IKernel m_Kernel;
        
//        [MethodImpl(MethodImplOptions.Synchronized)]
        internal static IKernel Ioc
        {
            get
            {
                if (m_Kernel == null)
                {
                    m_Kernel = new StandardKernel(new CodeSearcherNinjectModule());
                }

                return m_Kernel;
            }
        }

        public static IIndexer GetIndexer(String pathToStoreTheIndexFiles, String sourceCodePath, IList<String> fileExtensionsToLookFor)
        {
            if (String.IsNullOrWhiteSpace(pathToStoreTheIndexFiles)) throw new ArgumentNullException("pathToStoreTheIndexFiles");
            if (String.IsNullOrWhiteSpace(sourceCodePath)) throw new ArgumentNullException("sourceCodePath");
            if (fileExtensionsToLookFor == null || fileExtensionsToLookFor.Count == 0) throw new ArgumentNullException("fileExtensionsToLookFor");

            var indexer = Ioc.Get<IIndexer>(
                new ConstructorArgument("idxPath", pathToStoreTheIndexFiles),
                new ConstructorArgument("srcPath", sourceCodePath),
                new ConstructorArgument("fileExtensions", fileExtensionsToLookFor));

            return indexer;
        }

        public static ISearcher GetSearcher(String pathToIndexFiles)
        {
            if (String.IsNullOrWhiteSpace(pathToIndexFiles)) throw new ArgumentNullException("pathToIndexFiles");

            var searcher = Ioc.Get<ISearcher>(
                new ConstructorArgument("idxPath", pathToIndexFiles));

            return searcher;
        }

        internal static IFileReader GetFileReader(IList<String> fileExtensions)
        {
            if (fileExtensions == null || fileExtensions.Count == 0) throw new ArgumentNullException("fileExtensions");

            var reader = Ioc.Get<IFileReader>(
                new ConstructorArgument("fileExtensions", fileExtensions));
            return reader;
        }

        internal static ISearchResultContainer GetResultContainer(IList<Tuple<float, Document>> listOfFindings)
        {
            if (listOfFindings == null) throw new ArgumentNullException("topDocs");

            var container = Ioc.Get<ISearchResultContainer>(
                new ConstructorArgument("listOfFindings", listOfFindings));

            return container;
        }

        internal static ISearchResult GetResult(float searchScore, Document document)
        {
            if (document == null) throw new ArgumentNullException("document");

            var result = Ioc.Get<ISearchResult>(
                new ConstructorArgument("searchScore", searchScore),
                new ConstructorArgument("document", document));

            return result;
        }

        internal static Lucene.Net.Store.Directory GetLuceneDirectory(String indexPath)
        {
            return FSDirectory.Open(indexPath);
        }

        internal static Analyzer GetLuceneAnalyzer()
        {
			//return new WhitespaceAnalyzer();
			return GetSourceCodeAnalyzer();
        }

        internal static IndexWriter GetLuceneIndexWriter(Lucene.Net.Store.Directory indexDirectory, Analyzer analyzer)
        {
            return new IndexWriter(indexDirectory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED);
        }

        internal static Document GetLuceneDocument()
        {
            return new Document();
        }

        internal static Field GetLuceneStoredAndIndexedField(String name, String content)
        {
            return new Field(name,
                    content,
                    Field.Store.YES,
                    Field.Index.ANALYZED);
        }

        internal static Field GetLuceneNotStoredButIndexedField(String name, String content)
        {
            return new Field(name,
                    content,
                    Field.Store.NO,
                    Field.Index.ANALYZED);
        }

        internal static TermQuery GetLuceneSearchQuery(String fieldName, String searchPattern)
        {
            var term = new Term(fieldName, searchPattern);
            var query = new TermQuery(term);
            return query;
        }

		internal static Tokenizer GetSourceCodeTokenizer(TextReader input)
		{
			return new SourceCodeTokenizer(input);
		}

		internal static Analyzer GetSourceCodeAnalyzer()
		{
			return new SourceCodeAnalyzer();
		}
    }
}
