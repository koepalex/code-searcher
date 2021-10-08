using CodeSearcher.Interfaces;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CodeSearcher.App.ViewModels;
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
        private readonly Timer _timer;
        private const int _updateInterval = 2500;

        public MainWindow()
        {
            InitializeComponent();

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                DiagConsole?.ProcessInterface?.StopProcess();
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

            DiagConsole.ProcessInterface.StartProcess("CodeSearcher.WebAPI.exe", string.Empty);//"http://0.0.0.0:44444");
            //_timer = new Timer(_updateInterval);

            //_timer.Elapsed += async (sender, args) =>
            //{
            //    InitializeIndexView().Wait();
            //};
        }

        private async void OnMainWindowLoaded(object sender, RoutedEventArgs e)
        {
            await InitializeIndexView();
            //_timer.Start();
        }

        private async Task InitializeIndexView()
        {
            while (!DiagConsole.ProcessInterface.IsProcessRunning)
            {
                await Task.Delay(250);
            }

            _viewModel.StatusMessage = "Loading existing indexes from server";
            var indexes = await _viewModel.LoadIndexesAsync();

            IndexTreeView.Items.Clear();
            foreach (var index in indexes)
            {
                var header = GetHeaderNameFromIndex(index);

                var item = new TreeViewItem
                {
                    Header = header,
                    Tag = index,
                    Name = $"Item_{Math.Abs(index.ID).ToString()}",
                    IsEnabled = true
                };
                IndexTreeView.Items.Add(item);
            }

            if (IndexTreeView.Items.Count > 0)
            {
                ((TreeViewItem)IndexTreeView.Items[0]).IsSelected = true;
                _viewModel.StatusMessage = "Loading Indexes finished";
            }
            else
            {
                _viewModel.StatusMessage = "Not Indexes available";
            }

            SearchTextBox.Focus();
        }

        private string GetHeaderNameFromIndex(ICodeSearcherIndex index)
        {
            var sb = new StringBuilder();
            sb.Append(index.SourcePath?.Split("\\")?.LastOrDefault() ?? "error processing name");
            sb.Append(" {");
            for (var i = 0; i < index.FileExtensions.Count; i++)
            {
                sb.Append(index.FileExtensions[i]);
                if (i != (index.FileExtensions.Count - 1))
                {
                    sb.Append(",");
                }
            }
            sb.Append("}");
            return sb.ToString();
        }

        private void OnMainWindowClosing(object sender, CancelEventArgs e)
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

                TextEditor.ScrollToLine(finding.LineNumber);
                var documentLine = TextEditor.Document.GetLineByNumber(finding.LineNumber);
                TextEditor.Select(documentLine.Offset + finding.Position, finding.Length);
            }
            else
            {
                _viewModel.StatusMessage = $"Can't open file: {file?.Filename}, recreating the index should solve this";
            }
        }

        private async void NewIndexButtonClick(object sender, RoutedEventArgs e)
        {
            var window = new AddIndexWindow();
            window.Owner = this;
            window.ShowInTaskbar = false;
            var dialogResult = window.ShowDialog();
            if (dialogResult.GetValueOrDefault(false))
            {
                _viewModel.StatusMessage = $"Index with id created";
                await InitializeIndexView();
            }
        }

        private async void DeleteIndexButtonClick(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Do you want to delete the existing Index?", "Confirmation", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                await _viewModel.DeleteIndexAsync();
                await Task.Delay(500);
                await InitializeIndexView();
            }
        }

        private async void SearchTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var searchPattern = (sender as TextBox)?.Text;
                if (!string.IsNullOrWhiteSpace(searchPattern))
                {

                    var results = await _viewModel.LoadSearchResultAsync(searchPattern);

                    FindingTreeView.Items.Clear();

                    foreach (var findingResults in results.Where(r => r.Findings.Any()))
                    {
                        var item = new TreeViewItem
                        {
                            Header = Path.GetFileName(findingResults.Filename),
                            Tag = findingResults,
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
