namespace AssemblyUpdater.Models
{
    public class FrameworkProjectItem : NotifyBase
    {
        private string _project;
        private string _framework;

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

        public string Framework
        {
            get
            {
                return _framework;
            }
            set
            {
                _framework = value;
                OnPropertyChanged();
            }
        }

    }
}
