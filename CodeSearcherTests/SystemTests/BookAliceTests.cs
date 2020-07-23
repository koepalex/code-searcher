using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CodeSearcher.BusinessLogic.Indexer;
using CodeSearcher.BusinessLogic.Searcher;
using NUnit.Framework;

namespace CodeSearcher.Tests.SystemTests
{
    [TestFixture]
    [Category("NotSafeForCI")]
    public class BookAliceTests
    {
        private string m_IndexFolder;
        private readonly IList<string> m_Files = new List<string>(2048);
        private const string m_BookFolderName = "Alice Adventure in Wonderland";

        [OneTimeSetUp]
        public void OnTimeSetup()
        {
            // create new folder for lucene index
            var tempFolder = Path.GetTempPath();
            m_IndexFolder = Path.Combine(tempFolder, $"IndexFolder{DateTime.UtcNow.Ticks.ToString()}");

            if (Directory.Exists(m_IndexFolder))
            {
                Directory.Delete(m_IndexFolder, true);
            }
            Directory.CreateDirectory(m_IndexFolder);

            // create dummy files
            var sourcePath = TestHelper.GetPathToSystemTestData(m_BookFolderName);

            for (int i = 0; i < 2048; i++)
            {
                var fullPath = Path.Combine(sourcePath, $"{i.ToString()}.htm");
                m_Files.Add(fullPath);
                TestHelper.CreateSmallDummyFile(fullPath);
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (Directory.Exists(m_IndexFolder))
            {
                Directory.Delete(m_IndexFolder, true);
            }

            foreach (var file in m_Files)
            {
                File.Delete(file);
            }
        }

        #region Tests

        [Test]
        public void Test_CreateIndex_Of_Alice()
        {
            Assert.That(CreateIndexOfAlice(), Is.EqualTo(2049));
        }

        [Test]
        public void Test_Search_Span_Expect_320()
        {
            if (!Directory.EnumerateFiles(m_IndexFolder).Any())
            {
                CreateIndexOfAlice();
            }

            using (var searcher = new DefaultSearcher(m_IndexFolder))
            {
                searcher.SearchFileContent("span", 500, (resultContainer) =>
                {

                    Assert.That(resultContainer.Count(), Is.EqualTo(1));
                });
            }
        }
        #endregion

        private int CreateIndexOfAlice()
        {
            var sourcePath = TestHelper.GetPathToSystemTestData(m_BookFolderName);
            var allowedExtensions = new List<string> { ".htm" };
            int numberOfFiles = 0;
            using (var indexer = new DefaultIndexer(m_IndexFolder, sourcePath, allowedExtensions))
            {
                
                indexer.IndexerProcessFile += (sender, args) =>
                {
                    Debug.WriteLine("Processed file: {0}", args.FileName);
                    numberOfFiles++;
                };

                var task = indexer.CreateIndex();
                task.Wait();
            }

            return numberOfFiles;
        }
    }
}
