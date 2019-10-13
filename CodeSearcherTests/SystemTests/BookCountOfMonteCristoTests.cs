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
        public void Test_Search_Word_The()
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
        #endregion

        #region Private Implementation
        private CodeSearcherLogic GetCodeSearchLogic()
        {
            var loggerStub = new Mock<ICodeSearcherLogger>();

            return new CodeSearcherLogic(
                loggerStub.Object,
                () => m_IndexFolder,
                () => m_SourcePath,
                () => new List<string> { ".txt" });
        }
        #endregion
    }
}
