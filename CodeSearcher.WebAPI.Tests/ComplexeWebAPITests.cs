using CodeSearcher.BusinessLogic;
using CodeSearcher.BusinessLogic.Common;
using CodeSearcher.Interfaces;
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
    class VoidLogger : ICodeSearcherLogger
    {
        public void Debug(string message, params object[] parameter)
        {
        }

        public void Error(string message, params object[] parameter)
        {
        }

        public void Info(string message, params object[] parameter)
        {
        }
    }

    [TestFixture]
    [Category("NotSafeForCI")]
    [Order(500)]
    public class ComplexeWebAPITests
    {
        private TestServer m_TestServer;

        [SetUp]
        public void SetUp()
        {


            var manager = Factory.Get().GetCodeSearcherManager(new VoidLogger());
            var managementInformationTestPath = Path.Combine(WebTestHelper.GetPathToTestData("AppData"), FolderNames.ManagementFolder);
            if (!Directory.Exists(managementInformationTestPath))
            {
                Directory.CreateDirectory(managementInformationTestPath);
            }
            manager.ManagementInformationPath = managementInformationTestPath;

            var builder = new WebHostBuilder()
                .UseEnvironment("Development")
                .UseStartup<CodeSearcher.WebAPI.Startup>();
            m_TestServer = new TestServer(builder);

            var metaPath = WebTestHelper.GetPathToTestData("Meta");
            if (Directory.Exists(metaPath))
            {
                Directory.Delete(metaPath, true);
            }
        }

        [TearDown]
        public void TearDown()
        {
            m_TestServer.Dispose();

            // Cleanup, but not really  needed for the tests to work ...            
            // if the metaPath is created between tests, the second test does not work for some reason ...
            var metaPath = WebTestHelper.GetPathToTestData("Meta");
            if (Directory.Exists(metaPath))
            {
                Directory.Delete(metaPath, true);
            }
            var managementInformationTestPath = Path.Combine(WebTestHelper.GetPathToTestData("AppData"), FolderNames.ManagementFolder);
            if (Directory.Exists(managementInformationTestPath))
            {
                Directory.Delete(managementInformationTestPath, true);
            }
        }

        [Test]
        [Order(1)]
        [NonParallelizable]
        public async Task Test_DeleteIndex_Expect_Success()
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
            using (_ = await client.PostAsync(APIRoutes.CreateIndexRoute, requestPayload))
            {
            }

            //TODO can be optimized with API method to get information if background job still running
            ICodeSearcherIndex[] indexesModel;
            int count = 0;
            do
            {
                using (var response = await client.GetAsync(APIRoutes.IndexListRoute))
                {
                    var responsePayload = await response.Content.ReadAsStringAsync();
                    var settings = new JsonSerializerSettings();
                    settings.Converters.Add(Factory.Get().GetCodeSearcherIndexJsonConverter());
                    indexesModel = JsonConvert.DeserializeObject<ICodeSearcherIndex[]>(responsePayload, settings);
                    Assert.That(indexesModel, Is.Not.Null);
                }
                await Task.Delay(250);
                //timeout
                Assert.That(count++, Is.LessThan(100));
            } while (indexesModel.Length < 1);

            var deleteRequestModel = new DeleteIndexRequest
            {
                IndexID = indexesModel[0].ID
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

            // Cleanup, but not really  needed for the tests to work ...            
            // if the metaPath is created between tests, the second test does not work for some reason ...
            var metaPath = WebTestHelper.GetPathToTestData("Meta");
            if (Directory.Exists(metaPath))
            {
                Directory.Delete(metaPath, true);
            }
        }

        [Test]
        [Order(2)]
        [NonParallelizable]
        public async Task Test_SearchInIndex_Expect_Success()
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
            using (_ = await client.PostAsync(APIRoutes.CreateIndexRoute, requestPayload))
            {
            }

            ICodeSearcherIndex[] indexesModel;
            int count = 0;
            do
            {
                using (var response = await client.GetAsync(APIRoutes.IndexListRoute))
                {
                    var responsePayload = await response.Content.ReadAsStringAsync();
                    var settings = new JsonSerializerSettings();
                    settings.Converters.Add(Factory.Get().GetCodeSearcherIndexJsonConverter());
                    indexesModel = JsonConvert.DeserializeObject<ICodeSearcherIndex[]>(responsePayload, settings);
                    Assert.That(indexesModel, Is.Not.Null);
                }
                await Task.Delay(250);
                //timeout
                Assert.That(count++, Is.LessThan(100));
            } while (indexesModel.Length < 1);

            var searchModel = new SearchIndexRequest()
            {
                IndexID = indexesModel[0].ID,
                SearchWord = "erat"
            };
            using (var requestPayload = new StringContent(JsonConvert.SerializeObject(searchModel), Encoding.UTF8, "application/json"))
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    Content = requestPayload,
                    RequestUri = new Uri(client.BaseAddress, APIRoutes.SearchInIndexRoute)
                };
                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();

                    var responsePayload = await response.Content.ReadAsStringAsync();
                    var settings = new JsonSerializerSettings();
                    settings.Converters.Add(Factory.Get().GetDetailedResultJsonConverter());
                    settings.Converters.Add(Factory.Get().GetFindingsInFileJsonConverter());
                    var searchIndex = JsonConvert.DeserializeObject<SearchIndexResponse>(responsePayload, settings);
                }
            }

            // Cleanup, but not really  needed for the tests to work ...            
            // if the metaPath is created between tests, the second test does not work for some reason ...
            var metaPath = WebTestHelper.GetPathToTestData("Meta");
            if (Directory.Exists(metaPath))
            {
                Directory.Delete(metaPath, true);
            }
        }

        [Test]
        [Order(3)]
        [NonParallelizable]
        public async Task Test_CreateIndexStatusApi_Expect_Success()
        {
            using var client = m_TestServer.CreateClient();
            var newPath = WebTestHelper.GetPathToTestData("Meta");
            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }
            var configureModel = new { managementInformationPath = newPath };
            using (var requestPayload = new StringContent(JsonConvert.SerializeObject(configureModel), Encoding.UTF8, "application/json"))
            using (var response = await client.PutAsync(APIRoutes.ConfigurationRoute, requestPayload))
            {
                response.EnsureSuccessStatusCode();
            }

            var createIndexModel = new CreateIndexRequest()
            {
                SourcePath = WebTestHelper.GetPathToTestData("01_ToIndex"),
                FileExtensions = new[] { ".txt" }
            };
            CreateIndexResponse createIndexResponse = null;
            using (var requestPayload = new StringContent(JsonConvert.SerializeObject(createIndexModel), Encoding.UTF8, "application/json"))
            using (var response = await client.PostAsync(APIRoutes.CreateIndexRoute, requestPayload))
            {
                response.EnsureSuccessStatusCode();
                var responsePayload = await response.Content.ReadAsStringAsync();
                var settings = new JsonSerializerSettings();
                createIndexResponse = JsonConvert.DeserializeObject<CreateIndexResponse>(responsePayload, settings);
            }

            CreateIndexStatusResponse createIndexStatusResponse = null;
            do
            {
                var createIndexStatusModel = new CreateIndexStatusRequest
                {
                    JobId = createIndexResponse.IndexingJobId
                };

                using (var requestPayload = new StringContent(JsonConvert.SerializeObject(createIndexStatusModel), Encoding.UTF8, "application/json"))
                {
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        Content = requestPayload,
                        RequestUri = new Uri(client.BaseAddress, APIRoutes.CreateIndexStatusRoute)
                    };

                    using (var response = await client.SendAsync(request))
                    {
                        response.EnsureSuccessStatusCode();
                        var responsePayload = await response.Content.ReadAsStringAsync();
                        var settings = new JsonSerializerSettings();
                        createIndexStatusResponse = JsonConvert.DeserializeObject<CreateIndexStatusResponse>(responsePayload, settings);
                        Assert.That(createIndexStatusResponse, Is.Not.Null);
                        Assert.That(createIndexStatusResponse.Exists, Is.True);
                    }
                }
            } while (!createIndexStatusResponse.IndexingFinished);
            Assert.That(createIndexStatusResponse.IndexId, Is.Not.SameAs(-1));

            // Cleanup, but not really  needed for the tests to work ...            
            // if the metaPath is created between tests, the second test does not work for some reason ...
            var metaPath = WebTestHelper.GetPathToTestData("Meta");
            if (Directory.Exists(metaPath))
            {
                Directory.Delete(metaPath, true);
            }
        }
    }
}
