using CodeSearcher.BusinessLogic;
using CodeSearcher.Interfaces;
using CodeSearcher.Interfaces.API.Model.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

            foreach (var index in getIndexes.Indexes)
            {
                var item = new TreeViewItem();
                item.Header = index.SourcePath;
                item.Tag = index;
                item.Name = $"Item_{index.ID.ToString()}";
                item.IsEnabled = true;
                IndexTreeView.Items.Add(item);
            }

            ((TreeViewItem)IndexTreeView.Items[0]).IsSelected = true;
            //IndexTreeView.InvalidateVisual();

            SearchTextBox.Focus();
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

        private void SearchTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var searchPattern = (sender as TextBox)?.Text;
                if (string.IsNullOrWhiteSpace(searchPattern))
                {
                    // Call search
                }
            }
        }
    }
}
