using System.IO;

namespace AssemblyUpdater.Models
{
    public class FrameworkProjectItem : NotifyBase
    {
        private string _projectFullPath;
        private string _frameworkVersion;
        private string _assemblyVersion;
        private string _assemblyFile;
        private MicrosoftFrameworkType _frameworkType;


        public string ProjectFullPath
        {
            get
            {
                return _projectFullPath;
            }
            set
            {
                _projectFullPath = value;
                OnPropertyChanged();
            }
        }

        public string Project
        {
            get
            {
                return Path.GetFileNameWithoutExtension(_projectFullPath);
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

        public string AssemblyVersion
        {
            get
            {
                return _assemblyVersion;
            }
            set
            {
                _assemblyVersion = value;
                OnPropertyChanged();
            }
        }

        public string AssemblyFile
        {
            get
            {
                return _assemblyFile;
            }
            set
            {
                _assemblyFile = value;
                OnPropertyChanged();
            }
        }

    }
}
