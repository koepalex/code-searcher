using CodeSearcher.Interfaces.API.Model.Requests;
using CodeSearcher.Interfaces.API.Model.Response;
using CodeSearcher.Interfaces.Constants;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using NUnit.Framework;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CodeSearcher.WebAPI.Tests
{
    [TestFixture]
    [Category("SafeForCI")]
    [Order(200)]
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

            // CleanUp ...
            var metaPath = WebTestHelper.GetPathToTestData("Meta");
            if (Directory.Exists(metaPath))
            {
                Directory.Delete(metaPath, true);
            }
        }

        [Test]
        [Order(1)]
        public async Task Test_Route_Expect_200OK()
        {
            using var client = m_TestServer.CreateClient();
            using var response = await client.GetAsync(APIRoutes.CodeSearcherRoute);
            response.EnsureSuccessStatusCode();
        }

        [Test]
        [Order(2)]
        public async Task Test_ConfigureManagementPath_WithNonExistingPath_Expect_400BadRequest()
        {
            using var client = m_TestServer.CreateClient();
            var model = new { managementInformationPath = @"B:\Hope\This\Dont\Exist" };
            using var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            using var response = await client.PutAsync(APIRoutes.ConfigurationRoute, content);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        [Order(3)]
        public async Task Test_ConfigureManagementPath_WithEmptyPath_Expect_400BadRequest()
        {
            using var client = m_TestServer.CreateClient();
            var model = new { managementInformationPath = string.Empty };
            using var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            using var response = await client.PutAsync(APIRoutes.ConfigurationRoute, content);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        [Order(4)]
        public async Task Test_ConfigureManagementPath_NullPath_Expect_400BadRequest()
        {
            using var client = m_TestServer.CreateClient();
            var model = new { managementInformationPath = string.Empty };
            using var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            using var response = await client.PutAsync(APIRoutes.ConfigurationRoute, content);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        [Order(5)]
        public async Task Test_ConfigureManagementPath_Expect_200OK()
        {
            var metaPath = WebTestHelper.GetPathToTestData("Meta");
            if (!Directory.Exists(metaPath))
            {                
                Directory.CreateDirectory(metaPath);
            }
            using var client = m_TestServer.CreateClient();            
            var model = new { managementInformationPath = metaPath };
            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            
            using var response = await client.PutAsync(APIRoutes.ConfigurationRoute, content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        [Order(6)]
        public async Task Test_ConfigureManagementPath_Expect_ConfigurationApplied()
        {
            var metaPath = WebTestHelper.GetPathToTestData("Meta");
            if (!Directory.Exists(metaPath))
            {                
                Directory.CreateDirectory(metaPath);
            }
            using var client = m_TestServer.CreateClient();            
            var model = new { managementInformationPath = metaPath };
            using var requestPayload = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            using (_ = await client.PutAsync(APIRoutes.ConfigurationRoute, requestPayload))
            {
            }

            using var response = await client.GetAsync(APIRoutes.ConfigurationRoute);
            response.EnsureSuccessStatusCode();
            
            var responsePayload = await response.Content.ReadAsStringAsync();
            var configurationModel = JsonConvert.DeserializeObject<ConfigureResponse>(responsePayload);
            Assert.That(configurationModel, Is.Not.Null);
            Assert.That(configurationModel.ManagementInformationPath, Is.EqualTo(metaPath));
        }

        [Test]
        [Order(7)]
        public async Task Test_CreateNewIndex_Expect_NotEmptyJobId()
        {
            using var client = m_TestServer.CreateClient();
            var newPath = WebTestHelper.GetPathToTestData("Meta");
            var configureModel = new { managementInformationPath = newPath };
            using (var requestPayload = new StringContent(JsonConvert.SerializeObject(configureModel), Encoding.UTF8, "application/json"))
            using (_ = await client.PutAsync(APIRoutes.ConfigurationRoute, requestPayload))
            {
            }

            var createIndexModel = new CreateIndexRequest()
            {
                SourcePath = WebTestHelper.GetPathToTestData("01_ToIndex"),
                FileExtensions = new[] { ".txt" }
            };
            using (var requestPayload = new StringContent(JsonConvert.SerializeObject(createIndexModel), Encoding.UTF8, "application/json"))
            using (var response = await client.PostAsync(APIRoutes.CreateIndexRoute, requestPayload))
            {
                response.EnsureSuccessStatusCode();
                var responsePayload = await response.Content.ReadAsStringAsync();
                var indexModel = JsonConvert.DeserializeObject<CreateIndexResponse>(responsePayload);
                Assert.That(indexModel, Is.Not.Null);
                Assert.That(indexModel.IndexingJobId, Is.Not.Null);
                Assert.That(indexModel.IndexingJobId, Is.Not.Empty);
            }
        }

    }
}