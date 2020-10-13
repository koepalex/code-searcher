using CodeSearcher.BusinessLogic;
using CodeSearcher.Interfaces;
using CodeSearcher.Interfaces.API.Model.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
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
using ICSharpCode.AvalonEdit.Document;
using Path = System.IO.Path;

namespace CodeSearcher.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly AppViewModel _viewModel;

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

            _viewModel = new AppViewModel();
            DataContext = _viewModel;

            DiagConsole.ProcessInterface.StartProcess("CodeSearcher.WebAPI.exe", string.Empty);
        }

        private async void OnMainWindowLoaded(object sender, RoutedEventArgs e)
        {
            while (!DiagConsole.ProcessInterface.IsProcessRunning)
            {
                await Task.Delay(250);
            }

            _viewModel.StatusMessage = "Loading existing indexes from server";
            var indexes = await _viewModel.LoadIndexesAsync();

            foreach (var index in indexes)
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

            if (IndexTreeView.Items.Count > 0)
            {
                ((TreeViewItem) IndexTreeView.Items[0]).IsSelected = true;
                _viewModel.StatusMessage = "Loading Indexes finished";
            }
            else
            {
                _viewModel.StatusMessage = "Not Indexes available";
            }

            SearchTextBox.Focus();
        }

        private async void OnMainWindowClosing(object sender, CancelEventArgs e)
        {
            DiagConsole.ProcessInterface.StopProcess();
        }

        private void IndexTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _viewModel.SelectedIndex = (IndexTreeView.SelectedItem as TreeViewItem)?.Tag as ICodeSearcherIndex;

            DeleteIndexButton.IsEnabled = _viewModel.SelectedIndex != null;
        }

        private void FindingTreeViewSelectionItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var file = ((FindingTreeView.SelectedItem as TreeViewItem)?.Parent as TreeViewItem)?.Tag as IDetailedSearchResult;
            var finding = (FindingTreeView.SelectedItem as TreeViewItem)?.Tag as IFindingInFile;
            if (file != null && finding != null && File.Exists(file.Filename))
            {
                TextEditor.Document = new TextDocument(new StringTextSource(File.ReadAllText(file.Filename)));
                double vertOffset = (TextEditor.TextArea.TextView.DefaultLineHeight) * finding.LineNumber;
                TextEditor.ScrollToVerticalOffset(vertOffset);
            }
            else
            {
                _viewModel.StatusMessage = $"Can't open file: {file?.Filename}, recreating the index should solve this";
            }
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

                    var results = await _viewModel.LoadSearchResultAsync(searchPattern);
                    foreach (var findingResults in results)
                    {
                        var item = new TreeViewItem
                        {
                            Header = Path.GetFileName(findingResults.Filename),
                            Tag = findingResults,
                            Name = Path.GetFileNameWithoutExtension(findingResults.Filename),
                            IsEnabled = true,
                        };

                        foreach (var lineFinding in findingResults.Findings)
                        {
                            var subItem = new TreeViewItem
                            {
                                Header = $"LineNumber: {lineFinding.LineNumber} Position: {lineFinding.Position} Length: {lineFinding.Length}",
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
