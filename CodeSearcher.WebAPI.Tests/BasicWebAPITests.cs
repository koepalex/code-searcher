using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using NUnit.Framework;
using System.Threading.Tasks;

namespace CodeSearcher.WebAPI.Tests
{
    [TestFixture]
    [Category("SafeForCI")]
    public class BasicWebAPITests
    {
        private TestServer m_TestServer;

        [SetUp]
        public void SetUp()
        {
            var builder = new WebHostBuilder()
                .UseEnvironment("Development")
                .UseStartup<CodeSearcher.WebAPI.Startup>();

            m_TestServer = new TestServer(builder);
        }

        [TearDown]
        public void TearDown()
        {
            m_TestServer.Dispose();
        }

        [Test]
        public async Task Test_Route_Expect_200OK()
        {
            var client = m_TestServer.CreateClient();

            using (var response = await client.GetAsync("/api/CodeSearcher"))
            {
                response.EnsureSuccessStatusCode();
            }
        }
    }
}