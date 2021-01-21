namespace AssemblyUpdater.Models
{
    public class FrameworkProjectItem : NotifyBase
    {
        private string _project;
        private string _frameworkVersion;
        private MicrosoftFrameworkType _frameworkType;

        public string Project
        {
            get
            {
                return _project;
            }
            set
            {
                _project = value;
                OnPropertyChanged();
            }
        }

        public string FrameworkVersion
        {
            get
            {
                return _frameworkVersion;
            }
            set
            {
                _frameworkVersion = value;
                OnPropertyChanged();
            }
        }

        public MicrosoftFrameworkType FrameworkType
        {
            get
            {
                return _frameworkType;
            }
            set
            {
                _frameworkType = value;
                OnPropertyChanged();
            }
        }

    }
}
