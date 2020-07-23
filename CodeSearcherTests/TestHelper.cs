using System;
using System.IO;
using System.Reflection;

namespace CodeSearcher.Tests
{
    internal static class TestHelper
	{
        private static readonly Random m_Random = new Random(DateTime.Now.Millisecond);
        internal const string BigFileName = "BigBoy.bin";

        internal static string GetTestEnvironmentPath
		{
			get
			{
				return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			}
		}

        internal static string GetPathToIntegrationTestData(string folder)
        {
            return Path.Combine(GetTestEnvironmentPath, "IntegrationTests", "TestData", folder);
        }

        internal static string GetPathToSystemTestData(string folder)
        {
            return Path.Combine(GetTestEnvironmentPath, "SystemTests", "DownloadedTestData", folder);
        }

        internal static void CreateBigDummyFile(string fullPath)
        {
            using (var fs = new FileStream(fullPath, FileMode.CreateNew))
            {
                for (ulong i = 0; i < (10 * 1024 * 1024); i++)
                {
                    fs.WriteByte((byte)(m_Random.Next() % 255));
                }
                fs.Close();
            }
        }

        internal static void CreateSmallDummyFile(string fullPath)
        {
            using (var fs = new FileStream(fullPath, FileMode.OpenOrCreate))
            {
                fs.Seek(42, SeekOrigin.Begin);
                fs.WriteByte(42);
            }
        }
    }
}

