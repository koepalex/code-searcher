using System;
using System.Windows;
using System.Windows.Forms;
using CodeSearcher.App.ViewModels;

namespace CodeSearcher.App
{
    /// <summary>
    /// Interaction logic for AddIndexWindow.xaml
    /// </summary>
    public partial class AddIndexWindow : Window
    {
        private AddIndexViewModel _viewModel;
        public AddIndexWindow()
        {
            InitializeComponent();
            _viewModel = new AddIndexViewModel();
            DataContext = _viewModel;
        }

        private void OnCancelButtonClick(object sender, EventArgs args)
        {
            Close();
        }

        private void OnBrowseButtonClick(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.UseDescriptionForTitle = true;
                dialog.Description = "Browse Folder to be indexed";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    _viewModel.SourcePath = dialog.SelectedPath;
                }
            }
        }

        private async void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            if(_viewModel.Validate())
            {
                System.Windows.MessageBox.Show("Please check input, directory to index need to exist and files extension can't be empty");
                return;
            }

            await _viewModel.LoadSearchResultAsync();
            DialogResult = true;
            Close();
        }
    }
}
