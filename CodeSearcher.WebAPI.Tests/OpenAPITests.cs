using CodeSearcher.Interfaces.Constants;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using NUnit.Framework;
using System.Net;
using System.Threading.Tasks;

namespace CodeSearcher.WebAPI.Tests
{
    [TestFixture]
    [Category("SafeForCI")]
    class OpenAPITests
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
        public async Task Test_Root_Expect_301PermanentlyMoved_To_SwaggerUI()
        {
            using (var client = m_TestServer.CreateClient())
            using (var response = await client.GetAsync("/"))
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.MovedPermanently));

                var newLocation = response.Headers.Location.ToString();
                Assert.That(newLocation, Is.EqualTo("index.html"));
            }
        }

        [Test]
        public async Task Test_SwaggerUI_Expect_200OK()
        {
            using (var client = m_TestServer.CreateClient())
            using (var response = await client.GetAsync(APIRoutes.OpenApiUiRoute))
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

                var content = await response.Content.ReadAsStringAsync();
                Assert.That(content.Contains("Swagger UI"));
            }
        }

        [Test]
        public async Task Test_SwaggerJson_Expect_200OK()
        {
            using (var client = m_TestServer.CreateClient())
            using (var response = await client.GetAsync(APIRoutes.OpenApiDefinitionRoute))
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

                var content = await response.Content.ReadAsStringAsync();
                Assert.That(content.Contains("Code Searcher API"));
            }
        }
    }
}
