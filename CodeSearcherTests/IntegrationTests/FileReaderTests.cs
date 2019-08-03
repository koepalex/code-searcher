using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using CodeSearcher.BusinessLogic.Io;
using NUnit.Framework;

namespace CodeSearcher.Tests.IntegrationTests
{
    [TestFixture]
	public class FileReaderTests
	{
        #region Setup & TearDown
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            string pathToSearch = TestHelper.GetPathToTestData("014_BigBinaryFile");
            var fullPath = Path.Combine(pathToSearch, TestHelper.BigFileName);

            if (!File.Exists(fullPath))
            {
                Directory.CreateDirectory(pathToSearch);
                TestHelper.CreateDummyFile(fullPath);
            }
        }
        #endregion
        #region Tests

        #region 001_FilesWithWrongExtension
        [Test]
		public void Test_NoFileWithCorrectExtension_Expect_Nothing()
		{
			string pathToSearch = TestHelper.GetPathToTestData("001_FilesWithWrongExtension");

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
			string pathToSearch = TestHelper.GetPathToTestData("002_FilesWithoutContent");

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
			string pathToSearch = TestHelper.GetPathToTestData("011_FolderInFolder");

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
            string pathToSearch = TestHelper.GetPathToTestData("012_SimpleText_LoremIpsum_As_Pattern");

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
            string pathToSearch = TestHelper.GetPathToTestData("013_Recrusive");

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

        #region 014_BigBinaryFile

        [Test]
        public void Test_BigBinaryFiles_Expect_Text_Interpretation()
        {
            string pathToSearch = TestHelper.GetPathToTestData("014_BigBinaryFile");

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
        public void Test_Xml_Expect_Not_Problem()
        {
            string pathToSearch = TestHelper.GetPathToTestData("014_XML_StarWars");

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

