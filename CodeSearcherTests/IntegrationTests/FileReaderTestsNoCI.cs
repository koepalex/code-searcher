using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CodeSearcher.BusinessLogic.Io;
using NUnit.Framework;

namespace CodeSearcher.Tests.IntegrationTests
{
    [TestFixture]
    public class FileReaderTestsNoCI
	{
        #region Test Classes
        private class RightsManager : IDisposable
        {
            private readonly DirectoryInfo m_DirectoryInfo;
            private readonly IList<FileStream> m_Files;

            internal RightsManager(DirectoryInfo directoryInfo)
            {
                m_DirectoryInfo = directoryInfo;
                m_Files = new List<FileStream>(10);
                foreach(var file in m_DirectoryInfo.GetFiles())
                {
                    var fs = File.Open(file.FullName, FileMode.Open, FileAccess.ReadWrite);
                    m_Files.Add(fs);
                }
            }

            public void Dispose()
            {
                foreach(var file in m_Files)
                {
                    file.Close();
                }
            }
        }
        #endregion

        #region Setup & TearDown
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            string pathToSearch = TestHelper.GetPathToIntegrationTestData("014_BigBinaryFile");
            var fullPath = Path.Combine(pathToSearch, TestHelper.BigFileName);

            if (!File.Exists(fullPath))
            {
                Directory.CreateDirectory(pathToSearch);
                TestHelper.CreateBigDummyFile(fullPath);
            }
        }
        #endregion

        #region Tests

        #region 014_BigBinaryFile

        [Test]
        [Category("NotSafeForCI")]
        public void Test_BigBinaryFiles_Expect_Text_Interpretation()
        {
            string pathToSearch = TestHelper.GetPathToIntegrationTestData("014_BigBinaryFile");

            var fileReader = new FileReader(new List<string> { ".bin" });
            Assert.NotNull(fileReader, "can't create FileReader");

            int numberOfFiles = 0;

            var task = fileReader.ReadFilesAsync(pathToSearch, files =>
            {
                numberOfFiles += files.Count;
                Debug.Write(files[0].Text);
            });

            task.Wait();

            Assert.That(numberOfFiles, Is.EqualTo(1));
        }
        #endregion

        #region 014_XML_StarWars

        [Test]
        [Category("NotSafeForCI")]
        public void Test_Xml_Expect_Not_Problem()
        {
            string pathToSearch = TestHelper.GetPathToIntegrationTestData("014_XML_StarWars");

            var fileReader = new FileReader(new List<string> { ".xml" });
            Assert.NotNull(fileReader, "can't create FileReader");

            var task = fileReader.ReadFilesAsync(pathToSearch, files =>
            {
                Assert.That(files[0].Text.Length, Is.EqualTo(410));
            });

            task.Wait();
        }
        #endregion

        #endregion



    }
}

