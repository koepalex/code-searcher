using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeSearcher.BusinessLogic.Indexer;
using CodeSearcher.BusinessLogic.Searcher;
using NUnit.Framework;

namespace CodeSearcher.Tests.SystemTests
{
    [TestFixture]
    public class BookAliceTests
    {
        private string m_IndexFolder;
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
        }

        #region Tests

        [Test]
        public void Test_CreateIndex_Of_Alice(bool throwAssert = true)
        {
            var sourcePath = TestHelper.GetPathToSystemTestData(m_BookFolderName);
            var allowedExtensions = new List<string> { ".htm" };
            using (var indexer = new DefaultIndexer(m_IndexFolder, sourcePath, allowedExtensions))
            {
                int numberOfFiles = 0;
                indexer.IndexerProcessFile += (sender, args) => {
                    Debug.WriteLine("Processed file: {0}", args.FileName);
                    numberOfFiles++;
                };

                var task = indexer.CreateIndex();
                task.Wait();

                if (throwAssert)
                {
                    Assert.That(numberOfFiles, Is.EqualTo(1));
                }
            }
        }

        [Test]
        public void Test_Search_Span_Expect_320()
        {
            if (!Directory.EnumerateFiles(m_IndexFolder).Any())
            {
                Test_CreateIndex_Of_Alice(false);
            }

            var searcher = new DefaultSearcher(m_IndexFolder);
            searcher.SearchFileContent("span", 500, (resultContainer) =>
            {
                
                Assert.That(resultContainer.Count(), Is.EqualTo(1));
            });
        }
        #endregion
    }
}
