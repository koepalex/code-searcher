using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CodeSearcher.BusinessLogic;
using CodeSearcher.Interfaces;
using CodeSearcher.Interfaces.API.Model.Requests;
using CodeSearcher.Interfaces.API.Model.Response;
using CodeSearcher.Interfaces.Constants;
using Newtonsoft.Json;

namespace CodeSearcher.App
{
    public class AppViewModel : INotifyPropertyChanged
    {
        private const string _baseAddress = @"http://localhost:5000";

        private string _statusMessage;
        public string StatusMessage
        {
            get
            {
                return _statusMessage;
            }
            set
            {
                _statusMessage = value;
                NotifyPropertChanged(nameof(StatusMessage));
            }
        }

        private ICodeSearcherIndex _selectedIndex;

        public ICodeSearcherIndex SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }
            set
            {
                _selectedIndex = value;
                NotifyPropertChanged(nameof(SelectedIndex));
            }
        }

        public ObservableCollection<ICodeSearcherIndex> Indexes { get; set; }

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        public AppViewModel()
        {
            Indexes = new ObservableCollection<ICodeSearcherIndex>();
        }

        public void NotifyPropertChanged(string propertyName)
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
                    searchResponse = JsonConvert.DeserializeObject<SearchIndexResponse>(responsePayload, settings);
                }
            }

            return searchResponse?.Results ?? Enumerable.Empty<IDetailedSearchResult>();

        }
    }
}
