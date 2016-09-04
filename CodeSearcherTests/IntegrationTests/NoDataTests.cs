using System;
using System.Collections.Generic;
using System.IO;
using CodeSearcher.BusinessLogic.Io;
using NUnit.Framework;

namespace CodeSearcher.Tests
{
	
	public class NoDataTests
	{
		[Test]
		public void Test_NoFileWithCorrectExtension_Expect_Nothing()
		{
			string pathToTests = TestHelper.GetTestEnvironmentPath;
			string pathToSearch = Path.Combine(pathToTests, "IntegrationTests", "TestData", "001_FilesWithWrongExtension");

			var fileReader = new FileReader(new List<string> { "*.txt" });
			Assert.NotNull(fileReader, "can't create FileReader");

			bool noFilesFound = true;
			fileReader.ReadFilesAsync(pathToSearch, files => 
			{
				noFilesFound = files.Count == 0;
			});

			Assert.IsTrue(noFilesFound, "Found Files but shouldn't");
		}
	}
}

