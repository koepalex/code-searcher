using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Net;
using System.Net.Http;
using System.Text;
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

        [Test]
        public async Task Test_ConfigureManagementPath_WithNonExistingPath_Expect_400BadRequest()
        {
            var client = m_TestServer.CreateClient();

            var model = new { managementInformationPath = @"B:\Hope\This\Dont\Exist" };
            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            using (var response = await client.PutAsync("/api/CodeSearcher/configure", content))
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            }
                    
        }

        [Test]
        public async Task Test_ConfigureManagementPath_Expect_200OK()
        {
            var client = m_TestServer.CreateClient();

            var model = new { managementInformationPath = WebTestHelper.GetPathToTestData("Meta") };
            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            using (var response = await client.PutAsync("/api/CodeSearcher/configure", content))
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            }
        }
    }
}