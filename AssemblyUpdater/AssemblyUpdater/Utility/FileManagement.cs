using AssemblyUpdater.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssemblyUpdater.Utility
{
    public static class FileManagement
    {
        public static void UpdateSingleFile(AssemblyFileItem item, string newVersion, string solutionPath, List<string> _versionsMismatch, double CurrentlyUpdatedValues)
        {
            string file = item.File;
            var completeFilePath = solutionPath + file;
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
