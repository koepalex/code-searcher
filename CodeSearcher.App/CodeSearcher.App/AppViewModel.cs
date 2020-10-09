using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using CodeSearcher.Interfaces;

namespace CodeSearcher.App
{
    public class AppViewModel : INotifyPropertyChanged
    {
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
    }
}
