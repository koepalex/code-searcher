using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CodeSearcher.BusinessLogic;
using CodeSearcher.Interfaces;
using CodeSearcher.Interfaces.API.Model.Requests;
using CodeSearcher.Interfaces.API.Model.Response;
using CodeSearcher.Interfaces.Constants;
using Newtonsoft.Json;

namespace CodeSearcher.App.ViewModels
{
    public class AppViewModel : INotifyPropertyChanged
    {
        private const string _baseAddress = @"http://localhost:5000";

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                NotifyPropertyChanged();
            }
        }

        private ICodeSearcherIndex _selectedIndex;

        public ICodeSearcherIndex SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<ICodeSearcherIndex> Indexes { get; set; }

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        public AppViewModel()
        {
            Indexes = new ObservableCollection<ICodeSearcherIndex>();
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task<IEnumerable<ICodeSearcherIndex>> LoadIndexesAsync()
        {
            using var client = new HttpClient();
            client.BaseAddress = new Uri(_baseAddress);

            var response = await client.GetAsync(APIRoutes.CodeSearcherRoute);

            var responsePayload = await response.Content.ReadAsStringAsync();

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(Factory.Get().GetCodeSearcherIndexJsonConverter());
            var getIndexes = JsonConvert.DeserializeObject<GetIndexesResponse>(responsePayload, settings);

            return getIndexes?.Indexes ?? Enumerable.Empty<ICodeSearcherIndex>();
        }

        public async Task<IEnumerable<IDetailedSearchResult>> LoadSearchResultAsync(string searchPattern)
        {
            if (SelectedIndex == null)
            {
                return Enumerable.Empty<IDetailedSearchResult>();
            }

            SearchIndexResponse searchResponse;
            
            using var client = new HttpClient();
            client.BaseAddress = new Uri(_baseAddress);

            var searchRequest = new SearchIndexRequest
            {
                IndexID = SelectedIndex.ID,
                SearchWord = searchPattern
            };

            using (var requestPayload = new StringContent(JsonConvert.SerializeObject(searchRequest), Encoding.UTF8, "application/json"))
            {
                using var request = new HttpRequestMessage
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
                    searchResponse = JsonConvert.DeserializeObject<SearchIndexResponse>(responsePayload, settings);
                }
            }

            return searchResponse?.Results ?? Enumerable.Empty<IDetailedSearchResult>();

        }

        public async Task DeleteIndexAsync()
        {
            if (SelectedIndex == null)
            {
                return;
            }

            using var client = new HttpClient();
            client.BaseAddress = new Uri(_baseAddress);

            var deleteRequest = new DeleteIndexRequest
            {
                IndexID = SelectedIndex.ID
            };
            using (var requestPayload = new StringContent(JsonConvert.SerializeObject(deleteRequest), Encoding.UTF8, "application/json"))
            {
                using var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Delete,
                    Content = requestPayload,
                    RequestUri = new Uri(client.BaseAddress, APIRoutes.CreateIndexRoute)
                };
                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                }
            }
        }
    }
}
