using AssemblyUpdater.Models;
using AssemblyUpdater.Properties;
using AssemblyUpdater.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml;

namespace AssemblyUpdater
{
    public class MainWindowPageViewModel : NotifyBase
    {
        public bool _isBusy;
        private ObservableCollection<FrameworkProjectItem> _projectsToUpdate;
        private ObservableCollection<AssemblyFileItem> _filesToUpdate;
        private string _lastUpdatedFolder;
        private string _lastReadVersion;
        private List<string> _versionsMismatch;

        private string _solutionFilename, _solutionPath;
        private string _displayedVersion, _toWriteVersion, _displayedFrameworkVersion, _toWriteFrameworkVersion;

        private double _currentlyUpdatedValues;

        public MainWindowPageViewModel()
        {
            SolutionPath = Settings.Default.LastUsedDirectory;

            _versionsMismatch = new List<string>();

            SetupCommand = new RelayCommand(x => !IsBusy, x => Setup());
            CmdUpdateSingleFile = new RelayCommand(x => !IsBusy, x => UpdateSingleFileWithNotification((AssemblyFileItem)x));

            CmdUpdateSingleFile = new RelayCommand(x => !IsBusy, x => UpdateSingleFileWithNotification((AssemblyFileItem)x));
            CmdUpdateVersion = new RelayCommand(x => !IsBusy, x => ExecuteUpdateVersion());
            CmdRefreshVersion = new RelayCommand(x => !IsBusy, x => ExecuteRefreshVersion());
            CmdUpdateFramework = new RelayCommand(x => !IsBusy, x => ExecuteUpdateFrameworkVersion());

            IsBusy = true;

            Task.Run(() => this.LoadDataFromFolder(SolutionPath, true)).Wait();

            IsBusy = false;
        }

        public ICommand SetupCommand { get; set; }
        public ICommand CmdUpdateFramework { get; set; }
        public ICommand CmdUpdateSingleFile { get; set; }
        public ICommand CmdUpdateVersion { get; set; }
        public ICommand CmdRefreshVersion { get; set; }

        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                _isBusy = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string SolutionPath
        {
            get
            {
                return _solutionPath;
            }
            set
            {
                _solutionPath = value;
                OnPropertyChanged();
            }
        }

