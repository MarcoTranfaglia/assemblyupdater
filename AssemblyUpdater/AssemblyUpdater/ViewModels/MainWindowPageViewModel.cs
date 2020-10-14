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
        private string[] _displayedVersion, _toWriteVersion, _displayedFrameworkVersion, _toWriteFrameworkVersion;

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

        public string[] DisplayedVersion
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

        public string[] ToWriteVersion
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

        public string[] DisplayedFrameworkVersion
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

        public string[] ToWriteFrameworkVersion
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

                    FilesToUpdateList = new ObservableCollection<AssemblyFileItem>(await GetFilesToUpdate(newFolder));
                    ProjectsToUpdateList = new ObservableCollection<FrameworkProjectItem>(await GetProjectsToUpdate(newFolder));

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
        private async Task<List<AssemblyFileItem>> GetFilesToUpdate(string rootDirectory)
        {
            var fileList = await Task.Run(() => LookForAssemblies(rootDirectory));
            List<AssemblyFileItem> resList = null;

            resList = new List<AssemblyFileItem>();
            string assemblyVersion = "";
            foreach (var file in fileList)
            {
                AssemblyFileItem item = new AssemblyFileItem();

                item.File = file.Replace(rootDirectory, "");
                item.Version = ReadVersion(item.File);
                assemblyVersion = item.Version;
                resList.Add(item);
            }

            _lastReadVersion = assemblyVersion;
            DisplayedVersion = assemblyVersion.Split('.');
            return resList;
        }

        /// <summary>
        /// Gets a list of relative path for the files to be updated.
        /// </summary>
        /// <param name="rootDirectory"></param>
        /// <returns></returns>
        private async Task<List<FrameworkProjectItem>> GetProjectsToUpdate(string rootDirectory)
        {
            var fileList = await Task.Run(() => LookForProjects(rootDirectory));
            List<FrameworkProjectItem> resList = null;

            resList = new List<FrameworkProjectItem>();
            string frameworkVersion = "";
            foreach (var file in fileList)
            {
                FrameworkProjectItem item = new FrameworkProjectItem();

                item.Project = file.Replace(rootDirectory, "");
                item.Framework = ReadFrameworkVersion(item.Project);
                frameworkVersion = item.Framework;
                resList.Add(item);
            }

            DisplayedFrameworkVersion = frameworkVersion.Split('.');
            return resList;
        }

        private List<string> LookForProjects(string rootDirectory)
        {
            List<string> filesToUpdate = new List<string>();

            var currFiles = Directory.GetFiles(rootDirectory);
            var fileToUpdate = currFiles.FirstOrDefault(x => Constants.PROJECT_REGEX.Any(x.Contains));
            var myRegex = new Regex(Constants.PROJECT_REGEX);
            var resultList = currFiles.Where(x => myRegex.IsMatch(x)).ToList();
            fileToUpdate = resultList.FirstOrDefault();

            if (fileToUpdate == null)
            {
                var currDirectories = Directory.GetDirectories(rootDirectory);
                Parallel.ForEach(currDirectories, x => filesToUpdate.AddRange(LookForProjects(x)));
            }
            else
            {
                filesToUpdate.Add(fileToUpdate);
            }

            return filesToUpdate;
        }

        private List<string> LookForAssemblies(string rootDirectory)
        {
            List<string> filesToUpdate = new List<string>();

            var currFiles = Directory.GetFiles(rootDirectory);
            var fileToUpdate = currFiles.FirstOrDefault(x => Constants.FILE_NAMES_CHECK.Any(x.Contains));

            if (fileToUpdate == null)
            {
                var currDirectories = Directory.GetDirectories(rootDirectory);
                Parallel.ForEach(currDirectories, x => filesToUpdate.AddRange(LookForAssemblies(x)));
            }
            else
            {
                filesToUpdate.Add(fileToUpdate);
            }

            return filesToUpdate;
        }

        private string ReadFrameworkVersion(string relativeFilePath)
        {
            if (!string.IsNullOrEmpty(relativeFilePath) && !string.IsNullOrEmpty(SolutionPath))
            {
                string frameworkVersion = string.Empty;
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(File.ReadAllText(SolutionPath + relativeFilePath));
                XmlNamespaceManager mgr = new XmlNamespaceManager(xmlDoc.NameTable);
                mgr.AddNamespace("x", "http://schemas.microsoft.com/developer/msbuild/2003");

                foreach (XmlNode item in xmlDoc.SelectNodes("//x:TargetFrameworkVersion", mgr))
                {
                    frameworkVersion = item.InnerText.ToString();
                }


                if (DisplayedFrameworkVersion == null)
                {
                    DisplayedFrameworkVersion = frameworkVersion.Split(".");
                }

                return frameworkVersion;
            }

            return "0.0";
        }

        private void WriteFrameworkVersion(string relativeFilePath, string newFrameworkVersion)
        {
            if (!string.IsNullOrEmpty(relativeFilePath) && !string.IsNullOrEmpty(SolutionPath))
            {
                string frameworkVersion = string.Empty;
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(File.ReadAllText(SolutionPath + relativeFilePath));
                XmlNamespaceManager mgr = new XmlNamespaceManager(xmlDoc.NameTable);
                mgr.AddNamespace("x", "http://schemas.microsoft.com/developer/msbuild/2003");

                foreach (XmlNode item in xmlDoc.SelectNodes("//x:TargetFrameworkVersion", mgr))
                {
                    item.InnerText = newFrameworkVersion;
                }

                xmlDoc.Save(SolutionPath + relativeFilePath);
            }

        }

        private string ReadVersion(string relativeFilePath)
        {
            if (!string.IsNullOrEmpty(relativeFilePath) && !string.IsNullOrEmpty(SolutionPath))
            {
                string[] readText = File.ReadAllLines(SolutionPath + relativeFilePath);
                string assemblyVersion = string.Empty;
                string assemblyVersionFile = string.Empty;

                //C#
                var versionInfoLines = readText.Where(t => t.Contains("[assembly: AssemblyVersion") && !t.StartsWith("//"));

                //C++
                if (!versionInfoLines.Any())
                {
                    versionInfoLines = readText.Where(t => t.Contains("[assembly:AssemblyVersionAttribute") && !t.StartsWith("//"));
                }

                foreach (string item in versionInfoLines)
                {
                    assemblyVersion = item.Substring(item.IndexOf('(') + 2, item.LastIndexOf(')') - item.IndexOf('(') - 3);
                }

                //C#
                var versionFileLines = readText.Where(t => t.Contains("[assembly: AssemblyFileVersion") && !t.StartsWith("//"));

                //C++
                if (!versionFileLines.Any())
                {
                    versionFileLines = readText.Where(t => t.Contains("[assembly:AssemblyFileVersionAttribute") && !t.StartsWith("//"));
                }

                foreach (string item in versionFileLines)
                {
                    assemblyVersionFile = item.Substring(item.IndexOf('(') + 2, item.LastIndexOf(')') - item.IndexOf('(') - 3);
                }

                if (assemblyVersion != assemblyVersionFile)
                {
                    string message = string.Format(Resources.VERSIONS_MISMATCH_FILE, SolutionPath + relativeFilePath, assemblyVersion, assemblyVersionFile);
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
                        DisplayedVersion = assemblyVersion.Split(".");
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
                DisplayedVersion = FilesToUpdateList.LastOrDefault()?.Version.Split('.');
                ToWriteVersion = FilesToUpdateList.FirstOrDefault()?.Version.Split('.'); 
                return true;
            }
            return true;
        }

        private bool UpdateFrameworkVersionFields(bool updateVersionField)
        {
            if (updateVersionField)
            {
                DisplayedFrameworkVersion = ProjectsToUpdateList.LastOrDefault()?.Framework.Split('.');
                ToWriteFrameworkVersion = ProjectsToUpdateList.FirstOrDefault()?.Framework.Split('.'); 
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
                                FileManagement.UpdateSingleFile(item, newVersion, SolutionPath, _versionsMismatch, CurrentlyUpdatedValues);
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
                + ToWriteFrameworkVersion[2] != null ? "."+ToWriteFrameworkVersion[2] : "";
            foreach (var item in ProjectsToUpdateList)
            {
                await Task.Run(() =>
                {
                    WriteFrameworkVersion(item.Project, frameworkVersionToWrite);
                    UpdateFrameworkVersionFields(true);
                    System.Windows.Forms.MessageBox.Show(Resources.OPERATION_COMPLETED);
                });
            }
        }

        public void UpdateSingleFileWithNotification(AssemblyFileItem item)
        {
            string newVersion = string.Join(".", ToWriteVersion);
            FileManagement.UpdateSingleFile(item, newVersion, SolutionPath, _versionsMismatch, CurrentlyUpdatedValues);
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