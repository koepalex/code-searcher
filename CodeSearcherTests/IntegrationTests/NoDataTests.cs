using System;
using System.Collections.Generic;
using System.IO;
using CodeSearcher.BusinessLogic.Io;
using NUnit.Framework;

namespace CodeSearcher.Tests
{
	[TestFixture]
	public class IntegrationTests
	{
		#region Fields
		private string m_PathToTestsData;
		#endregion

		#region Setup and TearDown
		[SetUp]
		public void SetUp()
		{

		}

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			m_PathToTestsData = TestHelper.GetTestEnvironmentPath;
		}

		[TearDown]
		public void TearDown()
		{
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
		}
		#endregion

		#region Tests

		#region 001_FilesWithWrongExtension
		[Test]
		public void Test_NoFileWithCorrectExtension_Expect_Nothing()
		{
			string pathToSearch = GetPathToTestData("001_FilesWithWrongExtension");

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
			string pathToSearch = GetPathToTestData("002_FilesWithoutContent");

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

		#region 003_FolderInFolder
		[Test]
		public void Test_FolderInFolder_Expect_AllFilesFound()
		{
			string pathToSearch = GetPathToTestData("011_FolderInFolder");

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
		#endregion

		#region Private Implementation
		private string GetPathToTestData(string folder)
		{
			return Path.Combine(m_PathToTestsData, "IntegrationTests", "TestData", folder);
		}
		#endregion
	}
}

