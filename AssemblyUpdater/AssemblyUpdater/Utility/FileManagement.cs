using AssemblyUpdater.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyUpdater.Utility
{
    public static class FileManagement
    {

        public static string LookForAssemblyInfo(string projectDirectory)
        {
            string directory = Path.GetDirectoryName(projectDirectory);
            string propertiesDirectory = Path.Combine(directory, "Properties");
            var currFiles = Directory.GetFiles(propertiesDirectory);

            return currFiles.FirstOrDefault(x => Constants.ASSEMBLYINFO_FILES.Any(x.Contains));
        }

        public static List<string> LookForAssemblyInfosRecurse(string rootDirectory)
        {
            List<string> filesToUpdate = new List<string>();

            var currFiles = Directory.GetFiles(rootDirectory);


            var fileToUpdate = currFiles.FirstOrDefault(x => Constants.ASSEMBLYINFO_FILES.Any(x.Contains));

            if (fileToUpdate == null)
            {
                var currDirectories = Directory.GetDirectories(rootDirectory);
                Parallel.ForEach(currDirectories, x => filesToUpdate.AddRange(LookForAssemblyInfosRecurse(x)));
            }
            else
            {
                filesToUpdate.Add(fileToUpdate);
            }

            return filesToUpdate;
        }

        public static AssemblyFileItem ReadSingleAssemblyInfoFile(string absolutePathProjectFile )
        {
            string assemblyInfoFile = FileManagement.LookForAssemblyInfo(absolutePathProjectFile);
            string assemblyVersion = string.Empty;
            string assemblyVersionFile = string.Empty;

            if (!string.IsNullOrEmpty(assemblyInfoFile))
            {
                string[] readText = File.ReadAllLines(assemblyInfoFile);
             

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

            }

            AssemblyFileItem fileItem = new AssemblyFileItem();
            fileItem.File = Path.GetFileNameWithoutExtension(absolutePathProjectFile);
            fileItem.Version = assemblyVersion;
            return fileItem;
        }

        public static void UpdateSingleVersion(AssemblyFileItem item, string newVersion, string solutionPath, List<string> _versionsMismatch, double CurrentlyUpdatedValues)
        {
            string file = item.File;
            var completeFilePath = Path.Combine(solutionPath, file);

            var readText = File.ReadAllText(completeFilePath);
            string[] readLines = File.ReadAllLines(completeFilePath);
            string result = readText;

            //Check AssemblyVersion or AssemblyFileVersion are totally missing
            IEnumerable<string> versionInfoLines = null;
            IEnumerable<string> versionFileInfoLines = null;
            if (item.File.EndsWith(".cs"))
            {
                versionInfoLines = readLines.Where(t => t.Contains("[assembly: AssemblyVersion") && !t.StartsWith("//"));
                versionFileInfoLines = readLines.Where(t => t.Contains("[assembly: AssemblyFileVersion") && !t.StartsWith("//"));
            }
            else if (item.File.EndsWith(".cpp"))
            {
                versionInfoLines = readLines.Where(t => t.Contains("[assembly:AssemblyVersionAttribute") && !t.StartsWith("//"));
                versionFileInfoLines = readLines.Where(t => t.Contains("[assembly:AssemblyFileVersionAttribute") && !t.StartsWith("//"));
            }


            if (versionInfoLines.Count() == 0)
            {
                result += "\n";
                if (item.File.EndsWith(".cs"))
                {
                    result += string.Format("[assembly: AssemblyVersion(\"{0}\")]", newVersion);
                }
                else if (item.File.EndsWith(".cpp"))
                {
                    result += string.Format("[assembly:AssemblyVersionAttribute(\"{0}\")]", newVersion);
                }

                if (versionFileInfoLines.Count() == 0)
                {
                    if (versionInfoLines.Count() == 0)
                    {
                        result += "\n";
                    }
                    if (item.File.EndsWith(".cs"))
                    {
                        result += string.Format("[assembly: AssemblyFileVersion(\"{0}\")]", newVersion);
                    }
                    else if (item.File.EndsWith(".cpp"))
                    {
                        result += string.Format("[assembly:AssemblyFileVersionAttribute(\"{0}\")]", newVersion);
                    }

                }
            }

            //Foreach because it should owerwrite all the mismatch versions
            foreach (string mismatchVersion in _versionsMismatch)
            {
                result = result.Replace(mismatchVersion, newVersion);
            }

            File.WriteAllText(completeFilePath, result);
            item.Version = newVersion;

            CurrentlyUpdatedValues++;
        }
    }

}
