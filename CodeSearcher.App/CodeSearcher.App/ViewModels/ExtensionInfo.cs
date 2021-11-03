namespace CodeSearcher.App.ViewModels
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ExtensionInfo : INotifyPropertyChanged
    {
        private string _extension;
        public string Extension
        {
            get => _extension;
            set
            {
                _extension = value;
                NotifyPropertyChanged();
            }
        }

        private bool _show;
        public bool Show
        {
            get => _show;
            set
            {
                _show = value;
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
