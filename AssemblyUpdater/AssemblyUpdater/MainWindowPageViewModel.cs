using AssemblyUpdater.Models;
using AssemblyUpdater.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace AssemblyUpdater
{
    public class MainWindowPageViewModel : NotifyBase
    {
        public bool _isBusy;
        private ObservableCollection<AssemblyFileItem> _filesToUpdate;
        private string _lastUpdatedFolder;
        private string _lastReadVersion;
        private string _solutionPath;
        private string _solutionFilename;
        private string[] _displayedVersion;
        private double _currentlyUpdatedValues;



        public MainWindowPageViewModel()
        {
            SolutionPath = Settings.Default.LastUsedDirectory;

            CmdSelectFolder = new RelayCommand(x => !IsBusy, x => ExecuteSelectFolder());
            CmdUpdateVersion = new RelayCommand(x => !IsBusy, x => ExecuteUpdateVersion());
            CmdRefreshVersion = new RelayCommand(x => !IsBusy, x => ExecuteRefreshVersion());

            IsBusy = true;

            LoadDataFromFolder(SolutionPath, true);

            IsBusy = false;
        }

        public ICommand CmdSelectFolder { get; set; }
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
            await Task.Delay(50);

            _lastUpdatedFolder = newFolder;

            try
            {
                if (CheckForSlnFiles(newFolder))
                {
                    CurrentlyUpdatedValues = 0;
                    FilesToUpdateList = new ObservableCollection<AssemblyFileItem>(await GetFilesToUpdate(newFolder));

                    if (FilesToUpdateList.Any())
                    {
                        if (UpdateVersion(updateVersionField))
                        {
                            Settings.Default.LastUsedDirectory = newFolder;
                            Settings.Default.Save();

                            return true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message, "Error");
                FilesToUpdateList = null;
                DisplayedVersion = null;
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
                    "The selected directory does not contain .sln files",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                FilesToUpdateList = null;
                DisplayedVersion = null;

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

            foreach (var file in fileList)
            {
                AssemblyFileItem item = new AssemblyFileItem();

                item.File = file.Replace(rootDirectory, "");
                item.Version = ReadVersion(item.File);
                resList.Add(item);

            }

            return resList;
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

        private string ReadVersion(string relativeFilePath)
        {
            if (!string.IsNullOrEmpty(relativeFilePath) && !string.IsNullOrEmpty(SolutionPath))
            {
                var content = File.ReadAllText(SolutionPath + relativeFilePath);
                if (!string.IsNullOrEmpty(content))
                {
                    //TODO TRY DIFFERENT FORMAT OF VERSION REGEX
                    Regex versionRegex = new Regex(Constants.VERSION_REGEX);

                    var match = versionRegex.Match(content);
                    if (match.Success)
                    {
                        _lastReadVersion = match.Value;

                        DisplayedVersion = match.Value.Split('.');

                        string readableVersion = DisplayedVersion[0] + "." + DisplayedVersion[1];
                        if (DisplayedVersion.Length > 2)
                        {
                            readableVersion += "." + DisplayedVersion[2];
                        }
                        if (DisplayedVersion.Length > 3)
                        {
                            readableVersion += "." + DisplayedVersion[3];
                        }
                        return readableVersion;
                    }
                    else
                    {
                        return "";
                    }

                }
                else
                {
                    System.Windows.Forms.MessageBox.Show($"Empty Content of file: {relativeFilePath}", "Error");
                    return "";
                }
            }
            else
            {
                System.Windows.Forms.MessageBox.Show($"Path non correct  {relativeFilePath}", "Error");
                return "";
            }
        }


        private bool UpdateVersion(bool updateVersionField)
        {
            if (updateVersionField)
            {
                DisplayedVersion = FilesToUpdateList.LastOrDefault()?.Version.Split('.');
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
            if (!SolutionPath.Equals(_lastUpdatedFolder) || string.IsNullOrEmpty(_lastReadVersion) || DisplayedVersion == null)
            {
                canUpdate = await LoadDataFromFolder(SolutionPath, false);
            }

            if (canUpdate)
            {
                var newVersion = string.Join(".", DisplayedVersion);

                if (!string.IsNullOrEmpty(_lastReadVersion))
                {
                    if (!newVersion.Equals(_lastReadVersion))
                    {
                        foreach (var item in FilesToUpdateList)
                        {
                            var file = item.File;
                            await Task.Run(() =>
                            {
                                var completeFilePath = SolutionPath + file;
                                var contents = File.ReadAllText(completeFilePath);
                                string result = contents.Replace(_lastReadVersion, newVersion);
                                File.WriteAllText(completeFilePath, result);
                                item.Version = newVersion;
                            });

                            CurrentlyUpdatedValues++;
                        }

                        _lastReadVersion = newVersion;
                        System.Windows.Forms.MessageBox.Show("Operation completed successfully.");
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("The assemblies are already updated. No changes are necessary.");
                    }
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Couldn't read current assembly version.", "Error");
                }
            }

            IsBusy = false;
        }

        private async void ExecuteSelectFolder()
        {
            var folderBrowser = new FolderBrowserDialog();
            folderBrowser.RootFolder = Environment.SpecialFolder.Desktop;
            folderBrowser.SelectedPath = SolutionPath;

            var result = folderBrowser.ShowDialog();
            if (result == DialogResult.OK)
            {
                IsBusy = true;

                SolutionPath = folderBrowser.SelectedPath;
                await LoadDataFromFolder(folderBrowser.SelectedPath, true);

                IsBusy = false;
            }
        }
    }
}