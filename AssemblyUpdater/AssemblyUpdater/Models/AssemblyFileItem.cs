namespace AssemblyUpdater.Models
{
    public class AssemblyFileItem : NotifyBase
    {
        private string _file;
        private string _version;

        public string File
        {
            get
            {
                return _file;
            }
            set
            {
                _file = value;
                OnPropertyChanged();
            }
        }

        public string Version
        {
            get
            {
                return _version;
            }
            set
            {
                _version = value;
                OnPropertyChanged();
            }
        }

    }
}
