using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace CodeSearcher.App.ViewModels
{
    class AddIndexViewModel : INotifyPropertyChanged
    {
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

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
