using System.IO;
using System.Reflection;

namespace CodeSearcher.WebAPI.Tests
{
    internal static class WebTestHelper
    {

        internal static string GetTestEnvironmentPath
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
        }

        internal static string GetPathToTestData(string folder)
        {
            return Path.Combine(GetTestEnvironmentPath, "TestData", folder);
        }

    }
}
