using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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
    }
}
