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

            //Read in current directory 
            var currFiles = Directory.GetFiles(directory);
            var toReturn = currFiles.FirstOrDefault(x => Constants.ASSEMBLYINFO_FILES.Any(x.Contains));

            if (!string.IsNullOrEmpty(toReturn))
            {
                return toReturn;
            }
            else
            {
                //or in Properties directory
                string propertiesDirectory = Path.Combine(directory, "Properties");
                if (Directory.Exists(propertiesDirectory))
                {
                    currFiles = Directory.GetFiles(propertiesDirectory);

                    return currFiles.FirstOrDefault(x => Constants.ASSEMBLYINFO_FILES.Any(x.Contains));
                }
            }

            return string.Empty;

        }

        public static FrameworkProjectItem ReadSingleAssemblyInfoFile(FrameworkProjectItem projectItem)
        {
            string assemblyInfoFile = FileManagement.LookForAssemblyInfo(projectItem.ProjectFullPath);
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

            projectItem.AssemblyFile = assemblyInfoFile;
            projectItem.AssemblyVersion = assemblyVersion;
            return projectItem;
        }

        public static void UpdateSingleVersion(FrameworkProjectItem item, string newVersion, double CurrentlyUpdatedValues)
        {
            
            if (item.FrameworkType == MicrosoftFrameworkType.NETCore || item.FrameworkType == MicrosoftFrameworkType.NETStandard)
            {
                if (!string.IsNullOrEmpty(item.ProjectFullPath))
                FrameworkManager.WriteAssemblyFrameworkVersion(item.ProjectFullPath, newVersion);
            }
            else
            {
                if (!string.IsNullOrEmpty(item.AssemblyVersion))
                    UpdateSingleAssemblyFile(item, newVersion);
            }

            CurrentlyUpdatedValues++;

        }

        private static void UpdateSingleAssemblyFile(FrameworkProjectItem item, string newVersion)
        {
            string completeFilePath = item.AssemblyFile;
            var readText = File.ReadAllText(completeFilePath);
            string[] readLines = File.ReadAllLines(completeFilePath);
            string result = readText;

            //Check AssemblyVersion or AssemblyFileVersion are totally missing
            IEnumerable<string> versionInfoLines = null;
            IEnumerable<string> versionFileInfoLines = null;
            if (item.AssemblyFile.EndsWith(".cs"))
            {
                versionInfoLines = readLines.Where(t => t.Contains("[assembly: AssemblyVersion") && !t.StartsWith("//"));
                versionFileInfoLines = readLines.Where(t => t.Contains("[assembly: AssemblyFileVersion") && !t.StartsWith("//"));
            }
            else if (item.AssemblyFile.EndsWith(".cpp"))
            {
                versionInfoLines = readLines.Where(t => t.Contains("[assembly:AssemblyVersionAttribute") && !t.StartsWith("//"));
                versionFileInfoLines = readLines.Where(t => t.Contains("[assembly:AssemblyFileVersionAttribute") && !t.StartsWith("//"));
            }

            //Check if are totally missing
            if (versionInfoLines.Count() == 0)
            {
                result += "\n";
                if (item.AssemblyFile.EndsWith(".cs"))
                {
                    result += string.Format("[assembly: AssemblyVersion(\"{0}\")]", newVersion);
                }
                else if (item.AssemblyFile.EndsWith(".cpp"))
                {
                    result += string.Format("[assembly:AssemblyVersionAttribute(\"{0}\")]", newVersion);
                }

                if (versionFileInfoLines.Count() == 0)
                {
                    if (versionInfoLines.Count() == 0)
                    {
                        result += "\n";
                    }
                    if (item.AssemblyFile.EndsWith(".cs"))
                    {
                        result += string.Format("[assembly: AssemblyFileVersion(\"{0}\")]", newVersion);
                    }
                    else if (item.AssemblyFile.EndsWith(".cpp"))
                    {
                        result += string.Format("[assembly:AssemblyFileVersionAttribute(\"{0}\")]", newVersion);
                    }

                }
            }
            else //just replace numbers
            {
                result = result.Replace(item.AssemblyVersion, newVersion);
            }

            File.WriteAllText(completeFilePath, result);
            item.AssemblyVersion = newVersion;
        }



    }
}

