using CodeSearcher.Interfaces.API.Model.Requests;
using CodeSearcher.Interfaces.API.Model.Response;
using CodeSearcher.Interfaces.Constants;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CodeSearcher.App.ViewModels
{
    class AddIndexViewModel : INotifyPropertyChanged
    {
        private const string _baseAddress = @"http://localhost:5000";

        public AddIndexViewModel()
        {
            Extensions = ".cs;.json;.xml";
        }
        private string _sourcePath;
        public string SourcePath
        {
            get => _sourcePath;
            set
            {
                _sourcePath = value;
                NotifyPropertyChanged();
            }
        }

        private string _extensions;
        public string Extensions
        {
            get => _extensions;
            set
            {
                _extensions = value;
                NotifyPropertyChanged();
            }
        }

        private string _jobId;
        public string JobId
        {
            get => _jobId;
            set
            {
                _jobId = value;
                NotifyPropertyChanged();
            }
        }

        public bool Validate()
        {
            var valid = true;
            valid &= !string.IsNullOrWhiteSpace(SourcePath);
            valid &= File.Exists(SourcePath);
            valid &= !string.IsNullOrWhiteSpace(Extensions);
            valid &= Regex.IsMatch(Extensions, @"(\.[a-zA-Z0-9]+)+");
            return valid;
        }

        public async Task LoadSearchResultAsync()
        {
            CreateIndexResponse createIndexResponse;

            using var client = new HttpClient();
            client.BaseAddress = new Uri(_baseAddress);

            var searchRequest = new CreateIndexRequest
            {
                SourcePath = SourcePath,
                FileExtensions = Extensions.Split(";")
            };

            using (var requestPayload = new StringContent(JsonConvert.SerializeObject(searchRequest), Encoding.UTF8, "application/json"))
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    Content = requestPayload,
                    RequestUri = new Uri(client.BaseAddress, APIRoutes.CreateIndexRoute)
                };
                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();

                    var responsePayload = await response.Content.ReadAsStringAsync();
                    createIndexResponse = JsonConvert.DeserializeObject<CreateIndexResponse>(responsePayload);
                    JobId = createIndexResponse.IndexingJobId;
                }
            }

            //wait until index has been created
            while (true)
            {
                await Task.Delay(250);

                var jobCompletedRequest = new CreateIndexStatusRequest
                {
                    JobId = JobId
                };
                using (var requestPayload = new StringContent(JsonConvert.SerializeObject(jobCompletedRequest), Encoding.UTF8, "application/json"))
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
                        var jobCompletedResponse = JsonConvert.DeserializeObject<CreateIndexStatusResponse>(responsePayload);
                        if (jobCompletedResponse.IndexingFinished)
                        {
                            break;
                        }
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
