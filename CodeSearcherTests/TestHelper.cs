using System;
using System.IO;
using System.Reflection;

namespace CodeSearcher.Tests
{
	internal static class TestHelper
	{
		internal static string GetTestEnvironmentPath
		{
			get
			{
				return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			}
		}
	}
}

