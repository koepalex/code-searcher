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
            var logic = GetCodeSearchLogicForIndexing();
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
            var logic = GetCodeSearchLogicForLookup();
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
                () => { },
                () => { return (searchWord, true); },
                printerStub.Object,
                (timeSpan) => { },
                () => { });

            printerStub.VerifyAll();
        }
        #endregion

        #region Private Implementation
        private CodeSearcherLogic GetCodeSearchLogicForLookup()
        {
            Mock<ICmdLineHandler> cmdHandlerStub = GetDefaultCmdLineHandlerStub();
            cmdHandlerStub.SetupGet(x => x[It.Is<string>(s => s.Equals("ProgramMode"))]).Returns("s");

            var loggerStub = new Mock<ICodeSearcherLogger>();

            return new CodeSearcherLogic(
                cmdHandlerStub.Object,
                loggerStub.Object,
                () => cmdHandlerStub.Object["IndexPath"],
                () => cmdHandlerStub.Object["SourcePath"],
                () => new List<string> { ".txt" });
        }

        private CodeSearcherLogic GetCodeSearchLogicForIndexing()
        {
            Mock<ICmdLineHandler> cmdHandlerStub = GetDefaultCmdLineHandlerStub();
            cmdHandlerStub.SetupGet(x => x[It.Is<string>(s => s.Equals("ProgramMode"))]).Returns("i");
            cmdHandlerStub.SetupGet(x => x[It.Is<string>(s => s.Equals("SourcePath"))]).Returns(m_SourcePath);

            var loggerStub = new Mock<ICodeSearcherLogger>();

            return new CodeSearcherLogic(
                cmdHandlerStub.Object,
                loggerStub.Object,
                () => cmdHandlerStub.Object["IndexPath"],
                () => cmdHandlerStub.Object["SourcePath"],
                () => new List<string> { ".txt" });
        }

        private Mock<ICmdLineHandler> GetDefaultCmdLineHandlerStub()
        {
            var cmdHandlerStub = new Mock<ICmdLineHandler>();
            cmdHandlerStub.SetupGet(x => x.ExportToFile).Returns("ExportToFile");
            cmdHandlerStub.SetupGet(x => x.FileExtensions).Returns("FileExtensions");
            cmdHandlerStub.SetupGet(x => x.HitsPerPage).Returns("HitsPerPage");
            cmdHandlerStub.SetupGet(x => x.IndexPath).Returns("IndexPath");
            cmdHandlerStub.SetupGet(x => x.NumberOfHits).Returns("NumberOfHits");
            cmdHandlerStub.SetupGet(x => x.ProgramMode).Returns("ProgramMode");
            cmdHandlerStub.SetupGet(x => x.SearchedWord).Returns("SearchedWord");
            cmdHandlerStub.SetupGet(x => x.SourcePath).Returns("SourcePath");

            cmdHandlerStub.SetupGet(x => x[It.Is<string>(s => s.Equals("NumberOfHits"))]).Returns("1000");
            cmdHandlerStub.SetupGet(x => x[It.Is<string>(s => s.Equals("IndexPath"))]).Returns(m_IndexFolder);
            //cmdHandlerStub.SetupGet(x => x[It.Is<string>(s => s.Equals("IndexPath"))]).Returns(@"C:\test\index");
            cmdHandlerStub.SetupGet(x => x[It.Is<string>(s => s.Equals("FileExtensions"))]).Returns(".txt");


            return cmdHandlerStub;
        }
        #endregion
    }
}
