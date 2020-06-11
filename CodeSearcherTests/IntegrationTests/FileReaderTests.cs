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
    [Category("SafeForCI")]
    public class FileReaderTests
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

        #region 001_FilesWithWrongExtension
        [Test]
		public void Test_NoFileWithCorrectExtension_Expect_Nothing()
		{
			string pathToSearch = TestHelper.GetPathToIntegrationTestData("001_FilesWithWrongExtension");

			var fileReader = new FileReader(new List<string> { ".txt" });
			Assert.NotNull(fileReader, "can't create FileReader");

			bool noFilesFound = true;
			var task = fileReader.ReadFilesAsync(pathToSearch, files =>
			{
				noFilesFound = files.Count == 0;
			});

			task.Wait();

			Assert.IsTrue(noFilesFound, "Found Files but shouldn't");
		}
		#endregion

		#region 002_FilesWithoutContent
		[Test]
		public void Test_FilesWithoutContent_Expect_AllFilesFound()
		{
			string pathToSearch = TestHelper.GetPathToIntegrationTestData("002_FilesWithoutContent");

			var fileReader = new FileReader(new List<string> { ".cs", ".csproj", ".xml" });
			Assert.NotNull(fileReader, "can't create FileReader");

			int numberOfFiles = 0;

			var task = fileReader.ReadFilesAsync(pathToSearch, files => {
				numberOfFiles = files.Count;
			});

			task.Wait();

			Assert.That(numberOfFiles, Is.EqualTo(3), "found wrong numer of files");
		}
		#endregion

		#region 011_FolderInFolder
		[Test]
		public void Test_FolderInFolder_Expect_AllFilesFound()
		{
			string pathToSearch = TestHelper.GetPathToIntegrationTestData("011_FolderInFolder");

			var fileReader = new FileReader(new List<string> { ".cs", ".csproj", ".xml" });
			Assert.NotNull(fileReader, "can't create FileReader");

			int numberOfFiles = 0;

			var task = fileReader.ReadFilesAsync(pathToSearch, files =>
			{
				numberOfFiles += files.Count;
			});

			task.Wait();

			Assert.That(numberOfFiles, Is.EqualTo(2), "found wrong numer of files");
		}
        #endregion

        #region 012_SimpleText_LoremIpsum_As_Pattern
        [Test]
        public void Test_LoremIpsumText_Expect_All_Files_When_FilePattern_Is_Tribled()
        {
            string pathToSearch = TestHelper.GetPathToIntegrationTestData("012_SimpleText_LoremIpsum_As_Pattern");

            var fileReader = new FileReader(new List<string> { ".cs", ".cs", ".cs" });
            Assert.NotNull(fileReader, "can't create FileReader");

            int numberOfFiles = 0;

            var task = fileReader.ReadFilesAsync(pathToSearch, files =>
            {
                numberOfFiles += files.Count;
            });

            task.Wait();

            Assert.That(numberOfFiles, Is.EqualTo(5));
        }
        #endregion

        #region 013_Recrusive
        [Test]
        public void Test_Recrusion_Expect_Not_Endless_Loop()
        {
            string pathToSearch = TestHelper.GetPathToIntegrationTestData("013_Recrusive");

            var fileReader = new FileReader(new List<string> { ".lnk"});
            Assert.NotNull(fileReader, "can't create FileReader");

            int numberOfFiles = 0;

            var task = fileReader.ReadFilesAsync(pathToSearch, files =>
            {
                numberOfFiles += files.Count;
            });

            task.Wait();

            Assert.That(numberOfFiles, Is.EqualTo(1));
        }
        #endregion

        #region 015_NoAccessRights

        [Test]
        public void Test_AccessFilesWithoutAccessRights_Expect_No_Crash()
        {
            var pathToSearch = TestHelper.GetPathToIntegrationTestData("015_NoAccessRights");
            var dirInfo = new DirectoryInfo(pathToSearch);
            using (var rightsManager = new RightsManager(dirInfo))
            {
                var fileReader = new FileReader(new List<string> { ".cs" });
                int numberOfFiles = 0;
                var task = fileReader.ReadFilesAsync(pathToSearch, files =>
                {
                    numberOfFiles = files.Where(f => !f.ErrorOccurred).Count();
                });

                task.Wait();

                Assert.That(numberOfFiles, Is.EqualTo(0));
            }
        }
        #endregion

        #endregion



    }
}