        public string SolutionFilename
        {
            get
            {
                return _solutionFilename;
            }
            set
            {
                _solutionFilename = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<AssemblyFileItem> FilesToUpdateList
        {
            get
            {
                return _filesToUpdate;
            }
            set
            {
                _filesToUpdate = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<FrameworkProjectItem> ProjectsToUpdateList
        {
            get
            {
                return _projectsToUpdate;
            }
            set
            {
                _projectsToUpdate = value;
                OnPropertyChanged();
            }
        }

        public string DisplayedVersion
        {
            get
            {
                return _displayedVersion;
            }
            set
            {
                _displayedVersion = value;
                OnPropertyChanged();
            }
        }

        public string ToWriteVersion
        {
            get
            {
                return _toWriteVersion;
            }
            set
            {
                _toWriteVersion = value;
                OnPropertyChanged();
            }
        }

        public string DisplayedFrameworkVersion
        {
            get
            {
                return _displayedFrameworkVersion;
            }
            set
            {
                _displayedFrameworkVersion = value;
                OnPropertyChanged();
            }
        }

        public string ToWriteFrameworkVersion
        {
            get
            {
                return _toWriteFrameworkVersion;
            }
            set
            {
                _toWriteFrameworkVersion = value;
                OnPropertyChanged();
            }
        }

        public double CurrentlyUpdatedValues
        {
            get
            {
                return _currentlyUpdatedValues;
            }
            set
            {
                _currentlyUpdatedValues = value;
                OnPropertyChanged();
            }
        }

        private async Task<bool> LoadDataFromFolder(string newFolder, bool updateVersionField)
        {
            _versionsMismatch = new List<string>();

            _lastUpdatedFolder = newFolder;

            try
            {
                if (CheckForSlnFiles(newFolder))
                {
                    CurrentlyUpdatedValues = 0;

                    var applicationData = await GetApplicationData(newFolder);

                    FilesToUpdateList = new ObservableCollection<AssemblyFileItem>(applicationData.AssemblyFiles);
                    ProjectsToUpdateList = new ObservableCollection<FrameworkProjectItem>(applicationData.ProjectFiles);

                    if (_versionsMismatch.Count > 1)
                    {
                        string message = string.Format(Resources.VERSIONS_MISMATCH, String.Join(", ", _versionsMismatch));
                        System.Windows.Forms.MessageBox.Show(message, "Warning");
                    }

                    if (FilesToUpdateList.Any())
                    {
                        if (UpdateVersionFields(updateVersionField))
                        {
                            return true;
                        }
                    }
                    if (ProjectsToUpdateList.Any())
                    {
                        if (UpdateFrameworkVersionFields(updateVersionField))
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message, "Error");
                FilesToUpdateList = null;
                ProjectsToUpdateList = null;
                DisplayedVersion = null;
                ToWriteVersion = null;
                DisplayedFrameworkVersion = null;
                ToWriteFrameworkVersion = null;
            }

            return false;
        }

        private bool CheckForSlnFiles(string dirPath)
        {
            if (string.IsNullOrEmpty(dirPath))
            {
                return false;
            }

            string[] possibleSolutionFilenames = Directory.GetFiles(dirPath, Constants.SOLUTION_REGEX);

            if (possibleSolutionFilenames.Length == 0)
            {
                System.Windows.MessageBox.Show(
                    Resources.NO_SLN_FILES,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                FilesToUpdateList = null;
                DisplayedVersion = null;
                ProjectsToUpdateList = null;
                DisplayedFrameworkVersion = null;

                return false;
            }

            SolutionFilename = possibleSolutionFilenames[0].Substring(possibleSolutionFilenames[0].LastIndexOf('\\') + 1);
            return true;
        }

        /// <summary>
        /// Gets a list of relative path for the files to be updated.
        /// </summary>
        /// <param name="rootDirectory"></param>
        /// <returns></returns>
        private async Task<ApplicationData> GetApplicationData(string rootDirectory)
        {
            ApplicationData applicationData = new ApplicationData();
            List<AssemblyFileItem> assemblyList = new List<AssemblyFileItem>();
            List<FrameworkProjectItem> projectsList = new List<FrameworkProjectItem>();
            string assemblyVersion = "";
            string frameworkVersion = "";
            List<string> projectFileList = await Task.Run(() => LookForProjects(rootDirectory));
            foreach (string projectFile in projectFileList)
            {
                FrameworkProjectItem projectItem = FrameworkManager.ReadFramework(projectFile);
                frameworkVersion = projectItem.FrameworkVersion;
                projectsList.Add(projectItem);

                if (projectItem.FrameworkType == MicrosoftFrameworkType.NETCore || projectItem.FrameworkType == MicrosoftFrameworkType.NETStandard)
                {
                    AssemblyFileItem item = new AssemblyFileItem();

                    item.File = Path.GetFileNameWithoutExtension(projectFile);
                    item.Version = FrameworkManager.ReadVersionFromProjectFile(projectFile);
                    assemblyVersion = item.Version;
                    assemblyList.Add(item);
                }
                else  //NET Framework
                {
                    AssemblyFileItem item = FileManagement.ReadSingleAssemblyInfoFile(projectFile);

                    assemblyVersion = item.Version;
                    assemblyList.Add(item);
                }
            }

            _lastReadVersion = assemblyVersion;
            DisplayedVersion = assemblyVersion;
            DisplayedFrameworkVersion = frameworkVersion;

            applicationData.AssemblyFiles = assemblyList;
            applicationData.ProjectFiles = projectsList;
            return applicationData;
        }


        private List<string> LookForProjects(string rootDirectory)
        {
            string[] files = System.IO.Directory.GetFiles(rootDirectory, "*.csproj", SearchOption.AllDirectories);

            return files.ToList();
        }




        private string ReadVersion(string relativeFilePath)
        {
            if (!string.IsNullOrEmpty(relativeFilePath) && !string.IsNullOrEmpty(SolutionPath))
            {
                string absolutePath = Path.Combine(SolutionPath, relativeFilePath);
                AssemblyFileItem fileItem = FileManagement.ReadSingleAssemblyInfoFile(absolutePath);
                string assemblyVersion = fileItem.Version;
                string assemblyVersionFile = fileItem.File;

                if (assemblyVersion != assemblyVersionFile)
                {
                    string message = string.Format(Resources.VERSIONS_MISMATCH_FILE, absolutePath, assemblyVersion, assemblyVersionFile);
                    System.Windows.Forms.MessageBox.Show(message, "Warning");
                }

                if (!string.IsNullOrEmpty(assemblyVersion))
                {
                    if (_lastReadVersion != assemblyVersion
                        &&
                        !_versionsMismatch.Contains(assemblyVersion)
                        )
                    {
                        _versionsMismatch.Add(assemblyVersion);
                    }

                    if (DisplayedVersion == null)
                    {
                        DisplayedVersion = assemblyVersion;
                    }
                    return assemblyVersion;
                }
                else
                {
                    string message = String.Format(Resources.NO_ASSEMBLY_INFO, SolutionPath + relativeFilePath);
                    System.Windows.Forms.MessageBox.Show(message, "Error");
                    _versionsMismatch.Add(Constants.NO_ASSEMBLY_VERSION);
                    return Constants.NO_ASSEMBLY_VERSION;
                }
            }
            else
            {
                string message = string.Format(Resources.INCORRECT_PATH, SolutionPath + relativeFilePath);
                System.Windows.Forms.MessageBox.Show(message, "Error");
                return Constants.NO_ASSEMBLY_VERSION;
            }
        }


        private bool UpdateVersionFields(bool updateVersionField)
        {
            if (updateVersionField)
            {
                DisplayedVersion = FilesToUpdateList.LastOrDefault()?.Version;
                ToWriteVersion = FilesToUpdateList.FirstOrDefault()?.Version;
                return true;
            }
            return true;
        }

        private bool UpdateFrameworkVersionFields(bool updateVersionField)
        {
            if (updateVersionField)
            {
                DisplayedFrameworkVersion = ProjectsToUpdateList.LastOrDefault()?.FrameworkVersion;
                ToWriteFrameworkVersion = ProjectsToUpdateList.FirstOrDefault()?.FrameworkVersion;
                return true;
            }
            return true;
        }

        private async void ExecuteRefreshVersion()
        {
            if (!IsBusy)
            {
                IsBusy = true;

                await LoadDataFromFolder(SolutionPath, true);

                IsBusy = false;
            }
        }

        private async void ExecuteUpdateVersion()
        {
            IsBusy = true;
            CurrentlyUpdatedValues = 0;

            bool canUpdate = true;
            if (!SolutionPath.Equals(_lastUpdatedFolder) || string.IsNullOrEmpty(_lastReadVersion) || ToWriteVersion == null)
            {
                canUpdate = await LoadDataFromFolder(SolutionPath, false);
            }

            if (canUpdate)
            {
                var newVersion = string.Join(".", ToWriteVersion);

                if (!string.IsNullOrEmpty(_lastReadVersion))
                {
                    if (!newVersion.Equals(_lastReadVersion)
                        || _versionsMismatch.Count > 1)
                    {
                        foreach (var item in FilesToUpdateList)
                        {
                            var file = item.File;
                            await Task.Run(() =>
                            {
                                FileManagement.UpdateSingleVersion(item, newVersion, SolutionPath, _versionsMismatch, CurrentlyUpdatedValues);
                            });
                        }

                        _lastReadVersion = newVersion;
                        UpdateVersionFields(true);
                        System.Windows.Forms.MessageBox.Show(Resources.OPERATION_COMPLETED);
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show(Resources.ASSEMBLIES_ALREADY_UPDATED);
                    }
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show(Resources.CANNOT_READ_ASSEMBLY, "Error");
                }
            }

            IsBusy = false;
        }

        private async void ExecuteUpdateFrameworkVersion()
        {
            string frameworkVersionToWrite = "v" + ToWriteFrameworkVersion[0] + "." + ToWriteFrameworkVersion[1]
                + ToWriteFrameworkVersion[2] != null ? "." + ToWriteFrameworkVersion[2] : "";
            foreach (var item in ProjectsToUpdateList)
            {
                await Task.Run(() =>
                {
                    FrameworkManager.WriteFrameworkVersion(Path.Combine(SolutionPath, item.Project), frameworkVersionToWrite);
                    UpdateFrameworkVersionFields(true);
                    System.Windows.Forms.MessageBox.Show(Resources.OPERATION_COMPLETED);
                });
            }
        }

        public void UpdateSingleFileWithNotification(AssemblyFileItem item)
        {
            string newVersion = string.Join(".", ToWriteVersion);
            FileManagement.UpdateSingleVersion(item, newVersion, SolutionPath, _versionsMismatch, CurrentlyUpdatedValues);
            System.Windows.Forms.MessageBox.Show(Resources.OPERATION_COMPLETED);
        }

        private bool InitializeApp()
        {
            if (string.IsNullOrEmpty(SolutionPath))
            {
                MessageBox.Show("Setup all the configuration values before proceeding.");
                return false;
            }

            LoadDataFromFolder(SolutionPath, true);
            return true;
        }
        private void Setup(bool forceInitialization = false)
        {
            var dlg = new SetupDlg();
            var dlgRes = dlg.ShowDialog();
            SolutionPath = Settings.Default.LastUsedDirectory;
            if (dlgRes == true)
            {
                if (InitializeApp())
                {
                    MessageBox.Show("The settings have been updated.");
                }
                else
                {
                    Setup(true);
                }
            }
            else if (forceInitialization)
            {
                MessageBox.Show("You can't continue until all configuration values are set up.");
                Setup(true);
            }
        }

    }
}