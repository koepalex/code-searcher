using CodeSearcher.BusinessLogic;
using CodeSearcher.Interfaces.API.Model.Requests;
using CodeSearcher.Interfaces.API.Model.Response;
using CodeSearcher.Interfaces.Constants;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CodeSearcher.WebAPI.Tests
{
    [TestFixture]
    [Category("NotSafeForCI")]
    [Order(500)]
    public class ComplexeWebAPITests
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
        public async Task Test_DeleteIndex_Expect_Success()
        {
            using (var client = m_TestServer.CreateClient())
            {
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

                //TODO can be optimized with API method to get information if background job still running
                GetIndexesResponse indexesModel;
                int count = 0;
                do
                {
                    using (var response = await client.GetAsync(APIRoutes.CodeSearcherRoute))
                    {
                        var responsePayload = await response.Content.ReadAsStringAsync();
                        var settings = new JsonSerializerSettings();
                        settings.Converters.Add(Factory.Get().GetCodeSearcherIndexJsonConverter());
                        indexesModel = JsonConvert.DeserializeObject<GetIndexesResponse>(responsePayload, settings);
                        Assert.That(indexesModel, Is.Not.Null);
                        Assert.That(indexesModel.Indexes, Is.Not.Null);
                    }
                    await Task.Delay(250);
                    //timeout
                    Assert.That(count++, Is.LessThan(100));
                } while (indexesModel.Indexes.Length < 1);

                var deleteRequestModel = new DeleteIndexRequest
                {
                    IndexID = indexesModel.Indexes[0].ID
                };

                // simplified API client.DeleteAsync doesn't allow to set content
                var deleteRequest = new HttpRequestMessage
                {
                    Method = HttpMethod.Delete,
                    RequestUri = new Uri(client.BaseAddress, APIRoutes.CreateIndexRoute),
                    Content = new StringContent(JsonConvert.SerializeObject(deleteRequestModel), Encoding.UTF8, "application/json")
                };

                using (var response = await client.SendAsync(deleteRequest))
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
}
