using System;
using NUnit.Framework;
using System.Collections.Generic;
using CodeSearcher.BusinessLogic.Io;
using CodeSearcher.BusinessLogic.Indexer;
using CodeSearcher.BusinessLogic.Searcher;
using CodeSearcher.BusinessLogic;
using System.IO;


namespace CodeSearcher.Tests
{
    public class Foo
    {
        private IList<string> m_AllowedExtension = new List<string>() { ".cs", ".sln", ".csproj" };
        private const string m_SourcePath = @"/Volumes/Data/temp/";
        private const string m_IndexPath = @"/Volumes/Data/temp/100idx";

        [Test]
        public void foo()
        {
            var fr = new FileReader(m_AllowedExtension);
            Assert.NotNull(fr, "can't create FileReader");

            fr.ReadFilesAsync(m_SourcePath, (fileStr) =>
            {
                foreach(var str in fileStr) {
                    Console.WriteLine(str.FilePath);
                }
            });
        }

        [Test]
        public void foo2()
        {
            var fr = new FileReader(m_AllowedExtension);
            Assert.NotNull(fr, "can't create FileReader");

            fr.ReadFilesAsync(m_SourcePath, (fileStr) =>
            {
                foreach (var str in fileStr)
                {
                    Console.WriteLine(str.Text);
                }
            });
        }

        [Test]
        public void foo3()
        {
            if (Directory.Exists(m_IndexPath)) Directory.Delete(m_IndexPath, true);

            using (var indexer = new DefaultIndexer(m_IndexPath, m_SourcePath, m_AllowedExtension))
            {
                indexer.IndexerProcessFile += (sender, args) => {
                    Console.WriteLine("Processed file: {0}", args.FileName);
                };

                var task = indexer.CreateIndex();

                task.Wait();
            }
        }


		// Source path need at least on file (*.cs) with the seperateName "Logger" to successfull run this test
        [Test]
        public void foo4()
        {
            using (var searcher = new DefaultSearcher(m_IndexPath))
            {
                searcher.SearchFileContent("Logger", 250, (searchResultContainer) =>
                {
                    Assert.AreNotEqual(0, searchResultContainer.NumberOfHits);
                });
            }
        }

        [Test]
        public void foo5()
        {
            using(var searcher = Factory.GetSearcher(m_IndexPath))
            {
                Assert.IsNotNull(searcher);
            }
        }

        [Test]
        public void foo6()
        {
            using (var indexer = Factory.GetIndexer(m_IndexPath, m_SourcePath, m_AllowedExtension))
            {
                Assert.IsNotNull(indexer);
            }
        }

        [Test]
        public void foo7()
        {
            using(var searcher = Factory.GetSearcher(m_IndexPath))
            {
                searcher.SearchFileContent("Logger", 250, (searchResultContainer) =>
                {
                    foreach(var result in searchResultContainer)
                    {
                        Assert.AreNotEqual(0, result.SearchScore);
                        Assert.That(result.FileName, Is.Not.Null);
                        Assert.That(result.FileName, Is.Not.Empty);
                    }
                });
            }
        }

        [Test]
        public void foo8()
        {
            using (var searcher = Factory.GetSearcher(m_IndexPath))
            {
                searcher.SearchFileContent("NotExistingPAttern2342%$54", 250, (searchResultContainer) =>
                {
                    Assert.AreEqual(4, searchResultContainer.NumberOfHits);
                });
            }
        }
    }
}
