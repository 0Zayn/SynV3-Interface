using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;

namespace SynV3
{
    public class CollapsibleItem : INotifyPropertyChanged
    {
        public string? Header { get; set; }
        public Geometry? IconData { get; set; }
        public ObservableCollection<SubItem> SubItems { get; } = new ObservableCollection<SubItem>();

        private bool _IsExpanded;
        public bool IsExpanded
        {
            get => _IsExpanded;
            set
            {
                if (_IsExpanded != value)
                {
                    _IsExpanded = value;
                    OnPropertyChanged(nameof(IsExpanded));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string Property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Property));
        }
    }

    public class SubItem
    {
        public string? Name { get; set; }
        public string? Script { get; set; }
    }
}
