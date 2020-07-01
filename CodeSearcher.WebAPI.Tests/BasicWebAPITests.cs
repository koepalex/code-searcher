using CodeSearcher.BusinessLogic;
using CodeSearcher.Interfaces.API.Model.Requests;
using CodeSearcher.Interfaces.API.Model.Response;
using CodeSearcher.Interfaces.Constants;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
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
            var metaPath = WebTestHelper.GetPathToTestData("Meta");
            if (Directory.Exists(metaPath))
            {
                Directory.Delete(metaPath, true);
                Directory.CreateDirectory(metaPath);
            }
        }

        [TearDown]
        public void TearDown()
        {
            m_TestServer.Dispose();
            var metaPath = WebTestHelper.GetPathToTestData("Meta");
            if (Directory.Exists(metaPath))
            {
                Directory.Delete(metaPath, true);
                Directory.CreateDirectory(metaPath);
            }
        }

        [Test]
        public async Task Test_Route_Expect_200OK()
        {
            var client = m_TestServer.CreateClient();

            using (var response = await client.GetAsync(APIRoutes.CodeSearcherRoute))
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
            using (var response = await client.PutAsync(APIRoutes.ConfigurationRoute, content))
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            }
                    
        }

        [Test]
        public async Task Test_ConfigureManagementPath_WithEmptyPath_Expect_400BadRequest()
        {
            var client = m_TestServer.CreateClient();

            var model = new { managementInformationPath = string.Empty };
            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            using (var response = await client.PutAsync(APIRoutes.ConfigurationRoute, content))
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            }

        }

        [Test]
        public async Task Test_ConfigureManagementPath_NullPath_Expect_400BadRequest()
        {
            var client = m_TestServer.CreateClient();

            var model = new { managementInformationPath = string.Empty };
            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            using (var response = await client.PutAsync(APIRoutes.ConfigurationRoute, content))
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
            using (var response = await client.PutAsync(APIRoutes.ConfigurationRoute, content))
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            }
        }

        [Test]
        public async Task Test_ConfigureManagementPath_Expect_ConfigurationApplied()
        {
            var client = m_TestServer.CreateClient();

            var newPath = WebTestHelper.GetPathToTestData("Meta");
            var model = new { managementInformationPath = newPath };
            var requestPayload = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            using (_ = await client.PutAsync(APIRoutes.ConfigurationRoute, requestPayload))
            {
            }

            using (var response = await client.GetAsync(APIRoutes.ConfigurationRoute))
            {
                response.EnsureSuccessStatusCode();
                var responsePayload = await response.Content.ReadAsStringAsync();
                var configurationModel = JsonConvert.DeserializeObject<ConfigureResponse>(responsePayload);
                Assert.That(configurationModel, Is.Not.Null);
                Assert.That(configurationModel.ManagementInformationPath, Is.EqualTo(newPath));
            }
        }

        [Test]
        public async Task Test_CreateNewIndex_Expect_NotEmptyJobId()
        {
            var client = m_TestServer.CreateClient();

            var newPath = WebTestHelper.GetPathToTestData("Meta");
            var configureModel = new { managementInformationPath = newPath };
            var requestPayload = new StringContent(JsonConvert.SerializeObject(configureModel), Encoding.UTF8, "application/json");
            using (_ = await client.PutAsync(APIRoutes.ConfigurationRoute, requestPayload))
            {
            }

            var createIndexModel = new CreateIndexRequest()
            {
                SourcePath = WebTestHelper.GetPathToTestData("01_ToIndex"),
                FileExtensions = new [] { ".txt" }
            };
            requestPayload = new StringContent(JsonConvert.SerializeObject(createIndexModel), Encoding.UTF8, "application/json");
            using (var response = await client.PostAsync(APIRoutes.CreateIndexRoute, requestPayload))
            {
                response.EnsureSuccessStatusCode();
                var responsePayload = await response.Content.ReadAsStringAsync();
                var indexModel = JsonConvert.DeserializeObject<CreateIndexResponse>(responsePayload);
                Assert.That(indexModel, Is.Not.Null);
                Assert.That(indexModel.IndexingJobId, Is.Not.Null); //TODO indexing in Background w/ correct ManagementPath
                Assert.That(indexModel.IndexingJobId, Is.Not.Empty);
            }
        }

        [Test]
        public async Task Test_DeleteIndex_Expect_Success()
        {
            var client = m_TestServer.CreateClient();

            var newPath = WebTestHelper.GetPathToTestData("Meta");
            var configureModel = new { managementInformationPath = newPath };
            var requestPayload = new StringContent(JsonConvert.SerializeObject(configureModel), Encoding.UTF8, "application/json");
            using (_ = await client.PutAsync(APIRoutes.ConfigurationRoute, requestPayload))
            {
            }

            var createIndexModel = new CreateIndexRequest()
            {
                SourcePath = WebTestHelper.GetPathToTestData("01_ToIndex"),
                FileExtensions = new[] { ".txt" }
            };
            requestPayload = new StringContent(JsonConvert.SerializeObject(createIndexModel), Encoding.UTF8, "application/json");
            using (_ = await client.PostAsync(APIRoutes.CreateIndexRoute, requestPayload))
            {
            }

            Thread.Sleep(5000);
            int indexIdToDelete;
            using (var response = await client.GetAsync(APIRoutes.CodeSearcherRoute))
            {
                var responsePayload = await response.Content.ReadAsStringAsync();
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(Factory.Get().GetCodeSearcherIndexJsonConverter());
                var indexesModel = JsonConvert.DeserializeObject<GetIndexesResponse>(responsePayload, settings);
                Assert.That(indexesModel, Is.Not.Null);
                Assert.That(indexesModel.Indexes, Is.Not.Null);
                Assert.That(indexesModel.Indexes, Is.Not.Empty);
                Assert.That(indexesModel.Indexes.Length, Is.EqualTo(1));
                indexIdToDelete = indexesModel.Indexes[0].ID;
            }

            var uri = $"{APIRoutes.CodeSearcherRoute}?IndexID=\"{indexIdToDelete.ToString()}\"";
            using (var response = await client.DeleteAsync(uri))
            {
                response.EnsureSuccessStatusCode();
                var responsePayload = await response.Content.ReadAsStringAsync();
                var deleteModel = JsonConvert.DeserializeObject<DeleteIndexResponse>(responsePayload);
                Assert.That(deleteModel, Is.Not.Null);
                Assert.That(deleteModel.Succeeded, Is.True);
            }

        }
    }
}