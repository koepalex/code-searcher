using CodeSearcher.BusinessLogic;
using CodeSearcher.Interfaces;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace CodeSearcher.Tests.SystemTests
{
    [TestFixture]
    [Category("NotSafeForCI")]
    public class BookCountOfMonteCristoTests
    {
        private string m_IndexFolder;
        private string m_SourcePath;
        private const string m_BookFolderName = "The Count of Monte Cristo";
        private const string m_BookFileName = "The Count of Monte Cristo.txt";

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
            m_SourcePath = TestHelper.GetPathToSystemTestData(m_BookFolderName);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (Directory.Exists(m_IndexFolder))
            {
                Directory.Delete(m_IndexFolder, true);
            }
        }

        #region Test Classes

        private class TestResultExporterAdapter : IResultExporter
        {
            private MemoryStream _stream;
            private IResultExporter _exporter;

            internal TestResultExporterAdapter()
            {
                _stream = new MemoryStream();
                var writer = new StreamWriter(_stream);
                _exporter = Factory.GetResultExporter(writer);
                
            }
            public void Dispose()
            {
                _exporter?.Dispose();
                _stream?.Close();
            }

            public void Export(ISearchResultContainer searchResultContainer, string searchedWord)
            {
                _exporter.Export(searchResultContainer, searchedWord);
            }

            internal void Verify()
            {
                var reader = new StreamReader(_stream);
                _stream.Seek(0, SeekOrigin.Begin);
                int counter = 0;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    counter++;

                    if (counter == 0)
                    {
                        Assert.That(line, Is.EqualTo("56;look at the file size will have to do, but we will try to see a"));
                    }

                    if(counter == 1019)
                    {
                        Assert.That(line, Is.EqualTo("34246;old reports, which I was so anxious to put an end to, will"));
                    }

                    if(counter == 1957)
                    {
                        Assert.That(line, Is.EqualTo("62009;\"the count's generosity is too overwhelming; Valentine will"));
                    }
                }
                Assert.That(counter, Is.EqualTo(1957));
                Assert.That(_stream.Length, Is.EqualTo(121472));
            }
        }
        #endregion

        #region Tests

        [Test]
        [Order(1)]
        [NonParallelizable]
        public void Test_CreateIndexOfTheCountOfMonteCristo()
        {
            var logic = GetCodeSearchLogic();
            logic.CreateNewIndex(
                () => { },
                (fileName) =>
                {
                    Assert.That(fileName.EndsWith(m_BookFileName));
                },
                (numberOfFiles, timeSpan) =>
                {
                    Assert.That(numberOfFiles, Is.EqualTo(1));
                });
        }

        [Test]
        [Order(2)]
        [NonParallelizable]
        public void Test_Search_Word_Large()
        {
            var logic = GetCodeSearchLogic();
            string searchWord = "large";

            var printerStub = new Mock<ISingleResultPrinter>();
            printerStub.SetupAllProperties();
            printerStub.Setup(
                x => x.Print(
                    It.IsAny<string>(),
                    It.IsAny<string>()
                )
            );

            logic.SearchWithinExistingIndex(
                startCallback: () => { },
                getSearchWord: () => { return (searchWord, true); },
                getMaximumNumberOfHits: () => { return 1000; },
                getHitsPerPage: () => { return -1; },
                getExporter: () => { return (false, null); },
                getSingleResultPrinter: () => { return printerStub.Object; },
                finishedCallback: (timeSpan) => { },
                endOfSearchCallback: () => { });

            printerStub.VerifyAll();
        }

        [Test]
        [Order(3)]
        [NonParallelizable]
        public void Test_SearchAndExport_Word_The()
        {
            var logic = GetCodeSearchLogic();
            string searchWord = "the";

            var printerStub = new Mock<ISingleResultPrinter>();
            printerStub.SetupAllProperties();

            var exporterStub = new Mock<IResultExporter>();
            exporterStub.Setup(x => x.Export(
                It.IsAny<ISearchResultContainer>(),
                It.Is<string>(
                    s => s.Equals("the"))));

            logic.SearchWithinExistingIndex(
                startCallback: () => { },
                getSearchWord: () => { return (searchWord, true); },
                getMaximumNumberOfHits: () => { return 1000; },
                getHitsPerPage: () => { return -1; },
                getExporter: () => { return (true, exporterStub.Object); },
                getSingleResultPrinter: () => { return printerStub.Object; },
                finishedCallback: (timeSpan) => { },
                endOfSearchCallback: () => { });

            exporterStub.VerifyAll();
        }

        [Test]
        [Order(4)]
        [NonParallelizable]
        public void Test_SearchAndExport_Word_Will()
        {
            var logic = GetCodeSearchLogic();
            string searchWord = "Will";

            var printerStub = new Mock<ISingleResultPrinter>();
            printerStub.SetupAllProperties();

            var exporter = new TestResultExporterAdapter();

            logic.SearchWithinExistingIndex(
                startCallback: () => { },
                getSearchWord: () => { return (searchWord, true); },
                getMaximumNumberOfHits: () => { return 2000; },
                getHitsPerPage: () => { return -1; },
                getExporter: () => { return (true, exporter); },
                getSingleResultPrinter: () => { return printerStub.Object; },
                finishedCallback: (timeSpan) => { },
                endOfSearchCallback: () => { },
                exportFinishedCallback: () =>
                {
                    exporter.Verify();
                    exporter.Dispose();
                });
        }
        #endregion

        #region Private Implementation
        private ICodeSearcherLogic GetCodeSearchLogic()
        {
            var loggerStub = new Mock<ICodeSearcherLogger>();

            return Factory.GetCodeSearcherLogic(
                loggerStub.Object,
                () => m_IndexFolder,
                () => m_SourcePath,
                () => new List<string> { ".txt" });
        }
        #endregion
    }
}
