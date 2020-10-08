using CodeSearcher.BusinessLogic;
using CodeSearcher.Interfaces;
using CodeSearcher.Interfaces.API.Model.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CodeSearcher.Interfaces.API.Model.Requests;
using CodeSearcher.Interfaces.Constants;
using Path = System.IO.Path;

namespace CodeSearcher.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ICodeSearcherIndex _selectedIndex;

        public MainWindow()
        {
            InitializeComponent();

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                var sb = new StringBuilder();
                var ex = (Exception) args.ExceptionObject;
                while (ex != null)
                {
                    sb.AppendLine(ex.Message);
                    sb.AppendLine();
                    ex = ex.InnerException;
                }

                MessageBox.Show(sb.ToString());
                Environment.FailFast("unhandled exception");
            };
        }

        private async void OnMainWindowLoaded(object sender, RoutedEventArgs e)
        {
            using var client = new HttpClient();
            client.BaseAddress = new Uri(@"http://localhost:5000");
         
            var response = await client.GetAsync("/api/CodeSearcher");

            var responsePayload = await response.Content.ReadAsStringAsync();

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(Factory.Get().GetCodeSearcherIndexJsonConverter());
            var getIndexes = JsonConvert.DeserializeObject<GetIndexesResponse>(responsePayload, settings);

            if (getIndexes != null)
            {
                foreach (var index in getIndexes.Indexes)
                {
                    var item = new TreeViewItem
                    {
                        Header = index.SourcePath,
                        Tag = index,
                        Name = $"Item_{Math.Abs(index.ID).ToString()}",
                        IsEnabled = true
                    };
                    IndexTreeView.Items.Add(item);
                }

                ((TreeViewItem) IndexTreeView.Items[0]).IsSelected = true;

                SearchTextBox.Focus();
            }
        }

        private void IndexTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _selectedIndex = (IndexTreeView.SelectedItem as TreeViewItem)?.Tag as ICodeSearcherIndex;

            DeleteIndexButton.IsEnabled = _selectedIndex != null;
        }

        private void NewIndexButtonClick(object sender, RoutedEventArgs e)
        {
            // Show Create New Index Dialog
        }

        private void DeleteIndexButtonClick(object sender, RoutedEventArgs e)
        {
            // Show Confirmation Dialog
        }

        private async void SearchTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var searchPattern = (sender as TextBox)?.Text;
                if (!string.IsNullOrWhiteSpace(searchPattern))
                {
                    SearchIndexResponse searchResponse;
                    // Call search
                    using var client = new HttpClient();
                    client.BaseAddress = new Uri(@"http://localhost:5000");

                    var searchRequest = new SearchIndexRequest
                    {
                        IndexID = _selectedIndex.ID,
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

                    if (searchResponse != null)
                    {
                        foreach (var findingResults in searchResponse.Results)
                        {
                            var item = new TreeViewItem
                            {
                                Header = findingResults.Filename,
                                Tag = findingResults,
                                Name = Path.GetFileNameWithoutExtension(findingResults.Filename),
                                IsEnabled = true,
                            };

                            foreach (var lineFinding in findingResults.Findings)
                            {
                                var subItem = new TreeViewItem
                                {
                                    Header = findingResults.Filename,
                                    Tag = lineFinding,
                                    Name = $"Line{lineFinding.LineNumber}Pos{lineFinding.Position}Len{lineFinding.Length}",
                                    IsEnabled = true,
                                };
                                item.Items.Add(subItem);
                            }
                            FindingTreeView.Items.Add(item);
                        }
                    }
                }
            }
        }
    }
}
